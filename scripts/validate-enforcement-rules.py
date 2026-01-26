#!/usr/bin/env python3
"""
Enforcement Rules Validator for AI Coding Factory

Reads config/enforcement/rules.yaml and validates the current project state
against the defined enforcement rules.

Usage:
    python3 scripts/validate-enforcement-rules.py [--phase PHASE] [--verbose]

Options:
    --phase PHASE    Current SDLC phase (ideation, development, validation, release, maintenance)
    --verbose        Show detailed output
    --check-only     Only check, don't block
"""

import argparse
import os
import re
import subprocess
import sys
from pathlib import Path
from typing import Dict, List, Optional, Tuple

try:
    import yaml
except ImportError:
    print("ERROR: PyYAML not installed. Run: pip install pyyaml")
    sys.exit(1)


class Colors:
    """ANSI color codes for terminal output."""
    RED = '\033[0;31m'
    GREEN = '\033[0;32m'
    YELLOW = '\033[1;33m'
    BLUE = '\033[0;34m'
    BOLD = '\033[1m'
    NC = '\033[0m'  # No Color


def load_rules(rules_path: str) -> Dict:
    """Load enforcement rules from YAML file."""
    if not os.path.exists(rules_path):
        print(f"{Colors.RED}ERROR:{Colors.NC} Rules file not found: {rules_path}")
        sys.exit(1)

    with open(rules_path, 'r') as f:
        return yaml.safe_load(f)


def get_git_staged_files() -> List[str]:
    """Get list of staged files."""
    try:
        result = subprocess.run(
            ['git', 'diff', '--cached', '--name-only'],
            capture_output=True, text=True, check=True
        )
        return result.stdout.strip().split('\n') if result.stdout.strip() else []
    except subprocess.CalledProcessError:
        return []


def get_git_modified_files() -> List[str]:
    """Get list of modified files (staged + unstaged)."""
    try:
        result = subprocess.run(
            ['git', 'diff', '--name-only', 'HEAD'],
            capture_output=True, text=True, check=True
        )
        return result.stdout.strip().split('\n') if result.stdout.strip() else []
    except subprocess.CalledProcessError:
        return []


def get_last_commit_message() -> str:
    """Get the last commit message."""
    try:
        result = subprocess.run(
            ['git', 'log', '-1', '--format=%s'],
            capture_output=True, text=True, check=True
        )
        return result.stdout.strip()
    except subprocess.CalledProcessError:
        return ""


def check_story_id_in_message(message: str) -> bool:
    """Check if commit message contains story ID (ACF-###)."""
    return bool(re.search(r'ACF-\d+', message))


def check_file_matches_patterns(filepath: str, patterns: List[str]) -> bool:
    """Check if filepath matches any of the glob patterns."""
    from fnmatch import fnmatch
    for pattern in patterns:
        if fnmatch(filepath, pattern):
            return True
    return False


def check_for_potential_secrets(content: str) -> List[str]:
    """Check content for potential secrets."""
    secret_patterns = [
        (r'(?i)(api[_-]?key|apikey)\s*[=:]\s*["\']?[\w-]{20,}', 'Potential API key'),
        (r'(?i)(password|passwd|pwd)\s*[=:]\s*["\'][^"\']+["\']', 'Hardcoded password'),
        (r'(?i)(secret|token)\s*[=:]\s*["\']?[\w-]{20,}', 'Potential secret/token'),
        (r'(?i)(aws_access_key_id|aws_secret)\s*[=:]', 'AWS credentials'),
        (r'-----BEGIN (RSA |EC |DSA )?PRIVATE KEY-----', 'Private key'),
        (r'(?i)bearer\s+[\w-]{20,}', 'Bearer token'),
    ]

    findings = []
    for pattern, description in secret_patterns:
        if re.search(pattern, content):
            findings.append(description)
    return findings


