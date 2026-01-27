/**
 * Unit tests for DiscoveryFilters component
 * Story: ACF-010 - Creator Discovery
 */

import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { DiscoveryFilters } from '@/components/discovery/DiscoveryFilters';
import type { DiscoveryFilters as DiscoveryFiltersType } from '@/types/discovery';
import { createWrapper } from '../../utils/test-utils';

// Mock the hooks
vi.mock('@/hooks/use-discovery', () => ({
  useNicheCategories: () => ({
    data: ['Gaming', 'Tech', 'Lifestyle', 'Beauty', 'Travel'],
  }),
  usePlatforms: () => ({
    data: ['YouTube', 'TikTok', 'Instagram', 'Twitter'],
  }),
}));

vi.mock('@/hooks/use-debounce', () => ({
  useDebounce: (value: string) => value, // No debounce for tests
}));

const mockFilters: DiscoveryFiltersType = {};

const mockOnFiltersChange = {
  updateNiches: vi.fn(),
  updatePlatforms: vi.fn(),
  updateAudienceSize: vi.fn(),
  updateSearch: vi.fn(),
  updateOpenToCollab: vi.fn(),
  clearFilters: vi.fn(),
};

describe('DiscoveryFilters', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render search input', () => {
    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    expect(
      screen.getByPlaceholderText(/search creators by name/i)
    ).toBeInTheDocument();
  });

  it('should render open to collab button', () => {
    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    expect(
      screen.getByRole('button', { name: /open to collab/i })
    ).toBeInTheDocument();
  });

  it('should render audience size select', () => {
    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByRole('combobox')).toBeInTheDocument();
  });

  it('should render filters button', () => {
    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    expect(
      screen.getByRole('button', { name: /filters/i })
    ).toBeInTheDocument();
  });

  it('should update search when typing', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    const searchInput = screen.getByPlaceholderText(/search creators by name/i);
    await user.type(searchInput, 'gaming');

    await waitFor(() => {
      expect(mockOnFiltersChange.updateSearch).toHaveBeenCalled();
    });
  });

  it('should clear search when clicking X', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={{ search: 'test' }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    const searchInput = screen.getByPlaceholderText(/search creators by name/i);
    await user.clear(searchInput);
    await user.type(searchInput, 'new search');

    // Find and click the clear button
    const clearButton = searchInput.parentElement?.querySelector('button');
    if (clearButton) {
      await user.click(clearButton);
    }

    expect(searchInput).toHaveValue('');
  });

  it('should toggle open to collab filter', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    const collabButton = screen.getByRole('button', { name: /open to collab/i });
    await user.click(collabButton);

    expect(mockOnFiltersChange.updateOpenToCollab).toHaveBeenCalledWith(true);
  });

  it('should toggle open to collab filter off when already active', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={{ openToCollab: true }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    const collabButton = screen.getByRole('button', { name: /open to collab/i });
    await user.click(collabButton);

    expect(mockOnFiltersChange.updateOpenToCollab).toHaveBeenCalledWith(undefined);
  });

  it('should show filter count badge when filters are active', () => {
    render(
      <DiscoveryFilters
        filters={{ niches: ['Gaming'] }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={2}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('should show clear all button when filters are active', () => {
    render(
      <DiscoveryFilters
        filters={{ niches: ['Gaming'] }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    expect(
      screen.getByRole('button', { name: /clear all/i })
    ).toBeInTheDocument();
  });

  it('should not show clear all button when no filters active', () => {
    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    expect(
      screen.queryByRole('button', { name: /clear all/i })
    ).not.toBeInTheDocument();
  });

  it('should call clearFilters when clear all clicked', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={{ niches: ['Gaming'] }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    const clearAllButton = screen.getByRole('button', { name: /clear all/i });
    await user.click(clearAllButton);

    expect(mockOnFiltersChange.clearFilters).toHaveBeenCalled();
  });

  it('should display active niche filter tags', () => {
    render(
      <DiscoveryFilters
        filters={{ niches: ['Gaming', 'Tech'] }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText('Gaming')).toBeInTheDocument();
    expect(screen.getByText('Tech')).toBeInTheDocument();
  });

  it('should display active platform filter tags', () => {
    render(
      <DiscoveryFilters
        filters={{ platforms: ['YouTube', 'TikTok'] }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    expect(screen.getByText('YouTube')).toBeInTheDocument();
    expect(screen.getByText('TikTok')).toBeInTheDocument();
  });

  it('should remove niche filter when tag X clicked', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={{ niches: ['Gaming', 'Tech'] }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    // Find the Gaming text and its sibling X button
    const gamingText = screen.getByText('Gaming');
    const badge = gamingText.closest('[class*="badge"]') ?? gamingText.parentElement;
    const closeButton = badge?.querySelector('button');

    expect(closeButton).not.toBeNull();
    if (closeButton) {
      await user.click(closeButton);
    }

    expect(mockOnFiltersChange.updateNiches).toHaveBeenCalledWith(['Tech']);
  });

  it('should open filters sheet when filters button clicked', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    const filtersButton = screen.getByRole('button', { name: /filters/i });
    await user.click(filtersButton);

    await waitFor(() => {
      expect(screen.getByText('Filter Creators')).toBeInTheDocument();
    });
  });

  it('should show niche checkboxes in sheet', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    await user.click(screen.getByRole('button', { name: /filters/i }));

    await waitFor(() => {
      expect(screen.getByText('Niches')).toBeInTheDocument();
      expect(screen.getByLabelText('Gaming')).toBeInTheDocument();
      expect(screen.getByLabelText('Tech')).toBeInTheDocument();
    });
  });

  it('should show platform checkboxes in sheet', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    await user.click(screen.getByRole('button', { name: /filters/i }));

    await waitFor(() => {
      expect(screen.getByText('Platforms')).toBeInTheDocument();
      expect(screen.getByLabelText('YouTube')).toBeInTheDocument();
      expect(screen.getByLabelText('TikTok')).toBeInTheDocument();
    });
  });

  it('should toggle niche checkbox', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    await user.click(screen.getByRole('button', { name: /filters/i }));

    await waitFor(() => {
      expect(screen.getByLabelText('Gaming')).toBeInTheDocument();
    });

    await user.click(screen.getByLabelText('Gaming'));

    expect(mockOnFiltersChange.updateNiches).toHaveBeenCalledWith(['Gaming']);
  });

  it('should toggle platform checkbox', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={mockFilters}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={0}
      />,
      { wrapper: createWrapper() }
    );

    await user.click(screen.getByRole('button', { name: /filters/i }));

    await waitFor(() => {
      expect(screen.getByLabelText('YouTube')).toBeInTheDocument();
    });

    await user.click(screen.getByLabelText('YouTube'));

    expect(mockOnFiltersChange.updatePlatforms).toHaveBeenCalledWith(['YouTube']);
  });

  it('should show checked state for active niche filters', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={{ niches: ['Gaming'] }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    await user.click(screen.getByRole('button', { name: /filters/i }));

    await waitFor(() => {
      // Custom checkbox shows bg-primary class when checked
      const gamingCheckbox = screen.getByLabelText('Gaming');
      const checkboxIndicator = gamingCheckbox.querySelector('div');
      expect(checkboxIndicator?.className).toContain('bg-primary');
    });
  });

  it('should show unchecked state for inactive niche filters', async () => {
    const user = userEvent.setup();

    render(
      <DiscoveryFilters
        filters={{ niches: ['Gaming'] }}
        onFiltersChange={mockOnFiltersChange}
        activeFilterCount={1}
      />,
      { wrapper: createWrapper() }
    );

    await user.click(screen.getByRole('button', { name: /filters/i }));

    await waitFor(() => {
      // Custom checkbox shows border-input class when unchecked
      const techCheckbox = screen.getByLabelText('Tech');
      const checkboxIndicator = techCheckbox.querySelector('div');
      expect(checkboxIndicator?.className).toContain('border-input');
    });
  });
});
