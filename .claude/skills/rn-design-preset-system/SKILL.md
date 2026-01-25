---
name: rn-design-preset-system
description: "React Native design presets - switch between minimalist modern, glass aesthetic, and other curated design styles"
---

# React Native Design Preset System

## Overview

Design preset system for React Native enabling seamless style switching between curated design approaches while maintaining consistency.

## Available Presets

### 1. Minimalist Modern (Default)
- Clean, spacious layouts
- Monochromatic + single accent
- Subtle shadows (0-10% opacity)
- Best for: SaaS, productivity apps

### 2. Glass Aesthetic
- Transparency with backdrop blur
- Layered depth
- Semi-transparent surfaces
- Best for: Premium apps, fintech

### 3. Timeless Classic
- High accessibility (WCAG AAA)
- Conservative colors
- Proven UI patterns
- Best for: Enterprise, government

### 4. Bold Brutalist
- High contrast (black/white/accent)
- Sharp corners
- Bold typography
- Best for: Creative, portfolios

---

## Applying Presets

```typescript
// theme/presets/minimalistModern.ts
export const minimalistModern = {
  name: 'minimalist-modern',
  colors: {
    primary: '#3B82F6',
    background: '#FFFFFF',
    surface: '#FFFFFF',
    text: '#171717',
    border: '#E5E5E5',
  },
  spacing: { xs: 4, sm: 8, md: 16, lg: 24, xl: 32 },
  radius: { sm: 4, md: 8, lg: 12 },
  shadows: {
    sm: { shadowOpacity: 0.05, elevation: 1 },
    md: { shadowOpacity: 0.08, elevation: 3 },
  },
};

// theme/presets/glassAesthetic.ts
export const glassAesthetic = {
  name: 'glass-aesthetic',
  colors: {
    primary: '#6366F1',
    background: 'rgba(255, 255, 255, 0.1)',
    surface: 'rgba(255, 255, 255, 0.2)',
    text: '#FFFFFF',
    border: 'rgba(255, 255, 255, 0.3)',
  },
  blur: { light: 10, medium: 20, heavy: 40 },
  // ... other tokens
};
```

---

## Related Skills

- `rn-design-system-foundation` - Token architecture
- `rn-fundamentals` - Component patterns
