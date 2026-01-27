# Profile Feature Testing Checklist - ACF-015

## Pre-Testing Setup

1. Ensure backend is running on `http://localhost:5000`
2. Start frontend: `npm run dev`
3. Have a test user account ready
4. Have test images ready for avatar upload (< 5MB)

## Manual Testing Checklist

### 1. Navigation
- [ ] Sidebar shows "Profile" menu item with UserCircle icon
- [ ] Clicking "Profile" navigates to `/dashboard/profile`
- [ ] Profile page requires authentication (redirects if not logged in)

### 2. Profile Edit Page Load
- [ ] Page title shows "Edit Profile"
- [ ] "Back" button works correctly
- [ ] Loading skeleton shows while fetching data
- [ ] Profile data loads and populates form fields
- [ ] All sections render: Avatar, Basic Info, Niches, Collaboration

### 3. Avatar Upload
- [ ] Current avatar displays (or initials fallback)
- [ ] Drag-and-drop zone accepts image files
- [ ] "Choose File" button opens file picker
- [ ] Invalid file types show error (e.g., PDF, TXT)
- [ ] Files > 5MB show error message
- [ ] Valid image shows preview before upload
- [ ] Upload button shows loading spinner
- [ ] Success toast appears after upload
- [ ] Avatar updates in UI after upload
- [ ] "Remove" button appears when avatar exists
- [ ] Remove button clears avatar
- [ ] Error toast shows on upload failure

### 4. Display Name Field
- [ ] Current display name pre-fills
- [ ] Character counter shows correctly (e.g., "15/100")
- [ ] Can type and edit name
- [ ] Required validation shows error if empty
- [ ] Max length enforced (100 chars)
- [ ] Form detects unsaved changes

### 5. Username Field
- [ ] Current username displays
- [ ] Field is disabled (read-only)
- [ ] Helper text shows "Username cannot be changed"

### 6. Bio Field
- [ ] Current bio pre-fills
- [ ] Textarea allows multi-line input
- [ ] Character counter shows correctly (e.g., "250/500")
- [ ] Required validation shows error if empty
- [ ] Max length enforced (500 chars)
- [ ] Preserves line breaks

### 7. Niche Tags
- [ ] Selected tags display as removable badges
- [ ] Tag counter shows (e.g., "3/5")
- [ ] Can remove tags by clicking X
- [ ] Common tags display as clickable badges
- [ ] Clicking common tag adds it
- [ ] Custom tag input accepts text
- [ ] Pressing Enter or + button adds custom tag
- [ ] Cannot add more than 5 tags
- [ ] Duplicate tag shows error
- [ ] Empty tag shows error
- [ ] Tag counter updates correctly

### 8. Collaboration Toggle
- [ ] Switch reflects current status
- [ ] Clicking switch toggles on/off
- [ ] Preferences textarea shows when ON
- [ ] Preferences textarea hides when OFF
- [ ] Character counter for preferences (e.g., "150/1000")
- [ ] Preferences are optional (can be empty)
- [ ] Max length enforced (1000 chars)

### 9. Form Validation
- [ ] "Save Changes" disabled when no changes
- [ ] "Save Changes" enabled when form modified
- [ ] Required fields show error on submit if empty
- [ ] All validation errors display correctly
- [ ] Can submit valid form
- [ ] Success toast appears on save
- [ ] Form resets "unsaved changes" after save
- [ ] Error toast appears on save failure

### 10. Public Profile View
- [ ] Navigate to `/profile/testuser` (replace with actual username)
- [ ] Page shows "Creator Profile" title
- [ ] "Back" button works
- [ ] Profile card displays all info:
  - [ ] Avatar or initials
  - [ ] Display name
  - [ ] Username (@username)
  - [ ] Bio text
  - [ ] Niche tags as badges
  - [ ] "Open to Collaborations" badge (if applicable)
  - [ ] Connected platforms with status
- [ ] Invalid username shows 404 message
- [ ] "Profile not found" page has working navigation

### 11. Loading States
- [ ] Initial page load shows skeleton
- [ ] Save button shows "Saving..." with spinner
- [ ] Avatar upload shows "Uploading..." with spinner
- [ ] Buttons disabled during operations

