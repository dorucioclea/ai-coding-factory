'use client';

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter, useSearchParams } from 'next/navigation';
import { useCallback, useEffect, useMemo } from 'react';

import { integrationService } from '@/lib/integrations';
import { queryKeys } from '@/lib/query-client';
import type {
  OAuthCallbackParams,
  PlatformType,
} from '@/types/integrations';

/**
 * Hook for fetching connection status
 */
export function useConnectionStatus() {
  return useQuery({
    queryKey: queryKeys.integrations.status(),
    queryFn: () => integrationService.getConnectionStatus(),
    staleTime: 1000 * 60 * 2, // 2 minutes
    refetchOnMount: true,
    refetchOnWindowFocus: true,
  });
}

/**
 * Hook for initiating OAuth flow
 */
export function useInitiateOAuth() {
  return useMutation({
    mutationFn: (platform: PlatformType) =>
      integrationService.initiateOAuth(platform),
    onSuccess: (response) => {
      // Redirect to OAuth authorization URL
      window.location.href = response.authorizationUrl;
    },
  });
}

/**
 * Hook for completing OAuth flow
 */
export function useCompleteOAuth() {
  const queryClient = useQueryClient();
  const router = useRouter();

  return useMutation({
    mutationFn: (params: OAuthCallbackParams) =>
      integrationService.completeOAuth(params),
    onSuccess: () => {
      // Invalidate connection status to refetch
      queryClient.invalidateQueries({
        queryKey: queryKeys.integrations.status(),
      });
      // Redirect to integrations page
      router.push('/dashboard/integrations?connected=true');
    },
  });
}

/**
 * Hook for disconnecting a platform
 */
export function useDisconnectPlatform() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (platform: PlatformType) =>
      integrationService.disconnectPlatform(platform),
    onSuccess: () => {
      // Invalidate connection status to refetch
      queryClient.invalidateQueries({
        queryKey: queryKeys.integrations.status(),
      });
    },
  });
}

/**
 * Hook for syncing a platform
 */
export function useSyncPlatform() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (platform: PlatformType) =>
      integrationService.syncPlatform(platform),
    onSuccess: () => {
      // Invalidate connection status to refetch
      queryClient.invalidateQueries({
        queryKey: queryKeys.integrations.status(),
      });
    },
  });
}

/**
 * Hook for handling OAuth callback automatically
 * Call this in the OAuth callback page
 */
export function useOAuthCallback() {
  const searchParams = useSearchParams();
  const completeOAuth = useCompleteOAuth();

  const handleCallback = useCallback(() => {
    const code = searchParams.get('code');
    const state = searchParams.get('state');
    const platform = searchParams.get('platform') as PlatformType | null;

    if (code && state && platform) {
      completeOAuth.mutate({
        code,
        state,
        platform,
      });
    }
  }, [searchParams, completeOAuth]);

  useEffect(() => {
    handleCallback();
  }, [handleCallback]);

  return {
    isCompleting: completeOAuth.isPending,
    error: completeOAuth.error,
  };
}

/**
 * Combined hook for integrations management
 */
export function useIntegrations() {
  const {
    data: statusData,
    isLoading,
    error,
    refetch,
  } = useConnectionStatus();
  const initiateOAuth = useInitiateOAuth();
  const disconnectPlatform = useDisconnectPlatform();
  const syncPlatform = useSyncPlatform();

  const connections = useMemo(
    () => statusData?.connections ?? [],
    [statusData?.connections]
  );
  const availablePlatforms = useMemo(
    () => statusData?.availablePlatforms ?? [],
    [statusData?.availablePlatforms]
  );

  const getConnection = useCallback(
    (platform: PlatformType) => {
      return connections.find((conn) => conn.platformType === platform);
    },
    [connections]
  );

  const isConnected = useCallback(
    (platform: PlatformType) => {
      const connection = getConnection(platform);
      return connection?.status === 'Connected';
    },
    [getConnection]
  );

  const hasError = useCallback(
    (platform: PlatformType) => {
      const connection = getConnection(platform);
      return connection?.status === 'Error';
    },
    [getConnection]
  );

  return {
    // Data
    connections,
    availablePlatforms,
    getConnection,
    isConnected,
    hasError,

    // State
    isLoading,
    error,
    refetch,

    // Actions
    connect: initiateOAuth.mutate,
    disconnect: disconnectPlatform.mutate,
    sync: syncPlatform.mutate,

    // Mutation states
    isConnecting: initiateOAuth.isPending,
    isDisconnecting: disconnectPlatform.isPending,
    isSyncing: syncPlatform.isPending,

    // Mutation errors
    connectError: initiateOAuth.error,
    disconnectError: disconnectPlatform.error,
    syncError: syncPlatform.error,
  };
}

export default useIntegrations;
