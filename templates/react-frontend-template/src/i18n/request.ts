import { getRequestConfig } from 'next-intl/server';
import { headers } from 'next/headers';

/**
 * Supported locales
 */
export const locales = ['en', 'es', 'fr', 'de', 'pt'] as const;
export type Locale = (typeof locales)[number];
export const defaultLocale: Locale = 'en';

/**
 * Get locale from Accept-Language header or default
 */
function getLocaleFromHeaders(): Locale {
  try {
    const headersList = headers();
    const acceptLanguage = headersList.get('accept-language');

    if (acceptLanguage) {
      // Parse Accept-Language header and find first matching locale
      const preferredLocales = acceptLanguage
        .split(',')
        .map((lang) => lang.split(';')[0]?.trim().split('-')[0])
        .filter(Boolean);

      for (const preferred of preferredLocales) {
        if (locales.includes(preferred as Locale)) {
          return preferred as Locale;
        }
      }
    }
  } catch {
    // Headers not available (e.g., during static generation)
  }

  return defaultLocale;
}

export default getRequestConfig(async () => {
  const locale = getLocaleFromHeaders();

  return {
    locale,
    messages: (await import(`./messages/${locale}.json`)).default,
  };
});
