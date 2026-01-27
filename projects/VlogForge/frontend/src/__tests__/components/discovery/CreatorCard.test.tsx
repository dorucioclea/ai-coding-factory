/**
 * Unit tests for CreatorCard component
 * Story: ACF-010 - Creator Discovery
 */

import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { CreatorCard } from '@/components/discovery/CreatorCard';
import type { DiscoveryCreatorDto } from '@/types/discovery';

// Mock next/link
vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}));

const mockCreator: DiscoveryCreatorDto = {
  id: 'creator-1',
  username: 'testcreator',
  displayName: 'Test Creator',
  bio: 'A passionate content creator sharing gaming tips and tech reviews.',
  profilePictureUrl: 'https://example.com/avatar.jpg',
  openToCollaborations: true,
  nicheTags: ['gaming', 'tech', 'reviews'],
  platforms: [
    { platformType: 'YouTube', handle: '@testcreator', followerCount: 50000 },
    { platformType: 'TikTok', handle: '@testcreator', followerCount: 25000 },
  ],
  totalFollowers: 75000,
};

describe('CreatorCard', () => {
  it('should render creator display name', () => {
    render(<CreatorCard creator={mockCreator} />);

    expect(screen.getByText('Test Creator')).toBeInTheDocument();
  });

  it('should render creator username with @ prefix', () => {
    render(<CreatorCard creator={mockCreator} />);

    expect(screen.getByText('@testcreator')).toBeInTheDocument();
  });

  it('should render creator bio', () => {
    render(<CreatorCard creator={mockCreator} />);

    expect(
      screen.getByText(/A passionate content creator/)
    ).toBeInTheDocument();
  });

  it('should render formatted follower count', () => {
    render(<CreatorCard creator={mockCreator} />);

    expect(screen.getByText('75.0K followers')).toBeInTheDocument();
  });

  it('should render niche tags', () => {
    render(<CreatorCard creator={mockCreator} />);

    expect(screen.getByText('gaming')).toBeInTheDocument();
    expect(screen.getByText('tech')).toBeInTheDocument();
    expect(screen.getByText('reviews')).toBeInTheDocument();
  });

  it('should show +N badge when more than 3 niche tags', () => {
    const creatorWithManyTags: DiscoveryCreatorDto = {
      ...mockCreator,
      nicheTags: ['gaming', 'tech', 'reviews', 'tutorials', 'vlogs'],
    };

    render(<CreatorCard creator={creatorWithManyTags} />);

    // Should show first 3 tags and +2 badge
    expect(screen.getByText('gaming')).toBeInTheDocument();
    expect(screen.getByText('tech')).toBeInTheDocument();
    expect(screen.getByText('reviews')).toBeInTheDocument();
    expect(screen.getByText('+2')).toBeInTheDocument();
  });

  it('should render Open badge when open to collaborations', () => {
    render(<CreatorCard creator={mockCreator} />);

    expect(screen.getByText('Open')).toBeInTheDocument();
  });

  it('should not render Open badge when not open to collaborations', () => {
    const closedCreator: DiscoveryCreatorDto = {
      ...mockCreator,
      openToCollaborations: false,
    };

    render(<CreatorCard creator={closedCreator} />);

    expect(screen.queryByText('Open')).not.toBeInTheDocument();
  });

  it('should render platforms', () => {
    render(<CreatorCard creator={mockCreator} />);

    expect(screen.getByText('YouTube')).toBeInTheDocument();
    expect(screen.getByText('TikTok')).toBeInTheDocument();
  });

  it('should link to creator profile', () => {
    render(<CreatorCard creator={mockCreator} />);

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', '/profile/testcreator');
  });

  it('should render without bio', () => {
    const creatorWithoutBio: DiscoveryCreatorDto = {
      ...mockCreator,
      bio: '',
    };

    render(<CreatorCard creator={creatorWithoutBio} />);

    expect(screen.getByText('Test Creator')).toBeInTheDocument();
  });

  it('should render without platforms', () => {
    const creatorWithoutPlatforms: DiscoveryCreatorDto = {
      ...mockCreator,
      platforms: [],
    };

    render(<CreatorCard creator={creatorWithoutPlatforms} />);

    expect(screen.getByText('Test Creator')).toBeInTheDocument();
  });

  it('should render initials in avatar fallback', () => {
    const creatorWithoutAvatar: DiscoveryCreatorDto = {
      ...mockCreator,
      profilePictureUrl: undefined,
    };

    render(<CreatorCard creator={creatorWithoutAvatar} />);

    // Initials should be "TC" for "Test Creator"
    expect(screen.getByText('TC')).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const { container } = render(
      <CreatorCard creator={mockCreator} className="custom-class" />
    );

    const card = container.querySelector('.custom-class');
    expect(card).toBeInTheDocument();
  });
});
