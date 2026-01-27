'use client';

import { AlertCircle, Loader2 } from 'lucide-react';
import { useState } from 'react';

import {
  ContentFilters,
  ContentIdeaCard,
  ContentSearch,
  CreateIdeaModal,
} from '@/components/content';
import { Button, Skeleton } from '@/components/ui';
import { useContentIdeas, useDeleteIdea, useUpdateIdea } from '@/hooks';
import type {
  ContentFilters as ContentFiltersType,
  ContentIdeaResponse,
  UpdateContentIdeaRequest,
} from '@/types';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui';
import { EditIdeaForm } from '@/components/content';

export default function ContentPage() {
  const [filters, setFilters] = useState<ContentFiltersType>({
    sortBy: 'createdAt',
    sortDirection: 'desc',
    page: 1,
    pageSize: 20,
  });

  const [editingIdea, setEditingIdea] = useState<ContentIdeaResponse | null>(
    null
  );

  const [deletingId, setDeletingId] = useState<string | null>(null);

  const { data, isLoading, error, refetch } = useContentIdeas(filters);
  const updateMutation = useUpdateIdea();
  const deleteMutation = useDeleteIdea();

  const handleSearch = (search: string) => {
    setFilters({
      ...filters,
      search: search || undefined,
      page: 1,
    });
  };

  const handleEdit = (idea: ContentIdeaResponse) => {
    setEditingIdea(idea);
  };

  const handleUpdate = async (updateData: UpdateContentIdeaRequest) => {
    if (!editingIdea) return;

    try {
      await updateMutation.mutateAsync({
        id: editingIdea.id,
        data: updateData,
      });
      setEditingIdea(null);
    } catch (error) {
      console.error('Failed to update content idea:', error);
      throw error;
    }
  };

  const handleDeleteConfirm = (id: string) => {
    setDeletingId(id);
  };

  const handleDelete = async () => {
    if (!deletingId) return;

    try {
      await deleteMutation.mutateAsync(deletingId);
      setDeletingId(null);
    } catch (error) {
      console.error('Failed to delete content idea:', error);
    }
  };

  return (
    <div className="flex h-full flex-col">
      {/* Header */}
      <div className="border-b bg-background px-6 py-4">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Content Ideas</h1>
            <p className="text-sm text-muted-foreground">
              Manage and track your content ideas from concept to publication
            </p>
          </div>
          <CreateIdeaModal onSuccess={() => refetch()} />
        </div>
      </div>

      {/* Filters and Search */}
      <div className="border-b bg-muted/20 px-6 py-4">
        <div className="space-y-4">
          <ContentSearch
            value={filters.search ?? ''}
            onChange={handleSearch}
          />
          <ContentFilters filters={filters} onChange={setFilters} />
        </div>
      </div>

      {/* Content List */}
      <div className="flex-1 overflow-y-auto px-6 py-6">
        {isLoading && (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-64 w-full" />
            ))}
          </div>
        )}

        {error && (
          <div className="flex flex-col items-center justify-center py-12">
            <AlertCircle className="mb-4 h-12 w-12 text-destructive" />
            <h3 className="mb-2 text-lg font-semibold">
              Failed to load content ideas
            </h3>
            <p className="mb-4 text-sm text-muted-foreground">
              {error instanceof Error ? error.message : 'An error occurred'}
            </p>
            <Button onClick={() => refetch()}>Try Again</Button>
          </div>
        )}

        {!isLoading && !error && data && (
          <>
            {data.items.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12">
                <div className="mb-4 rounded-full bg-muted p-3">
                  <FileText className="h-8 w-8 text-muted-foreground" />
                </div>
                <h3 className="mb-2 text-lg font-semibold">
                  No content ideas yet
                </h3>
                <p className="mb-4 text-sm text-muted-foreground">
                  {filters.search || filters.status !== undefined || filters.platformTag
                    ? 'No ideas match your filters. Try adjusting them.'
                    : 'Start by creating your first content idea.'}
                </p>
                {!filters.search && filters.status === undefined && !filters.platformTag && (
                  <CreateIdeaModal onSuccess={() => refetch()} />
                )}
              </div>
            ) : (
              <>
                <div className="mb-4 text-sm text-muted-foreground">
                  Showing {data.items.length} of {data.totalCount} ideas
                </div>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                  {data.items.map((idea) => (
                    <ContentIdeaCard
                      key={idea.id}
                      idea={idea}
                      onEdit={handleEdit}
                      onDelete={handleDeleteConfirm}
                    />
                  ))}
                </div>
              </>
            )}
          </>
        )}
      </div>

      {/* Edit Dialog */}
      <Dialog open={!!editingIdea} onOpenChange={() => setEditingIdea(null)}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Edit Content Idea</DialogTitle>
            <DialogDescription>
              Update your content idea details and platform targets.
            </DialogDescription>
          </DialogHeader>
          {editingIdea && (
            <EditIdeaForm
              idea={editingIdea}
              onSubmit={handleUpdate}
              onCancel={() => setEditingIdea(null)}
              isSubmitting={updateMutation.isPending}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={!!deletingId} onOpenChange={() => setDeletingId(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Content Idea</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete this content idea? This action
              cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <div className="flex justify-end gap-3">
            <Button
              variant="outline"
              onClick={() => setDeletingId(null)}
              disabled={deleteMutation.isPending}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={deleteMutation.isPending}
            >
              {deleteMutation.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Deleting...
                </>
              ) : (
                'Delete'
              )}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}

// Import FileText for empty state
import { FileText } from 'lucide-react';
