'use client';

import { useState } from 'react';
import { Plus, CheckCircle2, Circle, Clock } from 'lucide-react';
import { Button, Badge } from '@/components/ui';
import type { SharedProjectTaskDto, SharedProjectTaskStatus } from '@/types/shared-project';
import { TaskStatusConfig } from '@/types/shared-project';

interface ProjectTaskListProps {
  tasks: SharedProjectTaskDto[];
  isActive: boolean;
  onAddTask: (title: string, description?: string) => void;
  onUpdateTaskStatus: (taskId: string, status: SharedProjectTaskStatus) => void;
  isAdding?: boolean;
  isUpdating?: boolean;
}

/**
 * Task list component for shared projects
 * Story: ACF-013
 */
export function ProjectTaskList({
  tasks,
  isActive,
  onAddTask,
  onUpdateTaskStatus,
  isAdding = false,
  isUpdating = false,
}: ProjectTaskListProps) {
  const [showAddForm, setShowAddForm] = useState(false);
  const [newTitle, setNewTitle] = useState('');
  const [newDescription, setNewDescription] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTitle.trim()) return;
    onAddTask(newTitle.trim(), newDescription.trim() || undefined);
    setNewTitle('');
    setNewDescription('');
    setShowAddForm(false);
  };

  const getStatusIcon = (status: SharedProjectTaskStatus) => {
    switch (status) {
      case 'Completed':
        return <CheckCircle2 className="h-4 w-4 text-green-500" />;
      case 'InProgress':
        return <Clock className="h-4 w-4 text-blue-500" />;
      default:
        return <Circle className="h-4 w-4 text-muted-foreground" />;
    }
  };

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">Tasks ({tasks.length})</h3>
        {isActive && (
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowAddForm(!showAddForm)}
          >
            <Plus className="h-4 w-4 mr-1" />
            Add Task
          </Button>
        )}
      </div>

      {showAddForm && (
        <form onSubmit={handleSubmit} className="border rounded-lg p-3 space-y-2">
          <input
            type="text"
            placeholder="Task title"
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            className="w-full px-3 py-1.5 border rounded text-sm"
            maxLength={200}
            required
          />
          <textarea
            placeholder="Description (optional)"
            value={newDescription}
            onChange={(e) => setNewDescription(e.target.value)}
            className="w-full px-3 py-1.5 border rounded text-sm resize-none"
            rows={2}
            maxLength={2000}
          />
          <div className="flex gap-2">
            <Button type="submit" size="sm" disabled={isAdding || !newTitle.trim()}>
              {isAdding ? 'Adding...' : 'Add'}
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

      {tasks.length === 0 ? (
        <p className="text-sm text-muted-foreground py-4 text-center">
          No tasks yet. Add a task to get started.
        </p>
      ) : (
        <div className="space-y-2">
          {tasks.map((task) => {
            const statusConfig = TaskStatusConfig[task.status];
            return (
              <div
                key={task.id}
                className="flex items-start gap-3 p-3 border rounded-lg"
              >
                {isActive ? (
                  <button
                    type="button"
                    className="mt-0.5"
                    disabled={isUpdating}
                    onClick={() => {
                      const nextStatus: SharedProjectTaskStatus =
                        task.status === 'Open'
                          ? 'InProgress'
                          : task.status === 'InProgress'
                            ? 'Completed'
                            : 'Open';
                      onUpdateTaskStatus(task.id, nextStatus);
                    }}
                  >
                    {getStatusIcon(task.status)}
                  </button>
                ) : (
                  <span className="mt-0.5">{getStatusIcon(task.status)}</span>
                )}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span
                      className={`text-sm font-medium ${task.status === 'Completed' ? 'line-through text-muted-foreground' : ''}`}
                    >
                      {task.title}
                    </span>
                    <Badge
                      variant={statusConfig.variant}
                    >
                      {statusConfig.label}
                    </Badge>
                  </div>
                  {task.description && (
                    <p className="text-xs text-muted-foreground mt-0.5">
                      {task.description}
                    </p>
                  )}
                  {task.dueDate && (
                    <p className="text-xs text-muted-foreground mt-0.5">
                      Due: {new Date(task.dueDate).toLocaleDateString()}
                    </p>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
