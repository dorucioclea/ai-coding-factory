# /add-mobile-screen - Add React Native Screen

Scaffold a new Expo Router screen with observability and design system integration.

## Usage
```
/add-mobile-screen <ScreenName> [route-path] [options]
```

Examples:
- `/add-mobile-screen ProductDetail /products/[id]`
- `/add-mobile-screen Settings /settings`
- `/add-mobile-screen OrderHistory /(tabs)/orders`
- `/add-mobile-screen Login /(auth)/login --minimal`

Options:
- `--minimal` - Basic screen without data fetching
- `--list` - Include FlatList pattern
- `--form` - Include form with validation
- `--protected` - Require authentication
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Parse Screen Information

Extract:
- Screen name (PascalCase)
- Route path (determines file location)
- Screen type (minimal, list, form)
- Auth requirement

### 2. Determine File Location

Map route path to Expo Router file structure:

| Route Path | File Location |
|------------|---------------|
| `/products/[id]` | `app/products/[id].tsx` |
| `/(tabs)/orders` | `app/(tabs)/orders.tsx` |
| `/(auth)/login` | `app/(auth)/login.tsx` |
| `/settings/profile` | `app/settings/profile.tsx` |

### 3. Generate Screen Component

**Standard Screen Template**:
```typescript
// app/<route-path>.tsx
import { View, Text, StyleSheet } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Stack, useLocalSearchParams } from 'expo-router';
import * as Sentry from '@sentry/react-native';
import { useTheme } from '@/theme';
import { useScreenTransaction } from '@/observability/useScreenTransaction';
import { ScreenErrorBoundary } from '@/observability/ErrorBoundary';

export default function <ScreenName>Screen() {
  const { colors, spacing } = useTheme();
  const params = useLocalSearchParams<{ id: string }>();
  const { markInteractive } = useScreenTransaction('<ScreenName>');

  // Track screen load
  useEffect(() => {
    markInteractive();
  }, []);

  return (
    <ScreenErrorBoundary screenName="<ScreenName>">
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background.primary }]}>
        <Stack.Screen options={{ title: '<Screen Title>' }} />

        {/* Screen content */}

      </SafeAreaView>
    </ScreenErrorBoundary>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
});
```

**List Screen Template** (`--list`):
```typescript
import { FlatList, RefreshControl } from 'react-native';
import { useQuery } from '@tanstack/react-query';

export default function <ScreenName>Screen() {
  const { data, isLoading, error, refetch, isRefetching } = use<Entity>List();

  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorView error={error} onRetry={refetch} />;

  return (
    <SafeAreaView style={styles.container}>
      <FlatList
        data={data}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => <<Entity>Card item={item} />}
        refreshControl={
          <RefreshControl refreshing={isRefetching} onRefresh={refetch} />
        }
        contentContainerStyle={styles.list}
        ItemSeparatorComponent={() => <View style={styles.separator} />}
        ListEmptyComponent={<EmptyState message="No items found" />}
      />
    </SafeAreaView>
  );
}
```

**Form Screen Template** (`--form`):
```typescript
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const schema = z.object({
  // Define schema
});

type FormData = z.infer<typeof schema>;

export default function <ScreenName>Screen() {
  const { control, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const mutation = use<Action>Mutation();

  const onSubmit = async (data: FormData) => {
    try {
      await mutation.mutateAsync(data);
      router.back();
    } catch (error) {
      // Handle error
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : 'height'}>
        <ScrollView contentContainerStyle={styles.form}>
          <Controller
            control={control}
            name="fieldName"
            render={({ field: { onChange, value } }) => (
              <Input
                label="Field Label"
                value={value}
                onChangeText={onChange}
                error={errors.fieldName?.message}
              />
            )}
          />

          <Button
            label="Submit"
            onPress={handleSubmit(onSubmit)}
            loading={isSubmitting}
          />
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}
```

### 4. Generate Supporting Files

**For protected screens**:
- Ensure parent layout has auth check
- Add to protected route group

**For list screens**:
- Create corresponding query hook
- Create list item component

**For form screens**:
- Create Zod schema
- Create mutation hook

### 5. Add Tests

```typescript
// __tests__/<ScreenName>.test.tsx
import { render, screen } from '@testing-library/react-native';
import { renderWithProviders } from '@/test/utils';
import <ScreenName>Screen from '../<route-path>';

describe('<ScreenName>Screen', () => {
  it('renders correctly', () => {
    renderWithProviders(<<ScreenName>Screen />);
    expect(screen.getByText('<Expected Text>')).toBeTruthy();
  });

  it('tracks screen load', () => {
    renderWithProviders(<<ScreenName>Screen />);
    expect(Sentry.startTransaction).toHaveBeenCalledWith({
      name: '<ScreenName>',
      op: 'ui.load',
    });
  });
});
```

## Output

```markdown
## Screen Created: <ScreenName>

### Files Created
- `app/<route-path>.tsx` - Screen component
- `components/features/<feature>/<Component>.tsx` - Supporting components
- `hooks/api/use<Entity>.ts` - Data hooks (if needed)
- `__tests__/<ScreenName>.test.tsx` - Tests

### Route Information
- Path: /<route>
- Protected: Yes/No
- Tab: (tabs)/<name> or None

### Integration
- Observability: Screen transaction + error boundary
- Design System: Theme tokens used
- State: [TanStack Query / Redux / Local]

### Next Steps
1. Implement business logic
2. Connect to API
3. Add accessibility labels
4. Run tests: `npm test <ScreenName>`
```

## Example

```
User: /add-mobile-screen ProductDetail /products/[id] --protected --story ACF-042

Claude: Creating ProductDetail screen...

Files created:
- app/products/[id].tsx
- components/features/products/ProductDetailView.tsx
- hooks/api/useProduct.ts
- __tests__/ProductDetail.test.tsx

Screen is protected and linked to ACF-042.

Run `npm run ios` to test the new screen.
```
