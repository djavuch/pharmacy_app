import { create } from "zustand";
import { UserProfileDto } from "@/shared/types";
import { accountApi, userApi, setTokens, clearTokens, getAccessToken } from "@/shared/api";
import { useCartStore } from "@/shared/stores/cart";

interface AuthState {
  profile: UserProfileDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  /** Login and store tokens */
  login: (email: string, password: string, rememberMe?: boolean) => Promise<void>;

  /** Register and store tokens */
  register: (
    email: string,
    password: string,
    confirmPassword: string,
    firstName: string,
    lastName: string,
    dateOfBirth: string,
    phoneNumber?: string
  ) => Promise<void>;

  /** Load profile from server */
  loadProfile: () => Promise<void>;

  /** Logout and clear state */
  logout: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  profile: null,
  isAuthenticated: false,
  isLoading: false,

  login: async (email, password, rememberMe = false) => {
    set({ isLoading: true });
    try {
      const result = await accountApi.login({ email, password, rememberMe });
      if (!result.token || !result.refreshToken) {
        set({ isLoading: false });
        throw new Error(result.message || "Login failed");
      }
      setTokens(result.token, result.refreshToken);
      set({ isLoading: false });

      await get().loadProfile();
      await useCartStore.getState().loadCart();
    } catch (error) {
      set({ isLoading: false });
      throw error;
    }
  },

  register: async (email, password, confirmPassword, firstName, lastName, dateOfBirth, phoneNumber) => {
    set({ isLoading: true });
    try {
      await accountApi.register({
        email,
        password,
        confirmPassword,
        firstName,
        lastName,
        dateOfBirth,
        phoneNumber,
      });
      set({ isLoading: false });
    } catch (error) {
      set({ isLoading: false });
      throw error;
    }
  },

  loadProfile: async () => {
    const token = await getAccessToken();
    if (!token) {
      set({ profile: null, isAuthenticated: false });
      return;
    }
    try {
      const profile = await userApi.getProfile();
      set({ profile, isAuthenticated: true });
    } catch {
      set({ profile: null, isAuthenticated: false });
    }
  },

  logout: async () => {
    const refreshToken = await (async () => {
      if (typeof window === "undefined") return null;
      return localStorage.getItem("refreshToken");
    })();
    if (refreshToken) {
      try {
        await accountApi.logout(refreshToken);
      } catch {
        // Ignore logout errors
      }
    }
    clearTokens();
    set({ profile: null, isAuthenticated: false });
    await useCartStore.getState().loadCart();
  },
}));
