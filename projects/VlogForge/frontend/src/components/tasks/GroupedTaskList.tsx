'use client';

import { useState } from 'react';
import { Loader2, ChevronDown, ChevronRight } from 'lucide-react';
import { Badge } from '@/components/ui';
import { useTask } from '@/hooks/use-tasks';
import { TaskCard } from './TaskCard';
import { TaskDetailModal } from './TaskDetailModal';
import {
  AssignmentStatus,
  AssignmentStatusLabels,
  AssignmentStatusColors,
  type TaskAssignmentResponse,
  type GroupedTasks,
} from '@/types';

interface GroupedTaskListProps {
  groupedTasks: GroupedTasks;
  isLoading?: boolean;
  onStatusChange: (taskId: string, status: AssignmentStatus) => void;
  onAddComment: (taskId: string, content: string) => Promise<void>;
  isUpdatingStatus?: boolean;
  isAddingComment?: boolean;
  className?: string;
}

const STATUS_ORDER: AssignmentStatus[] = [
  AssignmentStatus.NotStarted,
  AssignmentStatus.InProgress,
  AssignmentStatus.Completed,
];

/**
 * Task list grouped by status with collapsible sections.
 * ACF-014 AC2: Tasks grouped by status (Not Started, In Progress, Completed)
 * ACF-014 AC3: Within each group, tasks sorted by due date (earliest first)
 */
export function GroupedTaskList({
  groupedTasks,
  isLoading = false,
  onStatusChange,
  onAddComment,
  isUpdatingStatus = false,
  isAddingComment = false,
  className,
}: GroupedTaskListProps) {
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);
  const [collapsedGroups, setCollapsedGroups] = useState<Set<AssignmentStatus>>(
    new Set()
  );

  const allTasks = [
    ...groupedTasks[AssignmentStatus.NotStarted],
    ...groupedTasks[AssignmentStatus.InProgress],
    ...groupedTasks[AssignmentStatus.Completed],
  ];

  // Fetch full task detail (with comments & history) from server when selected
  // ACF-014 AC5: Task detail modal needs comments and history
  const { data: fetchedTask } = useTask(selectedTaskId ?? '', true);
  const selectedTask = fetchedTask ?? allTasks.find((t) => t.id === selectedTaskId) ?? null;

  const toggleGroup = (status: AssignmentStatus) => {
    setCollapsedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(status)) {
        next.delete(status);
      } else {
        next.add(status);
      }
      return next;
    });
  };

  const handleTaskClick = (taskId: string) => {
    setSelectedTaskId(taskId);
  };

  const handleCloseModal = () => {
    setSelectedTaskId(null);
  };

  const handleModalStatusChange = (status: AssignmentStatus) => {
    if (selectedTaskId) {
      onStatusChange(selectedTaskId, status);
    }
  };

  const handleModalAddComment = async (content: string) => {
    if (selectedTaskId) {
      await onAddComment(selectedTaskId, content);
    }
  };

  const totalCount = allTasks.length;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (totalCount === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">
          No tasks assigned to you yet.
        </p>
      </div>
    );
  }

  return (
    <div className={className}>
      <div className="space-y-8">
        {STATUS_ORDER.map((status) => {
          const tasks = groupedTasks[status];
          const isCollapsed = collapsedGroups.has(status);

          return (
            <StatusGroup
              key={status}
              status={status}
              tasks={tasks}
              isCollapsed={isCollapsed}
              onToggle={() => toggleGroup(status)}
              onStatusChange={onStatusChange}
              onTaskClick={handleTaskClick}
              isUpdatingStatus={isUpdatingStatus}
              selectedTaskId={selectedTaskId}
            />
          );
        })}
      </div>

      <TaskDetailModal
        task={selectedTask}
        isOpen={!!selectedTaskId}
        onClose={handleCloseModal}
        onStatusChange={handleModalStatusChange}
        onAddComment={handleModalAddComment}
        isUpdatingStatus={isUpdatingStatus}
        isAddingComment={isAddingComment}
      />
    </div>
  );
}

interface StatusGroupProps {
  status: AssignmentStatus;
  tasks: TaskAssignmentResponse[];
  isCollapsed: boolean;
  onToggle: () => void;
  onStatusChange: (taskId: string, status: AssignmentStatus) => void;
  onTaskClick: (taskId: string) => void;
  isUpdatingStatus: boolean;
  selectedTaskId: string | null;
}

function StatusGroup({
  status,
  tasks,
  isCollapsed,
  onToggle,
  onStatusChange,
  onTaskClick,
  isUpdatingStatus,
  selectedTaskId,
}: StatusGroupProps) {
  return (
    <section>
      <button
        type="button"
        onClick={onToggle}
        className="flex items-center gap-3 mb-4 w-full text-left group"
      >
        {isCollapsed ? (
          <ChevronRight className="h-5 w-5 text-muted-foreground group-hover:text-foreground transition-colors" />
        ) : (
          <ChevronDown className="h-5 w-5 text-muted-foreground group-hover:text-foreground transition-colors" />
        )}
        <h2 className="text-lg font-semibold">
          {AssignmentStatusLabels[status]}
        </h2>
        <Badge variant={AssignmentStatusColors[status]}>
          {tasks.length}
        </Badge>
      </button>

      {!isCollapsed && (
        <>
          {tasks.length === 0 ? (
            <p className="text-sm text-muted-foreground ml-8 py-4">
              No tasks with this status.
            </p>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 ml-8">
              {tasks.map((task) => (
                <TaskCard
                  key={task.id}
                  task={task}
                  onStatusChange={onStatusChange}
                  onClick={onTaskClick}
                  isUpdating={isUpdatingStatus && selectedTaskId === task.id}
                />
              ))}
            </div>
          )}
        </>
      )}
    </section>
  );
}

export default GroupedTaskList;
