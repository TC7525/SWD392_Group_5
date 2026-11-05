import React, { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { type NavTabKey } from '../../types/auth.types';
import { Header } from './Header';
import { Sidebar } from './Sidebar';

interface MainLayoutProps {
  activeTab: NavTabKey;
  onSelectTab: (tab: NavTabKey) => void;
  children: React.ReactNode;
}

export const MainLayout: React.FC<MainLayoutProps> = ({
  activeTab,
  onSelectTab,
  children,
}) => {
  const { errorMsg, successMsg, notifyError, notifySuccess } = useAuth();
  const [collapsed, setCollapsed] = useState(false);

  return (
    <div className="app-layout">
      <Sidebar
        activeTab={activeTab}
        onSelectTab={onSelectTab}
        collapsed={collapsed}
      />
      <div className="main-area">
        <Header
          activeTab={activeTab}
          onToggleSidebar={() => setCollapsed((c) => !c)}
          onSelectTab={onSelectTab}
        />
        <main className="content-container">
          {errorMsg && (
            <div className="alert-banner error">
              <span>⚠️ {errorMsg}</span>
              <button
                style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'inherit', fontWeight: 700 }}
                onClick={() => notifyError('')}
              >
                ✕
              </button>
            </div>
          )}
          {successMsg && (
            <div className="alert-banner success">
              <span>✓ {successMsg}</span>
              <button
                style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'inherit', fontWeight: 700 }}
                onClick={() => notifySuccess('')}
              >
                ✕
              </button>
            </div>
          )}
          {children}
        </main>
      </div>
    </div>
  );
};
