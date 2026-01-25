import * as SecureStore from 'expo-secure-store';

const STORAGE_KEYS = {
  ACCESS_TOKEN: 'auth_access_token',
  REFRESH_TOKEN: 'auth_refresh_token',
  TOKEN_EXPIRY: 'auth_token_expiry',
} as const;

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

export const authStorage = {
  // Access Token
  async getAccessToken(): Promise<string | null> {
    return SecureStore.getItemAsync(STORAGE_KEYS.ACCESS_TOKEN);
  },

  async setAccessToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(STORAGE_KEYS.ACCESS_TOKEN, token);
  },

  // Refresh Token
  async getRefreshToken(): Promise<string | null> {
    return SecureStore.getItemAsync(STORAGE_KEYS.REFRESH_TOKEN);
  },

  async setRefreshToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(STORAGE_KEYS.REFRESH_TOKEN, token);
  },

  // Token Expiry
  async getTokenExpiry(): Promise<number | null> {
    const expiry = await SecureStore.getItemAsync(STORAGE_KEYS.TOKEN_EXPIRY);
    return expiry ? parseInt(expiry, 10) : null;
  },

  async setTokenExpiry(expiry: number): Promise<void> {
    await SecureStore.setItemAsync(STORAGE_KEYS.TOKEN_EXPIRY, expiry.toString());
  },

  // Set all tokens
  async setTokens(tokens: AuthTokens): Promise<void> {
    await Promise.all([
      this.setAccessToken(tokens.accessToken),
      this.setRefreshToken(tokens.refreshToken),
      this.setTokenExpiry(tokens.expiresAt),
    ]);
  },

  // Get all tokens
  async getTokens(): Promise<AuthTokens | null> {
    const [accessToken, refreshToken, expiresAt] = await Promise.all([
      this.getAccessToken(),
      this.getRefreshToken(),
      this.getTokenExpiry(),
    ]);

    if (!accessToken || !refreshToken || !expiresAt) {
      return null;
    }

    return { accessToken, refreshToken, expiresAt };
  },

  // Check if token is expired
  async isTokenExpired(): Promise<boolean> {
    const expiry = await this.getTokenExpiry();
    if (!expiry) return true;

    // Consider expired 5 minutes before actual expiry
    const bufferMs = 5 * 60 * 1000;
    return Date.now() >= expiry - bufferMs;
  },

  // Clear all auth data
  async clearAll(): Promise<void> {
    await Promise.all([
      SecureStore.deleteItemAsync(STORAGE_KEYS.ACCESS_TOKEN),
      SecureStore.deleteItemAsync(STORAGE_KEYS.REFRESH_TOKEN),
      SecureStore.deleteItemAsync(STORAGE_KEYS.TOKEN_EXPIRY),
    ]);
  },
};
