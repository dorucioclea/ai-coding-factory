'use client';

import { Users } from 'lucide-react';
import { Label, Switch, Textarea } from '@/components/ui';
import { ProfileConstraints } from '@/types';
import { cn } from '@/lib/utils';

interface CollaborationToggleProps {
  isOpen: boolean;
  preferences?: string;
  onToggle: (isOpen: boolean) => void;
  onPreferencesChange: (preferences: string) => void;
  className?: string;
}

/**
 * Component for managing collaboration availability
 */
export function CollaborationToggle({
  isOpen,
  preferences = '',
  onToggle,
  onPreferencesChange,
  className,
}: CollaborationToggleProps) {
  return (
    <div className={cn('space-y-4', className)}>
      {/* Toggle Switch */}
      <div className="flex items-center justify-between rounded-lg border p-4">
        <div className="flex items-center gap-3">
          <div className="rounded-full bg-primary/10 p-2">
            <Users className="h-5 w-5 text-primary" />
          </div>
          <div>
            <Label htmlFor="collaboration-toggle" className="cursor-pointer">
              Open to Collaborations
            </Label>
            <p className="text-xs text-muted-foreground">
              Let other creators know you&apos;re available for collaborations
            </p>
          </div>
        </div>
        <Switch
          id="collaboration-toggle"
          checked={isOpen}
          onCheckedChange={onToggle}
        />
      </div>

      {/* Collaboration Preferences */}
      {isOpen && (
        <div className="space-y-2">
          <Label htmlFor="collaboration-preferences">
            Collaboration Preferences
            <span className="text-xs text-muted-foreground ml-2">
              (Optional)
            </span>
          </Label>
          <Textarea
            id="collaboration-preferences"
            placeholder="Describe what kind of collaborations you&apos;re interested in, your availability, requirements, etc."
            value={preferences}
            onChange={(e) => onPreferencesChange(e.target.value)}
            maxLength={ProfileConstraints.maxCollaborationPreferencesLength}
            rows={4}
            className="resize-none"
          />
          <p className="text-xs text-muted-foreground text-right">
            {preferences.length}/
            {ProfileConstraints.maxCollaborationPreferencesLength}
          </p>
        </div>
      )}
    </div>
  );
}

export default CollaborationToggle;
