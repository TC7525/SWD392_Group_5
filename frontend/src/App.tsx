import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { Navbar } from './components/Navbar';
import { LoginPage } from './pages/LoginPage';
import { PurchaseOrderListPage } from './pages/PurchaseOrderListPage';
import { CreatePurchaseOrderPage } from './pages/CreatePurchaseOrderPage';
import { PurchaseOrderDetailPage } from './pages/PurchaseOrderDetailPage';
import { ReceivingListPage } from './pages/ReceivingListPage';
import { ReceivingDetailPage } from './pages/ReceivingDetailPage';
import { InventoryListPage } from './pages/InventoryListPage';
import './App.css';

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
              <Route path="/receiving" element={<ReceivingListPage />} />
              <Route path="/receiving/:id" element={<ReceivingDetailPage />} />

              {/* Inventory (UC15, UC16 - Task 10 / Task 11) */}
              <Route path="/inventory" element={<InventoryListPage />} />

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
