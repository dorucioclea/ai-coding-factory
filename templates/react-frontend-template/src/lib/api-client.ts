import { ApiError, type ProblemDetails, type ApiResponse } from '@/types';

/**
 * Environment-based API configuration
 */
const API_BASE_URL =
  process.env['NEXT_PUBLIC_API_URL'] ?? 'http://localhost:5000/api';

/**
 * HTTP methods supported by the API client
 */
type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

/**
 * Request configuration options
 */
interface RequestConfig {
  headers?: Record<string, string>;
  params?: Record<string, string | number | boolean | undefined>;
  signal?: AbortSignal;
  cache?: RequestCache;
  next?: NextFetchRequestConfig;
}

interface NextFetchRequestConfig {
  revalidate?: number | false;
  tags?: string[];
}

/**
 * Token storage utilities
 * Uses cookies for SSR compatibility
 */
const TOKEN_STORAGE_KEY = 'auth-token';
const REFRESH_TOKEN_KEY = 'refresh-token';

export const tokenStorage = {
  getAccessToken(): string | null {
    if (typeof window === 'undefined') return null;
    return document.cookie
      .split('; ')
      .find((row) => row.startsWith(`${TOKEN_STORAGE_KEY}=`))
      ?.split('=')[1] ?? null;
  },

  setAccessToken(token: string, expiresIn: number): void {
    if (typeof window === 'undefined') return;
    const expires = new Date(Date.now() + expiresIn * 1000);
    document.cookie = `${TOKEN_STORAGE_KEY}=${token}; expires=${expires.toUTCString()}; path=/; SameSite=Strict; Secure`;
  },

  getRefreshToken(): string | null {
    if (typeof window === 'undefined') return null;
    return document.cookie
      .split('; ')
      .find((row) => row.startsWith(`${REFRESH_TOKEN_KEY}=`))
      ?.split('=')[1] ?? null;
  },

  setRefreshToken(token: string): void {
    if (typeof window === 'undefined') return;
    // Refresh tokens have longer expiry - 7 days
    const expires = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000);
    document.cookie = `${REFRESH_TOKEN_KEY}=${token}; expires=${expires.toUTCString()}; path=/; SameSite=Strict; Secure`;
  },

  clearTokens(): void {
    if (typeof window === 'undefined') return;
    document.cookie = `${TOKEN_STORAGE_KEY}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
    document.cookie = `${REFRESH_TOKEN_KEY}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
  },
};

/**
 * Build URL with query parameters
 */
function buildUrl(
  endpoint: string,
  params?: Record<string, string | number | boolean | undefined>
): string {
  const url = new URL(endpoint, API_BASE_URL);

  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined) {
        url.searchParams.append(key, String(value));
      }
    });
  }

  return url.toString();
}

/**
 * Parse error response to ApiError
 */
async function parseErrorResponse(response: Response): Promise<ApiError> {
  try {
    const contentType = response.headers.get('content-type');

    if (contentType?.includes('application/json')) {
      const problem = (await response.json()) as ProblemDetails;
      return ApiError.fromProblemDetails(problem);
    }

    const text = await response.text();
    return new ApiError(response.status, response.statusText, text);
  } catch {
    return new ApiError(
      response.status,
      response.statusText,
      'An unexpected error occurred'
    );
  }
}

/**
 * Core fetch wrapper with auth handling
 */
async function fetchWithAuth<T>(
  endpoint: string,
  method: HttpMethod,
  config?: RequestConfig,
  body?: unknown
): Promise<T> {
  const url = buildUrl(endpoint, config?.params);

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    Accept: 'application/json',
    ...config?.headers,
  };

  // Add auth token if available
  const token = tokenStorage.getAccessToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
    signal: config?.signal,
    cache: config?.cache,
    credentials: 'include',
    ...(config?.next && { next: config.next }),
  });

  // Handle 204 No Content
  if (response.status === 204) {
    return undefined as T;
  }

  // Handle error responses
  if (!response.ok) {
    const error = await parseErrorResponse(response);

    // Handle token expiration - attempt refresh
    if (error.isUnauthorized() && tokenStorage.getRefreshToken()) {
      const refreshed = await attemptTokenRefresh();
      if (refreshed) {
        // Retry the original request
        return fetchWithAuth<T>(endpoint, method, config, body);
      }
    }

    throw error;
  }

  // Parse JSON response
  const data = (await response.json()) as T;
  return data;
}

/**
 * Attempt to refresh the access token
 */
async function attemptTokenRefresh(): Promise<boolean> {
  const refreshToken = tokenStorage.getRefreshToken();
  if (!refreshToken) return false;

  try {
    const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) {
      tokenStorage.clearTokens();
      return false;
    }

    const data = (await response.json()) as ApiResponse<{
      accessToken: string;
      refreshToken: string;
      expiresIn: number;
    }>;

    if (data.success && data.data) {
      tokenStorage.setAccessToken(data.data.accessToken, data.data.expiresIn);
      tokenStorage.setRefreshToken(data.data.refreshToken);
      return true;
    }

    tokenStorage.clearTokens();
    return false;
  } catch {
    tokenStorage.clearTokens();
    return false;
  }
}

/**
 * API client with typed methods
 */
export const apiClient = {
  /**
   * GET request
   */
  get<T>(endpoint: string, config?: RequestConfig): Promise<T> {
    return fetchWithAuth<T>(endpoint, 'GET', config);
  },

  /**
   * POST request
   */
  post<T>(endpoint: string, body?: unknown, config?: RequestConfig): Promise<T> {
    return fetchWithAuth<T>(endpoint, 'POST', config, body);
  },

  /**
   * PUT request
   */
  put<T>(endpoint: string, body?: unknown, config?: RequestConfig): Promise<T> {
    return fetchWithAuth<T>(endpoint, 'PUT', config, body);
  },

  /**
   * PATCH request
   */
  patch<T>(endpoint: string, body?: unknown, config?: RequestConfig): Promise<T> {
    return fetchWithAuth<T>(endpoint, 'PATCH', config, body);
  },

  /**
   * DELETE request
   */
  delete<T>(endpoint: string, config?: RequestConfig): Promise<T> {
    return fetchWithAuth<T>(endpoint, 'DELETE', config);
  },
};

export default apiClient;
