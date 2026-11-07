import React from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { type NavTabKey } from '../../types/auth.types';
import { CustomSelect } from '../common/CustomSelect';

interface HeaderProps {
  activeTab: NavTabKey;
  onToggleSidebar: () => void;
  onSelectTab: (tab: NavTabKey) => void;
}

const TAB_LABELS: Record<NavTabKey, string> = {
  dashboard: 'Tổng quan',
  products: 'Hàng hóa',
  warehouses: 'Kho hàng',
  contacts: 'Đối tượng',
  'purchase-orders': 'Nhập kho',
  'outbound-orders': 'Xuất kho',
  'stock-transfers': 'Chuyển kho',
  'all-vouchers': 'Quản lý phiếu',
  inventory: 'Tồn kho',
  reports: 'Báo cáo',
  history: 'Lịch sử',
  templates: 'Biểu mẫu',
  users: 'Quản trị',
  settings: 'Cài đặt',
};

export const Header: React.FC<HeaderProps> = ({
  activeTab,
  onToggleSidebar,
  onSelectTab,
}) => {
  const { user, switchWarehouse, logout } = useAuth();

  if (!user) return null;

  return (
    <header className="topbar">
      <div className="topbar-left">
        <button
          className="sidebar-toggle-btn"
          onClick={onToggleSidebar}
          title="Thu gọn / Mở rộng thanh điều hướng"
        >
          ☰
        </button>
        <div className="breadcrumb">
          <span
            style={{
              cursor: 'pointer',
              display: 'inline-flex',
              alignItems: 'center',
              gap: 4,
            }}
            onClick={() => onSelectTab('dashboard')}
          >
            🏠 Kho
          </span>
          <span>›</span>
          <span className="breadcrumb-current">{TAB_LABELS[activeTab]}</span>
        </div>
      </div>

      <div className="topbar-right">
        <button
          className="topbar-icon-btn"
          title="Cảnh báo tồn kho & phiếu chờ duyệt"
          onClick={() => onSelectTab('inventory')}
        >
          🔔
          <span className="notif-badge">4</span>
        </button>

        <button
          className="topbar-icon-btn"
          title="Làm mới dữ liệu"
          onClick={() => window.location.reload()}
        >
          ↻
        </button>

        <CustomSelect
          variant="pill"
          value={user.current_warehouse_id}
          onChange={(val) => switchWarehouse(val)}
          options={user.assigned_warehouses.map((w) => ({
            value: w.warehouse_id,
            label: `📍 ${w.warehouse_name.toUpperCase()} (${w.warehouse_code})`,
            subLabel: w.address,
          }))}
        />

        <button className="topbar-logout-btn" onClick={logout}>
          <span>↪</span>
          <span>Đăng xuất</span>
        </button>

        <button className="topbar-icon-btn" title="Giao diện Sáng / Tối">
          🌙
        </button>
      </div>
    </header>
  );
};
