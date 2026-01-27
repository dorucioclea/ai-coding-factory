'use client';

import { useState } from 'react';
import { Filter, Loader2 } from 'lucide-react';
import {
  Button,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui';
import { TaskCard } from './TaskCard';
import { TaskDetailModal } from './TaskDetailModal';
import {
  AssignmentStatus,
  AssignmentStatusLabels,
  type TaskAssignmentResponse,
} from '@/types';

interface TaskListProps {
  tasks: TaskAssignmentResponse[];
  isLoading?: boolean;
  onStatusChange: (taskId: string, status: AssignmentStatus) => void;
  onAddComment: (taskId: string, content: string) => Promise<void>;
  isUpdatingStatus?: boolean;
  isAddingComment?: boolean;
  className?: string;
}

/**
 * Filterable task list with detail modal
 * ACF-015 Phase 6
 */
export function TaskList({
  tasks,
  isLoading = false,
  onStatusChange,
  onAddComment,
  isUpdatingStatus = false,
  isAddingComment = false,
  className,
}: TaskListProps) {
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [overdueFilter, setOverdueFilter] = useState<string>('all');

  const selectedTask = tasks.find((t) => t.id === selectedTaskId) ?? null;

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

  const filteredTasks = tasks.filter((task) => {
    if (statusFilter !== 'all') {
      const filterStatus = parseInt(statusFilter, 10);
      if (task.status !== filterStatus) return false;
    }

    if (overdueFilter === 'overdue' && !task.isOverdue) return false;
    if (overdueFilter === 'not-overdue' && task.isOverdue) return false;

    return true;
  });

  return (
    <div className={className}>
      {/* Filters */}
      <div className="mb-6 flex flex-wrap items-center gap-4">
        <div className="flex items-center gap-2">
          <Filter className="h-4 w-4 text-muted-foreground" />
          <span className="text-sm font-medium">Filters:</span>
        </div>

        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="Filter by status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Statuses</SelectItem>
            <SelectItem value={AssignmentStatus.NotStarted.toString()}>
              {AssignmentStatusLabels[AssignmentStatus.NotStarted]}
            </SelectItem>
            <SelectItem value={AssignmentStatus.InProgress.toString()}>
              {AssignmentStatusLabels[AssignmentStatus.InProgress]}
            </SelectItem>
            <SelectItem value={AssignmentStatus.Completed.toString()}>
              {AssignmentStatusLabels[AssignmentStatus.Completed]}
            </SelectItem>
          </SelectContent>
        </Select>

        <Select value={overdueFilter} onValueChange={setOverdueFilter}>
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="Filter by overdue" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Tasks</SelectItem>
            <SelectItem value="overdue">Overdue Only</SelectItem>
            <SelectItem value="not-overdue">Not Overdue</SelectItem>
          </SelectContent>
        </Select>

        {(statusFilter !== 'all' || overdueFilter !== 'all') && (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              setStatusFilter('all');
              setOverdueFilter('all');
            }}
          >
            Clear Filters
          </Button>
        )}
      </div>

      {/* Loading State */}
      {isLoading && (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      )}

      {/* Empty State */}
      {!isLoading && filteredTasks.length === 0 && (
        <div className="text-center py-12">
          <p className="text-muted-foreground">
            {tasks.length === 0
              ? 'No tasks assigned to you yet.'
              : 'No tasks match your filters.'}
          </p>
        </div>
      )}

      {/* Task Cards */}
      {!isLoading && filteredTasks.length > 0 && (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {filteredTasks.map((task) => (
            <TaskCard
              key={task.id}
              task={task}
              onStatusChange={onStatusChange}
              onClick={handleTaskClick}
              isUpdating={isUpdatingStatus && selectedTaskId === task.id}
            />
          ))}
        </div>
      )}

      {/* Task Detail Modal */}
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

export default TaskList;
