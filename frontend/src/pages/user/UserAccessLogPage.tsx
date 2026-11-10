import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { Icon } from '../../components/common/Icon';
import { useAuth } from '../../contexts/AuthContext';
import { masterDataService } from '../../services/masterDataService';
import {
  type AccountItem,
  type PermissionItem,
  type RoleItem,
  type WarehouseItem,
} from '../../types/master.types';

interface MatrixDefinition {
  moduleKey: string;
  moduleName: string;
  group: string;
  viewCode: string;
  editCode?: string;
  exportCode?: string;
  approveCode?: string;
}

const MATRIX_DEFINITIONS: MatrixDefinition[] = [
  {
    moduleKey: 'DASHBOARD',
    moduleName: 'Tổng quan',
    group: 'TỔNG QUAN',
    viewCode: 'DASHBOARD.VIEW',
  },
  {
    moduleKey: 'PRODUCT',
    moduleName: 'Hàng hóa',
    group: 'KHAI BÁO',
    viewCode: 'PRODUCT.VIEW',
    editCode: 'PRODUCT.MANAGE',
    exportCode: 'PRODUCT.EXPORT',
  },
  {
    moduleKey: 'WAREHOUSE',
    moduleName: 'Kho hàng',
    group: 'KHAI BÁO',
    viewCode: 'WAREHOUSE.VIEW',
    editCode: 'WAREHOUSE.MANAGE',
    exportCode: 'WAREHOUSE.EXPORT',
  },
  {
    moduleKey: 'PARTNER',
    moduleName: 'Đối tượng (NCC / Khách hàng)',
    group: 'KHAI BÁO',
    viewCode: 'PARTNER.VIEW',
    editCode: 'PARTNER.MANAGE',
    exportCode: 'PARTNER.EXPORT',
  },
  {
    moduleKey: 'PURCHASE_ORDER',
    moduleName: 'Nhập kho',
    group: 'NGHIỆP VỤ',
    viewCode: 'PO.VIEW',
    editCode: 'PO.MANAGE',
    exportCode: 'PO.EXPORT',
    approveCode: 'PO.APPROVE',
  },
  {
    moduleKey: 'OUTBOUND_ORDER',
    moduleName: 'Xuất kho',
    group: 'NGHIỆP VỤ',
    viewCode: 'SO.VIEW',
    editCode: 'SO.MANAGE',
    exportCode: 'SO.EXPORT',
    approveCode: 'SO.APPROVE',
  },
  {
    moduleKey: 'STOCK_TRANSFER',
    moduleName: 'Chuyển kho',
    group: 'NGHIỆP VỤ',
    viewCode: 'TRF.VIEW',
    editCode: 'TRF.MANAGE',
    exportCode: 'TRF.EXPORT',
    approveCode: 'TRF.APPROVE',
  },
  {
    moduleKey: 'INVENTORY',
    moduleName: 'Tồn kho',
    group: 'NGHIỆP VỤ',
    viewCode: 'INVENTORY.VIEW',
    editCode: 'INVENTORY.MANAGE',
    exportCode: 'INVENTORY.EXPORT',
  },
  {
    moduleKey: 'REPORT',
    moduleName: 'Báo cáo N-X-T',
    group: 'BÁO CÁO',
    viewCode: 'REPORT.VIEW',
    exportCode: 'REPORT.EXPORT',
  },
  {
    moduleKey: 'AUDIT_LOG',
    moduleName: 'Lịch sử & Nhật ký',
    group: 'BÁO CÁO',
    viewCode: 'AUDIT_LOG.VIEW',
    exportCode: 'AUDIT_LOG.EXPORT',
  },
];

interface PermissionMatrixRow {
  module: string;
  group: string;
  canView: boolean;
  canEdit: boolean;
  canExport: boolean;
  canApprove: boolean;
  hasEdit: boolean;
  hasExport: boolean;
  hasApprove: boolean;
}

