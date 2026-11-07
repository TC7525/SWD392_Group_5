import React, { useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { useAuth } from '../../contexts/AuthContext';
import {
  outboundOrderService,
  purchaseOrderService,
  stockTransferService,
} from '../../services/orderService';
import { type NavTabKey } from '../../types/auth.types';

interface UnifiedVoucher {
  id: string;
  type: 'INBOUND' | 'OUTBOUND' | 'TRANSFER';
  code: string;
  date: string;
  warehouse_name: string;
  partner_name: string;
  note: string;
  creator: string;
  quantity: number;
  status_name: string;
}

interface AllVouchersPageProps {
  onNavigate?: (tab: NavTabKey) => void;
}

export const AllVouchersPage: React.FC<AllVouchersPageProps> = ({
  onNavigate,
}) => {
  const { user, notifyError, notifySuccess } = useAuth();
  const [vouchers, setVouchers] = useState<UnifiedVoucher[]>([]);
  const [fromDate, setFromDate] = useState('2026-01-01');
  const [toDate, setToDate] = useState('2026-12-31');
  const [filterType, setFilterType] = useState('ALL');
  const [keyword, setKeyword] = useState('');
  const [pageSize, setPageSize] = useState('10');

  useEffect(() => {
    if (!user) return;
    Promise.all([
      purchaseOrderService.search(user.current_warehouse_id),
      outboundOrderService.search(user.current_warehouse_id),
      stockTransferService.search(),
    ])
      .then(([poRes, soRes, tfRes]) => {
        const list: UnifiedVoucher[] = [
          ...(poRes.items || []).map((po) => ({
            id: `po-${po.id}`,
            type: 'INBOUND' as const,
            code: po.purchase_order_code,
            date: po.order_date,
            warehouse_name: po.warehouse_name,
            partner_name: po.supplier_name,
            note: po.note || 'Nhập kho thiết bị từ Nhà cung cấp',
            creator: 'Quản trị viên',
            quantity: po.total_ordered_quantity,
            status_name: po.status_name,
          })),
          ...(soRes.items || []).map((so) => ({
            id: `so-${so.id}`,
            type: 'OUTBOUND' as const,
            code: so.order_code,
            date: so.order_date,
            warehouse_name: so.warehouse_name,
            partner_name: so.customer_name,
            note: so.note || 'Xuất kho giao hàng cho Khách hàng',
            creator: 'Quản trị viên',
            quantity: so.total_quantity,
            status_name: so.status_name,
          })),
          ...(tfRes.items || []).map((tf) => ({
            id: `tf-${tf.id}`,
            type: 'TRANSFER' as const,
            code: tf.stock_transfer_code,
            date: tf.transfer_date,
            warehouse_name: tf.source_warehouse_name,
            partner_name: `→ ${tf.destination_warehouse_name}`,
            note: tf.note || 'Điều chuyển kho nội bộ',
            creator: 'Quản trị viên',
            quantity: tf.total_quantity,
            status_name: tf.status_name,
          })),
        ];

        list.sort(
          (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
        );
        setVouchers(list);
      })
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải Tổng hợp phiếu')
      );
  }, [user, notifyError]);

  const filtered = vouchers.filter((v) => {
    const matchType = filterType === 'ALL' || v.type === filterType;
    const matchKw =
      !keyword ||
      v.code.toLowerCase().includes(keyword.toLowerCase()) ||
      v.partner_name.toLowerCase().includes(keyword.toLowerCase()) ||
      v.note.toLowerCase().includes(keyword.toLowerCase());
    return matchType && matchKw;
  });

  return (
    <div>
      {/* PAGE HEADER (Screen 8) */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#3b82f6' }}>📋</span> Quản Lý Tổng Hợp Phiếu
          </h1>
          <p>
            Tổng hợp toàn bộ phiếu Nhập kho, Xuất kho và Chuyển kho, theo dõi luồng giao dịch và trạng thái phê duyệt
          </p>
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
            + Chuyển kho
          </button>
          <button
            className="btn btn-outline"
            onClick={() => notifySuccess('Đã xuất Tổng hợp phiếu ra Excel!')}
          >
            📗 Excel
          </button>
        </div>
      </div>

      {/* MAIN TABLE CARD */}
      <div className="surface-card">
        <div className="filter-bar">
          <div className="filter-group">
            <span>Từ:</span>
            <input
              type="date"
              className="filter-input"
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
            />
          </div>
          <div className="filter-group">
            <span>Đến:</span>
            <input
              type="date"
              className="filter-input"
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
            />
          </div>

          <CustomSelect
            variant="filter"
            value={filterType}
            onChange={setFilterType}
            options={[
              { value: 'ALL', label: 'Tất cả loại phiếu' },
              { value: 'INBOUND', label: '↓ Phiếu Nhập kho' },
              { value: 'OUTBOUND', label: '↑ Phiếu Xuất kho' },
              { value: 'TRANSFER', label: '⇄ Phiếu Chuyển kho' },
            ]}
          />

          <div className="search-box-wrapper">
            <span className="search-box-icon">🔍</span>
            <input
              className="search-box-input"
              placeholder="Tìm số phiếu, đối tượng, diễn giải..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </div>
        </div>

        <div className="table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>LOẠI PHIẾU</th>
                <th>SỐ PHIẾU</th>
                <th>NGÀY CHỨNG TỪ</th>
                <th>KHO HÀNG</th>
                <th>ĐỐI TƯỢNG</th>
                <th>DIỄN GIẢI</th>
                <th>NGƯỜI LẬP</th>
                <th style={{ textAlign: 'right' }}>TỔNG SỐ LƯỢNG</th>
                <th>TRẠNG THÁI</th>
                <th style={{ textAlign: 'center' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((v) => (
                <tr key={v.id}>
                  <td>
                    {v.type === 'INBOUND' && (
                      <span className="status-pill emerald">↓ Nhập</span>
                    )}
                    {v.type === 'OUTBOUND' && (
                      <span className="status-pill danger">↑ Xuất</span>
                    )}
                    {v.type === 'TRANSFER' && (
                      <span className="status-pill warning">⇄ Chuyển</span>
                    )}
                  </td>
                  <td>
                    <span className="voucher-code">{v.code}</span>
                  </td>
                  <td>{new Date(v.date).toLocaleDateString('vi-VN')}</td>
                  <td style={{ fontWeight: 600 }}>
                    {v.warehouse_name.toUpperCase()}
                  </td>
                  <td>{v.partner_name}</td>
                  <td style={{ color: '#475569' }}>{v.note}</td>
                  <td>{v.creator}</td>
                  <td style={{ textAlign: 'right', fontWeight: 700 }}>
                    {v.quantity.toLocaleString('vi-VN')}
                  </td>
                  <td>
                    <span className="status-pill emerald">{v.status_name}</span>
                  </td>
                  <td style={{ textAlign: 'center' }}>
                    <button
                      className="icon-action-btn"
                      title="In phiếu"
                      onClick={() => window.print()}
                    >
                      🖨️
                    </button>
                    <button
                      className="icon-action-btn"
                      title="Mở phân hệ tương ứng"
                      onClick={() =>
                        onNavigate?.(
                          v.type === 'INBOUND'
                            ? 'purchase-orders'
                            : v.type === 'OUTBOUND'
                            ? 'outbound-orders'
                            : 'stock-transfers'
                        )
                      }
                    >
                      ✏️
                    </button>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && (
                <tr>
                  <td colSpan={10} style={{ textAlign: 'center', padding: 24, color: '#64748b' }}>
                    Không tìm thấy chứng từ nào phù hợp.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        <div className="pagination-bar">
          <span>
            Hiển thị <strong>1 - {filtered.length}</strong> trên tổng số{' '}
            <strong>{filtered.length}</strong> dòng
          </span>
          <div className="pagination-controls">
            <span>Dòng/trang:</span>
            <CustomSelect
              variant="sm"
              value={pageSize}
              onChange={setPageSize}
              options={[
                { value: '10', label: '10' },
                { value: '20', label: '20' },
              ]}
            />
            <button className="page-btn">«</button>
            <button className="page-btn active">1</button>
            <button className="page-btn">»</button>
          </div>
        </div>
      </div>
    </div>
  );
};
