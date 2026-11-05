import React from 'react';
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
    icon: string;
    iconColor?: string;
  }[];
}

const NAV_GROUPS: NavGroup[] = [
  {
    sectionTitle: 'I. TỔNG QUAN',
    items: [{ key: 'dashboard', label: 'Tổng quan', icon: '◔', iconColor: '#10b981' }],
  },
  {
    sectionTitle: 'II. KHAI BÁO',
    items: [
      { key: 'products', label: 'Hàng hóa', icon: '📦', iconColor: '#38bdf8' },
      { key: 'warehouses', label: 'Kho hàng', icon: '🏭', iconColor: '#818cf8' },
      { key: 'contacts', label: 'Đối tượng', icon: '👥', iconColor: '#fbbf24' },
    ],
  },
  {
    sectionTitle: 'III. NGHIỆP VỤ',
    items: [
      { key: 'purchase-orders', label: 'Nhập kho', icon: '↓', iconColor: '#10b981' },
      { key: 'outbound-orders', label: 'Xuất kho', icon: '↑', iconColor: '#ef4444' },
      { key: 'stock-transfers', label: 'Chuyển kho', icon: '⇄', iconColor: '#f59e0b' },
      { key: 'all-vouchers', label: 'Quản lý phiếu', icon: '📋', iconColor: '#60a5fa' },
      { key: 'inventory', label: 'Tồn kho', icon: '🎄', iconColor: '#10b981' },
    ],
  },
  {
    sectionTitle: 'IV. BÁO CÁO',
    items: [
      { key: 'reports', label: 'Báo cáo', icon: '📊', iconColor: '#34d399' },
      { key: 'history', label: 'Lịch sử', icon: '🕒', iconColor: '#94a3b8' },
    ],
  },
  {
    sectionTitle: 'V. BIỂU MẪU',
    items: [{ key: 'templates', label: 'Lưu mẫu', icon: '📁', iconColor: '#fbbf24' }],
  },
  {
    sectionTitle: 'VI. CẤU HÌNH',
    items: [
      { key: 'users', label: 'Quản trị', icon: '🛡️', iconColor: '#38bdf8' },
      { key: 'settings', label: 'Cài đặt', icon: '⚙️', iconColor: '#94a3b8' },
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
        <div className="brand-logo-box">🏢</div>
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
                    {item.icon}
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
            ↪
          </button>
        )}
      </div>
    </aside>
  );
};
