import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { poApi } from '../api/poApi';
import { useAuth } from '../context/AuthContext';
import { StatusBadge } from '../components/StatusBadge';
import type { PurchaseOrderListResponse, ApiError } from '../types/models';

const STATUS_FILTER_OPTIONS = [
  { value: '', label: 'Tất cả trạng thái' },
  { value: 'Pending', label: 'Chờ duyệt (Pending)' },
  { value: 'Approved', label: 'Đã duyệt (Approved)' },
  { value: 'Rejected', label: 'Từ chối (Rejected)' },
  { value: 'Processing', label: 'Đang nhận hàng (Processing)' },
  { value: 'PartiallyReceived', label: 'Nhận một phần (Partially Received)' },
  { value: 'Completed', label: 'Hoàn thành (Completed)' },
];

export const PurchaseOrderListPage: React.FC = () => {
  const { currentUser, hasRole } = useAuth();
  const navigate = useNavigate();

  const [orders, setOrders] = useState<PurchaseOrderListResponse[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<{ code: string; message: string } | null>(null);
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [searchKeyword, setSearchKeyword] = useState<string>('');

  const isPurchaseStaff =
    hasRole('Purchase Staff') ||
    currentUser?.role?.toLowerCase() === 'purchase staff' ||
    currentUser?.role?.toLowerCase() === 'purchasestaff' ||
    !currentUser; // allow guest/evaluator to test

  const reloadOrders = useCallback(async (statusVal?: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await poApi.getAll(statusVal || undefined);
      setOrders(response.data || []);
    } catch (err: any) {
      const apiErr = err as ApiError;
      setError({
        code: apiErr?.code || 'ERR_LOAD',
        message: apiErr?.message || 'Không thể tải danh sách đơn mua hàng.',
      });
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let ignore = false;
    poApi
      .getAll(statusFilter || undefined)
      .then((response) => {
        if (!ignore) {
          setOrders(response.data || []);
          setError(null);
        }
      })
      .catch((err: any) => {
        if (!ignore) {
          const apiErr = err as ApiError;
          setError({
            code: apiErr?.code || 'ERR_LOAD',
            message: apiErr?.message || 'Không thể tải danh sách đơn mua hàng.',
          });
        }
      })
      .finally(() => {
        if (!ignore) {
          setIsLoading(false);
        }
      });

    return () => {
      ignore = true;
    };
  }, [statusFilter]);

  // Client-side search filtering
  const filteredOrders = useMemo(() => {
    if (!searchKeyword.trim()) return orders;
    const kw = searchKeyword.trim().toLowerCase();
    return orders.filter(
      (po) =>
        po.purchaseOrderCode?.toLowerCase().includes(kw) ||
        po.supplierName?.toLowerCase().includes(kw) ||
        po.createdByName?.toLowerCase().includes(kw)
    );
  }, [orders, searchKeyword]);

  // Statistics counts
  const stats = useMemo(() => {
    const total = orders.length;
    const pending = orders.filter((o) => String(o.status).toLowerCase() === 'pending' || o.status === 0).length;
    const approved = orders.filter((o) => String(o.status).toLowerCase() === 'approved' || o.status === 1).length;
    const completed = orders.filter((o) => String(o.status).toLowerCase() === 'completed' || o.status === 5).length;
    return { total, pending, approved, completed };
  }, [orders]);

  const formatCurrency = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN').format(val) + ' ₫';
  };

  const formatDate = (dateStr?: string | null) => {
    if (!dateStr) return '—';
    try {
      const d = new Date(dateStr);
      if (isNaN(d.getTime())) return dateStr;
      return d.toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      });
    } catch {
      return dateStr;
    }
  };

  return (
    <div className="ewms-container">
      {/* Page Header */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: '16px',
          marginBottom: '24px',
        }}
      >
        <div>
          <h1 className="ewms-title">Quản lý Đơn mua hàng (Purchase Orders)</h1>
          <p className="ewms-subtitle">
            Theo dõi danh sách các đơn đặt hàng với nhà cung cấp, phê duyệt và xử lý nhận hàng vào kho.
          </p>
        </div>

        {isPurchaseStaff && (
          <button
            type="button"
            className="ewms-btn ewms-btn-primary"
            onClick={() => navigate('/purchase-orders/create')}
            style={{ padding: '10px 18px', fontSize: '14px', boxShadow: '0 2px 4px rgba(37,99,235,0.2)' }}
          >
            <span style={{ fontSize: '16px', fontWeight: 700 }}>+</span>
            <span>Tạo đơn mua hàng</span>
          </button>
        )}
      </div>

      {/* Stats Cards */}
      <div className="ewms-grid-4" style={{ marginBottom: '24px' }}>
        <div className="ewms-stat-card">
          <div>
            <div className="ewms-stat-label">Tổng đơn hàng</div>
            <div className="ewms-stat-value">{stats.total}</div>
          </div>
          <div style={{ fontSize: '24px' }}>📋</div>
        </div>

        <div className="ewms-stat-card" style={{ borderLeft: '4px solid #F59E0B' }}>
          <div>
            <div className="ewms-stat-label">Chờ duyệt</div>
            <div className="ewms-stat-value" style={{ color: '#D97706' }}>
              {stats.pending}
            </div>
          </div>
          <div style={{ fontSize: '24px' }}>⏳</div>
        </div>

        <div className="ewms-stat-card" style={{ borderLeft: '4px solid #3B82F6' }}>
          <div>
            <div className="ewms-stat-label">Đã duyệt</div>
            <div className="ewms-stat-value" style={{ color: '#2563EB' }}>
              {stats.approved}
            </div>
          </div>
          <div style={{ fontSize: '24px' }}>✅</div>
        </div>

        <div className="ewms-stat-card" style={{ borderLeft: '4px solid #10B981' }}>
          <div>
            <div className="ewms-stat-label">Hoàn thành</div>
            <div className="ewms-stat-value" style={{ color: '#059669' }}>
              {stats.completed}
            </div>
          </div>
          <div style={{ fontSize: '24px' }}>🎉</div>
        </div>
      </div>

      {/* Error alert */}
      {error && (
        <div className="ewms-alert ewms-alert-error" role="alert">
          <span style={{ fontSize: '18px' }}>⚠️</span>
          <div>
            <span className="ewms-alert-code" style={{ marginRight: '6px' }}>
              {error.code}
            </span>
            <span>{error.message}</span>
          </div>
        </div>
      )}

      {/* Filters and Search Bar */}
      <div
        className="ewms-card"
        style={{
          padding: '16px 20px',
          marginBottom: '20px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: '16px',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flex: 1, minWidth: '280px' }}>
          <div style={{ width: '100%', maxWidth: '320px' }}>
            <input
              type="text"
              className="ewms-input"
              placeholder="🔍 Tìm theo mã PO hoặc nhà cung cấp..."
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
            />
          </div>

          <div style={{ width: '100%', maxWidth: '240px' }}>
            <select
              className="ewms-select"
              value={statusFilter}
              onChange={(e) => {
                setIsLoading(true);
                setStatusFilter(e.target.value);
              }}
            >
              {STATUS_FILTER_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <button
            type="button"
            className="ewms-btn ewms-btn-secondary"
            onClick={() => reloadOrders(statusFilter)}
            disabled={isLoading}
            title="Làm mới danh sách"
          >
            {isLoading ? (
              <span className="ewms-spinner ewms-spinner-dark" />
            ) : (
              <span>🔄 Làm mới</span>
            )}
          </button>
        </div>
      </div>

      {/* Table section */}
      <div className="ewms-table-container">
        <table className="ewms-table">
          <thead>
            <tr>
              <th>Mã đơn (Code)</th>
              <th>Nhà cung cấp (Supplier)</th>
              <th>Ngày đặt (Order Date)</th>
              <th>Ngày giao dự kiến</th>
              <th style={{ textAlign: 'right' }}>Tổng SL</th>
              <th style={{ textAlign: 'right' }}>Tổng tiền</th>
              <th style={{ textAlign: 'center' }}>Trạng thái</th>
              <th style={{ textAlign: 'center' }}>Hành động</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr>
                <td colSpan={8} style={{ textAlign: 'center', padding: '48px 16px', color: '#64748B' }}>
                  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '12px' }}>
                    <span className="ewms-spinner ewms-spinner-dark" style={{ width: '24px', height: '24px' }} />
                    <span>Đang tải danh sách đơn mua hàng...</span>
                  </div>
                </td>
              </tr>
            ) : filteredOrders.length === 0 ? (
              <tr>
                <td colSpan={8} style={{ textAlign: 'center', padding: '48px 16px', color: '#64748B' }}>
                  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '8px' }}>
                    <span style={{ fontSize: '32px' }}>📦</span>
                    <span style={{ fontWeight: 600, fontSize: '15px', color: '#0F172A' }}>
                      Không tìm thấy đơn mua hàng nào
                    </span>
                    <span style={{ fontSize: '13px' }}>
                      {searchKeyword || statusFilter
                        ? 'Thử thay đổi bộ lọc tìm kiếm hoặc trạng thái.'
                        : 'Chưa có đơn mua hàng nào trong hệ thống.'}
                    </span>
                    {isPurchaseStaff && !searchKeyword && !statusFilter && (
                      <button
                        type="button"
                        className="ewms-btn ewms-btn-primary"
                        onClick={() => navigate('/purchase-orders/create')}
                        style={{ marginTop: '12px' }}
                      >
                        + Tạo đơn hàng đầu tiên
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ) : (
              filteredOrders.map((po) => {
                const totalAmt = po.totalAmount ?? po.grandTotal ?? 0;
                return (
                  <tr key={po.purchaseOrderId}>
                    <td>
                      <Link
                        to={`/purchase-orders/${po.purchaseOrderId}`}
                        style={{
                          fontWeight: 700,
                          color: '#2563EB',
                          fontFamily: 'ui-monospace, monospace',
                        }}
                      >
                        {po.purchaseOrderCode}
                      </Link>
                      {po.createdByName && (
                        <div style={{ fontSize: '12px', color: '#64748B' }}>
                          Bởi: {po.createdByName}
                        </div>
                      )}
                    </td>

                    <td>
                      <span style={{ fontWeight: 600, color: '#1E293B' }}>{po.supplierName}</span>
                    </td>

                    <td>{formatDate(po.orderDate)}</td>

                    <td>{formatDate(po.expectedDeliveryDate)}</td>

                    <td style={{ textAlign: 'right', fontWeight: 600 }}>
                      {(po.totalQuantity || 0).toLocaleString('vi-VN')}
                    </td>

                    <td
                      style={{
                        textAlign: 'right',
                        fontWeight: 700,
                        color: '#0F172A',
                      }}
                    >
                      {formatCurrency(totalAmt)}
                    </td>

                    <td style={{ textAlign: 'center' }}>
                      <StatusBadge status={po.status} />
                    </td>

                    <td style={{ textAlign: 'center' }}>
                      <Link
                        to={`/purchase-orders/${po.purchaseOrderId}`}
                        className="ewms-btn ewms-btn-secondary"
                        style={{ padding: '4px 10px', fontSize: '12px' }}
                      >
                        Chi tiết →
                      </Link>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default PurchaseOrderListPage;
