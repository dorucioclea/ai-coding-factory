import { z } from 'zod';

import { TeamRole } from '@/types';

/**
 * Create team form schema
 * Matches backend CreateTeamRequest validation
 */
export const createTeamSchema = z.object({
  name: z
    .string()
    .min(1, 'Team name is required')
    .max(100, 'Team name must be 100 characters or less')
    .trim()
    .regex(
      /^[a-zA-Z0-9\s\-_]+$/,
      'Team name can only contain letters, numbers, spaces, hyphens, and underscores'
    ),
  description: z
    .string()
    .max(500, 'Description must be 500 characters or less')
    .trim()
    .optional(),
});

export type CreateTeamFormData = z.infer<typeof createTeamSchema>;

/**
 * Invite member form schema
 * Matches backend InviteMemberRequest validation
 */
export const inviteMemberSchema = z.object({
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Please enter a valid email address')
    .max(255, 'Email must be less than 255 characters')
    .transform((email) => email.toLowerCase().trim()),
  role: z
    .nativeEnum(TeamRole, {
      errorMap: () => ({ message: 'Please select a valid role' }),
    })
    .refine((role) => role !== TeamRole.Owner, {
      message: 'Cannot invite someone as Owner',
    }),
});

export type InviteMemberFormData = z.infer<typeof inviteMemberSchema>;

/**
 * Change member role form schema
 * Matches backend ChangeMemberRoleRequest validation
 */
export const changeMemberRoleSchema = z.object({
  role: z
    .nativeEnum(TeamRole, {
      errorMap: () => ({ message: 'Please select a valid role' }),
    })
    .refine((role) => role !== TeamRole.Owner, {
      message: 'Cannot change role to Owner',
    }),
});

export type ChangeMemberRoleFormData = z.infer<typeof changeMemberRoleSchema>;

/**
 * Update team form schema
 * For editing team details
 */
export const updateTeamSchema = z.object({
  name: z
    .string()
    .min(1, 'Team name is required')
    .max(100, 'Team name must be 100 characters or less')
    .trim()
    .regex(
      /^[a-zA-Z0-9\s\-_]+$/,
      'Team name can only contain letters, numbers, spaces, hyphens, and underscores'
    )
    .optional(),
  description: z
    .string()
    .max(500, 'Description must be 500 characters or less')
    .trim()
    .optional(),
});

export type UpdateTeamFormData = z.infer<typeof updateTeamSchema>;
