'use client';

import { useState, useCallback } from 'react';
import { Inbox, Send, MessageSquare } from 'lucide-react';
import { Button, Skeleton } from '@/components/ui';
import { CollaborationRequestCard } from '@/components/collaborations';
import {
  useCollaborationInbox,
  useSentCollaborations,
  useAcceptCollaborationRequest,
  useDeclineCollaborationRequest,
} from '@/hooks/use-collaborations';
import type { CollaborationRequestStatus } from '@/types/collaboration';

type Tab = 'inbox' | 'sent';

const STATUS_OPTIONS: { label: string; value: CollaborationRequestStatus | undefined }[] = [
  { label: 'All', value: undefined },
  { label: 'Pending', value: 'Pending' },
  { label: 'Accepted', value: 'Accepted' },
  { label: 'Declined', value: 'Declined' },
];

/**
 * Collaboration requests page (inbox + sent)
 * Story: ACF-011
 */
export default function CollaborationsPage() {
  const [activeTab, setActiveTab] = useState<Tab>('inbox');
  const [statusFilter, setStatusFilter] = useState<CollaborationRequestStatus | undefined>(
    undefined
  );
  const [page, setPage] = useState(1);
  const [actionRequestId, setActionRequestId] = useState<string | null>(null);

  const inboxQuery = useCollaborationInbox(
    statusFilter,
    page,
    20,
    activeTab === 'inbox'
  );
  const sentQuery = useSentCollaborations(
    statusFilter,
    page,
    20,
    activeTab === 'sent'
  );

  const { mutate: acceptRequest, isPending: isAcceptPending } = useAcceptCollaborationRequest();
  const { mutate: declineRequest, isPending: isDeclinePending } = useDeclineCollaborationRequest();

  const currentQuery = activeTab === 'inbox' ? inboxQuery : sentQuery;
  const requests = currentQuery.data?.items ?? [];
  const totalCount = currentQuery.data?.totalCount ?? 0;
  const hasNextPage = currentQuery.data?.hasNextPage ?? false;
  const hasPreviousPage = currentQuery.data?.hasPreviousPage ?? false;

  const handleAccept = useCallback(
    (requestId: string) => {
      setActionRequestId(requestId);
      acceptRequest(requestId, {
        onSettled: () => setActionRequestId(null),
      });
    },
    [acceptRequest]
  );

  const handleDecline = useCallback(
    (requestId: string) => {
      setActionRequestId(requestId);
      declineRequest({ requestId }, {
        onSettled: () => setActionRequestId(null),
      });
    },
    [declineRequest]
  );

  const handleTabChange = useCallback((tab: Tab) => {
    setActiveTab(tab);
    setPage(1);
    setStatusFilter(undefined);
  }, []);

  const handleStatusChange = useCallback((status: CollaborationRequestStatus | undefined) => {
    setStatusFilter(status);
    setPage(1);
  }, []);

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-2">
        <MessageSquare className="h-6 w-6" />
        <h1 className="text-2xl font-bold">Collaborations</h1>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 border-b pb-2">
        <Button
          variant={activeTab === 'inbox' ? 'default' : 'outline'}
          size="sm"
          onClick={() => handleTabChange('inbox')}
        >
          <Inbox className="h-4 w-4 mr-1.5" />
          Inbox
          {inboxQuery.data?.totalCount !== undefined && inboxQuery.data.totalCount > 0 && (
            <span className="ml-1.5 rounded-full bg-primary-foreground/20 px-1.5 text-xs">
              {inboxQuery.data.totalCount}
            </span>
          )}
        </Button>
        <Button
          variant={activeTab === 'sent' ? 'default' : 'outline'}
          size="sm"
          onClick={() => handleTabChange('sent')}
        >
          <Send className="h-4 w-4 mr-1.5" />
          Sent
        </Button>
      </div>

      {/* Status Filter */}
      <div className="flex gap-1.5 flex-wrap">
        {STATUS_OPTIONS.map((option) => (
          <Button
            key={option.label}
            variant={statusFilter === option.value ? 'default' : 'outline'}
            size="sm"
            onClick={() => handleStatusChange(option.value)}
          >
            {option.label}
          </Button>
        ))}
      </div>

      {/* Results Count */}
      {!currentQuery.isLoading && (
        <p className="text-sm text-muted-foreground">
          {totalCount === 0
            ? 'No collaboration requests'
            : `${totalCount} request${totalCount === 1 ? '' : 's'}`}
        </p>
      )}

      {/* Error State */}
      {currentQuery.error && (
        <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive">
          <h3 className="font-semibold">Error loading requests</h3>
          <p className="text-sm mt-1">
            {currentQuery.error instanceof Error
              ? currentQuery.error.message
              : 'An unexpected error occurred'}
          </p>
        </div>
      )}

      {/* Loading State */}
      {currentQuery.isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
      ) : requests.length > 0 ? (
        <>
          {/* Request List */}
          <div className="space-y-3">
            {requests.map((request) => (
              <CollaborationRequestCard
                key={request.id}
                request={request}
                viewMode={activeTab}
                onAccept={handleAccept}
                onDecline={handleDecline}
                isAccepting={isAcceptPending && actionRequestId === request.id}
                isDeclining={isDeclinePending && actionRequestId === request.id}
              />
            ))}
          </div>

          {/* Pagination */}
          {(hasNextPage || hasPreviousPage) && (
            <div className="flex justify-center gap-2 pt-4">
              <Button
                variant="outline"
                size="sm"
                disabled={!hasPreviousPage}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Previous
              </Button>
              <span className="flex items-center text-sm text-muted-foreground px-2">
                Page {page}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={!hasNextPage}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </>
      ) : (
        /* Empty State */
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <div className="rounded-full bg-muted p-6 mb-4">
            {activeTab === 'inbox' ? (
              <Inbox className="h-12 w-12 text-muted-foreground" />
            ) : (
              <Send className="h-12 w-12 text-muted-foreground" />
            )}
          </div>
          <h2 className="text-xl font-semibold mb-2">
            {activeTab === 'inbox' ? 'No incoming requests' : 'No sent requests'}
          </h2>
          <p className="text-muted-foreground max-w-md">
            {activeTab === 'inbox'
              ? 'When other creators send you collaboration proposals, they will appear here.'
              : 'Discover creators and send them collaboration requests from the Discover page.'}
          </p>
        </div>
      )}
    </div>
  );
}
