# /mobile-deploy - Deploy Mobile App

Build and deploy React Native/Expo app using EAS Build.

## Usage
```
/mobile-deploy [profile] [options]
```

Examples:
- `/mobile-deploy` - Build for preview (default)
- `/mobile-deploy production`
- `/mobile-deploy preview --platform ios`
- `/mobile-deploy production --submit`

Profiles:
- `development` - Development client build
- `preview` - Internal testing (TestFlight/Internal)
- `production` - App Store/Play Store release

Options:
- `--platform <ios|android|all>` - Target platform (default: all)
- `--submit` - Auto-submit to stores after build
- `--local` - Build locally instead of EAS cloud
- `--message <msg>` - Build message/changelog

## Instructions

When invoked:

### 1. Validate Configuration

Check for required files:
- `app.json` or `app.config.ts`
- `eas.json`
- Environment secrets configured

### 2. Run Pre-Deploy Checks

```bash
# Type check
npm run type-check

# Lint
npm run lint

# Run tests
npm test

# Check for security issues
npm audit
```

### 3. Build Configuration

**eas.json** structure:
```json
{
  "cli": {
    "version": ">= 5.0.0"
  },
  "build": {
    "development": {
      "developmentClient": true,
      "distribution": "internal",
      "ios": {
        "simulator": true
      },
      "env": {
        "APP_ENV": "development"
      }
    },
    "preview": {
      "distribution": "internal",
      "channel": "preview",
      "ios": {
        "resourceClass": "m-medium"
      },
      "env": {
        "APP_ENV": "preview"
      }
    },
    "production": {
      "channel": "production",
      "ios": {
        "resourceClass": "m-medium"
      },
      "android": {
        "buildType": "app-bundle"
      },
      "env": {
        "APP_ENV": "production"
      }
    }
  },
  "submit": {
    "production": {
      "ios": {
        "appleId": "your@email.com",
        "ascAppId": "1234567890",
        "appleTeamId": "XXXXXXXXXX"
      },
      "android": {
        "serviceAccountKeyPath": "./google-service-account.json",
        "track": "production"
      }
    }
  }
}
```

### 4. Execute Build

**Development Build**:
```bash
# iOS Simulator
eas build --profile development --platform ios

# Android Emulator
eas build --profile development --platform android
```

**Preview Build**:
```bash
# Both platforms
eas build --profile preview --platform all

# iOS only (for TestFlight)
eas build --profile preview --platform ios
```

**Production Build**:
```bash
# Both platforms
eas build --profile production --platform all

# With auto-submit
eas build --profile production --platform all --auto-submit
```

### 5. Submit to Stores (Optional)

**iOS (App Store Connect)**:
```bash
eas submit --platform ios --profile production
```

**Android (Play Store)**:
```bash
eas submit --platform android --profile production
```

### 6. OTA Updates (Production Hot Fixes)

```bash
# Update preview channel
eas update --branch preview --message "Bug fix for login"

# Update production channel
eas update --branch production --message "v1.0.1 hotfix"
```

## Environment Configuration

### Required Secrets

Set via `eas secret:create`:
```bash
# Sentry
eas secret:create --name SENTRY_DSN --value "https://xxx@sentry.io/xxx"
eas secret:create --name SENTRY_AUTH_TOKEN --value "sntrys_xxx"

# API URLs
eas secret:create --name API_URL --value "https://api.production.com"

# Other secrets
eas secret:create --name SOME_API_KEY --value "key_xxx"
```

### app.config.ts Dynamic Config

```typescript
// app.config.ts
export default ({ config }) => ({
  ...config,
  name: process.env.APP_ENV === 'production' ? 'MyApp' : 'MyApp (Dev)',
  ios: {
    ...config.ios,
    bundleIdentifier:
      process.env.APP_ENV === 'production'
        ? 'com.mycompany.myapp'
        : 'com.mycompany.myapp.dev',
  },
  android: {
    ...config.android,
    package:
      process.env.APP_ENV === 'production'
        ? 'com.mycompany.myapp'
        : 'com.mycompany.myapp.dev',
  },
  extra: {
    sentryDsn: process.env.SENTRY_DSN,
    apiUrl: process.env.API_URL,
    eas: {
      projectId: 'your-project-id',
    },
  },
});
```

## CI/CD Integration

### GitHub Actions Workflow

```yaml
# .github/workflows/eas-build.yml
name: EAS Build

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'

      - name: Install dependencies
        run: npm ci

      - name: Lint
        run: npm run lint

      - name: Type check
        run: npm run type-check

      - name: Test
        run: npm test

      - uses: expo/expo-github-action@v8
        with:
          eas-version: latest
          token: ${{ secrets.EXPO_TOKEN }}

      - name: Build Preview
        if: github.event_name == 'pull_request'
        run: eas build --profile preview --platform all --non-interactive

      - name: Build Production
        if: github.ref == 'refs/heads/main'
        run: eas build --profile production --platform all --non-interactive --auto-submit
```

## Version Management

### Automatic Version Bumping

```bash
# Patch version (1.0.0 -> 1.0.1)
npm version patch

# Minor version (1.0.0 -> 1.1.0)
npm version minor

# Major version (1.0.0 -> 2.0.0)
npm version major
```

### Build Number Management

```typescript
// app.config.ts
const buildNumber = process.env.EAS_BUILD_NUMBER || '1';

export default {
  ios: {
    buildNumber,
  },
  android: {
    versionCode: parseInt(buildNumber, 10),
  },
};
```

## Output

```markdown
## Deployment: <profile>

### Build Status
- Profile: <profile>
- Platform: <platform>
- Build ID: <eas-build-id>

### Pre-Deploy Checks
- [x] Type check passed
- [x] Lint passed
- [x] Tests passed (42/42)
- [x] No security vulnerabilities

### Build Links
- iOS: https://expo.dev/builds/xxx
- Android: https://expo.dev/builds/yyy

### Next Steps

**For Preview builds:**
1. iOS: Install via TestFlight invite
2. Android: Download APK from build link

**For Production builds:**
1. iOS: Review in App Store Connect
2. Android: Review in Google Play Console

### OTA Update (if needed)
```bash
eas update --branch <channel> --message "Description"
```
```

## Rollback Procedures

### Revert to Previous Build

```bash
# List recent builds
eas build:list

# Resubmit previous build
eas submit --platform ios --id <previous-build-id>
```

### Revert OTA Update

```bash
# List updates
eas update:list

# Rollback to previous update
eas update:rollback --branch production
```

## Troubleshooting

### Common Issues

**Build fails with missing credentials:**
```bash
eas credentials
# Follow prompts to configure
```

**Submit fails with signing issues:**
```bash
# iOS
eas credentials --platform ios
# Select "Build Credentials" > "Set up for new build"

# Android
eas credentials --platform android
# Upload new keystore if needed
```

**OTA update not applying:**
1. Check channel matches build profile
2. Verify app is connected to EAS Update
3. Check for native code changes (requires new build)
