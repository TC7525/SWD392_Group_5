/* oxlint-disable react/only-export-components */
import React, { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { User, LoginRequest, LoginResponse, ApiError } from '../types/models';
import { authApi, DEMO_PERSONAS, type DemoPersonaRole } from '../api/authApi';

export interface AuthContextType {
  currentUser: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  login: (credentials: LoginRequest) => Promise<LoginResponse>;
  quickLogin: (role: DemoPersonaRole) => Promise<LoginResponse>;
  logout: () => void;
  hasRole: (roleName: string) => boolean;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const TOKEN_KEY = 'token';
const USER_KEY = 'ewms_user';

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [currentUser, setCurrentUser] = useState<User | null>(() => {
    try {
      const savedUser = localStorage.getItem(USER_KEY) || localStorage.getItem('user');
      return savedUser ? (JSON.parse(savedUser) as User) : null;
    } catch {
      return null;
    }
  });

  const [token, setToken] = useState<string | null>(() => {
    return localStorage.getItem(TOKEN_KEY) || localStorage.getItem('ewms_token');
  });

  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (token) {
      localStorage.setItem(TOKEN_KEY, token);
    } else {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem('ewms_token');
    }
  }, [token]);

  useEffect(() => {
    if (currentUser) {
      localStorage.setItem(USER_KEY, JSON.stringify(currentUser));
      localStorage.setItem('user', JSON.stringify(currentUser));
    } else {
      localStorage.removeItem(USER_KEY);
      localStorage.removeItem('user');
    }
  }, [currentUser]);

  const clearError = () => setError(null);

  const handleAuthSuccess = (data: LoginResponse): LoginResponse => {
    setToken(data.token);
    setCurrentUser(data.user);
    setError(null);
    return data;
  };

  const login = async (credentials: LoginRequest): Promise<LoginResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await authApi.login(credentials);
      return handleAuthSuccess(response.data);
    } catch (err: any) {
      const message = (err as ApiError)?.message || 'Đăng nhập thất bại';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const quickLogin = async (role: DemoPersonaRole): Promise<LoginResponse> => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await authApi.quickLogin(role);
      return handleAuthSuccess(response.data);
    } catch (err: any) {
      const message = (err as ApiError)?.message || 'Đăng nhập nhanh thất bại';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    setToken(null);
    setCurrentUser(null);
    setError(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem('ewms_token');
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem('user');
  };

  const hasRole = (roleName: string): boolean => {
    if (!currentUser) return false;
    const target = roleName.trim().toLowerCase();
    if (currentUser.role && currentUser.role.trim().toLowerCase() === target) {
      return true;
    }
    if (Array.isArray(currentUser.roles)) {
      return currentUser.roles.some((r) => r.trim().toLowerCase() === target);
    }
    return false;
  };

  const value: AuthContextType = {
    currentUser,
    token,
    isAuthenticated: !!token && !!currentUser,
    isLoading,
    error,
    login,
    quickLogin,
    logout,
    hasRole,
    clearError,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export { DEMO_PERSONAS };
export type { DemoPersonaRole };
