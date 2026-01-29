/**
 * Unit tests for UnreadBadge component
 * Story: ACF-012 - Real-Time Messaging
 */

import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { UnreadBadge } from '@/components/messaging/UnreadBadge';

describe('UnreadBadge', () => {
  it('should render the count when count > 0', () => {
    render(<UnreadBadge count={5} />);

    expect(screen.getByText('5')).toBeInTheDocument();
  });

  it('should return null when count is 0', () => {
    const { container } = render(<UnreadBadge count={0} />);

    expect(container.firstChild).toBeNull();
  });

  it('should show 99+ when count exceeds 99', () => {
    render(<UnreadBadge count={150} />);

    expect(screen.getByText('99+')).toBeInTheDocument();
  });

  it('should show exact count at 99', () => {
    render(<UnreadBadge count={99} />);

    expect(screen.getByText('99')).toBeInTheDocument();
  });

  it('should have aria-label with count', () => {
    render(<UnreadBadge count={7} />);

    expect(screen.getByLabelText('7 unread messages')).toBeInTheDocument();
  });

  it('should have aria-label with 99+ for large counts', () => {
    render(<UnreadBadge count={200} />);

    expect(screen.getByLabelText('99+ unread messages')).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    render(<UnreadBadge count={3} className="ml-2" />);

    const badge = screen.getByText('3');
    expect(badge.className).toContain('ml-2');
  });
});
