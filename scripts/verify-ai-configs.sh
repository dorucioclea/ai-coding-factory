#!/bin/bash
# ==============================================================================
# verify-ai-configs.sh - Cross-client AI configuration verification
# ==============================================================================
# Validates that all AI coding client configurations are properly set up
# Supports: Claude Code, Cursor, Codex CLI, OpenCode, Gemini (via AGENTS.md)
# ==============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counters
PASSED=0
FAILED=0
WARNINGS=0

# Helper functions
pass() {
    echo -e "${GREEN}✓${NC} $1"
    ((PASSED++))
}

fail() {
    echo -e "${RED}✗${NC} $1"
    ((FAILED++))
}

warn() {
    echo -e "${YELLOW}⚠${NC} $1"
    ((WARNINGS++))
}

info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

section() {
    echo ""
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${BLUE}  $1${NC}"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

# ==============================================================================
# Claude Code Verification
# ==============================================================================
verify_claude_code() {
    section "Claude Code Configuration"

    # Check .claude directory exists
    if [[ -d "$REPO_ROOT/.claude" ]]; then
        pass ".claude/ directory exists"
    else
        fail ".claude/ directory missing"
        return
    fi

    # Check settings.json
    if [[ -f "$REPO_ROOT/.claude/settings.json" ]]; then
        if python3 -c "import json; json.load(open('$REPO_ROOT/.claude/settings.json'))" 2>/dev/null; then
            pass "settings.json is valid JSON"
        else
            fail "settings.json has invalid JSON syntax"
        fi
    else
        fail "settings.json missing"
    fi

    # Check mcp-servers.json
    if [[ -f "$REPO_ROOT/.claude/mcp-servers.json" ]]; then
        if python3 -c "import json; json.load(open('$REPO_ROOT/.claude/mcp-servers.json'))" 2>/dev/null; then
            pass "mcp-servers.json is valid JSON"

            # Check for enabled servers
            enabled_count=$(python3 -c "
import json
with open('$REPO_ROOT/.claude/mcp-servers.json') as f:
    data = json.load(f)
    count = sum(1 for s in data.get('mcpServers', {}).values() if s.get('enabled', False))
    print(count)
" 2>/dev/null || echo "0")

            if [[ "$enabled_count" -gt 0 ]]; then
                pass "$enabled_count MCP server(s) enabled"
            else
                warn "No MCP servers enabled"
            fi
        else
            fail "mcp-servers.json has invalid JSON syntax"
        fi
    else
        warn "mcp-servers.json missing (optional)"
    fi

    # Check commands directory
    if [[ -d "$REPO_ROOT/.claude/commands" ]]; then
        cmd_count=$(find "$REPO_ROOT/.claude/commands" -name "*.md" 2>/dev/null | wc -l | tr -d ' ')
        if [[ "$cmd_count" -gt 0 ]]; then
            pass "$cmd_count command(s) found"
        else
            warn "No commands found in .claude/commands/"
        fi
    else
        warn ".claude/commands/ directory missing"
    fi

    # Check skills directory
    if [[ -d "$REPO_ROOT/.claude/skills" ]]; then
        skill_count=$(find "$REPO_ROOT/.claude/skills" -name "SKILL.md" 2>/dev/null | wc -l | tr -d ' ')
        if [[ "$skill_count" -gt 0 ]]; then
            pass "$skill_count skill(s) found"
        else
            warn "No skills found in .claude/skills/"
        fi
    else
        warn ".claude/skills/ directory missing"
    fi

    # Check agents directory
    if [[ -d "$REPO_ROOT/.claude/agents" ]]; then
        agent_count=$(find "$REPO_ROOT/.claude/agents" -name "*.md" 2>/dev/null | wc -l | tr -d ' ')
        if [[ "$agent_count" -gt 0 ]]; then
            pass "$agent_count agent(s) found"
        else
            warn "No agents found in .claude/agents/"
        fi
    else
        warn ".claude/agents/ directory missing"
    fi

    # Check hooks
    if [[ -d "$REPO_ROOT/.claude/hooks" ]]; then
        hook_count=$(find "$REPO_ROOT/.claude/hooks" -name "*.sh" 2>/dev/null | wc -l | tr -d ' ')
        if [[ "$hook_count" -gt 0 ]]; then
            pass "$hook_count hook script(s) found"

            # Check if hooks are executable
            non_exec=$(find "$REPO_ROOT/.claude/hooks" -name "*.sh" ! -executable 2>/dev/null | wc -l | tr -d ' ')
            if [[ "$non_exec" -gt 0 ]]; then
                warn "$non_exec hook(s) not executable"
            fi
        else
            warn "No hooks found in .claude/hooks/"
        fi
    else
        warn ".claude/hooks/ directory missing"
    fi

    # Check CLAUDE.md
    if [[ -f "$REPO_ROOT/CLAUDE.md" ]]; then
        pass "CLAUDE.md exists"
    else
        fail "CLAUDE.md missing (required for Claude Code)"
    fi
}

# ==============================================================================
# Cursor Verification
# ==============================================================================
verify_cursor() {
    section "Cursor Configuration"

    # Check .cursor directory exists
    if [[ -d "$REPO_ROOT/.cursor" ]]; then
        pass ".cursor/ directory exists"
    else
        warn ".cursor/ directory missing"
        return
    fi

    # Check rules directory
    if [[ -d "$REPO_ROOT/.cursor/rules" ]]; then
        rule_count=$(find "$REPO_ROOT/.cursor/rules" -name "*.mdc" 2>/dev/null | wc -l | tr -d ' ')
        if [[ "$rule_count" -gt 0 ]]; then
            pass "$rule_count .mdc rule file(s) found"

            # Validate each rule has YAML frontmatter
            for rule in "$REPO_ROOT/.cursor/rules"/*.mdc; do
                if [[ -f "$rule" ]]; then
                    if head -n 1 "$rule" | grep -q "^---$"; then
                        :  # Has frontmatter
                    else
                        warn "$(basename "$rule") missing YAML frontmatter"
                    fi
                fi
            done
        else
            warn "No .mdc rule files found in .cursor/rules/"
        fi
    else
        warn ".cursor/rules/ directory missing"
    fi
}

# ==============================================================================
# Codex CLI Verification
# ==============================================================================
verify_codex() {
    section "Codex CLI Configuration"

    # Check .codex directory exists
    if [[ -d "$REPO_ROOT/.codex" ]]; then
        pass ".codex/ directory exists"
    else
        warn ".codex/ directory missing"
        return
    fi

    # Check config.toml
    if [[ -f "$REPO_ROOT/.codex/config.toml" ]]; then
        pass "config.toml exists"

        # Basic TOML validation (check for syntax errors)
        if python3 -c "
try:
    import tomllib
    with open('$REPO_ROOT/.codex/config.toml', 'rb') as f:
        tomllib.load(f)
    exit(0)
except:
    try:
        import toml
        toml.load('$REPO_ROOT/.codex/config.toml')
        exit(0)
    except:
        exit(1)
" 2>/dev/null; then
            pass "config.toml is valid TOML"
        else
            warn "config.toml may have syntax issues (install tomllib/toml to verify)"
        fi
    else
        warn "config.toml missing"
    fi

    # Check instructions.md
    if [[ -f "$REPO_ROOT/.codex/instructions.md" ]]; then
        pass "instructions.md exists"
    else
        warn "instructions.md missing (Codex uses this for context)"
    fi
}

# ==============================================================================
# OpenCode Verification
# ==============================================================================
verify_opencode() {
    section "OpenCode Configuration"

    # Check .opencode directory exists
    if [[ -d "$REPO_ROOT/.opencode" ]]; then
        pass ".opencode/ directory exists"
    else
        warn ".opencode/ directory missing"
        return
    fi

    # Check opencode.json
    if [[ -f "$REPO_ROOT/.opencode/opencode.json" ]]; then
        if python3 -c "import json; json.load(open('$REPO_ROOT/.opencode/opencode.json'))" 2>/dev/null; then
            pass "opencode.json is valid JSON"
        else
            fail "opencode.json has invalid JSON syntax"
        fi
    else
        warn "opencode.json missing"
    fi

    # Check agents directory
    if [[ -d "$REPO_ROOT/.opencode/agent" ]]; then
        agent_count=$(find "$REPO_ROOT/.opencode/agent" -name "*.md" 2>/dev/null | wc -l | tr -d ' ')
        if [[ "$agent_count" -gt 0 ]]; then
            pass "$agent_count OpenCode agent(s) found"
        else
            warn "No agents in .opencode/agent/"
        fi
    fi

    # Check skills directory
    if [[ -d "$REPO_ROOT/.opencode/skill" ]]; then
        skill_count=$(find "$REPO_ROOT/.opencode/skill" -name "*.md" 2>/dev/null | wc -l | tr -d ' ')
        if [[ "$skill_count" -gt 0 ]]; then
            pass "$skill_count OpenCode skill(s) found"
        else
            warn "No skills in .opencode/skill/"
        fi
    fi
}

# ==============================================================================
# Gemini / AGENTS.md Verification
# ==============================================================================
verify_gemini() {
    section "Gemini / AGENTS.md Standard"

    # Check AGENTS.md (vendor-neutral standard)
    if [[ -f "$REPO_ROOT/AGENTS.md" ]]; then
        pass "AGENTS.md exists"

        # Check if it's a symlink to CLAUDE.md
        if [[ -L "$REPO_ROOT/AGENTS.md" ]]; then
            target=$(readlink "$REPO_ROOT/AGENTS.md")
            pass "AGENTS.md is symlink to $target"
        else
            info "AGENTS.md is a standalone file"
        fi
    else
        warn "AGENTS.md missing (recommended for cross-client compatibility)"
    fi

    # Check GEMINI.md
    if [[ -f "$REPO_ROOT/GEMINI.md" ]]; then
        pass "GEMINI.md exists"

        if [[ -L "$REPO_ROOT/GEMINI.md" ]]; then
            target=$(readlink "$REPO_ROOT/GEMINI.md")
            pass "GEMINI.md is symlink to $target"
        fi
    else
        warn "GEMINI.md missing (optional)"
    fi
}

# ==============================================================================
# Cross-client Consistency Checks
# ==============================================================================
verify_consistency() {
    section "Cross-client Consistency"

    # Check if TDD rules exist in multiple places
    local tdd_locations=0
    [[ -f "$REPO_ROOT/.claude/skills/tdd/SKILL.md" ]] && ((tdd_locations++))
    [[ -f "$REPO_ROOT/.cursor/rules/tdd.mdc" ]] && ((tdd_locations++))

    if [[ "$tdd_locations" -ge 2 ]]; then
        pass "TDD rules present in multiple clients"
    else
        warn "TDD rules not consistently configured across clients"
    fi

    # Check if security rules exist in multiple places
    local security_locations=0
    [[ -f "$REPO_ROOT/.claude/skills/security/SKILL.md" ]] && ((security_locations++))
    [[ -f "$REPO_ROOT/.cursor/rules/security.mdc" ]] && ((security_locations++))

    if [[ "$security_locations" -ge 2 ]]; then
        pass "Security rules present in multiple clients"
    else
        warn "Security rules not consistently configured across clients"
    fi

    # Check sync script exists
    if [[ -f "$REPO_ROOT/scripts/sync-ai-configs.sh" ]]; then
        pass "sync-ai-configs.sh exists for keeping clients in sync"
    else
        warn "sync-ai-configs.sh missing (recommended for maintaining consistency)"
    fi
}

# ==============================================================================
# Environment Variables Check
# ==============================================================================
verify_env_vars() {
    section "Environment Variables"

    # Check for required env vars for enabled MCP servers
    local missing_vars=()

    if [[ -f "$REPO_ROOT/.claude/mcp-servers.json" ]]; then
        # Check GitHub token
        if python3 -c "
import json
with open('$REPO_ROOT/.claude/mcp-servers.json') as f:
    data = json.load(f)
    github = data.get('mcpServers', {}).get('github', {})
    if github.get('enabled', False):
        exit(0)
    exit(1)
" 2>/dev/null; then
            if [[ -z "${GITHUB_TOKEN:-}" ]]; then
                warn "GITHUB_TOKEN not set (required for GitHub MCP server)"
            else
                pass "GITHUB_TOKEN is set"
            fi
        fi
    fi

    info "Set environment variables in your shell profile or .env file"
}

# ==============================================================================
# Main
# ==============================================================================
main() {
    echo ""
    echo "╔══════════════════════════════════════════════════════════════════════════╗"
    echo "║           AI Coding Clients Configuration Verification                    ║"
    echo "╚══════════════════════════════════════════════════════════════════════════╝"
    echo ""
    info "Repository: $REPO_ROOT"

    verify_claude_code
    verify_cursor
    verify_codex
    verify_opencode
    verify_gemini
    verify_consistency
    verify_env_vars

    section "Summary"

    echo ""
    echo -e "  ${GREEN}Passed:${NC}   $PASSED"
    echo -e "  ${RED}Failed:${NC}   $FAILED"
    echo -e "  ${YELLOW}Warnings:${NC} $WARNINGS"
    echo ""

    if [[ "$FAILED" -gt 0 ]]; then
        echo -e "${RED}Some checks failed. Please review the output above.${NC}"
        exit 1
    elif [[ "$WARNINGS" -gt 0 ]]; then
        echo -e "${YELLOW}All required checks passed with warnings.${NC}"
        exit 0
    else
        echo -e "${GREEN}All checks passed!${NC}"
        exit 0
    fi
}

main "$@"
