'use client';

import * as React from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { NextIntlClientProvider } from 'next-intl';

import { ThemeProvider } from './theme-provider';
import { getQueryClient } from '@/lib/query-client';
import { Toaster } from 'sonner';

interface ProvidersProps {
  children: React.ReactNode;
  locale?: string;
  messages?: Record<string, unknown>;
}

export function Providers({
  children,
  locale = 'en',
  messages = {},
}: ProvidersProps) {
  const queryClient = getQueryClient();

  return (
    <QueryClientProvider client={queryClient}>
      <NextIntlClientProvider locale={locale} messages={messages}>
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          {children}
          <Toaster richColors closeButton position="bottom-right" />
        </ThemeProvider>
      </NextIntlClientProvider>
      {process.env['NODE_ENV'] === 'development' && (
        <ReactQueryDevtools initialIsOpen={false} />
      )}
    </QueryClientProvider>
  );
}

export default Providers;
