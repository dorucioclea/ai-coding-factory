'use client';

import { Search, X, Filter, Users, Check } from 'lucide-react';
import { useState, useCallback, useEffect } from 'react';
import {
  Button,
  Input,
  Badge,
  Label,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  Separator,
} from '@/components/ui';
import type { DiscoveryFilters, AudienceSizeRange } from '@/types/discovery';
import { AudienceSizeLabels } from '@/types/discovery';
import { useNicheCategories, usePlatforms } from '@/hooks/use-discovery';
import { useDebounce } from '@/hooks/use-debounce';
import { cn } from '@/lib/utils';

interface DiscoveryFiltersProps {
  filters: DiscoveryFilters;
  onFiltersChange: {
    updateNiches: (niches: string[]) => void;
    updatePlatforms: (platforms: string[]) => void;
    updateAudienceSize: (audienceSize: AudienceSizeRange | undefined) => void;
    updateSearch: (search: string) => void;
    updateOpenToCollab: (openToCollab: boolean | undefined) => void;
    clearFilters: () => void;
  };
  activeFilterCount: number;
}

/**
 * Custom checkbox component using button styling
 */
function FilterCheckbox({
  id,
  checked,
  onChange,
  label,
}: {
  id: string;
  checked: boolean;
  onChange: () => void;
  label: string;
}) {
  return (
    <button
      type="button"
      id={id}
      onClick={onChange}
      className="flex items-center space-x-2 text-left w-full py-1 hover:bg-muted/50 rounded-md px-1"
    >
      <div
        className={cn(
          'h-4 w-4 rounded border flex items-center justify-center',
          checked
            ? 'bg-primary border-primary text-primary-foreground'
            : 'border-input'
        )}
      >
        {checked && <Check className="h-3 w-3" />}
      </div>
      <Label htmlFor={id} className="text-sm cursor-pointer">
        {label}
      </Label>
    </button>
  );
}

/**
 * Filter controls for creator discovery
 * Story: ACF-010
 */
