import React, { useCallback, useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { useAuth } from '../../contexts/AuthContext';
import { masterDataService } from '../../services/masterDataService';
import {
  type AccountItem,
  type RoleItem,
  type WarehouseItem,
} from '../../types/master.types';

interface PermissionMatrixRow {
  module: string;
  group: string;
  canView: boolean;
  canEdit: boolean;
  canExport: boolean;
  canApprove: boolean;
}

const DEFAULT_MATRIX: PermissionMatrixRow[] = [
  {
    module: 'Tổng quan',
    group: 'TỔNG QUAN',
    canView: true,
    canEdit: false,
    canExport: false,
    canApprove: false,
  },
  {
    module: 'Hàng hóa',
    group: 'KHAI BÁO',
    canView: true,
    canEdit: true,
    canExport: true,
    canApprove: false,
  },
  {
    module: 'Kho hàng',
    group: 'KHAI BÁO',
    canView: true,
    canEdit: true,
    canExport: true,
    canApprove: false,
  },
  {
    module: 'Đối tượng (NCC / Khách hàng)',
    group: 'KHAI BÁO',
    canView: true,
    canEdit: true,
    canExport: true,
    canApprove: false,
  },
  {
    module: 'Nhập kho',
    group: 'NGHIỆP VỤ',
    canView: true,
    canEdit: true,
    canExport: true,
    canApprove: true,
  },
  {
    module: 'Xuất kho',
    group: 'NGHIỆP VỤ',
    canView: true,
    canEdit: true,
    canExport: true,
    canApprove: true,
  },
  {
    module: 'Chuyển kho',
    group: 'NGHIỆP VỤ',
    canView: true,
    canEdit: true,
    canExport: true,
    canApprove: true,
  },
  {
    module: 'Tồn kho',
    group: 'NGHIỆP VỤ',
    canView: true,
    canEdit: true,
    canExport: true,
    canApprove: false,
  },
  {
    module: 'Báo cáo N-X-T',
    group: 'BÁO CÁO',
    canView: true,
    canEdit: false,
    canExport: true,
    canApprove: false,
  },
  {
    module: 'Lịch sử & Nhật ký',
    group: 'BÁO CÁO',
    canView: true,
    canEdit: false,
    canExport: true,
    canApprove: false,
  },
];

export const UserAccessLogPage: React.FC = () => {
  const { user, notifyError, notifySuccess } = useAuth();
  const [accounts, setAccounts] = useState<AccountItem[]>([]);
  const [roles, setRoles] = useState<RoleItem[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseItem[]>([]);
  const [pageSize, setPageSize] = useState('10');

  // Modal Cập nhật / Thêm Tài Khoản (Screen 2)
  const [showModal, setShowModal] = useState(false);
  const [editingAcc, setEditingAcc] = useState<AccountItem | null>(null);
  const [empCode, setEmpCode] = useState('');
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('Admin@123');
  const [roleId, setRoleId] = useState('');
  const [selectedWhId, setSelectedWhId] = useState('ALL');
  const [status, setStatus] = useState(1);
  const [matrix, setMatrix] = useState<PermissionMatrixRow[]>(DEFAULT_MATRIX);

  const loadData = useCallback(() => {
    masterDataService
      .searchAccounts()
      .then((res) => setAccounts(res.items || []))
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải danh sách tài khoản')
      );
    masterDataService
      .searchRoles()
      .then((res) => {
        const list = res.items || [];
        setRoles(list);
        if (list.length > 0 && !roleId) setRoleId(list[0].id);
      })
      .catch(() => {});
    masterDataService
      .searchWarehouses()
      .then((res) => setWarehouses(res.items || []))
      .catch(() => {});
  }, [roleId, notifyError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const openCreateModal = () => {
    setEditingAcc(null);
    setEmpCode('');
    setFullName('');
    setEmail('');
    setPassword('Admin@123');
    setRoleId(roles[0]?.id || '');
    setSelectedWhId('ALL');
    setStatus(1);
    setMatrix(DEFAULT_MATRIX.map((r) => ({ ...r })));
    setShowModal(true);
  };

  const openEditModal = (acc: AccountItem) => {
    setEditingAcc(acc);
    setEmpCode(acc.employee_code);
    setFullName(acc.full_name);
    setEmail(acc.email);
    setPassword('••••••••');
    setRoleId(acc.role_id || roles[0]?.id || '');
    setSelectedWhId(
      acc.assigned_warehouses.length >= warehouses.length
        ? 'ALL'
        : acc.assigned_warehouses[0]?.warehouse_id || 'ALL'
    );
    setStatus(acc.status);
    const isAdmin = acc.role_code === 'ADMIN';
    setMatrix(
      DEFAULT_MATRIX.map((r) => ({
        ...r,
        canApprove: isAdmin ? r.canApprove : false,
      }))
    );
    setShowModal(true);
  };

  const handleSaveAccount = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const whIds =
        selectedWhId === 'ALL' ? warehouses.map((w) => w.id) : [selectedWhId];

      if (editingAcc) {
        await masterDataService.updateAccount({
          id: editingAcc.id,
          employee_code: empCode,
          full_name: fullName,
          email,
          role_id: roleId,
          warehouse_ids: whIds,
          status,
        });
        notifySuccess(`Đã cập nhật tài khoản '${fullName}' thành công!`);
      } else {
        await masterDataService.createAccount({
          employee_code: empCode || undefined,
          full_name: fullName,
          email: email || `${empCode.toLowerCase() || 'user'}@ewms.vn`,
          password: password || 'Admin@123',
          role_id: roleId,
          warehouse_ids: whIds,
          status,
        });
        notifySuccess(`Đã thêm tài khoản '${fullName}' thành công!`);
      }
      setShowModal(false);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi lưu tài khoản');
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

  return (
    <div>
      {/* PAGE HEADER (Screen 2) */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#10b981' }}>🛡️</span> Phân Quyền & Quản Trị Hệ Thống
          </h1>
          <p>Quản lý tài khoản người dùng và ma trận phân quyền chi tiết vai trò</p>
        </div>
        <div className="page-actions">
          <button className="btn btn-emerald" onClick={openCreateModal}>
            👤+ Thêm tài khoản
          </button>
        </div>
      </div>

      {/* MAIN TABLE CARD */}
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
            👥 Danh Sách Tài Khoản Đăng Nhập & Phân Quyền Vai Trò
          </div>
          <span className="status-pill emerald">
            ● Đang trực tuyến: {user?.employee_code} ({user?.full_name})
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
                <th>PHÂN QUYỀN</th>
                <th style={{ textAlign: 'center' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((acc, idx) => {
                const isAdmin = acc.role_code === 'ADMIN';
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
                        <span className="status-pill emerald">👑 Toàn quyền</span>
                      ) : (
                        <span style={{ display: 'inline-flex', gap: 6 }}>
                          <span className="status-pill blue">9/10 Tab</span>
                          <span className="status-pill warning">🖨️ In/Excel</span>
                        </span>
                      )}
                    </td>
                    <td style={{ textAlign: 'center' }}>
                      <button
                        className="icon-action-btn"
                        title="Cập nhật Tài khoản & Ma trận phân quyền"
                        onClick={() => openEditModal(acc)}
                      >
                        ✏️
                      </button>
                      {!isAdmin && (
                        <button
                          className="icon-action-btn danger"
                          title="Xóa tài khoản"
                          onClick={() => handleDeleteAccount(acc)}
                        >
                          🗑️
                        </button>
                      )}
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
            <button className="page-btn">«</button>
            <button className="page-btn active">1</button>
            <button className="page-btn">»</button>
          </div>
        </div>
      </div>

      {/* MODAL CẬP NHẬT TÀI KHOẢN & MA TRẬN PHÂN QUYỀN CHI TIẾT (Screen 2) */}
      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="modal-dialog lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">
                {editingAcc ? 'Cập nhật Tài Khoản' : 'Thêm mới Tài Khoản'}
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setShowModal(false)}
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleSaveAccount}>
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
                      placeholder="VD: EMP006"
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
                      onChange={setRoleId}
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
                  }}
                >
                  <div style={{ fontWeight: 700, fontSize: 13.5, color: '#0f172a' }}>
                    🛡️ Ma Trận Phân Quyền Chi Tiết
                  </div>
                  <div style={{ display: 'flex', gap: 6 }}>
                    <button
                      type="button"
                      className="btn btn-outline btn-sm"
                      onClick={() =>
                        setMatrix(
                          matrix.map((r) => ({
                            ...r,
                            canView: true,
                            canEdit: true,
                            canExport: true,
                            canApprove: true,
                          }))
                        )
                      }
                    >
                      ✓✓ Chọn tất cả
                    </button>
                    <button
                      type="button"
                      className="btn btn-outline btn-sm"
                      onClick={() =>
                        setMatrix(
                          matrix.map((r) => ({
                            ...r,
                            canEdit: false,
                            canApprove: false,
                          }))
                        )
                      }
                    >
                      ✕ Bỏ chọn
                    </button>
                  </div>
                </div>

                <div className="table-wrapper">
                  <table className="data-table">
                    <thead>
                      <tr>
                        <th>PHÂN HỆ / TAB NGHIỆP VỤ</th>
                        <th style={{ textAlign: 'center' }}>XEM</th>
                        <th style={{ textAlign: 'center' }}>THÊM, SỬA, XÓA</th>
                        <th style={{ textAlign: 'center' }}>IN & XUẤT EXCEL</th>
                        <th style={{ textAlign: 'center' }}>PHÊ DUYỆT</th>
                      </tr>
                    </thead>
                    <tbody>
                      {matrix.map((row, idx) => (
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
                                const next = [...matrix];
                                next[idx].canView = e.target.checked;
                                setMatrix(next);
                              }}
                              style={{ accentColor: '#10b981', width: 16, height: 16 }}
                            />
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            <input
                              type="checkbox"
                              checked={row.canEdit}
                              onChange={(e) => {
                                const next = [...matrix];
                                next[idx].canEdit = e.target.checked;
                                setMatrix(next);
                              }}
                              style={{ accentColor: '#10b981', width: 16, height: 16 }}
                            />
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            <input
                              type="checkbox"
                              checked={row.canExport}
                              onChange={(e) => {
                                const next = [...matrix];
                                next[idx].canExport = e.target.checked;
                                setMatrix(next);
                              }}
                              style={{ accentColor: '#10b981', width: 16, height: 16 }}
                            />
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            <input
                              type="checkbox"
                              checked={row.canApprove}
                              onChange={(e) => {
                                const next = [...matrix];
                                next[idx].canApprove = e.target.checked;
                                setMatrix(next);
                              }}
                              style={{ accentColor: '#10b981', width: 16, height: 16 }}
                            />
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
                  * Ghi chú: Bỏ chọn "Xem" sẽ tự động ẩn tab tương ứng trên thanh điều hướng. Tab "Quản lý phiếu" sẽ tự động kế thừa quyền từ 3 tab Nhập, Xuất, Chuyển kho.
                </div>
              </div>

              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => setShowModal(false)}
                >
                  Hủy
                </button>
                <button type="submit" className="btn btn-emerald">
                  💾 Lưu Tài Khoản
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
