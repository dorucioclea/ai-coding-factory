'use client';

import { useEffect, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { CheckCircle, XCircle, Loader2, Users } from 'lucide-react';
import { Button, Card, CardContent, CardHeader, CardTitle } from '@/components/ui';
import { useAcceptInvitation, useAuth } from '@/hooks';
import { ApiError } from '@/types';

/**
 * Team Invitation Acceptance Page
 * ACF-015 Phase 5
 *
 * Handles team invitation token and accepts invitation
 */
export default function AcceptInvitationPage() {
  const params = useParams();
  const router = useRouter();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const token = params?.['token'] as string | undefined;

  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [errorMessage, setErrorMessage] = useState<string>('');
  const [teamId, setTeamId] = useState<string>('');

  const acceptInvitation = useAcceptInvitation();

  useEffect(() => {
    if (authLoading) return;

    if (!isAuthenticated) {
      // Redirect to login with return URL
      router.push(`/auth/login?returnUrl=/team/invite/${token}`);
      return;
    }

    if (!token) {
      setStatus('error');
      setErrorMessage('Invalid invitation link');
      return;
    }

    // Accept invitation
    const accept = async () => {
      try {
        const team = await acceptInvitation.mutateAsync(token);

        if (team?.id) {
          setTeamId(team.id);
        }

        setStatus('success');
      } catch (err) {
        setStatus('error');

        if (err instanceof ApiError) {
          if (err.status === 404) {
            setErrorMessage('Invitation not found or has expired');
          } else if (err.status === 400) {
            setErrorMessage('This invitation has already been accepted');
          } else {
            setErrorMessage(err.detail ?? err.title);
          }
        } else {
          setErrorMessage('Failed to accept invitation. Please try again.');
        }
      }
    };

    accept();
  }, [token, isAuthenticated, authLoading, acceptInvitation, router]);

  if (authLoading || status === 'loading') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <Card className="w-full max-w-md">
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Loader2 className="h-12 w-12 animate-spin text-primary mb-4" />
            <h2 className="text-xl font-semibold mb-2">Processing Invitation</h2>
            <p className="text-muted-foreground text-center">
              Please wait while we add you to the team...
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (status === 'success') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <Card className="w-full max-w-md">
          <CardHeader>
            <div className="flex justify-center mb-4">
              <div className="rounded-full bg-green-500/10 p-3">
                <CheckCircle className="h-12 w-12 text-green-500" />
              </div>
            </div>
            <CardTitle className="text-center text-2xl">Invitation Accepted!</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-center text-muted-foreground">
              You have successfully joined the team. Welcome aboard!
            </p>
            <div className="flex flex-col gap-2">
              {teamId ? (
                <Button onClick={() => router.push(`/dashboard/team/${teamId}`)}>
                  <Users className="h-4 w-4 mr-2" />
                  View Team
                </Button>
              ) : (
                <Button onClick={() => router.push('/dashboard/team')}>
                  <Users className="h-4 w-4 mr-2" />
                  View All Teams
                </Button>
              )}
              <Button variant="outline" onClick={() => router.push('/dashboard')}>
                Go to Dashboard
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <Card className="w-full max-w-md">
        <CardHeader>
          <div className="flex justify-center mb-4">
            <div className="rounded-full bg-destructive/10 p-3">
              <XCircle className="h-12 w-12 text-destructive" />
            </div>
          </div>
          <CardTitle className="text-center text-2xl">Invitation Error</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive text-center">
            {errorMessage || 'Unable to process invitation'}
          </div>
          <div className="flex flex-col gap-2">
            <Button variant="outline" onClick={() => router.push('/dashboard/team')}>
              View My Teams
            </Button>
            <Button variant="ghost" onClick={() => router.push('/dashboard')}>
              Go to Dashboard
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