export const UserAccessLogPage: React.FC = () => {
  const { user, notifyError, notifySuccess } = useAuth();

  // Navigation tab: 'accounts' or 'roles'
  const [activeTab, setActiveTab] = useState<'accounts' | 'roles'>('accounts');

  const [accounts, setAccounts] = useState<AccountItem[]>([]);
  const [roles, setRoles] = useState<RoleItem[]>([]);
  const [allPermissions, setAllPermissions] = useState<PermissionItem[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseItem[]>([]);
  const [pageSize, setPageSize] = useState('10');
  const [loading, setLoading] = useState(false);
  const [savingRole, setSavingRole] = useState(false);

  // Selected role in Role Management Tab
  const [selectedRoleId, setSelectedRoleId] = useState<string>('');
  const [roleMatrix, setRoleMatrix] = useState<PermissionMatrixRow[]>([]);

  // Modal Cập nhật / Thêm Tài Khoản
  const [showModal, setShowModal] = useState(false);
  const [editingAcc, setEditingAcc] = useState<AccountItem | null>(null);
  const [empCode, setEmpCode] = useState('');
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('Admin@123');
  const [roleId, setRoleId] = useState('');
  const [selectedWhId, setSelectedWhId] = useState('ALL');
  const [status, setStatus] = useState(1);
  const [modalMatrix, setModalMatrix] = useState<PermissionMatrixRow[]>([]);

  // Build matrix rows given active permission codes
  const buildMatrixFromPerms = useCallback(
    (rolePermCodes: Set<string>, isAdmin: boolean = false): PermissionMatrixRow[] => {
      return MATRIX_DEFINITIONS.map((def) => ({
        module: def.moduleName,
        group: def.group,
        canView: isAdmin || (def.viewCode ? rolePermCodes.has(def.viewCode) : false),
        canEdit: isAdmin || (def.editCode ? rolePermCodes.has(def.editCode) : false),
        canExport: isAdmin || (def.exportCode ? rolePermCodes.has(def.exportCode) : false),
        canApprove: isAdmin || (def.approveCode ? rolePermCodes.has(def.approveCode) : false),
        hasEdit: Boolean(def.editCode),
        hasExport: Boolean(def.exportCode),
        hasApprove: Boolean(def.approveCode),
      }));
    },
    []
  );

  // Extract selected permission IDs from matrix
  const extractPermIdsFromMatrix = useCallback(
    (matrixRows: PermissionMatrixRow[]): string[] => {
      const codeToIdMap = new Map<string, string>();
      for (const p of allPermissions) {
        codeToIdMap.set(p.permission_code, p.id);
      }

      const selectedCodes = new Set<string>();

      for (const row of matrixRows) {
        const def = MATRIX_DEFINITIONS.find((d) => d.moduleName === row.module);
        if (!def) continue;

        if (row.canView && def.viewCode) selectedCodes.add(def.viewCode);
        if (row.canEdit && def.editCode) selectedCodes.add(def.editCode);
        if (row.canExport && def.exportCode) selectedCodes.add(def.exportCode);
        if (row.canApprove && def.approveCode) selectedCodes.add(def.approveCode);
      }

      const ids: string[] = [];
      for (const code of selectedCodes) {
        const id = codeToIdMap.get(code);
        if (id) ids.push(id);
      }
      return ids;
    },
    [allPermissions]
  );

  // Load All System Data
  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [accRes, roleRes, whRes, permRes] = await Promise.all([
        masterDataService.searchAccounts(),
        masterDataService.searchRoles(),
        masterDataService.searchWarehouses(),
        masterDataService.getPermissions().catch(() => []),
      ]);

      const accList = accRes.items || [];
      const roleList = roleRes.items || [];
      const whList = whRes.items || [];
      const permList = Array.isArray(permRes) ? permRes : [];

      setAccounts(accList);
      setRoles(roleList);
      setWarehouses(whList);
      setAllPermissions(permList);

      if (roleList.length > 0 && !selectedRoleId) {
        setSelectedRoleId(roleList[0].id);
      }
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi tải dữ liệu hệ thống');
    } finally {
      setLoading(false);
    }
  }, [selectedRoleId, notifyError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Load selected role details for Role Management Tab
  useEffect(() => {
    if (!selectedRoleId || roles.length === 0) return;

    const r = roles.find((x) => x.id === selectedRoleId);
    if (!r) return;

    masterDataService
      .getRoleById(selectedRoleId)
      .then((detail) => {
        const permCodes = new Set<string>(
          (detail.permissions || []).map((p) => p.permission_code)
        );
        const isAdmin = detail.role_code === 'ADMIN';
        setRoleMatrix(buildMatrixFromPerms(permCodes, isAdmin));
      })
      .catch(() => {
        const permCodes = new Set<string>(
          (r.permissions || []).map((p) => p.permission_code)
        );
        const isAdmin = r.role_code === 'ADMIN';
        setRoleMatrix(buildMatrixFromPerms(permCodes, isAdmin));
      });
  }, [selectedRoleId, roles, buildMatrixFromPerms]);

  // Update modal matrix when modal role selection changes
  const updateModalMatrixForRole = useCallback(
    async (rId: string) => {
      if (!rId) return;
      try {
        const detail = await masterDataService.getRoleById(rId);
        const permCodes = new Set<string>(
          (detail.permissions || []).map((p) => p.permission_code)
        );
        const isAdmin = detail.role_code === 'ADMIN';
        setModalMatrix(buildMatrixFromPerms(permCodes, isAdmin));
      } catch {
        const found = roles.find((r) => r.id === rId);
        const permCodes = new Set<string>(
          (found?.permissions || []).map((p) => p.permission_code)
        );
        const isAdmin = found?.role_code === 'ADMIN';
        setModalMatrix(buildMatrixFromPerms(permCodes, isAdmin));
      }
    },
    [roles, buildMatrixFromPerms]
  );

  const openCreateModal = () => {
    setEditingAcc(null);
    setEmpCode('');
    setFullName('');
    setEmail('');
    setPassword('Admin@123');
    const initialRoleId = roles[0]?.id || '';
    setRoleId(initialRoleId);
    setSelectedWhId('ALL');
    setStatus(1);
    updateModalMatrixForRole(initialRoleId);
    setShowModal(true);
  };

  const openEditModal = (acc: AccountItem) => {
    setEditingAcc(acc);
    setEmpCode(acc.employee_code);
    setFullName(acc.full_name);
    setEmail(acc.email);
    setPassword('••••••••');
    const currentRoleId = acc.role_id || roles[0]?.id || '';
    setRoleId(currentRoleId);
    setSelectedWhId(
      acc.assigned_warehouses.length >= warehouses.length
        ? 'ALL'
        : acc.assigned_warehouses[0]?.warehouse_id || 'ALL'
    );
    setStatus(acc.status);
    updateModalMatrixForRole(currentRoleId);
    setShowModal(true);
  };

  const handleRoleChangeInModal = (newRoleId: string) => {
    setRoleId(newRoleId);
    updateModalMatrixForRole(newRoleId);
  };

  // Save Account (Update or Create)
  const handleSaveAccount = async (e: React.FormEvent, forceCreate = false) => {
    e.preventDefault();
    if (!empCode.trim()) {
      notifyError('Vui lòng nhập Tên đăng nhập (Mã NV)');
      return;
    }
    if (!fullName.trim()) {
      notifyError('Vui lòng nhập Họ và tên nhân viên');
      return;
    }
    try {
      const whIds =
        selectedWhId === 'ALL' ? warehouses.map((w) => w.id) : [selectedWhId];

      const isUpdate = Boolean(editingAcc) && !forceCreate;

      if (isUpdate && editingAcc) {
        await masterDataService.updateAccount({
          id: editingAcc.id,
          employee_code: empCode.trim(),
          full_name: fullName.trim(),
          email: email.trim(),
          role_id: roleId,
          warehouse_ids: whIds,
          status,
        });
        notifySuccess(`Đã cập nhật tài khoản '${fullName}' thành công!`);
      } else {
        await masterDataService.createAccount({
          employee_code: empCode.trim() || undefined,
          full_name: fullName.trim(),
          email: email.trim() || `${empCode.trim().toLowerCase() || 'user'}@ewms.vn`,
          password: password || 'Admin@123',
          role_id: roleId,
          warehouse_ids: whIds,
          status,
        });
        notifySuccess(`Đã tạo mới tài khoản '${fullName}' thành công!`);
      }
      setShowModal(false);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi lưu tài khoản');
    }
  };

  // Save Role Permission Matrix (RBAC Update)
  const handleSaveRoleMatrix = async (
    targetRole: RoleItem,
    matrixToSave: PermissionMatrixRow[]
  ) => {
    try {
      setSavingRole(true);
      const permIds = extractPermIdsFromMatrix(matrixToSave);
      await masterDataService.updateRole({
        id: targetRole.id,
        role_code: targetRole.role_code,
        role_name: targetRole.role_name,
        description: targetRole.description,
        status: targetRole.status ?? 1,
        permission_ids: permIds,
      });
      notifySuccess(
        `Đã cập nhật ma trận phân quyền cho vai trò '${targetRole.role_name}' thành công!`
      );
      await loadData();
    } catch (err: unknown) {
      notifyError(
        err instanceof Error ? err.message : 'Lỗi cập nhật ma trận quyền vai trò'
      );
    } finally {
      setSavingRole(false);
    }
  };

  const handleDeleteAccount = async (acc: AccountItem) => {
    if (!window.confirm(`Bạn có chắc muốn xóa tài khoản "${acc.full_name}"?`))
      return;
    try {
      await masterDataService.deleteAccount(acc.id);
      notifySuccess(`Đã xóa tài khoản '${acc.employee_code}'.`);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Không thể xóa tài khoản');
    }
  };

  const activeRoleObj = useMemo(
    () => roles.find((r) => r.id === selectedRoleId),
    [roles, selectedRoleId]
  );

  const modalRoleObj = useMemo(
    () => roles.find((r) => r.id === roleId),
    [roles, roleId]
  );

  return (
    <div>
      {/* PAGE HEADER */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#10b981' }}><Icon name="shield-check" /></span> Phân Quyền & Quản Trị Hệ Thống (RBAC)
          </h1>
          <p>
            Quản lý tài khoản người dùng và thiết lập ma trận phân quyền chi tiết cho từng vai trò
          </p>
        </div>
        <div className="page-actions" style={{ display: 'flex', gap: 8 }}>
          <button
            className="btn btn-outline"
            onClick={loadData}
            title="Tải lại dữ liệu"
            disabled={loading}
          >
            <Icon name="refresh" /> {loading ? 'Đang tải...' : 'Tải lại dữ liệu'}
          </button>
          <button className="btn btn-emerald" onClick={openCreateModal}>
            <Icon name="add" /> Thêm mới tài khoản
          </button>
        </div>
      </div>

      {/* TABS NAVIGATION */}
      <div style={{ display: 'flex', gap: 10, marginBottom: 16 }}>
        <button
          className={`btn ${activeTab === 'accounts' ? 'btn-emerald' : 'btn-outline'}`}
          onClick={() => setActiveTab('accounts')}
          style={{ padding: '8px 18px', fontWeight: 600 }}
        >
          <Icon name="users" /> Danh Sách Tài Khoản ({accounts.length})
        </button>
        <button
          className={`btn ${activeTab === 'roles' ? 'btn-emerald' : 'btn-outline'}`}
          onClick={() => setActiveTab('roles')}
          style={{ padding: '8px 18px', fontWeight: 600 }}
        >
          <Icon name="shield-check" /> Quản Lý Vai Trò & Ma Trận Phân Quyền ({roles.length})
        </button>
      </div>

      {/* TAB 1: DANH SÁCH TÀI KHOẢN */}
      {activeTab === 'accounts' && (
        <div className="surface-card">
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              marginBottom: 14,
            }}
          >
            <div style={{ fontWeight: 700, fontSize: 14, color: '#0f172a' }}>
              <Icon name="users" /> Danh Sách Tài Khoản Đăng Nhập & Phân Quyền Vai Trò
            </div>
            <span className="status-pill emerald">
              <Icon name="circle-small" /> Đang trực tuyến: {user?.employee_code} ({user?.full_name})
            </span>
          </div>

          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>TÊN ĐĂNG NHẬP</th>
                  <th>HỌ VÀ TÊN NHÂN VIÊN</th>
                  <th>VAI TRÒ / CHỨC VỤ</th>
                  <th>CHI NHÁNH KHO PHỤ TRÁCH</th>
                  <th>TRẠNG THÁI</th>
                  <th>EMAIL LIÊN HỆ</th>
                  <th>PHÂN QUYỀN VAI TRÒ</th>
                  <th style={{ textAlign: 'center' }}>THAO TÁC</th>
                </tr>
              </thead>
              <tbody>
                {accounts.map((acc, idx) => {
                  const isAdmin = acc.role_code === 'ADMIN';
                  const matchedRole = roles.find((r) => r.id === acc.role_id);
                  const permCount = matchedRole?.permissions?.length ?? 0;

                  return (
                    <tr key={acc.id}>
                      <td>{idx + 1}</td>
                      <td style={{ fontWeight: 700 }}>{acc.employee_code}</td>
                      <td style={{ fontWeight: 600 }}>{acc.full_name}</td>
                      <td>
                        <span
                          className={`status-pill ${
                            isAdmin ? 'emerald' : 'blue'
                          }`}
                        >
                          {acc.role_name}
                        </span>
                      </td>
                      <td>
                        {acc.assigned_warehouses.length >= 2
                          ? 'Toàn bộ chi nhánh kho'
                          : acc.assigned_warehouses
                              .map((w) => w.warehouse_name)
                              .join(', ') || 'Toàn bộ chi nhánh kho'}
                      </td>
                      <td>
                        <span
                          className={`status-pill ${
                            acc.status === 1 ? 'emerald' : 'danger'
                          }`}
                        >
                          {acc.status_name}
                        </span>
                      </td>
                      <td style={{ color: '#64748b' }}>{acc.email}</td>
                      <td>
                        {isAdmin ? (
                            <span className="status-pill emerald">
                              <Icon name="crown" /> Toàn quyền
                            </span>
                        ) : (
                          <span style={{ display: 'inline-flex', gap: 6 }}>
                            <span className="status-pill blue">
                              {permCount > 0 ? `${permCount} Quyền` : 'Phân theo vai trò'}
                            </span>
                            <span className="status-pill warning"><Icon name="print" /> In/Excel</span>
                          </span>
                        )}
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        <div
                          style={{
                            display: 'inline-flex',
                            gap: 6,
                            alignItems: 'center',
                            justifyContent: 'center',
                          }}
                        >
                          <button
                            className="btn btn-outline btn-sm"
                            style={{
                              padding: '4px 9px',
                              fontSize: 12,
                              display: 'inline-flex',
                              alignItems: 'center',
                              gap: 4,
                            }}
                            title="Cập nhật Tài khoản & Xem ma trận quyền"
                            onClick={() => openEditModal(acc)}
                          >
                            <Icon name="pencil" /> Cập nhật
                          </button>
                          {!isAdmin && (
                            <button
                              className="icon-action-btn danger"
                              title="Xóa tài khoản"
                              onClick={() => handleDeleteAccount(acc)}
                            >
                              <Icon name="trash" />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          <div className="pagination-bar">
            <span>
              Hiển thị <strong>1 - {accounts.length}</strong> trên tổng số{' '}
              <strong>{accounts.length}</strong> dòng
            </span>
            <div className="pagination-controls">
              <span>Dòng/trang:</span>
              <CustomSelect
                variant="sm"
                value={pageSize}
                onChange={setPageSize}
                options={[
                  { value: '10', label: '10' },
                  { value: '20', label: '20' },
                ]}
              />
              <button className="page-btn" aria-label="Trang trước"><Icon name="angle-double-small-left" /></button>
              <button className="page-btn active">1</button>
              <button className="page-btn" aria-label="Trang sau"><Icon name="angle-double-small-right" /></button>
            </div>
          </div>
        </div>
      )}

      {/* TAB 2: QUẢN LÝ VAI TRÒ & MA TRẬN PHÂN QUYỀN (RBAC) */}
      {activeTab === 'roles' && (
        <div style={{ display: 'grid', gridTemplateColumns: '320px 1fr', gap: 16 }}>
          {/* Cột Trái: Danh sách vai trò */}
          <div className="surface-card" style={{ height: 'fit-content' }}>
            <div
              style={{
                fontWeight: 700,
                fontSize: 14,
                color: '#0f172a',
                marginBottom: 12,
                borderBottom: '1px solid #e2e8f0',
                paddingBottom: 8,
              }}
            >
              <Icon name="shield-check" /> Danh Sách Vai Trò Hệ Thống
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
              {roles.map((r) => {
                const isSelected = r.id === selectedRoleId;
                return (
                  <div
                    key={r.id}
                    onClick={() => setSelectedRoleId(r.id)}
                    style={{
                      padding: '12px 14px',
                      borderRadius: 8,
                      cursor: 'pointer',
                      border: isSelected
                        ? '2px solid #10b981'
                        : '1px solid #e2e8f0',
                      backgroundColor: isSelected ? '#ecfdf5' : '#ffffff',
                      transition: 'all 0.15s ease',
                    }}
                  >
                    <div
                      style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        marginBottom: 4,
                      }}
                    >
                      <strong style={{ fontSize: 13.5, color: '#0f172a' }}>
                        {r.role_name}
                      </strong>
                      <span
                        className={`status-pill ${
                          r.role_code === 'ADMIN' ? 'emerald' : 'blue'
                        }`}
                        style={{ fontSize: 10.5 }}
                      >
                        {r.role_code}
                      </span>
                    </div>
                    <div style={{ fontSize: 11.5, color: '#64748b', lineHeight: 1.4 }}>
                      {r.description || 'Không có mô tả'}
                    </div>
                    <div
                      style={{
                        marginTop: 6,
                        fontSize: 11,
                        color: '#10b981',
                        fontWeight: 600,
                      }}
                    >
                      <Icon name="users" /> {r.user_count || 0} tài khoản đang sử dụng
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Cột Phải: Ma trận phân quyền chi tiết của vai trò đang chọn */}
          <div className="surface-card">
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                marginBottom: 14,
                borderBottom: '1px solid #e2e8f0',
                paddingBottom: 10,
              }}
            >
              <div>
                <div style={{ fontWeight: 700, fontSize: 15, color: '#0f172a' }}>
                  <Icon name="settings" /> Ma Trận Phân Quyền Chi Tiết Cho: {activeRoleObj?.role_name}
                </div>
                <div style={{ fontSize: 12, color: '#64748b', marginTop: 2 }}>
                  Mã vai trò: <code>{activeRoleObj?.role_code}</code> — Mọi người dùng thuộc vai trò này sẽ tự động kế thừa phân quyền dưới đây.
                </div>
              </div>

              <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                <button
                  type="button"
                  className="btn btn-outline btn-sm"
                  onClick={() =>
                    setRoleMatrix(
                      roleMatrix.map((r) => ({
                        ...r,
                        canView: true,
                        canEdit: r.hasEdit,
                        canExport: r.hasExport,
                        canApprove: r.hasApprove,
                      }))
                    )
                  }
                >
                  <Icon name="check" /> Chọn tất cả
                </button>
                <button
                  type="button"
                  className="btn btn-outline btn-sm"
                  onClick={() =>
                    setRoleMatrix(
                      roleMatrix.map((r) => ({
                        ...r,
                        canEdit: false,
                        canApprove: false,
                      }))
                    )
                  }
                >
                  <Icon name="cross-small" /> Bỏ chọn
                </button>
                {activeRoleObj && (
                  <button
                    type="button"
                    className="btn btn-emerald"
                    disabled={savingRole}
                    onClick={() => handleSaveRoleMatrix(activeRoleObj, roleMatrix)}
                    style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}
                  >
                    <Icon name="disk" /> {savingRole ? 'Đang lưu...' : 'Lưu Ma Trận Quyền'}
                  </button>
                )}
              </div>
            </div>

            <div className="table-wrapper">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>PHÂN HỆ / TAB NGHIỆP VỤ</th>
                    <th style={{ textAlign: 'center', width: 90 }}>XEM</th>
                    <th style={{ textAlign: 'center', width: 140 }}>THÊM, SỬA, XÓA</th>
                    <th style={{ textAlign: 'center', width: 130 }}>IN & XUẤT EXCEL</th>
                    <th style={{ textAlign: 'center', width: 120 }}>PHÊ DUYỆT</th>
                  </tr>
                </thead>
                <tbody>
                  {roleMatrix.map((row, idx) => (
                    <tr key={row.module}>
                      <td>
                        <strong>{row.module}</strong>{' '}
                        <span style={{ fontSize: 10.5, color: '#94a3b8' }}>
                          ({row.group})
                        </span>
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        <input
                          type="checkbox"
                          checked={row.canView}
                          onChange={(e) => {
                            const next = [...roleMatrix];
                            next[idx].canView = e.target.checked;
                            setRoleMatrix(next);
                          }}
                          style={{ accentColor: '#10b981', width: 16, height: 16, cursor: 'pointer' }}
                        />
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        {row.hasEdit ? (
                          <input
                            type="checkbox"
                            checked={row.canEdit}
                            onChange={(e) => {
                              const next = [...roleMatrix];
                              next[idx].canEdit = e.target.checked;
                              setRoleMatrix(next);
                            }}
                            style={{ accentColor: '#10b981', width: 16, height: 16, cursor: 'pointer' }}
                          />
                        ) : (
                          <span style={{ color: '#cbd5e1' }}>—</span>
                        )}
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        {row.hasExport ? (
                          <input
                            type="checkbox"
                            checked={row.canExport}
                            onChange={(e) => {
                              const next = [...roleMatrix];
                              next[idx].canExport = e.target.checked;
                              setRoleMatrix(next);
                            }}
                            style={{ accentColor: '#10b981', width: 16, height: 16, cursor: 'pointer' }}
                          />
                        ) : (
                          <span style={{ color: '#cbd5e1' }}>—</span>
                        )}
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        {row.hasApprove ? (
                          <input
                            type="checkbox"
                            checked={row.canApprove}
                            onChange={(e) => {
                              const next = [...roleMatrix];
                              next[idx].canApprove = e.target.checked;
                              setRoleMatrix(next);
                            }}
                            style={{ accentColor: '#10b981', width: 16, height: 16, cursor: 'pointer' }}
                          />
                        ) : (
                          <span style={{ color: '#cbd5e1' }}>—</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div
              style={{
                fontSize: 11.5,
                color: '#64748b',
                marginTop: 12,
                padding: '8px 12px',
                backgroundColor: '#f8fafc',
                borderRadius: 6,
                borderLeft: '3px solid #10b981',
              }}
            >
              <Icon name="bulb" /> <strong>Lưu ý:</strong> Khi bạn nhấn <em>"Lưu Ma Trận Quyền"</em>, backend sẽ cập nhật trực tiếp bảng <code>role_permissions</code> cho vai trò này qua API <code>PUT /api/role/update</code>. Người dùng đang đăng nhập với vai trò này sẽ áp dụng ngay khi phiên làm việc được làm mới.
            </div>
          </div>
        </div>
      )}

      {/* MODAL CẬP NHẬT TÀI KHOẢN & MA TRẬN PHÂN QUYỀN CHI TIẾT */}
      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div
            className="modal-dialog lg"
            onClick={(e) => e.stopPropagation()}
            style={{ maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}
          >
            <div className="modal-header">
              <div className="modal-title">
                {editingAcc ? (
                  <>
                    <Icon name="pencil" /> Cập nhật Tài Khoản
                  </>
                ) : (
                  <>
                    <Icon name="user-add" /> Thêm mới Tài Khoản
                  </>
                )}
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setShowModal(false)}
              >
                <Icon name="cross-small" />
              </button>
            </div>

            <form
              onSubmit={(e) => handleSaveAccount(e, false)}
              style={{
                display: 'flex',
                flexDirection: 'column',
                flex: 1,
                minHeight: 0,
                overflow: 'hidden',
              }}
            >
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label>
                      Tên đăng nhập (Mã NV) <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      value={empCode}
                      onChange={(e) => setEmpCode(e.target.value)}
                      placeholder="EMP001"
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Họ và tên nhân viên <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      value={fullName}
                      onChange={(e) => setFullName(e.target.value)}
                      placeholder="Nguyễn Văn Tuấn"
                      required
                    />
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label>
                      Vai trò / Phân quyền <span className="req">*</span>
                    </label>
                    <CustomSelect
                      variant="form"
                      value={roleId}
                      onChange={handleRoleChangeInModal}
                      options={roles.map((r) => ({
                        value: r.id,
                        label: `${r.role_name} (${r.role_code})`,
                      }))}
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Chi nhánh kho phụ trách <span className="req">*</span>
                    </label>
                    <CustomSelect
                      variant="form"
                      value={selectedWhId}
                      onChange={setSelectedWhId}
                      options={[
                        { value: 'ALL', label: 'Toàn bộ chi nhánh kho' },
                        ...warehouses.map((w) => ({
                          value: w.id,
                          label: `${w.warehouse_name.toUpperCase()} (${w.warehouse_code})`,
                        })),
                      ]}
                    />
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label>Mật khẩu khởi tạo / Email</label>
                    <input
                      className="form-control"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="email@ewms.vn"
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>Trạng thái tài khoản</label>
                    <CustomSelect
                      variant="form"
                      value={status}
                      onChange={(val) => setStatus(Number(val))}
                      options={[
                        { value: 1, label: 'Đang hoạt động' },
                        { value: 2, label: 'Tạm khóa' },
                      ]}
                    />
                  </div>
                </div>

                {/* MA TRẬN PHÂN QUYỀN CHI TIẾT */}
                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    margin: '16px 0 10px',
                    padding: '8px 10px',
                    backgroundColor: '#f1f5f9',
                    borderRadius: 6,
                  }}
                >
                  <div>
                    <div style={{ fontWeight: 700, fontSize: 13.5, color: '#0f172a' }}>
                      <Icon name="shield-check" /> Ma Trận Phân Quyền Chi Tiết
                    </div>
                    <div style={{ fontSize: 11.5, color: '#475569' }}>
                      Kế thừa từ vai trò:{' '}
                      <strong style={{ color: '#059669' }}>
                        {modalRoleObj?.role_name} ({modalRoleObj?.role_code})
                      </strong>
                    </div>
                  </div>

                  <div style={{ display: 'flex', gap: 6 }}>
                    <button
                      type="button"
                      className="btn btn-outline btn-sm"
                      onClick={() =>
                        setModalMatrix(
                          modalMatrix.map((r) => ({
                            ...r,
                            canView: true,
                            canEdit: r.hasEdit,
                            canExport: r.hasExport,
                            canApprove: r.hasApprove,
                          }))
                        )
                      }
                    >
                      <Icon name="check" /> Chọn tất cả
                    </button>
                    <button
                      type="button"
                      className="btn btn-outline btn-sm"
                      onClick={() =>
                        setModalMatrix(
                          modalMatrix.map((r) => ({
                            ...r,
                            canEdit: false,
                            canApprove: false,
                          }))
                        )
                      }
                    >
                      <Icon name="cross-small" /> Bỏ chọn
                    </button>
                    {modalRoleObj && (
                      <button
                        type="button"
                        className="btn btn-outline btn-sm"
                        style={{ borderColor: '#10b981', color: '#059669', fontWeight: 600 }}
                        disabled={savingRole}
                        onClick={() => handleSaveRoleMatrix(modalRoleObj, modalMatrix)}
                        title="Lưu các quyền đã chọn cho vai trò này trên toàn hệ thống"
                      >
                        <Icon name="disk" /> {savingRole ? 'Đang lưu...' : 'Lưu quyền vai trò'}
                      </button>
                    )}
                  </div>
                </div>

                <div className="table-wrapper">
                  <table className="data-table">
                    <thead>
                      <tr>
                        <th>PHÂN HỆ / TAB NGHIỆP VỤ</th>
                        <th style={{ textAlign: 'center', width: 80 }}>XEM</th>
                        <th style={{ textAlign: 'center', width: 130 }}>THÊM, SỬA, XÓA</th>
                        <th style={{ textAlign: 'center', width: 130 }}>IN & XUẤT EXCEL</th>
                        <th style={{ textAlign: 'center', width: 110 }}>PHÊ DUYỆT</th>
                      </tr>
                    </thead>
                    <tbody>
                      {modalMatrix.map((row, idx) => (
                        <tr key={row.module}>
                          <td>
                            <strong>{row.module}</strong>{' '}
                            <span style={{ fontSize: 10.5, color: '#94a3b8' }}>
                              ({row.group})
                            </span>
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            <input
                              type="checkbox"
                              checked={row.canView}
                              onChange={(e) => {
                                const next = [...modalMatrix];
                                next[idx].canView = e.target.checked;
                                setModalMatrix(next);
                              }}
                              style={{ accentColor: '#10b981', width: 16, height: 16, cursor: 'pointer' }}
                            />
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            {row.hasEdit ? (
                              <input
                                type="checkbox"
                                checked={row.canEdit}
                                onChange={(e) => {
                                  const next = [...modalMatrix];
                                  next[idx].canEdit = e.target.checked;
                                  setModalMatrix(next);
                                }}
                                style={{ accentColor: '#10b981', width: 16, height: 16, cursor: 'pointer' }}
                              />
                            ) : (
                              <span style={{ color: '#cbd5e1' }}>—</span>
                            )}
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            {row.hasExport ? (
                              <input
                                type="checkbox"
                                checked={row.canExport}
                                onChange={(e) => {
                                  const next = [...modalMatrix];
                                  next[idx].canExport = e.target.checked;
                                  setModalMatrix(next);
                                }}
                                style={{ accentColor: '#10b981', width: 16, height: 16, cursor: 'pointer' }}
                              />
                            ) : (
                              <span style={{ color: '#cbd5e1' }}>—</span>
                            )}
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            {row.hasApprove ? (
                              <input
                                type="checkbox"
                                checked={row.canApprove}
                                onChange={(e) => {
                                  const next = [...modalMatrix];
                                  next[idx].canApprove = e.target.checked;
                                  setModalMatrix(next);
                                }}
                                style={{ accentColor: '#10b981', width: 16, height: 16, cursor: 'pointer' }}
                              />
                            ) : (
                              <span style={{ color: '#cbd5e1' }}>—</span>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                <div
                  style={{
                    fontSize: 11,
                    color: '#64748b',
                    fontStyle: 'italic',
                    marginTop: 8,
                  }}
                >
                  * Phân quyền được áp dụng theo chuẩn RBAC: Thay đổi quyền hạn sẽ cập nhật cho toàn bộ tài khoản có vai trò "{modalRoleObj?.role_name}".
                </div>
              </div>

              <div className="modal-footer" style={{ flexShrink: 0 }}>
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => setShowModal(false)}
                >
                  <Icon name="cross-small" /> Hủy
                </button>
                {editingAcc ? (
                  <>
                    <button
                      type="button"
                      className="btn btn-outline"
                      title="Tạo mới tài khoản từ thông tin hiện tại"
                      onClick={(e) => handleSaveAccount(e, true)}
                    >
                      <Icon name="add" /> Tạo mới
                    </button>
                    <button type="submit" className="btn btn-emerald">
                      <Icon name="disk" /> Cập nhật
                    </button>
                  </>
                ) : (
                  <button type="submit" className="btn btn-emerald">
                    <Icon name="add" /> Tạo mới
                  </button>
                )}
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
