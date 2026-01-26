# Gate 4: Release → Maintenance

## Purpose
Validates that all release criteria are met before production deployment.

## Required Artifacts

| Artifact | Location | Description |
|----------|----------|-------------|
| Release Notes | `artifacts/traceability/release-notes.md` | What's in this release |
| Security Checklist | `.opencode/templates/artifacts/security-review-checklist.md` | Security sign-off |
| Release Checklist | `.opencode/templates/artifacts/release-readiness-checklist.md` | Release sign-off |

## Checklist

### Release Notes (RN)
- [ ] RN-1: Version number assigned
- [ ] RN-2: All included stories listed
- [ ] RN-3: Breaking changes documented
- [ ] RN-4: Migration instructions provided (if needed)
- [ ] RN-5: Known issues documented

### Security Sign-off (SEC)
- [ ] SEC-1: Security review checklist completed
- [ ] SEC-2: All security items addressed or documented
- [ ] SEC-3: No open critical/high vulnerabilities
- [ ] SEC-4: Secrets management verified
- [ ] SEC-5: Security team approval (if required)

### Release Readiness (READY)
- [ ] READY-1: All validation gates passed
- [ ] READY-2: Staging deployment successful
- [ ] READY-3: Smoke tests pass on staging
- [ ] READY-4: Rollback plan documented
- [ ] READY-5: Monitoring/alerting configured
- [ ] READY-6: On-call schedule confirmed

### Approval (APPR)
- [ ] APPR-1: Product Owner sign-off
- [ ] APPR-2: Technical Lead sign-off
- [ ] APPR-3: DevOps sign-off

### Deployment (DEPLOY)
- [ ] DEPLOY-1: Deployment runbook available
- [ ] DEPLOY-2: Database migration plan (if needed)
- [ ] DEPLOY-3: Feature flags configured (if needed)
- [ ] DEPLOY-4: Communication plan ready

## Validation Commands

```bash
# Verify all checklists exist and are complete
ls artifacts/traceability/release-notes.md
grep -c "\[x\]" .opencode/templates/artifacts/security-review-checklist.md
grep -c "\[x\]" .opencode/templates/artifacts/release-readiness-checklist.md

# Verify staging deployment
curl -f https://staging.example.com/health

# Run smoke tests
./scripts/smoke-tests.sh staging
```

## Exit Criteria

All items checked = **GATE PASSED** → Deploy to production
Any item unchecked = **GATE BLOCKED** → Cannot deploy

## Emergency Release Process

For critical hotfixes:
1. Document the emergency in `artifacts/incidents/`
2. Get verbal approval from stakeholders
3. Perform expedited testing
4. Deploy with monitoring
5. Complete full documentation post-release

## Post-Release

After successful release:
1. Tag the release in git
2. Archive release artifacts
3. Update project documentation
4. Send release communication
5. Transition to **Maintenance** phase

## Next Phase
Upon successful deployment, enter **Maintenance** phase.
