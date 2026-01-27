'use client';

import { useState } from 'react';
import { Send } from 'lucide-react';
import { Button, Textarea } from '@/components/ui';

interface CommentInputProps {
  onSubmit: (content: string) => Promise<void>;
  placeholder?: string;
  parentCommentId?: string;
  isSubmitting?: boolean;
  className?: string;
}

/**
 * Input form for adding comments to tasks
 * ACF-015 Phase 6
 */
export function CommentInput({
  onSubmit,
  placeholder = 'Add a comment...',
  isSubmitting = false,
  className,
}: CommentInputProps) {
  const [content, setContent] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!content.trim()) {
      setError('Comment cannot be empty');
      return;
    }

    try {
      setError(null);
      await onSubmit(content.trim());
      setContent('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add comment');
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setContent(e.target.value);
    if (error) setError(null);
  };

  return (
    <form onSubmit={handleSubmit} className={className}>
      <div className="space-y-2">
        <Textarea
          value={content}
          onChange={handleChange}
          placeholder={placeholder}
          rows={3}
          disabled={isSubmitting}
          className={error ? 'border-destructive' : ''}
        />
        {error && (
          <p className="text-sm text-destructive">{error}</p>
        )}
        <div className="flex justify-end">
          <Button
            type="submit"
            size="sm"
            disabled={!content.trim() || isSubmitting}
          >
            <Send className="mr-2 h-4 w-4" />
            {isSubmitting ? 'Posting...' : 'Post Comment'}
          </Button>
        </div>
      </div>
    </form>
  );
}

export default CommentInput;
