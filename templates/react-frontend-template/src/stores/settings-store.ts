import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

type Locale = 'en' | 'es' | 'fr' | 'de' | 'pt';
type Theme = 'light' | 'dark' | 'system';

/**
 * Settings Store interface
 */
interface SettingsState {
  // Theme
  theme: Theme;
  setTheme: (theme: Theme) => void;

  // Locale
  locale: Locale;
  setLocale: (locale: Locale) => void;

  // Display preferences
  compactMode: boolean;
  setCompactMode: (compact: boolean) => void;

  // Notification preferences
  notifications: {
    email: boolean;
    push: boolean;
    inApp: boolean;
  };
  setNotificationPreference: (
    type: keyof SettingsState['notifications'],
    enabled: boolean
  ) => void;

  // Table preferences
  tablePageSize: number;
  setTablePageSize: (size: number) => void;

  // Reset to defaults
  resetToDefaults: () => void;
}

const defaultSettings = {
  theme: 'system' as Theme,
  locale: 'en' as Locale,
  compactMode: false,
  notifications: {
    email: true,
    push: true,
    inApp: true,
  },
  tablePageSize: 10,
};

/**
 * Settings Store for user preferences
 * Persisted to localStorage
 */
export const useSettingsStore = create<SettingsState>()(
  persist(
    (set) => ({
      ...defaultSettings,

      setTheme: (theme) => set({ theme }),

      setLocale: (locale) => set({ locale }),

      setCompactMode: (compactMode) => set({ compactMode }),

      setNotificationPreference: (type, enabled) =>
        set((state) => ({
          notifications: {
            ...state.notifications,
            [type]: enabled,
          },
        })),

      setTablePageSize: (tablePageSize) => set({ tablePageSize }),

      resetToDefaults: () => set(defaultSettings),
    }),
    {
      name: 'settings-storage',
      storage: createJSONStorage(() => localStorage),
    }
  )
);

export default useSettingsStore;
