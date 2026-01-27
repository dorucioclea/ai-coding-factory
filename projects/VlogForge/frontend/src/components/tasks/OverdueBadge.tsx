'use client';

import { AlertCircle } from 'lucide-react';
import { Badge } from '@/components/ui';

interface OverdueBadgeProps {
  isOverdue: boolean;
  className?: string;
}

/**
 * Visual indicator for overdue tasks
 * ACF-015 Phase 6
 */
export function OverdueBadge({ isOverdue, className }: OverdueBadgeProps) {
  if (!isOverdue) return null;

  return (
    <Badge variant="destructive" className={className}>
      <AlertCircle className="mr-1 h-3 w-3" />
      Overdue
    </Badge>
  );
}

export default OverdueBadge;
