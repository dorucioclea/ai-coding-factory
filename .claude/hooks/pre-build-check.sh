#!/bin/bash
# Pre-Build Check Hook
# Run before `dotnet build` to validate project structure and dependencies
# Part of AI Coding Factory governance framework

set -e

# Colors for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

echo "========================================="
echo "Pre-Build Validation Check"
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

# Function to log success
log_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# 1. Check for .sln file
echo ""
echo "Checking for solution file..."
SLN_COUNT=$(find "$PROJECT_DIR" -maxdepth 2 -name "*.sln" 2>/dev/null | wc -l | tr -d ' ')
if [ "$SLN_COUNT" -eq 0 ]; then
    log_error "No .sln file found in project directory"
elif [ "$SLN_COUNT" -gt 1 ]; then
    log_warning "Multiple .sln files found. Ensure correct solution is being built"
    find "$PROJECT_DIR" -maxdepth 2 -name "*.sln" 2>/dev/null
else
    SLN_FILE=$(find "$PROJECT_DIR" -maxdepth 2 -name "*.sln" 2>/dev/null | head -1)
    log_success "Solution file found: $SLN_FILE"
fi

# 2. Check for .csproj files
echo ""
echo "Checking for project files..."
CSPROJ_COUNT=$(find "$PROJECT_DIR" -name "*.csproj" 2>/dev/null | wc -l | tr -d ' ')
if [ "$CSPROJ_COUNT" -eq 0 ]; then
    log_error "No .csproj files found"
else
    log_success "Found $CSPROJ_COUNT .csproj file(s)"
fi

# 3. Check if NuGet restore is needed
echo ""
echo "Checking NuGet packages..."
OBJ_DIRS=$(find "$PROJECT_DIR" -type d -name "obj" 2>/dev/null | wc -l | tr -d ' ')
if [ "$OBJ_DIRS" -eq 0 ] && [ "$CSPROJ_COUNT" -gt 0 ]; then
    log_warning "No obj directories found. Running 'dotnet restore' is recommended before build"
else
    # Check for project.assets.json in obj folders
    ASSETS_COUNT=$(find "$PROJECT_DIR" -path "*/obj/project.assets.json" 2>/dev/null | wc -l | tr -d ' ')
    if [ "$ASSETS_COUNT" -lt "$CSPROJ_COUNT" ]; then
        log_warning "Some projects may need restore. Found $ASSETS_COUNT project.assets.json for $CSPROJ_COUNT projects"
    else
        log_success "NuGet packages appear to be restored"
    fi
fi

# 4. Verify .NET SDK version compatibility
echo ""
echo "Checking .NET SDK version..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version 2>/dev/null || echo "unknown")
    MAJOR_VERSION=$(echo "$DOTNET_VERSION" | cut -d. -f1)

    if [ "$MAJOR_VERSION" -lt 8 ] 2>/dev/null; then
        log_error ".NET SDK version $DOTNET_VERSION is below required version 8.0"
    else
        log_success ".NET SDK version: $DOTNET_VERSION (>= 8.0 required)"
    fi

    # Check global.json if exists
    if [ -f "$PROJECT_DIR/global.json" ]; then
        REQUIRED_SDK=$(grep -o '"version"[[:space:]]*:[[:space:]]*"[^"]*"' "$PROJECT_DIR/global.json" 2>/dev/null | head -1 | cut -d'"' -f4)
        if [ -n "$REQUIRED_SDK" ]; then
            echo "  global.json requires SDK: $REQUIRED_SDK"
        fi
    fi
else
    log_error ".NET SDK not found in PATH"
fi

# 5. Check for common configuration issues
echo ""
echo "Checking project configuration..."

# Check for Directory.Build.props (recommended for multi-project solutions)
if [ "$CSPROJ_COUNT" -gt 1 ] && [ ! -f "$PROJECT_DIR/Directory.Build.props" ]; then
    log_warning "Multiple projects but no Directory.Build.props. Consider adding for consistent settings"
fi

# Check for duplicate package references
echo ""
echo "Checking for potential package issues..."
if [ "$CSPROJ_COUNT" -gt 0 ]; then
    # Look for packages that might cause version conflicts
    DUPLICATE_PACKAGES=$(find "$PROJECT_DIR" -name "*.csproj" -exec grep -h "PackageReference" {} \; 2>/dev/null | \
        sed -n 's/.*Include="\([^"]*\)".*/\1/p' | sort | uniq -d | head -5)

    if [ -n "$DUPLICATE_PACKAGES" ]; then
        echo "  Packages referenced in multiple projects (ensure version consistency):"
        echo "$DUPLICATE_PACKAGES" | while read pkg; do
            echo "    - $pkg"
        done
    fi
fi

# 6. Check Clean Architecture structure (if applicable)
echo ""
echo "Checking Clean Architecture structure..."
DOMAIN_PROJ=$(find "$PROJECT_DIR" -name "*.Domain.csproj" 2>/dev/null | head -1)
APP_PROJ=$(find "$PROJECT_DIR" -name "*.Application.csproj" 2>/dev/null | head -1)
INFRA_PROJ=$(find "$PROJECT_DIR" -name "*.Infrastructure.csproj" 2>/dev/null | head -1)
API_PROJ=$(find "$PROJECT_DIR" -name "*.Api.csproj" -o -name "*.WebApi.csproj" 2>/dev/null | head -1)

if [ -n "$DOMAIN_PROJ" ] || [ -n "$APP_PROJ" ]; then
    log_success "Clean Architecture structure detected"
    [ -n "$DOMAIN_PROJ" ] && echo "  Domain: $DOMAIN_PROJ"
    [ -n "$APP_PROJ" ] && echo "  Application: $APP_PROJ"
    [ -n "$INFRA_PROJ" ] && echo "  Infrastructure: $INFRA_PROJ"
    [ -n "$API_PROJ" ] && echo "  API: $API_PROJ"
fi

# Summary
echo ""
echo "========================================="
echo "Pre-Build Check Summary"
echo "========================================="
echo "Errors:   $ERRORS"
echo "Warnings: $WARNINGS"

if [ $ERRORS -gt 0 ]; then
    echo -e "${RED}Pre-build check FAILED. Please fix errors before building.${NC}"
    exit 1
else
    if [ $WARNINGS -gt 0 ]; then
        echo -e "${YELLOW}Pre-build check passed with warnings.${NC}"
    else
        echo -e "${GREEN}Pre-build check passed successfully.${NC}"
    fi
    exit 0
fi
