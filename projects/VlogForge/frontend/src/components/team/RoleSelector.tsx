'use client';

import { TeamRole, TeamRoleLabels, TeamRoleDescriptions } from '@/types';
import { Label } from '@/components/ui';

interface RoleSelectorProps {
  value: TeamRole;
  onChange: (role: TeamRole) => void;
  availableRoles?: TeamRole[];
  disabled?: boolean;
  className?: string;
}

/**
 * Role dropdown selector
 * ACF-015 Phase 5
 */
export function RoleSelector({
  value,
  onChange,
  availableRoles = [TeamRole.Viewer, TeamRole.Editor, TeamRole.Admin],
  disabled = false,
  className,
}: RoleSelectorProps) {
  return (
    <div className={className}>
      <Label htmlFor="role-select">Role</Label>
      <select
        id="role-select"
        value={value}
        onChange={(e) => onChange(parseInt(e.target.value, 10) as TeamRole)}
        disabled={disabled}
        className="mt-1 block w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {availableRoles.map((role) => (
          <option key={role} value={role}>
            {TeamRoleLabels[role]} - {TeamRoleDescriptions[role]}
          </option>
        ))}
      </select>
    </div>
  );
}

export default RoleSelector;
