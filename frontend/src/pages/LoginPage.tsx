import React, { useState } from 'react';
import { useNavigate, Navigate } from 'react-router-dom';
import { useAuth, DEMO_PERSONAS, type DemoPersonaRole } from '../context/AuthContext';
import type { ApiError } from '../types/models';

export const LoginPage: React.FC = () => {
  const { login, quickLogin, isAuthenticated, isLoading, error: authError, clearError } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [localError, setLocalError] = useState<{ code: string; message: string } | null>(null);
  const [submittingRole, setSubmittingRole] = useState<string | null>(null);

  if (isAuthenticated) {
    return <Navigate to="/purchase-orders" replace />;
  }

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocalError(null);
    clearError();

    if (!email.trim() || !password.trim()) {
      setLocalError({
        code: 'MSG01',
        message: 'Vui lòng nhập đầy đủ Email và Mật khẩu.',
      });
      return;
    }

    try {
      setSubmittingRole('form');
      await login({ email: email.trim(), password });
      navigate('/purchase-orders');
    } catch (err: any) {
      const apiErr = err as ApiError;
      setLocalError({
        code: apiErr?.code || 'MSG-EMP-01',
        message: apiErr?.message || 'Email hoặc mật khẩu không chính xác.',
      });
    } finally {
      setSubmittingRole(null);
    }
  };

  const handleDemoClick = async (roleKey: DemoPersonaRole) => {
    setLocalError(null);
    clearError();

    try {
      setSubmittingRole(roleKey);
      await quickLogin(roleKey);
      navigate('/purchase-orders');
    } catch (err: any) {
      const apiErr = err as ApiError;
      setLocalError({
        code: apiErr?.code || 'MSG-EMP-01',
        message: apiErr?.message || 'Đăng nhập vai trò demo thất bại.',
      });
    } finally {
      setSubmittingRole(null);
    }
  };

  const displayError = localError || (authError ? { code: 'MSG-EMP-01', message: authError } : null);

  const personaConfig: Record<DemoPersonaRole, { avatar: string; color: string; bg: string; border: string }> = {
    purchase_staff: {
      avatar: '🛒',
      color: '#1E40AF',
      bg: '#DBEAFE',
      border: '#93C5FD',
    },
    warehouse_manager: {
      avatar: '👔',
      color: '#6B21A8',
      bg: '#F3E8FF',
      border: '#D8B4FE',
    },
    warehouse_staff: {
      avatar: '📦',
      color: '#9A3412',
      bg: '#FFEDD5',
      border: '#FDBA74',
    },
  };

  return (
    <div
      style={{
        minHeight: 'calc(100vh - 120px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '32px 16px',
        backgroundColor: '#F8FAFC',
      }}
    >
      <div
        style={{
          width: '100%',
          maxWidth: '540px',
        }}
      >
        {/* Brand header */}
        <div style={{ textAlign: 'center', marginBottom: '28px' }}>
          <div
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              justifyContent: 'center',
              width: '54px',
              height: '54px',
              borderRadius: '12px',
              backgroundColor: '#2563EB',
              color: '#FFFFFF',
              fontWeight: 800,
              fontSize: '24px',
              marginBottom: '12px',
              boxShadow: '0 4px 6px -1px rgba(37, 99, 235, 0.3)',
            }}
          >
            E
          </div>
          <h1 style={{ fontSize: '24px', fontWeight: 700, color: '#0F172A' }}>
            Hệ Thống EWMS
          </h1>
          <p style={{ fontSize: '14px', color: '#64748B', marginTop: '4px' }}>
            Quản lý Kho & Mua Hàng Điện Tử Doanh Nghiệp
          </p>
        </div>

        {/* Error Alert Banner */}
        {displayError && (
          <div
            className="ewms-alert ewms-alert-error"
            role="alert"
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'flex-start',
            }}
          >
            <div style={{ display: 'flex', alignItems: 'flex-start', gap: '8px' }}>
              <span style={{ fontSize: '18px', lineHeight: 1 }}>⚠️</span>
              <div>
                <span className="ewms-alert-code" style={{ marginRight: '6px' }}>
                  {displayError.code}
                </span>
                <span style={{ fontWeight: 500 }}>{displayError.message}</span>
              </div>
            </div>
            <button
              type="button"
              onClick={() => {
                setLocalError(null);
                clearError();
              }}
              style={{
                background: 'transparent',
                border: 'none',
                cursor: 'pointer',
                color: '#991B1B',
                fontWeight: 700,
                fontSize: '16px',
                lineHeight: 1,
              }}
              title="Đóng thông báo"
            >
              ×
            </button>
          </div>
        )}

        {/* Login Card */}
        <div className="ewms-card" style={{ marginBottom: '24px' }}>
          <h2
            style={{
              fontSize: '18px',
              fontWeight: 600,
              color: '#0F172A',
              marginBottom: '18px',
              borderBottom: '1px solid #E2E8F0',
              paddingBottom: '12px',
            }}
          >
            Đăng nhập tài khoản
          </h2>

          <form onSubmit={handleFormSubmit}>
            <div className="ewms-form-group">
              <label htmlFor="login-email" className="ewms-label">
                Địa chỉ Email
              </label>
              <input
                id="login-email"
                type="email"
                className="ewms-input"
                placeholder="vd: staff_purchase@ewms.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={isLoading}
                required
              />
            </div>

            <div className="ewms-form-group" style={{ marginBottom: '20px' }}>
              <label htmlFor="login-password" className="ewms-label">
                Mật khẩu
              </label>
              <input
                id="login-password"
                type="password"
                className="ewms-input"
                placeholder="Nhập mật khẩu"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={isLoading}
                required
              />
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="ewms-btn ewms-btn-primary"
              style={{ width: '100%', padding: '11px', fontSize: '15px' }}
            >
              {submittingRole === 'form' ? (
                <>
                  <span className="ewms-spinner" />
                  <span>Đang xác thực...</span>
                </>
              ) : (
                'Đăng nhập'
              )}
            </button>
          </form>
        </div>

        {/* Demo Personas Section */}
        <div className="ewms-card">
          <div style={{ marginBottom: '14px' }}>
            <h3
              style={{
                fontSize: '14px',
                fontWeight: 700,
                textTransform: 'uppercase',
                letterSpacing: '0.05em',
                color: '#475569',
              }}
            >
              🚀 Đăng nhập nhanh theo vai trò Demo
            </h3>
            <p style={{ fontSize: '13px', color: '#64748B', marginTop: '2px' }}>
              Nhấp vào một trong các tài khoản mẫu dưới đây để đăng nhập ngay lập tức:
            </p>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
            {(Object.keys(DEMO_PERSONAS) as DemoPersonaRole[]).map((roleKey) => {
              const persona = DEMO_PERSONAS[roleKey];
              const conf = personaConfig[roleKey];
              const isCurrentSubmitting = submittingRole === roleKey;

              return (
                <button
                  key={roleKey}
                  type="button"
                  disabled={isLoading}
                  onClick={() => handleDemoClick(roleKey)}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    padding: '12px 14px',
                    borderRadius: '8px',
                    backgroundColor: '#FFFFFF',
                    border: '1px solid #E2E8F0',
                    cursor: isLoading ? 'not-allowed' : 'pointer',
                    textAlign: 'left',
                    transition: 'all 0.15s ease',
                    boxShadow: '0 1px 2px rgba(0,0,0,0.04)',
                    opacity: isLoading && !isCurrentSubmitting ? 0.6 : 1,
                  }}
                  onMouseEnter={(e) => {
                    if (!isLoading) {
                      e.currentTarget.style.borderColor = '#3B82F6';
                      e.currentTarget.style.backgroundColor = '#F8FAFC';
                    }
                  }}
                  onMouseLeave={(e) => {
                    if (!isLoading) {
                      e.currentTarget.style.borderColor = '#E2E8F0';
                      e.currentTarget.style.backgroundColor = '#FFFFFF';
                    }
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                    <div
                      style={{
                        width: '40px',
                        height: '40px',
                        borderRadius: '8px',
                        backgroundColor: conf.bg,
                        border: `1px solid ${conf.border}`,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '20px',
                        flexShrink: 0,
                      }}
                    >
                      {conf.avatar}
                    </div>

                    <div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <span style={{ fontWeight: 600, fontSize: '14px', color: '#0F172A' }}>
                          {persona.roleName}
                        </span>
                        <span
                          style={{
                            fontSize: '11px',
                            fontWeight: 600,
                            padding: '1px 6px',
                            borderRadius: '4px',
                            backgroundColor: conf.bg,
                            color: conf.color,
                            border: `1px solid ${conf.border}`,
                          }}
                        >
                          {persona.employeeCode}
                        </span>
                      </div>
                      <div style={{ fontSize: '12px', color: '#64748B', marginTop: '2px' }}>
                        {persona.email}
                      </div>
                    </div>
                  </div>

                  <div style={{ flexShrink: 0 }}>
                    {isCurrentSubmitting ? (
                      <span className="ewms-spinner ewms-spinner-dark" />
                    ) : (
                      <span
                        style={{
                          fontSize: '13px',
                          color: '#2563EB',
                          fontWeight: 600,
                          display: 'flex',
                          alignItems: 'center',
                          gap: '4px',
                        }}
                      >
                        Chọn →
                      </span>
                    )}
                  </div>
                </button>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
