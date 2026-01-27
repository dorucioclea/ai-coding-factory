'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';

import { approvalService } from '@/lib/approval';
import { QUERY_STALE_TIMES } from '@/lib/constants';
import { queryKeys } from '@/lib/query-client';
import type {
  ApproveContentRequest,
  ConfigureWorkflowRequest,
  ContentIdeaResponse,
  RequestChangesRequest,
  SubmitForApprovalRequest,
} from '@/types';

/**
 * Hook for fetching pending approvals for a team
 * Story: ACF-009
 */
export function usePendingApprovals(teamId: string) {
  return useQuery({
    queryKey: queryKeys.approvals.pending(teamId),
    queryFn: () => approvalService.getPendingApprovals(teamId),
    staleTime: QUERY_STALE_TIMES.APPROVALS,
    enabled: !!teamId,
  });
}

/**
 * Hook for fetching approval history for a content item
 * Story: ACF-009 AC5: Approval History
 */
export function useApprovalHistory(contentId: string) {
  return useQuery({
    queryKey: queryKeys.approvals.history(contentId),
    queryFn: () => approvalService.getApprovalHistory(contentId),
    staleTime: QUERY_STALE_TIMES.APPROVALS,
    enabled: !!contentId,
  });
}

/**
 * Hook for configuring workflow settings
 * Story: ACF-009 AC1: Configure Workflow
 */
export function useConfigureWorkflow() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      teamId,
      data,
    }: {
      teamId: string;
      data: ConfigureWorkflowRequest;
    }) => approvalService.configureWorkflow(teamId, data),
    onSuccess: (_, variables) => {
      // Invalidate team queries to refetch settings
      queryClient.invalidateQueries({
        queryKey: queryKeys.teams.detail(variables.teamId),
      });
    },
  });
}

/**
 * Hook for submitting content for approval
 * Story: ACF-009 AC2: Submit for Approval
 */
export function useSubmitForApproval() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      contentId,
      data,
    }: {
      contentId: string;
      data: SubmitForApprovalRequest;
    }) => approvalService.submitForApproval(contentId, data),
    onSuccess: (updatedContent: ContentIdeaResponse, variables) => {
      // Update content cache
      queryClient.setQueryData(
        queryKeys.content.detail(variables.contentId),
        updatedContent
      );

      // Invalidate content lists and pending approvals
      queryClient.invalidateQueries({
        queryKey: queryKeys.content.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.approvals.pending(variables.data.teamId),
      });
    },
  });
}

/**
 * Hook for approving content
 * Story: ACF-009 AC3: Approve Content
 */
export function useApproveContent() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      contentId,
      data,
    }: {
      contentId: string;
      data: ApproveContentRequest;
    }) => approvalService.approveContent(contentId, data),
    onSuccess: (updatedContent: ContentIdeaResponse, variables) => {
      // Update content cache
      queryClient.setQueryData(
        queryKeys.content.detail(variables.contentId),
        updatedContent
      );

      // Invalidate related queries
      queryClient.invalidateQueries({
        queryKey: queryKeys.content.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.approvals.pending(variables.data.teamId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.approvals.history(variables.contentId),
      });
    },
  });
}

/**
 * Hook for requesting changes on content
 * Story: ACF-009 AC4: Request Changes
 */
export function useRequestChanges() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      contentId,
      data,
    }: {
      contentId: string;
      data: RequestChangesRequest;
    }) => approvalService.requestChanges(contentId, data),
    onSuccess: (updatedContent: ContentIdeaResponse, variables) => {
      // Update content cache
      queryClient.setQueryData(
        queryKeys.content.detail(variables.contentId),
        updatedContent
      );

      // Invalidate related queries
      queryClient.invalidateQueries({
        queryKey: queryKeys.content.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.approvals.pending(variables.data.teamId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.approvals.history(variables.contentId),
      });
    },
  });
}

/**
 * Combined hook for approval operations
 * Story: ACF-009
 */
export function useApproval() {
  const configureWorkflowMutation = useConfigureWorkflow();
  const submitMutation = useSubmitForApproval();
  const approveMutation = useApproveContent();
  const requestChangesMutation = useRequestChanges();

  const configureWorkflow = useCallback(
    (teamId: string, data: ConfigureWorkflowRequest) =>
      configureWorkflowMutation.mutateAsync({ teamId, data }),
    [configureWorkflowMutation]
  );

  const submitForApproval = useCallback(
    (contentId: string, data: SubmitForApprovalRequest) =>
      submitMutation.mutateAsync({ contentId, data }),
    [submitMutation]
  );

  const approveContent = useCallback(
    (contentId: string, data: ApproveContentRequest) =>
      approveMutation.mutateAsync({ contentId, data }),
    [approveMutation]
  );

  const requestChanges = useCallback(
    (contentId: string, data: RequestChangesRequest) =>
      requestChangesMutation.mutateAsync({ contentId, data }),
    [requestChangesMutation]
  );

  return {
    // Actions
    configureWorkflow,
    submitForApproval,
    approveContent,
    requestChanges,

    // Mutation states
    isConfiguringWorkflow: configureWorkflowMutation.isPending,
    isSubmitting: submitMutation.isPending,
    isApproving: approveMutation.isPending,
    isRequestingChanges: requestChangesMutation.isPending,

    // Errors
    configureError: configureWorkflowMutation.error,
    submitError: submitMutation.error,
    approveError: approveMutation.error,
    requestChangesError: requestChangesMutation.error,
  };
}

export default useApproval;
