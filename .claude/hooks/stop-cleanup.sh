#!/bin/bash
# Hook: Cleanup checks after each response
# Runs on: Stop event

# Read stdin
INPUT=$(cat)

# Check for console.log in modified JS/TS files
if git rev-parse --git-dir > /dev/null 2>&1; then
    MODIFIED_FILES=$(git diff --name-only HEAD 2>/dev/null | grep -E '\.(ts|tsx|js|jsx)$')

    for FILE in $MODIFIED_FILES; do
        if [ -f "$FILE" ] && grep -q "console.log" "$FILE" 2>/dev/null; then
            echo "[Hook] WARNING: console.log found in $FILE" >&2
        fi
    done
fi

# Pass through
echo "$INPUT"
