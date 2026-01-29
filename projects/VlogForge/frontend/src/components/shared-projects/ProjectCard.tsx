'use client';

import { FolderOpen, Users, ListTodo, Link2, Clock } from 'lucide-react';
import { Badge } from '@/components/ui';
import type { SharedProjectDto } from '@/types/shared-project';
import { ProjectStatusConfig } from '@/types/shared-project';

interface ProjectCardProps {
  project: SharedProjectDto;
  onClick: (projectId: string) => void;
}

/**
 * Card component for displaying a shared project summary
 * Story: ACF-013
 */
export function ProjectCard({ project, onClick }: ProjectCardProps) {
  const statusConfig = ProjectStatusConfig[project.status];
  const formattedDate = new Date(project.createdAt).toLocaleDateString();

  return (
    <button
      type="button"
      className="w-full text-left rounded-lg border p-4 hover:bg-accent/50 transition-colors"
      onClick={() => onClick(project.id)}
    >
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-2">
          <FolderOpen className="h-5 w-5 text-muted-foreground" />
          <h3 className="font-semibold text-lg">{project.name}</h3>
        </div>
        <Badge variant={statusConfig.variant}>
          {statusConfig.label}
        </Badge>
      </div>

      {project.description && (
        <p className="text-sm text-muted-foreground mt-2 line-clamp-2">
          {project.description}
        </p>
      )}

      <div className="flex items-center gap-4 mt-3 text-sm text-muted-foreground">
        <span className="flex items-center gap-1">
          <Users className="h-3.5 w-3.5" />
          {project.memberCount}
        </span>
        <span className="flex items-center gap-1">
          <ListTodo className="h-3.5 w-3.5" />
          {project.taskCount} tasks
        </span>
        <span className="flex items-center gap-1">
          <Link2 className="h-3.5 w-3.5" />
          {project.linkCount} links
        </span>
        <span className="flex items-center gap-1">
          <Clock className="h-3.5 w-3.5" />
          {formattedDate}
        </span>
      </div>
    </button>
  );
}
