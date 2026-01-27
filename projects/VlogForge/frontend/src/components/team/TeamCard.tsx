'use client';

import Link from 'next/link';
import { Users, Calendar } from 'lucide-react';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui';
import type { TeamResponse } from '@/types';

interface TeamCardProps {
  team: TeamResponse;
}

/**
 * Team summary card for team list
 * ACF-015 Phase 5
 */
export function TeamCard({ team }: TeamCardProps) {
  const createdDate = new Date(team.createdAt).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });

  return (
    <Link href={`/dashboard/team/${team.id}`}>
      <Card className="hover:border-primary transition-colors cursor-pointer">
        <CardHeader>
          <CardTitle>{team.name}</CardTitle>
          {team.description && (
            <CardDescription className="line-clamp-2">
              {team.description}
            </CardDescription>
          )}
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-4 text-sm text-muted-foreground">
            <div className="flex items-center gap-1">
              <Users className="h-4 w-4" />
              <span>
                {team.memberCount} {team.memberCount === 1 ? 'member' : 'members'}
              </span>
            </div>
            <div className="flex items-center gap-1">
              <Calendar className="h-4 w-4" />
              <span>Created {createdDate}</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}

export default TeamCard;
