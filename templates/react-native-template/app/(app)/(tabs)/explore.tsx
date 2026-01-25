import { View, Text, StyleSheet, TextInput } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useState, useEffect } from 'react';

import { useTheme } from '@/theme';
import { useScreenTransaction } from '@/observability/useScreenTransaction';

export default function ExploreScreen() {
  const { colors, spacing, typography, radius } = useTheme();
  const { markInteractive } = useScreenTransaction('Explore');
  const [searchQuery, setSearchQuery] = useState('');

  // Mark interactive once component is mounted
  useEffect(() => {
    markInteractive();
  }, [markInteractive]);

  return (
    <SafeAreaView
      style={[styles.container, { backgroundColor: colors.background.primary }]}
      edges={['top']}
    >
      <View style={[styles.content, { padding: spacing.lg }]}>
        <Text
          style={[
            styles.title,
            {
              color: colors.text.primary,
              fontSize: typography.fontSize['2xl'],
              fontFamily: typography.fontFamily.bold,
              marginBottom: spacing.lg,
            },
          ]}
        >
          Explore
        </Text>

        <View
          style={[
            styles.searchContainer,
            {
              backgroundColor: colors.background.secondary,
              borderRadius: radius.md,
              padding: spacing.md,
              marginBottom: spacing.lg,
            },
          ]}
        >
          <TextInput
            style={[
              styles.searchInput,
              {
                color: colors.text.primary,
                fontSize: typography.fontSize.md,
              },
            ]}
            placeholder="Search..."
            placeholderTextColor={colors.text.tertiary}
            value={searchQuery}
            onChangeText={setSearchQuery}
            testID="search-input"
          />
        </View>

        <View style={styles.emptyState}>
          <Text
            style={{
              color: colors.text.secondary,
              fontSize: typography.fontSize.md,
              textAlign: 'center',
            }}
          >
            Start typing to search
          </Text>
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    flex: 1,
  },
  title: {},
  searchContainer: {},
  searchInput: {
    height: 24,
  },
  emptyState: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
});
