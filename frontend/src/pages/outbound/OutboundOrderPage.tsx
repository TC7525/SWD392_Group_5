import React, { useCallback, useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { useAuth } from '../../contexts/AuthContext';
import { masterDataService } from '../../services/masterDataService';
import { outboundOrderService } from '../../services/orderService';
import { type CustomerItem, type ProductItem } from '../../types/master.types';
import { type OutboundOrderItem } from '../../types/order.types';

interface OutboundDraftLine {
  product_id: string;
  quantity: number;
  unit_price: number;
  note: string;
}

export const OutboundOrderPage: React.FC = () => {
  const { user, notifyError, notifySuccess } = useAuth();
  const [orders, setOrders] = useState<OutboundOrderItem[]>([]);
  const [customers, setCustomers] = useState<CustomerItem[]>([]);
  const [products, setProducts] = useState<ProductItem[]>([]);

  // Filters
  const [fromDate, setFromDate] = useState('2026-01-01');
  const [toDate, setToDate] = useState('2026-12-31');
  const [filterCustomerId, setFilterCustomerId] = useState('');
  const [filterStatus, setFilterStatus] = useState('ALL');
  const [keyword, setKeyword] = useState('');
  const [pageSize, setPageSize] = useState('10');

  // Modal Lập Phiếu Xuất Kho
  const [showModal, setShowModal] = useState(false);
  const [customerId, setCustomerId] = useState('');
  const [voucherDate, setVoucherDate] = useState(
    new Date().toISOString().slice(0, 10)
  );
  const [voucherNote, setVoucherNote] = useState('Xuất kho bán hàng cho đại lý');
  const [lines, setLines] = useState<OutboundDraftLine[]>([]);

  const loadOrders = useCallback(() => {
    if (!user) return;
    outboundOrderService
      .search(user.current_warehouse_id)
      .then((res) => setOrders(res.items || []))
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải Phiếu xuất kho')
      );
  }, [user, notifyError]);

  useEffect(() => {
    if (!user) return;
    loadOrders();
    masterDataService
      .searchCustomers()
      .then((res) => {
        const list = res.items || [];
        setCustomers(list);
        if (list.length > 0) setCustomerId(list[0].id);
      })
      .catch(() => {});
    masterDataService
      .searchProducts(user.current_warehouse_id)
      .then((res) => setProducts(res.items || []))
      .catch(() => {});
  }, [user, loadOrders]);

  const openCreateModal = () => {
    const firstProd = products[0];
    setCustomerId(customers[0]?.id || '');
    setVoucherDate(new Date().toISOString().slice(0, 10));
    setVoucherNote('Xuất kho giao khách hàng theo đơn đặt hàng');
    setLines(
      firstProd
        ? [
            {
              product_id: firstProd.id,
              quantity: 2,
              unit_price: firstProd.unit_price,
              note: '',
            },
          ]
        : []
    );
    setShowModal(true);
  };

  const handleAddLine = () => {
    const prod = products[lines.length % Math.max(1, products.length)] || products[0];
    if (!prod) return;
    setLines([
      ...lines,
      {
        product_id: prod.id,
        quantity: 1,
        unit_price: prod.unit_price,
        note: '',
      },
    ]);
  };

  const handleSaveOrder = async (approveImmediately: boolean) => {
    if (!user || !customerId || lines.length === 0) {
      notifyError('Vui lòng chọn Khách hàng và ít nhất 1 mặt hàng xuất kho.');
      return;
    }
    try {
      const createdId = await outboundOrderService.create({
        customer_id: customerId,
        warehouse_id: user.current_warehouse_id,
        note: voucherNote,
        items: lines.map((l) => ({
          product_id: l.product_id,
          quantity: Number(l.quantity),
          unit_price: Number(l.unit_price),
        })),
      });
      if (approveImmediately && createdId) {
        await outboundOrderService.approve(createdId).catch(() => {});
        notifySuccess('Đã lập và Phê duyệt Phiếu Xuất Kho thành công!');
      } else {
        notifySuccess('Đã lưu Phiếu Xuất Kho (Chờ duyệt)!');
      }
      setShowModal(false);
      loadOrders();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi lập Phiếu xuất kho');
    }
  };

  const handleApprove = async (id: string) => {
    try {
      await outboundOrderService.approve(id);
      notifySuccess('Đã duyệt Phiếu Xuất Kho!');
      loadOrders();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi duyệt phiếu xuất');
    }
  };

  const handleReject = async (id: string) => {
    const reason = window.prompt('Nhập lý do từ chối phiếu xuất:', 'Khách hủy đơn');
    if (!reason) return;
    try {
      await outboundOrderService.reject(id, reason);
      notifySuccess('Đã từ chối Phiếu Xuất Kho.');
      loadOrders();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi từ chối phiếu xuất');
    }
  };

  const handleIssueGoods = async (id: string) => {
    try {
      await outboundOrderService.issueGoods(id, 'Thực xuất kho giao khách hàng');
      notifySuccess('Đã thực xuất kho & trừ tồn kho thành công!');
      loadOrders();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi thực xuất kho');
    }
  };

  const filteredOrders = orders.filter((o) => {
    const matchCus = !filterCustomerId || o.customer_id === filterCustomerId;
    const matchSt = filterStatus === 'ALL' || String(o.status) === filterStatus;
    const matchKw =
      !keyword ||
      o.order_code.toLowerCase().includes(keyword.toLowerCase()) ||
      o.customer_name.toLowerCase().includes(keyword.toLowerCase()) ||
      (o.note && o.note.toLowerCase().includes(keyword.toLowerCase()));
    return matchCus && matchSt && matchKw;
  });

  const sumQty = filteredOrders.reduce((s, x) => s + x.total_quantity, 0);
  const sumAmount = filteredOrders.reduce((s, x) => s + x.total_amount, 0);

  const modalTotalQty = lines.reduce((s, l) => s + Number(l.quantity || 0), 0);
  const modalTotalValue = lines.reduce(
    (s, l) => s + Number(l.quantity || 0) * Number(l.unit_price || 0),
    0
  );

  return (
    <div>
      {/* PAGE HEADER */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#ef4444' }}>↑</span> Quản Lý Phiếu Xuất Kho
          </h1>
          <p>
            Quản lý lệnh xuất bán hàng, kiểm tra tồn kho khả dụng và trừ tồn kho tự động khi thực xuất
          </p>
        </div>
        <div className="page-actions">
          <button className="btn btn-danger" onClick={openCreateModal}>
            + Xuất kho
          </button>
          <button
            className="btn btn-outline"
            onClick={() => notifySuccess('Đã xuất danh sách Phiếu xuất kho ra Excel!')}
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
            value={filterCustomerId}
            onChange={setFilterCustomerId}
            options={[
              { value: '', label: 'Tất cả khách hàng...' },
              ...customers.map((c) => ({
                value: c.id,
                label: c.customer_name,
                subLabel: c.customer_code,
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
              { value: '4', label: 'Hoàn thành' },
              { value: '3', label: 'Từ chối' },
              { value: '5', label: 'Đã hủy' },
            ]}
          />

          <div className="search-box-wrapper">
            <span className="search-box-icon">🔍</span>
            <input
              className="search-box-input"
              placeholder="Tìm số phiếu xuất, khách hàng, diễn giải..."
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
                <th>NGÀY XUẤT</th>
                <th>KHO XUẤT</th>
                <th>KHÁCH HÀNG / ĐỐI TƯỢNG</th>
                <th>DIỄN GIẢI</th>
                <th>NGƯỜI LẬP</th>
                <th style={{ textAlign: 'right' }}>TỔNG SỐ LƯỢNG</th>
                <th style={{ textAlign: 'right' }}>TỔNG GIÁ TRỊ</th>
                <th>TRẠNG THÁI</th>
                <th style={{ textAlign: 'center' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {filteredOrders.map((so, idx) => (
                <tr key={so.id}>
                  <td>{idx + 1}</td>
                  <td>
                    <span className="voucher-code">{so.order_code}</span>
                  </td>
                  <td>{new Date(so.order_date).toLocaleDateString('vi-VN')}</td>
                  <td style={{ fontWeight: 600 }}>
                    {so.warehouse_name.toUpperCase()}
                  </td>
                  <td>{so.customer_name}</td>
                  <td style={{ color: '#475569' }}>
                    {so.note || 'Xuất kho bán hàng'}
                  </td>
                  <td>Quản trị viên</td>
                  <td style={{ textAlign: 'right', fontWeight: 700 }}>
                    {so.total_quantity}
                  </td>
                  <td style={{ textAlign: 'right' }} className="text-danger-bold">
                    {so.total_amount.toLocaleString('vi-VN')} ₫
                  </td>
                  <td>
                    <span
                      className={`status-pill ${
                        so.status === 4 || so.status === 2
                          ? 'emerald'
                          : so.status === 3 || so.status === 5
                          ? 'danger'
                          : 'amber'
                      }`}
                    >
                      {so.status_name}
                    </span>
                  </td>
                  <td style={{ textAlign: 'center', whiteSpace: 'nowrap' }}>
                    <button
                      className="icon-action-btn"
                      title="In phiếu xuất"
                      onClick={() => window.print()}
                    >
                      🖨️
                    </button>
                    {so.status === 1 && (
                      <>
                        <button
                          className="btn btn-emerald btn-sm"
                          style={{ marginLeft: 4 }}
                          onClick={() => handleApprove(so.id)}
                        >
                          ✓ Duyệt
                        </button>
                        <button
                          className="btn btn-danger btn-sm"
                          style={{ marginLeft: 4 }}
                          onClick={() => handleReject(so.id)}
                        >
                          ✕
                        </button>
                      </>
                    )}
                    {so.status === 2 && (
                      <button
                        className="btn btn-danger btn-sm"
                        style={{ marginLeft: 4 }}
                        onClick={() => handleIssueGoods(so.id)}
                      >
                        📤 Thực xuất kho
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {filteredOrders.length === 0 && (
                <tr>
                  <td colSpan={11} style={{ textAlign: 'center', padding: 24, color: '#64748b' }}>
                    Chưa có phiếu xuất kho nào trong kỳ.
                  </td>
                </tr>
              )}
            </tbody>
            <tfoot>
              <tr>
                <td colSpan={7}>TỔNG CỘNG:</td>
                <td style={{ textAlign: 'right' }} className="text-danger-bold">
                  {sumQty.toLocaleString('vi-VN')}
                </td>
                <td style={{ textAlign: 'right' }} className="text-danger-bold">
                  {sumAmount.toLocaleString('vi-VN')} ₫
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
            <button className="page-btn active">1</button>
          </div>
        </div>
      </div>

      {/* MODAL LẬP PHIẾU XUẤT KHO */}
      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="modal-dialog lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">
                <span style={{ color: '#ef4444' }}>📤</span> Lập Phiếu Xuất Kho Mới
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setShowModal(false)}
              >
                ✕
              </button>
            </div>
            <div className="modal-body">
              <div className="form-grid-4">
                <div className="form-group">
                  <label>Số phiếu xuất *</label>
                  <input
                    className="form-control"
                    value={`SO-${new Date().getFullYear()}-${String(
                      orders.length + 1
                    ).padStart(4, '0')}`}
                    readOnly
                  />
                </div>
                <div className="form-group">
                  <label>Ngày xuất *</label>
                  <input
                    type="date"
                    className="form-control"
                    value={voucherDate}
                    onChange={(e) => setVoucherDate(e.target.value)}
                  />
                </div>
                <div className="form-group">
                  <label>Kho xuất *</label>
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
                    Khách hàng / Đối tượng nhận <span className="req">*</span>
                  </label>
                  <CustomSelect
                    variant="form"
                    value={customerId}
                    onChange={setCustomerId}
                    options={customers.map((c) => ({
                      value: c.id,
                      label: `${c.customer_name} (${c.customer_code})`,
                      subLabel: c.phone,
                    }))}
                  />
                </div>
                <div className="form-group">
                  <label>
                    Diễn giải / Lý do xuất kho <span className="req">*</span>
                  </label>
                  <input
                    className="form-control"
                    value={voucherNote}
                    onChange={(e) => setVoucherNote(e.target.value)}
                  />
                </div>
              </div>

              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  margin: '16px 0 10px',
                }}
              >
                <div style={{ fontWeight: 700, fontSize: 13.5 }}>
                  ☰ Chi Tiết Hàng Hóa Xuất Kho
                </div>
                <button
                  type="button"
                  className="btn btn-outline btn-sm"
                  onClick={handleAddLine}
                >
                  + Thêm Dòng
                </button>
              </div>

              <div className="table-wrapper">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th style={{ width: 45 }}>STT</th>
                      <th>Mặt hàng (Kèm tồn kho hiện tại)</th>
                      <th style={{ width: 130 }}>Số lượng xuất</th>
                      <th style={{ width: 160 }}>Đơn giá xuất</th>
                      <th style={{ width: 160, textAlign: 'right' }}>Thành tiền</th>
                      <th style={{ width: 180 }}>Ghi chú</th>
                      <th style={{ width: 55, textAlign: 'center' }}>Xóa</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lines.map((line, idx) => {
                      const lineTotal =
                        Number(line.quantity || 0) * Number(line.unit_price || 0);
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
                                subLabel: `Tồn kho: ${p.current_warehouse_stock} • Đơn giá: ${p.unit_price.toLocaleString('vi-VN')} ₫`,
                              }))}
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              min={1}
                              className="form-control"
                              style={{ width: '100%' }}
                              value={line.quantity}
                              onChange={(e) => {
                                const next = [...lines];
                                next[idx].quantity = Number(e.target.value);
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
                            className="text-danger-bold"
                          >
                            {lineTotal.toLocaleString('vi-VN')} ₫
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
                              🗑️
                            </button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>

              <div className="voucher-summary-box">
                <div>
                  <div style={{ fontSize: 11.5, color: '#64748b' }}>
                    Tổng số lượng xuất:
                  </div>
                  <div style={{ fontSize: 18, fontWeight: 800, color: '#dc2626' }}>
                    {modalTotalQty.toLocaleString('vi-VN')}
                  </div>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <div style={{ fontSize: 11.5, color: '#64748b' }}>
                    Tổng giá trị phiếu xuất:
                  </div>
                  <div style={{ fontSize: 18, fontWeight: 800, color: '#dc2626' }}>
                    {modalTotalValue.toLocaleString('vi-VN')} ₫
                  </div>
                </div>
              </div>
            </div>

            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-outline"
                onClick={() => setShowModal(false)}
              >
                Hủy
              </button>
              <button
                type="button"
                className="btn btn-outline"
                onClick={() => handleSaveOrder(false)}
              >
                🔖 Lưu Bản Nháp
              </button>
              <button
                type="button"
                className="btn btn-danger"
                onClick={() => handleSaveOrder(true)}
              >
                ✓ Lưu & Duyệt Phiếu Xuất
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
