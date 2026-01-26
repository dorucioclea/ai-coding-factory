#!/usr/bin/env python3
"""
Phase Tracker for AI Coding Factory

Tracks the current SDLC phase and enforces gate transitions.
State is persisted in .factory/state/project-state.json

Usage:
    python3 scripts/phase-tracker.py status          # Show current phase
    python3 scripts/phase-tracker.py set PHASE       # Set phase (with gate check)
    python3 scripts/phase-tracker.py check-gate      # Check if current gate passes
    python3 scripts/phase-tracker.py history         # Show phase transition history
    python3 scripts/phase-tracker.py init [PHASE]    # Initialize tracking (default: ideation)
"""

import argparse
import json
import os
import sys
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional

# Find project root
SCRIPT_DIR = Path(__file__).parent
PROJECT_ROOT = SCRIPT_DIR.parent
STATE_DIR = PROJECT_ROOT / ".factory" / "state"
STATE_FILE = STATE_DIR / "project-state.json"
ORCHESTRATOR_FILE = PROJECT_ROOT / ".claude" / "agents" / "orchestrator.yaml"

# ANSI colors
class Colors:
    RED = '\033[0;31m'
    GREEN = '\033[0;32m'
    YELLOW = '\033[1;33m'
    BLUE = '\033[0;34m'
    CYAN = '\033[0;36m'
    BOLD = '\033[1m'
    NC = '\033[0m'


# Phase definitions (from orchestrator.yaml)
PHASES = {
    "ideation": {
        "description": "Define scope and constraints",
        "next": "development",
        "gate": "gate_1_ideation_complete",
        "required_artifacts": [
            "requirements.md",
            "epics.md OR user-stories.md",
            "architecture.md",
            "risk-log.md"
        ],
        "allowed_actions": ["requirements_gathering", "architecture_design", "risk_assessment"],
        "forbidden_actions": ["code_changes"]
    },
    "development": {
        "description": "Implement approved stories",
        "next": "validation",
        "gate": "gate_3_implementation_complete",
        "required_artifacts": [
            "story files (ACF-###.md)",
            "implementation code",
            "tests",
            "documentation updates"
        ],
        "allowed_actions": ["implement_stories", "write_tests", "update_docs"],
        "forbidden_actions": ["unreviewed_dependencies", "untracked_changes"]
    },
    "validation": {
        "description": "Run validation and quality checks",
        "next": "release",
        "gate": "gate_5_security_approved",
        "required_artifacts": [
            "test_report.xml",
            "coverage_report.xml",
            "traceability_report.md"
        ],
        "allowed_actions": ["run_validation_scripts", "run_ci_checks"],
        "forbidden_actions": ["disable_tests", "lower_coverage"]
    },
    "release": {
        "description": "Package and release",
        "next": "maintenance",
        "gate": "gate_6_release_ready",
        "required_artifacts": [
            "release_notes.md",
            "security_review_checklist.md",
            "release_readiness_checklist.md"
        ],
        "allowed_actions": ["package", "release"],
        "forbidden_actions": ["release_with_violations"]
    },
    "maintenance": {
        "description": "Operate and fix issues",
        "next": "ideation",  # Cycles back for new features
        "gate": None,
        "required_artifacts": [],
        "allowed_actions": ["operate", "fix_issues", "monitor"],
        "forbidden_actions": ["silent_hotfixes", "untracked_changes"]
    }
}


def load_state() -> Dict:
    """Load current project state."""
    if not STATE_FILE.exists():
        return None

    with open(STATE_FILE, 'r') as f:
        return json.load(f)


def save_state(state: Dict) -> None:
    """Save project state."""
    STATE_DIR.mkdir(parents=True, exist_ok=True)

    with open(STATE_FILE, 'w') as f:
        json.dump(state, f, indent=2, default=str)


