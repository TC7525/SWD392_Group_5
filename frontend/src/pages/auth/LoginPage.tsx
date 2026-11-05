import React, { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';

export const LoginPage: React.FC = () => {
  const { login, errorMsg } = useAuth();
  const [username, setUsername] = useState('EMP001');
  const [password, setPassword] = useState('Admin@123');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await login(username, password);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-screen">
      <div className="login-card">
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 12,
            marginBottom: 8,
          }}
        >
          <div className="brand-logo-box" style={{ width: 42, height: 42, fontSize: 20 }}>
            🏢
          </div>
          <div style={{ textAlign: 'left' }}>
            <h2 style={{ fontSize: 20, fontWeight: 800, color: '#0f172a' }}>
              Quản Lý Kho Điện Tử
            </h2>
            <div style={{ fontSize: 12, color: '#10b981', fontWeight: 700 }}>
              EWMS Enterprise • Version 3.0
            </div>
          </div>
        </div>

        <p
          style={{
            color: '#64748b',
            fontSize: 12.5,
            marginBottom: 20,
            textAlign: 'center',
          }}
        >
          Hệ thống Quản lý Nhập - Xuất - Tồn kho Đa Chi Nhánh (SWD392 Group 5)
        </p>

        {errorMsg && <div className="alert-banner error">{errorMsg}</div>}

        <div
          style={{
            fontSize: 11.5,
            fontWeight: 600,
            color: '#64748b',
            marginBottom: 6,
          }}
        >
          Chọn nhanh tài khoản mẫu (Mật khẩu: Admin@123):
        </div>
        <div className="quick-accounts">
          {[
            ['EMP001', 'Quản trị viên (Admin)'],
            ['EMP002', 'Quản lý kho (Manager)'],
            ['EMP003', 'Thủ kho (Staff)'],
            ['EMP004', 'Kinh doanh (Sales)'],
            ['EMP005', 'Mua hàng (Purchase)'],
          ].map(([code, label]) => (
            <button
              key={code}
              type="button"
              className="quick-account-chip"
              onClick={() => {
                setUsername(code);
                setPassword('Admin@123');
              }}
            >
              {label} ({code})
            </button>
          ))}
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group" style={{ marginBottom: 14 }}>
            <label>
              Mã nhân viên hoặc Email <span className="req">*</span>
            </label>
            <input
              className="form-control"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="VD: EMP001 hoặc admin@ewms.vn"
              required
            />
          </div>

          <div className="form-group" style={{ marginBottom: 20 }}>
            <label>
              Mật khẩu <span className="req">*</span>
            </label>
            <input
              type="password"
              className="form-control"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button
            type="submit"
            className="btn btn-emerald"
            style={{ width: '100%', height: 40, fontSize: 14 }}
            disabled={loading}
          >
            {loading ? 'Đang xác thực...' : '↪ Đăng nhập hệ thống'}
          </button>
        </form>
      </div>
    </div>
  );
};
