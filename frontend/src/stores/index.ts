import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AccountInfo } from '@azure/msal-browser';
import type { User } from '@/types';
import { authApi } from '@/lib/api';

interface AuthState {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  loginWithMicrosoft: (msalAccount: AccountInfo, accessToken?: string) => void;
  register: (data: { email: string; password: string; firstName: string; lastName: string; role?: string }) => Promise<void>;
  logout: () => void;
  setUser: (user: User) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isLoading: false,
      login: async (email, password) => {
        set({ isLoading: true });
        try {
          const { token, user } = await authApi.login(email, password);
          localStorage.setItem('rentthings_token', token);
          set({ token, user, isLoading: false });
        } catch {
          set({ isLoading: false });
          throw new Error('Invalid credentials');
        }
      },
      loginWithMicrosoft: (msalAccount, accessToken) => {
        const displayName = msalAccount.name ?? msalAccount.username;
        const nameParts = displayName.trim().split(/\s+/);
        const firstName = nameParts[0] ?? msalAccount.username;
        const lastName = nameParts.slice(1).join(' ') || firstName;
        const token = accessToken ?? 'msal-session';

        localStorage.setItem('rentthings_token', token);
        set({
          token,
          user: {
            id: msalAccount.localAccountId,
            email: msalAccount.username,
            firstName,
            lastName,
            role: 'Renter',
            trustScore: 0,
            trustLevel: 'Bronze',
            isVerified: true,
            createdAt: new Date().toISOString(),
          },
        });
      },
      register: async (data) => {
        set({ isLoading: true });
        try {
          const { token, user } = await authApi.register(data);
          localStorage.setItem('rentthings_token', token);
          set({ token, user, isLoading: false });
        } catch {
          set({ isLoading: false });
          throw new Error('Registration failed');
        }
      },
      logout: () => {
        localStorage.removeItem('rentthings_token');
        set({ user: null, token: null });
      },
      setUser: (user) => set({ user }),
    }),
    { name: 'rentthings-auth', partialize: (s) => ({ user: s.user, token: s.token }) },
  ),
);

interface UiState {
  viewMode: 'grid' | 'list';
  sidebarOpen: boolean;
  setViewMode: (mode: 'grid' | 'list') => void;
  toggleSidebar: () => void;
}

export const useUiStore = create<UiState>((set) => ({
  viewMode: 'grid',
  sidebarOpen: false,
  setViewMode: (mode) => set({ viewMode: mode }),
  toggleSidebar: () => set((s) => ({ sidebarOpen: !s.sidebarOpen })),
}));
