'use client';

import { useCallback, useState } from 'react';
import { X, Plus } from 'lucide-react';
import { Badge, Button, Input, Label } from '@/components/ui';
import { CommonNicheTags, ProfileConstraints } from '@/types';
import { cn } from '@/lib/utils';

interface NicheTagSelectorProps {
  selectedTags: string[];
  onChange: (tags: string[]) => void;
  maxTags?: number;
  className?: string;
}

/**
 * Component for selecting niche tags (max 5)
 */
export function NicheTagSelector({
  selectedTags,
  onChange,
  maxTags = ProfileConstraints.maxNicheTags,
  className,
}: NicheTagSelectorProps) {
  const [customTag, setCustomTag] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleAddTag = useCallback(
    (tag: string) => {
      setError(null);

      // Validate
      const trimmedTag = tag.trim();
      if (!trimmedTag) {
        setError('Tag cannot be empty');
        return;
      }

      if (selectedTags.length >= maxTags) {
        setError(`Maximum ${maxTags} tags allowed`);
        return;
      }

      if (selectedTags.some((t) => t.toLowerCase() === trimmedTag.toLowerCase())) {
        setError('Tag already added');
        return;
      }

      // Add tag (immutable)
      const newTags = [...selectedTags, trimmedTag];
      onChange(newTags);
      setCustomTag('');
    },
    [selectedTags, maxTags, onChange]
  );

  const handleRemoveTag = useCallback(
    (tagToRemove: string) => {
      // Remove tag (immutable)
      const newTags = selectedTags.filter((tag) => tag !== tagToRemove);
      onChange(newTags);
      setError(null);
    },
    [selectedTags, onChange]
  );

  const handleCustomTagSubmit = useCallback(
    (event: React.FormEvent) => {
      event.preventDefault();
      if (customTag) {
        handleAddTag(customTag);
      }
    },
    [customTag, handleAddTag]
  );

  const availableTags = CommonNicheTags.filter(
    (tag) => !selectedTags.includes(tag)
  );

  const canAddMore = selectedTags.length < maxTags;

  return (
    <div className={cn('space-y-4', className)}>
      <div>
        <Label>
          Niche Tags ({selectedTags.length}/{maxTags})
        </Label>
        <p className="text-xs text-muted-foreground mt-1">
          Select up to {maxTags} tags that describe your content niche
        </p>
      </div>

      {/* Selected Tags */}
      {selectedTags.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {selectedTags.map((tag) => (
            <Badge
              key={tag}
              variant="secondary"
              className="cursor-pointer hover:bg-secondary/80"
            >
              {tag}
              <button
                type="button"
                onClick={() => handleRemoveTag(tag)}
                className="ml-2 rounded-full hover:bg-destructive/20"
                aria-label={`Remove ${tag}`}
              >
                <X className="h-3 w-3" />
              </button>
            </Badge>
          ))}
        </div>
      )}

      {/* Common Tags */}
      {canAddMore && availableTags.length > 0 && (
        <div>
          <Label className="text-xs">Common Tags</Label>
          <div className="mt-2 flex flex-wrap gap-2">
            {availableTags.slice(0, 10).map((tag) => (
              <Badge
                key={tag}
                variant="outline"
                className="cursor-pointer hover:bg-accent"
                onClick={() => handleAddTag(tag)}
              >
                {tag}
                <Plus className="ml-1 h-3 w-3" />
              </Badge>
            ))}
          </div>
        </div>
      )}

      {/* Custom Tag Input */}
      {canAddMore && (
        <form onSubmit={handleCustomTagSubmit} className="flex gap-2">
          <Input
            type="text"
            placeholder="Add custom tag..."
            value={customTag}
            onChange={(e) => setCustomTag(e.target.value)}
            maxLength={50}
          />
          <Button
            type="submit"
            size="sm"
            disabled={!customTag.trim()}
          >
            <Plus className="h-4 w-4" />
          </Button>
        </form>
      )}

      {/* Error Display */}
      {error && (
        <div className="text-sm text-destructive">{error}</div>
      )}
    </div>
  );
}

export default NicheTagSelector;
