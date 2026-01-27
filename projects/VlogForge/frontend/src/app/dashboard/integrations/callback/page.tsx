import { Suspense } from 'react';
import { Loader2 } from 'lucide-react';
import OAuthCallbackContent from './OAuthCallbackContent';

/**
 * OAuth callback page
 * Handles the OAuth redirect after user authorizes platform connection
 */
export default function OAuthCallbackPage() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-[400px] items-center justify-center">
          <div className="flex flex-col items-center gap-6 text-center">
            <Loader2 className="h-16 w-16 animate-spin text-primary" />
            <div className="space-y-2">
              <h2 className="text-2xl font-bold">Loading...</h2>
            </div>
          </div>
        </div>
      }
    >
      <OAuthCallbackContent />
    </Suspense>
  );
}
