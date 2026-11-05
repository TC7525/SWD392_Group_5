import React, { useCallback, useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { useAuth } from '../../contexts/AuthContext';
import { masterDataService } from '../../services/masterDataService';
import { stockTransferService } from '../../services/orderService';
import { type ProductItem, type WarehouseItem } from '../../types/master.types';
import { type StockTransferItem } from '../../types/order.types';

export const StockTransferPage: React.FC = () => {
  const { user, notifyError, notifySuccess } = useAuth();
  const [transfers, setTransfers] = useState<StockTransferItem[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseItem[]>([]);
  const [products, setProducts] = useState<ProductItem[]>([]);
  const [keyword, setKeyword] = useState('');

  // Modal Lập Phiếu Chuyển Kho
  const [showModal, setShowModal] = useState(false);
  const [destWarehouseId, setDestWarehouseId] = useState('');
  const [productId, setProductId] = useState('');
  const [quantity, setQuantity] = useState(2);
  const [note, setNote] = useState('Điều chuyển cân đối tồn kho liên chi nhánh');

  const loadTransfers = useCallback(() => {
    stockTransferService
      .search()
      .then((res) => setTransfers(res.items || []))
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải Phiếu chuyển kho')
      );
  }, [notifyError]);

  useEffect(() => {
    if (!user) return;
    loadTransfers();
    masterDataService
      .searchWarehouses()
      .then((res) => {
        const list = res.items || [];
        setWarehouses(list);
        const other = list.find((w) => w.id !== user.current_warehouse_id);
        if (other) setDestWarehouseId(other.id);
      })
      .catch(() => {});
    masterDataService
      .searchProducts(user.current_warehouse_id)
      .then((res) => {
        const list = res.items || [];
        setProducts(list);
        if (list.length > 0) setProductId(list[0].id);
      })
      .catch(() => {});
  }, [user, loadTransfers]);

  const handleCreateTransfer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !destWarehouseId || !productId) return;
    try {
      await stockTransferService.create({
        source_warehouse_id: user.current_warehouse_id,
        destination_warehouse_id: destWarehouseId,
        note,
        items: [{ product_id: productId, quantity: Number(quantity) }],
      });
      notifySuccess('Đã lập Phiếu Chuyển Kho liên chi nhánh thành công!');
      setShowModal(false);
      loadTransfers();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi lập phiếu chuyển kho');
    }
  };

  const handleAction = async (
    action: 'approve' | 'dispatch' | 'receive',
    id: string
  ) => {
    try {
      if (action === 'approve') {
        await stockTransferService.approve(id);
        notifySuccess('Đã phê duyệt Phiếu Chuyển Kho!');
      } else if (action === 'dispatch') {
        await stockTransferService.dispatch(id, 'Xuất điều chuyển khỏi kho nguồn');
        notifySuccess('Đã Xuất Điều Chuyển (trừ tồn kho nguồn)!');
      } else {
        await stockTransferService.receive(id, 'Nhận điều chuyển tại kho đích');
        notifySuccess('Đã Nhận Điều Chuyển (cộng tồn kho đích)!');
      }
      loadTransfers();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi thao tác phiếu chuyển kho');
    }
  };

  const filtered = transfers.filter(
    (tf) =>
      !keyword ||
      tf.stock_transfer_code.toLowerCase().includes(keyword.toLowerCase()) ||
      tf.source_warehouse_name.toLowerCase().includes(keyword.toLowerCase()) ||
      tf.destination_warehouse_name.toLowerCase().includes(keyword.toLowerCase())
  );

  return (
    <div>
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#f59e0b' }}>⇄</span> Quản Lý Phiếu Chuyển Kho Liên Chi Nhánh
          </h1>
          <p>
            Điều chuyển hàng hóa nội bộ giữa các chi nhánh kho (Duyệt lệnh → Xuất kho nguồn → Nhập kho đích)
          </p>
        </div>
        <div className="page-actions">
          <button className="btn btn-warning" onClick={() => setShowModal(true)}>
            + Chuyển kho
          </button>
          <button
            className="btn btn-outline"
            onClick={() => notifySuccess('Đã xuất danh sách Phiếu chuyển kho ra Excel!')}
          >
            📗 Excel
          </button>
        </div>
      </div>

      <div className="surface-card">
        <div className="filter-bar">
          <div className="search-box-wrapper">
            <span className="search-box-icon">🔍</span>
            <input
              className="search-box-input"
              placeholder="Tìm số phiếu chuyển, kho nguồn, kho đích..."
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
                <th>NGÀY LẬP</th>
                <th>KHO XUẤT (NGUỒN)</th>
                <th>KHO NHẬP (ĐÍCH)</th>
                <th>DIỄN GIẢI</th>
                <th style={{ textAlign: 'right' }}>TỔNG SỐ LƯỢNG</th>
                <th>TRẠNG THÁI</th>
                <th style={{ textAlign: 'center' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((tf, idx) => (
                <tr key={tf.id}>
                  <td>{idx + 1}</td>
                  <td className="voucher-code">{tf.stock_transfer_code}</td>
                  <td>{new Date(tf.transfer_date).toLocaleDateString('vi-VN')}</td>
                  <td style={{ fontWeight: 600 }}>
                    {tf.source_warehouse_name.toUpperCase()}
                  </td>
                  <td style={{ fontWeight: 600 }}>
                    {tf.destination_warehouse_name.toUpperCase()}
                  </td>
                  <td>{tf.note || 'Điều chuyển nội bộ'}</td>
                  <td style={{ textAlign: 'right', fontWeight: 700 }}>
                    {tf.total_quantity}
                  </td>
                  <td>
                    <span
                      className={`status-pill ${
                        tf.status === 5
                          ? 'emerald'
                          : tf.status === 3 || tf.status === 6
                          ? 'danger'
                          : 'amber'
                      }`}
                    >
                      {tf.status_name}
                    </span>
                  </td>
                  <td style={{ textAlign: 'center' }}>
                    {tf.status === 1 && (
                      <button
                        className="btn btn-emerald btn-sm"
                        onClick={() => handleAction('approve', tf.id)}
                      >
                        ✓ Duyệt lệnh
                      </button>
                    )}
                    {tf.status === 2 && (
                      <button
                        className="btn btn-warning btn-sm"
                        onClick={() => handleAction('dispatch', tf.id)}
                      >
                        🚚 Xuất kho nguồn
                      </button>
                    )}
                    {tf.status === 4 && (
                      <button
                        className="btn btn-emerald btn-sm"
                        onClick={() => handleAction('receive', tf.id)}
                      >
                        📥 Nhận tại kho đích
                      </button>
                    )}
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && (
                <tr>
                  <td colSpan={9} style={{ textAlign: 'center', padding: 24, color: '#64748b' }}>
                    Chưa có phiếu chuyển kho nào.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="modal-dialog md" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">⇄ Lập Phiếu Chuyển Kho Nội Bộ</div>
              <button
                className="modal-close-btn"
                onClick={() => setShowModal(false)}
              >
                ✕
              </button>
            </div>
            <form onSubmit={handleCreateTransfer}>
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label>Kho xuất (Nguồn)</label>
                    <input
                      className="form-control"
                      value={user?.current_warehouse_name.toUpperCase() || ''}
                      readOnly
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Kho nhập (Đích) <span className="req">*</span>
                    </label>
                    <CustomSelect
                      variant="form"
                      value={destWarehouseId}
                      onChange={setDestWarehouseId}
                      options={warehouses
                        .filter((w) => w.id !== user?.current_warehouse_id)
                        .map((w) => ({
                          value: w.id,
                          label: `${w.warehouse_name.toUpperCase()} (${w.warehouse_code})`,
                          subLabel: w.address,
                        }))}
                    />
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label>
                      Mặt hàng điều chuyển <span className="req">*</span>
                    </label>
                    <CustomSelect
                      variant="form"
                      value={productId}
                      onChange={setProductId}
                      options={products.map((p) => ({
                        value: p.id,
                        label: `${p.product_code} - ${p.product_name}`,
                        subLabel: `Tồn kho hiện tại: ${p.current_warehouse_stock}`,
                      }))}
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Số lượng chuyển <span className="req">*</span>
                    </label>
                    <input
                      type="number"
                      min={1}
                      className="form-control"
                      value={quantity}
                      onChange={(e) => setQuantity(Number(e.target.value))}
                      required
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Diễn giải / Ghi chú điều chuyển</label>
                  <input
                    className="form-control"
                    value={note}
                    onChange={(e) => setNote(e.target.value)}
                  />
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
                <button type="submit" className="btn btn-warning">
                  ⇄ Tạo Phiếu Chuyển Kho
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
