import { useState, useEffect } from 'react';
import {
  View,
  Text,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
} from 'react-native';
import { Link, router } from 'expo-router';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

import { authService } from '@/services/auth/authService';
import { useTheme } from '@/theme';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { useScreenTransaction } from '@/observability/useScreenTransaction';

const forgotPasswordSchema = z.object({
  email: z.string().email('Please enter a valid email'),
});

type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>;

export default function ForgotPasswordScreen() {
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { colors, spacing, typography } = useTheme();
  const { markInteractive } = useScreenTransaction('ForgotPassword');

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordForm>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: '',
    },
  });

  // Mark interactive once component is mounted
  useEffect(() => {
    markInteractive();
  }, [markInteractive]);

  const onSubmit = async (data: ForgotPasswordForm) => {
    setIsLoading(true);
    setError(null);

    try {
      await authService.forgotPassword(data.email);
      setIsSuccess(true);
    } catch (err: any) {
      setError(err.message || 'Failed to send reset email');
    } finally {
      setIsLoading(false);
    }
  };

  if (isSuccess) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background.primary }]}>
        <View style={[styles.successContainer, { padding: spacing.lg }]}>
          <Text
            style={[
              styles.title,
              {
                color: colors.text.primary,
                fontSize: typography.fontSize['2xl'],
                fontFamily: typography.fontFamily.bold,
              },
            ]}
          >
            Check Your Email
          </Text>
          <Text
            style={[
              styles.message,
              {
                color: colors.text.secondary,
                fontSize: typography.fontSize.md,
                marginTop: spacing.md,
              },
            ]}
          >
            We've sent a password reset link to your email address.
          </Text>
          <Button
            label="Back to Login"
            onPress={() => router.replace('/login')}
            style={{ marginTop: spacing.xl }}
          />
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={[styles.container, { backgroundColor: colors.background.primary }]}>
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={styles.keyboardView}
      >
        <ScrollView
          contentContainerStyle={[styles.scrollContent, { padding: spacing.lg }]}
          keyboardShouldPersistTaps="handled"
        >
          <View style={styles.header}>
            <Text
              style={[
                styles.title,
                {
                  color: colors.text.primary,
                  fontSize: typography.fontSize['3xl'],
                  fontFamily: typography.fontFamily.bold,
                },
              ]}
            >
              Reset Password
            </Text>
            <Text
              style={[
                styles.subtitle,
                {
                  color: colors.text.secondary,
                  fontSize: typography.fontSize.md,
                  marginTop: spacing.sm,
                },
              ]}
            >
              Enter your email to receive a reset link
            </Text>
          </View>

          {error && (
            <View
              style={[
                styles.errorContainer,
                {
                  backgroundColor: colors.semantic.error + '10',
                  padding: spacing.md,
                  borderRadius: 8,
                  marginBottom: spacing.md,
                },
              ]}
            >
              <Text style={{ color: colors.semantic.error }}>{error}</Text>
            </View>
          )}

          <View style={[styles.form, { gap: spacing.md }]}>
            <Controller
              control={control}
              name="email"
              render={({ field: { onChange, onBlur, value } }) => (
                <Input
                  label="Email"
                  value={value}
                  onChangeText={onChange}
                  onBlur={onBlur}
                  error={errors.email?.message}
                  keyboardType="email-address"
                  autoCapitalize="none"
                  autoComplete="email"
                  textContentType="emailAddress"
                  placeholder="your@email.com"
                />
              )}
            />

            <Button
              label="Send Reset Link"
              onPress={handleSubmit(onSubmit)}
              loading={isLoading}
            />
          </View>

          <View style={[styles.links, { marginTop: spacing.lg }]}>
            <Link href="/login" asChild>
              <Text
                style={[
                  styles.link,
                  { color: colors.interactive.primary, fontSize: typography.fontSize.sm },
                ]}
              >
                Back to Login
              </Text>
            </Link>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  keyboardView: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    justifyContent: 'center',
  },
  successContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  header: {
    alignItems: 'center',
    marginBottom: 32,
  },
  title: {
    textAlign: 'center',
  },
  subtitle: {
    textAlign: 'center',
  },
  message: {
    textAlign: 'center',
  },
  errorContainer: {
    alignItems: 'center',
  },
  form: {
    width: '100%',
  },
  links: {
    alignItems: 'center',
  },
  link: {
    fontWeight: '600',
  },
});
