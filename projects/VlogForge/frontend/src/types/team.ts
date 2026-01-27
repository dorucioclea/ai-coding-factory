/**
 * Team Management types
 * ACF-015 Phase 5
 * Matches backend TeamDtos.cs
 */

/**
 * Team member role enum
 * Maps to backend TeamRole enum
 */
export enum TeamRole {
  Viewer = 0,
  Editor = 1,
  Admin = 2,
  Owner = 3,
}

/**
 * Team role labels for display
 */
export const TeamRoleLabels: Record<TeamRole, string> = {
  [TeamRole.Viewer]: 'Viewer',
  [TeamRole.Editor]: 'Editor',
  [TeamRole.Admin]: 'Admin',
  [TeamRole.Owner]: 'Owner',
};

/**
 * Team role descriptions
 */
export const TeamRoleDescriptions: Record<TeamRole, string> = {
  [TeamRole.Viewer]: 'Can view team content',
  [TeamRole.Editor]: 'Can view and edit team content',
  [TeamRole.Admin]: 'Can manage team members and content',
  [TeamRole.Owner]: 'Full control over team',
};

/**
 * Team summary response
 */
export interface TeamResponse {
  id: string;
  ownerId: string;
  name: string;
  description?: string;
  memberCount: number;
  createdAt: string;
}

/**
 * Team member response
 */
export interface TeamMemberResponse {
  id: string;
  userId: string;
  role: TeamRole;
  joinedAt: string;
  // Extended fields (may come from user service)
  email?: string;
  firstName?: string;
  lastName?: string;
  displayName?: string;
}

/**
 * Team with members response
 */
export interface TeamWithMembersResponse {
  id: string;
  ownerId: string;
  name: string;
  description?: string;
  createdAt: string;
  members: TeamMemberResponse[];
}

/**
 * Team invitation response
 */
export interface TeamInvitationResponse {
  id: string;
  teamId: string;
  email: string;
  role: TeamRole;
  expiresAt: string;
  isExpired: boolean;
  isAccepted: boolean;
  invitedAt?: string;
  invitedBy?: string;
}

/**
 * Create team request
 */
export interface CreateTeamRequest {
  name: string;
  description?: string;
}

/**
 * Invite member request
 */
export interface InviteMemberRequest {
  email: string;
  role: TeamRole;
}

/**
 * Accept invitation request
 */
export interface AcceptInvitationRequest {
  token: string;
}

/**
 * Change member role request
 */
export interface ChangeMemberRoleRequest {
  role: TeamRole;
}

/**
 * Team member with computed fields
 */
export interface TeamMemberWithUser extends TeamMemberResponse {
  displayName: string;
  initials: string;
  isOwner: boolean;
  canChangeRole: boolean;
  canRemove: boolean;
}

/**
 * Team permissions helper
 */
export interface TeamPermissions {
  canInvite: boolean;
  canRemoveMembers: boolean;
  canChangeRoles: boolean;
  canEditTeam: boolean;
  canDeleteTeam: boolean;
}

/**
 * Compute team permissions based on user's role
 */
export function getTeamPermissions(
  userRole: TeamRole,
  isOwner: boolean
): TeamPermissions {
  return {
    canInvite: userRole >= TeamRole.Admin,
    canRemoveMembers: userRole >= TeamRole.Admin,
    canChangeRoles: userRole >= TeamRole.Admin,
    canEditTeam: userRole >= TeamRole.Admin,
    canDeleteTeam: isOwner,
  };
}

/**
 * Check if user can modify target member
 */
export function canModifyMember(
  userRole: TeamRole,
  targetRole: TeamRole,
  isOwner: boolean,
  targetIsOwner: boolean
): boolean {
  // Cannot modify owner
  if (targetIsOwner) return false;

  // Owner can modify anyone
  if (isOwner) return true;

  // Admin can modify Editor and Viewer
  if (userRole === TeamRole.Admin) {
    return targetRole < TeamRole.Admin;
  }

  return false;
}

/**
 * Get available roles for assignment
 */
export function getAssignableRoles(userRole: TeamRole, isOwner: boolean): TeamRole[] {
  if (isOwner) {
    // Owner can assign any role except Owner
    return [TeamRole.Viewer, TeamRole.Editor, TeamRole.Admin];
  }

  if (userRole === TeamRole.Admin) {
    // Admin can assign Viewer and Editor
    return [TeamRole.Viewer, TeamRole.Editor];
  }

  return [];
}
