/**
 * Unit tests for ProjectCard component
 * Story: ACF-013 - Shared Project Spaces
 */

import { describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent } from '../../utils/test-utils';
import { ProjectCard } from '@/components/shared-projects/ProjectCard';
import type { SharedProjectDto } from '@/types/shared-project';

const mockProject: SharedProjectDto = {
  id: 'proj-1',
  name: 'Joint Video Project',
  description: 'A collaboration between two creators',
  status: 'Active',
  collaborationRequestId: 'collab-1',
  ownerId: 'user-1',
  memberCount: 2,
  taskCount: 3,
  linkCount: 1,
  createdAt: '2025-06-15T10:00:00Z',
  members: [
    { id: 'mem-1', userId: 'user-1', role: 'Owner', joinedAt: '2025-06-15T10:00:00Z' },
    { id: 'mem-2', userId: 'user-2', role: 'Member', joinedAt: '2025-06-15T10:00:00Z' },
  ],
};

describe('ProjectCard', () => {
  it('should render project name', () => {
    render(<ProjectCard project={mockProject} onClick={vi.fn()} />);
    expect(screen.getByText('Joint Video Project')).toBeDefined();
  });

  it('should render project description', () => {
    render(<ProjectCard project={mockProject} onClick={vi.fn()} />);
    expect(screen.getByText('A collaboration between two creators')).toBeDefined();
  });

  it('should render member count', () => {
    render(<ProjectCard project={mockProject} onClick={vi.fn()} />);
    expect(screen.getByText('2')).toBeDefined();
  });

  it('should render task count', () => {
    render(<ProjectCard project={mockProject} onClick={vi.fn()} />);
    expect(screen.getByText('3 tasks')).toBeDefined();
  });

  it('should render link count', () => {
    render(<ProjectCard project={mockProject} onClick={vi.fn()} />);
    expect(screen.getByText('1 links')).toBeDefined();
  });

  it('should render Active badge for active projects', () => {
    render(<ProjectCard project={mockProject} onClick={vi.fn()} />);
    expect(screen.getByText('Active')).toBeDefined();
  });

  it('should render Closed badge for closed projects', () => {
    const closedProject = { ...mockProject, status: 'Closed' as const };
    render(<ProjectCard project={closedProject} onClick={vi.fn()} />);
    expect(screen.getByText('Closed')).toBeDefined();
  });

  it('should call onClick with project id when clicked', () => {
    const onClick = vi.fn();
    render(<ProjectCard project={mockProject} onClick={onClick} />);

    const button = screen.getByRole('button');
    fireEvent.click(button);

    expect(onClick).toHaveBeenCalledWith('proj-1');
  });

  it('should not render description when absent', () => {
    const projectNoDesc = { ...mockProject, description: undefined };
    render(<ProjectCard project={projectNoDesc} onClick={vi.fn()} />);
    expect(screen.queryByText('A collaboration between two creators')).toBeNull();
  });
});
