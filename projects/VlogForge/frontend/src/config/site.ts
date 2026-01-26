/**
 * Site configuration
 * Centralized configuration for the application
 */
export const siteConfig = {
  name: process.env['NEXT_PUBLIC_APP_NAME'] ?? 'ProjectName',
  description: 'Enterprise application built with Next.js and .NET',
  url: process.env['NEXT_PUBLIC_APP_URL'] ?? 'http://localhost:3000',
  apiUrl: process.env['NEXT_PUBLIC_API_URL'] ?? 'http://localhost:5000/api',

  // Navigation links
  mainNav: [
    { title: 'Dashboard', href: '/dashboard' },
    { title: 'Settings', href: '/settings' },
    { title: 'Help', href: '/help' },
  ],

  // Footer links
  footerLinks: [
    { title: 'Privacy', href: '/privacy' },
    { title: 'Terms', href: '/terms' },
    { title: 'Contact', href: '/contact' },
  ],

  // Social links
  social: {
    twitter: 'https://twitter.com/yourcompany',
    github: 'https://github.com/yourcompany',
    linkedin: 'https://linkedin.com/company/yourcompany',
  },

  // Feature flags
  features: {
    darkMode: process.env['NEXT_PUBLIC_ENABLE_DARK_MODE'] === 'true',
    i18n: process.env['NEXT_PUBLIC_ENABLE_I18N'] === 'true',
  },
} as const;

export type SiteConfig = typeof siteConfig;
