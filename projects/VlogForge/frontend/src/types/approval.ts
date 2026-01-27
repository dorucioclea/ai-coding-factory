/**
 * Approval Workflow types
 * ACF-009
 * Matches backend ApprovalDtos.cs
 */

import { IdeaStatus } from './content';

/**
 * Approval action types
 */
export enum ApprovalAction {
  Submitted = 0,
  Approved = 1,
  ChangesRequested = 2,
  Resubmitted = 3,
}

/**
 * Approval action labels for display
 */
export const ApprovalActionLabels: Record<ApprovalAction, string> = {
  [ApprovalAction.Submitted]: 'Submitted for Review',
  [ApprovalAction.Approved]: 'Approved',
  [ApprovalAction.ChangesRequested]: 'Changes Requested',
  [ApprovalAction.Resubmitted]: 'Resubmitted',
};

/**
 * Approval action colors for UI
 */
export const ApprovalActionColors: Record<ApprovalAction, string> = {
  [ApprovalAction.Submitted]: 'blue',
  [ApprovalAction.Approved]: 'green',
  [ApprovalAction.ChangesRequested]: 'red',
  [ApprovalAction.Resubmitted]: 'orange',
};

/**
 * Approval record response
 */
export interface ApprovalRecordResponse {
  id: string;
  contentItemId: string;
  teamId: string;
  actorId: string;
  action: ApprovalAction;
  feedback?: string;
  previousStatus: IdeaStatus;
  newStatus: IdeaStatus;
  createdAt: string;
}

/**
 * Approval history response
 */
export interface ApprovalHistoryResponse {
  contentItemId: string;
  records: ApprovalRecordResponse[];
}

/**
 * Pending approvals response
 */
export interface PendingApprovalsResponse {
  items: PendingApprovalItem[];
  totalCount: number;
}

/**
 * Pending approval item
 */
export interface PendingApprovalItem {
  contentItemId: string;
  title: string;
  notes?: string;
  submittedByUserId: string;
  submittedAt: string;
  platformTags: string[];
}

/**
 * Workflow settings response
 */
export interface WorkflowSettingsResponse {
  teamId: string;
  requiresApproval: boolean;
  approverIds: string[];
}

/**
 * Configure workflow request
 */
export interface ConfigureWorkflowRequest {
  requiresApproval: boolean;
  approverIds?: string[];
}

/**
 * Submit for approval request
 */
export interface SubmitForApprovalRequest {
  teamId: string;
}

/**
 * Approve content request
 */
export interface ApproveContentRequest {
  teamId: string;
  feedback?: string;
}

/**
 * Request changes request
 */
export interface RequestChangesRequest {
  teamId: string;
  feedback: string;
}
