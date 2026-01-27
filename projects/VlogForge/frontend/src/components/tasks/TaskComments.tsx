'use client';

import { formatDistanceToNow } from 'date-fns';
import { MessageSquare } from 'lucide-react';
import { Avatar, AvatarFallback, Separator } from '@/components/ui';
import { CommentInput } from './CommentInput';
import type { TaskCommentResponse } from '@/types';

interface TaskCommentsProps {
  comments: TaskCommentResponse[];
  onAddComment: (content: string) => Promise<void>;
  isAddingComment?: boolean;
  className?: string;
}

/**
 * Threaded comments display for tasks
 * ACF-015 Phase 6
 */
export function TaskComments({
  comments,
  onAddComment,
  isAddingComment = false,
  className,
}: TaskCommentsProps) {
  const topLevelComments = comments.filter((c) => !c.parentCommentId);

  const getInitials = (authorId: string) => {
    return authorId.substring(0, 2).toUpperCase();
  };

  const formatDate = (dateString: string) => {
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch {
      return 'recently';
    }
  };

  return (
    <div className={className}>
      <div className="mb-4 flex items-center gap-2">
        <MessageSquare className="h-5 w-5 text-muted-foreground" />
        <h3 className="font-semibold text-lg">
          Comments ({comments.length})
        </h3>
      </div>

      <CommentInput
        onSubmit={onAddComment}
        isSubmitting={isAddingComment}
        className="mb-6"
      />

      <div className="space-y-4">
        {topLevelComments.length === 0 ? (
          <p className="text-sm text-muted-foreground text-center py-8">
            No comments yet. Be the first to comment!
          </p>
        ) : (
          topLevelComments.map((comment) => (
            <div key={comment.id} className="space-y-2">
              <div className="flex gap-3">
                <Avatar className="h-8 w-8">
                  <AvatarFallback className="text-xs">
                    {getInitials(comment.authorId)}
                  </AvatarFallback>
                </Avatar>
                <div className="flex-1 space-y-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium text-sm">
                      User {comment.authorId.substring(0, 8)}
                    </span>
                    <span className="text-xs text-muted-foreground">
                      {formatDate(comment.createdAt)}
                    </span>
                    {comment.isEdited && (
                      <span className="text-xs text-muted-foreground italic">
                        (edited)
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-foreground whitespace-pre-wrap">
                    {comment.content}
                  </p>
                </div>
              </div>
              <Separator className="my-2" />
            </div>
          ))
        )}
      </div>
    </div>
  );
}

export default TaskComments;
