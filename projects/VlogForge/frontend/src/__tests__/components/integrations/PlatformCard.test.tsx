/**
 * Unit tests for PlatformCard component
 * ACF-015 Phase 2 - Integrations
 */

import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { createWrapper } from '../../utils/test-utils';
import { PlatformCard } from '@/components/integrations/PlatformCard';
import { PLATFORM_METADATA } from '@/types/integrations';

// Mock the hooks
vi.mock('@/hooks', () => ({
  useInitiateOAuth: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
  useDisconnectPlatform: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
  useSyncPlatform: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
}));

// Mock child components
vi.mock('@/components/integrations/ConnectionStatusBadge', () => ({
  ConnectionStatusBadge: ({ status }: { status: string }) => (
    <span data-testid="connection-status">{status}</span>
  ),
}));

vi.mock('@/components/integrations/OAuthConnectButton', () => ({
  OAuthConnectButton: ({ platform }: { platform: string }) => (
    <button data-testid="connect-button">Connect {platform}</button>
  ),
}));

vi.mock('@/components/integrations/DisconnectConfirmModal', () => ({
  DisconnectConfirmModal: () => null,
}));

const mockYouTubeMetadata = PLATFORM_METADATA.YouTube;

const mockConnectedConnection = {
  id: 'conn-1',
  platformType: 'YouTube' as const,
  status: 'Connected' as const,
  platformAccountId: 'account-1',
  platformAccountName: 'Test Channel',
  lastSyncAt: '2024-01-15T10:00:00Z',
  createdAt: '2024-01-01T00:00:00Z',
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars -- Reserved for future tests
const _mockDisconnectedConnection = {
  id: 'conn-2',
  platformType: 'YouTube' as const,
  status: 'Disconnected' as const,
  platformAccountId: '',
  platformAccountName: '',
  createdAt: '2024-01-01T00:00:00Z',
};

const mockErrorConnection = {
  id: 'conn-3',
  platformType: 'YouTube' as const,
  status: 'Error' as const,
  platformAccountId: 'account-1',
  platformAccountName: 'Test Channel',
  lastSyncAt: '2024-01-10T10:00:00Z',
  errorMessage: 'Token expired',
  createdAt: '2024-01-01T00:00:00Z',
};

describe('PlatformCard', () => {
  it('should render platform name', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={undefined}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText('YouTube')).toBeInTheDocument();
  });

  it('should render platform description', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={undefined}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText(mockYouTubeMetadata.description)).toBeInTheDocument();
  });

  it('should show Connect button when not connected', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={undefined}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByTestId('connect-button')).toBeInTheDocument();
  });

  it('should show Disconnect button when connected', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={mockConnectedConnection}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByRole('button', { name: /disconnect/i })).toBeInTheDocument();
  });

  it('should show account name when connected', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={mockConnectedConnection}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText(/Test Channel/)).toBeInTheDocument();
  });

  it('should show error status and message', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={mockErrorConnection}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText(/Connection Error/)).toBeInTheDocument();
    expect(screen.getByText('Token expired')).toBeInTheDocument();
  });

  it('should show status badge when connection exists', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={mockConnectedConnection}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByTestId('connection-status')).toHaveTextContent('Connected');
  });

  it('should show sync button when connected', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={mockConnectedConnection}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByRole('button', { name: /sync/i })).toBeInTheDocument();
  });

  it('should render platform features', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={undefined}
      />,
      { wrapper: createWrapper() }
    );

    // YouTube metadata should have features listed
    mockYouTubeMetadata.features.forEach(feature => {
      expect(screen.getByText(feature)).toBeInTheDocument();
    });
  });

  it('should handle undefined connection gracefully', () => {
    render(
      <PlatformCard
        metadata={mockYouTubeMetadata}
        connection={undefined}
      />,
      { wrapper: createWrapper() }
    );

    // Should show connect button for undefined connection
    expect(screen.getByTestId('connect-button')).toBeInTheDocument();
  });
});
