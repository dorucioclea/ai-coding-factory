#!/bin/bash
# Post-Build Analyze Hook
# Run after `dotnet build` to check for warnings and architecture violations
# Part of AI Coding Factory governance framework

set -e

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "========================================="
echo "Post-Build Analysis"
echo "========================================="

WARNINGS=0
ISSUES=0

# Get the project directory (from arguments or current directory)
PROJECT_DIR="${1:-.}"

# Function to log issues
log_issue() {
    echo -e "${RED}ISSUE: $1${NC}"
    ((ISSUES++))
}

# Function to log warnings
log_warning() {
    echo -e "${YELLOW}WARNING: $1${NC}"
    ((WARNINGS++))
}

# Function to log info
log_info() {
    echo -e "${BLUE}INFO: $1${NC}"
}

# Function to log success
log_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# 1. Check for compiler warnings in build output
echo ""
echo "Checking for compiler warnings..."

# Look for recent build logs
BUILD_LOG_PATTERN="$PROJECT_DIR/**/msbuild*.log"
BINLOG_PATTERN="$PROJECT_DIR/**/msbuild.binlog"

# Try to find warnings from obj folder
WARNING_COUNT=0
find "$PROJECT_DIR" -path "*/obj/*" -name "*.AssemblyInfo.cs" -exec dirname {} \; 2>/dev/null | head -1 | while read dir; do
    if [ -d "$dir" ]; then
        # Check for warning files or logs
        if ls "$dir"/*.cache 2>/dev/null | head -1 > /dev/null; then
            echo "  Build artifacts found in: $dir"
        fi
    fi
done

# Check for common warning patterns in code
echo ""
echo "Scanning for potential warning sources..."

# Check for nullable reference type issues
NULLABLE_ISSUES=$(find "$PROJECT_DIR" -name "*.cs" -exec grep -l "#nullable disable" {} \; 2>/dev/null | wc -l | tr -d ' ')
if [ "$NULLABLE_ISSUES" -gt 0 ]; then
    log_warning "Found $NULLABLE_ISSUES files with nullable disabled. Consider enabling for better null safety"
fi

# Check for TODO/HACK comments
TODO_COUNT=$(find "$PROJECT_DIR" -name "*.cs" -exec grep -c "// TODO\|// HACK\|// FIXME" {} \; 2>/dev/null | awk '{s+=$1} END {print s}' || echo "0")
if [ "$TODO_COUNT" -gt 0 ] 2>/dev/null; then
    log_warning "Found $TODO_COUNT TODO/HACK/FIXME comments in codebase"
fi

# 2. Run Architecture Tests (if available)
echo ""
echo "Checking for architecture tests..."

ARCH_TEST_PROJECT=$(find "$PROJECT_DIR" -name "*ArchitectureTests*.csproj" -o -name "*Architecture.Tests*.csproj" 2>/dev/null | head -1)

if [ -n "$ARCH_TEST_PROJECT" ]; then
    log_success "Architecture test project found: $ARCH_TEST_PROJECT"
    echo "  Run 'dotnet test $ARCH_TEST_PROJECT' to validate architecture"
else
    # Check if NetArchTest is referenced in any test project
    NETARCHTEST_REF=$(find "$PROJECT_DIR" -name "*.csproj" -exec grep -l "NetArchTest" {} \; 2>/dev/null | head -1)
    if [ -n "$NETARCHTEST_REF" ]; then
        log_success "NetArchTest reference found in: $NETARCHTEST_REF"
    else
        log_info "No dedicated architecture tests found. Consider adding NetArchTest for dependency validation"
    fi
fi

# 3. Report layer dependency violations (basic check)
echo ""
echo "Checking Clean Architecture layer dependencies..."

DOMAIN_PROJ=$(find "$PROJECT_DIR" -name "*.Domain.csproj" 2>/dev/null | head -1)
APP_PROJ=$(find "$PROJECT_DIR" -name "*.Application.csproj" 2>/dev/null | head -1)
INFRA_PROJ=$(find "$PROJECT_DIR" -name "*.Infrastructure.csproj" 2>/dev/null | head -1)

if [ -n "$DOMAIN_PROJ" ]; then
    # Domain should have NO project references
    DOMAIN_REFS=$(grep -c "ProjectReference" "$DOMAIN_PROJ" 2>/dev/null || echo "0")
    if [ "$DOMAIN_REFS" -gt 0 ]; then
        log_issue "Domain layer has $DOMAIN_REFS project reference(s). Domain should have ZERO dependencies!"
        grep "ProjectReference" "$DOMAIN_PROJ" | sed 's/.*Include="\([^"]*\)".*/  - \1/'
    else
        log_success "Domain layer has no project dependencies (correct)"
    fi

    # Check Domain for infrastructure packages (violation)
    INFRA_PACKAGES=$(grep -E "EntityFramework|Npgsql|SqlClient|MongoDB|Redis" "$DOMAIN_PROJ" 2>/dev/null | wc -l | tr -d ' ')
    if [ "$INFRA_PACKAGES" -gt 0 ]; then
        log_issue "Domain layer references infrastructure packages!"
    fi
