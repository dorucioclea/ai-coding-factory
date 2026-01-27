import { apiClient } from './api-client';
import type {
  ContentIdeaResponse,
  ContentIdeasListResponse,
  CreateContentIdeaRequest,
  UpdateContentIdeaRequest,
  UpdateStatusRequest,
  ContentFilters,
  ApiResponse,
} from '@/types';

/**
 * Content service for content ideas operations
 * Matches backend ContentController endpoints
 */
export const contentService = {
  /**
   * Get list of content ideas with filters
   */
  async getContentIdeas(
    filters?: ContentFilters
  ): Promise<ContentIdeasListResponse> {
    const params: Record<string, string | number | boolean | undefined> = {};

    if (filters) {
      if (filters['status'] !== undefined) params['status'] = filters['status'];
      if (filters['platformTag']) params['platformTag'] = filters['platformTag'];
      if (filters['search']) params['search'] = filters['search'];
      if (filters['sortBy']) params['sortBy'] = filters['sortBy'];
      if (filters['sortDirection']) params['sortDirection'] = filters['sortDirection'];
      if (filters['page']) params['page'] = filters['page'];
      if (filters['pageSize']) params['pageSize'] = filters['pageSize'];
    }

    const response = await apiClient.get<
      ApiResponse<ContentIdeasListResponse>
    >('/content', { params });

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to fetch content ideas');
    }

    return response.data;
  },

  /**
   * Get single content idea by ID
   */
  async getContentIdea(id: string): Promise<ContentIdeaResponse> {
    const response = await apiClient.get<ApiResponse<ContentIdeaResponse>>(
      `/content/${id}`
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to fetch content idea');
    }

    return response.data;
  },

  /**
   * Create new content idea
   */
  async createContentIdea(
    data: CreateContentIdeaRequest
  ): Promise<ContentIdeaResponse> {
    const response = await apiClient.post<ApiResponse<ContentIdeaResponse>>(
      '/content',
      data
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to create content idea');
    }

    return response.data;
  },

  /**
   * Update existing content idea
   */
  async updateContentIdea(
    id: string,
    data: UpdateContentIdeaRequest
  ): Promise<ContentIdeaResponse> {
    const response = await apiClient.put<ApiResponse<ContentIdeaResponse>>(
      `/content/${id}`,
      data
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to update content idea');
    }

    return response.data;
  },

  /**
   * Delete content idea
   */
  async deleteContentIdea(id: string): Promise<void> {
    const response = await apiClient.delete<ApiResponse<void>>(
      `/content/${id}`
    );

    if (!response.success) {
      throw new Error(response.error ?? 'Failed to delete content idea');
    }
  },

  /**
   * Update content idea status
   */
  async updateContentIdeaStatus(
    id: string,
    data: UpdateStatusRequest
  ): Promise<ContentIdeaResponse> {
    const response = await apiClient.put<ApiResponse<ContentIdeaResponse>>(
      `/content/${id}/status`,
      data
    );

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to update status');
    }

    return response.data;
  },
};

export default contentService;
