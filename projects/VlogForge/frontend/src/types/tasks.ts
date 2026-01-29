/**
 * Task Assignment types for VlogForge
 * Matches backend TaskDtos.cs
 * Stories: ACF-008, ACF-014
 */

/**
 * Assignment status enum
 * Maps to backend AssignmentStatus
 */
export enum AssignmentStatus {
  NotStarted = 0,
  InProgress = 1,
  Completed = 2,
}

/**
 * Task history action enum
 * Maps to backend TaskHistoryAction
 * Story: ACF-014
 */
export enum TaskHistoryAction {
  Created = 0,
  StatusChanged = 1,
  Reassigned = 2,
  DueDateChanged = 3,
  CommentAdded = 4,
}

/**
 * Task comment response
 */
export interface TaskCommentResponse {
  id: string;
  authorId: string;
  content: string;
  parentCommentId?: string;
  isEdited: boolean;
  editedAt?: string;
  createdAt: string;
}

/**
 * Task history response
 * Story: ACF-014
 */
export interface TaskHistoryResponse {
  id: string;
  changedByUserId: string;
  action: TaskHistoryAction;
  description: string;
  createdAt: string;
}

/**
 * Task assignment response
 */
export interface TaskAssignmentResponse {
  id: string;
  contentItemId: string;
  teamId: string;
  assigneeId: string;
  assignedById: string;
  dueDate: string;
  status: AssignmentStatus;
  notes?: string;
  completedAt?: string;
  createdAt: string;
  isOverdue: boolean;
  comments: TaskCommentResponse[];
  history: TaskHistoryResponse[];
}

/**
 * Paginated task list response
 */
export interface TaskListResponse {
  items: TaskAssignmentResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Assign task request
 */
export interface AssignTaskRequest {
  contentItemId: string;
  assigneeId: string;
  dueDate: string;
  notes?: string;
}

/**
 * Update task status request
 */
export interface UpdateTaskStatusRequest {
  status: AssignmentStatus;
}

/**
 * Add comment request
 */
export interface AddCommentRequest {
  content: string;
  parentCommentId?: string;
}

/**
 * Task filters for queries
 */
export interface TaskFilters {
  page?: number;
  pageSize?: number;
  status?: AssignmentStatus;
  isOverdue?: boolean;
  sortBy?: 'dueDate' | 'createdAt' | 'status';
  sortDirection?: 'asc' | 'desc';
}

/**
 * Grouped tasks by status for ACF-014 AC2
 */
export interface GroupedTasks {
  [AssignmentStatus.NotStarted]: TaskAssignmentResponse[];
  [AssignmentStatus.InProgress]: TaskAssignmentResponse[];
  [AssignmentStatus.Completed]: TaskAssignmentResponse[];
}

/**
 * Status display helpers
 */
export const AssignmentStatusLabels: Record<AssignmentStatus, string> = {
  [AssignmentStatus.NotStarted]: 'Not Started',
  [AssignmentStatus.InProgress]: 'In Progress',
  [AssignmentStatus.Completed]: 'Completed',
};

export const AssignmentStatusColors: Record<
  AssignmentStatus,
  'default' | 'secondary' | 'success'
> = {
  [AssignmentStatus.NotStarted]: 'default',
  [AssignmentStatus.InProgress]: 'secondary',
  [AssignmentStatus.Completed]: 'success',
};

/**
 * Task history action display labels
 * Story: ACF-014
 */
export const TaskHistoryActionLabels: Record<TaskHistoryAction, string> = {
  [TaskHistoryAction.Created]: 'Created',
  [TaskHistoryAction.StatusChanged]: 'Status Changed',
  [TaskHistoryAction.Reassigned]: 'Reassigned',
  [TaskHistoryAction.DueDateChanged]: 'Due Date Changed',
  [TaskHistoryAction.CommentAdded]: 'Comment Added',
};
