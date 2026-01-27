import { apiClient } from './api-client';
import type { ApiResponse } from '@/types';
import type {
  ConnectionStatusResponse,
  OAuthInitiationResponse,
  OAuthCallbackParams,
  PlatformType,
} from '@/types/integrations';

/**
 * Integration service for platform connections
 * Matches backend IntegrationsController endpoints
 */
export const integrationService = {
  /**
   * Get connection status for all platforms
   */
  async getConnectionStatus(): Promise<ConnectionStatusResponse> {
    const response = await apiClient.get<
      ApiResponse<ConnectionStatusResponse>
    >('/integrations/status');

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to fetch connection status');
    }

    return response.data;
  },

  /**
   * Initiate OAuth flow for a platform
   */
  async initiateOAuth(platform: PlatformType): Promise<OAuthInitiationResponse> {
    const response = await apiClient.post<
      ApiResponse<OAuthInitiationResponse>
    >(`/integrations/${platform}/connect`);

    if (!response.success || !response.data) {
      throw new Error(response.error ?? 'Failed to initiate OAuth');
    }

    return response.data;
  },

  /**
   * Complete OAuth flow with callback parameters
   */
  async completeOAuth(params: OAuthCallbackParams): Promise<void> {
    const response = await apiClient.post<ApiResponse<void>>(
      `/integrations/${params.platform}/callback`,
      {
        code: params.code,
        state: params.state,
      }
    );

    if (!response.success) {
      throw new Error(response.error ?? 'Failed to complete OAuth');
    }
  },

  /**
   * Disconnect a platform
   */
  async disconnectPlatform(platform: PlatformType): Promise<void> {
    const response = await apiClient.delete<ApiResponse<void>>(
      `/integrations/${platform}`
    );

    if (!response.success) {
      throw new Error(response.error ?? 'Failed to disconnect platform');
    }
  },

  /**
   * Manually trigger sync for a platform
   */
  async syncPlatform(platform: PlatformType): Promise<void> {
    const response = await apiClient.post<ApiResponse<void>>(
      `/integrations/${platform}/sync`
    );

    if (!response.success) {
      throw new Error(response.error ?? 'Failed to sync platform');
    }
  },
};

export default integrationService;
