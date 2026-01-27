/**
 * Draggable content item for calendar (ACF-015 Phase 4)
 * Uses react-dnd for drag and drop functionality
 */

'use client';

import { useDrag } from 'react-dnd';
import { Card } from '@/components/ui';
import { cn } from '@/lib/utils';
import type { ContentIdeaResponse, DraggedItem } from '@/types';
import { STATUS_CONFIG } from '@/types';

interface DraggableContentItemProps {
  item: ContentIdeaResponse;
  sourceDate?: string;
}

const STATUS_COLORS = {
  gray: 'bg-gray-100 border-gray-300 dark:bg-gray-800 dark:border-gray-600',
  yellow:
    'bg-yellow-100 border-yellow-300 dark:bg-yellow-900 dark:border-yellow-600',
  orange:
    'bg-orange-100 border-orange-300 dark:bg-orange-900 dark:border-orange-600',
  blue: 'bg-blue-100 border-blue-300 dark:bg-blue-900 dark:border-blue-600',
  green: 'bg-green-100 border-green-300 dark:bg-green-900 dark:border-green-600',
} as const;

export function DraggableContentItem({
  item,
  sourceDate,
}: DraggableContentItemProps) {
  const statusConfig = STATUS_CONFIG[item.status];

  const [{ isDragging }, drag] = useDrag<
    DraggedItem,
    unknown,
    { isDragging: boolean }
  >({
    type: 'CONTENT_ITEM',
    item: {
      contentId: item.id,
      sourceDate,
    },
    collect: (monitor) => ({
      isDragging: monitor.isDragging(),
    }),
  });

  return (
    <Card
      ref={drag}
      className={cn(
        'cursor-move border-l-4 p-2 text-xs transition-opacity hover:shadow-md',
        STATUS_COLORS[statusConfig.color],
        isDragging && 'opacity-50'
      )}
    >
      <div className="line-clamp-2 font-medium">{item.title}</div>
      {item.platformTags.length > 0 && (
        <div className="mt-1 flex flex-wrap gap-1">
          {item.platformTags.slice(0, 2).map((tag) => (
            <span
              key={tag}
              className="rounded bg-white/50 px-1 py-0.5 text-[10px] dark:bg-black/20"
            >
              {tag}
            </span>
          ))}
          {item.platformTags.length > 2 && (
            <span className="rounded bg-white/50 px-1 py-0.5 text-[10px] dark:bg-black/20">
              +{item.platformTags.length - 2}
            </span>
          )}
        </div>
      )}
    </Card>
  );
}
