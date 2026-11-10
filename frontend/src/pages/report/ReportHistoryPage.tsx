import React, { useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { Icon } from '../../components/common/Icon';
import { useAuth } from '../../contexts/AuthContext';
import { inventoryService } from '../../services/inventoryService';
import { masterDataService } from '../../services/masterDataService';
import { type InventoryItem } from '../../types/inventory.types';
import { type AccessLogItem } from '../../types/master.types';

interface ReportHistoryPageProps {
  mode: 'reports' | 'history';
}

export const ReportHistoryPage: React.FC<ReportHistoryPageProps> = ({
  mode,
}) => {
  const { notifyError, notifySuccess } = useAuth();
  const [logs, setLogs] = useState<AccessLogItem[]>([]);
  const [inventories, setInventories] = useState<InventoryItem[]>([]);

  // Filters for History (Screen 10)
  const [filterDate, setFilterDate] = useState(
    new Date().toISOString().slice(0, 10)
  );
  const [filterAction, setFilterAction] = useState('ALL');
  const [filterModule, setFilterModule] = useState('ALL');
  const [filterUser, setFilterUser] = useState('ALL');
  const [keyword, setKeyword] = useState('');
  const [pageSize, setPageSize] = useState('10');

  useEffect(() => {
    if (mode === 'history') {
      masterDataService
        .searchAccessLogs()
        .then((res) => setLogs(res.items || []))
        .catch((err: unknown) =>
          notifyError(err instanceof Error ? err.message : 'Lỗi tải Nhật ký lịch sử')
        );
    } else {
      inventoryService
        .searchInventory({ page_size: 200 })
        .then((res) => setInventories(res.items || []))
        .catch((err: unknown) =>
          notifyError(err instanceof Error ? err.message : 'Lỗi tải Báo cáo N-X-T')
        );
    }
  }, [mode, notifyError]);

  const getActionBadge = (action: string) => {
    const upper = action.toUpperCase();
    if (upper.includes('CREATE') || upper.includes('RECEIVE')) {
      return (
        <span className="status-pill emerald">
          <Icon name="add" /> Thêm mới
        </span>
      );
    }
    if (upper.includes('DELETE') || upper.includes('REJECT')) {
      return (
        <span className="status-pill danger">
          <Icon name="trash" /> Xóa / Từ chối
        </span>
      );
    }
    if (upper.includes('LOGIN')) {
      return (
        <span className="status-pill blue">
          <Icon name="sign-in-alt" /> Đăng nhập
        </span>
      );
    }
    return (
      <span className="status-pill warning">
        <Icon name="pencil" /> Chỉnh sửa
      </span>
    );
  };

  const getModuleLabel = (mod: string) => {
    const map: Record<string, string> = {
      PURCHASE_ORDER: 'Nhập kho',
      OUTBOUND_ORDER: 'Xuất kho',
      STOCK_TRANSFER: 'Chuyển kho',
      INVENTORY: 'Tồn kho',
      PRODUCT: 'Hàng hóa',
      WAREHOUSE: 'Kho hàng',
      USER: 'Quản trị',
      AUTH: 'Hệ thống',
      SUPPLIER: 'Đối tượng',
      CUSTOMER: 'Đối tượng',
    };
    return map[mod] || mod;
  };

  if (mode === 'reports') {
    const totalVal = inventories.reduce((s, x) => s + x.total_stock_value, 0);
    const totalQty = inventories.reduce((s, x) => s + x.quantity, 0);
    return (
      <div>
        <div className="page-header">
          <div className="page-title-group">
            <h1>
              <Icon name="chart-histogram" color="#10b981" />
              Báo Cáo Tổng Hợp Nhập - Xuất - Tồn
            </h1>
            <p>
              Tổng hợp số liệu cân đối Nhập - Xuất - Tồn kho và định giá tài sản lưu kho theo kỳ kế toán
            </p>
          </div>
          <div className="page-actions">
            <button
              className="btn btn-outline"
              onClick={() => notifySuccess('Đã xuất Báo cáo Nhập - Xuất - Tồn ra Excel!')}
            >
              <Icon name="file-excel" /> Xuất Excel
            </button>
            <button className="btn btn-emerald" onClick={() => window.print()}>
              <Icon name="print" /> In báo cáo
            </button>
          </div>
        </div>

        <div className="surface-card">
          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>CHI NHÁNH KHO</th>
                  <th>MÃ HÀNG</th>
                  <th>TÊN HÀNG HÓA</th>
                  <th>NHÓM HÀNG</th>
                  <th style={{ textAlign: 'right' }}>TỒN CUỐI KỲ</th>
                  <th style={{ textAlign: 'right' }}>ĐƠN GIÁ VỐN</th>
                  <th style={{ textAlign: 'right' }}>GIÁ TRỊ TỒN KHO</th>
                </tr>
              </thead>
              <tbody>
                {inventories.map((inv, idx) => (
                  <tr key={inv.id}>
                    <td>{idx + 1}</td>
                    <td style={{ fontWeight: 600 }}>
                      {inv.warehouse_name.toUpperCase()}
                    </td>
                    <td className="voucher-code">{inv.product_code}</td>
                    <td style={{ fontWeight: 600 }}>{inv.product_name}</td>
                    <td>{inv.category_name}</td>
                    <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                      {inv.quantity.toLocaleString('vi-VN')}
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      {inv.unit_price.toLocaleString('vi-VN')} ₫
                    </td>
                    <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                      {inv.total_stock_value.toLocaleString('vi-VN')} ₫
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot>
                <tr>
                  <td colSpan={5}>TỔNG CỘNG TOÀN HỆ THỐNG:</td>
                  <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                    {totalQty.toLocaleString('vi-VN')} sp
                  </td>
                  <td />
                  <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                    {totalVal.toLocaleString('vi-VN')} ₫
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      </div>
    );
  }

  // Screen 10: Nhật Ký Lịch Sử Thao Tác
  const filteredLogs = logs.filter((l) => {
    const matchAction =
      filterAction === 'ALL' || l.action.toUpperCase().includes(filterAction);
    const matchModule =
      filterModule === 'ALL' || l.module.toUpperCase() === filterModule;
    const matchUser =
      filterUser === 'ALL' || (l.full_name || 'Quản trị viên') === filterUser;
    const matchKw =
      !keyword ||
      (l.description &&
        l.description.toLowerCase().includes(keyword.toLowerCase())) ||
      (l.full_name &&
        l.full_name.toLowerCase().includes(keyword.toLowerCase()));
    return matchAction && matchModule && matchUser && matchKw;
  });

  return (
    <div>
      {/* PAGE HEADER (Screen 10) */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <Icon name="time-past" color="#10b981" />
            Nhật Ký Lịch Sử Thao Tác
          </h1>
          <p>
            Ghi nhận chi tiết lịch sử Thêm, Sửa, Xóa dữ liệu của nhân viên (Mặc định lọc hôm nay)
          </p>
        </div>
        <div className="page-actions">
          <button
            className="btn btn-outline"
            onClick={() => setFilterDate(new Date().toISOString().slice(0, 10))}
          >
            <Icon name="calendar" /> Hôm nay
          </button>
          <button
            className="btn btn-outline"
            onClick={() => notifySuccess('Đã xuất Nhật ký lịch sử thao tác ra Excel!')}
          >
            <Icon name="file-excel" /> Excel
          </button>
        </div>
      </div>

      {/* MAIN CARD */}
      <div className="surface-card">
        <div className="filter-bar">
          <div className="filter-group">
            <span><Icon name="calendar" /> Ngày:</span>
            <input
              type="date"
              className="filter-input"
              value={filterDate}
              onChange={(e) => setFilterDate(e.target.value)}
            />
          </div>

          <CustomSelect
            variant="filter"
            value={filterAction}
            onChange={setFilterAction}
            options={[
              { value: 'ALL', label: 'Tất cả hành động (Thêm/Sửa/Xóa)' },
              { value: 'CREATE', label: 'Thêm mới' },
              { value: 'CREATE', label: 'Thêm mới' },
              { value: 'UPDATE', label: 'Chỉnh sửa / Duyệt' },
              { value: 'DELETE', label: 'Xóa' },
            ]}
          />

          <CustomSelect
            variant="filter"
            value={filterModule}
            onChange={setFilterModule}
            options={[
              { value: 'ALL', label: 'Tất cả phân hệ' },
              { value: 'PURCHASE_ORDER', label: 'Nhập kho' },
              { value: 'OUTBOUND_ORDER', label: 'Xuất kho' },
              { value: 'STOCK_TRANSFER', label: 'Chuyển kho' },
              { value: 'INVENTORY', label: 'Tồn kho' },
              { value: 'PRODUCT', label: 'Hàng hóa' },
            ]}
          />

          <CustomSelect
            variant="filter"
            value={filterUser}
            onChange={setFilterUser}
            options={[
              { value: 'ALL', label: 'Tất cả tài khoản' },
              { value: 'Nguyễn Quản Trị', label: 'Nguyễn Quản Trị (Admin)' },
            ]}
          />
        </div>

        <div className="filter-bar">
          <div className="search-box-wrapper">
            <span className="search-box-icon">              <Icon name="search" /></span>
            <input
              className="search-box-input"
              placeholder="Tìm người thực hiện, chi tiết nội dung..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </div>
        </div>

        <div className="table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th style={{ width: 55 }}>STT</th>
                <th style={{ width: 175 }}>THỜI GIAN</th>
                <th style={{ width: 250 }}>TÀI KHOẢN & NGƯỜI THỰC HIỆN</th>
                <th style={{ width: 140 }}>HÀNH ĐỘNG</th>
                <th style={{ width: 130 }}>PHÂN HỆ</th>
                <th>NỘI DUNG CHI TIẾT THAO TÁC THAY ĐỔI</th>
              </tr>
            </thead>
            <tbody>
              {filteredLogs.map((log, idx) => {
                const actorName = log.full_name || 'Quản trị viên';
                const avatarLetter = actorName.trim().charAt(0).toUpperCase();
                return (
                  <tr key={log.id}>
                    <td>{idx + 1}</td>
                    <td style={{ color: '#475569' }}>
                      <Icon name="clock" />{' '}
                      {log.created_on
                        ? new Date(log.created_on).toLocaleString('vi-VN')
                        : new Date().toLocaleString('vi-VN')}
                    </td>
                    <td>
                      <div
                        style={{
                          display: 'flex',
                          alignItems: 'center',
                          gap: 10,
                        }}
                      >
                        <div className="user-avatar-circle">{avatarLetter}</div>
                        <div>
                          <div style={{ fontWeight: 700, color: '#0f172a' }}>
                            {actorName}
                          </div>
                          <div style={{ fontSize: 11, color: '#64748b' }}>
                            {log.employee_code
                              ? `${log.employee_code} (Admin)`
                              : 'Quản trị viên (Admin)'}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td>{getActionBadge(log.action)}</td>
                    <td>
                      <span className="status-pill gray">
                        {getModuleLabel(log.module)}
                      </span>
                    </td>
                    <td style={{ fontWeight: 500, color: '#1e293b' }}>
                      {log.description ||
                        `${log.action} trên phân hệ ${log.module}`}
                    </td>
                  </tr>
                );
              })}
              {filteredLogs.length === 0 && (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', padding: 24, color: '#64748b' }}>
                    Chưa có nhật ký thao tác nào.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        <div className="pagination-bar">
          <span>
            Hiển thị <strong>1 - {filteredLogs.length}</strong> trên tổng số{' '}
            <strong>{filteredLogs.length}</strong> dòng
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
    </div>
  );
};
