// Minimalist Modern Theme - Design Tokens

// Color Palette
const palette = {
  // Primary - Blue
  primary: {
    50: '#EFF6FF',
    100: '#DBEAFE',
    200: '#BFDBFE',
    300: '#93C5FD',
    400: '#60A5FA',
    500: '#3B82F6',
    600: '#2563EB',
    700: '#1D4ED8',
    800: '#1E40AF',
    900: '#1E3A8A',
  },
  // Neutral - Gray
  neutral: {
    0: '#FFFFFF',
    50: '#FAFAFA',
    100: '#F5F5F5',
    200: '#E5E5E5',
    300: '#D4D4D4',
    400: '#A3A3A3',
    500: '#737373',
    600: '#525252',
    700: '#404040',
    800: '#262626',
    900: '#171717',
    950: '#0A0A0A',
  },
  // Semantic
  success: {
    light: '#DCFCE7',
    main: '#22C55E',
    dark: '#15803D',
  },
  warning: {
    light: '#FEF3C7',
    main: '#F59E0B',
    dark: '#B45309',
  },
  error: {
    light: '#FEE2E2',
    main: '#EF4444',
    dark: '#B91C1C',
  },
  info: {
    light: '#DBEAFE',
    main: '#3B82F6',
    dark: '#1D4ED8',
  },
};

// Light Theme Colors
export const lightColors = {
  background: {
    primary: palette.neutral[0],
    secondary: palette.neutral[50],
    tertiary: palette.neutral[100],
  },
  text: {
    primary: palette.neutral[900],
    secondary: palette.neutral[600],
    tertiary: palette.neutral[400],
    inverse: palette.neutral[0],
  },
  interactive: {
    primary: palette.primary[500],
    primaryHover: palette.primary[600],
    secondary: palette.neutral[200],
    secondaryHover: palette.neutral[300],
    disabled: palette.neutral[200],
  },
  border: {
    default: palette.neutral[200],
    focus: palette.primary[500],
  },
  semantic: {
    success: palette.success.main,
    warning: palette.warning.main,
    error: palette.error.main,
    info: palette.info.main,
  },
};

// Dark Theme Colors
export const darkColors = {
  background: {
    primary: palette.neutral[950],
    secondary: palette.neutral[900],
    tertiary: palette.neutral[800],
  },
  text: {
    primary: palette.neutral[50],
    secondary: palette.neutral[400],
    tertiary: palette.neutral[500],
    inverse: palette.neutral[900],
  },
  interactive: {
    primary: palette.primary[500],
    primaryHover: palette.primary[400],
    secondary: palette.neutral[700],
    secondaryHover: palette.neutral[600],
    disabled: palette.neutral[700],
  },
  border: {
    default: palette.neutral[700],
    focus: palette.primary[500],
  },
  semantic: {
    success: palette.success.main,
    warning: palette.warning.main,
    error: palette.error.main,
    info: palette.info.main,
  },
};

// Spacing (4px base scale)
export const spacing = {
  xxs: 2,
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
  xxl: 48,
  xxxl: 64,
};

// Typography
export const typography = {
  fontFamily: {
    regular: 'System',
    medium: 'System',
    semibold: 'System',
    bold: 'System',
  },
  fontSize: {
    xs: 12,
    sm: 14,
    md: 16,
    lg: 18,
    xl: 20,
    '2xl': 24,
    '3xl': 30,
    '4xl': 36,
  },
  lineHeight: {
    tight: 1.25,
    normal: 1.5,
    relaxed: 1.75,
  },
};

// Border Radius
export const radius = {
  none: 0,
  sm: 4,
  md: 8,
  lg: 12,
  xl: 16,
  full: 9999,
};

// Shadows
export const shadows = {
  none: {},
  sm: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.05,
    shadowRadius: 2,
    elevation: 1,
  },
  md: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.08,
    shadowRadius: 4,
    elevation: 3,
  },
  lg: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.1,
    shadowRadius: 8,
    elevation: 5,
  },
};

// Theme Type
export interface Theme {
  colors: typeof lightColors;
  spacing: typeof spacing;
  typography: typeof typography;
  radius: typeof radius;
  shadows: typeof shadows;
  isDark: boolean;
}

// Light Theme
export const lightTheme: Theme = {
  colors: lightColors,
  spacing,
  typography,
  radius,
  shadows,
  isDark: false,
};

// Dark Theme
export const darkTheme: Theme = {
  colors: darkColors,
  spacing,
  typography,
  radius,
  shadows,
  isDark: true,
};
