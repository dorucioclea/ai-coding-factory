'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';

import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import type {
  TeamResponse,
  TeamWithMembersResponse,
  CreateTeamRequest,
  InviteMemberRequest,
  ChangeMemberRoleRequest,
  ApiResponse,
} from '@/types';

/**
 * Hook for fetching user's teams
 * ACF-015 Phase 5
 */
export function useMyTeams() {
  return useQuery({
    queryKey: queryKeys.teams.lists(),
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<TeamResponse[]>>('/teams');
      return response.data ?? [];
    },
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook for fetching team with members
 * ACF-015 Phase 5
 */
export function useTeam(id: string | undefined) {
  return useQuery({
    // eslint-disable-next-line @tanstack/query/exhaustive-deps -- id is conditionally included
    queryKey: id ? queryKeys.teams.detail(id) : ['teams', 'detail', 'undefined'],
    queryFn: async () => {
      if (!id) throw new Error('Team ID is required');
      const response = await apiClient.get<ApiResponse<TeamWithMembersResponse>>(
        `/teams/${id}`
      );
      return response.data;
    },
    enabled: !!id,
    staleTime: 1000 * 60 * 2, // 2 minutes
  });
}

/**
 * Hook for creating a team
 * ACF-015 Phase 5
 */
export function useCreateTeam() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateTeamRequest) => {
      const response = await apiClient.post<ApiResponse<TeamResponse>>(
        '/teams',
        data
      );
      return response.data;
    },
    onSuccess: () => {
      // Invalidate teams list
      queryClient.invalidateQueries({ queryKey: queryKeys.teams.lists() });
    },
  });
}

/**
 * Hook for inviting a member to team
 * ACF-015 Phase 5
 */
export function useInviteMember(teamId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: InviteMemberRequest) => {
      const response = await apiClient.post<ApiResponse<void>>(
        `/teams/${teamId}/invite`,
        data
      );
      return response.data;
    },
    onSuccess: () => {
      // Invalidate team details and invitations
      queryClient.invalidateQueries({ queryKey: queryKeys.teams.detail(teamId) });
      queryClient.invalidateQueries({
        queryKey: queryKeys.teams.invitations(teamId),
      });
    },
  });
}

/**
 * Hook for accepting an invitation
 * ACF-015 Phase 5
 */
export function useAcceptInvitation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (token: string) => {
      const response = await apiClient.post<ApiResponse<TeamResponse>>(
        `/teams/invitations/${token}/accept`
      );
      return response.data;
    },
    onSuccess: () => {
      // Invalidate teams list
      queryClient.invalidateQueries({ queryKey: queryKeys.teams.lists() });
    },
  });
}

/**
 * Hook for changing a member's role
 * ACF-015 Phase 5
 */
export function useChangeMemberRole(teamId: string, userId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: ChangeMemberRoleRequest) => {
      const response = await apiClient.put<ApiResponse<void>>(
        `/teams/${teamId}/members/${userId}/role`,
        data
      );
      return response.data;
    },
    onSuccess: () => {
      // Invalidate team details
      queryClient.invalidateQueries({ queryKey: queryKeys.teams.detail(teamId) });
    },
  });
}

/**
 * Hook for removing a member from team
 * ACF-015 Phase 5
 */
export function useRemoveMember(teamId: string, userId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      const response = await apiClient.delete<ApiResponse<void>>(
        `/teams/${teamId}/members/${userId}`
      );
      return response.data;
    },
    onSuccess: () => {
      // Invalidate team details
      queryClient.invalidateQueries({ queryKey: queryKeys.teams.detail(teamId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.teams.lists() });
    },
  });
}

/**
 * Hook for team utilities
 * ACF-015 Phase 5
 */
export function useTeamUtils(teamId: string | undefined) {
  const { data: team } = useTeam(teamId);

  const getCurrentUserMember = useCallback(
    (userId: string | undefined) => {
      if (!team || !userId) return null;
      return team.members.find((m) => m.userId === userId);
    },
    [team]
  );

  const isOwner = useCallback(
    (userId: string | undefined) => {
      if (!team || !userId) return false;
      return team.ownerId === userId;
    },
    [team]
  );

  const canManageTeam = useCallback(
    (userId: string | undefined) => {
      const member = getCurrentUserMember(userId);
      if (!member) return false;
      // Admin and Owner can manage
      return member.role >= 2; // TeamRole.Admin
    },
    [getCurrentUserMember]
  );

  return {
    team,
    getCurrentUserMember,
    isOwner,
    canManageTeam,
  };
}

export default useMyTeams;
