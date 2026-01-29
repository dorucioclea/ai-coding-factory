/**
 * Shared project types for ACF-013
 */

export type SharedProjectStatus = 'Active' | 'Closed';

export type SharedProjectRole = 'Member' | 'Owner';

export type SharedProjectTaskStatus = 'Open' | 'InProgress' | 'Completed';

export type SharedProjectActivityType =
  | 'ProjectCreated'
  | 'MemberJoined'
  | 'MemberLeft'
  | 'TaskAdded'
  | 'TaskUpdated'
  | 'TaskCompleted'
  | 'LinkAdded'
  | 'LinkRemoved'
  | 'ProjectClosed';

export interface SharedProjectMemberDto {
  id: string;
  userId: string;
  role: SharedProjectRole;
  joinedAt: string;
}

export interface SharedProjectTaskDto {
  id: string;
  createdByUserId: string;
  title: string;
  description?: string;
  status: SharedProjectTaskStatus;
  assigneeId?: string;
  dueDate?: string;
  completedAt?: string;
  createdAt: string;
}

export interface SharedProjectLinkDto {
  id: string;
  addedByUserId: string;
  title: string;
  url: string;
  description?: string;
  createdAt: string;
}

export interface SharedProjectActivityDto {
  id: string;
  userId: string;
  activityType: SharedProjectActivityType;
  message: string;
  createdAt: string;
}

export interface SharedProjectDto {
  id: string;
  name: string;
  description?: string;
  status: SharedProjectStatus;
  collaborationRequestId: string;
  ownerId: string;
  memberCount: number;
  taskCount: number;
  linkCount: number;
  createdAt: string;
  closedAt?: string;
  members: SharedProjectMemberDto[];
}

export interface SharedProjectDetailDto {
  id: string;
  name: string;
  description?: string;
  status: SharedProjectStatus;
  collaborationRequestId: string;
  ownerId: string;
  createdAt: string;
  closedAt?: string;
  members: SharedProjectMemberDto[];
  tasks: SharedProjectTaskDto[];
  links: SharedProjectLinkDto[];
}

export interface SharedProjectListResponse {
  items: SharedProjectDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface SharedProjectActivityListResponse {
  items: SharedProjectActivityDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface AddProjectTaskPayload {
  title: string;
  description?: string;
  assigneeId?: string;
  dueDate?: string;
}

export interface UpdateProjectTaskPayload {
  title?: string;
  description?: string;
  status?: SharedProjectTaskStatus;
  assigneeId?: string;
  dueDate?: string;
}

export interface AddProjectLinkPayload {
  title: string;
  url: string;
  description?: string;
}

export const ProjectStatusConfig: Record<
  SharedProjectStatus,
  { label: string; variant: 'default' | 'secondary' | 'success' | 'destructive' | 'outline' }
> = {
  Active: { label: 'Active', variant: 'success' },
  Closed: { label: 'Closed', variant: 'secondary' },
};

export const TaskStatusConfig: Record<
  SharedProjectTaskStatus,
  { label: string; variant: 'default' | 'secondary' | 'success' | 'destructive' | 'outline' }
> = {
  Open: { label: 'Open', variant: 'default' },
  InProgress: { label: 'In Progress', variant: 'secondary' },
  Completed: { label: 'Completed', variant: 'success' },
};
