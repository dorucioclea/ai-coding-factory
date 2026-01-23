#!/bin/bash
# Skill Synchronization Script
# Syncs skills from .claude/skills/ to other IDE formats
# Supports: opencode, codex, cursor, gemini, kilocode, factory

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
PROJECT_ROOT="$(pwd)"
SOURCE_DIR="$PROJECT_ROOT/.claude/skills"
TARGETS=("opencode" "codex" "cursor" "gemini" "kilocode" "factory")

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[OK]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Function to convert skill name to target format
convert_skill_name() {
    local skill_name="$1"
    local target="$2"

    # Most IDEs use the same name, but some might need conversion
    echo "$skill_name"
}

# Function to get target skills directory
get_target_dir() {
    local target="$1"
    case "$target" in
        opencode) echo "$PROJECT_ROOT/.opencode/skill" ;;
        codex)    echo "$PROJECT_ROOT/.codex/skills" ;;
        cursor)   echo "$PROJECT_ROOT/.cursor/skills" ;;
        gemini)   echo "$PROJECT_ROOT/.gemini/skills" ;;
        kilocode) echo "$PROJECT_ROOT/.kilocode/skills" ;;
        factory)  echo "$PROJECT_ROOT/.factory/skills" ;;
        *)        echo "" ;;
    esac
}

# Function to sync a single skill
sync_skill() {
    local skill_path="$1"
    local target="$2"
    local target_dir="$3"

    local skill_name=$(basename "$skill_path")
    local skill_md="$skill_path/SKILL.md"

    # Skip if no SKILL.md
    if [ ! -f "$skill_md" ]; then
        return 0
    fi

    local target_skill_dir="$target_dir/$skill_name"

    # Create target directory
    mkdir -p "$target_skill_dir"

    # Copy SKILL.md (rename to skill.md for opencode)
    if [ "$target" = "opencode" ]; then
        cp "$skill_md" "$target_skill_dir/skill.md"
    else
        cp "$skill_md" "$target_skill_dir/SKILL.md"
    fi

    # Copy supporting files based on target format
    case "$target" in
        codex)
            # Codex uses references/ and assets/templates/
            if [ -d "$skill_path/templates" ]; then
                mkdir -p "$target_skill_dir/assets/templates"
                cp -r "$skill_path/templates/"* "$target_skill_dir/assets/templates/" 2>/dev/null || true
            fi
            if [ -d "$skill_path/references" ]; then
                mkdir -p "$target_skill_dir/references"
                cp -r "$skill_path/references/"* "$target_skill_dir/references/" 2>/dev/null || true
            fi
            # Copy other md files to references
            for f in "$skill_path"/*.md; do
                [ -f "$f" ] && [ "$(basename "$f")" != "SKILL.md" ] && {
                    mkdir -p "$target_skill_dir/references"
                    cp "$f" "$target_skill_dir/references/"
                }
            done
            ;;
        gemini)
            # Gemini uses references/ and templates/
            if [ -d "$skill_path/templates" ]; then
                mkdir -p "$target_skill_dir/templates"
                cp -r "$skill_path/templates/"* "$target_skill_dir/templates/" 2>/dev/null || true
            fi
            if [ -d "$skill_path/scripts" ]; then
                mkdir -p "$target_skill_dir/scripts"
                cp -r "$skill_path/scripts/"* "$target_skill_dir/scripts/" 2>/dev/null || true
            fi
            for f in "$skill_path"/*.md; do
                [ -f "$f" ] && [ "$(basename "$f")" != "SKILL.md" ] && {
                    mkdir -p "$target_skill_dir/references"
                    cp "$f" "$target_skill_dir/references/"
                }
            done
            ;;
        *)
            # Default: copy templates/, scripts/, and other md files directly
            if [ -d "$skill_path/templates" ]; then
                mkdir -p "$target_skill_dir/templates"
                cp -r "$skill_path/templates/"* "$target_skill_dir/templates/" 2>/dev/null || true
            fi
            if [ -d "$skill_path/scripts" ]; then
                mkdir -p "$target_skill_dir/scripts"
                cp -r "$skill_path/scripts/"* "$target_skill_dir/scripts/" 2>/dev/null || true
            fi
            if [ -d "$skill_path/resources" ]; then
                mkdir -p "$target_skill_dir/resources"
                cp -r "$skill_path/resources/"* "$target_skill_dir/resources/" 2>/dev/null || true
            fi
            if [ -d "$skill_path/rules" ]; then
                mkdir -p "$target_skill_dir/rules"
                cp -r "$skill_path/rules/"* "$target_skill_dir/rules/" 2>/dev/null || true
            fi
            for f in "$skill_path"/*.md; do
                [ -f "$f" ] && [ "$(basename "$f")" != "SKILL.md" ] && cp "$f" "$target_skill_dir/"
            done
            for f in "$skill_path"/*.json; do
                [ -f "$f" ] && cp "$f" "$target_skill_dir/"
            done
            ;;
    esac

    return 0
}

# Function to sync nested skills
sync_nested_skills() {
    local parent_path="$1"
    local target="$2"
    local target_dir="$3"

    local parent_name=$(basename "$parent_path")

    # Find nested SKILL.md files
    for nested_skill in "$parent_path"/*/; do
        if [ -f "$nested_skill/SKILL.md" ]; then
            local nested_name=$(basename "$nested_skill")
            local nested_target="$target_dir/$parent_name/$nested_name"

            mkdir -p "$nested_target"
            sync_skill "$nested_skill" "$target" "$target_dir/$parent_name"
        fi
    done
}

