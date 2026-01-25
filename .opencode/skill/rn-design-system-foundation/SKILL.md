---
name: rn-design-system-foundation
description: React Native design system - design tokens, theming, component library foundation with minimalist modern preset
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: mobile-development
  version: "1.0.0"
  platform: react-native
external_references:
  - context7: react-native
---

# React Native Design System Foundation

## Overview

Foundation for building a consistent, scalable design system in React Native/Expo applications. Implements the Minimalist Modern preset by default with support for dark mode and design tokens.

## When to Use

- Setting up design system for new React Native projects
- Implementing consistent theming across the app
- Creating reusable UI components
- Adding dark/light mode support
- Building component libraries

---

## Design Token Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Design Token System                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐     │
│  │   Colors    │    │  Typography │    │   Spacing   │     │
│  │  (Palette)  │    │   (Fonts)   │    │   (Scale)   │     │
│  └──────┬──────┘    └──────┬──────┘    └──────┬──────┘     │
│         │                  │                  │              │
│  ┌──────┴──────┐    ┌──────┴──────┐    ┌──────┴──────┐     │
│  │   Shadows   │    │   Radius    │    │ Animations  │     │
│  │ (Elevation) │    │ (Rounding)  │    │  (Motion)   │     │
│  └─────────────┘    └─────────────┘    └─────────────┘     │
│                                                              │
│                    ┌─────────────────┐                      │
│                    │   Theme Mode    │                      │
│                    │  (Light/Dark)   │                      │
│                    └─────────────────┘                      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Directory Structure

```
theme/
├── tokens/
│   ├── colors.ts           # Color palette and semantic colors
│   ├── typography.ts       # Font families, sizes, weights
│   ├── spacing.ts          # Spacing scale (4px base)
│   ├── shadows.ts          # Elevation system
│   ├── radius.ts           # Border radius scale
│   └── animations.ts       # Animation timings and easings
├── presets/
│   ├── minimalistModern.ts # Default preset
│   ├── glassMorphism.ts    # Glass aesthetic
│   └── index.ts
├── hooks/
│   └── useTheme.ts         # Theme hook
├── context/
│   └── ThemeProvider.tsx   # Theme context provider
├── components/             # Themed component exports
│   └── index.ts
└── index.ts                # Main export
```

---

## Color System

### Base Palette

```typescript
// theme/tokens/colors.ts
export const palette = {
  // Primary (Blue)
  primary: {
    50: '#EBF5FF',
    100: '#E1EFFE',
    200: '#C3DDFD',
    300: '#A4CAFE',
    400: '#76A9FA',
    500: '#3B82F6', // Main
    600: '#2563EB',
    700: '#1D4ED8',
    800: '#1E40AF',
    900: '#1E3A8A',
  },

  // Neutral (Gray)
  neutral: {
    0: '#FFFFFF',
    50: '#FAFAFA',
    100: '#F5F5F5',
    200: '#E5E5E5',
    300: '#D4D4D4',
    400: '#A3A3A3',
    500: '#737373',
    600: '#525252',
    700: '#404040',
    800: '#262626',
    900: '#171717',
    950: '#0A0A0A',
  },

  // Semantic
  semantic: {
    success: '#10B981',
    successLight: '#D1FAE5',
    warning: '#F59E0B',
    warningLight: '#FEF3C7',
    error: '#EF4444',
    errorLight: '#FEE2E2',
    info: '#3B82F6',
    infoLight: '#DBEAFE',
  },
} as const;

// Semantic color mapping for light/dark modes
export const lightColors = {
  // Backgrounds
  background: {
    primary: palette.neutral[0],
    secondary: palette.neutral[50],
    tertiary: palette.neutral[100],
  },

  // Surfaces
  surface: {
    default: palette.neutral[0],
    raised: palette.neutral[0],
    overlay: 'rgba(0, 0, 0, 0.5)',
  },

  // Text
  text: {
    primary: palette.neutral[900],
    secondary: palette.neutral[600],
    tertiary: palette.neutral[500],
    disabled: palette.neutral[400],
    inverse: palette.neutral[0],
  },

  // Borders
  border: {
    default: palette.neutral[200],
    subtle: palette.neutral[100],
    strong: palette.neutral[300],
  },

  // Interactive
  interactive: {
    primary: palette.primary[500],
    primaryHover: palette.primary[600],
    primaryPressed: palette.primary[700],
    secondary: palette.neutral[100],
    secondaryHover: palette.neutral[200],
  },

  // Feedback
  feedback: palette.semantic,
} as const;

export const darkColors = {
  background: {
    primary: palette.neutral[950],
    secondary: palette.neutral[900],
    tertiary: palette.neutral[800],
  },

  surface: {
    default: palette.neutral[900],
    raised: palette.neutral[800],
    overlay: 'rgba(0, 0, 0, 0.7)',
  },

  text: {
    primary: palette.neutral[50],
    secondary: palette.neutral[300],
    tertiary: palette.neutral[400],
    disabled: palette.neutral[600],
    inverse: palette.neutral[900],
  },

  border: {
    default: palette.neutral[800],
    subtle: palette.neutral[700],
    strong: palette.neutral[600],
  },

  interactive: {
    primary: palette.primary[500],
    primaryHover: palette.primary[400],
    primaryPressed: palette.primary[600],
    secondary: palette.neutral[800],
    secondaryHover: palette.neutral[700],
  },

  feedback: palette.semantic,
} as const;

export type Colors = typeof lightColors;
```

