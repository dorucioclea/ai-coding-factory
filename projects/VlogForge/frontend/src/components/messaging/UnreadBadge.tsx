/**
 * Unread message count badge
 * Story: ACF-012
 */

interface UnreadBadgeProps {
  count: number;
  className?: string;
}

export function UnreadBadge({ count, className = '' }: UnreadBadgeProps) {
  if (count === 0) {
    return null;
  }

  const displayCount = count > 99 ? '99+' : count;

  return (
    <span
      aria-label={`${displayCount} unread messages`}
      className={`inline-flex items-center justify-center rounded-full bg-primary text-primary-foreground text-xs px-1.5 py-0.5 min-w-[1.25rem] font-medium ${className}`}
    >
      {displayCount}
    </span>
  );
}
