# /mobile-e2e - Generate E2E Tests

Generate end-to-end tests for React Native using Maestro or Detox.

## Usage
```
/mobile-e2e <flow-name> [options]
```

Examples:
- `/mobile-e2e login`
- `/mobile-e2e checkout --detox`
- `/mobile-e2e onboarding --maestro`
- `/mobile-e2e user-registration --story ACF-042`

Options:
- `--maestro` (default) - Generate Maestro YAML flow
- `--detox` - Generate Detox TypeScript test
- `--story <ACF-###>` - Link to story ID
- `--critical` - Mark as critical flow (runs on every PR)

## Instructions

When invoked:

### 1. Identify Flow Steps

Analyze the feature and identify:
- Entry point (which screen)
- User actions (taps, inputs, gestures)
- Assertions (what should be visible)
- Exit conditions (success state)

### 2. Generate Maestro Flow

```yaml
# e2e/flows/<flow-name>.yaml
appId: ${APP_ID}
---
# Flow: <Flow Name>
# Story: <ACF-###>
# Critical: true/false

- launchApp:
    clearState: true

# Step 1: Navigate to entry point
- assertVisible: "Welcome"

# Step 2: Perform user actions
- tapOn: "Log In"

- tapOn:
    id: "email-input"
- inputText: "test@example.com"

- tapOn:
    id: "password-input"
- inputText: "password123"

# Step 3: Submit action
- tapOn:
    id: "login-button"

# Step 4: Assert success state
- assertVisible:
    text: "Home"
    timeout: 10000

- assertVisible: "Welcome back"
```

### 3. Generate Detox Test (if --detox)

```typescript
// e2e/<flow-name>.test.ts
describe('<Flow Name>', () => {
  beforeAll(async () => {
    await device.launchApp();
  });

  beforeEach(async () => {
    await device.reloadReactNative();
  });

  it('completes <flow-name> successfully', async () => {
    // Step 1: Entry point
    await expect(element(by.text('Welcome'))).toBeVisible();

    // Step 2: User actions
    await element(by.id('email-input')).typeText('test@example.com');
    await element(by.id('password-input')).typeText('password123');

    // Step 3: Submit
    await element(by.id('login-button')).tap();

    // Step 4: Assert success
    await waitFor(element(by.text('Home')))
      .toBeVisible()
      .withTimeout(10000);

    await expect(element(by.text('Welcome back'))).toBeVisible();
  });

  it('shows error with invalid credentials', async () => {
    await element(by.id('email-input')).typeText('wrong@example.com');
    await element(by.id('password-input')).typeText('wrongpass');
    await element(by.id('login-button')).tap();

    await expect(element(by.text('Invalid credentials'))).toBeVisible();
  });
});
```

## Common Flow Templates

### Login Flow

```yaml
# e2e/flows/login.yaml
appId: ${APP_ID}
---
- launchApp:
    clearState: true

- assertVisible: "Welcome"
- tapOn: "Log In"

- tapOn:
    id: "email-input"
- inputText: "user@test.com"

- tapOn:
    id: "password-input"
- inputText: "TestPassword123"

- tapOn:
    id: "login-button"

- assertVisible:
    text: "Home"
    timeout: 10000
```

### Registration Flow

```yaml
# e2e/flows/registration.yaml
appId: ${APP_ID}
---
- launchApp:
    clearState: true

- tapOn: "Sign Up"

- tapOn:
    id: "name-input"
- inputText: "Test User"

- tapOn:
    id: "email-input"
- inputText: "newuser@test.com"

- tapOn:
    id: "password-input"
- inputText: "SecurePass123!"

- tapOn:
    id: "confirm-password-input"
- inputText: "SecurePass123!"

- tapOn:
    id: "register-button"

- assertVisible:
    text: "Welcome, Test User"
    timeout: 15000
```

### Search Flow

```yaml
# e2e/flows/search.yaml
appId: ${APP_ID}
---
- launchApp

- tapOn:
    id: "search-tab"

- tapOn:
    id: "search-input"
- inputText: "running shoes"

- tapOn: "Search"

- assertVisible:
    text: "results"
    timeout: 5000

- tapOn:
    index: 0

- assertVisible:
    id: "product-detail"
```

### Checkout Flow

```yaml
# e2e/flows/checkout.yaml
appId: ${APP_ID}
---
- launchApp

# Login first
- runFlow: login.yaml

# Add item to cart
- tapOn:
    id: "search-tab"
- tapOn:
    id: "search-input"
- inputText: "test product"
- tapOn: "Search"
- tapOn:
    index: 0
- tapOn:
    id: "add-to-cart"

# Go to cart
- tapOn:
    id: "cart-tab"
- assertVisible: "Your Cart"

# Checkout
- tapOn:
    id: "checkout-button"

# Payment
- tapOn:
    id: "card-number"
- inputText: "4242424242424242"

- tapOn:
    id: "expiry"
- inputText: "12/25"

- tapOn:
    id: "cvv"
- inputText: "123"

- tapOn:
    id: "pay-button"

- assertVisible:
    text: "Order Confirmed"
    timeout: 20000
```

## Running Tests

### Maestro

```bash
# Run single flow
maestro test e2e/flows/login.yaml

# Run all flows
maestro test e2e/flows/

# Run with recording
maestro test e2e/flows/login.yaml --record

# Run in CI
maestro cloud --app-file ./app.apk e2e/flows/
```

### Detox

```bash
# Build for testing
detox build --configuration ios.sim.debug

# Run tests
detox test --configuration ios.sim.debug

# Run specific test
detox test --configuration ios.sim.debug e2e/login.test.ts
```

## CI Configuration

### GitHub Actions with Maestro

```yaml
# .github/workflows/e2e.yml
name: E2E Tests

on:
  pull_request:
    branches: [main]

jobs:
  e2e:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-node@v4
        with:
          node-version: '20'

      - run: npm ci

      - name: Build app
        run: eas build --platform ios --profile preview --local

      - name: Install Maestro
        run: |
          curl -Ls "https://get.maestro.mobile.dev" | bash
          export PATH="$PATH:$HOME/.maestro/bin"

      - name: Run E2E tests
        run: maestro test e2e/flows/
```

## Output

```markdown
## E2E Test Created: <flow-name>

### File Created
- `e2e/flows/<flow-name>.yaml` (Maestro)
- OR `e2e/<flow-name>.test.ts` (Detox)

### Flow Steps
1. Launch app with clean state
2. Navigate to <entry-point>
3. <Action 1>
4. <Action 2>
5. Assert <success-state>

### Test IDs Required
Ensure these testIDs exist in your components:
- `email-input`
- `password-input`
- `login-button`

### Run Commands
```bash
# Maestro
maestro test e2e/flows/<flow-name>.yaml

# Detox
detox test e2e/<flow-name>.test.ts
```

### CI Integration
Add to `.github/workflows/e2e.yml` for automated testing.
```
