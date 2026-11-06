import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { Navbar } from './components/Navbar';
import { LoginPage } from './pages/LoginPage';
import { PurchaseOrderListPage } from './pages/PurchaseOrderListPage';
import { CreatePurchaseOrderPage } from './pages/CreatePurchaseOrderPage';
import { PurchaseOrderDetailPage } from './pages/PurchaseOrderDetailPage';
import './App.css';

// Placeholder view for routes in upcoming tasks
const PlaceholderPage: React.FC<{ title: string; subtitle: string; icon: string }> = ({
  title,
  subtitle,
  icon,
}) => (
  <div className="ewms-container" style={{ textAlign: 'center', padding: '64px 16px' }}>
    <div className="ewms-card" style={{ maxWidth: '600px', margin: '0 auto', padding: '40px 24px' }}>
      <div style={{ fontSize: '48px', marginBottom: '16px' }}>{icon}</div>
      <h2 style={{ fontSize: '22px', fontWeight: 700, color: '#0F172A', marginBottom: '8px' }}>
        {title}
      </h2>
      <p style={{ fontSize: '14px', color: '#64748B', marginBottom: '24px' }}>
        {subtitle}
      </p>
      <a href="/purchase-orders" className="ewms-btn ewms-btn-primary">
        ← Về danh sách đơn mua hàng
      </a>
    </div>
  </div>
);

export const App: React.FC = () => {
  return (
    <BrowserRouter>
      <AuthProvider>
        <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh', backgroundColor: '#F8FAFC' }}>
          <Navbar />
          <main style={{ flex: 1 }}>
            <Routes>
              {/* Authentication */}
              <Route path="/login" element={<LoginPage />} />

              {/* Purchase Orders (UC09, UC10, UC11) */}
              <Route path="/purchase-orders" element={<PurchaseOrderListPage />} />
              <Route path="/purchase-orders/create" element={<CreatePurchaseOrderPage />} />
              <Route path="/purchase-orders/:id" element={<PurchaseOrderDetailPage />} />

              {/* Receiving (UC12, UC13, UC14 - Task 10) */}
              <Route
                path="/receiving"
                element={
                  <PlaceholderPage
                    icon="📦"
                    title="Phân hệ Nhận hàng & Kiểm đếm (Receiving)"
                    subtitle="Chức năng nhận hàng, kiểm đếm số lượng và nhập kho vật lý (Task 10)."
                  />
                }
              />
              <Route
                path="/receiving/:id"
                element={
                  <PlaceholderPage
                    icon="🔍"
                    title="Chi tiết kiểm đếm hàng nhận"
                    subtitle="Giao diện kiểm tra số lượng và xác nhận nhập kho (Task 10)."
                  />
                }
              />

              {/* Inventory (UC15, UC16 - Task 11) */}
              <Route
                path="/inventory"
                element={
                  <PlaceholderPage
                    icon="📊"
                    title="Báo cáo Tồn kho & Lịch sử Giao dịch"
                    subtitle="Xem mức tồn theo từng kho và lịch sử ghi nhận giao dịch IN (Task 11)."
                  />
                }
              />

              {/* Root and Fallback redirects */}
              <Route path="/" element={<Navigate to="/purchase-orders" replace />} />
              <Route path="*" element={<Navigate to="/purchase-orders" replace />} />
            </Routes>
          </main>
          <footer
            style={{
              padding: '16px',
              textAlign: 'center',
              fontSize: '12px',
              color: '#94A3B8',
              borderTop: '1px solid #E2E8F0',
              backgroundColor: '#FFFFFF',
            }}
          >
            EWMS - Enterprise Electronic Warehouse Management System &copy; 2026
          </footer>
        </div>
      </AuthProvider>
    </BrowserRouter>
  );
};

export default App;
