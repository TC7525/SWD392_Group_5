import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { poApi } from '../api/poApi';
import { receivingApi } from '../api/receivingApi';
import { StatusBadge } from '../components/StatusBadge';
import type {
  PurchaseOrderResponse,
  CheckQuantityItemResult,
  ApiError,
} from '../types/models';

const WAREHOUSE_OPTIONS = [
  { warehouseId: 1, warehouseCode: 'WH-HN-01', warehouseName: 'Kho Tổng Hà Nội' },
  { warehouseId: 2, warehouseCode: 'WH-HCM-01', warehouseName: 'Kho Tổng TP. Hồ Chí Minh' },
];

export const ReceivingDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const poId = Number(id);

  const [po, setPo] = useState<PurchaseOrderResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<{ code: string; message: string } | null>(null);

  // Form states
  const [selectedWarehouseId, setSelectedWarehouseId] = useState<number>(1);
  const [receivingNote, setReceivingNote] = useState<string>('');
  const [receivingQuantities, setReceivingQuantities] = useState<Record<number, number>>({});

  // Action status states
  const [isChecking, setIsChecking] = useState<boolean>(false);
  const [isStarting, setIsStarting] = useState<boolean>(false);
  const [isConfirming, setIsConfirming] = useState<boolean>(false);
  const [isCancelling, setIsCancelling] = useState<boolean>(false);

  // Validation feedback
  const [itemValidationMap, setItemValidationMap] = useState<Record<number, CheckQuantityItemResult>>({});
  const [alertBanner, setAlertBanner] = useState<{
    type: 'success' | 'error' | 'warning' | 'info';
    code?: string;
    message: string;
  } | null>(null);

  // Modals
  const [showConfirmModal, setShowConfirmModal] = useState<boolean>(false);
  const [showCancelModal, setShowCancelModal] = useState<boolean>(false);

  // Fetch PO detail helper for manual reloads
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
      const data = res.data;
      setPo(data);

      // Initialize receiving quantities with 0 or remaining quantity
      const initialQuantities: Record<number, number> = {};
      if (data && data.items) {
        for (const item of data.items) {
          initialQuantities[item.productId] = 0;
        }
      }
      setReceivingQuantities(initialQuantities);
    } catch (err: any) {
      const apiErr = err as ApiError;
      setError({
        code: apiErr?.code || 'ERR_LOAD_DETAIL',
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
          const data = res.data;
          setPo(data);
          setError(null);

          const initialQuantities: Record<number, number> = {};
          if (data && data.items) {
            for (const item of data.items) {
              initialQuantities[item.productId] = 0;
            }
          }
          setReceivingQuantities(initialQuantities);
        }
      })
      .catch((err: any) => {
        if (!ignore) {
          const apiErr = err as ApiError;
          setError({
            code: apiErr?.code || 'ERR_LOAD_DETAIL',
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

  // Status flags
  const isApproved = useMemo(() => {
    if (!po) return false;
    const s = String(po.status).toLowerCase();
    return s === 'approved' || po.status === 1;
  }, [po]);

  const isPartiallyReceived = useMemo(() => {
    if (!po) return false;
    const s = String(po.status).toLowerCase();
    return s === 'partiallyreceived' || po.status === 4;
  }, [po]);

  const isProcessing = useMemo(() => {
    if (!po) return false;
    const s = String(po.status).toLowerCase();
    return s === 'processing' || po.status === 3;
  }, [po]);

  const isCompleted = useMemo(() => {
    if (!po) return false;
    const s = String(po.status).toLowerCase();
    return s === 'completed' || po.status === 5;
  }, [po]);

  // Calculate items with quantities to receive
  const activeReceivingItems = useMemo(() => {
    if (!po || !po.items) return [];
    return po.items
      .filter((item) => (receivingQuantities[item.productId] || 0) > 0)
      .map((item) => ({
        ...item,
        qtyToReceive: receivingQuantities[item.productId] || 0,
      }));
  }, [po, receivingQuantities]);

  const totalReceivingUnits = useMemo(() => {
    return activeReceivingItems.reduce((sum, item) => sum + item.qtyToReceive, 0);
  }, [activeReceivingItems]);

  // Formatters
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

  const formatCurrency = (val?: number) => {
    if (val === undefined || val === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN').format(val) + ' ₫';
  };

  // Handle quantity input change
  const handleQuantityChange = (productId: number, valueStr: string) => {
    const parsed = valueStr === '' ? 0 : parseInt(valueStr, 10);
    const safeVal = isNaN(parsed) ? 0 : Math.max(0, parsed);

    setReceivingQuantities((prev) => ({
      ...prev,
      [productId]: safeVal,
    }));

    // Clear item-specific error if now below remaining
    if (po) {
      const item = po.items?.find((i) => i.productId === productId);
      if (item && safeVal <= item.remainingQuantity) {
        setItemValidationMap((prev) => {
          const next = { ...prev };
          delete next[productId];
          return next;
        });
      }
    }
  };

  // Helper: Fill all remaining
  const handleFillAllRemaining = () => {
    if (!po || !po.items) return;
    const updated: Record<number, number> = {};
    for (const item of po.items) {
      updated[item.productId] = Math.max(0, item.remainingQuantity);
    }
    setReceivingQuantities(updated);
    setItemValidationMap({});
    setAlertBanner(null);
  };

  // Helper: Reset all to 0
  const handleResetQuantities = () => {
    if (!po || !po.items) return;
    const updated: Record<number, number> = {};
    for (const item of po.items) {
      updated[item.productId] = 0;
    }
    setReceivingQuantities(updated);
    setItemValidationMap({});
    setAlertBanner(null);
  };

  // Action 1: "Kiểm tra số lượng" (UC12)
  const handleCheckQuantity = async () => {
    if (!po || !po.items) return;

    // Check if user entered at least one quantity
    const itemsToCheck = po.items
      .filter((item) => (receivingQuantities[item.productId] || 0) > 0)
      .map((item) => ({
        productId: item.productId,
        quantity: receivingQuantities[item.productId] || 0,
      }));

    if (itemsToCheck.length === 0) {
      setAlertBanner({
        type: 'warning',
        code: 'MSG01',
        message: 'Vui lòng nhập số lượng nhận (> 0) cho ít nhất một sản phẩm trước khi kiểm tra.',
      });
      return;
    }

    setIsChecking(true);
    setAlertBanner(null);
    try {
      const res = await receivingApi.checkQuantity(po.purchaseOrderId, itemsToCheck);
      const data = res.data;

      const map: Record<number, CheckQuantityItemResult> = {};
      let hasError = false;

      if (data && data.items) {
        for (const itemRes of data.items) {
          map[itemRes.productId] = itemRes;
          if (!itemRes.isValid) {
            hasError = true;
          }
        }
      }
      setItemValidationMap(map);

      if (!data.valid || hasError) {
        const errorMsg =
          data.message ||
          'MSG09: Received quantity cannot exceed the remaining ordered quantity.';
        setAlertBanner({
          type: 'error',
          code: 'MSG09',
          message: errorMsg,
        });
      } else {
        setAlertBanner({
          type: 'success',
          code: 'VALID',
          message: '✓ Kiểm tra số lượng hoàn tất: Tất cả số lượng nhận đều hợp lệ và sẵn sàng nhập kho.',
        });
      }
    } catch (err: any) {
      const apiErr = err as ApiError;
      setAlertBanner({
        type: 'error',
        code: apiErr?.code || 'MSG09',
        message: apiErr?.message || 'MSG09: Received quantity cannot exceed the remaining ordered quantity.',
      });
    } finally {
      setIsChecking(false);
    }
  };

  // Action 2: "Bắt đầu nhận hàng" (UC13)
  const handleStartReceiving = async () => {
    if (!po) return;
    setIsStarting(true);
    setAlertBanner(null);
    try {
      const res = await receivingApi.startReceiving(po.purchaseOrderId);
      setPo(res.data);
      setAlertBanner({
        type: 'info',
        code: 'UC13',
        message: 'Trạng thái đơn hàng đã chuyển sang Đang nhận hàng (Processing). Bây giờ bạn có thể xác nhận nhập kho.',
      });
    } catch (err: any) {
      const apiErr = err as ApiError;
      setAlertBanner({
        type: 'error',
        code: apiErr?.code || 'ERR_START',
        message: apiErr?.message || 'Không thể bắt đầu nhận hàng. Vui lòng thử lại.',
      });
    } finally {
      setIsStarting(false);
    }
  };

  // Action 4: "Hủy thao tác" (UC13)
  const handleCancelReceiving = async () => {
    if (!po) return;
    setIsCancelling(true);
    setAlertBanner(null);
    try {
      const res = await receivingApi.cancelReceiving(po.purchaseOrderId);
      setPo(res.data);
      setShowCancelModal(false);
      setAlertBanner({
        type: 'info',
        code: 'CANCELLED',
        message: 'Đã hủy thao tác nhận hàng. Trạng thái đơn hàng đã được khôi phục về trước đó.',
      });
    } catch (err: any) {
      const apiErr = err as ApiError;
      setAlertBanner({
        type: 'error',
        code: apiErr?.code || 'ERR_CANCEL',
        message: apiErr?.message || 'Không thể hủy thao tác nhận hàng.',
      });
    } finally {
      setIsCancelling(false);
    }
  };

  // Pre-confirm check before opening modal
  const handleOpenConfirmModal = () => {
    if (!po || !po.items) return;

    if (activeReceivingItems.length === 0) {
      setAlertBanner({
        type: 'warning',
        code: 'MSG01',
        message: 'Vui lòng nhập số lượng nhận (> 0) cho ít nhất một sản phẩm cần nhập kho.',
      });
      return;
    }

    // Check if any item exceeds remaining
    let invalidItem = false;
    for (const item of po.items) {
      const qty = receivingQuantities[item.productId] || 0;
      if (qty > item.remainingQuantity) {
        invalidItem = true;
        break;
      }
    }

    if (invalidItem) {
      setAlertBanner({
        type: 'error',
        code: 'MSG09',
        message: 'MSG09: Received quantity cannot exceed the remaining ordered quantity. Vui lòng kiểm tra lại số lượng nhận.',
      });
      return;
    }

    setShowConfirmModal(true);
  };

  // Action 3: "Xác nhận nhập kho" (UC13 + UC14)
  const handleConfirmReceive = async () => {
    if (!po || !po.items) return;

    setIsConfirming(true);
    setAlertBanner(null);

    const itemsPayload = activeReceivingItems.map((item) => ({
      purchaseOrderItemId: item.purchaseOrderItemId,
      productId: item.productId,
      receivedQuantity: item.qtyToReceive,
    }));

    try {
      const res = await receivingApi.confirmReceive(po.purchaseOrderId, {
        warehouseId: selectedWarehouseId,
        note: receivingNote || 'Nhập kho hàng hóa từ đơn PO ' + po.purchaseOrderCode,
        items: itemsPayload,
      });

      setShowConfirmModal(false);

      // On success, backend returns MSG12
      const msgCode = res.code || 'MSG12';
      const msgText =
        res.message || 'Sản phẩm đã được nhận và tồn kho đã được cập nhật thành công.';

      setAlertBanner({
        type: 'success',
        code: msgCode,
        message: `[${msgCode}] ${msgText} Hàng đã được nhập vào ${
          WAREHOUSE_OPTIONS.find((w) => w.warehouseId === selectedWarehouseId)?.warehouseName
        }.`,
      });

      // Reload fresh PO detail
      if (res.data) {
        setPo(res.data);
      } else {
        await fetchOrderDetail();
      }

      // Reset quantities for items that are now fulfilled
      const resetQtys: Record<number, number> = {};
      for (const item of po.items) {
        resetQtys[item.productId] = 0;
      }
      setReceivingQuantities(resetQtys);
      setItemValidationMap({});
    } catch (err: any) {
      const apiErr = err as ApiError;
      setShowConfirmModal(false);
      setAlertBanner({
        type: 'error',
        code: apiErr?.code || 'MSG17',
        message:
          apiErr?.message ||
          'Không thể hoàn tất nhập kho. Không có thay đổi tồn kho nào được lưu.',
      });
    } finally {
      setIsConfirming(false);
    }
  };

  const selectedWarehouse = WAREHOUSE_OPTIONS.find((w) => w.warehouseId === selectedWarehouseId);

  if (isLoading) {
    return (
      <div className="ewms-container" style={{ textAlign: 'center', padding: '80px 16px' }}>
        <div className="ewms-spinner ewms-spinner-dark" style={{ width: '36px', height: '36px', marginBottom: '16px' }} />
        <div style={{ fontSize: '15px', color: '#64748B' }}>Đang tải thông tin đơn nhận hàng...</div>
      </div>
    );
  }

  if (error || !po) {
    return (
      <div className="ewms-container" style={{ padding: '40px 16px' }}>
        <div className="ewms-card" style={{ maxWidth: '600px', margin: '0 auto', textAlign: 'center', padding: '40px 24px' }}>
          <div style={{ fontSize: '48px', marginBottom: '16px' }}>⚠️</div>
          <h2 style={{ fontSize: '20px', fontWeight: 700, color: '#0F172A', marginBottom: '8px' }}>
            Không tìm thấy thông tin đơn hàng
          </h2>
          <p style={{ fontSize: '14px', color: '#64748B', marginBottom: '24px' }}>
            {error?.message || 'Đơn hàng không tồn tại hoặc bạn không có quyền truy cập.'}
          </p>
          <Link to="/receiving" className="ewms-btn ewms-btn-primary">
            ← Quay lại danh sách nhận hàng
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="ewms-container">
      {/* Breadcrumb & Navigation */}
      <div style={{ marginBottom: '16px', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Link
          to="/receiving"
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '6px',
            fontSize: '14px',
            color: '#475569',
            fontWeight: 500,
          }}
        >
          <span>←</span>
          <span>Quay lại danh sách nhận hàng</span>
        </Link>

        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <Link
            to={`/purchase-orders/${po.purchaseOrderId}`}
            style={{ fontSize: '13px', color: '#2563EB', fontWeight: 600 }}
          >
            Xem chi tiết PO gốc ↗
          </Link>
          <span style={{ color: '#CBD5E1' }}>|</span>
          <Link to="/inventory" style={{ fontSize: '13px', color: '#059669', fontWeight: 600 }}>
            Kiểm tra Tồn kho & Giao dịch ↗
          </Link>
        </div>
      </div>

      {/* Page Title & Status Header */}
      <div
        style={{
          display: 'flex',
          alignItems: 'flex-start',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: '16px',
          marginBottom: '24px',
        }}
      >
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flexWrap: 'wrap' }}>
            <h1 className="ewms-title" style={{ fontFamily: 'monospace', letterSpacing: '-0.02em' }}>
              {po.purchaseOrderCode}
            </h1>
            <StatusBadge status={po.status} size="lg" />
          </div>
          <p className="ewms-subtitle" style={{ marginTop: '6px' }}>
            Quy trình kiểm đếm số lượng thực tế, đối chiếu đơn hàng và xác nhận nhập kho vật lý (UC12, UC13, UC14).
          </p>
        </div>

        {/* Quick status notice */}
        {isCompleted && (
          <div
            style={{
              padding: '8px 16px',
              backgroundColor: '#ECFDF5',
              border: '1px solid #A7F3D0',
              borderRadius: '8px',
              color: '#065F46',
              fontSize: '13px',
              fontWeight: 600,
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
            }}
          >
            <span>🎉</span>
            <span>Đơn hàng đã hoàn thành nhận đủ 100% hàng hóa!</span>
          </div>
        )}
      </div>

      {/* Global Alert Banner */}
      {alertBanner && (
        <div
          className={`ewms-alert ewms-alert-${alertBanner.type}`}
          role="alert"
          style={{ marginBottom: '24px' }}
        >
          <span style={{ fontSize: '18px' }}>
            {alertBanner.type === 'success' && '✅'}
            {alertBanner.type === 'error' && '❌'}
            {alertBanner.type === 'warning' && '⚠️'}
            {alertBanner.type === 'info' && 'ℹ️'}
          </span>
          <div style={{ flex: 1 }}>
            {alertBanner.code && (
              <span className="ewms-alert-code" style={{ marginRight: '8px' }}>
                {alertBanner.code}
              </span>
            )}
            <span>{alertBanner.message}</span>
          </div>
          <button
            type="button"
            onClick={() => setAlertBanner(null)}
            style={{
              background: 'transparent',
              border: 'none',
              cursor: 'pointer',
              fontSize: '16px',
              color: 'inherit',
              opacity: 0.7,
            }}
          >
            ✕
          </button>
        </div>
      )}

      {/* PO Information Summary Card */}
      <div className="ewms-card" style={{ marginBottom: '24px' }}>
        <h2 style={{ fontSize: '16px', fontWeight: 700, color: '#1E293B', marginBottom: '16px', borderBottom: '1px solid #F1F5F9', paddingBottom: '10px' }}>
          Thông tin đơn mua hàng
        </h2>

        <div className="ewms-grid-4">
          <div>
            <div style={{ fontSize: '12px', color: '#64748B', marginBottom: '2px' }}>Nhà cung cấp</div>
            <div style={{ fontWeight: 600, color: '#0F172A' }}>{po.supplierName || '—'}</div>
            {po.supplierCode && (
              <div style={{ fontSize: '11px', color: '#94A3B8', fontFamily: 'monospace' }}>
                {po.supplierCode}
              </div>
            )}
          </div>

          <div>
            <div style={{ fontSize: '12px', color: '#64748B', marginBottom: '2px' }}>Ngày đặt hàng</div>
            <div style={{ fontWeight: 600, color: '#0F172A' }}>{formatDate(po.orderDate)}</div>
          </div>

          <div>
            <div style={{ fontSize: '12px', color: '#64748B', marginBottom: '2px' }}>Ngày giao dự kiến</div>
            <div style={{ fontWeight: 600, color: '#0F172A' }}>
              {formatDate(po.expectedDeliveryDate)}
            </div>
          </div>

          <div>
            <div style={{ fontSize: '12px', color: '#64748B', marginBottom: '2px' }}>Người tạo đơn / Duyệt</div>
            <div style={{ fontWeight: 600, color: '#0F172A' }}>{po.createdByName || '—'}</div>
            {po.approvedByName && (
              <div style={{ fontSize: '12px', color: '#059669' }}>
                Duyệt bởi: {po.approvedByName}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Warehouse Selection & Receiving Notes */}
      <div className="ewms-card" style={{ marginBottom: '24px' }}>
        <h2 style={{ fontSize: '16px', fontWeight: 700, color: '#1E293B', marginBottom: '16px', borderBottom: '1px solid #F1F5F9', paddingBottom: '10px' }}>
          Cấu hình kho đích & Ghi chú đợt nhận
        </h2>

        <div className="ewms-grid-2">
          {/* Warehouse Dropdown */}
          <div className="ewms-form-group" style={{ marginBottom: 0 }}>
            <label className="ewms-label" htmlFor="warehouse-select">
              Kho tiếp nhận nhập hàng <span style={{ color: '#EF4444' }}>*</span>
            </label>
            <select
              id="warehouse-select"
              className="ewms-select"
              value={selectedWarehouseId}
              onChange={(e) => setSelectedWarehouseId(Number(e.target.value))}
              disabled={isCompleted}
              style={{ fontWeight: 600 }}
            >
              {WAREHOUSE_OPTIONS.map((w) => (
                <option key={w.warehouseId} value={w.warehouseId}>
                  {w.warehouseName} ({w.warehouseCode})
                </option>
              ))}
            </select>
            <p style={{ fontSize: '12px', color: '#64748B', marginTop: '6px' }}>
              Hàng hóa sau khi xác nhận nhập kho sẽ được cộng trực tiếp vào số lượng tồn kho của kho này.
            </p>
          </div>

          {/* Receiving Note Input */}
          <div className="ewms-form-group" style={{ marginBottom: 0 }}>
            <label className="ewms-label" htmlFor="receiving-note">
              Ghi chú nhận hàng đợt này
            </label>
            <input
              id="receiving-note"
              type="text"
              className="ewms-input"
              placeholder="VD: Kiểm đếm đợt 1 nguyên đai nguyên kiện, đã ký biên bản giao nhận..."
              value={receivingNote}
              onChange={(e) => setReceivingNote(e.target.value)}
              disabled={isCompleted}
            />
            <p style={{ fontSize: '12px', color: '#64748B', marginTop: '6px' }}>
              Ghi chú sẽ được lưu vào biên bản giao dịch nhập kho (Inventory Transaction) để phục vụ kiểm toán.
            </p>
          </div>
        </div>
      </div>

      {/* Items Table for Receiving & Inspection */}
      <div className="ewms-card" style={{ marginBottom: '24px', paddingBottom: '16px' }}>
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            flexWrap: 'wrap',
            gap: '12px',
            marginBottom: '16px',
            borderBottom: '1px solid #F1F5F9',
            paddingBottom: '12px',
          }}
        >
          <div>
            <h2 style={{ fontSize: '16px', fontWeight: 700, color: '#1E293B' }}>
              Danh sách sản phẩm kiểm đếm
            </h2>
            <p style={{ fontSize: '13px', color: '#64748B' }}>
              Nhập số lượng thực nhận cho từng mặt hàng. Số lượng không được vượt quá số lượng còn lại (`MSG09`).
            </p>
          </div>

          {!isCompleted && (
            <div style={{ display: 'flex', gap: '8px' }}>
              <button
                type="button"
                className="ewms-btn ewms-btn-secondary"
                onClick={handleFillAllRemaining}
                style={{ padding: '6px 12px', fontSize: '12px' }}
                title="Tự động điền đầy đủ số lượng còn lại cho toàn bộ sản phẩm"
              >
                ⚡ Nhận tất cả còn lại
              </button>
              <button
                type="button"
                className="ewms-btn ewms-btn-secondary"
                onClick={handleResetQuantities}
                style={{ padding: '6px 12px', fontSize: '12px' }}
                title="Xóa trắng số lượng nhận"
              >
                ↺ Đặt lại 0
              </button>
            </div>
          )}
        </div>

        <div className="ewms-table-container">
          <table className="ewms-table">
            <thead>
              <tr>
                <th style={{ width: '50px', textAlign: 'center' }}>STT</th>
                <th style={{ width: '120px' }}>Mã SP</th>
                <th>Tên sản phẩm</th>
                <th style={{ width: '110px', textAlign: 'right' }}>Đơn giá</th>
                <th style={{ width: '90px', textAlign: 'right' }}>Đã đặt</th>
                <th style={{ width: '90px', textAlign: 'right' }}>Đã nhận</th>
                <th style={{ width: '90px', textAlign: 'right' }}>Còn lại</th>
                <th style={{ width: '170px', textAlign: 'center', backgroundColor: '#EFF6FF' }}>
                  Số lượng nhận đợt này *
                </th>
                <th style={{ width: '140px', textAlign: 'center' }}>Kết quả kiểm đếm</th>
              </tr>
            </thead>
            <tbody>
              {po.items?.map((item, idx) => {
                const receivingQty = receivingQuantities[item.productId] ?? 0;
                const remaining = item.remainingQuantity;
                const isExceeded = receivingQty > remaining;
                const itemValidation = itemValidationMap[item.productId];

                return (
                  <tr
                    key={item.purchaseOrderItemId}
                    style={{
                      backgroundColor: isExceeded ? '#FEF2F2' : undefined,
                    }}
                  >
                    <td style={{ textAlign: 'center', color: '#64748B', fontSize: '13px' }}>
                      {idx + 1}
                    </td>

                    <td>
                      <span
                        style={{
                          fontWeight: 700,
                          fontFamily: 'monospace',
                          color: '#2563EB',
                          fontSize: '13px',
                        }}
                      >
                        {item.productCode || `SP-${item.productId}`}
                      </span>
                    </td>

                    <td>
                      <div style={{ fontWeight: 600, color: '#1E293B' }}>{item.productName}</div>
                      {isExceeded && (
                        <div style={{ fontSize: '12px', color: '#DC2626', fontWeight: 600, marginTop: '2px' }}>
                          ⚠️ Vượt quá số lượng còn lại ({remaining})!
                        </div>
                      )}
                    </td>

                    <td style={{ textAlign: 'right', color: '#475569', fontSize: '13px' }}>
                      {formatCurrency(item.unitPrice)}
                    </td>

                    <td style={{ textAlign: 'right', fontWeight: 600, color: '#1E293B' }}>
                      {item.orderedQuantity.toLocaleString('vi-VN')}
                    </td>

                    <td
                      style={{
                        textAlign: 'right',
                        fontWeight: 600,
                        color: item.receivedQuantity > 0 ? '#059669' : '#64748B',
                      }}
                    >
                      {item.receivedQuantity.toLocaleString('vi-VN')}
                    </td>

                    <td
                      style={{
                        textAlign: 'right',
                        fontWeight: 700,
                        color: remaining > 0 ? '#EA580C' : '#94A3B8',
                      }}
                    >
                      {remaining.toLocaleString('vi-VN')}
                    </td>

                    {/* Receiving Input Column */}
                    <td style={{ textAlign: 'center', backgroundColor: '#F8FAFC' }}>
                      {remaining === 0 || isCompleted ? (
                        <span style={{ fontSize: '13px', color: '#059669', fontWeight: 600 }}>
                          ✓ Đã nhận đủ
                        </span>
                      ) : (
                        <div style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                          <input
                            type="number"
                            min="0"
                            max={remaining}
                            className="ewms-input"
                            value={receivingQty === 0 ? '' : receivingQty}
                            placeholder="0"
                            onChange={(e) => handleQuantityChange(item.productId, e.target.value)}
                            disabled={isCompleted}
                            style={{
                              width: '90px',
                              textAlign: 'center',
                              fontWeight: 700,
                              fontSize: '14px',
                              borderColor: isExceeded ? '#EF4444' : undefined,
                              backgroundColor: isExceeded ? '#FFF' : undefined,
                            }}
                          />
                          <button
                            type="button"
                            className="ewms-btn ewms-btn-secondary"
                            onClick={() => handleQuantityChange(item.productId, String(remaining))}
                            disabled={isCompleted}
                            style={{ padding: '4px 8px', fontSize: '11px' }}
                            title="Điền tối đa"
                          >
                            Max
                          </button>
                        </div>
                      )}
                    </td>

                    {/* Validation Status Column */}
                    <td style={{ textAlign: 'center' }}>
                      {isExceeded ? (
                        <span
                          style={{
                            display: 'inline-block',
                            padding: '3px 8px',
                            backgroundColor: '#FEE2E2',
                            color: '#991B1B',
                            borderRadius: '12px',
                            fontSize: '12px',
                            fontWeight: 600,
                          }}
                        >
                          Lỗi: MSG09
                        </span>
                      ) : itemValidation ? (
                        itemValidation.isValid ? (
                          <span
                            style={{
                              display: 'inline-block',
                              padding: '3px 8px',
                              backgroundColor: '#DCFCE7',
                              color: '#166534',
                              borderRadius: '12px',
                              fontSize: '12px',
                              fontWeight: 600,
                            }}
                          >
                            ✓ Hợp lệ
                          </span>
                        ) : (
                          <span
                            style={{
                              display: 'inline-block',
                              padding: '3px 8px',
                              backgroundColor: '#FEE2E2',
                              color: '#991B1B',
                              borderRadius: '12px',
                              fontSize: '12px',
                              fontWeight: 600,
                            }}
                          >
                            ✕ Không hợp lệ
                          </span>
                        )
                      ) : receivingQty > 0 ? (
                        <span
                          style={{
                            display: 'inline-block',
                            padding: '3px 8px',
                            backgroundColor: '#FEF3C7',
                            color: '#92400E',
                            borderRadius: '12px',
                            fontSize: '12px',
                            fontWeight: 500,
                          }}
                        >
                          Chưa kiểm
                        </span>
                      ) : (
                        <span style={{ color: '#94A3B8', fontSize: '12px' }}>—</span>
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {/* Summary Footer Bar */}
        <div
          style={{
            marginTop: '16px',
            padding: '12px 16px',
            backgroundColor: '#F8FAFC',
            borderRadius: '6px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            flexWrap: 'wrap',
            gap: '12px',
          }}
        >
          <div style={{ fontSize: '13px', color: '#475569' }}>
            Tổng số mặt hàng nhận đợt này: <strong>{activeReceivingItems.length}</strong> sản phẩm
          </div>
          <div style={{ fontSize: '14px', fontWeight: 700, color: '#1E293B' }}>
            Tổng số lượng nhận đợt này:{' '}
            <span style={{ color: '#2563EB', fontSize: '16px' }}>
              {totalReceivingUnits.toLocaleString('vi-VN')}
            </span>{' '}
            đơn vị
          </div>
        </div>
      </div>

      {/* Action Buttons Panel (UC12, UC13, UC14) */}
      <div
        className="ewms-card"
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexWrap: 'wrap',
          gap: '16px',
          padding: '20px 24px',
          backgroundColor: '#FFFFFF',
          borderTop: '3px solid #2563EB',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <span style={{ fontSize: '20px' }}>⚙️</span>
          <div>
            <div style={{ fontWeight: 700, fontSize: '14px', color: '#0F172A' }}>
              Quy trình thao tác nhận hàng
            </div>
            <div style={{ fontSize: '12px', color: '#64748B' }}>
              {isProcessing
                ? 'Đơn hàng đang ở trạng thái Processing. Hãy xác nhận nhập kho khi kiểm đếm xong.'
                : isCompleted
                ? 'Đơn hàng đã hoàn thành đầy đủ, không còn mặt hàng nào chờ nhận.'
                : 'Bấm Kiểm tra số lượng (UC12), sau đó Bắt đầu nhận hàng (UC13) và Xác nhận nhập kho (UC14).'}
            </div>
          </div>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flexWrap: 'wrap' }}>
          {/* Action 1: "Kiểm tra số lượng" (UC12) */}
          <button
            type="button"
            className="ewms-btn ewms-btn-outline"
            onClick={handleCheckQuantity}
            disabled={isChecking || isCompleted || totalReceivingUnits === 0}
            style={{ padding: '10px 18px' }}
          >
            {isChecking ? (
              <>
                <div className="ewms-spinner ewms-spinner-dark" />
                <span>Đang kiểm tra...</span>
              </>
            ) : (
              <>
                <span>🔍</span>
                <span>Kiểm tra số lượng (UC12)</span>
              </>
            )}
          </button>

          {/* Action 2: "Bắt đầu nhận hàng" (UC13) - Enabled when Approved or PartiallyReceived */}
          {(isApproved || isPartiallyReceived) && !isProcessing && !isCompleted && (
            <button
              type="button"
              className="ewms-btn ewms-btn-secondary"
              onClick={handleStartReceiving}
              disabled={isStarting}
              style={{
                padding: '10px 18px',
                backgroundColor: '#EDE9FE',
                color: '#5B21B6',
                borderColor: '#C4B5FD',
              }}
            >
              {isStarting ? (
                <>
                  <div className="ewms-spinner ewms-spinner-dark" />
                  <span>Đang cập nhật...</span>
                </>
              ) : (
                <>
                  <span>🚀</span>
                  <span>Bắt đầu nhận hàng (UC13)</span>
                </>
              )}
            </button>
          )}

          {/* Action 4: "Hủy thao tác" (UC13) - Enabled when in Processing */}
          {isProcessing && (
            <button
              type="button"
              className="ewms-btn ewms-btn-danger"
              onClick={() => setShowCancelModal(true)}
              disabled={isCancelling}
              style={{ padding: '10px 18px' }}
            >
              <span>✕</span>
              <span>Hủy thao tác</span>
            </button>
          )}

          {/* Action 3: "Xác nhận nhập kho" (UC13 + UC14) */}
          <button
            type="button"
            className="ewms-btn ewms-btn-success"
            onClick={handleOpenConfirmModal}
            disabled={!isProcessing || totalReceivingUnits === 0 || isConfirming}
            style={{
              padding: '10px 20px',
              fontSize: '14px',
              fontWeight: 700,
              boxShadow: isProcessing && totalReceivingUnits > 0 ? '0 2px 6px rgba(16,185,129,0.3)' : undefined,
            }}
          >
            {isConfirming ? (
              <>
                <div className="ewms-spinner" />
                <span>Đang ghi nhận nhập kho...</span>
              </>
            ) : (
              <>
                <span>📥</span>
                <span>Xác nhận nhập kho (UC14)</span>
              </>
            )}
          </button>
        </div>
      </div>

      {/* Confirmation Modal (UC13 + UC14) */}
      {showConfirmModal && (
        <div className="ewms-modal-backdrop" onClick={() => setShowConfirmModal(false)}>
          <div
            className="ewms-modal"
            style={{
              maxWidth: '650px',
              width: '100%',
              backgroundColor: '#FFFFFF',
              borderRadius: '12px',
              padding: '28px',
              boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.2)',
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '12px' }}>
              <span style={{ fontSize: '28px' }}>📦</span>
              <div>
                <h3 style={{ fontSize: '18px', fontWeight: 700, color: '#0F172A', margin: 0 }}>
                  Xác nhận Nhập kho Hàng hóa
                </h3>
                <p style={{ fontSize: '13px', color: '#64748B', margin: '2px 0 0' }}>
                  Đơn hàng: <strong style={{ color: '#2563EB' }}>{po.purchaseOrderCode}</strong>
                </p>
              </div>
            </div>

            <div
              style={{
                backgroundColor: '#EFF6FF',
                border: '1px solid #BFDBFE',
                borderRadius: '8px',
                padding: '14px 16px',
                marginBottom: '16px',
                fontSize: '13px',
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px' }}>
                <span style={{ color: '#475569' }}>Kho tiếp nhận:</span>
                <strong style={{ color: '#1E40AF' }}>
                  {selectedWarehouse?.warehouseName} ({selectedWarehouse?.warehouseCode})
                </strong>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: '#475569' }}>Ghi chú nhập:</span>
                <span style={{ color: '#1E293B', fontStyle: receivingNote ? 'normal' : 'italic' }}>
                  {receivingNote || 'Không có ghi chú'}
                </span>
              </div>
            </div>

            {/* Modal Items Breakdown */}
            <div style={{ marginBottom: '16px' }}>
              <div style={{ fontSize: '13px', fontWeight: 600, color: '#334155', marginBottom: '8px' }}>
                Danh sách sản phẩm nhập kho ({activeReceivingItems.length} mặt hàng):
              </div>
              <div style={{ maxHeight: '200px', overflowY: 'auto', border: '1px solid #E2E8F0', borderRadius: '6px' }}>
                <table className="ewms-table" style={{ fontSize: '13px' }}>
                  <thead>
                    <tr>
                      <th>Sản phẩm</th>
                      <th style={{ width: '120px', textAlign: 'right' }}>Số lượng nhập</th>
                    </tr>
                  </thead>
                  <tbody>
                    {activeReceivingItems.map((item) => (
                      <tr key={item.purchaseOrderItemId}>
                        <td>
                          <div style={{ fontWeight: 600 }}>{item.productName}</div>
                          <div style={{ fontSize: '11px', color: '#64748B', fontFamily: 'monospace' }}>
                            {item.productCode}
                          </div>
                        </td>
                        <td style={{ textAlign: 'right', fontWeight: 700, color: '#059669', fontSize: '14px' }}>
                          +{item.qtyToReceive.toLocaleString('vi-VN')}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                padding: '10px 14px',
                backgroundColor: '#F8FAFC',
                borderRadius: '6px',
                marginBottom: '20px',
                fontSize: '14px',
                fontWeight: 700,
              }}
            >
              <span>Tổng số lượng nhập kho:</span>
              <span style={{ color: '#059669', fontSize: '16px' }}>
                +{totalReceivingUnits.toLocaleString('vi-VN')} đơn vị
              </span>
            </div>

            <p style={{ fontSize: '13px', color: '#64748B', marginBottom: '24px' }}>
              ⚠️ Thao tác này sẽ cập nhật trực tiếp số lượng tồn kho vật lý và ghi lại nhật ký giao dịch
              nhập kho (IN transaction) hoàn chỉnh.
            </p>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
              <button
                type="button"
                className="ewms-btn ewms-btn-secondary"
                onClick={() => setShowConfirmModal(false)}
                disabled={isConfirming}
              >
                Hủy bỏ
              </button>
              <button
                type="button"
                className="ewms-btn ewms-btn-success"
                onClick={handleConfirmReceive}
                disabled={isConfirming}
                style={{ padding: '10px 20px', fontWeight: 700 }}
              >
                {isConfirming ? (
                  <>
                    <div className="ewms-spinner" />
                    <span>Đang xử lý...</span>
                  </>
                ) : (
                  <>
                    <span>✓</span>
                    <span>Xác nhận nhập kho ngay</span>
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Cancel Confirmation Modal */}
      {showCancelModal && (
        <div className="ewms-modal-backdrop" onClick={() => setShowCancelModal(false)}>
          <div
            className="ewms-modal"
            style={{
              maxWidth: '480px',
              width: '100%',
              backgroundColor: '#FFFFFF',
              borderRadius: '12px',
              padding: '24px',
              boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.2)',
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <div style={{ fontSize: '36px', marginBottom: '12px', textAlign: 'center' }}>⚠️</div>
            <h3 style={{ fontSize: '18px', fontWeight: 700, color: '#0F172A', textAlign: 'center', marginBottom: '8px' }}>
              Hủy phiên nhận hàng?
            </h3>
            <p style={{ fontSize: '14px', color: '#64748B', textAlign: 'center', marginBottom: '20px' }}>
              Thao tác này sẽ hủy phiên nhận hàng hiện tại và khôi phục trạng thái đơn hàng về trước đó mà không làm thay đổi tồn kho.
            </p>

            <div style={{ display: 'flex', justifyContent: 'center', gap: '12px' }}>
              <button
                type="button"
                className="ewms-btn ewms-btn-secondary"
                onClick={() => setShowCancelModal(false)}
                disabled={isCancelling}
              >
                Đóng
              </button>
              <button
                type="button"
                className="ewms-btn ewms-btn-danger"
                onClick={handleCancelReceiving}
                disabled={isCancelling}
              >
                {isCancelling ? 'Đang hủy...' : 'Đồng ý hủy'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ReceivingDetailPage;
