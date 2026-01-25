---
name: rn-deployment
description: "React Native deployment - EAS Build, App Store/Play Store submission, OTA updates, CI/CD pipelines for Expo apps"
---

# React Native Deployment

## Overview

Deployment patterns for React Native/Expo applications including EAS Build, app store submission, OTA updates, and CI/CD pipeline configuration.

## When to Use

- Building production apps with EAS
- Submitting to App Store and Play Store
- Setting up OTA updates
- Configuring CI/CD pipelines
- Managing multiple environments

---

## EAS Build Configuration

### eas.json

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
      }
    },
    "preview": {
      "distribution": "internal",
      "channel": "preview"
    },
    "production": {
      "channel": "production",
      "ios": {
        "resourceClass": "m-medium"
      },
      "android": {
        "buildType": "apk"
      }
    }
  },
  "submit": {
    "production": {
      "ios": {
        "appleId": "your@email.com",
        "ascAppId": "1234567890"
      },
      "android": {
        "serviceAccountKeyPath": "./google-service-account.json",
        "track": "production"
      }
    }
  }
}
```

---

## Build Commands

```bash
# Development build (internal testing)
eas build --profile development --platform all

# Preview build (TestFlight/Internal)
eas build --profile preview --platform all

# Production build
eas build --profile production --platform all

# Submit to stores
eas submit --platform ios --profile production
eas submit --platform android --profile production
```

---

## Environment Secrets

```bash
# Set EAS secrets
eas secret:create --name SENTRY_AUTH_TOKEN --value <token>
eas secret:create --name API_URL --value https://api.production.com

# List secrets
eas secret:list
```

---

## GitHub Actions CI/CD

```yaml
# .github/workflows/build.yml
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

      - run: npm ci
      - run: npm run lint
      - run: npm run test

      - uses: expo/expo-github-action@v8
        with:
          eas-version: latest
          token: ${{ secrets.EXPO_TOKEN }}

      - run: eas build --platform all --non-interactive --profile preview
```

---

## OTA Updates

```bash
# Publish update to preview channel
eas update --branch preview --message "Bug fixes"

# Publish to production
eas update --branch production --message "v1.0.1 release"
```

---

## Related Skills

- `rn-observability-setup` - Source maps for Sentry
- `rn-testing` - Pre-deployment testing
