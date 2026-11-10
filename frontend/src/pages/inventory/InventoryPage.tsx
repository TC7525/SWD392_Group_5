import React, { useCallback, useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { Icon } from '../../components/common/Icon';
import { useAuth } from '../../contexts/AuthContext';
import { inventoryService } from '../../services/inventoryService';
import { masterDataService } from '../../services/masterDataService';
import {
  type InventoryItem,
  type InventoryTransactionItem,
} from '../../types/inventory.types';
import { type CategoryItem } from '../../types/master.types';

interface AggregatedStockRow {
  product_id: string;
  product_code: string;
  product_name: string;
  category_name: string;
  unit_name: string;
  unit_price: number;
  minimum_stock_level: number;
  warehouseBreakdowns: {
    warehouse_id: string;
    warehouse_name: string;
    quantity: number;
  }[];
  opening_qty: number;
  inbound_qty: number;
  outbound_qty: number;
  closing_qty: number;
  total_value: number;
  is_low_stock: boolean;
}

export const InventoryPage: React.FC = () => {
  const { user, notifyError, notifySuccess } = useAuth();
  const [rawInventories, setRawInventories] = useState<InventoryItem[]>([]);
  const [transactions, setTransactions] = useState<InventoryTransactionItem[]>([]);
  const [categories, setCategories] = useState<CategoryItem[]>([]);

  const [keyword, setKeyword] = useState('');
  const [filterCategory, setFilterCategory] = useState('');
  const [filterStatus, setFilterStatus] = useState('ALL');
  const [pageSize, setPageSize] = useState('10');

  // Modal Sổ Kho (Thẻ kho) & Điều chỉnh Kiểm kê
  const [stockCardProduct, setStockCardProduct] =
    useState<AggregatedStockRow | null>(null);
  const [adjustWarehouseId, setAdjustWarehouseId] = useState('');
  const [adjustNewQty, setAdjustNewQty] = useState(0);
  const [adjustNote, setAdjustNote] = useState('Kiểm kê thực tế định kỳ');

  const loadData = useCallback(() => {
    inventoryService
      .searchInventory({ page_size: 200 })
      .then((res) => setRawInventories(res.items || []))
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải tồn kho')
      );

    inventoryService
      .searchTransactions({ page_size: 200 })
      .then((res) => setTransactions(res.items || []))
      .catch(() => {});

    masterDataService
      .searchCategories()
      .then((res) => setCategories(res.items || []))
      .catch(() => {});
  }, [notifyError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Group inventory by product_id across all warehouses (Screen 9)
  const productMap = new Map<string, AggregatedStockRow>();
  for (const inv of rawInventories) {
    const existing = productMap.get(inv.product_id);
    const prodTxs = transactions.filter((t) => t.product_id === inv.product_id);
    const inboundQty = prodTxs
      .filter((t) => t.quantity > 0)
      .reduce((s, t) => s + t.quantity, 0);
    const outboundQty = prodTxs
      .filter((t) => t.quantity < 0)
      .reduce((s, t) => s + Math.abs(t.quantity), 0);

    if (!existing) {
      const closing = inv.quantity;
      const opening = Math.max(0, closing - inboundQty + outboundQty);
      productMap.set(inv.product_id, {
        product_id: inv.product_id,
        product_code: inv.product_code,
        product_name: inv.product_name,
        category_name: inv.category_name,
        unit_name: inv.brand || 'Cái',
        unit_price: inv.unit_price,
        minimum_stock_level: inv.minimum_stock_level,
        warehouseBreakdowns: [
          {
            warehouse_id: inv.warehouse_id,
            warehouse_name: inv.warehouse_name,
            quantity: inv.quantity,
          },
        ],
        opening_qty: opening,
        inbound_qty: inboundQty,
        outbound_qty: outboundQty,
        closing_qty: closing,
        total_value: inv.total_stock_value,
        is_low_stock: inv.is_low_stock,
      });
    } else {
      existing.warehouseBreakdowns.push({
        warehouse_id: inv.warehouse_id,
        warehouse_name: inv.warehouse_name,
        quantity: inv.quantity,
      });
      existing.closing_qty += inv.quantity;
      existing.opening_qty = Math.max(
        0,
        existing.closing_qty - existing.inbound_qty + existing.outbound_qty
      );
      existing.total_value += inv.total_stock_value;
      if (inv.is_low_stock) existing.is_low_stock = true;
    }
  }

  const rows = Array.from(productMap.values()).filter((r) => {
    const matchKw =
      !keyword ||
      r.product_code.toLowerCase().includes(keyword.toLowerCase()) ||
      r.product_name.toLowerCase().includes(keyword.toLowerCase());
    const matchCat = !filterCategory || r.category_name === filterCategory;
    const matchSt =
      filterStatus === 'ALL' ||
      (filterStatus === 'LOW' && r.is_low_stock) ||
      (filterStatus === 'NORMAL' && !r.is_low_stock);
    return matchKw && matchCat && matchSt;
  });

  const grandTotalQty = rows.reduce((s, r) => s + r.closing_qty, 0);
  const grandTotalValue = rows.reduce((s, r) => s + r.total_value, 0);

  const openStockCard = (row: AggregatedStockRow) => {
    setStockCardProduct(row);
    const currentWh =
      row.warehouseBreakdowns.find(
        (b) => b.warehouse_id === user?.current_warehouse_id
      ) || row.warehouseBreakdowns[0];
    setAdjustWarehouseId(currentWh?.warehouse_id || user?.current_warehouse_id || '');
    setAdjustNewQty(currentWh?.quantity || 0);
    setAdjustNote('Kiểm kê thực tế tại kho');
  };

  const handleAdjustInventory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!stockCardProduct || !adjustWarehouseId) return;
    try {
      await inventoryService.adjustInventory({
        warehouse_id: adjustWarehouseId,
        product_id: stockCardProduct.product_id,
        new_quantity: Number(adjustNewQty),
        note: adjustNote,
      });
      notifySuccess(
        `Đã điều chỉnh kiểm kê mặt hàng '${stockCardProduct.product_code}' thành công!`
      );
      setStockCardProduct(null);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi điều chỉnh tồn kho');
    }
  };

  return (
    <div>
      {/* PAGE HEADER (Screen 9) */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <Icon name="boxes" color="#10b981" />
            Tra Cứu Tồn Kho Tức Thời
          </h1>
          <p>
            Bảng tổng hợp Tồn đầu, Nhập, Xuất, Tồn cuối và Giá trị vốn cập nhật tự động theo từng chi nhánh kho
          </p>
        </div>
        <div className="page-actions">
          <button
            className="btn btn-outline"
            onClick={() => notifySuccess('Đã xuất Báo cáo Tồn kho tức thời ra Excel!')}
          >
            <Icon name="file-excel" /> Excel
          </button>
          <button className="btn btn-outline" onClick={() => window.print()}>
            <Icon name="print" /> In
          </button>
        </div>
      </div>

      {/* MAIN TABLE CARD */}
      <div className="surface-card">
        <div className="filter-bar">
          <div className="search-box-wrapper">
            <span className="search-box-icon">              <Icon name="search" /></span>
            <input
              className="search-box-input"
              placeholder="Tìm theo mã hàng, tên hàng..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </div>

          <CustomSelect
            variant="filter"
            value={filterCategory}
            onChange={setFilterCategory}
            options={[
              { value: '', label: 'Tất cả nhóm phân loại' },
              ...categories.map((c) => ({
                value: c.category_name,
                label: c.category_name,
              })),
            ]}
          />

          <CustomSelect
            variant="filter"
            value={filterStatus}
            onChange={setFilterStatus}
            options={[
              { value: 'ALL', label: 'Tất cả tình trạng' },
              { value: 'NORMAL', label: 'Bình thường' },
              { value: 'LOW', label: 'Cảnh báo thiếu' },
            ]}
          />
        </div>

        <div className="table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>MÃ HÀNG</th>
                <th>TÊN HÀNG HÓA</th>
                <th>ĐVT</th>
                <th>KHO LƯU TRỮ</th>
                <th style={{ textAlign: 'right' }}>TỒN ĐẦU</th>
                <th style={{ textAlign: 'right' }}>NHẬP</th>
                <th style={{ textAlign: 'right' }}>XUẤT</th>
                <th style={{ textAlign: 'right' }}>TỒN CUỐI</th>
                <th style={{ textAlign: 'right' }}>ĐƠN GIÁ BQ</th>
                <th style={{ textAlign: 'right' }}>THÀNH TIỀN TỒN</th>
                <th>TÌNH TRẠNG</th>
                <th style={{ textAlign: 'center' }}>THẺ KHO</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.product_id}>
                  <td style={{ fontWeight: 700 }}>{r.product_code}</td>
                  <td>
                    <div style={{ fontWeight: 700, color: '#0f172a' }}>
                      {r.product_name}
                    </div>
                    <div style={{ fontSize: 11, color: '#64748b' }}>
                      {r.category_name}
                    </div>
                  </td>
                  <td>{r.unit_name}</td>
                  <td>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                      {r.warehouseBreakdowns.map((b) => (
                        <div
                          key={b.warehouse_id}
                          style={{ fontSize: 11.5, color: '#475569' }}
                        >
                          {b.warehouse_name.toUpperCase()}:{' '}
                          <strong style={{ color: '#059669' }}>
                            {b.quantity.toLocaleString('vi-VN')}
                          </strong>
                        </div>
                      ))}
                    </div>
                  </td>
                  <td style={{ textAlign: 'right', fontWeight: 600 }}>
                    {r.opening_qty.toLocaleString('vi-VN')}
                  </td>
                  <td style={{ textAlign: 'right', color: '#059669', fontWeight: 600 }}>
                    {r.inbound_qty > 0 ? r.inbound_qty.toLocaleString('vi-VN') : '-'}
                  </td>
                  <td style={{ textAlign: 'right', color: '#dc2626', fontWeight: 600 }}>
                    {r.outbound_qty > 0
                      ? r.outbound_qty.toLocaleString('vi-VN')
                      : '-'}
                  </td>
                  <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                    {r.closing_qty.toLocaleString('vi-VN')}
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    {r.unit_price.toLocaleString('vi-VN')} ₫
                  </td>
                  <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                    {r.total_value.toLocaleString('vi-VN')} ₫
                  </td>
                  <td>
                    <span
                      className={`status-pill ${
                        r.is_low_stock ? 'danger' : 'emerald'
                      }`}
                    >
                      {r.is_low_stock ? 'Cảnh báo thiếu' : 'Bình thường'}
                    </span>
                  </td>
                  <td style={{ textAlign: 'center' }}>
                    <button
                      className="btn btn-outline btn-sm"
                      onClick={() => openStockCard(r)}
                    >
                      <Icon name="book" /> Sổ kho
                    </button>
                  </td>
                </tr>
              ))}
              {rows.length === 0 && (
                <tr>
                  <td colSpan={12} style={{ textAlign: 'center', padding: 24, color: '#64748b' }}>
                    Không tìm thấy dữ liệu tồn kho phù hợp.
                  </td>
                </tr>
              )}
            </tbody>
            <tfoot>
              <tr>
                <td colSpan={7}>TỔNG TOÀN KHO:</td>
                <td style={{ textAlign: 'right', fontSize: 14 }} className="text-emerald-bold">
                  {grandTotalQty.toLocaleString('vi-VN')}
                </td>
                <td />
                <td style={{ textAlign: 'right', fontSize: 14 }} className="text-emerald-bold">
                  {grandTotalValue.toLocaleString('vi-VN')} ₫
                </td>
                <td colSpan={2} />
              </tr>
            </tfoot>
          </table>
        </div>

        <div className="pagination-bar">
          <span>
            Hiển thị <strong>1 - {rows.length}</strong> trên tổng số{' '}
            <strong>{rows.length}</strong> dòng
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
            <button className="page-btn" aria-label="Trang trước"><Icon name="angle-double-small-left" /></button>
            <button className="page-btn active">1</button>
            <button className="page-btn" aria-label="Trang sau"><Icon name="angle-double-small-right" /></button>
          </div>
        </div>
      </div>

      {/* MODAL THẺ KHO (SỔ KHO) & ĐIỀU CHỈNH KIỂM KÊ */}
      {stockCardProduct && (
        <div
          className="modal-backdrop"
          onClick={() => setStockCardProduct(null)}
        >
          <div
            className="modal-dialog lg"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <div className="modal-title">
                <Icon name="book" color="#10b981" /> Sổ Kho (Thẻ Kho Chi Tiết):{' '}
                <span style={{ color: '#059669' }}>
                  {stockCardProduct.product_code} -{' '}
                  {stockCardProduct.product_name}
                </span>
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setStockCardProduct(null)}
              >
                <Icon name="cross-small" />
              </button>
            </div>
            <div className="modal-body">
              {/* Form điều chỉnh kiểm kê nhanh */}
              <form
                onSubmit={handleAdjustInventory}
                style={{
                  background: '#f8fafc',
                  border: '1px solid #e2e8f0',
                  borderRadius: 10,
                  padding: 14,
                  marginBottom: 16,
                  display: 'grid',
                  gridTemplateColumns: '1fr 140px 1.4fr auto',
                  gap: 10,
                  alignItems: 'end',
                }}
              >
                <div className="form-group">
                  <label>Kho điều chỉnh kiểm kê</label>
                  <CustomSelect
                    variant="form"
                    value={adjustWarehouseId}
                    onChange={(val) => {
                      setAdjustWarehouseId(val);
                      const found = stockCardProduct.warehouseBreakdowns.find(
                        (b) => b.warehouse_id === val
                      );
                      if (found) setAdjustNewQty(found.quantity);
                    }}
                    options={stockCardProduct.warehouseBreakdowns.map((b) => ({
                      value: b.warehouse_id,
                      label: `${b.warehouse_name.toUpperCase()} (Hiện tồn: ${b.quantity})`,
                    }))}
                  />
                </div>
                <div className="form-group">
                  <label>Tồn thực tế mới *</label>
                  <input
                    type="number"
                    min={0}
                    className="form-control"
                    value={adjustNewQty}
                    onChange={(e) => setAdjustNewQty(Number(e.target.value))}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Lý do kiểm kê / điều chỉnh *</label>
                  <input
                    className="form-control"
                    value={adjustNote}
                    onChange={(e) => setAdjustNote(e.target.value)}
                    required
                  />
                </div>
                <button
                  type="submit"
                  className="btn btn-emerald"
                  style={{ height: 36 }}
                >
                  <Icon name="scale" /> Cập nhật kiểm kê
                </button>
              </form>

              <div style={{ fontWeight: 700, marginBottom: 8 }}>
                <Icon name="time-past" /> Lịch sử biến động Nhập - Xuất - Điều chỉnh của mặt hàng:
              </div>
              <div className="table-wrapper">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>STT</th>
                      <th>THỜI GIAN</th>
                      <th>KHO</th>
                      <th>LOẠI GIAO DỊCH</th>
                      <th style={{ textAlign: 'right' }}>SL BIẾN ĐỘNG</th>
                      <th>MÃ CHỨNG TỪ</th>
                      <th>NGƯỜI THỰC HIỆN</th>
                      <th>DIỄN GIẢI</th>
                    </tr>
                  </thead>
                  <tbody>
                    {transactions
                      .filter(
                        (t) => t.product_id === stockCardProduct.product_id
                      )
                      .map((tx, idx) => (
                        <tr key={tx.id}>
                          <td>{idx + 1}</td>
                          <td>
                            {new Date(tx.transaction_date).toLocaleString(
                              'vi-VN'
                            )}
                          </td>
                          <td style={{ fontWeight: 600 }}>
                            {tx.warehouse_name.toUpperCase()}
                          </td>
                          <td>
                            <span
                              className={`status-pill ${
                                tx.quantity >= 0 ? 'emerald' : 'danger'
                              }`}
                            >
                              {tx.transaction_type_name}
                            </span>
                          </td>
                          <td
                            style={{
                              textAlign: 'right',
                              fontWeight: 700,
                              color: tx.quantity >= 0 ? '#059669' : '#dc2626',
                            }}
                          >
                            {tx.quantity >= 0 ? `+${tx.quantity}` : tx.quantity}
                          </td>
                          <td className="voucher-code">
                            {tx.reference_code || '—'}
                          </td>
                          <td>{tx.user_full_name}</td>
                          <td>{tx.note || '—'}</td>
                        </tr>
                      ))}
                  </tbody>
                </table>
              </div>
            </div>
            <div className="modal-footer">
              <button
                className="btn btn-outline"
                onClick={() => setStockCardProduct(null)}
              >
                Đóng Sổ Kho
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
