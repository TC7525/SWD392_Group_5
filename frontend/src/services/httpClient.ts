import { type ApiResponse } from '../types/auth.types';

export const API_BASE_URL =
  import.meta.env.VITE_API_URL || 'http://localhost:5150';

export async function httpRequest<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = localStorage.getItem('ewms_token');
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}/${path}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    const errorText = await response.text();
    let errorMessage = errorText || `HTTP Error ${response.status}`;
    try {
      const parsed = JSON.parse(errorText);
      errorMessage =
        parsed.message ||
        parsed.Message ||
        parsed.detail ||
        parsed.title ||
        errorText;
    } catch {
      // plain text error message from BadException / NotFoundException
    }
    throw new Error(errorMessage);
  }

  const json = (await response.json()) as ApiResponse<T>;
  const data = (json.result !== undefined ? json.result : json.Result) as any;

  // Normalize camelCase `items` and PascalCase `Items` in PagedResponse<T>
  if (data && typeof data === 'object') {
    if (Array.isArray(data.items) && !data.Items) {
      data.Items = data.items;
    } else if (Array.isArray(data.Items) && !data.items) {
      data.items = data.Items;
    }
  }

  return data as T;
}
