'use client';

import { Calendar, Edit2, MoreVertical, Trash2 } from 'lucide-react';
import { useState } from 'react';

import { ApprovalActions } from '@/components/approval';
import {
  Button,
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui';
import { useAuth } from '@/hooks';
import { useUpdateIdeaStatus } from '@/hooks';
import { cn } from '@/lib/utils';
import type { ContentIdeaResponse, IdeaStatus } from '@/types';

import { StatusBadge } from './StatusBadge';
import { StatusDropdown } from './StatusDropdown';

interface ContentIdeaCardProps {
  idea: ContentIdeaResponse;
  onEdit?: (idea: ContentIdeaResponse) => void;
  onDelete?: (id: string) => void;
  className?: string;
  /** Team ID for approval workflow */
  teamId?: string;
  /** Whether current user can approve content */
  canApprove?: boolean;
  /** Callback when approval action completes */
  onApprovalComplete?: () => void;
}

export function ContentIdeaCard({
  idea,
  onEdit,
  onDelete,
  className,
  teamId,
  canApprove = false,
  onApprovalComplete,
}: ContentIdeaCardProps) {
  const { user } = useAuth();
  const [isStatusChanging, setIsStatusChanging] = useState(false);
  const updateStatusMutation = useUpdateIdeaStatus();

  const isOwner = user?.id === idea.userId;

  const handleStatusChange = async (newStatus: IdeaStatus) => {
    try {
      setIsStatusChanging(true);
      await updateStatusMutation.mutateAsync({
        id: idea.id,
        status: { status: newStatus },
      });
    } catch {
      // Error is handled by React Query - mutation.error will be set
    } finally {
      setIsStatusChanging(false);
    }
  };

  const formattedDate = idea.scheduledDate
    ? new Date(idea.scheduledDate).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      })
    : null;

  const formattedCreatedAt = new Date(idea.createdAt).toLocaleDateString(
    'en-US',
    {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    }
  );

  return (
    <Card className={cn('hover:shadow-md transition-shadow', className)}>
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between gap-2">
          <div className="flex-1 min-w-0">
            <CardTitle className="text-lg truncate">{idea.title}</CardTitle>
            <CardDescription className="mt-1.5 text-xs">
              Created {formattedCreatedAt}
            </CardDescription>
          </div>
          <div className="flex items-center gap-2">
            <StatusBadge status={idea.status} />
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                  <MoreVertical className="h-4 w-4" />
                  <span className="sr-only">Open menu</span>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem
                  onClick={() => onEdit?.(idea)}
                  className="cursor-pointer"
                >
                  <Edit2 className="mr-2 h-4 w-4" />
                  Edit
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  onClick={() => onDelete?.(idea.id)}
                  className="cursor-pointer text-destructive focus:text-destructive"
                >
                  <Trash2 className="mr-2 h-4 w-4" />
                  Delete
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </CardHeader>

      <CardContent className="pb-3">
        {idea.notes && (
          <p className="text-sm text-muted-foreground line-clamp-3 mb-3">
            {idea.notes}
          </p>
        )}

        {idea.platformTags.length > 0 && (
          <div className="flex flex-wrap gap-1.5">
            {idea.platformTags.map((tag) => (
              <span
                key={tag}
                className="inline-flex items-center rounded-md bg-secondary px-2 py-1 text-xs font-medium text-secondary-foreground"
              >
                {tag}
              </span>
            ))}
          </div>
        )}

        {formattedDate && (
          <div className="flex items-center gap-2 mt-3 text-sm text-muted-foreground">
            <Calendar className="h-4 w-4" />
            <span>Scheduled for {formattedDate}</span>
          </div>
        )}
      </CardContent>

      <CardFooter className="pt-3 border-t flex flex-col gap-3">
        <div className="w-full">
          <StatusDropdown
            currentStatus={idea.status}
            onChange={handleStatusChange}
            disabled={isStatusChanging}
          />
        </div>
        {teamId && (
          <ApprovalActions
            content={idea}
            teamId={teamId}
            canApprove={canApprove}
            isOwner={isOwner}
            onActionComplete={onApprovalComplete}
          />
        )}
      </CardFooter>
    </Card>
  );
}

export default ContentIdeaCard;
