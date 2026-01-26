/**
 * API types matching the backend Clean Architecture template
 * Follows RFC 7807 Problem Details for error responses
 */

/**
 * Standard API response wrapper
 * Matches backend ApiResponse<T> pattern
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  errors?: Record<string, string[]>;
  meta?: PaginationMeta;
}

/**
 * Pagination metadata
 */
export interface PaginationMeta {
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Paginated list response
 */
export interface PaginatedList<T> {
  items: T[];
  meta: PaginationMeta;
}

/**
 * RFC 7807 Problem Details for HTTP APIs
 * Matches backend exception handling middleware
 */
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}

/**
 * Sort direction
 */
export type SortDirection = 'asc' | 'desc';

/**
 * Generic query parameters for list endpoints
 */
export interface QueryParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: SortDirection;
  search?: string;
  filters?: Record<string, string | number | boolean>;
}

/**
 * Health check response
 * Matches backend /health endpoint
 */
export interface HealthCheckResponse {
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  totalDuration: string;
  entries: Record<
    string,
    {
      status: string;
      duration: string;
      description?: string;
      data?: Record<string, unknown>;
    }
  >;
}

/**
 * API error class for typed error handling
 */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly title: string,
    public readonly detail?: string,
    public readonly errors?: Record<string, string[]>
  ) {
    super(detail ?? title);
    this.name = 'ApiError';
  }

  static fromProblemDetails(problem: ProblemDetails): ApiError {
    return new ApiError(
      problem.status,
      problem.title,
      problem.detail,
      problem.errors
    );
  }

  /**
   * Check if error is a validation error (400)
   */
  isValidationError(): boolean {
    return this.status === 400;
  }

  /**
   * Check if error is unauthorized (401)
   */
  isUnauthorized(): boolean {
    return this.status === 401;
  }

  /**
   * Check if error is forbidden (403)
   */
  isForbidden(): boolean {
    return this.status === 403;
  }

  /**
   * Check if error is not found (404)
   */
  isNotFound(): boolean {
    return this.status === 404;
  }

  /**
   * Get field-level errors for form validation
   */
  getFieldErrors(): Record<string, string> {
    if (!this.errors) return {};
    const fieldErrors: Record<string, string> = {};
    for (const [field, messages] of Object.entries(this.errors)) {
      fieldErrors[field] = messages[0] ?? '';
    }
    return fieldErrors;
  }
}
