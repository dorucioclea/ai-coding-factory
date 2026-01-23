#!/bin/bash
# Pre-Release Checklist Hook
# Run before `/release` command to verify release readiness
# Part of AI Coding Factory governance framework

set -e

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo "========================================="
echo "Pre-Release Checklist"
echo "========================================="

PASSED=0
FAILED=0
WARNINGS=0

# Get the project directory (from arguments or current directory)
PROJECT_DIR="${1:-.}"

# Function to log pass
log_pass() {
    echo -e "${GREEN}[PASS]${NC} $1"
    ((PASSED++))
}

# Function to log fail
log_fail() {
    echo -e "${RED}[FAIL]${NC} $1"
    ((FAILED++))
}

# Function to log warning
log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
    ((WARNINGS++))
}

# Function to log info
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

# Function to log section
log_section() {
    echo ""
    echo -e "${CYAN}--- $1 ---${NC}"
}

# 1. Verify all tests pass
log_section "1. Test Verification"

SLN_FILE=$(find "$PROJECT_DIR" -maxdepth 2 -name "*.sln" 2>/dev/null | head -1)
if [ -n "$SLN_FILE" ]; then
    log_info "Running tests..."
    if dotnet test "$SLN_FILE" --nologo --no-build -v quiet 2>&1 | tee /tmp/test-output.txt | tail -5; then
        if grep -q "Passed!" /tmp/test-output.txt 2>/dev/null || grep -q "Test Run Successful" /tmp/test-output.txt 2>/dev/null; then
            TEST_SUMMARY=$(grep -E "Passed|Failed" /tmp/test-output.txt | tail -1)
            log_pass "All tests pass: $TEST_SUMMARY"
        else
            log_fail "Some tests failed. Review test output"
        fi
    else
        log_fail "Test execution failed"
    fi
    rm -f /tmp/test-output.txt
else
    log_warn "No solution file found. Cannot run tests automatically"
fi

# 2. Check coverage meets threshold (80%)
log_section "2. Code Coverage"

COVERAGE_THRESHOLD=80
COVERAGE_FILE=$(find "$PROJECT_DIR" -name "coverage.cobertura.xml" -o -name "coverage.xml" 2>/dev/null | head -1)

if [ -n "$COVERAGE_FILE" ]; then
    # Try to extract line coverage from Cobertura format
    if grep -q "line-rate" "$COVERAGE_FILE" 2>/dev/null; then
        LINE_RATE=$(grep -o 'line-rate="[^"]*"' "$COVERAGE_FILE" | head -1 | cut -d'"' -f2)
        if [ -n "$LINE_RATE" ]; then
            COVERAGE_PCT=$(echo "$LINE_RATE * 100" | bc 2>/dev/null | cut -d'.' -f1 || echo "0")
            if [ "$COVERAGE_PCT" -ge "$COVERAGE_THRESHOLD" ] 2>/dev/null; then
                log_pass "Code coverage: ${COVERAGE_PCT}% (threshold: ${COVERAGE_THRESHOLD}%)"
            else
                log_fail "Code coverage: ${COVERAGE_PCT}% is below threshold of ${COVERAGE_THRESHOLD}%"
            fi
        fi
    else
        log_warn "Coverage file format not recognized"
    fi
else
    log_warn "No coverage file found. Run tests with coverage collection first"
    echo "  Example: dotnet test --collect:\"XPlat Code Coverage\""
fi

# 3. Validate traceability (all stories have tests)
log_section "3. Traceability Validation"

STORY_DIR="$PROJECT_DIR/artifacts/stories"
if [ -d "$STORY_DIR" ]; then
    STORY_COUNT=$(find "$STORY_DIR" -name "ACF-*.md" 2>/dev/null | wc -l | tr -d ' ')
    log_info "Found $STORY_COUNT stories in $STORY_DIR"

    # Check for stories without linked tests
    UNLINKED_STORIES=0
    for story_file in $(find "$STORY_DIR" -name "ACF-*.md" 2>/dev/null); do
        STORY_ID=$(basename "$story_file" .md)
        # Search for story ID in test files
        TEST_REFS=$(find "$PROJECT_DIR" -name "*.cs" -path "*/tests/*" -exec grep -l "\"$STORY_ID\"" {} \; 2>/dev/null | wc -l | tr -d ' ')
        if [ "$TEST_REFS" -eq 0 ]; then
            ((UNLINKED_STORIES++))
            log_warn "Story $STORY_ID has no linked tests"
        fi
    done

    if [ "$UNLINKED_STORIES" -eq 0 ] && [ "$STORY_COUNT" -gt 0 ]; then
        log_pass "All stories have linked tests"
    elif [ "$UNLINKED_STORIES" -gt 0 ]; then
        log_fail "$UNLINKED_STORIES story(ies) without linked tests"
    fi
else
    log_warn "No stories directory found at $STORY_DIR"
fi

# 4. Check for security vulnerabilities
log_section "4. Security Check"

