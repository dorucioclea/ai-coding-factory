import { View, Text, StyleSheet, ScrollView, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useState, useCallback, useEffect } from 'react';

import { useAuth } from '@/hooks/useAuth';
import { useTheme } from '@/theme';
import { useScreenTransaction } from '@/observability/useScreenTransaction';
import { Card } from '@/components/ui/Card';

export default function HomeScreen() {
  const { user } = useAuth();
  const { colors, spacing, typography } = useTheme();
  const { markInteractive } = useScreenTransaction('Home');
  const [refreshing, setRefreshing] = useState(false);

  // Mark interactive once component is mounted
  useEffect(() => {
    markInteractive();
  }, [markInteractive]);

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    // Add refresh logic here
    await new Promise((resolve) => setTimeout(resolve, 1000));
    setRefreshing(false);
  }, []);

  return (
    <SafeAreaView
      style={[styles.container, { backgroundColor: colors.background.primary }]}
      edges={['top']}
    >
      <ScrollView
        contentContainerStyle={[styles.content, { padding: spacing.lg }]}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
      >
        <View style={styles.header}>
          <Text
            style={[
              styles.greeting,
              {
                color: colors.text.secondary,
                fontSize: typography.fontSize.md,
              },
            ]}
          >
            Welcome back,
          </Text>
          <Text
            style={[
              styles.name,
              {
                color: colors.text.primary,
                fontSize: typography.fontSize['2xl'],
                fontFamily: typography.fontFamily.bold,
              },
            ]}
          >
            {user?.name ?? 'User'}
          </Text>
        </View>

        <View style={[styles.section, { marginTop: spacing.xl }]}>
          <Text
            style={[
              styles.sectionTitle,
              {
                color: colors.text.primary,
                fontSize: typography.fontSize.lg,
                fontFamily: typography.fontFamily.semibold,
                marginBottom: spacing.md,
              },
            ]}
          >
            Quick Actions
          </Text>

          <Card
            style={{ marginBottom: spacing.md }}
            onPress={() => {
              // Navigate to feature
            }}
          >
            <Text
              style={{
                color: colors.text.primary,
                fontSize: typography.fontSize.md,
                fontFamily: typography.fontFamily.medium,
              }}
            >
              Feature Card
            </Text>
            <Text
              style={{
                color: colors.text.secondary,
                fontSize: typography.fontSize.sm,
                marginTop: spacing.xs,
              }}
            >
              Tap to explore this feature
            </Text>
          </Card>

          <Card>
            <Text
              style={{
                color: colors.text.primary,
                fontSize: typography.fontSize.md,
                fontFamily: typography.fontFamily.medium,
              }}
            >
              Another Card
            </Text>
            <Text
              style={{
                color: colors.text.secondary,
                fontSize: typography.fontSize.sm,
                marginTop: spacing.xs,
              }}
            >
              Description of this card
            </Text>
          </Card>
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
    marginBottom: 16,
  },
  greeting: {},
  name: {},
  section: {},
  sectionTitle: {},
});
