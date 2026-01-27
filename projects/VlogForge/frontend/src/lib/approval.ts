import { apiClient } from './api-client';
import type {
  ApprovalHistoryResponse,
  ApproveContentRequest,
  ConfigureWorkflowRequest,
  ContentIdeaResponse,
  PendingApprovalsResponse,
  RequestChangesRequest,
  SubmitForApprovalRequest,
  WorkflowSettingsResponse,
  ApiResponse,
} from '@/types';

/**
 * Approval service for approval workflow operations
 * Story: ACF-009
 * Matches backend ApprovalsController endpoints
 */
export const approvalService = {
  /**
   * Configure workflow settings for a team
   * AC1: Configure Workflow
   */
  async configureWorkflow(
    teamId: string,
    data: ConfigureWorkflowRequest
  ): Promise<WorkflowSettingsResponse> {
    const response = await apiClient.post<ApiResponse<WorkflowSettingsResponse>>(
      `/teams/${teamId}/workflow`,
      data
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to configure workflow');
    }

    return response.data;
  },

  /**
   * Get pending approvals for a team
   */
  async getPendingApprovals(teamId: string): Promise<PendingApprovalsResponse> {
    const response = await apiClient.get<ApiResponse<PendingApprovalsResponse>>(
      `/teams/${teamId}/pending-approvals`
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to fetch pending approvals');
    }

    return response.data;
  },

  /**
   * Submit content for approval
   * AC2: Submit for Approval
   */
  async submitForApproval(
    contentId: string,
    data: SubmitForApprovalRequest
  ): Promise<ContentIdeaResponse> {
    const response = await apiClient.post<ApiResponse<ContentIdeaResponse>>(
      `/content/${contentId}/submit`,
      data
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to submit for approval');
    }

    return response.data;
  },

  /**
   * Approve content
   * AC3: Approve Content
   */
  async approveContent(
    contentId: string,
    data: ApproveContentRequest
  ): Promise<ContentIdeaResponse> {
    const response = await apiClient.post<ApiResponse<ContentIdeaResponse>>(
      `/content/${contentId}/approve`,
      data
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to approve content');
    }

    return response.data;
  },

  /**
   * Request changes on content
   * AC4: Request Changes
   */
  async requestChanges(
    contentId: string,
    data: RequestChangesRequest
  ): Promise<ContentIdeaResponse> {
    const response = await apiClient.post<ApiResponse<ContentIdeaResponse>>(
      `/content/${contentId}/request-changes`,
      data
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to request changes');
    }

    return response.data;
  },

  /**
   * Get approval history for content
   * AC5: Approval History
   */
  async getApprovalHistory(contentId: string): Promise<ApprovalHistoryResponse> {
    const response = await apiClient.get<ApiResponse<ApprovalHistoryResponse>>(
      `/content/${contentId}/approval-history`
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to fetch approval history');
    }

    return response.data;
  },
};

export default approvalService;
