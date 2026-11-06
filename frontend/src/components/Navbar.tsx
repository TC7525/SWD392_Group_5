import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth, DEMO_PERSONAS, type DemoPersonaRole } from '../context/AuthContext';

export const Navbar: React.FC = () => {
  const { currentUser, isAuthenticated, logout, quickLogin, isLoading } = useAuth();
  let pathname = '';
  try {
    // eslint-disable-next-line react-hooks/rules-of-hooks
    const location = useLocation();
    pathname = location.pathname;
  } catch {
    // Graceful fallback if rendered outside Router in tests
    pathname = window.location.pathname;
  }

  const handleQuickSwitch = async (role: DemoPersonaRole) => {
    try {
      await quickLogin(role);
    } catch (err) {
      console.error('Quick switch persona failed:', err);
    }
  };

  const navItems = [
    { label: 'Đơn mua hàng', path: '/purchase-orders', roles: ['Purchase Staff', 'Warehouse Manager', 'Administrator'] },
    { label: 'Nhận hàng', path: '/receiving', roles: ['Warehouse Staff', 'Warehouse Manager', 'Administrator'] },
    { label: 'Tồn kho & Giao dịch', path: '/inventory', roles: ['Purchase Staff', 'Warehouse Staff', 'Warehouse Manager', 'Administrator'] },
  ];

  const getPersonaIsActive = (email: string) => {
    return currentUser?.email?.toLowerCase() === email.toLowerCase();
  };

  return (
    <header
      style={{
        backgroundColor: '#1E293B',
        color: '#F8FAFC',
        borderBottom: '1px solid #334155',
        boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
        position: 'sticky',
        top: 0,
        zIndex: 50,
      }}
    >
      <div
        style={{
          maxWidth: '1280px',
          margin: '0 auto',
          padding: '0 16px',
        }}
      >
        {/* Main top bar */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            height: '64px',
            gap: '16px',
          }}
        >
          {/* Logo & Brand */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '24px' }}>
            <Link
              to="/purchase-orders"
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '10px',
                textDecoration: 'none',
                color: 'inherit',
              }}
            >
              <div
                style={{
                  width: '36px',
                  height: '36px',
                  borderRadius: '8px',
                  backgroundColor: '#3B82F6',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontWeight: 700,
                  fontSize: '18px',
                  color: '#FFFFFF',
                }}
              >
                E
              </div>
              <div style={{ lineHeight: 1.2 }}>
                <span style={{ fontWeight: 700, fontSize: '18px', letterSpacing: '-0.02em' }}>
                  EWMS
                </span>
                <span
                  style={{
                    display: 'block',
                    fontSize: '11px',
                    color: '#94A3B8',
                    fontWeight: 500,
                  }}
                >
                  Kho & Mua hàng
                </span>
              </div>
            </Link>

            {/* Navigation links */}
            {isAuthenticated && (
              <nav style={{ display: 'flex', gap: '6px' }}>
                {navItems.map((item) => {
                  const isActive =
                    pathname === item.path ||
                    (item.path === '/purchase-orders' && pathname === '/');
                  return (
                    <Link
                      key={item.path}
                      to={item.path}
                      style={{
                        padding: '8px 14px',
                        borderRadius: '6px',
                        fontSize: '14px',
                        fontWeight: 500,
                        textDecoration: 'none',
                        color: isActive ? '#FFFFFF' : '#CBD5E1',
                        backgroundColor: isActive ? '#334155' : 'transparent',
                        transition: 'background-color 0.15s, color 0.15s',
                      }}
                    >
                      {item.label}
                    </Link>
                  );
                })}
              </nav>
            )}
          </div>

          {/* User profile & actions */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
            {isAuthenticated && currentUser ? (
              <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                <div style={{ textAlign: 'right', lineHeight: 1.3 }}>
                  <div style={{ fontSize: '14px', fontWeight: 600, color: '#F1F5F9' }}>
                    {currentUser.fullName}
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '6px', justifyContent: 'flex-end' }}>
                    <span
                      style={{
                        fontSize: '11px',
                        padding: '2px 8px',
                        borderRadius: '12px',
                        backgroundColor: '#0F172A',
                        color: '#38BDF8',
                        border: '1px solid #1E3A8A',
                        fontWeight: 600,
                      }}
                    >
                      {currentUser.role || 'User'}
                    </span>
                    <span style={{ fontSize: '11px', color: '#64748B' }}>
                      {currentUser.employeeCode}
                    </span>
                  </div>
                </div>

                <button
                  type="button"
                  onClick={logout}
                  style={{
                    padding: '6px 12px',
                    fontSize: '13px',
                    fontWeight: 500,
                    backgroundColor: '#334155',
                    color: '#F8FAFC',
                    border: '1px solid #475569',
                    borderRadius: '6px',
                    cursor: 'pointer',
                    transition: 'all 0.15s',
                  }}
                  title="Đăng xuất khỏi hệ thống"
                >
                  Đăng xuất
                </button>
              </div>
            ) : (
              <Link
                to="/login"
                style={{
                  padding: '6px 14px',
                  fontSize: '13px',
                  fontWeight: 600,
                  backgroundColor: '#2563EB',
                  color: '#FFFFFF',
                  borderRadius: '6px',
                  textDecoration: 'none',
                }}
              >
                Đăng nhập
              </Link>
            )}
          </div>
        </div>

        {/* Demo Quick Persona Switcher Toolbar */}
        <div
          style={{
            borderTop: '1px solid #334155',
            padding: '8px 0',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            flexWrap: 'wrap',
            gap: '8px',
            fontSize: '12px',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px', color: '#94A3B8' }}>
            <span
              style={{
                display: 'inline-block',
                width: '8px',
                height: '8px',
                borderRadius: '50%',
                backgroundColor: '#10B981',
              }}
            />
            <span style={{ fontWeight: 600, color: '#CBD5E1' }}>Chuyển nhanh vai trò Demo:</span>
          </div>

          <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
            {(Object.keys(DEMO_PERSONAS) as DemoPersonaRole[]).map((roleKey) => {
              const persona = DEMO_PERSONAS[roleKey];
              const isActive = getPersonaIsActive(persona.email);
              return (
                <button
                  key={roleKey}
                  type="button"
                  disabled={isLoading || isActive}
                  onClick={() => handleQuickSwitch(roleKey)}
                  style={{
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: '6px',
                    padding: '4px 10px',
                    fontSize: '12px',
                    fontWeight: isActive ? 700 : 500,
                    borderRadius: '6px',
                    cursor: isActive ? 'default' : 'pointer',
                    backgroundColor: isActive ? '#2563EB' : '#1E293B',
                    color: isActive ? '#FFFFFF' : '#CBD5E1',
                    border: isActive ? '1px solid #3B82F6' : '1px solid #475569',
                    transition: 'all 0.15s ease',
                    opacity: isLoading ? 0.7 : 1,
                  }}
                  title={`Đăng nhập nhanh với tài khoản: ${persona.email}`}
                >
                  {isActive && <span>✓</span>}
                  <span>{persona.roleName}</span>
                </button>
              );
            })}
          </div>
        </div>
      </div>
    </header>
  );
};

export default Navbar;