def init_state(phase: str = "ideation") -> Dict:
    """Initialize project state."""
    if phase not in PHASES:
        print(f"{Colors.RED}ERROR:{Colors.NC} Invalid phase: {phase}")
        print(f"Valid phases: {', '.join(PHASES.keys())}")
        sys.exit(1)

    state = {
        "phase": phase,
        "entered_at": datetime.now().isoformat(),
        "project_name": PROJECT_ROOT.name,
        "gate_checks": {},
        "history": [
            {
                "phase": phase,
                "entered_at": datetime.now().isoformat(),
                "action": "initialized"
            }
        ]
    }

    save_state(state)
    print(f"{Colors.GREEN}✓{Colors.NC} Initialized project tracking")
    print(f"  Phase: {Colors.CYAN}{phase}{Colors.NC}")
    print(f"  State file: {STATE_FILE}")

    return state


def show_status(state: Dict) -> None:
    """Show current phase status."""
    if not state:
        print(f"{Colors.YELLOW}Project tracking not initialized.{Colors.NC}")
        print(f"Run: python3 scripts/phase-tracker.py init")
        return

    phase = state["phase"]
    phase_info = PHASES[phase]

    print(f"\n{Colors.BOLD}AI Coding Factory - Phase Status{Colors.NC}")
    print("=" * 50)
    print(f"Project:     {state.get('project_name', 'Unknown')}")
    print(f"Phase:       {Colors.CYAN}{phase}{Colors.NC}")
    print(f"Description: {phase_info['description']}")
    print(f"Entered:     {state['entered_at']}")
    print(f"Next phase:  {phase_info['next']}")

    print(f"\n{Colors.BOLD}Required Artifacts:{Colors.NC}")
    for artifact in phase_info['required_artifacts']:
        print(f"  - {artifact}")

    print(f"\n{Colors.BOLD}Allowed Actions:{Colors.NC}")
    for action in phase_info['allowed_actions']:
        print(f"  {Colors.GREEN}✓{Colors.NC} {action}")

    print(f"\n{Colors.BOLD}Forbidden Actions:{Colors.NC}")
    for action in phase_info['forbidden_actions']:
        print(f"  {Colors.RED}✗{Colors.NC} {action}")

    if phase_info['gate']:
        print(f"\n{Colors.BOLD}Exit Gate:{Colors.NC} {phase_info['gate']}")
        print(f"  Checklist: config/gates/{phase}-gate.md")


def check_gate(state: Dict) -> bool:
    """Check if current phase gate passes."""
    if not state:
        print(f"{Colors.YELLOW}Project tracking not initialized.{Colors.NC}")
        return False

    phase = state["phase"]
    phase_info = PHASES[phase]
    gate = phase_info.get("gate")

    if not gate:
        print(f"{Colors.GREEN}✓{Colors.NC} No gate for {phase} phase")
        return True

    print(f"\n{Colors.BOLD}Gate Check: {gate}{Colors.NC}")
    print("-" * 50)

    # Check for gate checklist file
    gate_file = PROJECT_ROOT / "config" / "gates" / f"{phase}-gate.md"

    if not gate_file.exists():
        print(f"{Colors.YELLOW}!{Colors.NC} Gate checklist not found: {gate_file}")
        return False

    # Parse checklist and check items
    with open(gate_file, 'r') as f:
        content = f.read()

    # Count checked vs unchecked items
    checked = content.count('[x]') + content.count('[X]')
    unchecked = content.count('[ ]')
    total = checked + unchecked

    if total == 0:
        print(f"{Colors.YELLOW}!{Colors.NC} No checklist items found")
        return False

    percentage = (checked / total) * 100

    print(f"  Completed: {checked}/{total} ({percentage:.0f}%)")

    if percentage < 100:
        print(f"\n{Colors.YELLOW}Gate not passed.{Colors.NC}")
        print(f"Review and complete: {gate_file}")
        return False

    print(f"\n{Colors.GREEN}✓ Gate passed!{Colors.NC}")
    return True


