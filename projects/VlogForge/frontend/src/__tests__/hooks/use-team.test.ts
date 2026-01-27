/**
 * Unit tests for use-team hook
 * ACF-015 Phase 5 - Team Management
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  useMyTeams,
  useTeam,
  useCreateTeam,
  useInviteMember,
  useAcceptInvitation,
  useChangeMemberRole,
  useRemoveMember,
} from '@/hooks/use-team';

// Mock the api client
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';

const mockTeam = {
  id: 'team-1',
  name: 'Test Team',
  description: 'Test description',
  ownerId: 'user-1',
  createdAt: '2024-01-01T00:00:00Z',
  memberCount: 3,
};

const mockTeamWithMembers = {
  ...mockTeam,
  members: [
    {
      id: 'member-1',
      userId: 'user-1',
      displayName: 'Owner',
      role: 'Owner' as const,
      joinedAt: '2024-01-01T00:00:00Z',
    },
    {
      id: 'member-2',
      userId: 'user-2',
      displayName: 'Admin',
      role: 'Admin' as const,
      joinedAt: '2024-01-02T00:00:00Z',
    },
  ],
};

describe('useMyTeams', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch user teams successfully', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: [mockTeam] });

    const { result } = renderHook(() => useMyTeams(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual([mockTeam]);
    expect(apiClient.get).toHaveBeenCalledWith('/teams');
  });

  it('should return empty array when no teams', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: null });

    const { result } = renderHook(() => useMyTeams(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual([]);
  });

  it('should handle error state', async () => {
    vi.mocked(apiClient.get).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useMyTeams(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});

describe('useTeam', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch team with members', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: mockTeamWithMembers });

    const { result } = renderHook(() => useTeam('team-1'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockTeamWithMembers);
    expect(apiClient.get).toHaveBeenCalledWith('/teams/team-1');
  });

  it('should not fetch when id is undefined', async () => {
    const { result } = renderHook(() => useTeam(undefined), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(false);
    expect(apiClient.get).not.toHaveBeenCalled();
  });
});

describe('useCreateTeam', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should create team successfully', async () => {
    const newTeamData = {
      name: 'New Team',
      description: 'New team description',
    };
    vi.mocked(apiClient.post).mockResolvedValue({ data: { ...mockTeam, ...newTeamData } });

    const { result } = renderHook(() => useCreateTeam(), {
      wrapper: createWrapper(),
    });

    result.current.mutate(newTeamData);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/teams', newTeamData);
  });
});

describe('useInviteMember', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should invite member successfully', async () => {
    const inviteData = {
      email: 'newuser@example.com',
      role: 'Member' as const,
    };
    vi.mocked(apiClient.post).mockResolvedValue({ data: undefined });

    // Hook takes teamId as constructor argument
    const { result } = renderHook(() => useInviteMember('team-1'), {
      wrapper: createWrapper(),
    });

    result.current.mutate(inviteData);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/teams/team-1/invite', inviteData);
  });
});

describe('useAcceptInvitation', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should accept invitation successfully', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: mockTeam });

    const { result } = renderHook(() => useAcceptInvitation(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('invitation-token');

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.post).toHaveBeenCalledWith('/teams/invitations/invitation-token/accept');
  });
});

describe('useChangeMemberRole', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should change member role successfully', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({
      data: undefined,
    });

    // Hook takes teamId and userId as constructor arguments
    const { result } = renderHook(() => useChangeMemberRole('team-1', 'user-2'), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ role: 'Member' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.put).toHaveBeenCalledWith('/teams/team-1/members/user-2/role', {
      role: 'Member',
    });
  });
});

describe('useRemoveMember', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should remove member successfully', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: undefined });

    // Hook takes teamId and userId as constructor arguments
    const { result } = renderHook(() => useRemoveMember('team-1', 'user-2'), {
      wrapper: createWrapper(),
    });

    // Mutation takes no arguments since teamId/userId are in the hook
    result.current.mutate();

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiClient.delete).toHaveBeenCalledWith('/teams/team-1/members/user-2');
  });
});
