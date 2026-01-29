'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { ArrowLeft, FolderOpen, LogOut, XCircle } from 'lucide-react';
import { Button, Skeleton, Badge } from '@/components/ui';
import {
  ProjectTaskList,
  ProjectLinkList,
  ProjectActivityFeed,
} from '@/components/shared-projects';
import {
  useSharedProject,
  useProjectActivity,
  useAddProjectTask,
  useUpdateProjectTask,
  useAddProjectLink,
  useLeaveProject,
  useCloseProject,
} from '@/hooks/use-shared-projects';
import { ProjectStatusConfig } from '@/types/shared-project';
import type { SharedProjectTaskStatus } from '@/types/shared-project';

type Tab = 'tasks' | 'links' | 'activity';

/**
 * Shared project detail page
 * Story: ACF-013
 */
export default function SharedProjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<Tab>('tasks');
  const [showLeaveConfirm, setShowLeaveConfirm] = useState(false);
  const [showCloseConfirm, setShowCloseConfirm] = useState(false);

  const { data: project, isLoading, error } = useSharedProject(id);
  const activityQuery = useProjectActivity(id, 1, 50, activeTab === 'activity');
  const addTaskMutation = useAddProjectTask(id);
  const updateTaskMutation = useUpdateProjectTask(id);
  const addLinkMutation = useAddProjectLink(id);
  const leaveMutation = useLeaveProject();
  const closeMutation = useCloseProject();

  const isActive = project?.status === 'Active';

  const handleAddTask = (title: string, description?: string) => {
    addTaskMutation.mutate({ title, description });
  };

  const handleUpdateTaskStatus = (taskId: string, status: SharedProjectTaskStatus) => {
    updateTaskMutation.mutate({ taskId, status });
  };

  const handleAddLink = (title: string, url: string, description?: string) => {
    addLinkMutation.mutate({ title, url, description });
  };

  const handleLeave = () => {
    leaveMutation.mutate(id, {
      onSuccess: () => router.push('/dashboard/projects'),
    });
  };

  const handleClose = () => {
    closeMutation.mutate(id, {
      onSuccess: () => router.push('/dashboard/projects'),
    });
  };

  if (isLoading) {
    return (
      <div className="container mx-auto py-6 space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-96" />
        <Skeleton className="h-64" />
      </div>
    );
  }

  if (error || !project) {
    return (
      <div className="container mx-auto py-6">
        <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive">
          <h3 className="font-semibold">Error loading project</h3>
          <p className="text-sm mt-1">
            {error instanceof Error ? error.message : 'Project not found'}
          </p>
        </div>
      </div>
    );
  }

  const statusConfig = ProjectStatusConfig[project.status];

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="sm" onClick={() => router.push('/dashboard/projects')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <FolderOpen className="h-6 w-6" />
        <h1 className="text-2xl font-bold">{project.name}</h1>
        <Badge variant={statusConfig.variant}>
          {statusConfig.label}
        </Badge>
      </div>

      {project.description && (
        <p className="text-muted-foreground">{project.description}</p>
      )}

      {/* Members */}
      <div className="text-sm text-muted-foreground">
        {project.members.length} member{project.members.length !== 1 ? 's' : ''}
      </div>

      {/* Actions */}
      {isActive && (
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowLeaveConfirm(true)}
          >
            <LogOut className="h-4 w-4 mr-1" />
            Leave
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowCloseConfirm(true)}
          >
            <XCircle className="h-4 w-4 mr-1" />
            Close Project
          </Button>
        </div>
      )}

      {/* Leave Confirmation */}
      {showLeaveConfirm && (
        <div className="border rounded-lg p-4 bg-destructive/5">
          <p className="text-sm mb-3">Are you sure you want to leave this project?</p>
          <div className="flex gap-2">
            <Button
              variant="destructive"
              size="sm"
              onClick={handleLeave}
              disabled={leaveMutation.isPending}
            >
              {leaveMutation.isPending ? 'Leaving...' : 'Confirm Leave'}
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowLeaveConfirm(false)}
            >
              Cancel
            </Button>
          </div>
        </div>
      )}

      {/* Close Confirmation */}
      {showCloseConfirm && (
        <div className="border rounded-lg p-4 bg-destructive/5">
          <p className="text-sm mb-3">
            Are you sure you want to close this project? This will revoke access for all members.
          </p>
          <div className="flex gap-2">
            <Button
              variant="destructive"
              size="sm"
              onClick={handleClose}
              disabled={closeMutation.isPending}
            >
              {closeMutation.isPending ? 'Closing...' : 'Confirm Close'}
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowCloseConfirm(false)}
            >
              Cancel
            </Button>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="flex gap-2 border-b pb-2">
        {(['tasks', 'links', 'activity'] as Tab[]).map((tab) => (
          <Button
            key={tab}
            variant={activeTab === tab ? 'default' : 'outline'}
            size="sm"
            onClick={() => setActiveTab(tab)}
          >
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </Button>
        ))}
      </div>

      {/* Tab Content */}
      {activeTab === 'tasks' && (
        <ProjectTaskList
          tasks={project.tasks}
          isActive={isActive}
          onAddTask={handleAddTask}
          onUpdateTaskStatus={handleUpdateTaskStatus}
          isAdding={addTaskMutation.isPending}
          isUpdating={updateTaskMutation.isPending}
        />
      )}

      {activeTab === 'links' && (
        <ProjectLinkList
          links={project.links}
          isActive={isActive}
          onAddLink={handleAddLink}
          isAdding={addLinkMutation.isPending}
        />
      )}

      {activeTab === 'activity' && (
        <ProjectActivityFeed
          activities={activityQuery.data?.items ?? []}
          isLoading={activityQuery.isLoading}
        />
      )}
    </div>
  );
}
