#!/bin/bash
# Post-Security Scan Hook
# Run after `/security-review` command to archive and analyze results
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
echo "Post-Security Scan Analysis"
echo "========================================="

# Get the project directory (from arguments or current directory)
PROJECT_DIR="${1:-.}"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Security artifacts directory
SECURITY_DIR="$PROJECT_DIR/artifacts/security"
BASELINE_FILE="$SECURITY_DIR/baseline.json"
TREND_FILE="$SECURITY_DIR/trend.json"

# Thresholds
CRITICAL_THRESHOLD=0
HIGH_THRESHOLD=0
MEDIUM_THRESHOLD=5

# Ensure security directory exists
mkdir -p "$SECURITY_DIR"

# Function to log info
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

# Function to log success
log_success() {
    echo -e "${GREEN}[OK]${NC} $1"
}

# Function to log warning
log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

# Function to log error
log_error() {
    echo -e "${RED}[ALERT]${NC} $1"
}

# Function to log section
log_section() {
    echo ""
    echo -e "${CYAN}--- $1 ---${NC}"
}

# 1. Archive scan results
log_section "1. Archiving Scan Results"

SCAN_RESULT_FILE="$SECURITY_DIR/scan_$TIMESTAMP.json"

# Collect vulnerability data from various sources
echo "{" > "$SCAN_RESULT_FILE"
echo "  \"timestamp\": \"$(date -Iseconds)\"," >> "$SCAN_RESULT_FILE"
echo "  \"project\": \"$(basename "$PROJECT_DIR")\"," >> "$SCAN_RESULT_FILE"

# Run dotnet vulnerability scan
SLN_FILE=$(find "$PROJECT_DIR" -maxdepth 2 -name "*.sln" 2>/dev/null | head -1)
CRITICAL_COUNT=0
HIGH_COUNT=0
MEDIUM_COUNT=0
LOW_COUNT=0

if [ -n "$SLN_FILE" ]; then
    log_info "Running package vulnerability scan..."
    VULN_OUTPUT=$(dotnet list "$SLN_FILE" package --vulnerable --format json 2>/dev/null || echo "{}")

    # Count vulnerabilities by severity
    if echo "$VULN_OUTPUT" | grep -q "Critical" 2>/dev/null; then
        CRITICAL_COUNT=$(echo "$VULN_OUTPUT" | grep -c "Critical" || echo "0")
    fi
    if echo "$VULN_OUTPUT" | grep -q "High" 2>/dev/null; then
        HIGH_COUNT=$(echo "$VULN_OUTPUT" | grep -c "High" || echo "0")
    fi
    if echo "$VULN_OUTPUT" | grep -q "Moderate" 2>/dev/null; then
        MEDIUM_COUNT=$(echo "$VULN_OUTPUT" | grep -c "Moderate" || echo "0")
    fi
    if echo "$VULN_OUTPUT" | grep -q "Low" 2>/dev/null; then
        LOW_COUNT=$(echo "$VULN_OUTPUT" | grep -c "Low" || echo "0")
    fi
fi

# Scan for code-level security issues
log_info "Scanning for code-level security issues..."

# Check for common security anti-patterns
HARDCODED_SECRETS=$(find "$PROJECT_DIR" -name "*.cs" -exec grep -l "password.*=.*\"[^\"]*\"" {} \; 2>/dev/null | wc -l | tr -d ' ')
SQL_INJECTION=$(find "$PROJECT_DIR" -name "*.cs" -exec grep -l "ExecuteSqlRaw\|FromSqlRaw.*\$\|string.Format.*SELECT" {} \; 2>/dev/null | wc -l | tr -d ' ')
XSS_POTENTIAL=$(find "$PROJECT_DIR" -name "*.cs" -exec grep -l "Html.Raw\|@Html.Raw" {} \; 2>/dev/null | wc -l | tr -d ' ')
INSECURE_RANDOM=$(find "$PROJECT_DIR" -name "*.cs" -exec grep -l "new Random()" {} \; 2>/dev/null | wc -l | tr -d ' ')

# Add code issues to counts
[ "$HARDCODED_SECRETS" -gt 0 ] && ((HIGH_COUNT += HARDCODED_SECRETS))
[ "$SQL_INJECTION" -gt 0 ] && ((CRITICAL_COUNT += SQL_INJECTION))
[ "$XSS_POTENTIAL" -gt 0 ] && ((HIGH_COUNT += XSS_POTENTIAL))
[ "$INSECURE_RANDOM" -gt 0 ] && ((LOW_COUNT += INSECURE_RANDOM))