def check_domain_dependencies(filepath: str, content: str) -> List[str]:
    """Check if Domain layer has forbidden dependencies."""
    if '/Domain/' not in filepath:
        return []

    violations = []
    forbidden_namespaces = [
        'Infrastructure',
        'Application',
        'Microsoft.EntityFrameworkCore',
        'System.Net.Http',
        'Npgsql',
    ]

    for ns in forbidden_namespaces:
        if f'using {ns}' in content or f'using.*{ns}' in content:
            violations.append(f"Domain references forbidden namespace: {ns}")

    return violations


def validate_rule(rule: Dict, context: Dict) -> Tuple[bool, str]:
    """
    Validate a single rule against the current context.

    Returns:
        Tuple of (passed: bool, message: str)
    """
    rule_name = rule.get('name', 'unknown')
    condition = rule.get('condition', '')
    trigger = rule.get('trigger', '')
    paths = rule.get('paths', [])

    # Filter by trigger
    if trigger == 'pre_commit' and context.get('trigger') != 'pre_commit':
        return True, "Skipped (wrong trigger)"

    if trigger == 'phase_transition' and context.get('trigger') != 'phase_transition':
        return True, "Skipped (wrong trigger)"

    # Check path patterns if specified
    if paths:
        modified_files = context.get('modified_files', [])
        matching_files = [f for f in modified_files
                         if check_file_matches_patterns(f, paths)]
        if not matching_files:
            return True, "Skipped (no matching files)"
        context['matching_files'] = matching_files

    # Evaluate condition
    if condition == '!commit_message_has_story_id':
        message = context.get('commit_message', get_last_commit_message())
        if not check_story_id_in_message(message):
            return False, rule.get('message', 'Story ID missing')

    elif condition == 'contains_potential_secrets':
        for filepath in context.get('matching_files', context.get('modified_files', [])):
            if os.path.exists(filepath):
                with open(filepath, 'r', errors='ignore') as f:
                    content = f.read()
                secrets = check_for_potential_secrets(content)
                if secrets:
                    return False, f"Potential secrets in {filepath}: {', '.join(secrets)}"

    elif condition == 'domain_has_infrastructure_reference':
        for filepath in context.get('matching_files', []):
            if os.path.exists(filepath):
                with open(filepath, 'r', errors='ignore') as f:
                    content = f.read()
                violations = check_domain_dependencies(filepath, content)
                if violations:
                    return False, '\n'.join(violations)

    elif condition == 'file_staged':
        # For .env file blocking
        staged = get_git_staged_files()
        for filepath in context.get('matching_files', []):
            if filepath in staged:
                return False, rule.get('message', f'File should not be staged: {filepath}')

    elif condition == 'coverage < 80':
        # Would need to read coverage report
        coverage_file = Path('coverage.xml')
        if coverage_file.exists():
            # Simplified check - real implementation would parse XML
            pass

    return True, "Passed"


def validate_all_rules(rules_config: Dict, context: Dict, verbose: bool = False) -> Tuple[int, int, int]:
    """
    Validate all rules against context.

    Returns:
        Tuple of (passed, warnings, errors)
    """
    passed = 0
    warnings = 0
    errors = 0

    rules = rules_config.get('rules', [])

    for rule in rules:
        rule_name = rule.get('name', 'unknown')
        action = rule.get('action', 'warn')

        is_valid, message = validate_rule(rule, context)

        if is_valid:
            passed += 1
            if verbose:
                print(f"{Colors.GREEN}✓{Colors.NC} {rule_name}: {message}")
        else:
            if action == 'block':
                errors += 1
                print(f"{Colors.RED}✗{Colors.NC} {rule_name}: BLOCKED")
                print(f"  {message}")
            elif action == 'warn':
                warnings += 1
                print(f"{Colors.YELLOW}!{Colors.NC} {rule_name}: WARNING")
                print(f"  {message}")
            elif action == 'require_review':
                warnings += 1
                agent = rule.get('agent', 'code-reviewer')
                print(f"{Colors.BLUE}→{Colors.NC} {rule_name}: Review Required")
                print(f"  Agent: {agent}")
                print(f"  {message}")
            elif action == 'require_approval':
                warnings += 1
                print(f"{Colors.YELLOW}⚡{Colors.NC} {rule_name}: Approval Required")
                print(f"  {message}")

    return passed, warnings, errors


