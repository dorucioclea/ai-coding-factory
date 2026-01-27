'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useConfigureWorkflow } from '@/hooks';
import { useToast } from '@/hooks/use-toast';
import type { TeamMemberResponse, WorkflowSettingsResponse } from '@/types';
import { Settings, Users } from 'lucide-react';
import { cn } from '@/lib/utils';

interface WorkflowSettingsProps {
  teamId: string;
  currentSettings?: WorkflowSettingsResponse;
  members: TeamMemberResponse[];
  canManage: boolean;
  className?: string;
}

/**
 * Workflow settings configuration component
 * Story: ACF-009 AC1: Configure Workflow
 */
export function WorkflowSettings({
  teamId,
  currentSettings,
  members,
  canManage,
  className,
}: WorkflowSettingsProps) {
  const { toast } = useToast();
  const configureWorkflow = useConfigureWorkflow();

  const [requiresApproval, setRequiresApproval] = useState(
    currentSettings?.requiresApproval ?? false
  );
  const [selectedApprovers, setSelectedApprovers] = useState<string[]>(
    currentSettings?.approverIds ?? []
  );

  const handleSave = async () => {
    try {
      await configureWorkflow.mutateAsync({
        teamId,
        data: {
          requiresApproval,
          approverIds: selectedApprovers.length > 0 ? selectedApprovers : undefined,
        },
      });
      toast({
        title: 'Settings saved',
        description: 'Workflow settings have been updated.',
      });
    } catch {
      toast({
        title: 'Error',
        description: 'Failed to save workflow settings.',
        variant: 'destructive',
      });
    }
  };

  const toggleApprover = (userId: string) => {
    setSelectedApprovers((prev) =>
      prev.includes(userId)
        ? prev.filter((id) => id !== userId)
        : [...prev, userId]
    );
  };

  if (!canManage) {
    return (
      <Card className={cn(className)}>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Approval Workflow
          </CardTitle>
          <CardDescription>
            {currentSettings?.requiresApproval
              ? 'Content requires approval before scheduling'
              : 'No approval required'}
          </CardDescription>
        </CardHeader>
      </Card>
    );
  }

  return (
    <Card className={cn(className)}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Settings className="h-5 w-5" />
          Approval Workflow Settings
        </CardTitle>
        <CardDescription>
          Configure how content approval works for this team
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Enable/Disable Approval */}
        <div className="flex items-center justify-between">
          <div className="space-y-0.5">
            <Label htmlFor="requires-approval">Require Approval</Label>
            <p className="text-sm text-muted-foreground">
              Content must be approved before it can be scheduled
            </p>
          </div>
          <Switch
            id="requires-approval"
            checked={requiresApproval}
            onCheckedChange={setRequiresApproval}
          />
        </div>

        {/* Approvers Selection */}
        {requiresApproval && (
          <div className="space-y-4">
            <div className="flex items-center gap-2">
              <Users className="h-4 w-4 text-muted-foreground" />
              <Label>Designated Approvers</Label>
            </div>
            <p className="text-sm text-muted-foreground">
              Select specific team members who can approve content.
              If none selected, all Admins and the Owner can approve.
            </p>
            <div className="grid gap-2">
              {members.map((member) => (
                <div
                  key={member.userId}
                  className={cn(
                    'flex items-center justify-between rounded-lg border p-3 cursor-pointer transition-colors',
                    selectedApprovers.includes(member.userId)
                      ? 'border-primary bg-primary/5'
                      : 'hover:bg-muted/50'
                  )}
                  onClick={() => toggleApprover(member.userId)}
                >
                  <div className="flex items-center gap-3">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10 text-primary text-sm font-medium">
                      {member.displayName?.[0]?.toUpperCase() ?? '?'}
                    </div>
                    <div>
                      <p className="text-sm font-medium">
                        {member.displayName ?? member.email ?? 'Unknown'}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {member.email}
                      </p>
                    </div>
                  </div>
                  <div
                    className={cn(
                      'h-5 w-5 rounded-full border-2',
                      selectedApprovers.includes(member.userId)
                        ? 'bg-primary border-primary'
                        : 'border-muted-foreground'
                    )}
                  >
                    {selectedApprovers.includes(member.userId) && (
                      <svg
                        className="h-full w-full text-primary-foreground"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="3"
                      >
                        <polyline points="20 6 9 17 4 12" />
                      </svg>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Save Button */}
        <div className="flex justify-end">
          <Button
            onClick={handleSave}
            disabled={configureWorkflow.isPending}
          >
            {configureWorkflow.isPending ? 'Saving...' : 'Save Settings'}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

export default WorkflowSettings;
