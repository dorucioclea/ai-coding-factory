import { useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { router } from 'expo-router';

import { useAuth } from '@/hooks/useAuth';
import { useTheme } from '@/theme';
import { useScreenTransaction } from '@/observability/useScreenTransaction';
import { Button } from '@/components/ui/Button';
import { Card } from '@/components/ui/Card';

export default function ProfileScreen() {
  const { user, logout } = useAuth();
  const { colors, spacing, typography, radius } = useTheme();
  const { markInteractive } = useScreenTransaction('Profile');

  // Mark interactive once component is mounted
  useEffect(() => {
    markInteractive();
  }, [markInteractive]);

  const handleLogout = async () => {
    await logout();
    router.replace('/(auth)/login');
  };

  return (
    <SafeAreaView
      style={[styles.container, { backgroundColor: colors.background.primary }]}
      edges={['top']}
    >
      <ScrollView contentContainerStyle={[styles.content, { padding: spacing.lg }]}>
        <View style={styles.header}>
          <View
            style={[
              styles.avatar,
              {
                backgroundColor: colors.interactive.primary,
                borderRadius: radius.full,
                width: 80,
                height: 80,
                justifyContent: 'center',
                alignItems: 'center',
              },
            ]}
          >
            <Text
              style={{
                color: colors.text.inverse,
                fontSize: typography.fontSize['3xl'],
                fontFamily: typography.fontFamily.bold,
              }}
            >
              {user?.name?.charAt(0)?.toUpperCase() ?? 'U'}
            </Text>
          </View>
          <Text
            style={[
              styles.name,
              {
                color: colors.text.primary,
                fontSize: typography.fontSize.xl,
                fontFamily: typography.fontFamily.bold,
                marginTop: spacing.md,
              },
            ]}
          >
            {user?.name ?? 'User'}
          </Text>
          <Text
            style={[
              styles.email,
              {
                color: colors.text.secondary,
                fontSize: typography.fontSize.md,
                marginTop: spacing.xs,
              },
            ]}
          >
            {user?.email ?? 'user@example.com'}
          </Text>
        </View>

        <View style={[styles.section, { marginTop: spacing.xl }]}>
          <Card
            style={{ marginBottom: spacing.sm }}
            onPress={() => router.push('/settings')}
          >
            <Text
              style={{
                color: colors.text.primary,
                fontSize: typography.fontSize.md,
              }}
            >
              Settings
            </Text>
          </Card>

          <Card
            style={{ marginBottom: spacing.sm }}
            onPress={() => {
              // Navigate to notifications
            }}
          >
            <Text
              style={{
                color: colors.text.primary,
                fontSize: typography.fontSize.md,
              }}
            >
              Notifications
            </Text>
          </Card>

          <Card
            style={{ marginBottom: spacing.sm }}
            onPress={() => {
              // Navigate to help
            }}
          >
            <Text
              style={{
                color: colors.text.primary,
                fontSize: typography.fontSize.md,
              }}
            >
              Help & Support
            </Text>
          </Card>
        </View>

        <View style={{ marginTop: spacing.xl }}>
          <Button
            label="Sign Out"
            variant="secondary"
            onPress={handleLogout}
          />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    flexGrow: 1,
  },
  header: {
    alignItems: 'center',
    paddingVertical: 24,
  },
  avatar: {},
  name: {},
  email: {},
  section: {},
});
