#!/bin/bash
# Hook: Log PR URL and provide review command after PR creation
# Runs after: gh pr create

# Read stdin (hook input)
INPUT=$(cat)

# Extract PR URL from output
PR_URL=$(echo "$INPUT" | grep -o 'https://github.com/[^/]*/[^/]*/pull/[0-9]*' | head -1)

if [ -n "$PR_URL" ]; then
    echo "[Hook] PR created: $PR_URL" >&2
    REPO=$(echo "$PR_URL" | sed 's|https://github.com/\([^/]*/[^/]*\)/pull/.*|\1|')
    PR_NUM=$(echo "$PR_URL" | sed 's|.*/pull/\([0-9]*\)|\1|')
    echo "[Hook] To review: gh pr review $PR_NUM --repo $REPO" >&2
fi

# Pass through the input
echo "$INPUT"
