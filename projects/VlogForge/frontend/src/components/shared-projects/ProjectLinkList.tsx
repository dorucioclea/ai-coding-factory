'use client';

import { useState } from 'react';
import { Plus, ExternalLink } from 'lucide-react';
import { Button } from '@/components/ui';
import type { SharedProjectLinkDto } from '@/types/shared-project';

function isSafeUrl(url: string): boolean {
  try {
    const parsed = new URL(url);
    return ['http:', 'https:'].includes(parsed.protocol);
  } catch {
    return false;
  }
}

interface ProjectLinkListProps {
  links: SharedProjectLinkDto[];
  isActive: boolean;
  onAddLink: (title: string, url: string, description?: string) => void;
  isAdding?: boolean;
}

/**
 * Link list component for shared projects
 * Story: ACF-013
 */
export function ProjectLinkList({
  links,
  isActive,
  onAddLink,
  isAdding = false,
}: ProjectLinkListProps) {
  const [showAddForm, setShowAddForm] = useState(false);
  const [newTitle, setNewTitle] = useState('');
  const [newUrl, setNewUrl] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const [urlError, setUrlError] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTitle.trim() || !newUrl.trim()) return;
    if (!isSafeUrl(newUrl.trim())) {
      setUrlError('Please enter a valid HTTP or HTTPS URL.');
      return;
    }
    setUrlError('');
    onAddLink(newTitle.trim(), newUrl.trim(), newDescription.trim() || undefined);
    setNewTitle('');
    setNewUrl('');
    setNewDescription('');
    setShowAddForm(false);
  };

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">Links & Resources ({links.length})</h3>
        {isActive && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowAddForm(!showAddForm)}
          >
            <Plus className="h-4 w-4 mr-1" />
            Add Link
          </Button>
        )}
      </div>

      {showAddForm && (
        <form onSubmit={handleSubmit} className="border rounded-lg p-3 space-y-2">
          <input
            type="text"
            placeholder="Link title"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            className="w-full px-3 py-1.5 border rounded text-sm"
            maxLength={200}
            required
          />
          <input
            type="url"
            placeholder="https://..."
            value={newUrl}
            onChange={(e) => { setNewUrl(e.target.value); setUrlError(''); }}
            className="w-full px-3 py-1.5 border rounded text-sm"
            required
          />
          {urlError && (
            <p className="text-xs text-destructive">{urlError}</p>
          )}
          <textarea
            placeholder="Description (optional)"
            value={newDescription}
            onChange={(e) => setNewDescription(e.target.value)}
            className="w-full px-3 py-1.5 border rounded text-sm resize-none"
            rows={2}
            maxLength={500}
          />
          <div className="flex gap-2">
            <Button
              type="submit"
              size="sm"
              disabled={isAdding || !newTitle.trim() || !newUrl.trim()}
            >
              {isAdding ? 'Adding...' : 'Add Link'}
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => setShowAddForm(false)}
            >
              Cancel
            </Button>
          </div>
        </form>
      )}

      {links.length === 0 ? (
        <p className="text-sm text-muted-foreground py-4 text-center">
          No links yet. Share resources with your collaborators.
        </p>
      ) : (
        <div className="space-y-2">
          {links.map((link) => (
            <a
              key={link.id}
              href={isSafeUrl(link.url) ? link.url : '#'}
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-start gap-3 p-3 border rounded-lg hover:bg-accent/50 transition-colors"
            >
              <ExternalLink className="h-4 w-4 mt-0.5 text-muted-foreground flex-shrink-0" />
              <div className="flex-1 min-w-0">
                <span className="text-sm font-medium text-primary hover:underline">
                  {link.title}
                </span>
                {link.description && (
                  <p className="text-xs text-muted-foreground mt-0.5">
                    {link.description}
                  </p>
                )}
                <p className="text-xs text-muted-foreground mt-0.5 truncate">
                  {link.url}
                </p>
              </div>
            </a>
          ))}
        </div>
      )}
    </div>
  );
}
