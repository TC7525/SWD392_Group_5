import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { poApi } from '../api/poApi';
import { useAuth } from '../context/AuthContext';
import { StatusBadge } from '../components/StatusBadge';
import type { PurchaseOrderResponse, ApiError } from '../types/models';

export const PurchaseOrderDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { currentUser, hasRole } = useAuth();

  const [po, setPo] = useState<PurchaseOrderResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<{ code: string; message: string } | null>(null);
  const [actionSuccess, setActionSuccess] = useState<{ code: string; message: string } | null>(null);

  // Manager action states
  const [isApproving, setIsApproving] = useState<boolean>(false);
  const [isRejecting, setIsRejecting] = useState<boolean>(false);
  const [showApproveModal, setShowApproveModal] = useState<boolean>(false);
  const [showRejectModal, setShowRejectModal] = useState<boolean>(false);
  const [rejectionReason, setRejectionReason] = useState<string>('');
  const [rejectError, setRejectError] = useState<string | null>(null);

  const poId = Number(id);

  const fetchOrderDetail = useCallback(async () => {
    if (!poId || isNaN(poId)) {
      setError({ code: 'ERR_PARAM', message: 'Mã đơn mua hàng không hợp lệ.' });
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const res = await poApi.getById(poId);
      setPo(res.data);
    } catch (err: any) {
      const apiErr = err as ApiError;
      setError({
        code: apiErr?.code || 'ERR_LOAD',
        message: apiErr?.message || 'Không thể tải chi tiết đơn mua hàng.',
      });
    } finally {
      setIsLoading(false);
    }
  }, [poId]);

  useEffect(() => {
    let ignore = false;
    if (!poId || isNaN(poId)) {
      return;
    }

    poApi
      .getById(poId)
      .then((res) => {
        if (!ignore) {
          setPo(res.data);
          setError(null);
        }
      })
      .catch((err: any) => {
        if (!ignore) {
          const apiErr = err as ApiError;
          setError({
            code: apiErr?.code || 'ERR_LOAD',
            message: apiErr?.message || 'Không thể tải chi tiết đơn mua hàng.',
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
  }, [poId]);

  // Check if current status is Pending
  const isPending = useMemo(() => {
    if (!po) return false;
    const s = String(po.status).toLowerCase();
    return s === 'pending' || po.status === 0;
  }, [po]);

  // Check if current status is Approved or PartiallyReceived
  const isReceivable = useMemo(() => {
    if (!po) return false;
    const s = String(po.status).toLowerCase();
    return s === 'approved' || s === 'partiallyreceived' || po.status === 1 || po.status === 4;
  }, [po]);

  // Check if user is Warehouse Manager (or Admin)
  const isWarehouseManager = useMemo(() => {
    return (
      hasRole('Warehouse Manager') ||
      hasRole('Administrator') ||
      currentUser?.role?.toLowerCase() === 'warehouse manager' ||
      currentUser?.role?.toLowerCase() === 'warehousemanager' ||
      currentUser?.roles?.some((r) => r.toLowerCase().includes('manager'))
    );
  }, [currentUser, hasRole]);

  // Check if user is Warehouse Staff
  const isWarehouseStaff = useMemo(() => {
    return (
      hasRole('Warehouse Staff') ||
      currentUser?.role?.toLowerCase() === 'warehouse staff' ||
      currentUser?.role?.toLowerCase() === 'warehousestaff' ||
      isWarehouseManager
    );
  }, [currentUser, hasRole, isWarehouseManager]);

  // Formatters
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
        hour: '2-digit',
        minute: '2-digit',
      });
    } catch {
      return dateStr;
    }
  };

  // Handle Approve PO
  const handleApprove = async () => {
    if (!po) return;
    setIsApproving(true);
    setActionSuccess(null);
    setError(null);
    try {
      const res = await poApi.approve(po.purchaseOrderId);
      setShowApproveModal(false);
      setActionSuccess({
        code: res.code || 'MSG11',
        message: res.message || 'Duyệt đơn mua hàng thành công.',
      });
      await fetchOrderDetail();
    } catch (err: any) {
      const apiErr = err as ApiError;
      setError({
        code: apiErr?.code || 'MSG16',
        message: apiErr?.message || 'Không thể phê duyệt đơn hàng. Vui lòng thử lại.',
      });
      setShowApproveModal(false);
    } finally {
      setIsApproving(false);
    }
  };

  // Handle Reject PO
  const handleReject = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!po) return;

    if (!rejectionReason.trim()) {
      setRejectError('MSG01: Vui lòng nhập lý do từ chối.');
      return;
    }

    setIsRejecting(true);
    setRejectError(null);
    setActionSuccess(null);
    setError(null);
    try {
      const res = await poApi.reject(po.purchaseOrderId, rejectionReason.trim());
      setShowRejectModal(false);
      setRejectionReason('');
      setActionSuccess({
        code: res.code || 'MSG11',
        message: res.message || 'Đã từ chối đơn mua hàng thành công.',
      });
      await fetchOrderDetail();
    } catch (err: any) {
      const apiErr = err as ApiError;
      setRejectError(`${apiErr?.code || 'ERR'}: ${apiErr?.message || 'Từ chối đơn hàng thất bại.'}`);
    } finally {
      setIsRejecting(false);
    }
  };

  // Summary totals calculation
  const totals = useMemo(() => {
    if (!po?.items) {
      return {
        ordered: po?.totalOrderedQuantity ?? 0,
        received: po?.totalReceivedQuantity ?? 0,
        remaining: po?.totalRemainingQuantity ?? 0,
        grandTotal: po?.grandTotal ?? 0,
      };
    }

    let ordered = 0;
    let received = 0;
    let remaining = 0;
    let grandTotal = 0;

    for (const it of po.items) {
      ordered += it.orderedQuantity || 0;
      received += it.receivedQuantity || 0;
      remaining += it.remainingQuantity ?? (it.orderedQuantity - it.receivedQuantity);
      grandTotal += it.lineTotal || (it.orderedQuantity * it.unitPrice);
    }

    return { ordered, received, remaining, grandTotal };
  }, [po]);

  if (isLoading) {
    return (
      <div className="ewms-container" style={{ textAlign: 'center', padding: '64px 16px' }}>
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '16px' }}>
          <span className="ewms-spinner ewms-spinner-dark" style={{ width: '32px', height: '32px' }} />
          <span style={{ fontSize: '15px', color: '#64748B' }}>Đang tải thông tin chi tiết đơn hàng...</span>
        </div>
      </div>
    );
  }

  if (error && !po) {
    return (
      <div className="ewms-container">
        <div className="ewms-alert ewms-alert-error" role="alert" style={{ marginBottom: '20px' }}>
          <span style={{ fontSize: '18px' }}>⚠️</span>
          <div>
            <span className="ewms-alert-code" style={{ marginRight: '6px' }}>
              {error.code}
            </span>
            <span>{error.message}</span>
          </div>
        </div>
        <button
          type="button"
          className="ewms-btn ewms-btn-secondary"
          onClick={() => navigate('/purchase-orders')}
        >
          ← Quay lại danh sách
        </button>
      </div>
    );
  }

  if (!po) return null;

  const isRejected = String(po.status).toLowerCase() === 'rejected' || po.status === 2;

  return (
    <div className="ewms-container">
      {/* Back button */}
      <div style={{ marginBottom: '16px' }}>
        <Link
          to="/purchase-orders"
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '6px',
            fontSize: '14px',
            color: '#64748B',
            textDecoration: 'none',
            fontWeight: 500,
          }}
        >
          ← Quay lại danh sách đơn mua hàng
        </Link>
      </div>

      {/* Global Action Alerts */}
      {actionSuccess && (
        <div className="ewms-alert ewms-alert-success" role="alert">
          <span style={{ fontSize: '18px' }}>✓</span>
          <div>
            <span className="ewms-alert-code" style={{ marginRight: '6px' }}>
              {actionSuccess.code}
            </span>
            <span style={{ fontWeight: 600 }}>{actionSuccess.message}</span>
          </div>
        </div>
      )}

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

      {/* Top Header Card */}
      <div
        className="ewms-card"
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: '16px',
          padding: '20px 24px',
          marginBottom: '20px',
        }}
      >
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flexWrap: 'wrap' }}>
            <h1
              className="ewms-title"
              style={{
                fontFamily: 'ui-monospace, monospace',
                letterSpacing: '-0.02em',
                fontSize: '26px',
              }}
            >
              {po.purchaseOrderCode}
            </h1>
            <StatusBadge status={po.status} size="lg" showEnglish />
          </div>
          <p style={{ fontSize: '13px', color: '#64748B', marginTop: '6px' }}>
            Ngày lập đơn: {formatDate(po.orderDate || po.createdAt)}
          </p>
        </div>

        {/* Manager Action Bar (UC10, UC11) */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flexWrap: 'wrap' }}>
          {isWarehouseManager && isPending && (
            <>
              <button
                type="button"
                className="ewms-btn ewms-btn-danger"
                onClick={() => {
                  setRejectionReason('');
                  setRejectError(null);
                  setShowRejectModal(true);
                }}
                disabled={isApproving || isRejecting}
                style={{ padding: '9px 18px' }}
              >
                ✕ Từ chối đơn
              </button>

              <button
                type="button"
                className="ewms-btn ewms-btn-success"
                onClick={() => setShowApproveModal(true)}
                disabled={isApproving || isRejecting}
                style={{ padding: '9px 20px', boxShadow: '0 2px 4px rgba(16,185,129,0.2)' }}
              >
                ✓ Duyệt đơn hàng
              </button>
            </>
          )}

          {/* Quick link for warehouse staff if order is approved */}
          {isWarehouseStaff && isReceivable && (
            <Link
              to="/receiving"
              className="ewms-btn ewms-btn-primary"
              style={{ padding: '9px 18px', boxShadow: '0 2px 4px rgba(37,99,235,0.2)' }}
            >
              📦 Tiến hành nhận hàng →
            </Link>
          )}
        </div>
      </div>

      {/* Manager Review Notification Banner if Pending */}
      {isPending && isWarehouseManager && (
        <div
          className="ewms-alert ewms-alert-info"
          style={{
            borderLeft: '4px solid #3B82F6',
            backgroundColor: '#EFF6FF',
            padding: '16px 20px',
            marginBottom: '20px',
          }}
        >
          <span style={{ fontSize: '24px' }}>ℹ️</span>
          <div>
            <div style={{ fontWeight: 700, fontSize: '15px', color: '#1E40AF' }}>
              Đơn hàng đang chờ phê duyệt (Pending Review)
            </div>
            <div style={{ fontSize: '13px', color: '#1E3A8A', marginTop: '2px' }}>
              Bạn đang đăng nhập với tư cách <strong>Quản lý kho (Warehouse Manager)</strong>. Hãy kiểm tra danh sách sản phẩm và bấm <strong>Duyệt đơn hàng</strong> hoặc <strong>Từ chối</strong> kèm lý do.
            </div>
          </div>
        </div>
      )}

      {/* Rejection Alert Banner if status is Rejected */}
      {(isRejected || po.rejectionReason) && (
        <div
          className="ewms-alert ewms-alert-error"
          style={{
            borderLeft: '4px solid #EF4444',
            backgroundColor: '#FEF2F2',
            padding: '16px 20px',
            marginBottom: '20px',
          }}
        >
          <span style={{ fontSize: '24px' }}>🚫</span>
          <div>
            <div style={{ fontWeight: 700, fontSize: '15px', color: '#991B1B' }}>
              Đơn hàng đã bị từ chối phê duyệt (Rejected)
            </div>
            <div style={{ fontSize: '13px', color: '#7F1D1D', marginTop: '4px' }}>
              <strong>Lý do từ chối:</strong> {po.rejectionReason || 'Không có lý do cụ thể.'}
            </div>
            {po.approvedByName && (
              <div style={{ fontSize: '12px', color: '#991B1B', marginTop: '2px' }}>
                Người xử lý: {po.approvedByName} ({formatDate(po.approvedDate)})
              </div>
            )}
          </div>
        </div>
      )}

      {/* Metadata Overview Card */}
      <div className="ewms-card" style={{ marginBottom: '20px' }}>
        <h2
          style={{
            fontSize: '16px',
            fontWeight: 700,
            color: '#0F172A',
            marginBottom: '16px',
            borderBottom: '1px solid #E2E8F0',
            paddingBottom: '10px',
          }}
        >
          Thông tin đơn mua hàng
        </h2>

        <div className="ewms-grid-3">
          {/* Supplier Info */}
          <div style={{ padding: '12px', backgroundColor: '#F8FAFC', borderRadius: '8px' }}>
            <div style={{ fontSize: '12px', color: '#64748B', fontWeight: 600 }}>NHÀ CUNG CẤP</div>
            <div style={{ fontSize: '16px', fontWeight: 700, color: '#0F172A', marginTop: '4px' }}>
              {po.supplierName || '—'}
            </div>
            {po.supplierCode && (
              <div style={{ fontSize: '12px', color: '#2563EB', fontWeight: 600, marginTop: '2px' }}>
                Mã NCC: {po.supplierCode}
              </div>
            )}
          </div>

          {/* Creator & Approver Info */}
          <div style={{ padding: '12px', backgroundColor: '#F8FAFC', borderRadius: '8px' }}>
            <div style={{ fontSize: '12px', color: '#64748B', fontWeight: 600 }}>NGƯỜI LẬP & PHÊ DUYỆT</div>
            <div style={{ fontSize: '14px', color: '#0F172A', marginTop: '4px' }}>
              <strong>Người lập:</strong> {po.createdByName || `ID: ${po.createdBy}`}
            </div>
            <div style={{ fontSize: '14px', color: '#0F172A', marginTop: '4px' }}>
              <strong>Người duyệt:</strong>{' '}
              {po.approvedByName ? (
                <span style={{ color: '#059669', fontWeight: 600 }}>{po.approvedByName}</span>
              ) : (
                <span style={{ color: '#94A3B8' }}>Chưa duyệt</span>
              )}
            </div>
          </div>

          {/* Schedule & Timeline Info */}
          <div style={{ padding: '12px', backgroundColor: '#F8FAFC', borderRadius: '8px' }}>
            <div style={{ fontSize: '12px', color: '#64748B', fontWeight: 600 }}>THỜI GIAN & TIẾN ĐỘ</div>
            <div style={{ fontSize: '13px', color: '#0F172A', marginTop: '4px' }}>
              <strong>Ngày giao dự kiến:</strong> {formatDate(po.expectedDeliveryDate)}
            </div>
            {po.approvedDate && (
              <div style={{ fontSize: '13px', color: '#0F172A', marginTop: '2px' }}>
                <strong>Ngày duyệt:</strong> {formatDate(po.approvedDate)}
              </div>
            )}
            {po.completedDate && (
              <div style={{ fontSize: '13px', color: '#059669', marginTop: '2px' }}>
                <strong>Ngày hoàn thành:</strong> {formatDate(po.completedDate)}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Items Table Card */}
      <div className="ewms-card">
        <h2
          style={{
            fontSize: '16px',
            fontWeight: 700,
            color: '#0F172A',
            marginBottom: '16px',
            borderBottom: '1px solid #E2E8F0',
            paddingBottom: '10px',
          }}
        >
          Danh sách mặt hàng chi tiết
        </h2>

        <div className="ewms-table-container">
          <table className="ewms-table">
            <thead>
              <tr>
                <th style={{ width: '40px', textAlign: 'center' }}>STT</th>
                <th>Mã SP</th>
                <th>Tên sản phẩm</th>
                <th style={{ textAlign: 'right' }}>Đơn giá</th>
                <th style={{ textAlign: 'right' }}>SL Đặt</th>
                <th style={{ textAlign: 'right' }}>SL Đã nhận</th>
                <th style={{ textAlign: 'right' }}>SL Còn lại</th>
                <th style={{ textAlign: 'right' }}>Thành tiền</th>
              </tr>
            </thead>
            <tbody>
              {po.items && po.items.length > 0 ? (
                po.items.map((it, idx) => {
                  const remQty =
                    it.remainingQuantity !== undefined
                      ? it.remainingQuantity
                      : it.orderedQuantity - it.receivedQuantity;
                  const lineTotal = it.lineTotal ?? it.orderedQuantity * it.unitPrice;

                  return (
                    <tr key={it.purchaseOrderItemId || idx}>
                      <td style={{ textAlign: 'center', color: '#64748B', fontWeight: 600 }}>
                        {idx + 1}
                      </td>

                      <td style={{ fontFamily: 'ui-monospace, monospace', fontWeight: 600 }}>
                        {it.productCode || `SP-${it.productId}`}
                      </td>

                      <td>
                        <span style={{ fontWeight: 600, color: '#0F172A' }}>
                          {it.productName || `Sản phẩm #${it.productId}`}
                        </span>
                      </td>

                      <td style={{ textAlign: 'right' }}>{formatCurrency(it.unitPrice)}</td>

                      <td style={{ textAlign: 'right', fontWeight: 600 }}>
                        {it.orderedQuantity.toLocaleString('vi-VN')}
                      </td>

                      <td
                        style={{
                          textAlign: 'right',
                          fontWeight: 600,
                          color: it.receivedQuantity > 0 ? '#059669' : '#64748B',
                        }}
                      >
                        {it.receivedQuantity.toLocaleString('vi-VN')}
                      </td>

                      <td
                        style={{
                          textAlign: 'right',
                          fontWeight: 700,
                          color: remQty > 0 ? '#D97706' : '#10B981',
                        }}
                      >
                        {remQty.toLocaleString('vi-VN')}
                      </td>

                      <td
                        style={{
                          textAlign: 'right',
                          fontWeight: 700,
                          color: '#0F172A',
                          whiteSpace: 'nowrap',
                        }}
                      >
                        {formatCurrency(lineTotal)}
                      </td>
                    </tr>
                  );
                })
              ) : (
                <tr>
                  <td colSpan={8} style={{ textAlign: 'center', padding: '24px', color: '#64748B' }}>
                    Không có sản phẩm nào trong đơn hàng này.
                  </td>
                </tr>
              )}
            </tbody>
            <tfoot>
              <tr style={{ backgroundColor: '#F8FAFC', fontWeight: 700, borderTop: '2px solid #CBD5E1' }}>
                <td colSpan={4} style={{ textAlign: 'right', padding: '14px 16px', color: '#0F172A' }}>
                  TỔNG CỘNG:
                </td>
                <td style={{ textAlign: 'right', padding: '14px 16px', color: '#2563EB' }}>
                  {totals.ordered.toLocaleString('vi-VN')}
                </td>
                <td style={{ textAlign: 'right', padding: '14px 16px', color: '#059669' }}>
                  {totals.received.toLocaleString('vi-VN')}
                </td>
                <td style={{ textAlign: 'right', padding: '14px 16px', color: '#D97706' }}>
                  {totals.remaining.toLocaleString('vi-VN')}
                </td>
                <td
                  style={{
                    textAlign: 'right',
                    padding: '14px 16px',
                    fontSize: '16px',
                    color: '#0F172A',
                    whiteSpace: 'nowrap',
                  }}
                >
                  {formatCurrency(totals.grandTotal)}
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>

      {/* Confirmation Modal: Approve PO */}
      {showApproveModal && (
        <div className="ewms-modal-backdrop" onClick={() => !isApproving && setShowApproveModal(false)}>
          <div className="ewms-modal" onClick={(e) => e.stopPropagation()}>
            <div className="ewms-modal-header">
              <h3 className="ewms-modal-title">Xác nhận duyệt đơn hàng</h3>
              <button
                type="button"
                onClick={() => setShowApproveModal(false)}
                disabled={isApproving}
                style={{ background: 'transparent', border: 'none', fontSize: '18px', cursor: 'pointer' }}
              >
                ×
              </button>
            </div>
            <div className="ewms-modal-body">
              <p style={{ fontSize: '14px', color: '#334155', lineHeight: 1.5 }}>
                Bạn có chắc chắn muốn phê duyệt đơn mua hàng{' '}
                <strong style={{ color: '#2563EB' }}>{po.purchaseOrderCode}</strong> của nhà cung cấp{' '}
                <strong>{po.supplierName}</strong>?
              </p>
              <div
                style={{
                  marginTop: '16px',
                  padding: '12px',
                  borderRadius: '6px',
                  backgroundColor: '#F8FAFC',
                  fontSize: '13px',
                  color: '#64748B',
                }}
              >
                Sau khi được duyệt, đơn hàng sẽ chuyển sang trạng thái <strong>Đã duyệt (Approved)</strong> và nhân viên kho có thể tiến hành kiểm tra & nhận hàng vào kho.
              </div>
            </div>
            <div className="ewms-modal-footer">
              <button
                type="button"
                className="ewms-btn ewms-btn-secondary"
                onClick={() => setShowApproveModal(false)}
                disabled={isApproving}
              >
                Huỷ bỏ
              </button>
              <button
                type="button"
                className="ewms-btn ewms-btn-success"
                onClick={handleApprove}
                disabled={isApproving}
              >
                {isApproving ? (
                  <>
                    <span className="ewms-spinner" />
                    <span>Đang duyệt...</span>
                  </>
                ) : (
                  'Xác nhận duyệt'
                )}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Rejection Modal: Reject PO with Reason */}
      {showRejectModal && (
        <div className="ewms-modal-backdrop" onClick={() => !isRejecting && setShowRejectModal(false)}>
          <div className="ewms-modal" onClick={(e) => e.stopPropagation()}>
            <form onSubmit={handleReject}>
              <div className="ewms-modal-header">
                <h3 className="ewms-modal-title" style={{ color: '#EF4444' }}>
                  Từ chối đơn mua hàng
                </h3>
                <button
                  type="button"
                  onClick={() => setShowRejectModal(false)}
                  disabled={isRejecting}
                  style={{ background: 'transparent', border: 'none', fontSize: '18px', cursor: 'pointer' }}
                >
                  ×
                </button>
              </div>

              <div className="ewms-modal-body">
                {rejectError && (
                  <div className="ewms-alert ewms-alert-error" style={{ marginBottom: '14px' }}>
                    <span>⚠️</span>
                    <span>{rejectError}</span>
                  </div>
                )}

                <p style={{ fontSize: '14px', color: '#334155', marginBottom: '14px' }}>
                  Vui lòng cung cấp lý do từ chối đơn hàng{' '}
                  <strong style={{ color: '#2563EB' }}>{po.purchaseOrderCode}</strong> để thông báo cho nhân viên mua hàng:
                </p>

                <div className="ewms-form-group">
                  <label htmlFor="rejection-reason-text" className="ewms-label">
                    Lý do từ chối <span style={{ color: '#EF4444' }}>*</span>
                  </label>
                  <textarea
                    id="rejection-reason-text"
                    className="ewms-textarea"
                    rows={4}
                    placeholder="vd: Giá nhập vượt định mức quy định, hoặc nhà cung cấp chưa đáp ứng tiêu chuẩn chất lượng..."
                    value={rejectionReason}
                    onChange={(e) => setRejectionReason(e.target.value)}
                    disabled={isRejecting}
                    required
                  />
                </div>
              </div>

              <div className="ewms-modal-footer">
                <button
                  type="button"
                  className="ewms-btn ewms-btn-secondary"
                  onClick={() => setShowRejectModal(false)}
                  disabled={isRejecting}
                >
                  Huỷ
                </button>
                <button
                  type="submit"
                  className="ewms-btn ewms-btn-danger"
                  disabled={isRejecting}
                >
                  {isRejecting ? (
                    <>
                      <span className="ewms-spinner" />
                      <span>Đang từ chối...</span>
                    </>
                  ) : (
                    'Xác nhận từ chối'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default PurchaseOrderDetailPage;
