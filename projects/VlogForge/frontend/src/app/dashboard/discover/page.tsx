'use client';

import { useCallback, useEffect, useRef } from 'react';
import { Search, Users } from 'lucide-react';
import { Skeleton, Button } from '@/components/ui';
import { CreatorCard, DiscoveryFilters } from '@/components/discovery';
import { useDiscoverCreators, useDiscoveryFilters } from '@/hooks/use-discovery';

/**
 * Creator Discovery Page
 * Story: ACF-010
 *
 * Allows users to discover other creators with filtering and pagination
 */
export default function DiscoverCreatorsPage() {
  const {
    filters,
    updateNiches,
    updatePlatforms,
    updateAudienceSize,
    updateSearch,
    updateOpenToCollab,
    clearFilters,
    activeFilterCount,
  } = useDiscoveryFilters();

  const {
    data,
    isLoading,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
    error,
  } = useDiscoverCreators(filters);

  // Infinite scroll observer
  const loadMoreRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0]?.isIntersecting && hasNextPage && !isFetchingNextPage) {
          fetchNextPage();
        }
      },
      { threshold: 0.1 }
    );

    if (loadMoreRef.current) {
      observer.observe(loadMoreRef.current);
    }

    return () => observer.disconnect();
  }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

  const handleLoadMore = useCallback(() => {
    if (hasNextPage && !isFetchingNextPage) {
      fetchNextPage();
    }
  }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

  // Flatten pages into creators array
  const creators = data?.pages.flatMap((page) => page.items) ?? [];
  const totalCount = data?.pages[0]?.totalCount ?? 0;

  if (error) {
    return (
      <div className="container mx-auto py-6">
        <div className="rounded-lg bg-destructive/15 px-4 py-3 text-destructive">
          <h3 className="font-semibold">Error loading creators</h3>
          <p className="text-sm mt-1">
            {error instanceof Error ? error.message : 'An unexpected error occurred'}
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-2">
        <Search className="h-6 w-6" />
        <h1 className="text-2xl font-bold">Discover Creators</h1>
      </div>

      {/* Filters */}
      <DiscoveryFilters
        filters={filters}
        onFiltersChange={{
          updateNiches,
          updatePlatforms,
          updateAudienceSize,
          updateSearch,
          updateOpenToCollab,
          clearFilters,
        }}
        activeFilterCount={activeFilterCount}
      />

      {/* Results Count */}
      {!isLoading && (
        <p className="text-sm text-muted-foreground">
          {totalCount === 0
            ? 'No creators found'
            : `Found ${totalCount} creator${totalCount === 1 ? '' : 's'}`}
        </p>
      )}

      {/* Loading State */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {[1, 2, 3, 4, 5, 6, 7, 8].map((i) => (
            <Skeleton key={i} className="h-48" />
          ))}
        </div>
      ) : creators.length > 0 ? (
        <>
          {/* Creators Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {creators.map((creator) => (
              <CreatorCard key={creator.id} creator={creator} />
            ))}
          </div>

          {/* Load More */}
          <div ref={loadMoreRef} className="flex justify-center py-4">
            {isFetchingNextPage ? (
              <div className="flex items-center gap-2 text-muted-foreground">
                <div className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                Loading more...
              </div>
            ) : hasNextPage ? (
              <Button variant="outline" onClick={handleLoadMore}>
                Load More
              </Button>
            ) : (
              <p className="text-sm text-muted-foreground">
                You&apos;ve reached the end
              </p>
            )}
          </div>
        </>
      ) : (
        /* Empty State */
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <div className="rounded-full bg-muted p-6 mb-4">
            <Users className="h-12 w-12 text-muted-foreground" />
          </div>
          <h2 className="text-xl font-semibold mb-2">No creators found</h2>
          <p className="text-muted-foreground mb-6 max-w-md">
            {activeFilterCount > 0
              ? 'Try adjusting your filters or search term to find more creators.'
              : 'There are no creators to discover yet. Check back later!'}
          </p>
          {activeFilterCount > 0 && (
            <Button variant="outline" onClick={clearFilters}>
              Clear Filters
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
