---
name: rn-animations
description: React Native animations - Reanimated 3, gesture handling, micro-interactions, layout animations for mobile UX
license: MIT
compatibility: opencode
metadata:
  audience: developers
  workflow: mobile-development
  version: "1.0.0"
  platform: react-native
external_references:
  - context7: react-native-reanimated
  - context7: react-native-gesture-handler
---

# React Native Animations

## Overview

Animation patterns for React Native using Reanimated 3 and Gesture Handler for smooth, performant animations and interactions.

## When to Use

- Implementing UI animations and transitions
- Adding gesture-driven interactions
- Creating micro-interactions for UX polish
- Building custom animated components
- Screen transitions and layout animations

---

## Reanimated 3 Basics

### Shared Values & Animated Styles

```typescript
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  withTiming,
} from 'react-native-reanimated';

function AnimatedCard() {
  const scale = useSharedValue(1);
  const opacity = useSharedValue(1);

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [{ scale: scale.value }],
    opacity: opacity.value,
  }));

  const handlePressIn = () => {
    scale.value = withSpring(0.95);
    opacity.value = withTiming(0.8, { duration: 100 });
  };

  const handlePressOut = () => {
    scale.value = withSpring(1);
    opacity.value = withTiming(1, { duration: 100 });
  };

  return (
    <Pressable onPressIn={handlePressIn} onPressOut={handlePressOut}>
      <Animated.View style={[styles.card, animatedStyle]}>
        <Text>Animated Card</Text>
      </Animated.View>
    </Pressable>
  );
}
```

### Gesture Handler Integration

```typescript
import { Gesture, GestureDetector } from 'react-native-gesture-handler';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  runOnJS,
} from 'react-native-reanimated';

function SwipeableCard({ onSwipeLeft, onSwipeRight }) {
  const translateX = useSharedValue(0);
  const SWIPE_THRESHOLD = 100;

  const gesture = Gesture.Pan()
    .onUpdate((event) => {
      translateX.value = event.translationX;
    })
    .onEnd((event) => {
      if (event.translationX > SWIPE_THRESHOLD) {
        runOnJS(onSwipeRight)();
      } else if (event.translationX < -SWIPE_THRESHOLD) {
        runOnJS(onSwipeLeft)();
      }
      translateX.value = withSpring(0);
    });

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [{ translateX: translateX.value }],
  }));

  return (
    <GestureDetector gesture={gesture}>
      <Animated.View style={[styles.card, animatedStyle]}>
        <Text>Swipe me</Text>
      </Animated.View>
    </GestureDetector>
  );
}
```

---

## Layout Animations

```typescript
import Animated, { Layout, FadeIn, FadeOut } from 'react-native-reanimated';

function AnimatedList({ items }) {
  return (
    <View>
      {items.map((item) => (
        <Animated.View
          key={item.id}
          entering={FadeIn.duration(300)}
          exiting={FadeOut.duration(200)}
          layout={Layout.springify()}
        >
          <ListItem item={item} />
        </Animated.View>
      ))}
    </View>
  );
}
```

---

## Animation Hooks

```typescript
// hooks/useAnimatedPress.ts
import { useSharedValue, useAnimatedStyle, withSpring } from 'react-native-reanimated';

export function useAnimatedPress(config = { scale: 0.95 }) {
  const scale = useSharedValue(1);

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [{ scale: scale.value }],
  }));

  const handlers = {
    onPressIn: () => {
      scale.value = withSpring(config.scale);
    },
    onPressOut: () => {
      scale.value = withSpring(1);
    },
  };

  return { animatedStyle, handlers };
}
```

---

## Context7 Integration

When uncertain about Reanimated or gesture APIs, query Context7:

```
1. Use resolve-library-id to find: "react-native-reanimated", "react-native-gesture-handler"
2. Query specific topics:
   - "Reanimated shared value animations"
   - "React Native Gesture Handler pan gesture"
   - "Reanimated layout animations"
```

---

## Related Skills

- `rn-fundamentals` - Component patterns
- `rn-design-system-foundation` - Animation tokens
- `rn-navigation` - Screen transitions
