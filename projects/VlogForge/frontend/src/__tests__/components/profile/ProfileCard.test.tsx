/**
 * Unit tests for ProfileCard component
 * ACF-015 Phase 1 - Profile Management
 */

import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { ProfileCard } from '@/components/profile/ProfileCard';

const mockPublicProfile = {
  id: 'profile-1',
  username: 'testuser',
  displayName: 'Test User',
  bio: 'This is a test bio for the user',
  profilePictureUrl: 'https://example.com/avatar.jpg',
  nicheTags: ['tech', 'gaming', 'music'],
  openToCollaborations: true,
  connectedPlatforms: [
    { platform: 'YouTube', isConnected: true },
    { platform: 'TikTok', isConnected: false },
  ],
};

const mockCreatorProfile = {
  id: 'profile-1',
  userId: 'user-1',
  username: 'creatoruser',
  displayName: 'Creator User',
  bio: 'Creator bio',
  profilePictureUrl: 'https://example.com/creator.jpg',
  nicheTags: ['vlogging'],
  openToCollaborations: false,
  collaborationPreferences: 'Looking for video collaborations',
  connectedPlatforms: [
    { platform: 'YouTube', isConnected: true },
  ],
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-15T00:00:00Z',
};

describe('ProfileCard', () => {
  it('should render profile display name', () => {
    render(<ProfileCard profile={mockPublicProfile} />);

    expect(screen.getByText('Test User')).toBeInTheDocument();
  });

  it('should render profile username', () => {
    render(<ProfileCard profile={mockPublicProfile} />);

    expect(screen.getByText('@testuser')).toBeInTheDocument();
  });

  it('should render profile bio', () => {
    render(<ProfileCard profile={mockPublicProfile} />);

    expect(screen.getByText('This is a test bio for the user')).toBeInTheDocument();
  });

  it('should render avatar with fallback initials when image not loaded', () => {
    // Radix Avatar shows fallback in test environment (image load event doesn't fire)
    render(<ProfileCard profile={mockPublicProfile} />);

    // Should show fallback initials "TU" for "Test User"
    expect(screen.getByText('TU')).toBeInTheDocument();
  });

  it('should render niche tags as badges', () => {
    render(<ProfileCard profile={mockPublicProfile} />);

    expect(screen.getByText('tech')).toBeInTheDocument();
    expect(screen.getByText('gaming')).toBeInTheDocument();
    expect(screen.getByText('music')).toBeInTheDocument();
  });

  it('should show collaboration status when open', () => {
    render(<ProfileCard profile={mockPublicProfile} />);

    expect(screen.getByText('Open to Collaborations')).toBeInTheDocument();
  });

  it('should show connected platforms', () => {
    render(<ProfileCard profile={mockPublicProfile} />);

    expect(screen.getByText('YouTube')).toBeInTheDocument();
    expect(screen.getByText('TikTok')).toBeInTheDocument();
  });

  it('should not show collaboration badge when closed', () => {
    render(<ProfileCard profile={mockCreatorProfile} />);

    expect(screen.queryByText('Open to Collaborations')).not.toBeInTheDocument();
  });

  it('should show collaboration preferences for full profiles', () => {
    render(<ProfileCard profile={mockCreatorProfile} />);

    expect(screen.getByText('Looking for video collaborations')).toBeInTheDocument();
  });

  it('should handle profile without avatar (show initials)', () => {
    const profileWithoutAvatar = {
      ...mockPublicProfile,
      profilePictureUrl: undefined,
    };

    render(<ProfileCard profile={profileWithoutAvatar} />);

    // Should show fallback initials "TU" for "Test User"
    expect(screen.getByText('TU')).toBeInTheDocument();
  });

  it('should handle empty niche tags', () => {
    const profileWithoutTags = {
      ...mockPublicProfile,
      nicheTags: [],
    };

    render(<ProfileCard profile={profileWithoutTags} />);

    // Should still render without errors
    expect(screen.getByText('Test User')).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const { container } = render(
      <ProfileCard profile={mockPublicProfile} className="custom-class" />
    );

    expect(container.firstChild).toHaveClass('custom-class');
  });

  it('should show joined date for full profiles', () => {
    render(<ProfileCard profile={mockCreatorProfile} />);

    expect(screen.getByText(/Joined/)).toBeInTheDocument();
  });
});
