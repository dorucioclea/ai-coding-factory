# Quality Checklist: {FEATURE_NAME}

**Feature**: `{STORY_ID}-{feature-slug}`
**Date**: {DATE}
**Reviewer**: {Author}

---

## Pre-Implementation Checklist

### Requirements Ready

- [ ] User stories have acceptance criteria
- [ ] Success metrics defined
- [ ] Out of scope documented
- [ ] Dependencies identified
- [ ] Questions resolved (no [NEEDS CLARIFICATION] remaining)

### Technical Design Ready

- [ ] Data model reviewed
- [ ] API contracts defined
- [ ] Component structure planned
- [ ] Error handling approach decided
- [ ] Testing strategy documented

---

## Implementation Checklist

### Code Quality

- [ ] Follows project coding standards
- [ ] No compiler warnings
- [ ] No ESLint/Prettier errors
- [ ] No TODO comments (or tracked in backlog)
- [ ] Self-documenting (minimal comments needed)
- [ ] Functions < 50 lines
- [ ] Files < 400 lines (ideally)

### Domain Layer (Backend)

- [ ] Entities have proper validation
- [ ] Value objects are immutable
- [ ] Repository interfaces defined
- [ ] No infrastructure dependencies
- [ ] Domain events for side effects

### Application Layer (Backend)

- [ ] Commands/queries follow CQRS
- [ ] Validators implemented
- [ ] DTOs are flat (no nested entities)
- [ ] No business logic in handlers
- [ ] Proper error handling

### API Layer (Backend)

- [ ] Endpoints follow REST conventions
- [ ] Authorization attributes applied
- [ ] Input validation at boundary
- [ ] Consistent response format
- [ ] Swagger documentation generated

### Frontend Components

- [ ] TypeScript types match API
- [ ] Props are typed
- [ ] Loading states handled
- [ ] Error states handled
- [ ] Empty states handled
- [ ] Accessible (ARIA labels, keyboard nav)

### State Management

- [ ] Server state in TanStack Query
- [ ] Client state in Zustand (if needed)
- [ ] No prop drilling (>3 levels)
- [ ] Optimistic updates where appropriate

---

## Testing Checklist

### Unit Tests

- [ ] Domain logic covered
- [ ] Edge cases tested
- [ ] Error paths tested
- [ ] Mocks used appropriately
- [ ] Coverage > 80% for new code

### Integration Tests

- [ ] API endpoints tested
- [ ] Database operations verified
- [ ] Auth flows tested
- [ ] Error responses correct

### Frontend Tests

- [ ] Critical components tested
- [ ] Form validation tested
- [ ] Hooks tested
- [ ] Accessibility tested

### E2E Tests (if applicable)

- [ ] Happy path covered
- [ ] Critical error paths covered
- [ ] Cross-browser tested

---

## Security Checklist

### Authentication

- [ ] Auth required where needed
- [ ] Tokens validated properly
- [ ] Session management correct

### Authorization

- [ ] Resource ownership checked
- [ ] Role-based access enforced
- [ ] No privilege escalation possible

### Data Protection

- [ ] No sensitive data in logs
- [ ] No sensitive data in URLs
- [ ] Input sanitized
- [ ] SQL injection prevented (EF Core)
- [ ] XSS prevented (React escaping)

### API Security

- [ ] Rate limiting in place
- [ ] CORS configured correctly
- [ ] No mass assignment vulnerabilities
- [ ] Pagination enforced on lists

---

## Performance Checklist

### Backend

- [ ] N+1 queries avoided
- [ ] Proper indexes on queries
- [ ] Async for I/O operations
- [ ] Caching where appropriate

### Frontend

- [ ] No unnecessary re-renders
- [ ] Images optimized
- [ ] Bundle size reasonable
- [ ] Lazy loading where appropriate

---

## Documentation Checklist

- [ ] README updated (if needed)
- [ ] API documentation current
- [ ] Complex logic commented
- [ ] Architecture decisions recorded
- [ ] Changelog updated

---

## Pre-Merge Checklist

- [ ] All tests passing
- [ ] No merge conflicts
- [ ] Code reviewed
- [ ] Security review (if applicable)
- [ ] Documentation updated
- [ ] Demo/verification completed

---

## Post-Merge Checklist

- [ ] Deployment successful
- [ ] Smoke tests passing
- [ ] Monitoring/alerts configured
- [ ] Feature flags set (if applicable)
- [ ] Stakeholders notified

---

## Sign-off

| Role | Name | Date | Approved |
|------|------|------|----------|
| Developer | | | ☐ |
| Reviewer | | | ☐ |
| QA (if applicable) | | | ☐ |

---

## Notes

{Any additional notes, known issues, or follow-up items}
