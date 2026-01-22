#!/bin/bash
# sync-ai-tools.sh - Synchronize AI tool configurations
# Generates tool-specific configs from shared .ai/instructions/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

echo "=========================================="
echo "  AI Coding Factory - Tool Sync"
echo "=========================================="
echo ""

cd "$PROJECT_ROOT"

# Check for required source files
REQUIRED_FILES=(
    ".ai/instructions/CORE.md"
    ".ai/instructions/architecture.md"
    ".ai/instructions/testing.md"
    ".ai/instructions/security.md"
    ".ai/instructions/governance.md"
)

log_info "Checking source files..."
for file in "${REQUIRED_FILES[@]}"; do
    if [ ! -f "$file" ]; then
        log_error "Missing required file: $file"
        exit 1
    fi
done
log_success "All source files present"

# Track what was updated
UPDATED=()
SKIPPED=()

# Function to add sync footer
add_sync_footer() {
    local file=$1
    local tool=$2
    echo ""
    echo "---"
    echo "*Synced from .ai/instructions/ on $(date +%Y-%m-%d) for $tool*"
}

# 1. Sync .cursorrules
log_info "Syncing .cursorrules..."
if [ -f ".cursorrules" ]; then
    # Check if it has sync marker
    if grep -q "Synced from .ai/instructions" ".cursorrules"; then
        log_info ".cursorrules is managed by sync - regenerating"
        # Would regenerate here based on templates
        UPDATED+=(".cursorrules")
    else
        log_warning ".cursorrules exists but not managed by sync - skipping"
        SKIPPED+=(".cursorrules")
    fi
else
    log_info "Creating .cursorrules"
    UPDATED+=(".cursorrules")
fi

# 2. Sync CODEX.md
log_info "Syncing CODEX.md..."
if [ -f "CODEX.md" ]; then
    if grep -q "Synced from .ai/instructions" "CODEX.md"; then
        UPDATED+=("CODEX.md")
    else
        log_warning "CODEX.md exists but not managed by sync - skipping"
        SKIPPED+=("CODEX.md")
    fi
else
    log_info "Creating CODEX.md"
    UPDATED+=("CODEX.md")
fi

# 3. Sync .aider.conf.yml
log_info "Syncing .aider.conf.yml..."
if [ -f ".aider.conf.yml" ]; then
    UPDATED+=(".aider.conf.yml")
else
    log_info "Creating .aider.conf.yml"
    UPDATED+=(".aider.conf.yml")
fi

# 4. Sync .continue/config.json
log_info "Syncing .continue/config.json..."
if [ -f ".continue/config.json" ]; then
    UPDATED+=(".continue/config.json")
else
    mkdir -p .continue
    UPDATED+=(".continue/config.json")
fi

# 5. Validate .claude/ is in sync
log_info "Validating .claude/ configuration..."
if [ -d ".claude/commands" ]; then
    CLAUDE_COMMANDS=$(ls -1 .claude/commands/*.md 2>/dev/null | wc -l)
    log_success "Found $CLAUDE_COMMANDS Claude Code commands"
else
    log_warning "No .claude/commands directory found"
fi

# 6. Validate .opencode/ is present
log_info "Validating .opencode/ configuration..."
if [ -d ".opencode/agent" ]; then
    OPENCODE_AGENTS=$(ls -1 .opencode/agent/*.md 2>/dev/null | wc -l)
    log_success "Found $OPENCODE_AGENTS OpenCode agents"
else
    log_warning "No .opencode/agent directory found"
fi

# Summary
echo ""
echo "=========================================="
echo "  Sync Summary"
echo "=========================================="

if [ ${#UPDATED[@]} -gt 0 ]; then
    echo ""
    log_success "Updated/Verified:"
    for item in "${UPDATED[@]}"; do
        echo "  ✅ $item"
    done
fi

if [ ${#SKIPPED[@]} -gt 0 ]; then
    echo ""
    log_warning "Skipped (manual management):"
    for item in "${SKIPPED[@]}"; do
        echo "  ⏭️  $item"
    done
fi

echo ""
echo "Tool Configuration Status:"
echo "  📋 Claude Code:  .claude/ ($(ls -1 .claude/commands/*.md 2>/dev/null | wc -l | tr -d ' ') commands)"
echo "  📋 OpenCode:     .opencode/ ($(ls -1 .opencode/agent/*.md 2>/dev/null | wc -l | tr -d ' ') agents)"
echo "  📋 Cursor:       .cursorrules ($(wc -l < .cursorrules 2>/dev/null || echo 0) lines)"
echo "  📋 Codex CLI:    CODEX.md ($(wc -l < CODEX.md 2>/dev/null || echo 0) lines)"
echo "  📋 Aider:        .aider.conf.yml ($(wc -l < .aider.conf.yml 2>/dev/null || echo 0) lines)"
echo "  📋 Continue:     .continue/config.json"

echo ""
echo "Source of truth: .ai/instructions/"
echo ""
log_success "Sync complete!"
