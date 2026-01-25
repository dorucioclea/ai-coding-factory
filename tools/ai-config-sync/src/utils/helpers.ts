/**
 * Utility helper functions
 */

import { randomBytes } from "node:crypto";

/**
 * Generate a unique ID for sync jobs
 */
export function generateJobId(): string {
  const timestamp = Date.now().toString(36);
  const random = randomBytes(4).toString("hex");
  return `job-${timestamp}-${random}`;
}

/**
 * Format a date for display
 */
export function formatDate(date: Date): string {
  return date.toISOString().replace("T", " ").slice(0, 19);
}

/**
 * Format bytes for display
 */
export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/**
 * Pluralize a word
 */
export function pluralize(count: number, singular: string, plural?: string): string {
  const p = plural ?? `${singular}s`;
  return count === 1 ? singular : p;
}

/**
 * Truncate string for display
 */
export function truncate(str: string, maxLength: number): string {
  if (str.length <= maxLength) return str;
  return `${str.slice(0, maxLength - 3)}...`;
}

/**
 * Pad string to fixed width
 */
export function padRight(str: string, width: number): string {
  if (str.length >= width) return str.slice(0, width);
  return str + " ".repeat(width - str.length);
}

/**
 * Create a simple progress bar
 */
export function progressBar(current: number, total: number, width = 20): string {
  const percentage = Math.round((current / total) * 100);
  const filled = Math.round((current / total) * width);
  const empty = width - filled;
  return `[${"=".repeat(filled)}${" ".repeat(empty)}] ${percentage}%`;
}
