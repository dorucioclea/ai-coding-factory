#!/usr/bin/env bash
# Agent Synchronization Script
# Syncs agents from .claude/agents/ (canonical source) to other IDE formats
# Supports: opencode, codex, cursor, gemini, kilocode, factory
#
# Usage: ./scripts/sync-agents.sh [--all] [--target TARGET] [--dry-run]

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SOURCE_DIR="$PROJECT_ROOT/.claude/agents"

# Target list
TARGETS="opencode codex cursor gemini kilocode factory"

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[OK]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Function to get target directory
get_target_dir() {
    local target="$1"
    case "$target" in
        opencode) echo "$PROJECT_ROOT/.opencode/agent" ;;
        codex)    echo "$PROJECT_ROOT/.codex/agents" ;;
        cursor)   echo "$PROJECT_ROOT/.cursor/agents" ;;
        gemini)   echo "$PROJECT_ROOT/.gemini/agents" ;;
        kilocode) echo "$PROJECT_ROOT/.kilocode/agents" ;;
        factory)  echo "$PROJECT_ROOT/.factory/agents" ;;
        *)        echo "" ;;
    esac
}

# Function to get sync method
get_sync_method() {
    local target="$1"
    case "$target" in
        opencode) echo "symlink" ;;
        *)        echo "copy" ;;
    esac
}

# Function to sync agents to a target
sync_to_target() {
    local target="$1"
    local target_dir=$(get_target_dir "$target")
    local method=$(get_sync_method "$target")

    if [ -z "$target_dir" ]; then
        log_error "Unknown target: $target"
        return 1
    fi

    log_info "Syncing agents to $target ($target_dir) using $method..."

    # Create target directory
    mkdir -p "$target_dir"

    local agent_count=0

    # Sync each agent file
    for agent_file in "$SOURCE_DIR"/*.md "$SOURCE_DIR"/*.yaml; do
        [ -f "$agent_file" ] || continue

        local agent_name=$(basename "$agent_file")
        local target_file="$target_dir/$agent_name"

        if [ "$DRY_RUN" = true ]; then
            echo "  [dry-run] Would $method: $agent_name"
            agent_count=$((agent_count + 1))
            continue
        fi

        if [ "$method" = "symlink" ]; then
            # Remove existing file/symlink
            rm -f "$target_file" 2>/dev/null || true

            # Create relative symlink
            local rel_path="../../.claude/agents/$agent_name"
            ln -sf "$rel_path" "$target_file"
        else
            # Copy the file
            cp "$agent_file" "$target_file"
        fi

        agent_count=$((agent_count + 1))
    done

    log_success "Synced $agent_count agents to $target"
}

# Function to list agents
list_agents() {
    echo "=== Agents in $SOURCE_DIR ==="
    for agent_file in "$SOURCE_DIR"/*.md "$SOURCE_DIR"/*.yaml; do
        [ -f "$agent_file" ] || continue
        local name=$(basename "$agent_file")
        echo "  - $name"
    done | sort
}

# Function to show sync status
show_status() {
    echo "=== Agent Sync Status ==="
    echo ""
    echo "Canonical source: $SOURCE_DIR"
    local source_count=0
    for f in "$SOURCE_DIR"/*.md "$SOURCE_DIR"/*.yaml; do
        [ -f "$f" ] && source_count=$((source_count + 1))
    done
    echo "  Agents: $source_count"
    echo ""

    for target in $TARGETS; do
        local target_dir=$(get_target_dir "$target")
        local method=$(get_sync_method "$target")

        if [ -d "$target_dir" ]; then
            local count=0
            local symlink_count=0
            for f in "$target_dir"/*.md "$target_dir"/*.yaml; do
                [ -f "$f" ] 2>/dev/null && count=$((count + 1))
                [ -L "$f" ] 2>/dev/null && symlink_count=$((symlink_count + 1))
            done

            if [ "$method" = "symlink" ]; then
                echo "  $target: $count agents ($symlink_count symlinks)"
            else
                echo "  $target: $count agents (copies)"
            fi
        else
            echo "  $target: (not initialized)"
        fi
    done
}

# Print usage
usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Syncs agents from .claude/agents/ (canonical source) to other AI tool directories."
    echo ""
    echo "Options:"
    echo "  --all              Sync to all targets"
    echo "  --target TARGET    Sync to specific target (opencode, codex, cursor, gemini, kilocode, factory)"
    echo "  --list             List available agents"
    echo "  --status           Show sync status"
    echo "  --dry-run          Show what would be synced without making changes"
    echo "  --help             Show this help"
    echo ""
    echo "Sync Methods:"
    echo "  opencode  -> symlinks (agents stay in sync automatically)"
    echo "  codex     -> copies (Codex CLI reads from ~/.codex/ or .codex/)"
    echo "  cursor    -> copies"
    echo "  others    -> copies"
    echo ""
    echo "Examples:"
    echo "  $0 --all                    # Sync to all IDE directories"
    echo "  $0 --target opencode        # Sync only to .opencode/agent/"
    echo "  $0 --status                 # Show current sync status"
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
            list_agents
            exit 0
            ;;
        --status)
            show_status
            exit 0
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Validate source directory
if [ ! -d "$SOURCE_DIR" ]; then
    log_error "Source directory not found: $SOURCE_DIR"
    log_error "Please ensure .claude/agents/ exists with agent definitions"
    exit 1
fi

# Count source agents
TOTAL_AGENTS=0
for f in "$SOURCE_DIR"/*.md "$SOURCE_DIR"/*.yaml; do
    [ -f "$f" ] && TOTAL_AGENTS=$((TOTAL_AGENTS + 1))
done
log_info "Found $TOTAL_AGENTS agents in $SOURCE_DIR"

# Sync
if [ "$SYNC_ALL" = true ]; then
    for target in $TARGETS; do
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
show_status
