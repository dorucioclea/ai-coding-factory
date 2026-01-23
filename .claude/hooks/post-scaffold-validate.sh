#!/bin/bash
# Post-Scaffold Validate Hook
# Run after `/scaffold` command to validate generated project structure
# Part of AI Coding Factory governance framework

set -e

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "========================================="
echo "Post-Scaffold Validation"
echo "========================================="

ERRORS=0
WARNINGS=0
CHECKS_PASSED=0

# Get the scaffolded project directory (from arguments or current directory)
PROJECT_DIR="${1:-.}"
PROJECT_NAME="${2:-$(basename "$PROJECT_DIR")}"

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
    ((CHECKS_PASSED++))
}

# 1. Validate generated project structure
echo ""
echo "Validating project structure..."

# Check for solution file
SLN_FILE=$(find "$PROJECT_DIR" -maxdepth 2 -name "*.sln" 2>/dev/null | head -1)
if [ -n "$SLN_FILE" ]; then
    log_success "Solution file found: $SLN_FILE"
else
    log_error "No .sln file found in scaffolded project"
fi

# 2. Check all required files exist
echo ""
echo "Checking required files..."

REQUIRED_FILES=(
    "README.md"
    ".gitignore"
    "Directory.Build.props"
)

for file in "${REQUIRED_FILES[@]}"; do
    if [ -f "$PROJECT_DIR/$file" ]; then
        log_success "Found: $file"
    else
        log_warning "Missing: $file"
    fi
done

# 3. Verify Clean Architecture layers
echo ""
echo "Verifying Clean Architecture layers..."

EXPECTED_LAYERS=("Domain" "Application" "Infrastructure" "Api" "WebApi")
FOUND_LAYERS=0

for layer in "${EXPECTED_LAYERS[@]}"; do
    LAYER_PROJ=$(find "$PROJECT_DIR" -name "*.$layer.csproj" -o -name "*$layer*.csproj" 2>/dev/null | head -1)
    if [ -n "$LAYER_PROJ" ]; then
        log_success "Layer found: $layer"
        ((FOUND_LAYERS++))
    fi
done

if [ $FOUND_LAYERS -lt 3 ]; then
    log_warning "Expected at least 3 Clean Architecture layers (Domain, Application, Infrastructure/Api)"
fi

# Check src directory structure
if [ -d "$PROJECT_DIR/src" ]; then
    log_success "src/ directory exists"
else
    log_warning "No src/ directory. Consider organizing code under src/"
fi

# Check tests directory structure
if [ -d "$PROJECT_DIR/tests" ]; then
    log_success "tests/ directory exists"
else
    log_warning "No tests/ directory. Test projects should be in tests/"
fi

# 4. Validate project file contents
echo ""
echo "Validating project configurations..."

# Check for .NET 8 target
CSPROJ_FILES=$(find "$PROJECT_DIR" -name "*.csproj" 2>/dev/null)
NET8_COUNT=0
TOTAL_PROJECTS=0

echo "$CSPROJ_FILES" | while read csproj; do
    if [ -n "$csproj" ]; then
        ((TOTAL_PROJECTS++))
        if grep -q "net8.0" "$csproj" 2>/dev/null; then
            ((NET8_COUNT++))
        fi
    fi
done

# General .NET 8 check
if echo "$CSPROJ_FILES" | xargs grep -l "net8.0" 2>/dev/null | head -1 > /dev/null; then
    log_success ".NET 8 target framework configured"
else
    log_error "Projects not targeting .NET 8. Expected <TargetFramework>net8.0</TargetFramework>"
fi

# Check for nullable enabled
if echo "$CSPROJ_FILES" | xargs grep -l "<Nullable>enable</Nullable>" 2>/dev/null | head -1 > /dev/null; then
    log_success "Nullable reference types enabled"
else
    log_warning "Nullable reference types not enabled. Add <Nullable>enable</Nullable>"
fi

# Check for implicit usings
if echo "$CSPROJ_FILES" | xargs grep -l "<ImplicitUsings>enable</ImplicitUsings>" 2>/dev/null | head -1 > /dev/null; then
    log_success "Implicit usings enabled"
fi

# 5. Check for essential packages
echo ""
echo "Checking essential packages..."

