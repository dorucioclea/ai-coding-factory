import {
  Pressable,
  Text,
  ActivityIndicator,
  StyleSheet,
  ViewStyle,
  TextStyle,
} from 'react-native';
import { useTheme } from '@/theme';

interface ButtonProps {
  label: string;
  onPress: () => void;
  variant?: 'primary' | 'secondary' | 'outline';
  size?: 'sm' | 'md' | 'lg';
  disabled?: boolean;
  loading?: boolean;
  style?: ViewStyle;
  testID?: string;
}

export function Button({
  label,
  onPress,
  variant = 'primary',
  size = 'md',
  disabled = false,
  loading = false,
  style,
  testID,
}: ButtonProps) {
  const { colors, spacing, typography, radius } = useTheme();

  const isDisabled = disabled || loading;

  // Size configurations
  const sizeConfig = {
    sm: {
      paddingVertical: spacing.sm,
      paddingHorizontal: spacing.md,
      fontSize: typography.fontSize.sm,
    },
    md: {
      paddingVertical: spacing.md - 4,
      paddingHorizontal: spacing.lg,
      fontSize: typography.fontSize.md,
    },
    lg: {
      paddingVertical: spacing.md,
      paddingHorizontal: spacing.xl,
      fontSize: typography.fontSize.lg,
    },
  };

  // Variant styles
  const getVariantStyles = (): { container: ViewStyle; text: TextStyle } => {
    if (isDisabled) {
      return {
        container: {
          backgroundColor: colors.interactive.disabled,
        },
        text: {
          color: colors.text.tertiary,
        },
      };
    }

    switch (variant) {
      case 'primary':
        return {
          container: {
            backgroundColor: colors.interactive.primary,
          },
          text: {
            color: colors.text.inverse,
          },
        };
      case 'secondary':
        return {
          container: {
            backgroundColor: colors.interactive.secondary,
          },
          text: {
            color: colors.text.primary,
          },
        };
      case 'outline':
        return {
          container: {
            backgroundColor: 'transparent',
            borderWidth: 1,
            borderColor: colors.border.default,
          },
          text: {
            color: colors.text.primary,
          },
        };
    }
  };

  const variantStyles = getVariantStyles();
  const currentSize = sizeConfig[size];

  return (
    <Pressable
      onPress={onPress}
      disabled={isDisabled}
      testID={testID}
      accessibilityRole="button"
      accessibilityLabel={label}
      accessibilityState={{ disabled: isDisabled }}
      style={({ pressed }) => [
        styles.base,
        {
          paddingVertical: currentSize.paddingVertical,
          paddingHorizontal: currentSize.paddingHorizontal,
          borderRadius: radius.md,
          opacity: pressed && !isDisabled ? 0.8 : 1,
        },
        variantStyles.container,
        style,
      ]}
    >
      {loading ? (
        <ActivityIndicator
          size="small"
          color={variant === 'primary' ? colors.text.inverse : colors.text.primary}
        />
      ) : (
        <Text
          style={[
            styles.label,
            {
              fontSize: currentSize.fontSize,
              fontFamily: typography.fontFamily.semibold,
            },
            variantStyles.text,
          ]}
        >
          {label}
        </Text>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  base: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: 44, // Accessibility minimum
  },
  label: {
    textAlign: 'center',
    fontWeight: '600',
  },
});