# Main sync function
sync_to_target() {
    local target="$1"
    local target_dir=$(get_target_dir "$target")

    if [ -z "$target_dir" ]; then
        log_error "Unknown target: $target"
        return 1
    fi

    log_info "Syncing to $target ($target_dir)..."

    # Create target directory
    mkdir -p "$target_dir"

    local skill_count=0
    local nested_count=0

    # Sync each skill
    for skill_path in "$SOURCE_DIR"/*/; do
        [ -d "$skill_path" ] || continue

        local skill_name=$(basename "$skill_path")

        # Check if this skill has SKILL.md at top level
        if [ -f "$skill_path/SKILL.md" ]; then
            sync_skill "$skill_path" "$target" "$target_dir"
            ((skill_count++))
        fi

        # Check for nested skills
        for nested in "$skill_path"/*/; do
            if [ -f "$nested/SKILL.md" ]; then
                local parent_name=$(basename "$skill_path")
                local nested_name=$(basename "$nested")
                mkdir -p "$target_dir/$parent_name"
                sync_skill "$nested" "$target" "$target_dir/$parent_name"
                ((nested_count++))
            fi
        done
    done

    log_success "Synced $skill_count skills + $nested_count nested to $target"
}

# Print usage
usage() {
    echo "Usage: $0 [PROJECT_ROOT] [--target TARGET] [--all] [--dry-run]"
    echo ""
    echo "Options:"
    echo "  PROJECT_ROOT    Project root directory (default: current directory)"
    echo "  --target        Sync to specific target (opencode, codex, cursor, gemini, kilocode, factory)"
    echo "  --all           Sync to all targets"
    echo "  --list          List available skills"
    echo "  --dry-run       Show what would be synced without copying"
    echo ""
    echo "Examples:"
    echo "  $0 --all                    # Sync to all IDEs"
    echo "  $0 --target opencode        # Sync only to opencode"
    echo "  $0 /path/to/project --all   # Sync project at specific path"
}

# Parse arguments
DRY_RUN=false
SYNC_ALL=false
SPECIFIC_TARGET=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --target)
            SPECIFIC_TARGET="$2"
            shift 2
            ;;
        --all)
            SYNC_ALL=true
            shift
            ;;
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --list)
            echo "=== Skills in $SOURCE_DIR ==="
            find "$SOURCE_DIR" -name "SKILL.md" -type f | while read f; do
                dir=$(dirname "$f")
                name=$(grep -m1 "^name:" "$f" 2>/dev/null | cut -d: -f2 | tr -d ' ' || basename "$dir")
                echo "  - $name ($(echo "$dir" | sed "s|$SOURCE_DIR/||"))"
            done | sort
            exit 0
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            if [ -d "$1" ]; then
                PROJECT_ROOT="$1"
                SOURCE_DIR="$PROJECT_ROOT/.claude/skills"
            fi
            shift
            ;;
    esac
done

# Validate source directory
if [ ! -d "$SOURCE_DIR" ]; then
    log_error "Source directory not found: $SOURCE_DIR"
    exit 1
fi

# Count source skills
TOTAL_SKILLS=$(find "$SOURCE_DIR" -name "SKILL.md" -type f | wc -l | tr -d ' ')
log_info "Found $TOTAL_SKILLS skills in $SOURCE_DIR"

# Sync
if [ "$SYNC_ALL" = true ]; then
    for target in "${TARGETS[@]}"; do
        sync_to_target "$target"
    done
    log_success "Sync complete to all targets!"
elif [ -n "$SPECIFIC_TARGET" ]; then
    sync_to_target "$SPECIFIC_TARGET"
    log_success "Sync complete to $SPECIFIC_TARGET!"
else
    usage
    exit 1
fi

# Summary
echo ""
echo "=== Sync Summary ==="
for target in "${TARGETS[@]}"; do
    target_dir=$(get_target_dir "$target")
    if [ -d "$target_dir" ]; then
        count=$(find "$target_dir" -name "SKILL.md" -o -name "skill.md" 2>/dev/null | wc -l | tr -d ' ')
        echo "  $target: $count skills"
    fi
done
