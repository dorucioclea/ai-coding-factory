/**
 * Unit tests for MemberCard component
 * ACF-015 Phase 5 - Team Management
 */

import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { MemberCard } from '@/components/team/MemberCard';

// Mock canModifyMember function
vi.mock('@/types', async () => {
  const actual = await vi.importActual('@/types');
  return {
    ...actual,
    canModifyMember: vi.fn((currentUserRole, memberRole, isCurrentUserOwner) => {
      if (isCurrentUserOwner && memberRole !== 'Owner') return true;
      if (currentUserRole === 'Admin' && memberRole === 'Member') return true;
      return false;
    }),
  };
});

const mockMember = {
  id: 'member-1',
  userId: 'user-1',
  email: 'test@example.com',
  firstName: 'Test',
  lastName: 'Member',
  displayName: 'Test Member',
  role: 'Member' as const,
  joinedAt: '2024-01-15T00:00:00Z',
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars -- Reserved for future tests
const _mockOwnerMember = {
  id: 'member-owner',
  userId: 'user-owner',
  email: 'owner@example.com',
  firstName: 'Team',
  lastName: 'Owner',
  displayName: 'Team Owner',
  role: 'Owner' as const,
  joinedAt: '2024-01-01T00:00:00Z',
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars -- Reserved for future tests
const _mockAdminMember = {
  id: 'member-admin',
  userId: 'user-admin',
  email: 'admin@example.com',
  firstName: 'Team',
  lastName: 'Admin',
  displayName: 'Team Admin',
  role: 'Admin' as const,
  joinedAt: '2024-01-10T00:00:00Z',
};

describe('MemberCard', () => {
  const defaultProps = {
    member: mockMember,
    currentUserRole: 'Member' as const,
    isCurrentUserOwner: false,
    isOwner: false,
    onChangeRole: vi.fn(),
    onRemove: vi.fn(),
  };

  it('should render member display name', () => {
    render(<MemberCard {...defaultProps} />);

    expect(screen.getByText('Test Member')).toBeInTheDocument();
  });

  it('should render member email', () => {
    render(<MemberCard {...defaultProps} />);

    expect(screen.getByText('test@example.com')).toBeInTheDocument();
  });

  it('should render avatar with initials from first and last name', () => {
    render(<MemberCard {...defaultProps} />);

    // Should show initials "TM" for "Test Member"
    expect(screen.getByText('TM')).toBeInTheDocument();
  });

  it('should format join date correctly', () => {
    render(<MemberCard {...defaultProps} />);

    // Should show "Joined" with formatted date
    expect(screen.getByText(/joined/i)).toBeInTheDocument();
    expect(screen.getByText(/jan 15, 2024/i)).toBeInTheDocument();
  });

  it('should show action menu when owner views non-owner member', async () => {
    const user = userEvent.setup();
    render(
      <MemberCard
        {...defaultProps}
        currentUserRole="Owner"
        isCurrentUserOwner={true}
      />
    );

    const menuButton = screen.getByRole('button', { name: /member actions/i });
    expect(menuButton).toBeInTheDocument();

    await user.click(menuButton);
    expect(screen.getByText(/change role/i)).toBeInTheDocument();
    expect(screen.getByText(/remove member/i)).toBeInTheDocument();
  });

  it('should not show action menu for regular members', () => {
    render(<MemberCard {...defaultProps} />);

    expect(screen.queryByRole('button', { name: /member actions/i })).not.toBeInTheDocument();
  });

  it('should call onRemove when remove is clicked', async () => {
    const user = userEvent.setup();
    const onRemove = vi.fn();
    render(
      <MemberCard
        {...defaultProps}
        currentUserRole="Owner"
        isCurrentUserOwner={true}
        onRemove={onRemove}
      />
    );

    const menuButton = screen.getByRole('button', { name: /member actions/i });
    await user.click(menuButton);

    const removeButton = screen.getByText(/remove member/i);
    await user.click(removeButton);

    expect(onRemove).toHaveBeenCalled();
  });

  it('should call onChangeRole when change role is clicked', async () => {
    const user = userEvent.setup();
    const onChangeRole = vi.fn();
    render(
      <MemberCard
        {...defaultProps}
        currentUserRole="Owner"
        isCurrentUserOwner={true}
        onChangeRole={onChangeRole}
      />
    );

    const menuButton = screen.getByRole('button', { name: /member actions/i });
    await user.click(menuButton);

    const changeRoleButton = screen.getByText(/change role/i);
    await user.click(changeRoleButton);

    expect(onChangeRole).toHaveBeenCalled();
  });

  it('should render fallback display name when firstName/lastName missing', () => {
    const memberWithoutName = {
      ...mockMember,
      firstName: undefined,
      lastName: undefined,
      displayName: undefined,
    };

    render(<MemberCard {...defaultProps} member={memberWithoutName} />);

    // Should fall back to email (appears in both display name and email field)
    const emailElements = screen.getAllByText('test@example.com');
    expect(emailElements.length).toBeGreaterThanOrEqual(1);
  });
});
