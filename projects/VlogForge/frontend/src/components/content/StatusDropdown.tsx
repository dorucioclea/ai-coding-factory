'use client';

import { Check, ChevronDown } from 'lucide-react';
import { useState } from 'react';

import {
  Button,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui';
import {
  IdeaStatus,
  STATUS_CONFIG,
  getAvailableTransitions,
  isValidTransition,
} from '@/types';

interface StatusDropdownProps {
  currentStatus: IdeaStatus;
  onChange: (newStatus: IdeaStatus) => void;
  disabled?: boolean;
}

export function StatusDropdown({
  currentStatus,
  onChange,
  disabled,
}: StatusDropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const availableTransitions = getAvailableTransitions(currentStatus);
  const currentConfig = STATUS_CONFIG[currentStatus];

  const handleSelect = (newStatus: IdeaStatus) => {
    if (isValidTransition(currentStatus, newStatus)) {
      onChange(newStatus);
      setIsOpen(false);
    }
  };

  return (
    <DropdownMenu open={isOpen} onOpenChange={setIsOpen}>
      <DropdownMenuTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          disabled={disabled || availableTransitions.length === 0}
          className="h-8"
        >
          {currentConfig.label}
          {availableTransitions.length > 0 && (
            <ChevronDown className="ml-2 h-4 w-4" />
          )}
        </Button>
      </DropdownMenuTrigger>
      {availableTransitions.length > 0 && (
        <DropdownMenuContent align="end" className="w-48">
          {availableTransitions.map((status) => {
            const config = STATUS_CONFIG[status];
            return (
              <DropdownMenuItem
                key={status}
                onClick={() => handleSelect(status)}
                className="cursor-pointer"
              >
                <div className="flex w-full items-center justify-between">
                  <div>
                    <div className="font-medium">{config.label}</div>
                    <div className="text-xs text-muted-foreground">
                      {config.description}
                    </div>
                  </div>
                  {currentStatus === status && (
                    <Check className="ml-2 h-4 w-4" />
                  )}
                </div>
              </DropdownMenuItem>
            );
          })}
        </DropdownMenuContent>
      )}
    </DropdownMenu>
  );
}

export default StatusDropdown;
