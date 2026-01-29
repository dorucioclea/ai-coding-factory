import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api-client';
import { queryKeys } from '@/lib/query-client';
import type {
  SharedProjectDetailDto,
  SharedProjectListResponse,
  SharedProjectActivityListResponse,
  SharedProjectTaskDto,
  SharedProjectLinkDto,
  SharedProjectStatus,
  AddProjectTaskPayload,
  UpdateProjectTaskPayload,
  AddProjectLinkPayload,
} from '@/types/shared-project';

/**
 * Hook to get user's shared projects
 * Story: ACF-013
 */
export function useSharedProjects(
  status?: SharedProjectStatus,
  page = 1,
  pageSize = 20,
  enabled = true
) {
  return useQuery({
    queryKey: queryKeys.sharedProjects.list({ status, page, pageSize }),
    queryFn: () =>
      apiClient.get<SharedProjectListResponse>('/projects', {
        params: { status, page, pageSize },
      }),
    staleTime: 1000 * 60 * 2,
    enabled,
  });
}

/**
 * Hook to get a shared project by ID
 * Story: ACF-013
 */
export function useSharedProject(projectId: string, enabled = true) {
  return useQuery({
    queryKey: queryKeys.sharedProjects.detail(projectId),
    queryFn: () =>
      apiClient.get<SharedProjectDetailDto>(
        `/projects/${encodeURIComponent(projectId)}`
      ),
    staleTime: 1000 * 60 * 2,
    enabled: enabled && !!projectId,
  });
}

/**
 * Hook to get project activity feed
 * Story: ACF-013
 */
export function useProjectActivity(
  projectId: string,
  page = 1,
  pageSize = 50,
  enabled = true
) {
  return useQuery({
    queryKey: queryKeys.sharedProjects.activity(projectId, { page, pageSize }),
    queryFn: () =>
      apiClient.get<SharedProjectActivityListResponse>(
        `/projects/${encodeURIComponent(projectId)}/activity`,
        { params: { page, pageSize } }
      ),
    staleTime: 1000 * 60,
    enabled: enabled && !!projectId,
  });
}

/**
 * Hook to add a task to a shared project
 * Story: ACF-013
 */
export function useAddProjectTask(projectId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: AddProjectTaskPayload) =>
      apiClient.post<SharedProjectTaskDto>(
        `/projects/${encodeURIComponent(projectId)}/tasks`,
        payload
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.sharedProjects.detail(projectId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.sharedProjects.all,
      });
    },
  });
}

/**
 * Hook to update a task in a shared project
 * Story: ACF-013
 */
export function useUpdateProjectTask(projectId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      taskId,
      ...payload
    }: UpdateProjectTaskPayload & { taskId: string }) =>
      apiClient.put<SharedProjectDetailDto>(
        `/projects/${encodeURIComponent(projectId)}/tasks/${encodeURIComponent(taskId)}`,
        payload
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.sharedProjects.detail(projectId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.sharedProjects.all,
      });
    },
  });
}

/**
 * Hook to add a link to a shared project
 * Story: ACF-013
 */
export function useAddProjectLink(projectId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: AddProjectLinkPayload) =>
      apiClient.post<SharedProjectLinkDto>(
        `/projects/${encodeURIComponent(projectId)}/links`,
        payload
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.sharedProjects.detail(projectId),
      });
      queryClient.invalidateQueries({
        queryKey: queryKeys.sharedProjects.all,
      });
    },
  });
}

/**
 * Hook to leave a shared project
 * Story: ACF-013
 */
export function useLeaveProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (projectId: string) =>
      apiClient.post(`/projects/${encodeURIComponent(projectId)}/leave`),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.sharedProjects.all,
      });
    },
  });
}

/**
 * Hook to close a shared project
 * Story: ACF-013
 */
export function useCloseProject() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (projectId: string) =>
      apiClient.post(`/projects/${encodeURIComponent(projectId)}/close`),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.sharedProjects.all,
      });
    },
  });
}
