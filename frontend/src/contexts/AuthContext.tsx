import React, { createContext, useContext, useEffect, useState } from 'react';
import { authService } from '../services/authService';
import { type UserProfile } from '../types/auth.types';

interface AuthContextValue {
  user: UserProfile | null;
  loading: boolean;
  errorMsg: string | null;
  successMsg: string | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  switchWarehouse: (warehouseId: string) => Promise<void>;
  notifySuccess: (msg: string) => void;
  notifyError: (msg: string | null) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('ewms_token');
    if (token) {
      authService
        .getProfile()
        .then((profile) => setUser(profile))
        .catch(() => localStorage.removeItem('ewms_token'));
    }
  }, []);

  const notifySuccess = (msg: string) => {
    setErrorMsg(null);
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(null), 4000);
  };

  const notifyError = (msg: string | null) => {
    setErrorMsg(msg);
  };

  const login = async (username: string, password: string) => {
    setErrorMsg(null);
    setLoading(true);
    try {
      const res = await authService.login(username, password);
      localStorage.setItem('ewms_token', res.access_token);
      setUser(res.user);
      notifySuccess(`Xin chào ${res.user.full_name} (${res.user.role_name})!`);
    } catch (err: any) {
      setErrorMsg(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    localStorage.removeItem('ewms_token');
    setUser(null);
  };

  const switchWarehouse = async (warehouseId: string) => {
    try {
      const res = await authService.switchWarehouse(warehouseId);
      localStorage.setItem('ewms_token', res.access_token);
      setUser(res.user);
      notifySuccess(`Đã chuyển sang kho: ${res.user.current_warehouse_name}`);
    } catch (err: any) {
      setErrorMsg(err.message);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        errorMsg,
        successMsg,
        login,
        logout,
        switchWarehouse,
        notifySuccess,
        notifyError,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuth must be used inside AuthProvider');
  }
  return ctx;
};
