#!/bin/bash
# scripts/sync-ai-configs.sh
#
# Synchronizes AI coding assistant configurations across multiple tools:
# - Claude Code (.claude/)
# - OpenCode (.opencode/)
# - Cursor (.cursor/)
# - OpenAI Codex CLI (AGENTS.md)
# - Google Gemini (GEMINI.md)
#
# Usage: ./scripts/sync-ai-configs.sh [--dry-run]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DRY_RUN=false
if [[ "$1" == "--dry-run" ]]; then
    DRY_RUN=true
    echo "[DRY-RUN MODE] No changes will be made"
fi

log() {
    echo "[sync-ai-configs] $1"
}

run_cmd() {
    if $DRY_RUN; then
        echo "[would run] $*"
    else
        "$@"
    fi
}

cd "$PROJECT_ROOT"

# ============================================================================
# 1. Create AGENTS.md for Codex CLI / OpenCode compatibility
# ============================================================================
log "Syncing AGENTS.md (Codex/OpenCode compatibility)..."

if [ ! -e "AGENTS.md" ]; then
    if [ -f "CLAUDE.md" ]; then
        run_cmd ln -sf CLAUDE.md AGENTS.md
        log "Created AGENTS.md symlink to CLAUDE.md"
    else
        log "Warning: CLAUDE.md not found, cannot create AGENTS.md"
    fi
else
    log "AGENTS.md already exists"
fi

# ============================================================================
# 2. Create GEMINI.md for Google Gemini Code Assist
# ============================================================================
log "Syncing GEMINI.md (Gemini Code Assist compatibility)..."

if [ ! -e "GEMINI.md" ]; then
    if [ -f "CLAUDE.md" ]; then
        run_cmd ln -sf CLAUDE.md GEMINI.md
        log "Created GEMINI.md symlink to CLAUDE.md"
    else
        log "Warning: CLAUDE.md not found, cannot create GEMINI.md"
    fi
else
    log "GEMINI.md already exists"
fi

# ============================================================================
# 3. Create .aiexclude for Gemini (from .gitignore patterns)
# ============================================================================
log "Syncing .aiexclude (Gemini file exclusions)..."

if [ ! -f ".aiexclude" ] && [ -f ".gitignore" ]; then
    # Extract relevant patterns from .gitignore for AI exclusion
    run_cmd bash -c "grep -v '^#' .gitignore | grep -v '^\$' | head -50 > .aiexclude"
    log "Created .aiexclude from .gitignore patterns"
fi

# ============================================================================
# 4. Sync agents from .claude/agents to all targets
# ============================================================================
log "Syncing agents from .claude/agents/..."

if [ -f "$PROJECT_ROOT/scripts/sync-agents.sh" ]; then
    if $DRY_RUN; then
        run_cmd "$PROJECT_ROOT/scripts/sync-agents.sh" --all --dry-run
    else
        "$PROJECT_ROOT/scripts/sync-agents.sh" --all
    fi
else
    log "Warning: sync-agents.sh not found, skipping agent sync"
fi

# ============================================================================
# 5. Sync skills from .claude/skills to .opencode/skill
# ============================================================================
log "Syncing skills to .opencode/skill/..."

