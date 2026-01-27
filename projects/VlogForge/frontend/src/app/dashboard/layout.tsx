'use client';

import { ErrorBoundary } from '@/components/ErrorBoundary';
import { Header, Sidebar } from '@/components/layout';
import { useUIStore } from '@/stores';
import { cn } from '@/lib/utils';

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { sidebar } = useUIStore();

  return (
    <div className="min-h-screen">
      <Header />
      <Sidebar />
      <main
        className={cn(
          'min-h-[calc(100vh-3.5rem)] transition-all duration-300',
          sidebar.isOpen
            ? sidebar.isCollapsed
              ? 'md:pl-16'
              : 'md:pl-64'
            : 'md:pl-0'
        )}
      >
        <div className="container mx-auto p-6">
          <ErrorBoundary>{children}</ErrorBoundary>
        </div>
      </main>
    </div>
  );
}
