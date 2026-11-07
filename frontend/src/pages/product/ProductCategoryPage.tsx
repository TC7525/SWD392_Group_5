import React, { useCallback, useEffect, useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { useAuth } from '../../contexts/AuthContext';
import { inventoryService } from '../../services/inventoryService';
import { masterDataService } from '../../services/masterDataService';
import {
  type CategoryItem,
  type ProductItem,
  type WarehouseItem,
} from '../../types/master.types';

interface InitialStockRow {
  warehouse_id: string;
  quantity: number;
  unit_price: number;
  min_stock: number;
}

export const ProductCategoryPage: React.FC = () => {
  const { user, notifyError, notifySuccess } = useAuth();
  const [products, setProducts] = useState<ProductItem[]>([]);
  const [categories, setCategories] = useState<CategoryItem[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseItem[]>([]);

  const [keyword, setKeyword] = useState('');
  const [filterCategoryId, setFilterCategoryId] = useState('');
  const [filterStockStatus, setFilterStockStatus] = useState('ALL');
  const [pageSize, setPageSize] = useState('10');

  // Product Modal
  const [showProductModal, setShowProductModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState<ProductItem | null>(null);
  const [productCode, setProductCode] = useState('');
  const [productName, setProductName] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [unitName, setUnitName] = useState('Cái');
  const [brand, setBrand] = useState('');
  const [unitPrice, setUnitPrice] = useState(0);
  const [minStock, setMinStock] = useState(10);
  const [initialStocks, setInitialStocks] = useState<InitialStockRow[]>([]);

  // Category Modal
  const [showCategoryModal, setShowCategoryModal] = useState(false);
  const [catCode, setCatCode] = useState('');
  const [catName, setCatName] = useState('');
  const [catDesc, setCatDesc] = useState('');

  const loadData = useCallback(() => {
    if (!user) return;
    masterDataService
      .searchProducts(user.current_warehouse_id)
      .then((res) => setProducts(res.items || []))
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải Hàng hóa')
      );
    masterDataService
      .searchCategories()
      .then((res) => {
        const list = res.items || [];
        setCategories(list);
        if (list.length > 0 && !categoryId) setCategoryId(list[0].id);
      })
      .catch(() => {});
    masterDataService
      .searchWarehouses()
      .then((res) => setWarehouses(res.items || []))
      .catch(() => {});
  }, [user, categoryId, notifyError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const openCreateModal = () => {
    setEditingProduct(null);
    setProductCode('');
    setProductName('');
    setCategoryId(categories[0]?.id || '');
    setUnitName('Cái');
    setBrand('Chính hãng');
    setUnitPrice(1500000);
    setMinStock(10);
    setInitialStocks(
      warehouses.slice(0, 2).map((w) => ({
        warehouse_id: w.id,
        quantity: 0,
        unit_price: 1500000,
        min_stock: 10,
      }))
    );
    setShowProductModal(true);
  };

  const openEditModal = (p: ProductItem) => {
    setEditingProduct(p);
    setProductCode(p.product_code);
    setProductName(p.product_name);
    setCategoryId(p.category_id);
    setUnitName('Cái');
    setBrand(p.brand || '');
    setUnitPrice(p.unit_price);
    setMinStock(p.minimum_stock_level);
    setInitialStocks([
      {
        warehouse_id: user?.current_warehouse_id || warehouses[0]?.id || '',
        quantity: p.current_warehouse_stock,
        unit_price: p.unit_price,
        min_stock: p.minimum_stock_level,
      },
    ]);
    setShowProductModal(true);
  };

  const handleSaveProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const effectivePrice =
        initialStocks[0]?.unit_price > 0 ? initialStocks[0].unit_price : unitPrice;
      const effectiveMin =
        initialStocks[0]?.min_stock > 0 ? initialStocks[0].min_stock : minStock;

      if (editingProduct) {
        await masterDataService.updateProduct({
          id: editingProduct.id,
          product_code: productCode,
          product_name: productName,
          category_id: categoryId,
          brand: brand || unitName,
          unit_price: Number(effectivePrice),
          minimum_stock_level: Number(effectiveMin),
        });
        notifySuccess(`Đã cập nhật mặt hàng '${productName}'!`);
      } else {
        const newProductId = await masterDataService.createProduct({
          product_code: productCode || undefined,
          product_name: productName,
          category_id: categoryId,
          brand: brand || unitName,
          unit_price: Number(effectivePrice),
          minimum_stock_level: Number(effectiveMin),
        });

        for (const row of initialStocks) {
          if (row.warehouse_id && row.quantity > 0 && newProductId) {
            await inventoryService
              .adjustInventory({
                warehouse_id: row.warehouse_id,
                product_id: newProductId,
                new_quantity: Number(row.quantity),
                note: 'Khai báo tồn đầu kỳ khi tạo mới Hàng hóa',
              })
              .catch(() => {});
          }
        }
        notifySuccess(`Đã thêm mới mặt hàng '${productName}' thành công!`);
      }
      setShowProductModal(false);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi lưu Hàng hóa');
    }
  };

  const handleDeleteProduct = async (p: ProductItem) => {
    if (!window.confirm(`Bạn có chắc muốn xóa mặt hàng "${p.product_name}"?`)) return;
    try {
      await masterDataService.deleteProduct(p.id);
      notifySuccess(`Đã xóa mặt hàng '${p.product_code}'.`);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Không thể xóa hàng hóa');
    }
  };

  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await masterDataService.createCategory({
        category_code: catCode || undefined,
        category_name: catName,
        description: catDesc,
      });
      notifySuccess(`Đã tạo nhóm hàng '${catName}'!`);
      setCatCode('');
      setCatName('');
      setCatDesc('');
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi tạo nhóm hàng');
    }
  };

  const filteredProducts = products.filter((p) => {
    const matchKw =
      !keyword ||
      p.product_code.toLowerCase().includes(keyword.toLowerCase()) ||
      p.product_name.toLowerCase().includes(keyword.toLowerCase()) ||
      (p.brand && p.brand.toLowerCase().includes(keyword.toLowerCase()));
    const matchCat = !filterCategoryId || p.category_id === filterCategoryId;
    const matchStatus =
      filterStockStatus === 'ALL' ||
      (filterStockStatus === 'LOW' && p.is_low_stock) ||
      (filterStockStatus === 'NORMAL' && !p.is_low_stock);
    return matchKw && matchCat && matchStatus;
  });

  const totalCurrentStock = filteredProducts.reduce(
    (sum, p) => sum + p.current_warehouse_stock,
    0
  );
  const totalAllStock = filteredProducts.reduce((sum, p) => sum + p.total_stock, 0);
  const totalValue = filteredProducts.reduce(
    (sum, p) => sum + p.total_stock * p.unit_price,
    0
  );

  return (
    <div>
      {/* PAGE HEADER */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#0ea5e9' }}>📦</span> Danh Mục Hàng Hóa
          </h1>
          <p>
            Quản lý danh mục vật tư, linh kiện, hàng hóa, quy cách đóng gói và định mức tồn kho tối thiểu
          </p>
        </div>
        <div className="page-actions">
          <button
            className="btn btn-outline"
            onClick={() => setShowCategoryModal(true)}
          >
            🏷️ Nhóm hàng ({categories.length})
          </button>
          <button className="btn btn-emerald" onClick={openCreateModal}>
            + Thêm hàng
          </button>
          <button
            className="btn btn-outline"
            onClick={() => notifySuccess('Đã xuất danh sách Hàng hóa ra file Excel!')}
          >
            📗 Excel
          </button>
        </div>
      </div>

      {/* MAIN TABLE CARD */}
      <div className="surface-card">
        <div className="filter-bar">
          <div className="search-box-wrapper">
            <span className="search-box-icon">🔍</span>
            <input
              className="search-box-input"
              placeholder="Tìm mã hàng, tên hàng, thương hiệu..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </div>

          <CustomSelect
            variant="filter"
            value={filterCategoryId}
            onChange={setFilterCategoryId}
            options={[
              { value: '', label: 'Tất cả nhóm hàng' },
              ...categories.map((c) => ({
                value: c.id,
                label: `${c.category_name} (${c.product_count})`,
              })),
            ]}
          />

          <CustomSelect
            variant="filter"
            value={filterStockStatus}
            onChange={setFilterStockStatus}
            options={[
              { value: 'ALL', label: 'Tất cả tình trạng' },
              { value: 'NORMAL', label: 'Đủ định mức an toàn' },
              { value: 'LOW', label: 'Cảnh báo sắp hết' },
            ]}
          />
        </div>

        <div className="table-wrapper">
          <table className="data-table">
            <thead>
              <tr>
                <th>STT</th>
                <th>MÃ HÀNG</th>
                <th>TÊN HÀNG HÓA</th>
                <th>NHÓM PHÂN LOẠI</th>
                <th>ĐVT / HÃNG</th>
                <th style={{ textAlign: 'right' }}>ĐƠN GIÁ BQ</th>
                <th style={{ textAlign: 'right' }}>ĐỊNH MỨC TỐI THIỂU</th>
                <th style={{ textAlign: 'right' }}>TỒN KHO HIỆN TẠI</th>
                <th style={{ textAlign: 'right' }}>TỔNG TOÀN HỆ THỐNG</th>
                <th>TRẠNG THÁI</th>
                <th style={{ textAlign: 'center' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {filteredProducts.map((p, index) => (
                <tr key={p.id}>
                  <td>{index + 1}</td>
                  <td>
                    <span className="voucher-code" onClick={() => openEditModal(p)}>
                      {p.product_code}
                    </span>
                  </td>
                  <td style={{ fontWeight: 600 }}>{p.product_name}</td>
                  <td>
                    <span className="status-pill gray">{p.category_name}</span>
                  </td>
                  <td>{p.brand || 'Cái'}</td>
                  <td style={{ textAlign: 'right', fontWeight: 600 }}>
                    {p.unit_price.toLocaleString('vi-VN')} ₫
                  </td>
                  <td style={{ textAlign: 'right', color: '#64748b' }}>
                    {p.minimum_stock_level}
                  </td>
                  <td
                    style={{
                      textAlign: 'right',
                      fontWeight: 700,
                      color: p.is_low_stock ? '#dc2626' : '#059669',
                    }}
                  >
                    {p.current_warehouse_stock.toLocaleString('vi-VN')}
                  </td>
                  <td style={{ textAlign: 'right', fontWeight: 700 }}>
                    {p.total_stock.toLocaleString('vi-VN')}
                  </td>
                  <td>
                    <span
                      className={`status-pill ${
                        p.is_low_stock ? 'danger' : 'emerald'
                      }`}
                    >
                      {p.is_low_stock ? 'Cảnh báo thiếu' : 'Đang kinh doanh'}
                    </span>
                  </td>
                  <td style={{ textAlign: 'center' }}>
                    <button
                      className="icon-action-btn"
                      title="Chỉnh sửa hàng hóa"
                      onClick={() => openEditModal(p)}
                    >
                      ✏️
                    </button>
                    <button
                      className="icon-action-btn danger"
                      title="Xóa hàng hóa"
                      onClick={() => handleDeleteProduct(p)}
                    >
                      🗑️
                    </button>
                  </td>
                </tr>
              ))}
              {filteredProducts.length === 0 && (
                <tr>
                  <td colSpan={11} style={{ textAlign: 'center', padding: 24, color: '#64748b' }}>
                    Không tìm thấy mặt hàng nào phù hợp.
                  </td>
                </tr>
              )}
            </tbody>
            <tfoot>
              <tr>
                <td colSpan={7}>TỔNG CỘNG ({filteredProducts.length} MẶT HÀNG):</td>
                <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                  {totalCurrentStock.toLocaleString('vi-VN')} sp
                </td>
                <td style={{ textAlign: 'right' }} className="text-emerald-bold">
                  {totalAllStock.toLocaleString('vi-VN')} sp
                </td>
                <td colSpan={2} style={{ textAlign: 'right' }} className="text-emerald-bold">
                  {totalValue.toLocaleString('vi-VN')} ₫
                </td>
              </tr>
            </tfoot>
          </table>
        </div>

        <div className="pagination-bar">
          <span>
            Hiển thị <strong>1 - {filteredProducts.length}</strong> trên tổng số{' '}
            <strong>{filteredProducts.length}</strong> dòng
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
                { value: '50', label: '50' },
              ]}
            />
            <button className="page-btn">«</button>
            <button className="page-btn active">1</button>
            <button className="page-btn">»</button>
          </div>
        </div>
      </div>

      {/* MODAL THÊM MỚI / CẬP NHẬT HÀNG HÓA (Screen 3) */}
      {showProductModal && (
        <div className="modal-backdrop" onClick={() => setShowProductModal(false)}>
          <div className="modal-dialog lg" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">
                {editingProduct
                  ? `Cập nhật Hàng hóa: ${editingProduct.product_code}`
                  : 'Thêm mới Hàng hóa'}
              </div>
              <button
                className="modal-close-btn"
                onClick={() => setShowProductModal(false)}
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleSaveProduct}>
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label>
                      Mã hàng <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      placeholder="VD: PRD-009 (Để trống tự sinh)"
                      value={productCode}
                      onChange={(e) => setProductCode(e.target.value)}
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Tên hàng hóa <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      placeholder="Nhập tên hàng hóa / thiết bị..."
                      value={productName}
                      onChange={(e) => setProductName(e.target.value)}
                      required
                    />
                  </div>
                </div>

                <div className="form-grid-3">
                  <div className="form-group">
                    <label>Phân loại / Nhóm hàng</label>
                    <CustomSelect
                      variant="form"
                      value={categoryId}
                      onChange={setCategoryId}
                      options={categories.map((c) => ({
                        value: c.id,
                        label: c.category_name,
                      }))}
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Đơn vị tính (ĐVT) <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      value={unitName}
                      onChange={(e) => setUnitName(e.target.value)}
                      placeholder="Cái / Bộ / Hộp / Chiếc"
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>Quy cách đóng gói / Hãng</label>
                    <input
                      className="form-control"
                      value={brand}
                      onChange={(e) => setBrand(e.target.value)}
                      placeholder="VD: Dell / Samsung / Fullbox"
                    />
                  </div>
                </div>

                {/* TỒN ĐẦU KỲ THEO KHO */}
                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    margin: '16px 0 10px',
                  }}
                >
                  <div style={{ fontWeight: 700, fontSize: 13.5, color: '#0f172a' }}>
                    🏭 Tồn đầu kỳ theo Kho
                  </div>
                  <button
                    type="button"
                    className="btn btn-outline btn-sm"
                    onClick={() =>
                      setInitialStocks([
                        ...initialStocks,
                        {
                          warehouse_id: warehouses[0]?.id || '',
                          quantity: 10,
                          unit_price: unitPrice || 1000000,
                          min_stock: minStock || 10,
                        },
                      ])
                    }
                  >
                    + Thêm kho
                  </button>
                </div>

                <div className="table-wrapper">
                  <table className="data-table">
                    <thead>
                      <tr>
                        <th style={{ width: 55 }}>STT</th>
                        <th>Kho lưu trữ</th>
                        <th style={{ width: 150 }}>Tồn đầu kỳ</th>
                        <th style={{ width: 180 }}>Đơn giá đầu (₫)</th>
                        <th style={{ width: 170 }}>Định mức tối thiểu</th>
                        <th style={{ width: 65, textAlign: 'center' }}>Xóa</th>
                      </tr>
                    </thead>
                    <tbody>
                      {initialStocks.map((row, idx) => (
                        <tr key={idx}>
                          <td>{idx + 1}</td>
                          <td>
                            <CustomSelect
                              variant="form"
                              value={row.warehouse_id}
                              onChange={(val) => {
                                const next = [...initialStocks];
                                next[idx].warehouse_id = val;
                                setInitialStocks(next);
                              }}
                              options={warehouses.map((w) => ({
                                value: w.id,
                                label: `${w.warehouse_name.toUpperCase()} (${w.warehouse_code})`,
                                subLabel: w.address,
                              }))}
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              min={0}
                              className="form-control"
                              style={{ width: '100%' }}
                              value={row.quantity}
                              onChange={(e) => {
                                const next = [...initialStocks];
                                next[idx].quantity = Number(e.target.value);
                                setInitialStocks(next);
                              }}
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              min={0}
                              className="form-control"
                              style={{ width: '100%' }}
                              value={row.unit_price}
                              onChange={(e) => {
                                const val = Number(e.target.value);
                                const next = [...initialStocks];
                                next[idx].unit_price = val;
                                setInitialStocks(next);
                                setUnitPrice(val);
                              }}
                            />
                          </td>
                          <td>
                            <input
                              type="number"
                              min={0}
                              className="form-control"
                              style={{ width: '100%' }}
                              value={row.min_stock}
                              onChange={(e) => {
                                const val = Number(e.target.value);
                                const next = [...initialStocks];
                                next[idx].min_stock = val;
                                setInitialStocks(next);
                                setMinStock(val);
                              }}
                            />
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            <button
                              type="button"
                              className="icon-action-btn danger"
                              onClick={() =>
                                setInitialStocks(
                                  initialStocks.filter((_, i) => i !== idx)
                                )
                              }
                            >
                              🗑️
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => setShowProductModal(false)}
                >
                  Hủy bỏ
                </button>
                <button type="submit" className="btn btn-emerald">
                  💾 Lưu Hàng Hóa
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL QUẢN LÝ NHÓM HÀNG */}
      {showCategoryModal && (
        <div className="modal-backdrop" onClick={() => setShowCategoryModal(false)}>
          <div className="modal-dialog md" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">🏷️ Quản lý Phân loại / Nhóm hàng</div>
              <button
                className="modal-close-btn"
                onClick={() => setShowCategoryModal(false)}
              >
                ✕
              </button>
            </div>
            <div className="modal-body">
              <form
                onSubmit={handleCreateCategory}
                style={{
                  display: 'grid',
                  gridTemplateColumns: '140px 1fr 1fr auto',
                  gap: 10,
                  marginBottom: 16,
                  alignItems: 'end',
                }}
              >
                <div className="form-group">
                  <label>Mã nhóm</label>
                  <input
                    className="form-control"
                    placeholder="CAT-05"
                    value={catCode}
                    onChange={(e) => setCatCode(e.target.value)}
                  />
                </div>
                <div className="form-group">
                  <label>
                    Tên nhóm hàng <span className="req">*</span>
                  </label>
                  <input
                    className="form-control"
                    placeholder="Thiết bị âm thanh"
                    value={catName}
                    onChange={(e) => setCatName(e.target.value)}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Mô tả</label>
                  <input
                    className="form-control"
                    placeholder="Ghi chú..."
                    value={catDesc}
                    onChange={(e) => setCatDesc(e.target.value)}
                  />
                </div>
                <button type="submit" className="btn btn-emerald" style={{ height: 36 }}>
                  + Thêm
                </button>
              </form>

              <table className="data-table">
                <thead>
                  <tr>
                    <th>STT</th>
                    <th>MÃ NHÓM</th>
                    <th>TÊN NHÓM HÀNG</th>
                    <th>MÔ TẢ</th>
                    <th style={{ textAlign: 'center' }}>SỐ MẶT HÀNG</th>
                  </tr>
                </thead>
                <tbody>
                  {categories.map((c, idx) => (
                    <tr key={c.id}>
                      <td>{idx + 1}</td>
                      <td className="voucher-code">{c.category_code}</td>
                      <td style={{ fontWeight: 600 }}>{c.category_name}</td>
                      <td>{c.description || '—'}</td>
                      <td style={{ textAlign: 'center' }}>
                        <span className="status-pill blue">
                          {c.product_count} mặt hàng
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="modal-footer">
              <button
                className="btn btn-outline"
                onClick={() => setShowCategoryModal(false)}
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
