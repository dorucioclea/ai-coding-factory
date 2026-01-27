import { Suspense } from 'react';
import { Loader2 } from 'lucide-react';
import IntegrationsContent from './IntegrationsContent';

/**
 * Platform Connections page
 * Allows users to connect/disconnect social media platforms
 */
export default function IntegrationsPage() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-[400px] items-center justify-center">
          <div className="flex flex-col items-center gap-4">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
            <p className="text-sm text-muted-foreground">
              Loading integrations...
            </p>
          </div>
        </div>
      }
    >
      <IntegrationsContent />
    </Suspense>
  );
}