# Check for dotnet list package --vulnerable
if command -v dotnet &> /dev/null && [ -n "$SLN_FILE" ]; then
    log_info "Scanning for vulnerable packages..."
    VULN_OUTPUT=$(dotnet list "$SLN_FILE" package --vulnerable 2>&1 || echo "error")

    if echo "$VULN_OUTPUT" | grep -q "has no vulnerable packages" 2>/dev/null; then
        log_pass "No known vulnerable packages"
    elif echo "$VULN_OUTPUT" | grep -q "Critical\|High" 2>/dev/null; then
        CRITICAL_COUNT=$(echo "$VULN_OUTPUT" | grep -c "Critical" 2>/dev/null || echo "0")
        HIGH_COUNT=$(echo "$VULN_OUTPUT" | grep -c "High" 2>/dev/null || echo "0")
        log_fail "Found $CRITICAL_COUNT critical and $HIGH_COUNT high severity vulnerabilities"
    elif echo "$VULN_OUTPUT" | grep -q "Moderate\|Low" 2>/dev/null; then
        log_warn "Found moderate/low severity vulnerabilities. Consider updating packages"
    else
        log_pass "No vulnerable packages detected"
    fi
else
    log_warn "Cannot run vulnerability scan"
fi

# Check for hardcoded secrets
log_info "Scanning for potential secrets..."
SECRET_PATTERNS="password=\|apikey=\|secret=\|connectionstring=.*password"
SECRETS_FOUND=$(find "$PROJECT_DIR" -name "*.cs" -o -name "*.json" -o -name "*.config" 2>/dev/null | \
    xargs grep -il "$SECRET_PATTERNS" 2>/dev/null | \
    grep -v "appsettings.Development.json\|.example\|template" | wc -l | tr -d ' ')

if [ "$SECRETS_FOUND" -gt 0 ]; then
    log_fail "Potential hardcoded secrets found in $SECRETS_FOUND file(s)"
else
    log_pass "No obvious hardcoded secrets detected"
fi

# 5. Verify documentation is updated
log_section "5. Documentation Check"

README_FILE="$PROJECT_DIR/README.md"
if [ -f "$README_FILE" ]; then
    log_pass "README.md exists"

    # Check README was modified recently (within last 30 days)
    if [ "$(uname)" = "Darwin" ]; then
        README_MTIME=$(stat -f %m "$README_FILE")
        THIRTY_DAYS_AGO=$(($(date +%s) - 2592000))
    else
        README_MTIME=$(stat -c %Y "$README_FILE")
        THIRTY_DAYS_AGO=$(($(date +%s) - 2592000))
    fi

    if [ "$README_MTIME" -gt "$THIRTY_DAYS_AGO" ] 2>/dev/null; then
        log_pass "README.md updated within last 30 days"
    else
        log_warn "README.md not updated recently. Ensure documentation is current"
    fi
else
    log_fail "README.md not found"
fi

# Check for CHANGELOG
if [ -f "$PROJECT_DIR/CHANGELOG.md" ]; then
    log_pass "CHANGELOG.md exists"
else
    log_warn "No CHANGELOG.md found. Consider adding for release notes"
fi

# 6. Confirm version bump in .csproj
log_section "6. Version Check"

# Look for version in Directory.Build.props or .csproj files
VERSION_FILE="$PROJECT_DIR/Directory.Build.props"
if [ -f "$VERSION_FILE" ]; then
    VERSION=$(grep -o '<Version>[^<]*</Version>' "$VERSION_FILE" 2>/dev/null | head -1 | sed 's/<[^>]*>//g')
    if [ -n "$VERSION" ]; then
        log_pass "Version in Directory.Build.props: $VERSION"
    else
        log_warn "No <Version> tag in Directory.Build.props"
    fi
else
    # Check individual csproj files
    CSPROJ_VERSION=$(find "$PROJECT_DIR" -name "*.csproj" -exec grep -o '<Version>[^<]*</Version>' {} \; 2>/dev/null | head -1 | sed 's/<[^>]*>//g')
    if [ -n "$CSPROJ_VERSION" ]; then
        log_pass "Version found: $CSPROJ_VERSION"
    else
        log_warn "No version found in project files. Add <Version>x.y.z</Version>"
    fi
fi

# Check git tag vs version
if command -v git &> /dev/null && git rev-parse --git-dir > /dev/null 2>&1; then
    LATEST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
    if [ -n "$LATEST_TAG" ]; then
        log_info "Latest git tag: $LATEST_TAG"
    fi
fi

# 7. Final Summary
echo ""
echo "========================================="
echo "Pre-Release Checklist Summary"
echo "========================================="
echo -e "Passed:   ${GREEN}$PASSED${NC}"
echo -e "Failed:   ${RED}$FAILED${NC}"
echo -e "Warnings: ${YELLOW}$WARNINGS${NC}"
echo ""

TOTAL_CHECKS=$((PASSED + FAILED))
if [ $TOTAL_CHECKS -gt 0 ]; then
    PASS_RATE=$((PASSED * 100 / TOTAL_CHECKS))
    echo "Pass rate: $PASS_RATE%"
fi

echo ""
if [ $FAILED -gt 0 ]; then
    echo -e "${RED}❌ RELEASE BLOCKED${NC}"
    echo "Fix all failed checks before proceeding with release."
    echo ""
    echo "Checklist Status:"
    echo "  [ ] All tests pass"
    echo "  [ ] Coverage >= 80%"
    echo "  [ ] All stories have tests"
    echo "  [ ] No critical/high vulnerabilities"
    echo "  [ ] No hardcoded secrets"
    echo "  [ ] Documentation updated"
    echo "  [ ] Version bumped"
    exit 1
else
    if [ $WARNINGS -gt 0 ]; then
        echo -e "${YELLOW}⚠️  RELEASE READY WITH WARNINGS${NC}"
        echo "Review warnings before proceeding."
    else
        echo -e "${GREEN}✅ RELEASE READY${NC}"
        echo "All checks passed. Proceed with release."
    fi
    exit 0
fi
