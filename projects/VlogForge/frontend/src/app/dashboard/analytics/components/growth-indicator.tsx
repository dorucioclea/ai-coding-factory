'use client';

import { TrendingUp, TrendingDown, Minus } from 'lucide-react';

interface GrowthIndicatorProps {
  value: number;
  comparisonDays: number;
}

/**
 * Growth indicator showing percentage change with up/down arrows
 * Story: ACF-004 (AC5)
 */
export function GrowthIndicator({ value, comparisonDays }: GrowthIndicatorProps) {
  const isPositive = value > 0;
  const isNegative = value < 0;

  const colorClass = isPositive
    ? 'text-green-600'
    : isNegative
      ? 'text-red-600'
      : 'text-gray-500';

  const Icon = isPositive ? TrendingUp : isNegative ? TrendingDown : Minus;

  return (
    <div className={`flex items-center gap-1 text-xs ${colorClass}`}>
      <Icon className="h-3 w-3" />
      <span>
        {isPositive && '+'}
        {value.toFixed(1)}%
      </span>
      <span className="text-muted-foreground">vs last {comparisonDays} days</span>
    </div>
  );
}
