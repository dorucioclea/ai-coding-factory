/**
 * Unit tests for use-approval hook
 * Story: ACF-009 - Approval Workflows
 */

import { renderHook, waitFor } from '@testing-library/react';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { createWrapper } from '../utils/test-utils';
import {
  usePendingApprovals,
  useApprovalHistory,
  useConfigureWorkflow,
  useSubmitForApproval,
  useApproveContent,
  useRequestChanges,
  useApproval,
} from '@/hooks/use-approval';

// Mock the approval service
vi.mock('@/lib/approval', () => ({
  approvalService: {
    getPendingApprovals: vi.fn(),
    getApprovalHistory: vi.fn(),
    configureWorkflow: vi.fn(),
    submitForApproval: vi.fn(),
    approveContent: vi.fn(),
    requestChanges: vi.fn(),
  },
}));

import { approvalService } from '@/lib/approval';

const mockPendingApprovals = {
  items: [
    {
      contentItemId: 'content-1',
      title: 'Video Idea 1',
      notes: 'Notes for video 1',
      submittedByUserId: 'user-1',
      submittedAt: '2024-01-15T10:00:00Z',
      platformTags: ['youtube', 'tiktok'],
    },
    {
      contentItemId: 'content-2',
      title: 'Video Idea 2',
      notes: undefined,
      submittedByUserId: 'user-2',
      submittedAt: '2024-01-16T10:00:00Z',
      platformTags: ['instagram'],
    },
  ],
  totalCount: 2,
};

const mockApprovalHistory = {
  contentItemId: 'content-1',
  records: [
    {
      id: 'record-1',
      contentItemId: 'content-1',
      teamId: 'team-1',
      actorId: 'user-1',
      action: 0, // Submitted
      feedback: undefined,
      previousStatus: 1, // Draft
      newStatus: 2, // InReview
      createdAt: '2024-01-15T10:00:00Z',
    },
    {
      id: 'record-2',
      contentItemId: 'content-1',
      teamId: 'team-1',
      actorId: 'user-2',
      action: 1, // Approved
      feedback: 'Looks great!',
      previousStatus: 2, // InReview
      newStatus: 5, // Approved
      createdAt: '2024-01-16T10:00:00Z',
    },
  ],
};

const mockWorkflowSettings = {
  teamId: 'team-1',
  requiresApproval: true,
  approverIds: ['user-1', 'user-2'],
};

const mockContentResponse = {
  id: 'content-1',
  userId: 'user-1',
  title: 'Video Idea 1',
  notes: 'Notes',
  status: 5, // Approved
  platformTags: ['youtube'],
  createdAt: '2024-01-15T10:00:00Z',
  updatedAt: '2024-01-16T10:00:00Z',
};

describe('usePendingApprovals', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch pending approvals for a team', async () => {
    vi.mocked(approvalService.getPendingApprovals).mockResolvedValue(mockPendingApprovals);

    const { result } = renderHook(() => usePendingApprovals('team-1'), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockPendingApprovals);
    expect(approvalService.getPendingApprovals).toHaveBeenCalledWith('team-1');
  });

  it('should not fetch when teamId is empty', async () => {
    const { result } = renderHook(() => usePendingApprovals(''), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(false);
    expect(approvalService.getPendingApprovals).not.toHaveBeenCalled();
  });

  it('should handle error state', async () => {
    vi.mocked(approvalService.getPendingApprovals).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => usePendingApprovals('team-1'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});

describe('useApprovalHistory', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch approval history for content', async () => {
    vi.mocked(approvalService.getApprovalHistory).mockResolvedValue(mockApprovalHistory);

    const { result } = renderHook(() => useApprovalHistory('content-1'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data).toEqual(mockApprovalHistory);
    expect(approvalService.getApprovalHistory).toHaveBeenCalledWith('content-1');
  });

  it('should not fetch when contentId is empty', async () => {
    const { result } = renderHook(() => useApprovalHistory(''), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(false);
    expect(approvalService.getApprovalHistory).not.toHaveBeenCalled();
  });
});

describe('useConfigureWorkflow', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should configure workflow successfully', async () => {
    vi.mocked(approvalService.configureWorkflow).mockResolvedValue(mockWorkflowSettings);

    const { result } = renderHook(() => useConfigureWorkflow(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      teamId: 'team-1',
      data: { requiresApproval: true, approverIds: ['user-1'] },
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(approvalService.configureWorkflow).toHaveBeenCalledWith('team-1', {
      requiresApproval: true,
      approverIds: ['user-1'],
    });
  });
});

