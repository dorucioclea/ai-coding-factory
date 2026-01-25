import { useEffect } from 'react';
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

import { useAuth } from '@/hooks/useAuth';
import { useTheme } from '@/theme';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { useScreenTransaction } from '@/observability/useScreenTransaction';

const loginSchema = z.object({
  email: z.string().email('Please enter a valid email'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginScreen() {
  const { login, isLoading, error, clearError, isAuthenticated } = useAuth();
  const { colors, spacing, typography } = useTheme();
  const { markInteractive } = useScreenTransaction('Login');

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  // Mark interactive once component is mounted
  useEffect(() => {
    markInteractive();
  }, [markInteractive]);

  // Redirect on successful auth
  useEffect(() => {
    if (isAuthenticated) {
      router.replace('/(app)/(tabs)');
    }
  }, [isAuthenticated]);

  const onSubmit = async (data: LoginForm) => {
    clearError();
    await login(data);
  };

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
              Welcome Back
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
              Sign in to continue
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
                  testID="email-input"
                />
              )}
            />

            <Controller
              control={control}
              name="password"
              render={({ field: { onChange, onBlur, value } }) => (
                <Input
                  label="Password"
                  value={value}
                  onChangeText={onChange}
                  onBlur={onBlur}
                  error={errors.password?.message}
                  secureTextEntry
                  autoComplete="password"
                  textContentType="password"
                  placeholder="Enter your password"
                  testID="password-input"
                />
              )}
            />

            <Button
              label="Sign In"
              onPress={handleSubmit(onSubmit)}
              loading={isLoading}
              testID="login-button"
            />
          </View>

          <View style={[styles.links, { marginTop: spacing.lg }]}>
            <Link href="/forgot-password" asChild>
              <Text
                style={[
                  styles.link,
                  { color: colors.interactive.primary, fontSize: typography.fontSize.sm },
                ]}
              >
                Forgot Password?
              </Text>
            </Link>

            <View style={[styles.registerLink, { marginTop: spacing.md }]}>
              <Text style={{ color: colors.text.secondary, fontSize: typography.fontSize.sm }}>
                Don't have an account?{' '}
              </Text>
              <Link href="/register" asChild>
                <Text
                  style={[
                    styles.link,
                    { color: colors.interactive.primary, fontSize: typography.fontSize.sm },
                  ]}
                >
                  Sign Up
                </Text>
              </Link>
            </View>
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
  registerLink: {
    flexDirection: 'row',
    alignItems: 'center',
  },
});