ESSENTIAL_PACKAGES=(
    "MediatR:CQRS pattern"
    "FluentValidation:Input validation"
    "Serilog:Structured logging"
)

for pkg_info in "${ESSENTIAL_PACKAGES[@]}"; do
    PKG_NAME="${pkg_info%%:*}"
    PKG_DESC="${pkg_info##*:}"

    if echo "$CSPROJ_FILES" | xargs grep -l "$PKG_NAME" 2>/dev/null | head -1 > /dev/null; then
        log_success "Package found: $PKG_NAME ($PKG_DESC)"
    else
        log_info "Consider adding: $PKG_NAME for $PKG_DESC"
    fi
done

# 6. Run initial dotnet build to confirm compilable
echo ""
echo "Verifying project compiles..."

if [ -n "$SLN_FILE" ]; then
    if dotnet build "$SLN_FILE" --nologo -v quiet 2>&1; then
        log_success "Project builds successfully"
    else
        log_error "Project failed to build. Review build errors above"
    fi
else
    # Try building from project directory
    if dotnet build "$PROJECT_DIR" --nologo -v quiet 2>&1; then
        log_success "Project builds successfully"
    else
        log_error "Project failed to build. Review build errors above"
    fi
fi

# 7. Check for missing components
echo ""
echo "Checking for missing components..."

# Check for Program.cs or Startup.cs in API project
API_PROJ_DIR=$(find "$PROJECT_DIR" -name "*.Api.csproj" -o -name "*.WebApi.csproj" 2>/dev/null | head -1 | xargs dirname 2>/dev/null)
if [ -n "$API_PROJ_DIR" ]; then
    if [ -f "$API_PROJ_DIR/Program.cs" ]; then
        log_success "Program.cs found in API project"
    else
        log_error "Missing Program.cs in API project"
    fi
fi

# Check for appsettings.json
if find "$PROJECT_DIR" -name "appsettings.json" 2>/dev/null | head -1 > /dev/null; then
    log_success "appsettings.json found"
else
    log_warning "No appsettings.json found"
fi

# Check for appsettings.Development.json
if find "$PROJECT_DIR" -name "appsettings.Development.json" 2>/dev/null | head -1 > /dev/null; then
    log_success "appsettings.Development.json found"
else
    log_warning "No appsettings.Development.json found"
fi

# Check for launchSettings.json
if find "$PROJECT_DIR" -name "launchSettings.json" 2>/dev/null | head -1 > /dev/null; then
    log_success "launchSettings.json found"
else
    log_info "No launchSettings.json found. Consider adding for IDE support"
fi

# 8. Check test project setup
echo ""
echo "Checking test project setup..."

TEST_PROJECTS=$(find "$PROJECT_DIR" -name "*Tests*.csproj" -o -name "*Test*.csproj" 2>/dev/null | wc -l | tr -d ' ')
if [ "$TEST_PROJECTS" -gt 0 ]; then
    log_success "Found $TEST_PROJECTS test project(s)"

    # Check for xUnit
    if find "$PROJECT_DIR" -name "*Tests*.csproj" -exec grep -l "xunit" {} \; 2>/dev/null | head -1 > /dev/null; then
        log_success "xUnit test framework configured"
    fi
else
    log_warning "No test projects found. Consider adding unit tests"
fi

# Summary
echo ""
echo "========================================="
echo "Post-Scaffold Validation Summary"
echo "========================================="
echo "Project: $PROJECT_NAME"
echo "Checks passed: $CHECKS_PASSED"
echo "Errors:        $ERRORS"
echo "Warnings:      $WARNINGS"

if [ $ERRORS -gt 0 ]; then
    echo ""
    echo -e "${RED}Scaffold validation FAILED. Please fix errors before proceeding.${NC}"
    echo "The scaffolded project may not be ready for development."
    exit 1
else
    if [ $WARNINGS -gt 0 ]; then
        echo ""
        echo -e "${YELLOW}Scaffold validation passed with warnings.${NC}"
        echo "Review warnings above for potential improvements."
    else
        echo ""
        echo -e "${GREEN}Scaffold validation passed successfully!${NC}"
        echo "Project is ready for development."
    fi
    exit 0
fi