export function DiscoveryFilters({
  filters,
  onFiltersChange,
  activeFilterCount,
}: DiscoveryFiltersProps) {
  const { data: niches = [] } = useNicheCategories();
  const { data: platforms = [] } = usePlatforms();
  const [isOpen, setIsOpen] = useState(false);

  const [searchInput, setSearchInput] = useState(filters.search ?? '');
  const debouncedSearch = useDebounce(searchInput, 300);

  // Destructure to get stable function reference
  const { updateSearch } = onFiltersChange;

  // Update search filter when debounced value changes
  useEffect(() => {
    updateSearch(debouncedSearch);
  }, [debouncedSearch, updateSearch]);

  const handleNicheToggle = useCallback(
    (niche: string) => {
      const currentNiches = filters.niches ?? [];
      const newNiches = currentNiches.includes(niche)
        ? currentNiches.filter((n) => n !== niche)
        : [...currentNiches, niche];
      onFiltersChange.updateNiches(newNiches);
    },
    [filters.niches, onFiltersChange]
  );

  const handlePlatformToggle = useCallback(
    (platform: string) => {
      const currentPlatforms = filters.platforms ?? [];
      const newPlatforms = currentPlatforms.includes(platform)
        ? currentPlatforms.filter((p) => p !== platform)
        : [...currentPlatforms, platform];
      onFiltersChange.updatePlatforms(newPlatforms);
    },
    [filters.platforms, onFiltersChange]
  );

  const handleAudienceSizeChange = useCallback(
    (value: string) => {
      onFiltersChange.updateAudienceSize(
        value === 'all' ? undefined : (value as AudienceSizeRange)
      );
    },
    [onFiltersChange]
  );

  const handleOpenToCollabToggle = useCallback(() => {
    onFiltersChange.updateOpenToCollab(
      filters.openToCollab === true ? undefined : true
    );
  }, [filters.openToCollab, onFiltersChange]);

  const handleClearAll = useCallback(() => {
    setSearchInput('');
    onFiltersChange.clearFilters();
  }, [onFiltersChange]);

  return (
    <div className="space-y-4">
      {/* Search Bar */}
      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search creators by name..."
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          className="pl-9 pr-9"
        />
        {searchInput && (
          <button
            type="button"
            onClick={() => setSearchInput('')}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
          >
            <X className="h-4 w-4" />
          </button>
        )}
      </div>

      {/* Quick Filters Row */}
      <div className="flex flex-wrap items-center gap-2">
        {/* Open to Collaboration Toggle */}
        <Button
          variant={filters.openToCollab ? 'default' : 'outline'}
          size="sm"
          onClick={handleOpenToCollabToggle}
          className="gap-1"
        >
          <Users className="h-3 w-3" />
          Open to Collab
        </Button>

        {/* Audience Size Select */}
        <Select
          value={filters.audienceSize ?? 'all'}
          onValueChange={handleAudienceSizeChange}
        >
          <SelectTrigger className="w-[140px] h-9">
            <SelectValue placeholder="Audience Size" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Any size</SelectItem>
            <SelectItem value="Small">{AudienceSizeLabels.Small}</SelectItem>
            <SelectItem value="Medium">{AudienceSizeLabels.Medium}</SelectItem>
            <SelectItem value="Large">{AudienceSizeLabels.Large}</SelectItem>
          </SelectContent>
        </Select>

        {/* Filter Dialog for Niches & Platforms */}
        <Dialog open={isOpen} onOpenChange={setIsOpen}>
          <DialogTrigger asChild>
            <Button variant="outline" size="sm" className="gap-1">
              <Filter className="h-3 w-3" />
              Filters
              {activeFilterCount > 0 && (
                <Badge variant="secondary" className="ml-1 px-1.5 py-0 text-xs">
                  {activeFilterCount}
                </Badge>
              )}
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle>Filter Creators</DialogTitle>
            </DialogHeader>

            <div className="space-y-6 mt-4">
              {/* Niches */}
              <div>
                <h4 className="font-medium mb-3">Niches</h4>
                <div className="grid grid-cols-2 gap-1 max-h-48 overflow-y-auto">
                  {niches.map((niche) => (
                    <FilterCheckbox
                      key={niche}
                      id={`niche-${niche}`}
                      checked={filters.niches?.includes(niche) ?? false}
                      onChange={() => handleNicheToggle(niche)}
                      label={niche}
                    />
                  ))}
                </div>
              </div>

              <Separator />

              {/* Platforms */}
              <div>
                <h4 className="font-medium mb-3">Platforms</h4>
                <div className="grid grid-cols-2 gap-1">
                  {platforms.map((platform) => (
                    <FilterCheckbox
                      key={platform}
                      id={`platform-${platform}`}
                      checked={filters.platforms?.includes(platform) ?? false}
                      onChange={() => handlePlatformToggle(platform)}
                      label={platform}
                    />
                  ))}
                </div>
              </div>
            </div>

            <div className="flex justify-end mt-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setIsOpen(false)}
              >
                Done
              </Button>
            </div>
          </DialogContent>
        </Dialog>

        {/* Clear All */}
        {activeFilterCount > 0 && (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleClearAll}
            className="text-muted-foreground"
          >
            Clear all
          </Button>
        )}
      </div>

      {/* Active Filter Tags */}
      {(filters.niches?.length ?? 0) > 0 || (filters.platforms?.length ?? 0) > 0 ? (
        <div className="flex flex-wrap gap-1.5">
          {filters.niches?.map((niche) => (
            <Badge key={niche} variant="secondary" className="gap-1">
              {niche}
              <button
                type="button"
                onClick={() => handleNicheToggle(niche)}
                className="ml-0.5 hover:text-destructive"
              >
                <X className="h-3 w-3" />
              </button>
            </Badge>
          ))}
          {filters.platforms?.map((platform) => (
            <Badge key={platform} variant="outline" className="gap-1">
              {platform}
              <button
                type="button"
                onClick={() => handlePlatformToggle(platform)}
                className="ml-0.5 hover:text-destructive"
              >
                <X className="h-3 w-3" />
              </button>
            </Badge>
          ))}
        </div>
      ) : null}
    </div>
  );
}

export default DiscoveryFilters;