fi

if [ -n "$APP_PROJ" ]; then
    # Application should only reference Domain
    APP_REFS=$(grep "ProjectReference" "$APP_PROJ" 2>/dev/null | grep -v "Domain" | wc -l | tr -d ' ')
    if [ "$APP_REFS" -gt 0 ]; then
        log_issue "Application layer has non-Domain project reference(s)"
        grep "ProjectReference" "$APP_PROJ" | grep -v "Domain" | sed 's/.*Include="\([^"]*\)".*/  - \1/'
    else
        log_success "Application layer dependencies are valid"
    fi
fi

# 4. Check for common anti-patterns
echo ""
echo "Checking for common anti-patterns..."

# Check for Console.WriteLine in production code (not test)
CONSOLE_WRITES=$(find "$PROJECT_DIR" -name "*.cs" -not -path "*/tests/*" -not -path "*Test*" -exec grep -l "Console.WriteLine" {} \; 2>/dev/null | wc -l | tr -d ' ')
if [ "$CONSOLE_WRITES" -gt 0 ]; then
    log_warning "Found Console.WriteLine in $CONSOLE_WRITES production files. Use ILogger instead"
fi

# Check for hardcoded connection strings
HARDCODED_CONN=$(find "$PROJECT_DIR" -name "*.cs" -exec grep -l "Server=.*Database=" {} \; 2>/dev/null | wc -l | tr -d ' ')
if [ "$HARDCODED_CONN" -gt 0 ]; then
    log_issue "Potential hardcoded connection strings found in $HARDCODED_CONN file(s)"
fi

# 5. Generate build summary
echo ""
echo "========================================="
echo "Post-Build Analysis Summary"
echo "========================================="

# Count assemblies produced
DLL_COUNT=$(find "$PROJECT_DIR" -path "*/bin/*" -name "*.dll" 2>/dev/null | wc -l | tr -d ' ')
echo "Assemblies produced: $DLL_COUNT"
echo "Issues found:        $ISSUES"
echo "Warnings:           $WARNINGS"

# Output status
if [ $ISSUES -gt 0 ]; then
    echo ""
    echo -e "${RED}Post-build analysis found $ISSUES issue(s) that should be addressed.${NC}"
    echo "Review the issues above and consider fixing them."
    # Don't fail the build, but report issues
    exit 0
else
    if [ $WARNINGS -gt 0 ]; then
        echo ""
        echo -e "${YELLOW}Build successful with $WARNINGS warning(s).${NC}"
    else
        echo ""
        echo -e "${GREEN}Build analysis completed successfully. No issues found.${NC}"
    fi
    exit 0
fi
