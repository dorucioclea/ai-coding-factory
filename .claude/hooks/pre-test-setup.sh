#!/bin/bash
# Pre-Test Setup Hook
# Run before `dotnet test` to verify test prerequisites
# Part of AI Coding Factory governance framework

set -e

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "========================================="
echo "Pre-Test Setup Validation"
echo "========================================="

ERRORS=0
WARNINGS=0

# Get the project directory (from arguments or current directory)
PROJECT_DIR="${1:-.}"

# Function to log errors
log_error() {
    echo -e "${RED}ERROR: $1${NC}"
    ((ERRORS++))
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

# 1. Check if Docker is running (needed for TestContainers)
echo ""
echo "Checking Docker status..."

# Check if any test project uses TestContainers
USES_TESTCONTAINERS=$(find "$PROJECT_DIR" -name "*.csproj" -exec grep -l "Testcontainers\|TestContainers" {} \; 2>/dev/null | wc -l | tr -d ' ')

if [ "$USES_TESTCONTAINERS" -gt 0 ]; then
    log_info "TestContainers detected in $USES_TESTCONTAINERS project(s)"

    if command -v docker &> /dev/null; then
        if docker info &> /dev/null; then
            log_success "Docker is running"

            # Check Docker resources
            DOCKER_MEM=$(docker info 2>/dev/null | grep "Total Memory" | awk '{print $3, $4}')
            if [ -n "$DOCKER_MEM" ]; then
                echo "  Docker memory: $DOCKER_MEM"
            fi
        else
            log_error "Docker is installed but not running. TestContainers requires Docker to be running"
            echo "  Start Docker with: open -a Docker (macOS) or sudo systemctl start docker (Linux)"
        fi
    else
        log_error "Docker not found. TestContainers requires Docker to be installed"
    fi
else
    log_info "No TestContainers detected. Docker check skipped"
fi

# 2. Verify test database connection string (if applicable)
echo ""
echo "Checking test configuration..."

# Look for test settings files
TEST_SETTINGS=$(find "$PROJECT_DIR" -name "appsettings.Test.json" -o -name "appsettings.Testing.json" -o -name "testsettings.json" 2>/dev/null | head -5)

if [ -n "$TEST_SETTINGS" ]; then
    log_success "Test configuration file(s) found:"
    echo "$TEST_SETTINGS" | while read file; do
        echo "  - $file"

        # Check for placeholder values
        if grep -q "REPLACE_ME\|YOUR_CONNECTION_STRING\|localhost:5432\|Server=.\\\\" "$file" 2>/dev/null; then
            log_warning "Found placeholder values in $file. Ensure test database is configured"
        fi
    done
else
    log_info "No dedicated test settings files found. Tests may use in-memory databases"
fi

# Check for environment variables commonly used in tests
echo ""
echo "Checking test environment variables..."
TEST_ENV_VARS=("TEST_DATABASE_URL" "TEST_CONNECTION_STRING" "INTEGRATION_TEST_DB")
for var in "${TEST_ENV_VARS[@]}"; do
    if [ -n "${!var}" ]; then
        log_success "$var is set"
    fi
done

# 3. Ensure test fixtures are ready
echo ""
echo "Checking test fixtures..."

# Look for fixture/seed data files
FIXTURE_FILES=$(find "$PROJECT_DIR" -path "*/tests/*" \( -name "*.json" -o -name "*.sql" -o -name "*Fixture*" -o -name "*Seed*" \) 2>/dev/null | wc -l | tr -d ' ')

if [ "$FIXTURE_FILES" -gt 0 ]; then
    log_success "Found $FIXTURE_FILES test fixture/seed file(s)"
else
    log_info "No test fixture files detected"
fi

# Check for test base classes
TEST_BASE_CLASSES=$(find "$PROJECT_DIR" -path "*/tests/*" -name "*.cs" -exec grep -l "class.*TestBase\|class.*IntegrationTestBase" {} \; 2>/dev/null | wc -l | tr -d ' ')
if [ "$TEST_BASE_CLASSES" -gt 0 ]; then
    log_success "Test base classes found in $TEST_BASE_CLASSES file(s)"
fi

# 4. Check coverage tool availability
echo ""
echo "Checking coverage tools..."

# Check for Coverlet in test projects
COVERLET_REFS=$(find "$PROJECT_DIR" -name "*.csproj" -exec grep -l "coverlet" {} \; 2>/dev/null | wc -l | tr -d ' ')

if [ "$COVERLET_REFS" -gt 0 ]; then
    log_success "Coverlet referenced in $COVERLET_REFS project(s)"
else
    log_warning "Coverlet not found in test projects. Coverage reporting may not work"
    echo "  Add: <PackageReference Include=\"coverlet.collector\" Version=\"6.0.0\" />"
fi

# Check for reportgenerator
if command -v reportgenerator &> /dev/null; then
    log_success "ReportGenerator is installed (for HTML coverage reports)"
else
    log_info "ReportGenerator not installed. Install for HTML reports: dotnet tool install -g dotnet-reportgenerator-globaltool"
fi

# 5. Check test project structure
echo ""
echo "Checking test project structure..."

UNIT_TESTS=$(find "$PROJECT_DIR" -name "*UnitTests*.csproj" -o -name "*Unit.Tests*.csproj" 2>/dev/null | wc -l | tr -d ' ')
INT_TESTS=$(find "$PROJECT_DIR" -name "*IntegrationTests*.csproj" -o -name "*Integration.Tests*.csproj" 2>/dev/null | wc -l | tr -d ' ')
ARCH_TESTS=$(find "$PROJECT_DIR" -name "*ArchitectureTests*.csproj" -o -name "*Architecture.Tests*.csproj" 2>/dev/null | wc -l | tr -d ' ')

echo "  Unit test projects:         $UNIT_TESTS"
echo "  Integration test projects:  $INT_TESTS"
echo "  Architecture test projects: $ARCH_TESTS"

TOTAL_TESTS=$((UNIT_TESTS + INT_TESTS + ARCH_TESTS))
if [ "$TOTAL_TESTS" -eq 0 ]; then
    log_warning "No test projects found matching naming conventions"
fi

# 6. Verify xUnit packages
echo ""
echo "Checking test framework packages..."

XUNIT_REFS=$(find "$PROJECT_DIR" -name "*.csproj" -exec grep -l "xunit" {} \; 2>/dev/null | wc -l | tr -d ' ')
FLUENT_REFS=$(find "$PROJECT_DIR" -name "*.csproj" -exec grep -l "FluentAssertions" {} \; 2>/dev/null | wc -l | tr -d ' ')
MOQ_REFS=$(find "$PROJECT_DIR" -name "*.csproj" -exec grep -l "Moq\|NSubstitute" {} \; 2>/dev/null | wc -l | tr -d ' ')

[ "$XUNIT_REFS" -gt 0 ] && log_success "xUnit framework detected"
[ "$FLUENT_REFS" -gt 0 ] && log_success "FluentAssertions detected"
[ "$MOQ_REFS" -gt 0 ] && log_success "Mocking framework detected"

# Summary
echo ""
echo "========================================="
echo "Pre-Test Setup Summary"
echo "========================================="
echo "Errors:   $ERRORS"
echo "Warnings: $WARNINGS"

if [ $ERRORS -gt 0 ]; then
    echo ""
    echo -e "${RED}Pre-test setup check FAILED. Please fix errors before running tests.${NC}"
    exit 1
else
    if [ $WARNINGS -gt 0 ]; then
        echo ""
        echo -e "${YELLOW}Pre-test setup passed with warnings. Tests may still run but some features might not work.${NC}"
    else
        echo ""
        echo -e "${GREEN}Pre-test setup validated successfully. Ready to run tests.${NC}"
    fi
    exit 0
fi
