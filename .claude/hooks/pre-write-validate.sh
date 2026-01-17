#!/bin/bash
# Pre-write validation hook for Claude Code
# Validates file writes don't violate security or policy rules

set -e

# Parse hook input from stdin
INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')

if [ -z "$FILE_PATH" ]; then
    exit 0
fi

# Check for secrets patterns in the content
CONTENT=$(echo "$INPUT" | jq -r '.tool_input.content // empty')

# Patterns that should never be in code
FORBIDDEN_PATTERNS=(
    "password\s*=\s*['\"][^'\"]+['\"]"
    "api[_-]?key\s*=\s*['\"][^'\"]+['\"]"
    "secret\s*=\s*['\"][^'\"]+['\"]"
    "private[_-]?key"
    "-----BEGIN.*PRIVATE KEY-----"
    "AKIA[0-9A-Z]{16}"  # AWS access key
    "ghp_[a-zA-Z0-9]{36}"  # GitHub personal access token
)

for pattern in "${FORBIDDEN_PATTERNS[@]}"; do
    if echo "$CONTENT" | grep -qiE "$pattern"; then
        echo "BLOCKED: Potential secret detected in file content" >&2
        echo "Pattern matched: $pattern" >&2
        echo "Please use environment variables or .env.example for configuration" >&2
        exit 1
    fi
done

# Check that .env files are not being created (only .env.example allowed)
if [[ "$FILE_PATH" =~ \.env$ ]] && [[ ! "$FILE_PATH" =~ \.env\.example$ ]]; then
    echo "BLOCKED: Cannot create .env files directly" >&2
    echo "Use .env.example as a template instead" >&2
    exit 1
fi

# Warn about files that should have story IDs
if [[ "$FILE_PATH" =~ \.(cs|md)$ ]]; then
    # Check if it's a test file that should have story traits
    if [[ "$FILE_PATH" =~ [Tt]est.*\.cs$ ]] || [[ "$FILE_PATH" =~ \.Tests?/.*\.cs$ ]]; then
        if ! echo "$CONTENT" | grep -q 'Trait.*Story.*ACF-'; then
            echo "WARNING: Test file may be missing Story trait (ACF-###)" >&2
            echo "Consider adding: [Trait(\"Story\", \"ACF-###\")]" >&2
        fi
    fi
fi

exit 0