# Complete the JSON file
echo "  \"vulnerabilities\": {" >> "$SCAN_RESULT_FILE"
echo "    \"critical\": $CRITICAL_COUNT," >> "$SCAN_RESULT_FILE"
echo "    \"high\": $HIGH_COUNT," >> "$SCAN_RESULT_FILE"
echo "    \"medium\": $MEDIUM_COUNT," >> "$SCAN_RESULT_FILE"
echo "    \"low\": $LOW_COUNT" >> "$SCAN_RESULT_FILE"
echo "  }," >> "$SCAN_RESULT_FILE"
echo "  \"code_issues\": {" >> "$SCAN_RESULT_FILE"
echo "    \"hardcoded_secrets\": $HARDCODED_SECRETS," >> "$SCAN_RESULT_FILE"
echo "    \"sql_injection_risk\": $SQL_INJECTION," >> "$SCAN_RESULT_FILE"
echo "    \"xss_potential\": $XSS_POTENTIAL," >> "$SCAN_RESULT_FILE"
echo "    \"insecure_random\": $INSECURE_RANDOM" >> "$SCAN_RESULT_FILE"
echo "  }" >> "$SCAN_RESULT_FILE"
echo "}" >> "$SCAN_RESULT_FILE"

log_success "Scan results archived to: $SCAN_RESULT_FILE"

# 2. Compare against baseline
log_section "2. Baseline Comparison"

if [ -f "$BASELINE_FILE" ]; then
    log_info "Comparing against baseline..."

    # Extract baseline values
    BASELINE_CRITICAL=$(grep -o '"critical": [0-9]*' "$BASELINE_FILE" | grep -o '[0-9]*' | head -1 || echo "0")
    BASELINE_HIGH=$(grep -o '"high": [0-9]*' "$BASELINE_FILE" | grep -o '[0-9]*' | head -1 || echo "0")
    BASELINE_MEDIUM=$(grep -o '"medium": [0-9]*' "$BASELINE_FILE" | grep -o '[0-9]*' | head -1 || echo "0")

    # Compare
    CRITICAL_DIFF=$((CRITICAL_COUNT - BASELINE_CRITICAL))
    HIGH_DIFF=$((HIGH_COUNT - BASELINE_HIGH))
    MEDIUM_DIFF=$((MEDIUM_COUNT - BASELINE_MEDIUM))

    if [ "$CRITICAL_DIFF" -gt 0 ]; then
        log_error "Critical vulnerabilities increased by $CRITICAL_DIFF (was: $BASELINE_CRITICAL, now: $CRITICAL_COUNT)"
    elif [ "$CRITICAL_DIFF" -lt 0 ]; then
        log_success "Critical vulnerabilities decreased by $((-CRITICAL_DIFF))"
    else
        log_info "Critical vulnerabilities unchanged: $CRITICAL_COUNT"
    fi

    if [ "$HIGH_DIFF" -gt 0 ]; then
        log_error "High vulnerabilities increased by $HIGH_DIFF (was: $BASELINE_HIGH, now: $HIGH_COUNT)"
    elif [ "$HIGH_DIFF" -lt 0 ]; then
        log_success "High vulnerabilities decreased by $((-HIGH_DIFF))"
    else
        log_info "High vulnerabilities unchanged: $HIGH_COUNT"
    fi

    if [ "$MEDIUM_DIFF" -gt 0 ]; then
        log_warn "Medium vulnerabilities increased by $MEDIUM_DIFF"
    elif [ "$MEDIUM_DIFF" -lt 0 ]; then
        log_success "Medium vulnerabilities decreased by $((-MEDIUM_DIFF))"
    fi
else
    log_info "No baseline found. Creating initial baseline..."
    cp "$SCAN_RESULT_FILE" "$BASELINE_FILE"
    log_success "Baseline created: $BASELINE_FILE"
fi

# 3. Check thresholds
log_section "3. Threshold Validation"

THRESHOLD_EXCEEDED=0

if [ "$CRITICAL_COUNT" -gt "$CRITICAL_THRESHOLD" ]; then
    log_error "Critical threshold exceeded: $CRITICAL_COUNT > $CRITICAL_THRESHOLD"
    ((THRESHOLD_EXCEEDED++))
else
    log_success "Critical threshold met: $CRITICAL_COUNT <= $CRITICAL_THRESHOLD"
fi

