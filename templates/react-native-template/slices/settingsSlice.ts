import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import type { RootState } from '@/store';

type ThemeMode = 'light' | 'dark' | 'system';

interface SettingsState {
  themeMode: ThemeMode;
  notifications: {
    push: boolean;
    email: boolean;
  };
  language: string;
}

const initialState: SettingsState = {
  themeMode: 'system',
  notifications: {
    push: true,
    email: true,
  },
  language: 'en',
};

const settingsSlice = createSlice({
  name: 'settings',
  initialState,
  reducers: {
    setThemeMode: (state, action: PayloadAction<ThemeMode>) => {
      state.themeMode = action.payload;
    },
    setPushNotifications: (state, action: PayloadAction<boolean>) => {
      state.notifications.push = action.payload;
    },
    setEmailNotifications: (state, action: PayloadAction<boolean>) => {
      state.notifications.email = action.payload;
    },
    setLanguage: (state, action: PayloadAction<string>) => {
      state.language = action.payload;
    },
    resetSettings: () => initialState,
  },
});

export const {
  setThemeMode,
  setPushNotifications,
  setEmailNotifications,
  setLanguage,
  resetSettings,
} = settingsSlice.actions;

// Selectors
export const selectThemeMode = (state: RootState) => state.settings.themeMode;
export const selectNotifications = (state: RootState) => state.settings.notifications;
export const selectLanguage = (state: RootState) => state.settings.language;

export default settingsSlice.reducer;
