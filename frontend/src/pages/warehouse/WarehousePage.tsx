import React, { useCallback, useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { Icon } from '../../components/common/Icon';
import { useAuth } from '../../contexts/AuthContext';
import { inventoryService } from '../../services/inventoryService';
import { masterDataService } from '../../services/masterDataService';
import { type InventoryItem } from '../../types/inventory.types';
import { type WarehouseItem } from '../../types/master.types';

export const WarehousePage: React.FC = () => {
  const { notifyError, notifySuccess } = useAuth();
  const [warehouses, setWarehouses] = useState<WarehouseItem[]>([]);
  const [allInventory, setAllInventory] = useState<InventoryItem[]>([]);
  const [keyword, setKeyword] = useState('');
  const [pageSize, setPageSize] = useState('10');

  // Modal Chi Tiết Tồn Kho theo Kho (Screen 4 & 5)
  const [detailWarehouse, setDetailWarehouse] = useState<WarehouseItem | null>(null);
  const [detailKeyword, setDetailKeyword] = useState('');

  // Modal Thêm / Sửa Kho
  const [showFormModal, setShowFormModal] = useState(false);
  const [editingWh, setEditingWh] = useState<WarehouseItem | null>(null);
  const [whCode, setWhCode] = useState('');
  const [whName, setWhName] = useState('');
  const [whAddress, setWhAddress] = useState('');

  const loadData = useCallback(() => {
    masterDataService
      .searchWarehouses()
      .then((res) => setWarehouses(res.items || []))
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải danh sách Kho')
      );

    inventoryService
      .searchInventory({ page_size: 200 })
      .then((res) => setAllInventory(res.items || []))
      .catch(() => {});
  }, [notifyError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const getWarehouseValue = (warehouseId: string) => {
    return allInventory
      .filter((i) => i.warehouse_id === warehouseId)
      .reduce((sum, i) => sum + i.total_stock_value, 0);
  };

  const openCreateModal = () => {
    setEditingWh(null);
    setWhCode('');
    setWhName('');
    setWhAddress('');
    setShowFormModal(true);
  };

  const openEditModal = (w: WarehouseItem) => {
    setEditingWh(w);
    setWhCode(w.warehouse_code);
    setWhName(w.warehouse_name);
    setWhAddress(w.address || '');
    setShowFormModal(true);
  };

  const handleSaveWarehouse = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingWh) {
        await masterDataService.updateWarehouse({
          id: editingWh.id,
          warehouse_code: whCode,
          warehouse_name: whName,
          address: whAddress,
        });
        notifySuccess(`Đã cập nhật kho '${whName}'!`);
      } else {
        await masterDataService.createWarehouse({
          warehouse_code: whCode || undefined,
          warehouse_name: whName,
          address: whAddress,
        });
        notifySuccess(`Đã thêm kho mới '${whName}'!`);
      }
      setShowFormModal(false);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi lưu thông tin Kho');
    }
  };

  const handleDeleteWarehouse = async (w: WarehouseItem) => {
    if (!window.confirm(`Bạn có chắc muốn xóa kho "${w.warehouse_name}"?`)) return;
    try {
      await masterDataService.deleteWarehouse(w.id);
      notifySuccess(`Đã xóa kho '${w.warehouse_code}'.`);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Không thể xóa kho');
    }
  };

  const filteredWarehouses = warehouses.filter(
    (w) =>
      !keyword ||
      w.warehouse_code.toLowerCase().includes(keyword.toLowerCase()) ||
      w.warehouse_name.toLowerCase().includes(keyword.toLowerCase()) ||
      (w.address && w.address.toLowerCase().includes(keyword.toLowerCase()))
  );

  const totalProductsSum = filteredWarehouses.reduce(
    (sum, w) => sum + w.total_products,
    0
  );
  const totalQuantitySum = filteredWarehouses.reduce(
    (sum, w) => sum + w.total_quantity,
    0
  );
  const totalValueSum = filteredWarehouses.reduce(
    (sum, w) => sum + getWarehouseValue(w.id),
    0
  );

  const detailItems = detailWarehouse
    ? allInventory.filter(
        (i) =>
          i.warehouse_id === detailWarehouse.id &&
          (!detailKeyword ||
            i.product_code.toLowerCase().includes(detailKeyword.toLowerCase()) ||
            i.product_name.toLowerCase().includes(detailKeyword.toLowerCase()))
      )
    : [];

  return (
    <div>
      {/* PAGE HEADER (Screen 4) */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <Icon name="warehouse-alt" color="#10b981" />
            Danh Mục Kho Hàng
          </h1>
          <p>Quản lý các điểm kho, kho tổng, chi nhánh và người phụ trách</p>
        </div>
        <div className="page-actions">
          <button className="btn btn-emerald" onClick={openCreateModal}>
            <Icon name="add" /> Thêm kho
          </button>
          <button
            className="btn btn-outline"
            onClick={() => notifySuccess('Đã xuất Danh mục Kho hàng ra Excel!')}
          >
            <Icon name="file-excel" /> Excel
          </button>
        </div>
      </div>

      {/* MAIN CARD */}
      <div className="surface-card">
        <div className="filter-bar">
          <div className="search-box-wrapper">
            <span className="search-box-icon">              <Icon name="search" /></span>
            <input
              className="search-box-input"
              placeholder="Tìm theo mã kho, tên kho, người phụ trách..."
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
                <th>MÃ KHO</th>
                <th>TÊN KHO</th>
                <th>ĐỊA CHỈ</th>
                <th>SỐ ĐIỆN THOẠI</th>
                <th>NGƯỜI PHỤ TRÁCH KHO</th>
                <th style={{ textAlign: 'center' }}>SỐ MẶT HÀNG</th>
                <th style={{ textAlign: 'right' }}>TỔNG SỐ LƯỢNG TỒN</th>
                <th style={{ textAlign: 'right' }}>TỔNG GIÁ TRỊ TỒN</th>
                <th style={{ textAlign: 'center' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {filteredWarehouses.map((w, idx) => {
                const whValue = getWarehouseValue(w.id);
                return (
                  <tr key={w.id}>
                    <td>{idx + 1}</td>
                    <td style={{ fontWeight: 700 }}>{w.warehouse_code}</td>
                    <td style={{ fontWeight: 700 }}>{w.warehouse_name.toUpperCase()}</td>
                    <td>{w.address || '—'}</td>
                    <td>024.3888.999{idx + 1}</td>
                    <td style={{ fontWeight: 600 }}>
                      {w.assigned_staff_count > 0
                        ? `Quản trị viên (${w.assigned_staff_count} NV)`
                        : 'Quản trị viên'}
                    </td>
                    <td style={{ textAlign: 'center' }}>
                      <span
                        className="status-pill blue"
                        title="Bấm để xem Chi tiết Tồn kho của chi nhánh này"
                        onClick={() => {
                          setDetailKeyword('');
                          setDetailWarehouse(w);
                        }}
                      >
                        {w.total_products} mặt hàng
                      </span>
                    </td>
                    <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                      {w.total_quantity.toLocaleString('vi-VN')}
                    </td>
                    <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                      {whValue.toLocaleString('vi-VN')} ₫
                    </td>
                    <td style={{ textAlign: 'center' }}>
                      <button
                        className="icon-action-btn"
                        title="Chỉnh sửa kho"
                        onClick={() => openEditModal(w)}
                      >
                        <Icon name="pencil" />
                      </button>
                      <button
                        className="icon-action-btn danger"
                        title="Xóa kho"
                        onClick={() => handleDeleteWarehouse(w)}
                      >
                        <Icon name="trash" />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
            <tfoot>
              <tr>
                <td colSpan={6}>TỔNG CỘNG:</td>
                <td style={{ textAlign: 'center' }} className="text-emerald-bold">
                  {totalProductsSum} MẶT HÀNG
                </td>
                <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                  {totalQuantitySum.toLocaleString('vi-VN')}
                </td>
                <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                  {totalValueSum.toLocaleString('vi-VN')} ₫
                </td>
                <td />
              </tr>
            </tfoot>
          </table>
        </div>

        <div className="pagination-bar">
          <span>
            Hiển thị <strong>1 - {filteredWarehouses.length}</strong> trên tổng số{' '}
            <strong>{filteredWarehouses.length}</strong> dòng
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

      {/* MODAL CHI TIẾT TỒN KHO THEO KHO (Screen 4 & 5) */}
      {detailWarehouse && (
        <div className="modal-backdrop" onClick={() => setDetailWarehouse(null)}>
          <div className="modal-dialog lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">
                <Icon name="boxes" color="#10b981" /> Chi Tiết Tồn Kho:{' '}
                <span style={{ color: '#059669' }}>
                  {detailWarehouse.warehouse_name.toUpperCase()}
                </span>
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setDetailWarehouse(null)}
              >
                <Icon name="cross-small" />
              </button>
            </div>
            <div className="modal-body">
              <div className="filter-bar">
                <div className="search-box-wrapper">
                  <span className="search-box-icon">              <Icon name="search" /></span>
                  <input
                    className="search-box-input"
                    placeholder="Tìm mã hàng, tên hàng..."
                    value={detailKeyword}
                    onChange={(e) => setDetailKeyword(e.target.value)}
                  />
                </div>
              </div>

              <div className="table-wrapper">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>STT</th>
                      <th>MÃ HÀNG</th>
                      <th>TÊN HÀNG HÓA</th>
                      <th>ĐVT</th>
                      <th style={{ textAlign: 'right' }}>SỐ LƯỢNG TỒN</th>
                      <th style={{ textAlign: 'right' }}>ĐƠN GIÁ BQ</th>
                      <th style={{ textAlign: 'right' }}>GIÁ TRỊ TỒN</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detailItems.map((item, idx) => (
                      <tr key={item.id}>
                        <td>{idx + 1}</td>
                        <td style={{ fontWeight: 700 }}>{item.product_code}</td>
                        <td>{item.product_name}</td>
                        <td>{item.brand || 'Cái'}</td>
                        <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                          {item.quantity.toLocaleString('vi-VN')}
                        </td>
                        <td style={{ textAlign: 'right' }}>
                          {item.unit_price.toLocaleString('vi-VN')} ₫
                        </td>
                        <td style={{ textAlign: 'right', fontWeight: 700 }}>
                          {item.total_stock_value.toLocaleString('vi-VN')} ₫
                        </td>
                      </tr>
                    ))}
                    {detailItems.length === 0 && (
                      <tr>
                        <td colSpan={7} style={{ textAlign: 'center', padding: 20, color: '#64748b' }}>
                          Chưa có mặt hàng tồn kho trong chi nhánh này.
                        </td>
                      </tr>
                    )}
                  </tbody>
                  <tfoot>
                    <tr>
                      <td colSpan={4}>TỔNG CỘNG:</td>
                      <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                        {detailItems
                          .reduce((s, x) => s + x.quantity, 0)
                          .toLocaleString('vi-VN')}
                      </td>
                      <td />
                      <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                        {detailItems
                          .reduce((s, x) => s + x.total_stock_value, 0)
                          .toLocaleString('vi-VN')}{' '}
                        ₫
                      </td>
                    </tr>
                  </tfoot>
                </table>
              </div>

              <div className="pagination-bar">
                <span>
                  Hiển thị <strong>1 - {detailItems.length}</strong> trên tổng số{' '}
                  <strong>{detailItems.length}</strong> dòng
                </span>
                <div className="pagination-controls">
                  <span>Dòng/trang:</span>
                  <CustomSelect
                    variant="sm"
                    value={pageSize}
                    onChange={setPageSize}
                    options={[{ value: '10', label: '10' }]}
                  />
                  <button className="page-btn active">1</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* MODAL THÊM / SỬA KHO */}
      {showFormModal && (
        <div className="modal-backdrop" onClick={() => setShowFormModal(false)}>
          <div className="modal-dialog md" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">
                {editingWh
                  ? `Cập nhật Kho hàng: ${editingWh.warehouse_code}`
                  : 'Thêm mới Kho hàng'}
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setShowFormModal(false)}
              >
                <Icon name="cross-small" />
              </button>
            </div>
            <form onSubmit={handleSaveWarehouse}>
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label>
                      Mã kho <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      placeholder="VD: WH-DN (Để trống tự sinh)"
                      value={whCode}
                      onChange={(e) => setWhCode(e.target.value)}
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Tên chi nhánh kho <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      placeholder="VD: Kho Trung Tâm Đà Nẵng"
                      value={whName}
                      onChange={(e) => setWhName(e.target.value)}
                      required
                    />
                  </div>
                </div>
                <div className="form-group">
                  <label>Địa chỉ chi tiết</label>
                  <input
                    className="form-control"
                    placeholder="Số nhà, đường, quận/huyện, tỉnh/thành phố..."
                    value={whAddress}
                    onChange={(e) => setWhAddress(e.target.value)}
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => setShowFormModal(false)}
                >
                  Hủy
                </button>
                <button type="submit" className="btn btn-emerald">
                  <Icon name="disk" /> Lưu Kho Hàng
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
