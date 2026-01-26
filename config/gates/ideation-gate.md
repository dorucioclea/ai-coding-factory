# Gate 1: Ideation → Development

## Purpose
Validates that ideation phase is complete before allowing implementation work to begin.

## Required Artifacts

| Artifact | Location | Description |
|----------|----------|-------------|
| Requirements Spec | `artifacts/requirements.md` | Documented requirements |
| Epics | `artifacts/epics.md` | High-level feature groupings |
| User Stories | `artifacts/stories/` | At least one ACF-### story file |
| Architecture (Initial) | `docs/architecture/architecture.md` | Initial architecture design |
| Risk Log | `artifacts/risk-log.md` | Identified risks and mitigations |

## Checklist

### Requirements (REQ)
- [ ] REQ-1: All functional requirements documented
- [ ] REQ-2: All user stories have acceptance criteria
- [ ] REQ-3: Non-functional requirements quantified with metrics
- [ ] REQ-4: User journeys documented
- [ ] REQ-5: External API/integration requirements listed
- [ ] REQ-6: GDPR/privacy requirements identified (if applicable)

### Architecture (ARCH)
- [ ] ARCH-1: High-level system design documented
- [ ] ARCH-2: Technology stack defined
- [ ] ARCH-3: Layer boundaries established (Clean Architecture)
- [ ] ARCH-4: Database schema outlined
- [ ] ARCH-5: Security architecture considerations documented

### Risk (RISK)
- [ ] RISK-1: Technical risks identified
- [ ] RISK-2: Security risks identified
- [ ] RISK-3: Mitigation strategies documented
- [ ] RISK-4: Risk owners assigned

### Approval (APPR)
- [ ] APPR-1: Product Owner approval obtained
- [ ] APPR-2: Scrum Master confirms DoR met

## Validation Commands

```bash
# Check required artifacts exist
./scripts/validate-ideation-gate.sh

# Or manually verify:
test -f artifacts/requirements.md && echo "✓ requirements.md"
test -f artifacts/epics.md && echo "✓ epics.md"
test -d artifacts/stories && ls artifacts/stories/*.md | head -1 && echo "✓ stories exist"
test -f docs/architecture/architecture.md && echo "✓ architecture.md"
test -f artifacts/risk-log.md && echo "✓ risk-log.md"
```

## Exit Criteria

All items checked = **GATE PASSED**
Any item unchecked = **GATE BLOCKED** (document reason and remediation plan)

## Next Phase
Upon passing, proceed to **Development** phase.
