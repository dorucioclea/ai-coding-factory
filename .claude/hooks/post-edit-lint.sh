#!/bin/bash
# Hook: Lint and check edited files
# Runs after: Edit tool

# Read stdin (hook input with file_path)
INPUT=$(cat)

# Extract file path from JSON input
FILE_PATH=$(echo "$INPUT" | grep -o '"file_path"[[:space:]]*:[[:space:]]*"[^"]*"' | sed 's/.*"\([^"]*\)"$/\1/')

if [ -z "$FILE_PATH" ] || [ ! -f "$FILE_PATH" ]; then
    echo "$INPUT"
    exit 0
fi

# Check for console.log in JS/TS files
if [[ "$FILE_PATH" =~ \.(ts|tsx|js|jsx)$ ]]; then
    if grep -n "console.log" "$FILE_PATH" 2>/dev/null; then
        echo "[Hook] WARNING: console.log found in $FILE_PATH - remove before committing" >&2
    fi
fi

# Check for TODO/FIXME
if grep -n "TODO\|FIXME" "$FILE_PATH" 2>/dev/null | head -3; then
    echo "[Hook] NOTE: TODO/FIXME markers found in $FILE_PATH" >&2
fi

# Pass through the input
echo "$INPUT"
