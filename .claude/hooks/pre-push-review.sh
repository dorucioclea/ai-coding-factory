#!/bin/bash
# Hook: Review changes before git push
# Runs before: git push

echo "[Hook] Reviewing changes before push..." >&2

# Check for uncommitted changes
if [ -n "$(git status --porcelain)" ]; then
    echo "[Hook] WARNING: You have uncommitted changes" >&2
fi

# Check for TODO/FIXME in staged files
if git diff --cached --name-only | xargs grep -l "TODO\|FIXME" 2>/dev/null; then
    echo "[Hook] WARNING: TODO/FIXME found in staged files" >&2
fi

exit 0
