import React, { useState } from 'react';
import './App.css';
import { MainLayout } from './components/layout/MainLayout';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { LoginPage } from './pages/auth/LoginPage';
import { ContactPage } from './pages/contact/ContactPage';
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { PurchaseOrderPage } from './pages/inbound/PurchaseOrderPage';
import { InventoryPage } from './pages/inventory/InventoryPage';
import { AllVouchersPage } from './pages/order/AllVouchersPage';
import { OutboundOrderPage } from './pages/outbound/OutboundOrderPage';
import { ProductCategoryPage } from './pages/product/ProductCategoryPage';
import { ReportHistoryPage } from './pages/report/ReportHistoryPage';
import { PrintTemplatePage } from './pages/template/PrintTemplatePage';
import { StockTransferPage } from './pages/transfer/StockTransferPage';
import { UserAccessLogPage } from './pages/user/UserAccessLogPage';
import { WarehousePage } from './pages/warehouse/WarehousePage';
import { type NavTabKey } from './types/auth.types';

const AppContent: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<NavTabKey>('dashboard');

  if (!user) {
    return <LoginPage />;
  }

  return (
    <MainLayout activeTab={activeTab} onSelectTab={setActiveTab}>
      {activeTab === 'dashboard' && <DashboardPage onNavigate={setActiveTab} />}
      {activeTab === 'products' && <ProductCategoryPage />}
      {activeTab === 'warehouses' && <WarehousePage />}
      {activeTab === 'contacts' && <ContactPage />}
      {activeTab === 'purchase-orders' && <PurchaseOrderPage />}
      {activeTab === 'outbound-orders' && <OutboundOrderPage />}
      {activeTab === 'stock-transfers' && <StockTransferPage />}
      {activeTab === 'all-vouchers' && (
        <AllVouchersPage onNavigate={setActiveTab} />
      )}
      {activeTab === 'inventory' && <InventoryPage />}
      {activeTab === 'reports' && <ReportHistoryPage mode="reports" />}
      {activeTab === 'history' && <ReportHistoryPage mode="history" />}
      {activeTab === 'templates' && <PrintTemplatePage />}
      {activeTab === 'users' && <UserAccessLogPage />}
      {activeTab === 'settings' && <UserAccessLogPage />}
    </MainLayout>
  );
};

export function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
