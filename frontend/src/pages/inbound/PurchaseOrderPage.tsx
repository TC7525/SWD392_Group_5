import React, { useCallback, useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { Icon } from '../../components/common/Icon';
import { useAuth } from '../../contexts/AuthContext';
import { masterDataService } from '../../services/masterDataService';
import { purchaseOrderService } from '../../services/orderService';
import { type ProductItem, type SupplierItem } from '../../types/master.types';
import {
  type PurchaseOrderDetail,
  type PurchaseOrderItem,
} from '../../types/order.types';

interface DraftLineItem {
  product_id: string;
  ordered_quantity: number;
  unit_price: number;
  note: string;
}

export const PurchaseOrderPage: React.FC = () => {
  const { user, notifyError, notifySuccess } = useAuth();
  const [orders, setOrders] = useState<PurchaseOrderItem[]>([]);
  const [suppliers, setSuppliers] = useState<SupplierItem[]>([]);
  const [products, setProducts] = useState<ProductItem[]>([]);

  // Filters (Screen 6)
  const [fromDate, setFromDate] = useState('2026-01-01');
  const [toDate, setToDate] = useState('2026-12-31');
  const [filterSupplierId, setFilterSupplierId] = useState('');
  const [filterStatus, setFilterStatus] = useState('ALL');
  const [keyword, setKeyword] = useState('');
  const [pageSize, setPageSize] = useState('10');

  // Modal Lập / Xem Phiếu Nhập (Screen 7)
  const [showVoucherModal, setShowVoucherModal] = useState(false);
  const [supplierId, setSupplierId] = useState('');
  const [voucherDate, setVoucherDate] = useState(
    new Date().toISOString().slice(0, 10)
  );
  const [voucherNote, setVoucherNote] = useState(
    'Nhập kho thiết bị điện tử theo hợp đồng'
  );
  const [lines, setLines] = useState<DraftLineItem[]>([]);

  // Modal Thực nhập / Chi tiết PO
  const [selectedOrder, setSelectedOrder] =
    useState<PurchaseOrderDetail | null>(null);
  const [receiveQuantities, setReceiveQuantities] = useState<
    Record<string, number>
  >({});

  const loadOrders = useCallback(() => {
    if (!user) return;
    purchaseOrderService
      .search(user.current_warehouse_id)
      .then((res) => setOrders(res.items || []))
      .catch((err: unknown) =>
        notifyError(
          err instanceof Error ? err.message : 'Lỗi tải Phiếu nhập kho'
        )
      );
  }, [user, notifyError]);

  useEffect(() => {
    if (!user) return;
    loadOrders();
    masterDataService
      .searchSuppliers()
      .then((res) => {
        const list = res.items || [];
        setSuppliers(list);
        if (list.length > 0) setSupplierId(list[0].id);
      })
      .catch(() => {});
    masterDataService
      .searchProducts(user.current_warehouse_id)
      .then((res) => setProducts(res.items || []))
      .catch(() => {});
  }, [user, loadOrders]);

  const openCreateModal = () => {
    const firstProd = products[0];
    setSupplierId(suppliers[0]?.id || '');
    setVoucherDate(new Date().toISOString().slice(0, 10));
    setVoucherNote('Nhập kho lô hàng thiết bị mới');
    setLines(
      firstProd
        ? [
            {
              product_id: firstProd.id,
              ordered_quantity: 10,
              unit_price: firstProd.unit_price,
              note: '',
            },
          ]
        : []
    );
    setShowVoucherModal(true);
  };

  const handleAddLine = () => {
    const prod =
      products[lines.length % Math.max(1, products.length)] || products[0];
    if (!prod) return;
    setLines([
      ...lines,
      {
        product_id: prod.id,
        ordered_quantity: 5,
        unit_price: prod.unit_price,
        note: '',
      },
    ]);
  };

  const handleSaveOrder = async (approveImmediately: boolean) => {
    if (!user || !supplierId || lines.length === 0) {
      notifyError('Vui lòng chọn Nhà cung cấp và ít nhất 1 mặt hàng nhập kho.');
      return;
    }
    try {
      const createdId = await purchaseOrderService.create({
        supplier_id: supplierId,
        warehouse_id: user.current_warehouse_id,
        note: voucherNote,
        items: lines.map((l) => ({
          product_id: l.product_id,
          ordered_quantity: Number(l.ordered_quantity),
          unit_price: Number(l.unit_price),
        })),
      });

      if (approveImmediately && createdId) {
        await purchaseOrderService.approve(createdId).catch(() => {});
        const detail = await purchaseOrderService.getById(createdId);
        if (detail && detail.items.length > 0) {
          await purchaseOrderService.receiveGoods(
            createdId,
            detail.items.map((i) => ({
              purchase_order_item_id: i.id,
              receive_quantity: i.ordered_quantity,
            })),
            'Duyệt và Thực nhập kho toàn bộ'
          );
        }
        notifySuccess('Đã lập, Duyệt và Thực nhập kho thành công!');
      } else {
        notifySuccess('Đã lưu Phiếu Nhập Kho (Chờ duyệt) thành công!');
      }
      setShowVoucherModal(false);
      loadOrders();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi lập Phiếu nhập kho');
    }
  };

  const handleCancel = async (id: string) => {
    if (!window.confirm('Hủy phiếu nhập này? Đơn đã lưu sẽ chuyển sang trạng thái Đã hủy.')) {
      return;
    }
    try {
      await purchaseOrderService.cancel(id);
      notifySuccess('Đã hủy Phiếu Nhập Kho.');
      setSelectedOrder((current) => (current?.id === id ? null : current));
      loadOrders();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Không thể hủy phiếu nhập');
    }
  };

  const openDetailModal = async (id: string) => {
    try {
      const detail = await purchaseOrderService.getById(id);
      setSelectedOrder(detail);
      const initQty: Record<string, number> = {};
      detail.items.forEach((i) => {
        const rem =
          i.remaining_quantity ?? i.ordered_quantity - i.received_quantity;
        initQty[i.id] = Math.max(0, rem);
      });
      setReceiveQuantities(initQty);
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi tải chi tiết phiếu');
    }
  };

  const handleReceiveGoods = async () => {
    if (!selectedOrder) return;
    try {
      // Nếu phiếu đang ở trạng thái Chờ duyệt (1), tự động phê duyệt trước khi thực nhập
      if (selectedOrder.status === 1) {
        await purchaseOrderService.approve(selectedOrder.id);
      }

      const itemsToReceive = selectedOrder.items
        .filter((i) => (receiveQuantities[i.id] || 0) > 0)
        .map((i) => ({
          purchase_order_item_id: i.id,
          receive_quantity: Number(receiveQuantities[i.id]),
        }));

      await purchaseOrderService.receiveGoods(
        selectedOrder.id,
        itemsToReceive,
        'Thực nhập hàng hóa vào kho'
      );
      notifySuccess('Đã ghi nhận Thực nhập kho & cộng tồn kho tự động!');
      setSelectedOrder(null);
      loadOrders();
    } catch (err: unknown) {
      notifyError(
        err instanceof Error ? err.message : 'Lỗi thực nhập hàng hóa'
      );
    }
  };

  // eStatus_PurchaseOrder:
  // 1 = Chờ duyệt (Pending)
  // 2 = Đã duyệt (Approved)
  // 3 = Từ chối (Rejected)
  // 4 = Nhập một phần (PartiallyReceived)
  // 5 = Hoàn thành (Completed)
  // 6 = Đã hủy (Cancelled)
  const getStatusPillClass = (status: number) => {
    if (status === 5 || status === 2) return 'emerald';
    if (status === 3 || status === 6) return 'danger';
    return 'amber';
  };

  const filteredOrders = orders.filter((po) => {
    const matchSupplier =
      !filterSupplierId || po.supplier_id === filterSupplierId;
    const matchStatus =
      filterStatus === 'ALL' || String(po.status) === filterStatus;
    const matchKw =
      !keyword ||
      po.purchase_order_code.toLowerCase().includes(keyword.toLowerCase()) ||
      po.supplier_name.toLowerCase().includes(keyword.toLowerCase()) ||
      (po.note && po.note.toLowerCase().includes(keyword.toLowerCase()));
    return matchSupplier && matchStatus && matchKw;
  });

  const sumOrderedQty = filteredOrders.reduce(
    (s, x) => s + x.total_ordered_quantity,
    0
  );
  const sumAmount = filteredOrders.reduce((s, x) => s + x.total_amount, 0);

  const modalTotalQty = lines.reduce(
    (s, l) => s + Number(l.ordered_quantity || 0),
    0
  );
  const modalTotalValue = lines.reduce(
    (s, l) => s + Number(l.ordered_quantity || 0) * Number(l.unit_price || 0),
    0
  );

  return (
    <div>
      {/* PAGE HEADER (Screen 6) */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <Icon name="inbox-in" color="#10b981" />
            Quản Lý Phiếu Nhập Kho
          </h1>
          <p>
            Ghi nhận nhập hàng từ Nhà cung cấp và tự động cập nhật số lượng tồn kho sau mỗi lần thực nhập
          </p>
        </div>
        <div className="page-actions">
          <button className="btn btn-emerald" onClick={openCreateModal}>
            <Icon name="add" /> Nhập kho
          </button>
          <button
            className="btn btn-outline"
            onClick={() =>
              notifySuccess('Đã xuất danh sách Phiếu nhập kho ra Excel!')
            }
          >
            <Icon name="file-excel" /> Excel
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
            value={filterSupplierId}
            onChange={setFilterSupplierId}
            options={[
              { value: '', label: 'Tất cả nhà cung cấp...' },
              ...suppliers.map((s) => ({
                value: s.id,
                label: s.supplier_name,
                subLabel: s.supplier_code,
              })),
            ]}
          />

          <CustomSelect
            variant="filter"
            value={filterStatus}
            onChange={setFilterStatus}
            options={[
              { value: 'ALL', label: 'Tất cả trạng thái' },
              { value: '1', label: 'Chờ duyệt' },
              { value: '2', label: 'Đã duyệt' },
              { value: '4', label: 'Nhập một phần' },
              { value: '5', label: 'Hoàn thành' },
              { value: '3', label: 'Từ chối' },
              { value: '6', label: 'Đã hủy' },
            ]}
          />

          <div className="search-box-wrapper">
            <span className="search-box-icon">
              <Icon name="search" />
            </span>
            <input
              className="search-box-input"
              placeholder="Tìm số phiếu, nhà cung cấp, diễn giải..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </div>
        </div>

        <div className="table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>STT</th>
                <th>SỐ PHIẾU</th>
                <th>NGÀY NHẬP</th>
                <th>KHO NHẬP</th>
                <th>NHÀ CUNG CẤP / ĐỐI TƯỢNG</th>
                <th>DIỄN GIẢI</th>
                <th>NGƯỜI LẬP</th>
                <th style={{ textAlign: 'right' }}>TỔNG SỐ LƯỢNG</th>
                <th style={{ textAlign: 'right' }}>TỔNG GIÁ TRỊ</th>
                <th>TRẠNG THÁI</th>
                <th style={{ textAlign: 'center' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {filteredOrders.map((po, idx) => (
                <tr key={po.id}>
                  <td>{idx + 1}</td>
                  <td>
                    <span
                      className="voucher-code"
                      onClick={() => openDetailModal(po.id)}
                    >
                      {po.purchase_order_code}
                    </span>
                  </td>
                  <td>{new Date(po.order_date).toLocaleDateString('vi-VN')}</td>
                  <td style={{ fontWeight: 600 }}>
                    {po.warehouse_name.toUpperCase()}
                  </td>
                  <td>{po.supplier_name}</td>
                  <td style={{ color: '#475569' }}>
                    {po.note || 'Nhập kho thiết bị điện tử'}
                  </td>
                  <td>Quản trị viên</td>
                  <td style={{ textAlign: 'right', fontWeight: 700 }}>
                    {po.total_received_quantity}/{po.total_ordered_quantity}
                  </td>
                  <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                    {po.total_amount.toLocaleString('vi-VN')}
                  </td>
                  <td>
                    <span
                      className={`status-pill ${getStatusPillClass(po.status)}`}
                    >
                      {po.status_name}
                    </span>
                  </td>
                  <td style={{ textAlign: 'center', whiteSpace: 'nowrap' }}>
                    <button
                      className="icon-action-btn"
                      title="In phiếu nhập"
                      onClick={() => window.print()}
                    >
                      <Icon name="print" />
                    </button>
                    <button
                      className="icon-action-btn"
                      title="Xem chi tiết phiếu"
                      onClick={() => openDetailModal(po.id)}
                    >
                      <Icon name="pencil" />
                    </button>
                    {/* Đơn đã lưu (Chờ duyệt) mới được nhập kho hoặc hủy */}
                    {po.status === 1 && (
                      <>
                        <button
                          className="btn btn-emerald btn-sm"
                          style={{ marginLeft: 6 }}
                          onClick={() => openDetailModal(po.id)}
                        >
                          <Icon name="inbox-in" /> Nhập kho
                        </button>
                        <button
                          className="btn btn-danger btn-sm"
                          style={{ marginLeft: 4 }}
                          title="Hủy phiếu nhập đã lưu"
                          onClick={() => handleCancel(po.id)}
                        >
                          <Icon name="cross-small" /> Hủy
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
              {filteredOrders.length === 0 && (
                <tr>
                  <td
                    colSpan={11}
                    style={{ textAlign: 'center', padding: 24, color: '#64748b' }}
                  >
                    Chưa có phiếu nhập kho nào trong kỳ.
                  </td>
                </tr>
              )}
            </tbody>
            <tfoot>
              <tr>
                <td colSpan={7}>TỔNG CỘNG:</td>
                <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                  {sumOrderedQty.toLocaleString('vi-VN')}
                </td>
                <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                  {sumAmount.toLocaleString('vi-VN')} 
                </td>
                <td colSpan={2} />
              </tr>
            </tfoot>
          </table>
        </div>

        <div className="pagination-bar">
          <span>
            Hiển thị <strong>1 - {filteredOrders.length}</strong> trên tổng số{' '}
            <strong>{filteredOrders.length}</strong> dòng
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
            <button className="page-btn" aria-label="Trang trước">
              <Icon name="angle-double-small-left" />
            </button>
            <button className="page-btn active">1</button>
            <button className="page-btn" aria-label="Trang sau">
              <Icon name="angle-double-small-right" />
            </button>
          </div>
        </div>
      </div>

      {/* MODAL LẬP PHIẾU NHẬP KHO (Screen 7) */}
      {showVoucherModal && (
        <div
          className="modal-backdrop"
          onClick={() => setShowVoucherModal(false)}
        >
          <div
            className="modal-dialog lg"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <div className="modal-title">
                <Icon name="memo" color="#10b981" />
                Lập Phiếu Nhập Kho Mới
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setShowVoucherModal(false)}
              >
                <Icon name="cross-small" />
              </button>
            </div>

            <div className="modal-body">
              <div className="form-grid-4">
                <div className="form-group">
                  <label>
                    Số phiếu nhập <span className="req">*</span>
                  </label>
                  <input
                    className="form-control"
                    value={`PO-${new Date().getFullYear()}-${String(
                      orders.length + 1
                    ).padStart(4, '0')}`}
                    readOnly
                  />
                </div>
                <div className="form-group">
                  <label>
                    Ngày nhập <span className="req">*</span>
                  </label>
                  <input
                    type="date"
                    className="form-control"
                    value={voucherDate}
                    onChange={(e) => setVoucherDate(e.target.value)}
                  />
                </div>
                <div className="form-group">
                  <label>
                    Kho nhập <span className="req">*</span>
                  </label>
                  <input
                    className="form-control"
                    value={user?.current_warehouse_name.toUpperCase() || ''}
                    readOnly
                  />
                </div>
                <div className="form-group">
                  <label>Người lập phiếu</label>
                  <input
                    className="form-control"
                    value={user?.full_name || 'Quản trị viên'}
                    readOnly
                  />
                </div>
              </div>

              <div className="form-grid-2">
                <div className="form-group">
                  <label>
                    Nhà cung cấp / Đối tượng <span className="req">*</span>
                  </label>
                  <CustomSelect
                    variant="form"
                    value={supplierId}
                    onChange={setSupplierId}
                    options={suppliers.map((s) => ({
                      value: s.id,
                      label: `${s.supplier_name} (${s.supplier_code})`,
                      subLabel: s.phone,
                    }))}
                  />
                </div>
                <div className="form-group">
                  <label>
                    Diễn giải / Lý do nhập kho <span className="req">*</span>
                  </label>
                  <input
                    className="form-control"
                    value={voucherNote}
                    onChange={(e) => setVoucherNote(e.target.value)}
                    placeholder="Nhập diễn giải phiếu nhập..."
                  />
                </div>
              </div>

              {/* SUB-TABLE CHI TIẾT HÀNG HÓA NHẬP KHO */}
              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  margin: '16px 0 10px',
                }}
              >
                <div
                  style={{ fontWeight: 700, fontSize: 13.5, color: '#0f172a' }}
                >
                  <Icon name="list" /> Chi Tiết Hàng Hóa Nhập Kho
                </div>
                <button
                  type="button"
                  className="btn btn-outline btn-sm"
                  onClick={handleAddLine}
                >
                  <Icon name="add" /> Thêm Dòng
                </button>
              </div>

              <div className="table-wrapper">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th style={{ width: 45 }}>STT</th>
                      <th>Mặt hàng</th>
                      <th style={{ width: 85 }}>ĐVT</th>
                      <th style={{ width: 120 }}>Số lượng</th>
                      <th style={{ width: 150 }}>Đơn giá nhập</th>
                      <th style={{ width: 150, textAlign: 'right' }}>
                        Thành tiền
                      </th>
                      <th style={{ width: 180 }}>Ghi chú</th>
                      <th style={{ width: 55, textAlign: 'center' }}>Xóa</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((line, idx) => {
                      const prod = products.find(
                        (p) => p.id === line.product_id
                      );
                      const lineTotal =
                        Number(line.ordered_quantity || 0) *
                        Number(line.unit_price || 0);
                      return (
                        <tr key={idx}>
                          <td>{idx + 1}</td>
                          <td>
                            <CustomSelect
                              variant="form"
                              value={line.product_id}
                              onChange={(val) => {
                                const selected = products.find(
                                  (p) => p.id === val
                                );
                                const next = [...lines];
                                next[idx].product_id = val;
                                if (selected) {
                                  next[idx].unit_price = selected.unit_price;
                                }
                                setLines(next);
                              }}
                              options={products.map((p) => ({
                                value: p.id,
                                label: `${p.product_code} - ${p.product_name}`,
                                subLabel: `Đơn giá: ${p.unit_price.toLocaleString(
                                  'vi-VN'
                                )} ₫`,
                              }))}
                            />
                          </td>
                          <td>{prod?.brand || 'Cái'}</td>
                          <td>
                            <input
                              type="number"
                              min={1}
                              className="form-control"
                              style={{ width: '100%' }}
                              value={line.ordered_quantity}
                              onChange={(e) => {
                                const next = [...lines];
                                next[idx].ordered_quantity = Number(
                                  e.target.value
                                );
                                setLines(next);
                              }}
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              min={0}
                              className="form-control"
                              style={{ width: '100%' }}
                              value={line.unit_price}
                              onChange={(e) => {
                                const next = [...lines];
                                next[idx].unit_price = Number(e.target.value);
                                setLines(next);
                              }}
                            />
                          </td>
                          <td
                            style={{ textAlign: 'right' }}
                            className="text-emerald-bold"
                          >
                            {lineTotal.toLocaleString('vi-VN')} 
                          </td>
                          <td>
                            <input
                              className="form-control"
                              style={{ width: '100%' }}
                              placeholder="Ghi chú..."
                              value={line.note}
                              onChange={(e) => {
                                const next = [...lines];
                                next[idx].note = e.target.value;
                                setLines(next);
                              }}
                            />
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            <button
                              type="button"
                              className="icon-action-btn danger"
                              onClick={() =>
                                setLines(lines.filter((_, i) => i !== idx))
                              }
                            >
                              <Icon name="trash" />
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>

              {/* SUMMARY BOX */}
              <div className="voucher-summary-box">
                <div>
                  <div style={{ fontSize: 11.5, color: '#64748b' }}>
                    Tổng số lượng nhập:
                  </div>
                  <div
                    style={{ fontSize: 18, fontWeight: 800, color: '#059669' }}
                  >
                    {modalTotalQty.toLocaleString('vi-VN')}
                  </div>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <div style={{ fontSize: 11.5, color: '#64748b' }}>
                    Tổng giá trị phiếu nhập:
                  </div>
                  <div
                    style={{ fontSize: 18, fontWeight: 800, color: '#059669' }}
                  >
                    {modalTotalValue.toLocaleString('vi-VN')} ₫
                  </div>
                </div>
              </div>
            </div>

            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-outline"
                onClick={() => setShowVoucherModal(false)}
              >
                Hủy
              </button>
              <button
                type="button"
                className="btn btn-outline"
                onClick={() => handleSaveOrder(false)}
              >
                <Icon name="bookmark" /> Lưu Bản Nháp
              </button>
              <button
                type="button"
                className="btn btn-emerald"
                onClick={() => handleSaveOrder(true)}
              >
                <Icon name="check" /> Lưu & Duyệt Phiếu
              </button>
            </div>
          </div>
        </div>
      )}

      {/* MODAL CHI TIẾT & THỰC NHẬP KHO */}
      {selectedOrder && (
        <div className="modal-backdrop" onClick={() => setSelectedOrder(null)}>
          <div
            className="modal-dialog lg"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <div className="modal-title">
                <Icon name="inbox-in" color="#10b981" /> Chi Tiết Phiếu Nhập Kho:{' '}
                {selectedOrder.purchase_order_code}{' '}
                <span
                  className={`status-pill ${getStatusPillClass(
                    selectedOrder.status
                  )}`}
                >
                  {selectedOrder.status_name}
                </span>
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setSelectedOrder(null)}
              >
                <Icon name="cross-small" />
              </button>
            </div>
            <div className="modal-body">
              <div className="table-wrapper">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>STT</th>
                      <th>MÃ HÀNG</th>
                      <th>TÊN HÀNG HÓA</th>
                      <th style={{ textAlign: 'right' }}>ĐẶT MUA</th>
                      <th style={{ textAlign: 'right' }}>ĐÃ NHẬP</th>
                      <th style={{ textAlign: 'right' }}>CÒN LẠI</th>
                      <th style={{ textAlign: 'right' }}>ĐƠN GIÁ</th>
                      {selectedOrder.status === 1 && (
                        <th style={{ width: 160 }}>SL THỰC NHẬP ĐỢT NÀY</th>
                      )}
                    </tr>
                  </thead>
                  <tbody>
                    {selectedOrder.items.map((item, idx) => {
                      const rem =
                        item.remaining_quantity ??
                        item.ordered_quantity - item.received_quantity;
                      return (
                        <tr key={item.id}>
                          <td>{idx + 1}</td>
                          <td className="voucher-code">{item.product_code}</td>
                          <td style={{ fontWeight: 600 }}>
                            {item.product_name}
                          </td>
                          <td style={{ textAlign: 'right' }}>
                            {item.ordered_quantity}
                          </td>
                          <td
                            style={{ textAlign: 'right' }}
                            className="text-emerald-bold"
                          >
                            {item.received_quantity}
                          </td>
                          <td style={{ textAlign: 'right', fontWeight: 700 }}>
                            {rem}
                          </td>
                          <td style={{ textAlign: 'right' }}>
                            {item.unit_price.toLocaleString('vi-VN')} ₫
                          </td>
                          {selectedOrder.status === 1 && (
                            <td>
                              <input
                                type="number"
                                min={0}
                                max={rem}
                                className="form-control"
                                style={{ width: '100%' }}
                                value={receiveQuantities[item.id] ?? 0}
                                onChange={(e) =>
                                  setReceiveQuantities({
                                    ...receiveQuantities,
                                    [item.id]: Number(e.target.value),
                                  })
                                }
                              />
                            </td>
                          )}
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
            <div className="modal-footer">
              <button
                className="btn btn-outline"
                onClick={() => setSelectedOrder(null)}
              >
                Đóng
              </button>
              {selectedOrder.status === 1 && (
                <>
                  <button
                    className="btn btn-danger"
                    onClick={() => handleCancel(selectedOrder.id)}
                  >
                    <Icon name="cross-small" /> Hủy đơn
                  </button>
                  <button
                    className="btn btn-emerald"
                    onClick={handleReceiveGoods}
                  >
                    Xác nhận Thực Nhập
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
