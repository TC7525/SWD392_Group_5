import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { poApi } from '../api/poApi';
import type {
  SupplierOptionDto,
  ProductOptionDto,
  CreatePurchaseOrderRequest,
  ApiError,
} from '../types/models';

interface OrderItemRow {
  id: string; // unique local key
  productId: number | '';
  orderedQuantity: number | '';
  unitPrice: number | '';
}

const DEFAULT_MIN_DELIVERY_DATE = new Date().toISOString().split('T')[0];

export const CreatePurchaseOrderPage: React.FC = () => {
  const navigate = useNavigate();

  const [suppliers, setSuppliers] = useState<SupplierOptionDto[]>([]);
  const [products, setProducts] = useState<ProductOptionDto[]>([]);
  const [isLoadingFormData, setIsLoadingFormData] = useState<boolean>(true);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  // Quick Add Product Modal state
  const [showQuickAddModal, setShowQuickAddModal] = useState<boolean>(false);
  const [quickAddTargetRowId, setQuickAddTargetRowId] = useState<string | null>(null);
  const [newProductCode, setNewProductCode] = useState<string>('');
  const [newProductName, setNewProductName] = useState<string>('');
  const [newCategoryId, setNewCategoryId] = useState<number>(1);
  const [newBrand, setNewBrand] = useState<string>('');
  const [newUnitPrice, setNewUnitPrice] = useState<string>('');
  const [newDescription, setNewDescription] = useState<string>('');
  const [isSavingProduct, setIsSavingProduct] = useState<boolean>(false);
  const [quickAddError, setQuickAddError] = useState<string | null>(null);
  const [categories, setCategories] = useState<{ categoryId: number; categoryName: string }[]>([
    { categoryId: 1, categoryName: 'Điện tử & Di động' },
    { categoryId: 2, categoryName: 'Máy tính & Laptop' },
  ]);

  // Form fields
  const [supplierId, setSupplierId] = useState<number | ''>('');
  const [expectedDeliveryDate, setExpectedDeliveryDate] = useState<string>('');
  const [items, setItems] = useState<OrderItemRow[]>([
    { id: 'item-1', productId: '', orderedQuantity: 1, unitPrice: '' },
  ]);

  // Notifications
  const [errorMessage, setErrorMessage] = useState<{ code: string; message: string } | null>(null);
  const [successMessage, setSuccessMessage] = useState<{ code: string; message: string } | null>(null);

  useEffect(() => {
    let ignore = false;
    poApi
      .getFormData()
      .then((res) => {
        if (!ignore && res.data) {
          setSuppliers(res.data.suppliers || []);
          setProducts(res.data.products || []);
          if (res.data.categories && res.data.categories.length > 0) {
            setCategories(res.data.categories);
          }
        }
      })
      .catch((err: any) => {
        if (!ignore) {
          const apiErr = err as ApiError;
          setErrorMessage({
            code: apiErr?.code || 'ERR_LOAD',
            message: apiErr?.message || 'Không thể tải dữ liệu nhà cung cấp và sản phẩm.',
          });
        }
      })
      .finally(() => {
        if (!ignore) {
          setIsLoadingFormData(false);
        }
      });

    return () => {
      ignore = true;
    };
  }, []);

  // Minimum date for delivery picker
  const minDeliveryDate = DEFAULT_MIN_DELIVERY_DATE;

  const selectedSupplier = useMemo(() => {
    if (!supplierId) return null;
    return suppliers.find((s) => s.supplierId === Number(supplierId)) || null;
  }, [suppliers, supplierId]);

  const handleOpenQuickAddModal = (targetRowId?: string) => {
    setQuickAddTargetRowId(targetRowId || null);
    setNewProductCode('');
    setNewProductName('');
    setNewCategoryId(categories[0]?.categoryId || 1);
    setNewBrand('');
    setNewUnitPrice('');
    setNewDescription('');
    setQuickAddError(null);
    setShowQuickAddModal(true);
  };

  const handleSaveQuickProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newProductName.trim()) {
      setQuickAddError('Vui lòng nhập tên sản phẩm.');
      return;
    }
    const priceNum = newUnitPrice ? parseFloat(newUnitPrice) : 0;
    if (isNaN(priceNum) || priceNum < 0) {
      setQuickAddError('Đơn giá phải là số hợp lệ lớn hơn hoặc bằng 0.');
      return;
    }

    setIsSavingProduct(true);
    setQuickAddError(null);
    try {
      const res = await poApi.quickCreateProduct({
        productCode: newProductCode.trim() || undefined,
        productName: newProductName.trim(),
        categoryId: newCategoryId,
        brand: newBrand.trim() || undefined,
        unitPrice: priceNum,
        description: newDescription.trim() || undefined,
      });

      const createdProduct = res.data;
      if (createdProduct) {
        setProducts((prev) => [...prev, createdProduct]);

        if (quickAddTargetRowId) {
          setItems((prev) =>
            prev.map((row) =>
              row.id === quickAddTargetRowId
                ? { ...row, productId: createdProduct.productId, unitPrice: createdProduct.unitPrice }
                : row
            )
          );
        } else {
          setItems((prev) => [
            ...prev,
            {
              id: `item-${Date.now()}-${Math.random().toString(36).substring(2, 7)}`,
              productId: createdProduct.productId,
              orderedQuantity: 1,
              unitPrice: createdProduct.unitPrice,
            },
          ]);
        }

        setSuccessMessage({
          code: 'MSG10',
          message: `Đã tạo thành công sản phẩm "${createdProduct.productName}" và chọn vào đơn hàng.`,
        });
        setShowQuickAddModal(false);
      }
    } catch (err: any) {
      const apiErr = err as ApiError;
      setQuickAddError(apiErr?.message || 'Không thể tạo sản phẩm mới. Vui lòng kiểm tra lại.');
    } finally {
      setIsSavingProduct(false);
    }
  };

  const handleAddItem = () => {
    setItems((prev) => [
      ...prev,
      {
        id: `item-${Date.now()}-${Math.random().toString(36).substring(2, 7)}`,
        productId: '',
        orderedQuantity: 1,
        unitPrice: '',
      },
    ]);
  };

  const handleRemoveItem = (id: string) => {
    if (items.length <= 1) {
      setErrorMessage({
        code: 'MSG01',
        message: 'Đơn mua hàng phải có ít nhất 1 sản phẩm.',
      });
      return;
    }
    setItems((prev) => prev.filter((item) => item.id !== id));
  };

  const handleProductChange = (rowId: string, pIdStr: string) => {
    const pId = pIdStr ? Number(pIdStr) : '';
    const selectedProd = products.find((p) => p.productId === pId);

    setItems((prev) =>
      prev.map((row) => {
        if (row.id !== rowId) return row;
        return {
          ...row,
          productId: pId,
          unitPrice: selectedProd ? selectedProd.unitPrice : '',
        };
      })
    );
  };

  const handleQuantityChange = (rowId: string, qtyStr: string) => {
    const qty = qtyStr === '' ? '' : parseInt(qtyStr, 10);
    setItems((prev) =>
      prev.map((row) => {
        if (row.id !== rowId) return row;
        return {
          ...row,
          orderedQuantity: isNaN(qty as number) ? '' : qty,
        };
      })
    );
  };

  const handleUnitPriceChange = (rowId: string, priceStr: string) => {
    const price = priceStr === '' ? '' : parseFloat(priceStr);
    setItems((prev) =>
      prev.map((row) => {
        if (row.id !== rowId) return row;
        return {
          ...row,
          unitPrice: isNaN(price as number) ? '' : price,
        };
      })
    );
  };

  // Calculations
  const summary = useMemo(() => {
    let totalQty = 0;
    let grandTotal = 0;
    let validItemsCount = 0;

    for (const item of items) {
      const q = typeof item.orderedQuantity === 'number' ? item.orderedQuantity : 0;
      const p = typeof item.unitPrice === 'number' ? item.unitPrice : 0;
      if (item.productId && q > 0) {
        validItemsCount++;
        totalQty += q;
        grandTotal += q * p;
      }
    }

    return { totalQty, grandTotal, validItemsCount };
  }, [items]);

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('vi-VN').format(val) + ' ₫';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setSuccessMessage(null);

    // Validation 1: Supplier must be selected
    if (!supplierId) {
      setErrorMessage({
        code: 'MSG01',
        message: 'Vui lòng chọn nhà cung cấp.',
      });
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    // Validation 2: Supplier active check
    if (selectedSupplier && selectedSupplier.status?.toLowerCase() !== 'active') {
      setErrorMessage({
        code: 'MSG15',
        message: 'Nhà cung cấp đã ngưng hoạt động và không thể sử dụng để tạo đơn.',
      });
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    // Validation 3: At least 1 item
    if (items.length === 0) {
      setErrorMessage({
        code: 'MSG01',
        message: 'Danh sách sản phẩm không được để trống.',
      });
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    // Validation 4: Each item must have valid product
    for (let i = 0; i < items.length; i++) {
      const row = items[i];
      if (!row.productId) {
        setErrorMessage({
          code: 'MSG05',
          message: `Dòng số ${i + 1}: Vui lòng chọn sản phẩm hợp lệ.`,
        });
        window.scrollTo({ top: 0, behavior: 'smooth' });
        return;
      }

      const prodExists = products.some((p) => p.productId === row.productId);
      if (!prodExists) {
        setErrorMessage({
          code: 'MSG05',
          message: `Dòng số ${i + 1}: Sản phẩm đã chọn không tồn tại trong hệ thống.`,
        });
        window.scrollTo({ top: 0, behavior: 'smooth' });
        return;
      }

      // Validation 5: Quantity > 0
      const qty = typeof row.orderedQuantity === 'number' ? row.orderedQuantity : 0;
      if (qty <= 0) {
        setErrorMessage({
          code: 'MSG02',
          message: `Dòng số ${i + 1}: Số lượng sản phẩm phải lớn hơn 0.`,
        });
        window.scrollTo({ top: 0, behavior: 'smooth' });
        return;
      }

      // Unit price must be >= 0
      const price = typeof row.unitPrice === 'number' ? row.unitPrice : 0;
      if (price < 0) {
        setErrorMessage({
          code: 'MSG01',
          message: `Dòng số ${i + 1}: Đơn giá không thể là số âm.`,
        });
        window.scrollTo({ top: 0, behavior: 'smooth' });
        return;
      }
    }

    // Validation 6: Duplicate products check
    const productIds = items.map((i) => i.productId).filter(Boolean);
    const hasDuplicate = productIds.some((id, idx) => productIds.indexOf(id) !== idx);
    if (hasDuplicate) {
      setErrorMessage({
        code: 'MSG01',
        message: 'Mỗi sản phẩm chỉ được xuất hiện 1 dòng trong đơn hàng. Vui lòng tăng số lượng sản phẩm thay vì thêm nhiều dòng trùng nhau.',
      });
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    const payload: CreatePurchaseOrderRequest = {
      supplierId: Number(supplierId),
      expectedDeliveryDate: expectedDeliveryDate ? new Date(expectedDeliveryDate).toISOString() : null,
      items: items.map((it) => ({
        productId: Number(it.productId),
        orderedQuantity: Number(it.orderedQuantity),
        unitPrice: Number(it.unitPrice),
      })),
    };

    setIsSubmitting(true);
    try {
      const res = await poApi.create(payload);
      const code = res.code || 'MSG10';
      const msg = res.message || 'Tạo đơn mua hàng thành công.';
      setSuccessMessage({ code, message: msg });

      // Navigate back to list after short moment
      setTimeout(() => {
        navigate('/purchase-orders');
      }, 1200);
    } catch (err: any) {
      const apiErr = err as ApiError;
      setErrorMessage({
        code: apiErr?.code || 'MSG01',
        message: apiErr?.message || 'Không thể tạo đơn mua hàng. Vui lòng kiểm tra lại.',
      });
      window.scrollTo({ top: 0, behavior: 'smooth' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="ewms-container">
      {/* Breadcrumb & Navigation */}
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
          ← Quay lại danh sách đơn hàng
        </Link>
      </div>

      {/* Header */}
      <div style={{ marginBottom: '24px' }}>
        <h1 className="ewms-title">Tạo Đơn Mua Hàng Mới (Purchase Order)</h1>
        <p className="ewms-subtitle">
          (UC09) Chọn nhà cung cấp và lập danh sách các sản phẩm cần nhập vào kho.
        </p>
      </div>

      {/* Error alert */}
      {errorMessage && (
        <div className="ewms-alert ewms-alert-error" role="alert">
          <span style={{ fontSize: '18px' }}>⚠️</span>
          <div>
            <span className="ewms-alert-code" style={{ marginRight: '6px' }}>
              {errorMessage.code}
            </span>
            <span style={{ fontWeight: 500 }}>{errorMessage.message}</span>
          </div>
        </div>
      )}

      {/* Success alert */}
      {successMessage && (
        <div className="ewms-alert ewms-alert-success" role="alert">
          <span style={{ fontSize: '18px' }}>🎉</span>
          <div>
            <span className="ewms-alert-code" style={{ marginRight: '6px' }}>
              {successMessage.code}
            </span>
            <span style={{ fontWeight: 600 }}>{successMessage.message}</span>
            <span style={{ marginLeft: '8px', fontSize: '13px', opacity: 0.85 }}>
              (Đang chuyển hướng về danh sách...)
            </span>
          </div>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Supplier & Delivery Info Card */}
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
            1. Thông tin chung về đơn hàng
          </h2>

          <div className="ewms-grid-2">
            {/* Supplier select */}
            <div className="ewms-form-group">
              <label htmlFor="po-supplier" className="ewms-label">
                Nhà cung cấp <span style={{ color: '#EF4444' }}>*</span>
              </label>
              <select
                id="po-supplier"
                className="ewms-select"
                value={supplierId}
                onChange={(e) => setSupplierId(e.target.value ? Number(e.target.value) : '')}
                disabled={isLoadingFormData || isSubmitting}
                required
              >
                <option value="">-- Chọn nhà cung cấp --</option>
                {suppliers.map((sup) => (
                  <option key={sup.supplierId} value={sup.supplierId}>
                    [{sup.supplierCode}] {sup.supplierName}{' '}
                    {sup.status?.toLowerCase() !== 'active' ? '(Dừng hợp tác)' : ''}
                  </option>
                ))}
              </select>

              {/* Supplier metadata preview */}
              {selectedSupplier && (
                <div
                  style={{
                    marginTop: '10px',
                    padding: '10px 14px',
                    borderRadius: '6px',
                    backgroundColor: '#F8FAFC',
                    border: '1px solid #E2E8F0',
                    fontSize: '13px',
                    color: '#475569',
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                    <span style={{ fontWeight: 600, color: '#0F172A' }}>
                      {selectedSupplier.supplierName}
                    </span>
                    <span
                      style={{
                        padding: '1px 6px',
                        borderRadius: '4px',
                        fontSize: '11px',
                        fontWeight: 600,
                        backgroundColor:
                          selectedSupplier.status?.toLowerCase() === 'active' ? '#DCFCE7' : '#FEE2E2',
                        color:
                          selectedSupplier.status?.toLowerCase() === 'active' ? '#166534' : '#991B1B',
                      }}
                    >
                      {selectedSupplier.status?.toLowerCase() === 'active' ? 'Đang hợp tác' : 'Ngưng hoạt động'}
                    </span>
                  </div>
                  {selectedSupplier.contactName && <div>Người liên hệ: {selectedSupplier.contactName}</div>}
                  {selectedSupplier.phone && <div>Điện thoại: {selectedSupplier.phone}</div>}
                  {selectedSupplier.email && <div>Email: {selectedSupplier.email}</div>}
                  {selectedSupplier.address && <div>Địa chỉ: {selectedSupplier.address}</div>}
                </div>
              )}
            </div>

            {/* Expected Delivery Date */}
            <div className="ewms-form-group">
              <label htmlFor="po-expected-date" className="ewms-label">
                Ngày giao dự kiến
              </label>
              <input
                id="po-expected-date"
                type="date"
                className="ewms-input"
                value={expectedDeliveryDate}
                min={minDeliveryDate}
                onChange={(e) => setExpectedDeliveryDate(e.target.value)}
                disabled={isSubmitting}
              />
              <span style={{ display: 'block', fontSize: '12px', color: '#64748B', marginTop: '6px' }}>
                Dự kiến hàng về kho để nhân viên chuẩn bị tiếp nhận.
              </span>
            </div>
          </div>
        </div>

        {/* Dynamic Item Table Card */}
        <div className="ewms-card">
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              marginBottom: '16px',
              borderBottom: '1px solid #E2E8F0',
              paddingBottom: '10px',
            }}
          >
            <div>
              <h2 style={{ fontSize: '16px', fontWeight: 700, color: '#0F172A' }}>
                2. Danh sách sản phẩm đặt mua <span style={{ color: '#EF4444' }}>*</span>
              </h2>
              <span style={{ fontSize: '13px', color: '#64748B' }}>
                Mỗi sản phẩm phải có số lượng lớn hơn 0 (Quy tắc BR-03).
              </span>
            </div>

            <div style={{ display: 'flex', gap: '8px' }}>
              <button
                type="button"
                className="ewms-btn ewms-btn-outline"
                onClick={() => handleOpenQuickAddModal()}
                disabled={isSubmitting}
                style={{ fontSize: '13px', padding: '6px 14px', borderColor: '#10B981', color: '#059669', backgroundColor: '#ECFDF5' }}
              >
                + Tạo nhanh sản phẩm mới
              </button>
              <button
                type="button"
                className="ewms-btn ewms-btn-outline"
                onClick={handleAddItem}
                disabled={isSubmitting}
                style={{ fontSize: '13px', padding: '6px 14px' }}
              >
                + Thêm sản phẩm
              </button>
            </div>
          </div>

          <div className="ewms-table-container">
            <table className="ewms-table">
              <thead>
                <tr>
                  <th style={{ width: '40px', textAlign: 'center' }}>#</th>
                  <th style={{ minWidth: '280px' }}>Sản phẩm</th>
                  <th style={{ width: '160px', textAlign: 'right' }}>Đơn giá (VNĐ)</th>
                  <th style={{ width: '130px', textAlign: 'right' }}>Số lượng đặt</th>
                  <th style={{ width: '160px', textAlign: 'right' }}>Thành tiền</th>
                  <th style={{ width: '70px', textAlign: 'center' }}>Xoá</th>
                </tr>
              </thead>
              <tbody>
                {items.map((row, index) => {
                  const qty = typeof row.orderedQuantity === 'number' ? row.orderedQuantity : 0;
                  const price = typeof row.unitPrice === 'number' ? row.unitPrice : 0;
                  const lineTotal = qty * price;

                  return (
                    <tr key={row.id}>
                      <td style={{ textAlign: 'center', color: '#64748B', fontWeight: 600 }}>
                        {index + 1}
                      </td>

                      <td>
                        <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                          <select
                            className="ewms-select"
                            value={row.productId}
                            onChange={(e) => handleProductChange(row.id, e.target.value)}
                            disabled={isLoadingFormData || isSubmitting}
                            required
                            style={{ fontSize: '13px', flex: 1 }}
                          >
                            <option value="">-- Chọn sản phẩm --</option>
                            {products.map((p) => {
                              const isSelectedElsewhere = items.some(
                                (item) => item.id !== row.id && item.productId === p.productId
                              );
                              return (
                                <option
                                  key={p.productId}
                                  value={p.productId}
                                  disabled={isSelectedElsewhere}
                                >
                                  [{p.productCode}] {p.productName} ({formatCurrency(p.unitPrice)})
                                  {isSelectedElsewhere ? ' (Đã chọn ở dòng khác)' : ''}
                                </option>
                              );
                            })}
                          </select>
                          <button
                            type="button"
                            title="Tạo sản phẩm mới vào hệ thống và chọn luôn vào dòng này"
                            onClick={() => handleOpenQuickAddModal(row.id)}
                            disabled={isSubmitting}
                            style={{
                              padding: '7px 11px',
                              fontSize: '12px',
                              fontWeight: 600,
                              color: '#059669',
                              backgroundColor: '#ECFDF5',
                              border: '1px solid #A7F3D0',
                              borderRadius: '6px',
                              cursor: 'pointer',
                              whiteSpace: 'nowrap',
                              display: 'inline-flex',
                              alignItems: 'center',
                              gap: '4px',
                            }}
                          >
                            + Mới
                          </button>
                        </div>
                      </td>

                      <td>
                        <input
                          type="number"
                          className="ewms-input"
                          min="0"
                          step="1000"
                          placeholder="0"
                          value={row.unitPrice}
                          onChange={(e) => handleUnitPriceChange(row.id, e.target.value)}
                          disabled={isSubmitting}
                          required
                          style={{ textAlign: 'right', fontSize: '13px' }}
                        />
                      </td>

                      <td>
                        <input
                          type="number"
                          className="ewms-input"
                          min="1"
                          step="1"
                          placeholder="1"
                          value={row.orderedQuantity}
                          onChange={(e) => handleQuantityChange(row.id, e.target.value)}
                          disabled={isSubmitting}
                          required
                          style={{ textAlign: 'right', fontSize: '13px' }}
                        />
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

                      <td style={{ textAlign: 'center' }}>
                        <button
                          type="button"
                          onClick={() => handleRemoveItem(row.id)}
                          disabled={items.length <= 1 || isSubmitting}
                          style={{
                            background: 'transparent',
                            border: 'none',
                            color: items.length <= 1 ? '#CBD5E1' : '#EF4444',
                            cursor: items.length <= 1 ? 'not-allowed' : 'pointer',
                            fontSize: '18px',
                            fontWeight: 700,
                            padding: '4px 8px',
                            borderRadius: '4px',
                          }}
                          title="Xoá dòng sản phẩm"
                        >
                          ×
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Quick add row button below table */}
          <div style={{ marginTop: '14px' }}>
            <button
              type="button"
              className="ewms-btn ewms-btn-secondary"
              onClick={handleAddItem}
              disabled={isSubmitting}
              style={{ fontSize: '13px' }}
            >
              + Thêm một dòng sản phẩm khác
            </button>
          </div>
        </div>

        {/* Grand Total Summary Card */}
        <div
          className="ewms-card"
          style={{
            backgroundColor: '#F8FAFC',
            border: '2px dashed #CBD5E1',
            padding: '20px 24px',
          }}
        >
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              flexWrap: 'wrap',
              gap: '16px',
            }}
          >
            <div style={{ display: 'flex', gap: '28px', flexWrap: 'wrap' }}>
              <div>
                <div style={{ fontSize: '12px', color: '#64748B', fontWeight: 600 }}>SỐ MẶT HÀNG</div>
                <div style={{ fontSize: '20px', fontWeight: 700, color: '#0F172A' }}>
                  {summary.validItemsCount} / {items.length} mặt hàng
                </div>
              </div>

              <div>
                <div style={{ fontSize: '12px', color: '#64748B', fontWeight: 600 }}>TỔNG SỐ LƯỢNG ĐẶT</div>
                <div style={{ fontSize: '20px', fontWeight: 700, color: '#2563EB' }}>
                  {summary.totalQty.toLocaleString('vi-VN')} SP
                </div>
              </div>
            </div>

            <div style={{ textAlign: 'right' }}>
              <div style={{ fontSize: '12px', color: '#64748B', fontWeight: 600 }}>TỔNG TIỀN ĐƠN HÀNG (GRAND TOTAL)</div>
              <div style={{ fontSize: '28px', fontWeight: 800, color: '#0F172A' }}>
                {formatCurrency(summary.grandTotal)}
              </div>
            </div>
          </div>
        </div>

        {/* Submit & Cancel Actions */}
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'flex-end',
            gap: '12px',
            marginTop: '24px',
          }}
        >
          <button
            type="button"
            className="ewms-btn ewms-btn-secondary"
            onClick={() => navigate('/purchase-orders')}
            disabled={isSubmitting}
            style={{ padding: '10px 20px' }}
          >
            Huỷ bỏ
          </button>

          <button
            type="submit"
            className="ewms-btn ewms-btn-primary"
            disabled={isSubmitting || isLoadingFormData}
            style={{ padding: '10px 24px', fontSize: '15px' }}
          >
            {isSubmitting ? (
              <>
                <span className="ewms-spinner" />
                <span>Đang tạo đơn hàng...</span>
              </>
            ) : (
              '✓ Tạo đơn mua hàng (Gửi duyệt)'
            )}
          </button>
        </div>
      </form>

      {/* Quick Add Product Modal */}
      {showQuickAddModal && (
        <div
          className="ewms-modal-backdrop"
          onClick={() => !isSavingProduct && setShowQuickAddModal(false)}
        >
          <div
            className="ewms-modal"
            style={{ maxWidth: '540px' }}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="ewms-modal-header">
              <h3 className="ewms-modal-title" style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <span style={{ fontSize: '20px' }}>📦</span> Tạo nhanh sản phẩm mới
              </h3>
              <button
                type="button"
                onClick={() => setShowQuickAddModal(false)}
                disabled={isSavingProduct}
                style={{
                  background: 'transparent',
                  border: 'none',
                  fontSize: '20px',
                  cursor: 'pointer',
                  color: '#64748B',
                }}
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleSaveQuickProduct}>
              <div className="ewms-modal-body" style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
                {quickAddError && (
                  <div className="ewms-alert ewms-alert-error" style={{ fontSize: '13px', padding: '10px 14px' }}>
                    ⚠️ {quickAddError}
                  </div>
                )}

                <div>
                  <label className="ewms-label" style={{ fontSize: '13px', marginBottom: '4px', display: 'block' }}>
                    Tên sản phẩm <span style={{ color: '#EF4444' }}>*</span>
                  </label>
                  <input
                    type="text"
                    className="ewms-input"
                    placeholder="VD: iPad Air 11 inch M2 128GB"
                    value={newProductName}
                    onChange={(e) => setNewProductName(e.target.value)}
                    required
                    disabled={isSavingProduct}
                    autoFocus
                  />
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
                  <div>
                    <label className="ewms-label" style={{ fontSize: '13px', marginBottom: '4px', display: 'block' }}>
                      Mã sản phẩm (Tùy chọn)
                    </label>
                    <input
                      type="text"
                      className="ewms-input"
                      placeholder="VD: PRD-IPAD-AIR"
                      value={newProductCode}
                      onChange={(e) => setNewProductCode(e.target.value)}
                      disabled={isSavingProduct}
                    />
                    <span style={{ fontSize: '11px', color: '#94A3B8' }}>Để trống hệ thống sẽ tự sinh mã</span>
                  </div>

                  <div>
                    <label className="ewms-label" style={{ fontSize: '13px', marginBottom: '4px', display: 'block' }}>
                      Danh mục <span style={{ color: '#EF4444' }}>*</span>
                    </label>
                    <select
                      className="ewms-select"
                      value={newCategoryId}
                      onChange={(e) => setNewCategoryId(Number(e.target.value))}
                      disabled={isSavingProduct}
                      required
                    >
                      {categories.map((c) => (
                        <option key={c.categoryId} value={c.categoryId}>
                          {c.categoryName}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
                  <div>
                    <label className="ewms-label" style={{ fontSize: '13px', marginBottom: '4px', display: 'block' }}>
                      Hãng sản xuất (Brand)
                    </label>
                    <input
                      type="text"
                      className="ewms-input"
                      placeholder="VD: Apple, Logitech..."
                      value={newBrand}
                      onChange={(e) => setNewBrand(e.target.value)}
                      disabled={isSavingProduct}
                    />
                  </div>

                  <div>
                    <label className="ewms-label" style={{ fontSize: '13px', marginBottom: '4px', display: 'block' }}>
                      Đơn giá tham chiếu (VNĐ) <span style={{ color: '#EF4444' }}>*</span>
                    </label>
                    <input
                      type="number"
                      className="ewms-input"
                      min="0"
                      step="1000"
                      placeholder="VD: 16990000"
                      value={newUnitPrice}
                      onChange={(e) => setNewUnitPrice(e.target.value)}
                      required
                      disabled={isSavingProduct}
                    />
                  </div>
                </div>

                <div>
                  <label className="ewms-label" style={{ fontSize: '13px', marginBottom: '4px', display: 'block' }}>
                    Mô tả tóm tắt
                  </label>
                  <textarea
                    className="ewms-input"
                    rows={2}
                    placeholder="Mô tả thông số hoặc quy cách đóng gói..."
                    value={newDescription}
                    onChange={(e) => setNewDescription(e.target.value)}
                    disabled={isSavingProduct}
                    style={{ resize: 'vertical' }}
                  />
                </div>
              </div>

              <div className="ewms-modal-footer" style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
                <button
                  type="button"
                  className="ewms-btn ewms-btn-secondary"
                  onClick={() => setShowQuickAddModal(false)}
                  disabled={isSavingProduct}
                >
                  Huỷ
                </button>
                <button
                  type="submit"
                  className="ewms-btn ewms-btn-primary"
                  disabled={isSavingProduct}
                  style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}
                >
                  {isSavingProduct ? 'Đang lưu...' : '✓ Lưu & Chọn vào đơn'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default CreatePurchaseOrderPage;
