import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

/**
 * Sidebar state
 */
interface SidebarState {
  isOpen: boolean;
  isCollapsed: boolean;
}

/**
 * Modal state
 */
interface ModalState {
  isOpen: boolean;
  title?: string;
  content?: React.ReactNode;
}

/**
 * UI Store interface
 */
interface UIState {
  // Sidebar
  sidebar: SidebarState;
  setSidebarOpen: (isOpen: boolean) => void;
  setSidebarCollapsed: (isCollapsed: boolean) => void;
  toggleSidebar: () => void;

  // Modal
  modal: ModalState;
  openModal: (title: string, content?: React.ReactNode) => void;
  closeModal: () => void;

  // Command palette
  isCommandPaletteOpen: boolean;
  setCommandPaletteOpen: (isOpen: boolean) => void;
  toggleCommandPalette: () => void;

  // Mobile menu
  isMobileMenuOpen: boolean;
  setMobileMenuOpen: (isOpen: boolean) => void;
  toggleMobileMenu: () => void;

  // Sheet (drawer)
  isSheetOpen: boolean;
  sheetContent: React.ReactNode | null;
  openSheet: (content: React.ReactNode) => void;
  closeSheet: () => void;
}

/**
 * UI Store for client-side UI state
 * Persisted to localStorage for consistent UX across sessions
 */
export const useUIStore = create<UIState>()(
  persist(
    (set) => ({
      // Sidebar
      sidebar: {
        isOpen: true,
        isCollapsed: false,
      },
      setSidebarOpen: (isOpen) =>
        set((state) => ({
          sidebar: { ...state.sidebar, isOpen },
        })),
      setSidebarCollapsed: (isCollapsed) =>
        set((state) => ({
          sidebar: { ...state.sidebar, isCollapsed },
        })),
      toggleSidebar: () =>
        set((state) => ({
          sidebar: { ...state.sidebar, isOpen: !state.sidebar.isOpen },
        })),

      // Modal
      modal: {
        isOpen: false,
        title: undefined,
        content: undefined,
      },
      openModal: (title, content) =>
        set({
          modal: { isOpen: true, title, content },
        }),
      closeModal: () =>
        set({
          modal: { isOpen: false, title: undefined, content: undefined },
        }),

      // Command palette
      isCommandPaletteOpen: false,
      setCommandPaletteOpen: (isOpen) => set({ isCommandPaletteOpen: isOpen }),
      toggleCommandPalette: () =>
        set((state) => ({
          isCommandPaletteOpen: !state.isCommandPaletteOpen,
        })),

      // Mobile menu
      isMobileMenuOpen: false,
      setMobileMenuOpen: (isOpen) => set({ isMobileMenuOpen: isOpen }),
      toggleMobileMenu: () =>
        set((state) => ({
          isMobileMenuOpen: !state.isMobileMenuOpen,
        })),

      // Sheet
      isSheetOpen: false,
      sheetContent: null,
      openSheet: (content) =>
        set({ isSheetOpen: true, sheetContent: content }),
      closeSheet: () => set({ isSheetOpen: false, sheetContent: null }),
    }),
    {
      name: 'ui-storage',
      storage: createJSONStorage(() => localStorage),
      // Only persist sidebar state
      partialize: (state) => ({
        sidebar: state.sidebar,
      }),
    }
  )
);

export default useUIStore;
