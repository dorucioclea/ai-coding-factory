import { NextResponse } from 'next/server';

/**
 * Health check endpoint for container orchestration
 * Used by Docker HEALTHCHECK and Kubernetes probes
 */
export async function GET() {
  const health = {
    status: 'healthy',
    timestamp: new Date().toISOString(),
    uptime: process.uptime(),
    environment: process.env['NODE_ENV'],
  };

  return NextResponse.json(health);
}