def set_phase(state: Dict, new_phase: str, force: bool = False) -> None:
    """Transition to a new phase."""
    if not state:
        print(f"{Colors.YELLOW}Project tracking not initialized.{Colors.NC}")
        print(f"Run: python3 scripts/phase-tracker.py init")
        return

    if new_phase not in PHASES:
        print(f"{Colors.RED}ERROR:{Colors.NC} Invalid phase: {new_phase}")
        print(f"Valid phases: {', '.join(PHASES.keys())}")
        return

    current_phase = state["phase"]

    if current_phase == new_phase:
        print(f"Already in {new_phase} phase")
        return

    # Check if this is a valid transition
    expected_next = PHASES[current_phase]["next"]

    if new_phase != expected_next and not force:
        print(f"{Colors.YELLOW}WARNING:{Colors.NC} Unexpected transition")
        print(f"  Current: {current_phase}")
        print(f"  Expected next: {expected_next}")
        print(f"  Requested: {new_phase}")
        print(f"\nUse --force to override")
        return

    # Check gate before transition (unless forcing)
    if not force:
        gate_passed = check_gate(state)
        if not gate_passed:
            print(f"\n{Colors.RED}Cannot transition:{Colors.NC} Gate not passed")
            print(f"Use --force to override (not recommended)")
            return

    # Perform transition
    state["phase"] = new_phase
    state["entered_at"] = datetime.now().isoformat()
    state["history"].append({
        "phase": new_phase,
        "entered_at": datetime.now().isoformat(),
        "from_phase": current_phase,
        "action": "transitioned" if not force else "forced_transition"
    })

    save_state(state)

    print(f"\n{Colors.GREEN}✓{Colors.NC} Transitioned to {Colors.CYAN}{new_phase}{Colors.NC}")
    print(f"  From: {current_phase}")
    print(f"  Description: {PHASES[new_phase]['description']}")


def show_history(state: Dict) -> None:
    """Show phase transition history."""
    if not state:
        print(f"{Colors.YELLOW}Project tracking not initialized.{Colors.NC}")
        return

    print(f"\n{Colors.BOLD}Phase Transition History{Colors.NC}")
    print("=" * 50)

    for i, entry in enumerate(state.get("history", [])):
        phase = entry["phase"]
        timestamp = entry["entered_at"]
        action = entry.get("action", "unknown")
        from_phase = entry.get("from_phase", "")

        marker = "→" if i > 0 else "◆"
        from_str = f" (from {from_phase})" if from_phase else ""

        print(f"{marker} {Colors.CYAN}{phase}{Colors.NC}{from_str}")
        print(f"  {timestamp} - {action}")


def main():
    parser = argparse.ArgumentParser(
        description='Track SDLC phases for AI Coding Factory'
    )

    subparsers = parser.add_subparsers(dest='command', help='Commands')

    # status command
    subparsers.add_parser('status', help='Show current phase status')

    # init command
    init_parser = subparsers.add_parser('init', help='Initialize phase tracking')
    init_parser.add_argument('phase', nargs='?', default='ideation',
                            help='Initial phase (default: ideation)')

    # set command
    set_parser = subparsers.add_parser('set', help='Set phase (with gate check)')
    set_parser.add_argument('phase', help='Target phase')
    set_parser.add_argument('--force', '-f', action='store_true',
                           help='Force transition without gate check')

    # check-gate command
    subparsers.add_parser('check-gate', help='Check if current gate passes')

    # history command
    subparsers.add_parser('history', help='Show phase transition history')

    args = parser.parse_args()

    # Default to status if no command
    if not args.command:
        args.command = 'status'

    # Load state
    state = load_state()

    if args.command == 'init':
        init_state(args.phase)
    elif args.command == 'status':
        show_status(state)
    elif args.command == 'check-gate':
        check_gate(state)
    elif args.command == 'set':
        set_phase(state, args.phase, args.force)
    elif args.command == 'history':
        show_history(state)
    else:
        parser.print_help()


if __name__ == '__main__':
    main()
