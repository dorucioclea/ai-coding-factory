# /traceability - Generate Traceability Report

Generate and validate the story → test → commit → release traceability chain.

## Usage
```
/traceability [action]
```

Where action is:
- `report` (default) - Generate full traceability report
- `validate` - Validate linkage without generating report
- `gaps` - Show only stories missing test coverage
- `release-notes` - Generate release notes from completed stories

## Instructions

### Generate Report (`/traceability` or `/traceability report`)

```bash
python3 scripts/traceability/traceability.py report \
  --story-root artifacts/stories \
  --test-root tests \
  --output artifacts/traceability/traceability-report.md
```

### Validate Linkage (`/traceability validate`)

```bash
python3 scripts/traceability/traceability.py validate \
  --story-root artifacts/stories \
  --test-root tests
```

### Show Gaps (`/traceability gaps`)

Parse the validation output and show only:
- Stories without linked tests
- Tests without story references
- Commits without story IDs

### Generate Release Notes (`/traceability release-notes`)

```bash
python3 scripts/traceability/traceability.py release-notes \
  --story-root artifacts/stories \
  --output artifacts/traceability/release-notes.md
```

## Report Format

The traceability report should include:

```markdown
# Traceability Report
Generated: <timestamp>

## Summary
| Metric | Count |
|--------|-------|
| Total Stories | X |
| Stories with Tests | X |
| Stories without Tests | X |
| Coverage Percentage | X% |

## Story Coverage Matrix

| Story ID | Title | Tests | Commits | Status |
|----------|-------|-------|---------|--------|
| ACF-001 | <title> | 5 | 3 | Complete |
| ACF-002 | <title> | 0 | 0 | No Coverage |

## Detailed Linkage

### ACF-001 - <Title>
**Status**: Complete
**Tests**:
- OrderTests.cs:CreateOrder_ValidInput_ReturnsSuccess
- OrderTests.cs:CreateOrder_InvalidInput_ThrowsValidation

**Commits**:
- abc1234 ACF-001 Implement order creation
- def5678 ACF-001 Add validation tests

---

## Gaps and Recommendations

### Stories Missing Tests
- ACF-003: <title> - Requires unit tests

### Tests Missing Story IDs
- SomeTest.cs:TestMethod - Add [Trait("Story", "ACF-###")]

### Commits Missing Story IDs
- xyz9999 "Fixed bug" - Should reference ACF-###
```

## Exit Criteria

For `/traceability validate`:
- Exit 0 if all stories have linked tests
- Exit 1 if any gaps exist

Before any release, ensure:
- 100% story coverage (all stories have tests)
- All tests reference their story IDs
- All commits include story IDs

## Example

```
User: /traceability

Claude: Generating traceability report...

## Traceability Summary

| Metric | Count |
|--------|-------|
| Total Stories | 12 |
| Stories with Tests | 10 |
| Stories without Tests | 2 |
| Coverage | 83% |

### Gaps Found

Stories missing test coverage:
- ACF-011: API rate limiting
- ACF-012: Audit logging

Recommendations:
1. Add tests for ACF-011 with trait [Trait("Story", "ACF-011")]
2. Add tests for ACF-012 with trait [Trait("Story", "ACF-012")]

Full report: artifacts/traceability/traceability-report.md
```
