import { type LoginResponse, type UserProfile } from '../types/auth.types';
import { httpRequest } from './httpClient';

export const authService = {
  login: (username: string, password: string, warehouse_id?: string) =>
    httpRequest<LoginResponse>('api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password, warehouse_id }),
    }),

  getProfile: () => httpRequest<UserProfile>('api/auth/profile'),

  switchWarehouse: (warehouse_id: string) =>
    httpRequest<LoginResponse>('api/auth/switch-warehouse', {
      method: 'POST',
      body: JSON.stringify({ warehouse_id }),
    }),
};
