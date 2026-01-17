#!/bin/bash
# Post-test traceability hook for Claude Code
# Validates test-story linkage after test runs

set -e

# Parse hook input from stdin
INPUT=$(cat)

# Get the exit code from the test run
EXIT_CODE=$(echo "$INPUT" | jq -r '.tool_result.exit_code // 0')

if [ "$EXIT_CODE" -ne 0 ]; then
    echo "INFO: Tests failed - skipping traceability check" >&2
    exit 0
fi

# Check if traceability script exists
TRACEABILITY_SCRIPT="scripts/traceability/traceability.py"

if [ ! -f "$TRACEABILITY_SCRIPT" ]; then
    echo "INFO: Traceability script not found at $TRACEABILITY_SCRIPT" >&2
    exit 0
fi

# Run traceability validation
echo "Running traceability validation..." >&2

if python3 "$TRACEABILITY_SCRIPT" validate --quiet 2>/dev/null; then
    echo "Traceability: All stories have linked tests" >&2
else
    echo "WARNING: Traceability issues detected" >&2
    echo "Run: python3 $TRACEABILITY_SCRIPT validate" >&2
    echo "Some stories may be missing test coverage" >&2
fi

exit 0