if [ -d ".claude/skills" ] && [ -d ".opencode/skill" ]; then
    for skill_dir in .claude/skills/*/; do
        skill_name=$(basename "$skill_dir")
        target=".opencode/skill/$skill_name"

        # Skip if already a symlink pointing to the right place
        if [ -L "$target" ]; then
            log "  $skill_name: already symlinked"
            continue
        fi

        # Skip if directory exists but isn't empty (preserve existing content)
        if [ -d "$target" ] && [ "$(ls -A "$target" 2>/dev/null)" ]; then
            log "  $skill_name: directory exists with content, skipping"
            continue
        fi

        # Create symlink
        run_cmd rm -rf "$target" 2>/dev/null || true
        run_cmd ln -sf "../../.claude/skills/$skill_name" "$target"
        log "  $skill_name: symlinked"
    done
else
    log "Warning: .claude/skills or .opencode/skill not found"
fi

# ============================================================================
# 5. Generate Cursor rules from skills
# ============================================================================
log "Generating Cursor rules from skills..."

mkdir -p .cursor/rules 2>/dev/null || true

if [ -d ".claude/skills" ]; then
    for skill_file in .claude/skills/*/SKILL.md; do
        if [ ! -f "$skill_file" ]; then
            continue
        fi

        skill_name=$(basename "$(dirname "$skill_file")")
        mdc_file=".cursor/rules/${skill_name}.mdc"

        # Extract description from YAML frontmatter
        description=$(sed -n '/^---$/,/^---$/p' "$skill_file" | grep '^description:' | sed 's/description: *//' | tr -d '"' | head -1)

        if [ -z "$description" ]; then
            description="$skill_name skill rules"
        fi

        # Determine globs based on skill name
        case "$skill_name" in
            *react*|*frontend*)
                globs='["**/*.tsx", "**/*.jsx", "**/*.ts", "**/*.js"]'
                ;;
            *net*|*dotnet*|*csharp*)
                globs='["**/*.cs", "**/*.csproj", "**/*.sln"]'
                ;;
            *test*)
                globs='["**/*Test*.cs", "**/*Tests*.cs", "**/*.test.ts", "**/*.spec.ts"]'
                ;;
            *)
                globs='["**/*"]'
                ;;
        esac

        # Generate .mdc file
        if ! $DRY_RUN; then
            cat > "$mdc_file" << EOF
---
description: $description
globs: $globs
alwaysApply: false
---

EOF
            # Append skill content (without YAML frontmatter)
            sed '1,/^---$/d' "$skill_file" | sed '1,/^---$/d' >> "$mdc_file"
        fi

        log "  Generated: $mdc_file"
    done
else
    log "Warning: .claude/skills not found"
fi

# ============================================================================
# 6. Sync hooks configuration hints
# ============================================================================
log "Checking hooks compatibility..."

# Note: Hooks are not directly portable between tools
# Claude Code uses JSON hooks, Codex uses TOML, Cursor doesn't support hooks
echo "  Claude Code hooks: $(ls .claude/hooks/*.sh 2>/dev/null | wc -l | tr -d ' ') found"
echo "  Note: Hooks are tool-specific and cannot be directly ported"

# ============================================================================
# 7. Validate OpenCode config
# ============================================================================
log "Validating OpenCode configuration..."

if [ -f ".opencode/opencode.json" ]; then
    if command -v python3 &> /dev/null; then
        if python3 -m json.tool .opencode/opencode.json > /dev/null 2>&1; then
            log "  opencode.json: valid JSON"
        else
            log "  Warning: opencode.json has invalid JSON"
        fi
    else
        log "  Skipping JSON validation (python3 not found)"
    fi
fi

# ============================================================================
# Summary
# ============================================================================
echo ""
echo "============================================"
echo "Sync complete!"
echo "============================================"
echo ""
echo "Files synced:"
echo "  - AGENTS.md     -> Codex CLI, OpenCode"
echo "  - GEMINI.md     -> Google Gemini Code Assist"
echo "  - .aiexclude    -> Gemini file exclusions"
echo "  - .opencode/    -> OpenCode skills (symlinked)"
echo "  - .cursor/rules -> Cursor rules (generated)"
echo ""
echo "Tool compatibility:"
echo "  Claude Code: Primary configuration in .claude/"
echo "  OpenCode:    Skills symlinked from .claude/skills/"
echo "  Cursor:      Rules generated in .cursor/rules/"
echo "  Codex CLI:   Uses AGENTS.md + ~/.codex/skills/"
echo "  Gemini:      Uses GEMINI.md + .aiexclude"
echo ""