def print_delegation_info(rules_config: Dict):
    """Print agent delegation information."""
    delegation = rules_config.get('delegation', {})

    print(f"\n{Colors.BOLD}Agent Delegation Map:{Colors.NC}")
    print("-" * 40)

    for domain, config in delegation.items():
        primary = config.get('primary', 'unknown')
        collaborators = config.get('collaborators', [])

        collab_str = f" (with: {', '.join(collaborators)})" if collaborators else ""
        print(f"  {domain}: {primary}{collab_str}")


def print_cross_cutting_info(rules_config: Dict):
    """Print cross-cutting collaboration information."""
    cross_cutting = rules_config.get('cross_cutting', [])

    if not cross_cutting:
        return

    print(f"\n{Colors.BOLD}Cross-Cutting Collaborations:{Colors.NC}")
    print("-" * 40)

    for collab in cross_cutting:
        skill = collab.get('skill', 'unknown')
        owner = collab.get('owner', 'unknown')
        collaborators = collab.get('collaborators', [])
        reason = collab.get('reason', '')

        print(f"  {skill}:")
        print(f"    Owner: {owner}")
        print(f"    Collaborators: {', '.join(collaborators)}")
        print(f"    Reason: {reason}")


def main():
    parser = argparse.ArgumentParser(
        description='Validate enforcement rules for AI Coding Factory'
    )
    parser.add_argument(
        '--phase',
        choices=['ideation', 'development', 'validation', 'release', 'maintenance'],
        default='development',
        help='Current SDLC phase'
    )
    parser.add_argument(
        '--trigger',
        choices=['pre_commit', 'post_edit', 'phase_transition', 'pull_request'],
        default='pre_commit',
        help='Validation trigger type'
    )
    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Show verbose output'
    )
    parser.add_argument(
        '--check-only',
        action='store_true',
        help='Only check, return 0 even on failures'
    )
    parser.add_argument(
        '--show-delegation',
        action='store_true',
        help='Show agent delegation information'
    )
    parser.add_argument(
        '--rules-file',
        default='config/enforcement/rules.yaml',
        help='Path to rules file'
    )

    args = parser.parse_args()

    # Find project root
    script_dir = Path(__file__).parent
    project_root = script_dir.parent
    os.chdir(project_root)

    print(f"{Colors.BOLD}AI Coding Factory - Enforcement Rules Validator{Colors.NC}")
    print("=" * 50)
    print(f"Phase: {args.phase}")
    print(f"Trigger: {args.trigger}")
    print()

    # Load rules
    rules_config = load_rules(args.rules_file)

    if args.show_delegation:
        print_delegation_info(rules_config)
        print_cross_cutting_info(rules_config)
        return 0

    # Build context
    context = {
        'phase': args.phase,
        'trigger': args.trigger,
        'modified_files': get_git_modified_files(),
        'staged_files': get_git_staged_files(),
        'commit_message': get_last_commit_message(),
    }

    if args.verbose:
        print(f"Modified files: {len(context['modified_files'])}")
        print(f"Staged files: {len(context['staged_files'])}")
        print()

    # Validate rules
    passed, warnings, errors = validate_all_rules(rules_config, context, args.verbose)

    # Summary
    print()
    print("-" * 50)
    print(f"{Colors.BOLD}Summary:{Colors.NC}")
    print(f"  {Colors.GREEN}Passed:{Colors.NC} {passed}")
    print(f"  {Colors.YELLOW}Warnings:{Colors.NC} {warnings}")
    print(f"  {Colors.RED}Errors:{Colors.NC} {errors}")

    if errors > 0:
        print()
        print(f"{Colors.RED}VALIDATION FAILED{Colors.NC}")
        print("Fix the errors above before proceeding.")
        if not args.check_only:
            return 1
    elif warnings > 0:
        print()
        print(f"{Colors.YELLOW}VALIDATION PASSED WITH WARNINGS{Colors.NC}")
        print("Consider addressing the warnings above.")
    else:
        print()
        print(f"{Colors.GREEN}VALIDATION PASSED{Colors.NC}")

    return 0


if __name__ == '__main__':
    sys.exit(main())
