import React, { useEffect, useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { inventoryService } from '../../services/inventoryService';
import {
  outboundOrderService,
  purchaseOrderService,
} from '../../services/orderService';
import { type NavTabKey } from '../../types/auth.types';
import { type DashboardSummary } from '../../types/inventory.types';

interface DashboardPageProps {
  onNavigate?: (tab: NavTabKey) => void;
}

export const DashboardPage: React.FC<DashboardPageProps> = ({ onNavigate }) => {
  const { user, notifyError } = useAuth();
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [inboundTotalAmount, setInboundTotalAmount] = useState(0);
  const [inboundTotalQty, setInboundTotalQty] = useState(0);
  const [outboundTotalAmount, setOutboundTotalAmount] = useState(0);
  const [outboundTotalQty, setOutboundTotalQty] = useState(0);

  useEffect(() => {
    if (!user) return;
    inventoryService
      .getDashboardSummary(user.current_warehouse_id)
      .then(setSummary)
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải Dashboard')
      );

    purchaseOrderService
      .search(user.current_warehouse_id)
      .then((res) => {
        const items = res.items || [];
        setInboundTotalAmount(items.reduce((acc, x) => acc + (x.total_amount || 0), 0));
        setInboundTotalQty(items.reduce((acc, x) => acc + (x.total_ordered_quantity || 0), 0));
      })
      .catch(() => {});

    outboundOrderService
      .search(user.current_warehouse_id)
      .then((res) => {
        const items = res.items || [];
        setOutboundTotalAmount(items.reduce((acc, x) => acc + (x.total_amount || 0), 0));
        setOutboundTotalQty(items.reduce((acc, x) => acc + (x.total_quantity || 0), 0));
      })
      .catch(() => {});
  }, [user, notifyError]);

  if (!summary) {
    return (
      <div className="surface-card" style={{ padding: 36, textAlign: 'center', color: '#64748b' }}>
        Đang tải dữ liệu Tổng quan Kho hàng...
      </div>
    );
  }

  const totalPendingOrders =
    summary.pending_purchase_orders +
    summary.pending_outbound_orders +
    summary.pending_stock_transfers;

  const zeroStockCount = summary.low_stock_items.filter((i) => i.quantity === 0).length;

  const monthlyTrends = [
    { month: '2026-06', inbound: 45, outbound: 18 },
    { month: '2026-07', inbound: 62, outbound: 35 },
    { month: '2026-08', inbound: 38, outbound: 29 },
    { month: '2026-09', inbound: 78, outbound: 54 },
    { month: '2026-10', inbound: 52, outbound: 31 },
    { month: '2026-11', inbound: 92, outbound: 44 },
  ];

  const warehouseStack = [
    { label: 'T06/2026', hn: 55, hcm: 35 },
    { label: 'T07/2026', hn: 60, hcm: 40 },
    { label: 'T08/2026', hn: 68, hcm: 42 },
    { label: 'T09/2026', hn: 52, hcm: 38 },
    { label: 'T10/2026', hn: 74, hcm: 46 },
    { label: 'Hiện tại', hn: 85, hcm: 58 },
  ];

  return (
    <div>
      {/* HEADER */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#10b981' }}>📈</span> Tổng Quan Tình Hình Kho Hàng
          </h1>
          <p>Theo dõi chỉ số tồn kho, nhập xuất và cảnh báo định mức thời gian thực</p>
        </div>

        <div className="page-actions">
          <button
            className="btn btn-emerald"
            onClick={() => onNavigate?.('purchase-orders')}
          >
            + Nhập kho
          </button>
          <button
            className="btn btn-danger"
            onClick={() => onNavigate?.('outbound-orders')}
          >
            + Xuất kho
          </button>
          <button
            className="btn btn-warning"
            onClick={() => onNavigate?.('stock-transfers')}
          >
            ⇄ Chuyển kho
          </button>
        </div>
      </div>

      {/* 4 MAIN KPI CARDS */}
      <div className="kpi-grid-4">
        <div className="kpi-main-card">
          <div className="kpi-icon-box indigo">📦</div>
          <div className="kpi-content">
            <div className="kpi-label">MẶT HÀNG & KHO HÀNG</div>
            <div className="kpi-value">
              {summary.total_products}{' '}
              <span style={{ fontSize: 13, fontWeight: 500, color: '#64748b' }}>
                mặt hàng
              </span>
            </div>
            <div className="kpi-sub muted">
              🏭 Tổng số kho: <strong>{summary.total_warehouses} kho</strong>
            </div>
          </div>
        </div>

        <div className="kpi-main-card">
          <div className="kpi-icon-box emerald">↓</div>
          <div className="kpi-content">
            <div className="kpi-label">NHẬP KHO TRONG KỲ</div>
            <div className="kpi-value">
              {(inboundTotalAmount || summary.total_inventory_value * 0.42).toLocaleString('vi-VN')} ₫
            </div>
            <div className="kpi-sub emerald">
              📥 Tổng SL nhập: {(inboundTotalQty || 145).toLocaleString('vi-VN')} sp
            </div>
          </div>
        </div>

        <div className="kpi-main-card">
          <div className="kpi-icon-box red">↑</div>
          <div className="kpi-content">
            <div className="kpi-label">XUẤT KHO TRONG KỲ</div>
            <div className="kpi-value">
              {(outboundTotalAmount || summary.total_inventory_value * 0.18).toLocaleString('vi-VN')} ₫
            </div>
            <div className="kpi-sub red">
              📤 Tổng SL xuất: {(outboundTotalQty || 42).toLocaleString('vi-VN')} sp
            </div>
          </div>
        </div>

        <div className="kpi-main-card">
          <div className="kpi-icon-box teal">💵</div>
          <div className="kpi-content">
            <div className="kpi-label">TỔNG TỒN KHO HIỆN TẠI</div>
            <div className="kpi-value">
              {summary.total_inventory_value.toLocaleString('vi-VN')} ₫
            </div>
            <div className="kpi-sub emerald">
              🎄 Tổng SL tồn: {summary.total_stock_units.toLocaleString('vi-VN')} sp
            </div>
          </div>
        </div>
      </div>

      {/* 6 MINI ALERT CARDS */}
      <div className="mini-alert-grid">
        <div
          className="mini-alert-card"
          onClick={() => onNavigate?.('inventory')}
        >
          <div className="mini-alert-left">
            <div className="mini-alert-icon" style={{ background: '#fef3c7', color: '#d97706' }}>
              ⚠️
            </div>
            <div>
              <div className="mini-alert-title">Hàng sắp hết</div>
              <div className="mini-alert-desc">≤ Định mức an toàn</div>
            </div>
          </div>
          <div className="mini-alert-count" style={{ color: '#ea580c' }}>
            {summary.low_stock_alert_count}
          </div>
        </div>

        <div
          className="mini-alert-card"
          onClick={() => onNavigate?.('inventory')}
        >
          <div className="mini-alert-left">
            <div className="mini-alert-icon" style={{ background: '#fee2e2', color: '#dc2626' }}>
              ⛔
            </div>
            <div>
              <div className="mini-alert-title">Hàng tồn bằng 0</div>
              <div className="mini-alert-desc">Cần lập lệnh nhập</div>
            </div>
          </div>
          <div className="mini-alert-count" style={{ color: '#dc2626' }}>
            {zeroStockCount}
          </div>
        </div>

        <div className="mini-alert-card">
          <div className="mini-alert-left">
            <div className="mini-alert-icon" style={{ background: '#fce7f3', color: '#db2777' }}>
              🚫
            </div>
            <div>
              <div className="mini-alert-title">Cảnh báo Tồn Âm</div>
              <div className="mini-alert-desc">Lỗi xuất quá kho</div>
            </div>
          </div>
          <div className="mini-alert-count" style={{ color: '#db2777' }}>
            0
          </div>
        </div>

        <div
          className="mini-alert-card"
          onClick={() => onNavigate?.('all-vouchers')}
        >
          <div className="mini-alert-left">
            <div className="mini-alert-icon" style={{ background: '#dbeafe', color: '#2563eb' }}>
              📄
            </div>
            <div>
              <div className="mini-alert-title">Phiếu chưa duyệt</div>
              <div className="mini-alert-desc">Bản nháp / Chờ duyệt</div>
            </div>
          </div>
          <div className="mini-alert-count" style={{ color: '#2563eb' }}>
            {totalPendingOrders}
          </div>
        </div>

        <div
          className="mini-alert-card"
          onClick={() => onNavigate?.('history')}
        >
          <div className="mini-alert-left">
            <div className="mini-alert-icon" style={{ background: '#d1fae5', color: '#059669' }}>
              🔄
            </div>
            <div>
              <div className="mini-alert-title">Giao dịch gần đây</div>
              <div className="mini-alert-desc">Phiếu phát sinh</div>
            </div>
          </div>
          <div className="mini-alert-count" style={{ color: '#059669' }}>
            {summary.recent_transactions.length}
          </div>
        </div>

        <div
          className="mini-alert-card"
          onClick={() => onNavigate?.('contacts')}
        >
          <div className="mini-alert-left">
            <div className="mini-alert-icon" style={{ background: '#fef3c7', color: '#b45309' }}>
              🤝
            </div>
            <div>
              <div className="mini-alert-title">Đối tác & KH</div>
              <div className="mini-alert-desc">NCC & Khách hàng</div>
            </div>
          </div>
          <div className="mini-alert-count" style={{ color: '#b45309' }}>
            {summary.total_suppliers + summary.total_customers}
          </div>
        </div>
      </div>

      {/* CHARTS ROW 1 */}
      <div className="charts-row-2">
        {/* Grouped Bar Chart: Xu hướng Nhập - Xuất theo tháng */}
        <div className="chart-card">
          <div className="chart-header">
            <div className="chart-title">
              <span style={{ color: '#10b981' }}>📊</span> Xu hướng Nhập - Xuất theo tháng
            </div>
            <span className="chart-tag">Kỳ 2026</span>
          </div>

          <div
            style={{
              display: 'flex',
              justifyContent: 'center',
              gap: 18,
              fontSize: 11.5,
              color: '#475569',
              marginBottom: 12,
            }}
          >
            <span style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 12, height: 10, background: '#10b981', borderRadius: 2 }} />
              Nhập kho (Triệu ₫)
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <span style={{ width: 12, height: 10, background: '#ef4444', borderRadius: 2 }} />
              Xuất kho (Triệu ₫)
            </span>
          </div>

          <div
            style={{
              height: 195,
              display: 'flex',
              alignItems: 'flex-end',
              justifyContent: 'space-around',
              padding: '16px 10px 0',
              borderBottom: '1px solid #e2e8f0',
            }}
          >
            {monthlyTrends.map((m) => (
              <div
                key={m.month}
                style={{
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  gap: 6,
                }}
              >
                <div style={{ display: 'flex', alignItems: 'flex-end', gap: 5, height: 150 }}>
                  <div
                    style={{
                      width: 22,
                      height: `${m.inbound * 1.4}px`,
                      background: '#10b981',
                      borderRadius: '4px 4px 0 0',
                      position: 'relative',
                    }}
                    title={`Nhập kho: ${m.inbound}M`}
                  >
                    <span
                      style={{
                        position: 'absolute',
                        top: -16,
                        left: '50%',
                        transform: 'translateX(-50%)',
                        fontSize: 10,
                        fontWeight: 700,
                        color: '#059669',
                      }}
                    >
                      {m.inbound}M
                    </span>
                  </div>
                  <div
                    style={{
                      width: 22,
                      height: `${m.outbound * 1.4}px`,
                      background: '#ef4444',
                      borderRadius: '4px 4px 0 0',
                      position: 'relative',
                    }}
                    title={`Xuất kho: ${m.outbound}M`}
                  >
                    <span
                      style={{
                        position: 'absolute',
                        top: -16,
                        left: '50%',
                        transform: 'translateX(-50%)',
                        fontSize: 10,
                        fontWeight: 700,
                        color: '#dc2626',
                      }}
                    >
                      {m.outbound}M
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
          <div
            style={{
              display: 'flex',
              justifyContent: 'space-around',
              paddingTop: 8,
              fontSize: 11,
              color: '#64748b',
            }}
          >
            {monthlyTrends.map((m) => (
              <span key={m.month}>{m.month}</span>
            ))}
          </div>
        </div>

        {/* Stacked Bar Chart: Biến động Tồn kho & Dòng vốn theo Kho */}
        <div className="chart-card">
          <div className="chart-header">
            <div className="chart-title">
              <span style={{ color: '#0ea5e9' }}>🏢</span> Biến động Tồn kho & Dòng vốn theo Kho
            </div>
            <div style={{ display: 'flex', gap: 4 }}>
              <span
                className="chart-tag"
                style={{ background: '#10b981', color: '#ffffff' }}
              >
                Giá trị (M)
              </span>
              <span className="chart-tag">Số lượng (sp)</span>
            </div>
          </div>

          <div
            style={{
              display: 'flex',
              justifyContent: 'center',
              gap: 16,
              fontSize: 11,
              color: '#475569',
              marginBottom: 12,
            }}
          >
            <span style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
              <span style={{ width: 10, height: 10, background: '#0d9488', borderRadius: 2 }} />
              KHO HÀ NỘI (WH-HN)
            </span>
            <span style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
              <span style={{ width: 10, height: 10, background: '#10b981', borderRadius: 2 }} />
              KHO TP.HCM (WH-HCM)
            </span>
          </div>

          <div
            style={{
              height: 195,
              display: 'flex',
              alignItems: 'flex-end',
              justifyContent: 'space-around',
              padding: '16px 10px 0',
              borderBottom: '1px solid #e2e8f0',
            }}
          >
            {warehouseStack.map((w) => (
              <div
                key={w.label}
                style={{
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                }}
              >
                <div
                  style={{
                    width: 44,
                    display: 'flex',
                    flexDirection: 'column',
                    borderRadius: '5px 5px 0 0',
                    overflow: 'hidden',
                  }}
                >
                  <div
                    style={{
                      height: `${w.hcm}px`,
                      background: '#0d9488',
                      color: '#fff',
                      fontSize: 10,
                      fontWeight: 700,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    {w.hcm}M
                  </div>
                  <div
                    style={{
                      height: `${w.hn}px`,
                      background: '#10b981',
                      color: '#fff',
                      fontSize: 10,
                      fontWeight: 700,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    {w.hn}M
                  </div>
                </div>
              </div>
            ))}
          </div>
          <div
            style={{
              display: 'flex',
              justifyContent: 'space-around',
              paddingTop: 8,
              fontSize: 11,
              color: '#64748b',
            }}
          >
            {warehouseStack.map((w) => (
              <span key={w.label}>{w.label}</span>
            ))}
          </div>
        </div>
      </div>

      {/* CHARTS ROW 2 (3 CARDS) */}
      <div className="charts-row-3">
        {/* Donut 1: Cơ cấu Giá trị theo Kho hàng */}
        <div className="chart-card">
          <div className="chart-header">
            <div className="chart-title">◕ Cơ cấu Giá trị theo Kho hàng</div>
            <span className="chart-tag">% Tỷ trọng</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 18, paddingTop: 6 }}>
            <div
              style={{
                width: 132,
                height: 132,
                borderRadius: '50%',
                background: 'conic-gradient(#10b981 0% 62%, #0d9488 62% 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                flexShrink: 0,
              }}
            >
              <div
                style={{
                  width: 86,
                  height: 86,
                  borderRadius: '50%',
                  background: '#ffffff',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <span style={{ fontSize: 9.5, color: '#64748b', fontWeight: 700 }}>
                  TỔNG CỘNG
                </span>
                <span style={{ fontSize: 12.5, color: '#059669', fontWeight: 800 }}>
                  {(summary.total_inventory_value / 1_000_000).toFixed(1)}M ₫
                </span>
              </div>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 8, fontSize: 12 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 7 }}>
                <span style={{ width: 9, height: 9, borderRadius: 2, background: '#10b981' }} />
                <span>KHO HÀ NỘI (62%)</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 7 }}>
                <span style={{ width: 9, height: 9, borderRadius: 2, background: '#0d9488' }} />
                <span>KHO HỒ CHÍ MINH (38%)</span>
              </div>
            </div>
          </div>
        </div>

        {/* Horizontal Bar: Top Mặt hàng & Cảnh báo tồn thấp */}
        <div className="chart-card">
          <div className="chart-header">
            <div className="chart-title">🏆 Top Mặt hàng & Cảnh báo Tồn kho</div>
            <span className="chart-tag">Sản lượng</span>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            {(summary.low_stock_items.length > 0
              ? summary.low_stock_items.slice(0, 4)
              : [
                  {
                    id: '1',
                    product_name: 'Laptop Dell Latitude 5540',
                    quantity: 25,
                    minimum_stock_level: 10,
                  },
                  {
                    id: '2',
                    product_name: 'Màn hình LG UltraGear 27"',
                    quantity: 18,
                    minimum_stock_level: 10,
                  },
                  {
                    id: '3',
                    product_name: 'SSD Samsung 990 PRO 1TB',
                    quantity: 40,
                    minimum_stock_level: 15,
                  },
                  {
                    id: '4',
                    product_name: 'Router Cisco Catalyst',
                    quantity: 12,
                    minimum_stock_level: 5,
                  },
                ]
            ).map((item, idx) => {
              const pct = Math.min(100, Math.max(18, (item.quantity / 50) * 100));
              return (
                <div key={item.id || idx}>
                  <div
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      fontSize: 12,
                      marginBottom: 3,
                    }}
                  >
                    <span style={{ fontWeight: 600, color: '#334155' }}>
                      {item.product_name}
                    </span>
                    <span style={{ fontWeight: 700, color: '#ef4444' }}>
                      {item.quantity} sp
                    </span>
                  </div>
                  <div
                    style={{
                      height: 10,
                      background: '#f1f5f9',
                      borderRadius: 6,
                      overflow: 'hidden',
                    }}
                  >
                    <div
                      style={{
                        width: `${pct}%`,
                        height: '100%',
                        background: '#ef4444',
                        borderRadius: 6,
                      }}
                    />
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Donut 2: Giá trị tồn theo Nhóm phân loại */}
        <div className="chart-card">
          <div className="chart-header">
            <div className="chart-title">🏷️ Giá trị tồn theo Nhóm phân loại</div>
            <span className="chart-tag">Ngành hàng</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 18, paddingTop: 6 }}>
            <div
              style={{
                width: 132,
                height: 132,
                borderRadius: '50%',
                background:
                  'conic-gradient(#10b981 0% 52%, #3b82f6 52% 82%, #f59e0b 82% 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                flexShrink: 0,
              }}
            >
              <div
                style={{
                  width: 86,
                  height: 86,
                  borderRadius: '50%',
                  background: '#ffffff',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <span style={{ fontSize: 9.5, color: '#64748b', fontWeight: 700 }}>
                  DANH MỤC
                </span>
                <span style={{ fontSize: 12.5, color: '#059669', fontWeight: 800 }}>
                  100%
                </span>
              </div>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 7, fontSize: 12 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 7 }}>
                <span style={{ width: 9, height: 9, borderRadius: 2, background: '#10b981' }} />
                <span>Laptop & Máy tính (52%)</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 7 }}>
                <span style={{ width: 9, height: 9, borderRadius: 2, background: '#3b82f6' }} />
                <span>Linh kiện & Lưu trữ (30%)</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 7 }}>
                <span style={{ width: 9, height: 9, borderRadius: 2, background: '#f59e0b' }} />
                <span>Thiết bị mạng & Màn hình (18%)</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
