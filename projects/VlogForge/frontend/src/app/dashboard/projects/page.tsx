'use client';

import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { FolderOpen } from 'lucide-react';
import { Button, Skeleton } from '@/components/ui';
import { ProjectCard } from '@/components/shared-projects';
import { useSharedProjects } from '@/hooks/use-shared-projects';
import type { SharedProjectStatus } from '@/types/shared-project';

const STATUS_OPTIONS: { label: string; value: SharedProjectStatus | undefined }[] = [
  { label: 'All', value: undefined },
  { label: 'Active', value: 'Active' },
  { label: 'Closed', value: 'Closed' },
];

/**
 * Shared projects list page
 * Story: ACF-013
 */
export default function SharedProjectsPage() {
  const router = useRouter();
  const [statusFilter, setStatusFilter] = useState<SharedProjectStatus | undefined>(undefined);
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = useSharedProjects(statusFilter, page);

  const projects = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const hasNextPage = data?.hasNextPage ?? false;
  const hasPreviousPage = data?.hasPreviousPage ?? false;

  const handleProjectClick = useCallback(
    (projectId: string) => {
      router.push(`/dashboard/projects/${projectId}`);
    },
    [router]
  );

  const handleStatusChange = useCallback((status: SharedProjectStatus | undefined) => {
    setStatusFilter(status);
    setPage(1);
  }, []);

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex items-center gap-2">
        <FolderOpen className="h-6 w-6" />
        <h1 className="text-2xl font-bold">Shared Projects</h1>
      </div>

      <div className="flex gap-1.5 flex-wrap">
        {STATUS_OPTIONS.map((option) => (
          <Button
            key={option.label}
            variant={statusFilter === option.value ? 'default' : 'outline'}
            size="sm"
            onClick={() => handleStatusChange(option.value)}
          >
            {option.label}
          </Button>
        ))}
      </div>

      {!isLoading && (
        <p className="text-sm text-muted-foreground">
          {totalCount === 0
            ? 'No shared projects'
            : `${totalCount} project${totalCount === 1 ? '' : 's'}`}
        </p>
      )}

      {error && (
        <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive">
          <h3 className="font-semibold">Error loading projects</h3>
          <p className="text-sm mt-1">
            {error instanceof Error ? error.message : 'An unexpected error occurred'}
          </p>
        </div>
      )}

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-28" />
          ))}
        </div>
      ) : projects.length > 0 ? (
        <>
          <div className="space-y-3">
            {projects.map((project) => (
              <ProjectCard
                key={project.id}
                project={project}
                onClick={handleProjectClick}
              />
            ))}
          </div>

          {(hasNextPage || hasPreviousPage) && (
            <div className="flex justify-center gap-2 pt-4">
              <Button
                variant="outline"
                size="sm"
                disabled={!hasPreviousPage}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Previous
              </Button>
              <span className="flex items-center text-sm text-muted-foreground px-2">
                Page {page}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={!hasNextPage}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </>
      ) : (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <div className="rounded-full bg-muted p-6 mb-4">
            <FolderOpen className="h-12 w-12 text-muted-foreground" />
          </div>
          <h2 className="text-xl font-semibold mb-2">No shared projects yet</h2>
          <p className="text-muted-foreground max-w-md">
            When you accept a collaboration request, a shared project space will be created automatically.
          </p>
        </div>
      )}
    </div>
  );
}