---

## Typography

```typescript
// theme/tokens/typography.ts
import { Platform } from 'react-native';

export const fontFamilies = {
  // System fonts for native performance
  sans: Platform.select({
    ios: 'System',
    android: 'Roboto',
    default: 'System',
  }),
  mono: Platform.select({
    ios: 'Menlo',
    android: 'monospace',
    default: 'monospace',
  }),
} as const;

export const fontSizes = {
  xs: 12,
  sm: 14,
  base: 16,
  lg: 18,
  xl: 20,
  '2xl': 24,
  '3xl': 30,
  '4xl': 36,
  '5xl': 48,
} as const;

export const fontWeights = {
  normal: '400' as const,
  medium: '500' as const,
  semibold: '600' as const,
  bold: '700' as const,
};

export const lineHeights = {
  tight: 1.25,
  snug: 1.375,
  normal: 1.5,
  relaxed: 1.625,
  loose: 2,
} as const;

export const letterSpacings = {
  tighter: -0.5,
  tight: -0.25,
  normal: 0,
  wide: 0.25,
  wider: 0.5,
} as const;

// Predefined text styles
export const textStyles = {
  // Headings
  h1: {
    fontSize: fontSizes['4xl'],
    fontWeight: fontWeights.bold,
    lineHeight: fontSizes['4xl'] * lineHeights.tight,
    letterSpacing: letterSpacings.tight,
  },
  h2: {
    fontSize: fontSizes['3xl'],
    fontWeight: fontWeights.bold,
    lineHeight: fontSizes['3xl'] * lineHeights.tight,
    letterSpacing: letterSpacings.tight,
  },
  h3: {
    fontSize: fontSizes['2xl'],
    fontWeight: fontWeights.semibold,
    lineHeight: fontSizes['2xl'] * lineHeights.snug,
  },
  h4: {
    fontSize: fontSizes.xl,
    fontWeight: fontWeights.semibold,
    lineHeight: fontSizes.xl * lineHeights.snug,
  },

  // Body
  bodyLarge: {
    fontSize: fontSizes.lg,
    fontWeight: fontWeights.normal,
    lineHeight: fontSizes.lg * lineHeights.normal,
  },
  body: {
    fontSize: fontSizes.base,
    fontWeight: fontWeights.normal,
    lineHeight: fontSizes.base * lineHeights.normal,
  },
  bodySmall: {
    fontSize: fontSizes.sm,
    fontWeight: fontWeights.normal,
    lineHeight: fontSizes.sm * lineHeights.normal,
  },

  // Labels
  label: {
    fontSize: fontSizes.sm,
    fontWeight: fontWeights.medium,
    lineHeight: fontSizes.sm * lineHeights.tight,
  },
  labelSmall: {
    fontSize: fontSizes.xs,
    fontWeight: fontWeights.medium,
    lineHeight: fontSizes.xs * lineHeights.tight,
  },

  // Caption
  caption: {
    fontSize: fontSizes.xs,
    fontWeight: fontWeights.normal,
    lineHeight: fontSizes.xs * lineHeights.normal,
  },
} as const;
```