describe('useSubmitForApproval', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should submit content for approval successfully', async () => {
    vi.mocked(approvalService.submitForApproval).mockResolvedValue(mockContentResponse);

    const { result } = renderHook(() => useSubmitForApproval(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      contentId: 'content-1',
      data: { teamId: 'team-1' },
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(approvalService.submitForApproval).toHaveBeenCalledWith('content-1', {
      teamId: 'team-1',
    });
  });
});

describe('useApproveContent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should approve content successfully', async () => {
    vi.mocked(approvalService.approveContent).mockResolvedValue(mockContentResponse);

    const { result } = renderHook(() => useApproveContent(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      contentId: 'content-1',
      data: { teamId: 'team-1', feedback: 'Looks great!' },
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(approvalService.approveContent).toHaveBeenCalledWith('content-1', {
      teamId: 'team-1',
      feedback: 'Looks great!',
    });
  });

  it('should approve content without feedback', async () => {
    vi.mocked(approvalService.approveContent).mockResolvedValue(mockContentResponse);

    const { result } = renderHook(() => useApproveContent(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      contentId: 'content-1',
      data: { teamId: 'team-1' },
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
  });
});

describe('useRequestChanges', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should request changes successfully', async () => {
    const updatedContent = { ...mockContentResponse, status: 6 }; // ChangesRequested
    vi.mocked(approvalService.requestChanges).mockResolvedValue(updatedContent);

    const { result } = renderHook(() => useRequestChanges(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      contentId: 'content-1',
      data: { teamId: 'team-1', feedback: 'Please improve intro' },
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(approvalService.requestChanges).toHaveBeenCalledWith('content-1', {
      teamId: 'team-1',
      feedback: 'Please improve intro',
    });
  });
});

describe('useApproval', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should provide all approval actions', async () => {
    const { result } = renderHook(() => useApproval(), {
      wrapper: createWrapper(),
    });

    expect(result.current.submitForApproval).toBeDefined();
    expect(result.current.approveContent).toBeDefined();
    expect(result.current.requestChanges).toBeDefined();
    expect(result.current.configureWorkflow).toBeDefined();
  });

  it('should have pending states initially false', async () => {
    const { result } = renderHook(() => useApproval(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isSubmitting).toBe(false);
    expect(result.current.isApproving).toBe(false);
    expect(result.current.isRequestingChanges).toBe(false);
    expect(result.current.isConfiguringWorkflow).toBe(false);
  });

  it('should submit for approval using combined hook', async () => {
    vi.mocked(approvalService.submitForApproval).mockResolvedValue(mockContentResponse);

    const { result } = renderHook(() => useApproval(), {
      wrapper: createWrapper(),
    });

    await result.current.submitForApproval('content-1', { teamId: 'team-1' });

    expect(approvalService.submitForApproval).toHaveBeenCalledWith('content-1', {
      teamId: 'team-1',
    });
  });

  it('should approve content using combined hook', async () => {
    vi.mocked(approvalService.approveContent).mockResolvedValue(mockContentResponse);

    const { result } = renderHook(() => useApproval(), {
      wrapper: createWrapper(),
    });

    await result.current.approveContent('content-1', { teamId: 'team-1' });

    expect(approvalService.approveContent).toHaveBeenCalledWith('content-1', {
      teamId: 'team-1',
    });
  });

  it('should request changes using combined hook', async () => {
    const updatedContent = { ...mockContentResponse, status: 6 };
    vi.mocked(approvalService.requestChanges).mockResolvedValue(updatedContent);

    const { result } = renderHook(() => useApproval(), {
      wrapper: createWrapper(),
    });

    await result.current.requestChanges('content-1', {
      teamId: 'team-1',
      feedback: 'Needs work',
    });

    expect(approvalService.requestChanges).toHaveBeenCalledWith('content-1', {
      teamId: 'team-1',
      feedback: 'Needs work',
    });
  });
});
