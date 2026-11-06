import apiClient from './client';
import type { ApiResponse, LoginRequest, LoginResponse } from '../types/models';

export type DemoPersonaRole = 'purchase_staff' | 'warehouse_manager' | 'warehouse_staff';

export interface DemoPersona {
  roleKey: DemoPersonaRole;
  label: string;
  email: string;
  password: string;
  roleName: string;
  employeeCode: string;
}

export const DEMO_PERSONAS: Record<DemoPersonaRole, DemoPersona> = {
  purchase_staff: {
    roleKey: 'purchase_staff',
    label: 'Nhân viên mua hàng (Purchase Staff)',
    email: 'staff_purchase@ewms.com',
    password: '123456',
    roleName: 'Purchase Staff',
    employeeCode: 'EMP-PUR-01',
  },
  warehouse_manager: {
    roleKey: 'warehouse_manager',
    label: 'Quản lý kho (Warehouse Manager)',
    email: 'manager_kho@ewms.com',
    password: '123456',
    roleName: 'Warehouse Manager',
    employeeCode: 'EMP-MGR-01',
  },
  warehouse_staff: {
    roleKey: 'warehouse_staff',
    label: 'Thủ kho (Warehouse Staff)',
    email: 'staff_kho@ewms.com',
    password: '123456',
    roleName: 'Warehouse Staff',
    employeeCode: 'EMP-STF-01',
  },
};

export const authApi = {
  login: async (request: LoginRequest): Promise<ApiResponse<LoginResponse>> => {
    const res = await apiClient.post<any, ApiResponse<LoginResponse>>('/auth/login', request);
    return res;
  },

  quickLogin: async (role: DemoPersonaRole): Promise<ApiResponse<LoginResponse>> => {
    const persona = DEMO_PERSONAS[role];
    if (!persona) {
      throw new Error(`Unknown persona role: ${role}`);
    }
    return authApi.login({ email: persona.email, password: persona.password });
  },
};

export default authApi;
