#!/bin/bash
# Pre-commit validation hook for Claude Code
# Validates changes before allowing commits

set -e

echo "Running pre-commit validation..."

# Get list of staged files
STAGED_FILES=$(git diff --cached --name-only --diff-filter=ACM)

if [ -z "$STAGED_FILES" ]; then
    echo "No staged files to validate"
    exit 0
fi

ERRORS=0
WARNINGS=0

# Check for story ID in commit message (if provided via stdin)
check_commit_message() {
    local msg="$1"
    if [ -n "$msg" ] && ! echo "$msg" | grep -qE "^ACF-[0-9]+"; then
        echo "WARNING: Commit message should start with story ID (ACF-###)"
        ((WARNINGS++))
    fi
}

# Check for secrets in staged files
check_for_secrets() {
    local file="$1"

    # Patterns that should never be in code
    local patterns=(
        "password\s*=\s*['\"][^'\"]{8,}['\"]"
        "api[_-]?key\s*=\s*['\"][^'\"]+['\"]"
        "secret\s*=\s*['\"][^'\"]+['\"]"
        "-----BEGIN.*PRIVATE KEY-----"
        "AKIA[0-9A-Z]{16}"
        "ghp_[a-zA-Z0-9]{36}"
        "sk_live_[a-zA-Z0-9]+"
    )

    for pattern in "${patterns[@]}"; do
        if git show ":$file" 2>/dev/null | grep -qiE "$pattern"; then
            echo "ERROR: Potential secret detected in $file"
            echo "  Pattern: $pattern"
            ((ERRORS++))
        fi
    done
}

# Check test files have story traits
check_test_story_traits() {
    local file="$1"

    if [[ "$file" =~ [Tt]est.*\.cs$ ]] || [[ "$file" =~ \.Tests?/.*\.cs$ ]]; then
        if ! git show ":$file" 2>/dev/null | grep -q 'Trait.*Story.*ACF-'; then
            echo "WARNING: Test file missing Story trait: $file"
            echo "  Add: [Trait(\"Story\", \"ACF-###\")]"
            ((WARNINGS++))
        fi
    fi
}

# Check for .env files (only .env.example allowed)
check_env_files() {
    local file="$1"

    if [[ "$file" =~ \.env$ ]] && [[ ! "$file" =~ \.env\.example$ ]]; then
        echo "ERROR: Cannot commit .env files directly: $file"
        echo "  Use .env.example instead"
        ((ERRORS++))
    fi
}

# Check C# files follow naming conventions
check_csharp_conventions() {
    local file="$1"

    if [[ "$file" =~ \.cs$ ]]; then
        # Check for Console.WriteLine
        if git show ":$file" 2>/dev/null | grep -q "Console\.WriteLine"; then
            echo "WARNING: Found Console.WriteLine in $file"
            echo "  Use ILogger instead"
            ((WARNINGS++))
        fi

        # Check for generic Exception
        if git show ":$file" 2>/dev/null | grep -qE "throw new Exception\("; then
            echo "WARNING: Generic Exception thrown in $file"
            echo "  Use specific exception types"
            ((WARNINGS++))
        fi
    fi
}

# Run checks on each staged file
echo "Checking ${#STAGED_FILES[@]} staged files..."

for file in $STAGED_FILES; do
    # Skip deleted files
    if [ ! -f "$file" ]; then
        continue
    fi

    check_for_secrets "$file"
    check_test_story_traits "$file"
    check_env_files "$file"
    check_csharp_conventions "$file"
done

# Summary
echo ""
echo "Pre-commit validation complete:"
echo "  Errors: $ERRORS"
echo "  Warnings: $WARNINGS"

if [ $ERRORS -gt 0 ]; then
    echo ""
    echo "Commit blocked due to errors. Please fix the issues above."
    exit 1
fi

if [ $WARNINGS -gt 0 ]; then
    echo ""
    echo "Commit allowed with warnings. Consider addressing the issues above."
fi

exit 0
