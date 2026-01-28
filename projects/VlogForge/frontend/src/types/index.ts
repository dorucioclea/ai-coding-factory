// Types barrel export
export * from './auth';
export * from './api';
export * from './analytics';
export * from './team';
export * from './content';
export * from './calendar';
export * from './profile';
// Re-export integrations types with renamed PlatformType to avoid conflict
export {
  type PlatformConnectionDto,
  type ConnectionStatusResponse,
  type OAuthInitiationResponse,
  type OAuthCallbackParams,
  type DisconnectPlatformRequest,
  type PlatformType as IntegrationPlatformType,
  type ConnectionStatus,
  type PlatformMetadata,
  PLATFORM_METADATA,
} from './integrations';
export * from './tasks';
export * from './approval';
export * from './collaboration';
