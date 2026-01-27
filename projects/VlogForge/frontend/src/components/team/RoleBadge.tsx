'use client';

import { Badge } from '@/components/ui';
import { TeamRole, TeamRoleLabels } from '@/types';

interface RoleBadgeProps {
  role: TeamRole;
  className?: string;
}

/**
 * Role badge with color coding
 * ACF-015 Phase 5
 *
 * Owner = purple
 * Admin = blue
 * Editor = green
 * Viewer = gray
 */
export function RoleBadge({ role, className }: RoleBadgeProps) {
  const variants = {
    [TeamRole.Owner]: 'bg-purple-500 text-white hover:bg-purple-600',
    [TeamRole.Admin]: 'bg-blue-500 text-white hover:bg-blue-600',
    [TeamRole.Editor]: 'bg-green-500 text-white hover:bg-green-600',
    [TeamRole.Viewer]: 'bg-gray-500 text-white hover:bg-gray-600',
  };

  return (
    <Badge
      className={`${variants[role]} ${className ?? ''}`}
    >
      {TeamRoleLabels[role]}
    </Badge>
  );
}

export default RoleBadge;
