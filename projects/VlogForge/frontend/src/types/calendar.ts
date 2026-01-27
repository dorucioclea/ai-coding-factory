/**
 * Calendar types for content scheduling
 * Related to ACF-015 Phase 4
 */

import type { ContentIdeaResponse } from './content';

/**
 * Calendar day with associated content items
 */
export interface CalendarDay {
  date: string; // ISO date format (YYYY-MM-DD)
  items: ContentIdeaResponse[];
}

/**
 * Calendar month view
 */
export interface CalendarMonth {
  year: number;
  month: number; // 1-12
  days: CalendarDay[];
}

/**
 * Update schedule request
 */
export interface UpdateScheduleRequest {
  scheduledDate: string; // ISO date format
}

/**
 * Calendar view mode
 */
export type CalendarViewMode = 'month' | 'week';

/**
 * Calendar navigation
 */
export interface CalendarNavigation {
  year: number;
  month: number;
  view: CalendarViewMode;
}

/**
 * Drag and drop types
 */
export interface DraggedItem {
  contentId: string;
  sourceDate?: string;
}

/**
 * Drop result
 */
export interface DropResult {
  contentId: string;
  targetDate: string;
}
