#!/bin/bash
# Post-commit validation hook for Claude Code
# Validates commit messages follow story ID convention

set -e

# Parse hook input from stdin
INPUT=$(cat)

# Get the command that was executed
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Check if this is a commit command
if ! echo "$COMMAND" | grep -q "git commit"; then
    exit 0
fi

# Get the last commit message
LAST_COMMIT_MSG=$(git log -1 --format="%s" 2>/dev/null || echo "")

if [ -z "$LAST_COMMIT_MSG" ]; then
    exit 0
fi

# Check for story ID in commit message
if ! echo "$LAST_COMMIT_MSG" | grep -qE "^ACF-[0-9]+"; then
    echo "WARNING: Commit message does not start with story ID (ACF-###)" >&2
    echo "Commit: $LAST_COMMIT_MSG" >&2
    echo "Consider amending with: git commit --amend -m 'ACF-### $LAST_COMMIT_MSG'" >&2
fi

# Check for Co-Authored-By line
FULL_COMMIT_MSG=$(git log -1 --format="%B" 2>/dev/null || echo "")
if ! echo "$FULL_COMMIT_MSG" | grep -q "Co-Authored-By:"; then
    echo "INFO: Consider adding Co-Authored-By for AI attribution" >&2
fi

exit 0
