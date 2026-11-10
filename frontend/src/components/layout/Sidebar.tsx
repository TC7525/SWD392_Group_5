import React from 'react';
import { Icon, type IconName } from '../common/Icon';
import { useAuth } from '../../contexts/AuthContext';
import { type NavTabKey } from '../../types/auth.types';

interface SidebarProps {
  activeTab: NavTabKey;
  onSelectTab: (tab: NavTabKey) => void;
  collapsed?: boolean;
}

interface NavGroup {
  sectionTitle: string;
  items: {
    key: NavTabKey;
    label: string;
    icon: IconName;
    iconColor?: string;
  }[];
}

const NAV_GROUPS: NavGroup[] = [
  {
    sectionTitle: 'I. TỔNG QUAN',
    items: [{ key: 'dashboard', label: 'Tổng quan', icon: 'apps', iconColor: '#10b981' }],
  },
  {
    sectionTitle: 'II. KHAI BÁO',
    items: [
      { key: 'products', label: 'Hàng hóa', icon: 'box', iconColor: '#38bdf8' },
      { key: 'warehouses', label: 'Kho hàng', icon: 'warehouse-alt', iconColor: '#818cf8' },
      { key: 'contacts', label: 'Đối tượng', icon: 'users', iconColor: '#fbbf24' },
    ],
  },
  {
    sectionTitle: 'III. NGHIỆP VỤ',
    items: [
      { key: 'purchase-orders', label: 'Nhập kho', icon: 'inbox-in', iconColor: '#10b981' },
      { key: 'outbound-orders', label: 'Xuất kho', icon: 'inbox-out', iconColor: '#ef4444' },
      { key: 'stock-transfers', label: 'Chuyển kho', icon: 'exchange', iconColor: '#f59e0b' },
      { key: 'all-vouchers', label: 'Quản lý phiếu', icon: 'clipboard-list', iconColor: '#60a5fa' },
      { key: 'inventory', label: 'Tồn kho', icon: 'boxes', iconColor: '#10b981' },
    ],
  },
  {
    sectionTitle: 'IV. BÁO CÁO',
    items: [
      { key: 'reports', label: 'Báo cáo', icon: 'chart-histogram', iconColor: '#34d399' },
      { key: 'history', label: 'Lịch sử', icon: 'time-past', iconColor: '#94a3b8' },
    ],
  },
  {
    sectionTitle: 'V. BIỂU MẪU',
    items: [{ key: 'templates', label: 'Lưu mẫu', icon: 'folder', iconColor: '#fbbf24' }],
  },
  {
    sectionTitle: 'VI. CẤU HÌNH',
    items: [
      { key: 'users', label: 'Quản trị', icon: 'shield-check', iconColor: '#38bdf8' },
      { key: 'settings', label: 'Cài đặt', icon: 'settings', iconColor: '#94a3b8' },
    ],
  },
];

export const Sidebar: React.FC<SidebarProps> = ({
  activeTab,
  onSelectTab,
  collapsed = false,
}) => {
  const { user, logout } = useAuth();
  const avatarChar = (user?.full_name || 'Q').trim().charAt(0).toUpperCase();

  return (
    <aside className={`sidebar ${collapsed ? 'collapsed' : ''}`}>
      <div className="sidebar-brand">
        <div className="brand-logo-box">
          <Icon name="building" />
        </div>
        {!collapsed && (
          <div className="brand-text">
            <h2>Quản Lý Kho</h2>
            <span>Version 3.0</span>
          </div>
        )}
      </div>

      <div className="sidebar-nav-scroll">
        {NAV_GROUPS.map((group) => (
          <div key={group.sectionTitle} className="nav-section">
            {!collapsed && (
              <div className="nav-section-title">{group.sectionTitle}</div>
            )}
            {group.items.map((item) => {
              const isActive = activeTab === item.key;
              return (
                <button
                  key={item.key}
                  className={`nav-item ${isActive ? 'active' : ''}`}
                  onClick={() => onSelectTab(item.key)}
                  title={item.label}
                >
                  <span
                    className="nav-icon"
                    style={{ color: isActive ? '#10b981' : item.iconColor }}
                  >
                    <Icon name={item.icon} />
                  </span>
                  {!collapsed && <span>{item.label}</span>}
                </button>
              );
            })}
          </div>
        ))}
      </div>

      <div className="sidebar-footer">
        <div className="sidebar-user-info">
          <div className="user-avatar-circle">{avatarChar}</div>
          {!collapsed && (
            <div className="sidebar-user-text">
              <div className="sidebar-user-name">
                {user?.full_name || 'Quản trị viên'}
              </div>
              <div className="sidebar-user-role">
                {user?.role_name || 'Admin'}
              </div>
            </div>
          )}
        </div>
        {!collapsed && (
          <button
            className="sidebar-logout-icon"
            onClick={logout}
            title="Đăng xuất"
          >
            <Icon name="sign-out-alt" />
          </button>
        )}
      </div>
    </aside>
  );
};