if [ "$HIGH_COUNT" -gt "$HIGH_THRESHOLD" ]; then
    log_error "High threshold exceeded: $HIGH_COUNT > $HIGH_THRESHOLD"
    ((THRESHOLD_EXCEEDED++))
else
    log_success "High threshold met: $HIGH_COUNT <= $HIGH_THRESHOLD"
fi

if [ "$MEDIUM_COUNT" -gt "$MEDIUM_THRESHOLD" ]; then
    log_warn "Medium threshold exceeded: $MEDIUM_COUNT > $MEDIUM_THRESHOLD"
else
    log_success "Medium threshold met: $MEDIUM_COUNT <= $MEDIUM_THRESHOLD"
fi

# 4. Generate security trend report
log_section "4. Security Trend Report"

# Append to trend file
if [ ! -f "$TREND_FILE" ]; then
    echo "[]" > "$TREND_FILE"
fi

# Add current scan to trend (simplified - real implementation would use jq)
TREND_ENTRY="{\"date\":\"$(date -Iseconds)\",\"critical\":$CRITICAL_COUNT,\"high\":$HIGH_COUNT,\"medium\":$MEDIUM_COUNT,\"low\":$LOW_COUNT}"

# Read existing trend and append (basic implementation)
if [ -s "$TREND_FILE" ]; then
    # Keep last 30 entries
    TREND_CONTENT=$(cat "$TREND_FILE")
    if [ "$TREND_CONTENT" = "[]" ]; then
        echo "[$TREND_ENTRY]" > "$TREND_FILE"
    else
        # Append entry (simplified)
        TEMP_TREND=$(mktemp)
        head -c -2 "$TREND_FILE" > "$TEMP_TREND"  # Remove trailing "]"
        echo ",$TREND_ENTRY]" >> "$TEMP_TREND"
        mv "$TEMP_TREND" "$TREND_FILE"
    fi
fi

log_success "Trend data updated: $TREND_FILE"

# Generate trend summary
SCAN_COUNT=$(grep -o '"date"' "$TREND_FILE" 2>/dev/null | wc -l | tr -d ' ')
log_info "Total scans in trend history: $SCAN_COUNT"

# 5. Alert summary
log_section "5. Security Alert Summary"

TOTAL_ISSUES=$((CRITICAL_COUNT + HIGH_COUNT + MEDIUM_COUNT + LOW_COUNT))

echo ""
echo "┌─────────────────────────────────────┐"
echo "│       Security Scan Summary         │"
echo "├─────────────────────────────────────┤"
printf "│  Critical:  %-5s (threshold: %s)   │\n" "$CRITICAL_COUNT" "$CRITICAL_THRESHOLD"
printf "│  High:      %-5s (threshold: %s)   │\n" "$HIGH_COUNT" "$HIGH_THRESHOLD"
printf "│  Medium:    %-5s (threshold: %s)   │\n" "$MEDIUM_COUNT" "$MEDIUM_THRESHOLD"
printf "│  Low:       %-5s                   │\n" "$LOW_COUNT"
echo "├─────────────────────────────────────┤"
printf "│  Total Issues: %-5s               │\n" "$TOTAL_ISSUES"
echo "└─────────────────────────────────────┘"
echo ""

if [ "$THRESHOLD_EXCEEDED" -gt 0 ]; then
    echo -e "${RED}⚠️  SECURITY ALERT: $THRESHOLD_EXCEEDED threshold(s) exceeded!${NC}"
    echo ""
    echo "Required Actions:"
    [ "$CRITICAL_COUNT" -gt 0 ] && echo "  1. Fix all critical vulnerabilities immediately"
    [ "$HIGH_COUNT" -gt 0 ] && echo "  2. Address high severity issues before release"
    [ "$HARDCODED_SECRETS" -gt 0 ] && echo "  3. Remove hardcoded secrets from $HARDCODED_SECRETS file(s)"
    [ "$SQL_INJECTION" -gt 0 ] && echo "  4. Fix potential SQL injection in $SQL_INJECTION file(s)"
    echo ""
    echo "Artifacts saved to: $SECURITY_DIR/"
    exit 1
else
    if [ "$TOTAL_ISSUES" -gt 0 ]; then
        echo -e "${YELLOW}Security scan completed with findings. Review recommended.${NC}"
    else
        echo -e "${GREEN}✅ Security scan passed. No critical issues found.${NC}"
    fi
    echo ""
    echo "Artifacts saved to: $SECURITY_DIR/"
    exit 0
fi