### 12. Error States
- [ ] Network error shows error message
- [ ] Backend error shows user-friendly message
- [ ] Toast notifications appear for all errors
- [ ] Can retry after error

### 13. Responsive Design
- [ ] Mobile view (< 640px):
  - [ ] Form layouts stack vertically
  - [ ] Avatar upload is usable
  - [ ] Buttons are accessible
- [ ] Tablet view (640px - 1024px):
  - [ ] Layout adjusts appropriately
  - [ ] Sidebar behavior correct
- [ ] Desktop view (> 1024px):
  - [ ] Full layout displays correctly
  - [ ] Max-width container centers content

### 14. Accessibility
- [ ] Tab navigation works through all fields
- [ ] Can submit form with Enter key
- [ ] Error messages are announced
- [ ] Invalid fields have aria-invalid
- [ ] File input has aria-label
- [ ] Focus states visible on all interactive elements

### 15. Dark Mode
- [ ] Toggle dark mode in settings
- [ ] Profile page renders correctly in dark mode
- [ ] All text is readable
- [ ] Contrast is sufficient
- [ ] Images/avatars display correctly

### 16. Edge Cases
- [ ] Profile with no avatar (shows initials)
- [ ] Profile with 0 tags (validation error)
- [ ] Profile with max tags (5)
- [ ] Profile with very long bio (500 chars)
- [ ] Profile with special characters in name
- [ ] Profile with emojis in fields
- [ ] Collaboration OFF, then ON, then save
- [ ] Rapid clicks on save button
- [ ] Navigate away with unsaved changes (browser warning)

## API Testing

### Test Endpoints with curl or Postman

1. **Get Current User Profile**
   ```bash
   curl -H "Authorization: Bearer YOUR_TOKEN" \
     http://localhost:5000/api/profiles/me
   ```
   Expected: 200 OK with CreatorProfileResponse

2. **Get Public Profile**
   ```bash
   curl http://localhost:5000/api/profiles/testuser
   ```
   Expected: 200 OK with PublicProfileResponse

3. **Update Profile**
   ```bash
   curl -X PUT \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"displayName":"New Name","bio":"New bio","nicheTags":["Tech","Gaming"],"openToCollaborations":true}' \
     http://localhost:5000/api/profiles/me
   ```
   Expected: 200 OK with updated profile

4. **Upload Avatar**
   ```bash
   curl -X POST \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -F "avatar=@path/to/image.jpg" \
     http://localhost:5000/api/profiles/me/avatar
   ```
   Expected: 200 OK with avatar URL

## Performance Testing

- [ ] Profile page loads in < 2 seconds
- [ ] Form interactions feel responsive (< 100ms)
- [ ] Avatar preview shows immediately
- [ ] No lag when typing in textarea
- [ ] Tag selection is instant
- [ ] Page doesn't freeze during uploads

## Browser Compatibility

Test in:
- [ ] Chrome/Edge (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Mobile Safari (iOS)
- [ ] Chrome Mobile (Android)

## Security Checklist

- [ ] Cannot access edit page without authentication
- [ ] Cannot edit another user's profile
- [ ] Avatar uploads validated on client and server
- [ ] No XSS vulnerabilities in displayed text
- [ ] Authorization token sent with all requests
- [ ] Sensitive data not exposed in public profile

## Cleanup After Testing

- [ ] Delete test avatars from server
- [ ] Reset test user profile to original state
- [ ] Clear browser cache and local storage
- [ ] Check for any console errors
- [ ] Review network tab for failed requests

## Bugs Found

| # | Description | Severity | Status |
|---|-------------|----------|--------|
| 1 |             |          |        |
| 2 |             |          |        |

## Test Results Summary

- **Total Tests**: 80+
- **Passed**: ___
- **Failed**: ___
- **Blocked**: ___
- **Date Tested**: ___
- **Tester**: ___
- **Environment**: Dev/Staging/Prod

## Sign-off

- [ ] Frontend developer sign-off
- [ ] Backend developer sign-off
- [ ] QA sign-off
- [ ] Product owner sign-off
