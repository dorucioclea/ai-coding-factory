import { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TextInputProps,
  StyleSheet,
  ViewStyle,
} from 'react-native';
import { useTheme } from '@/theme';

interface InputProps extends Omit<TextInputProps, 'style'> {
  label?: string;
  error?: string;
  containerStyle?: ViewStyle;
}

export function Input({
  label,
  error,
  containerStyle,
  ...textInputProps
}: InputProps) {
  const { colors, spacing, typography, radius } = useTheme();
  const [isFocused, setIsFocused] = useState(false);

  const borderColor = error
    ? colors.semantic.error
    : isFocused
      ? colors.border.focus
      : colors.border.default;

  return (
    <View style={[styles.container, containerStyle]}>
      {label && (
        <Text
          style={[
            styles.label,
            {
              color: colors.text.primary,
              fontSize: typography.fontSize.sm,
              fontFamily: typography.fontFamily.medium,
              marginBottom: spacing.xs,
            },
          ]}
        >
          {label}
        </Text>
      )}
      <TextInput
        style={[
          styles.input,
          {
            backgroundColor: colors.background.secondary,
            color: colors.text.primary,
            fontSize: typography.fontSize.md,
            borderRadius: radius.md,
            borderWidth: 1,
            borderColor,
            paddingHorizontal: spacing.md,
            paddingVertical: spacing.md - 4,
          },
        ]}
        placeholderTextColor={colors.text.tertiary}
        onFocus={() => setIsFocused(true)}
        onBlur={(e) => {
          setIsFocused(false);
          textInputProps.onBlur?.(e);
        }}
        {...textInputProps}
      />
      {error && (
        <Text
          style={[
            styles.error,
            {
              color: colors.semantic.error,
              fontSize: typography.fontSize.sm,
              marginTop: spacing.xs,
            },
          ]}
          accessibilityRole="alert"
        >
          {error}
        </Text>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    width: '100%',
  },
  label: {
    fontWeight: '500',
  },
  input: {
    minHeight: 44, // Accessibility minimum
  },
  error: {},
});
