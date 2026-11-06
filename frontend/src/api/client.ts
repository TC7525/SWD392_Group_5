import axios, { type AxiosResponse } from 'axios';
import type { ApiError } from '../types/models';

const envUrl = (import.meta.env.VITE_API_URL as string | undefined)?.trim();
const rawBase = envUrl && envUrl.length > 0 ? envUrl : 'http://localhost:5000';
const baseURL = rawBase.endsWith('/api') ? rawBase : `${rawBase.replace(/\/+$/, '')}/api`;

export const apiClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
});

apiClient.interceptors.request.use(
  (config) => {
    const token =
      localStorage.getItem('token') ||
      localStorage.getItem('ewms_token') ||
      localStorage.getItem('accessToken');

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    // Backend wraps data in ApiResponse<T>: { code, message, data }
    return response.data;
  },
  (error) => {
    const errorData = error.response?.data;
    const formattedError: ApiError = {
      code: errorData?.code || (error.response ? `HTTP_${error.response.status}` : 'ERR_NETWORK'),
      message:
        errorData?.message ||
        error.message ||
        'An unexpected server error occurred.',
      status: error.response?.status,
      errors: errorData?.errors,
    };
    return Promise.reject(formattedError);
  }
);

export default apiClient;