---

## Spacing System

```typescript
// theme/tokens/spacing.ts
// 4px base scale
export const spacing = {
  0: 0,
  px: 1,
  0.5: 2,
  1: 4,
  1.5: 6,
  2: 8,
  2.5: 10,
  3: 12,
  3.5: 14,
  4: 16,
  5: 20,
  6: 24,
  7: 28,
  8: 32,
  9: 36,
  10: 40,
  11: 44,
  12: 48,
  14: 56,
  16: 64,
  20: 80,
  24: 96,
  28: 112,
  32: 128,
} as const;

// Semantic spacing aliases
export const semanticSpacing = {
  none: spacing[0],
  xs: spacing[1],     // 4
  sm: spacing[2],     // 8
  md: spacing[4],     // 16
  lg: spacing[6],     // 24
  xl: spacing[8],     // 32
  '2xl': spacing[12], // 48
  '3xl': spacing[16], // 64
} as const;
```

---

## Shadow System

```typescript
// theme/tokens/shadows.ts
import { Platform } from 'react-native';

// Minimalist Modern: Subtle shadows (0-10% opacity)
export const shadows = {
  none: {
    shadowColor: 'transparent',
    shadowOffset: { width: 0, height: 0 },
    shadowOpacity: 0,
    shadowRadius: 0,
    elevation: 0,
  },

  sm: Platform.select({
    ios: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 1 },
      shadowOpacity: 0.05,
      shadowRadius: 2,
    },
    android: {
      elevation: 1,
    },
    default: {},
  }),

  md: Platform.select({
    ios: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 2 },
      shadowOpacity: 0.08,
      shadowRadius: 4,
    },
    android: {
      elevation: 3,
    },
    default: {},
  }),

  lg: Platform.select({
    ios: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.1,
      shadowRadius: 8,
    },
    android: {
      elevation: 6,
    },
    default: {},
  }),

  xl: Platform.select({
    ios: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 8 },
      shadowOpacity: 0.1,
      shadowRadius: 16,
    },
    android: {
      elevation: 12,
    },
    default: {},
  }),
} as const;
```

---

## Border Radius

```typescript
// theme/tokens/radius.ts
export const radius = {
  none: 0,
  sm: 4,
  md: 8,
  lg: 12,
  xl: 16,
  '2xl': 24,
  full: 9999,
} as const;
```

---

## Theme Provider

```typescript
// theme/context/ThemeProvider.tsx
import React, { createContext, useContext, useMemo } from 'react';
import { useColorScheme as useRNColorScheme } from 'react-native';

import { lightColors, darkColors, Colors } from '../tokens/colors';
import { spacing, semanticSpacing } from '../tokens/spacing';
import { textStyles, fontFamilies } from '../tokens/typography';
import { shadows } from '../tokens/shadows';
import { radius } from '../tokens/radius';

interface Theme {
  colors: Colors;
  spacing: typeof spacing;
  semanticSpacing: typeof semanticSpacing;
  textStyles: typeof textStyles;
  fontFamilies: typeof fontFamilies;
  shadows: typeof shadows;
  radius: typeof radius;
  isDark: boolean;
}

const ThemeContext = createContext<Theme | undefined>(undefined);

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const colorScheme = useRNColorScheme();
  const isDark = colorScheme === 'dark';

  const theme = useMemo<Theme>(() => ({
    colors: isDark ? darkColors : lightColors,
    spacing,
    semanticSpacing,
    textStyles,
    fontFamilies,
    shadows,
    radius,
    isDark,
  }), [isDark]);

  return (
    <ThemeContext.Provider value={theme}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme(): Theme {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider');
  }
  return context;
}
```

---

## Themed Components

### Button Component

