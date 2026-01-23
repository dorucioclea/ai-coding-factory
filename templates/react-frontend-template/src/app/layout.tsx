import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { getMessages } from 'next-intl/server';

import { Providers } from '@/components/layout';
import '@/styles/globals.css';

const inter = Inter({
  subsets: ['latin'],
  variable: '--font-sans',
});

export const metadata: Metadata = {
  title: {
    default: 'ProjectName',
    template: '%s | ProjectName',
  },
  description: 'Enterprise application built with Next.js and .NET',
  keywords: ['Next.js', 'React', 'TypeScript', 'Enterprise'],
  authors: [{ name: 'Your Company' }],
  creator: 'Your Company',
  metadataBase: new URL(
    process.env['NEXT_PUBLIC_APP_URL'] ?? 'http://localhost:3000'
  ),
  openGraph: {
    type: 'website',
    locale: 'en_US',
    url: '/',
    title: 'ProjectName',
    description: 'Enterprise application built with Next.js and .NET',
    siteName: 'ProjectName',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'ProjectName',
    description: 'Enterprise application built with Next.js and .NET',
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const messages = await getMessages();

  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${inter.variable} font-sans antialiased`}>
        <Providers messages={messages}>{children}</Providers>
      </body>
    </html>
  );
}
