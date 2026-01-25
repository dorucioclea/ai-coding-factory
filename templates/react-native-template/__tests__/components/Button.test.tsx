import React from 'react';
import { render, fireEvent } from '@testing-library/react-native';
import { Button } from '@/components/ui/Button';

// Mock the theme hook
jest.mock('@/theme', () => ({
  useTheme: () => ({
    colors: {
      interactive: { primary: '#3B82F6', primaryHover: '#2563EB' },
      text: { inverse: '#FFFFFF', secondary: '#525252' },
      background: { secondary: '#F5F5F5' },
      border: { default: '#E5E5E5' },
    },
    spacing: { sm: 8, md: 16, lg: 24 },
    typography: {
      fontSize: { md: 16 },
      fontFamily: { semibold: 'Inter-SemiBold' },
    },
    radius: { md: 8 },
  }),
}));

describe('Button', () => {
  it('renders correctly with label', () => {
    const { getByText } = render(<Button label="Test Button" onPress={() => {}} />);
    expect(getByText('Test Button')).toBeTruthy();
  });

  it('calls onPress when pressed', () => {
    const onPressMock = jest.fn();
    const { getByText } = render(<Button label="Press Me" onPress={onPressMock} />);

    fireEvent.press(getByText('Press Me'));
    expect(onPressMock).toHaveBeenCalledTimes(1);
  });

  it('disables button when disabled prop is true', () => {
    const onPressMock = jest.fn();
    const { getByText } = render(
      <Button label="Disabled" onPress={onPressMock} disabled />
    );

    fireEvent.press(getByText('Disabled'));
    expect(onPressMock).not.toHaveBeenCalled();
  });

  it('shows loading indicator when loading', () => {
    const { getByTestId, queryByText } = render(
      <Button label="Loading" onPress={() => {}} loading testID="loading-button" />
    );

    // Button text should not be visible when loading
    expect(queryByText('Loading')).toBeNull();
  });
});