```typescript
// components/ui/Button.tsx
import React from 'react';
import {
  Pressable,
  Text,
  ActivityIndicator,
  StyleSheet,
  ViewStyle,
  TextStyle,
} from 'react-native';
import { useTheme } from '@/theme';

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';
type ButtonSize = 'sm' | 'md' | 'lg';

interface ButtonProps {
  children: React.ReactNode;
  variant?: ButtonVariant;
  size?: ButtonSize;
  disabled?: boolean;
  loading?: boolean;
  onPress?: () => void;
  style?: ViewStyle;
  testID?: string;
}

export function Button({
  children,
  variant = 'primary',
  size = 'md',
  disabled = false,
  loading = false,
  onPress,
  style,
  testID = 'button',
}: ButtonProps) {
  const theme = useTheme();

  const containerStyles = getContainerStyles(theme, variant, size, disabled);
  const textStyles = getTextStyles(theme, variant, size);

  return (
    <Pressable
      testID={testID}
      onPress={onPress}
      disabled={disabled || loading}
      style={({ pressed }) => [
        containerStyles.base,
        pressed && containerStyles.pressed,
        disabled && containerStyles.disabled,
        style,
      ]}
    >
      {loading ? (
        <ActivityIndicator
          testID="loading-indicator"
          color={textStyles.color}
          size="small"
        />
      ) : (
        <Text style={textStyles}>{children}</Text>
      )}
    </Pressable>
  );
}

function getContainerStyles(
  theme: ReturnType<typeof useTheme>,
  variant: ButtonVariant,
  size: ButtonSize,
  disabled: boolean
) {
  const { colors, radius, semanticSpacing } = theme;

  const sizeStyles = {
    sm: { paddingVertical: semanticSpacing.xs, paddingHorizontal: semanticSpacing.sm },
    md: { paddingVertical: semanticSpacing.sm, paddingHorizontal: semanticSpacing.md },
    lg: { paddingVertical: semanticSpacing.md, paddingHorizontal: semanticSpacing.lg },
  };

  const variantStyles = {
    primary: {
      base: { backgroundColor: colors.interactive.primary },
      pressed: { backgroundColor: colors.interactive.primaryPressed },
    },
    secondary: {
      base: { backgroundColor: colors.interactive.secondary, borderWidth: 1, borderColor: colors.border.default },
      pressed: { backgroundColor: colors.interactive.secondaryHover },
    },
    ghost: {
      base: { backgroundColor: 'transparent' },
      pressed: { backgroundColor: colors.interactive.secondary },
    },
    danger: {
      base: { backgroundColor: colors.feedback.error },
      pressed: { backgroundColor: '#DC2626' },
    },
  };

  return {
    base: {
      ...sizeStyles[size],
      ...variantStyles[variant].base,
      borderRadius: radius.md,
      alignItems: 'center' as const,
      justifyContent: 'center' as const,
    },
    pressed: variantStyles[variant].pressed,
    disabled: { opacity: 0.5 },
  };
}

function getTextStyles(
  theme: ReturnType<typeof useTheme>,
  variant: ButtonVariant,
  size: ButtonSize
): TextStyle {
  const { colors, textStyles } = theme;

  const sizeStyles = {
    sm: textStyles.labelSmall,
    md: textStyles.label,
    lg: textStyles.bodyLarge,
  };

  const colorMap = {
    primary: colors.text.inverse,
    secondary: colors.text.primary,
    ghost: colors.interactive.primary,
    danger: colors.text.inverse,
  };

  return {
    ...sizeStyles[size],
    color: colorMap[variant],
    fontWeight: '600',
  };
}
```

---

## Usage Example

```typescript
// screens/HomeScreen.tsx
import { View, Text, StyleSheet } from 'react-native';
import { useTheme } from '@/theme';
import { Button, Card, Input } from '@/components/ui';

export function HomeScreen() {
  const theme = useTheme();

  return (
    <View style={[styles.container, { backgroundColor: theme.colors.background.primary }]}>
      <Text style={[theme.textStyles.h2, { color: theme.colors.text.primary }]}>
        Welcome
      </Text>

      <Card>
        <Text style={[theme.textStyles.body, { color: theme.colors.text.secondary }]}>
          This is a themed card component
        </Text>
      </Card>

      <View style={{ gap: theme.semanticSpacing.sm }}>
        <Button variant="primary">Primary Action</Button>
        <Button variant="secondary">Secondary</Button>
        <Button variant="ghost">Ghost</Button>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 16,
  },
});
```

---

## Context7 Integration

Query for design system patterns:

```
1. resolve-library-id: "react-native"
2. Query: "React Native StyleSheet theming"
3. Query: "React Native dark mode implementation"
```

---

## Related Skills

- `rn-design-preset-system` - Switch between design presets
- `rn-fundamentals` - Component patterns
- `rn-animations` - Animation system
