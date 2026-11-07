import React, { useState } from 'react';
import { CustomSelect } from '../../components/common/CustomSelect';
import { useAuth } from '../../contexts/AuthContext';

interface PrintTemplateItem {
  id: string;
  code: string;
  name: string;
  category: 'Nhập kho' | 'Xuất kho' | 'Chuyển kho' | 'Kiểm kê';
  defaultRows: number;
  description: string;
  signatories: string;
}

const INITIAL_TEMPLATES: PrintTemplateItem[] = [
  {
    id: 'tpl-1',
    code: '01 - NHẬP KHO',
    name: 'PHIẾU NHẬP KHO',
    category: 'Nhập kho',
    defaultRows: 8,
    description: 'Mẫu chuẩn theo quy định của Bộ Tài Chính (Thông tư 200/2014/TT-BTC).',
    signatories: 'Người lập phiếu, Người giao hàng, Thủ kho, Kế toán trưởng, Giám đốc',
  },
  {
    id: 'tpl-2',
    code: '02 - XUẤT KHO',
    name: 'PHIẾU XUẤT KHO',
    category: 'Xuất kho',
    defaultRows: 8,
    description: 'Mẫu chuẩn theo quy định của Bộ Tài Chính (Thông tư 200/2014/TT-BTC).',
    signatories: 'Người lập phiếu, Người nhận hàng, Thủ kho, Kế toán trưởng, Giám đốc',
  },
  {
    id: 'tpl-3',
    code: '03 - CHUYỂN KHO',
    name: 'PHIẾU XUẤT KIÊM VẬN CHUYỂN NỘI BỘ',
    category: 'Chuyển kho',
    defaultRows: 10,
    description: 'Mẫu chứng từ điều chuyển hàng hóa nội bộ liên chi nhánh kho.',
    signatories: 'Người lập lệnh, Thủ kho xuất, Người vận chuyển, Thủ kho nhập',
  },
];

export const PrintTemplatePage: React.FC = () => {
  const { notifySuccess } = useAuth();
  const [templates, setTemplates] =
    useState<PrintTemplateItem[]>(INITIAL_TEMPLATES);
  const [keyword, setKeyword] = useState('');
  const [filterCategory, setFilterCategory] = useState('ALL');

  // Modal Cập nhật Biểu mẫu (Screen 11)
  const [editingTpl, setEditingTpl] = useState<PrintTemplateItem | null>(null);
  const [tplCode, setTplCode] = useState('');
  const [tplName, setTplName] = useState('');
  const [tplCategory, setTplCategory] =
    useState<PrintTemplateItem['category']>('Nhập kho');
  const [tplRows, setTplRows] = useState(8);
  const [tplSignatories, setTplSignatories] = useState('');

  const openEditModal = (tpl?: PrintTemplateItem) => {
    if (tpl) {
      setEditingTpl(tpl);
      setTplCode(tpl.code);
      setTplName(tpl.name);
      setTplCategory(tpl.category);
      setTplRows(tpl.defaultRows);
      setTplSignatories(tpl.signatories);
    } else {
      setEditingTpl({
        id: `tpl-${Date.now()}`,
        code: '04 - MẪU MỚI',
        name: 'BIỂU MẪU KHO MỚI',
        category: 'Nhập kho',
        defaultRows: 8,
        description: 'Mẫu chuẩn theo quy định của Bộ Tài Chính.',
        signatories: 'Người lập phiếu, Thủ kho, Kế toán trưởng',
      });
      setTplCode('04 - MẪU MỚI');
      setTplName('BIỂU MẪU KHO MỚI');
      setTplCategory('Nhập kho');
      setTplRows(8);
      setTplSignatories('Người lập phiếu, Thủ kho, Kế toán trưởng');
    }
  };

  const handleSaveTemplate = (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingTpl) return;
    const updated: PrintTemplateItem = {
      ...editingTpl,
      code: tplCode,
      name: tplName,
      category: tplCategory,
      defaultRows: Number(tplRows),
      signatories: tplSignatories,
    };
    const exists = templates.some((t) => t.id === updated.id);
    if (exists) {
      setTemplates(templates.map((t) => (t.id === updated.id ? updated : t)));
    } else {
      setTemplates([...templates, updated]);
    }
    setEditingTpl(null);
    notifySuccess(`Đã lưu cấu hình biểu mẫu "${tplName}" thành công!`);
  };

  const filtered = templates.filter((t) => {
    const matchCat = filterCategory === 'ALL' || t.category === filterCategory;
    const matchKw =
      !keyword ||
      t.code.toLowerCase().includes(keyword.toLowerCase()) ||
      t.name.toLowerCase().includes(keyword.toLowerCase());
    return matchCat && matchKw;
  });

  return (
    <div>
      {/* PAGE HEADER (Screen 11) */}
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#10b981' }}>📁</span> Thư Mục Biểu Mẫu In Ấn & Lưu Trữ
          </h1>
          <p>Quản lý, tạo mới, chỉnh sửa và in ấn các biểu mẫu theo chuẩn Bộ Tài Chính</p>
        </div>
        <div className="page-actions">
          <button
            className="btn btn-emerald"
            onClick={() => openEditModal(undefined)}
          >
            + Thêm biểu mẫu
          </button>
        </div>
      </div>

      {/* SEARCH BAR */}
      <div className="surface-card">
        <div className="filter-bar" style={{ marginBottom: 0 }}>
          <div className="search-box-wrapper">
            <span className="search-box-icon">🔍</span>
            <input
              className="search-box-input"
              placeholder="Tìm kiếm tên hoặc mã biểu mẫu..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </div>
          <CustomSelect
            variant="filter"
            value={filterCategory}
            onChange={setFilterCategory}
            options={[
              { value: 'ALL', label: 'Tất cả nghiệp vụ' },
              { value: 'Nhập kho', label: 'Nhập kho' },
              { value: 'Xuất kho', label: 'Xuất kho' },
              { value: 'Chuyển kho', label: 'Chuyển kho' },
            ]}
          />
        </div>
      </div>

      {/* TEMPLATE CARDS GRID */}
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(440px, 1fr))',
          gap: 16,
        }}
      >
        {filtered.map((tpl) => (
          <div
            key={tpl.id}
            className="surface-card"
            style={{ marginBottom: 0, display: 'flex', flexDirection: 'column', gap: 12 }}
          >
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                borderBottom: '1px solid #f1f5f9',
                paddingBottom: 8,
              }}
            >
              <span
                style={{
                  fontSize: 12,
                  fontWeight: 700,
                  color: tpl.category === 'Xuất kho' ? '#0284c7' : '#059669',
                }}
              >
                {tpl.category}
              </span>
              <div style={{ display: 'flex', gap: 6 }}>
                <span className="status-pill gray">☰ {tpl.defaultRows} dòng</span>
                <span className="status-pill gray">{tpl.category}</span>
              </div>
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
              <div
                style={{
                  width: 40,
                  height: 40,
                  borderRadius: 10,
                  background: '#ecfdf5',
                  color: '#10b981',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: 18,
                }}
              >
                📄
              </div>
              <div>
                <div style={{ fontSize: 15, fontWeight: 800, color: '#0f172a' }}>
                  {tpl.name}
                </div>
                <div style={{ fontSize: 12, color: '#64748b', fontWeight: 600 }}>
                  {tpl.code}
                </div>
              </div>
            </div>

            <div style={{ fontSize: 12.5, color: '#64748b' }}>
              {tpl.description}
            </div>

            <div
              style={{
                background: '#f8fafc',
                border: '1px solid #e2e8f0',
                borderRadius: 8,
                padding: '8px 12px',
                fontSize: 12,
                color: '#334155',
              }}
            >
              <strong>Ký duyệt:</strong> {tpl.signatories}
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <button
                className="btn btn-outline"
                style={{ flex: 1 }}
                onClick={() => window.print()}
              >
                🖨️ In mẫu
              </button>
              <button
                className="icon-action-btn"
                title="Cập nhật biểu mẫu"
                onClick={() => openEditModal(tpl)}
              >
                ✏️
              </button>
              <button
                className="icon-action-btn danger"
                title="Xóa biểu mẫu"
                onClick={() =>
                  setTemplates(templates.filter((x) => x.id !== tpl.id))
                }
              >
                🗑️
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* MODAL CẬP NHẬT BIỂU MẪU (Screen 11) */}
      {editingTpl && (
        <div className="modal-backdrop" onClick={() => setEditingTpl(null)}>
          <div className="modal-dialog" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">Cập nhật Biểu mẫu</div>
              <button
                className="modal-close-btn"
                onClick={() => setEditingTpl(null)}
              >
                ✕
              </button>
            </div>
            <form onSubmit={handleSaveTemplate}>
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label>
                      Ký hiệu mã mẫu <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      value={tplCode}
                      onChange={(e) => setTplCode(e.target.value)}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>Phân loại nghiệp vụ</label>
                    <CustomSelect
                      variant="form"
                      value={tplCategory}
                      onChange={(val) =>
                        setTplCategory(val as PrintTemplateItem['category'])
                      }
                      options={[
                        { value: 'Nhập kho', label: 'Nhập kho' },
                        { value: 'Xuất kho', label: 'Xuất kho' },
                        { value: 'Chuyển kho', label: 'Chuyển kho' },
                      ]}
                    />
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label>
                      Tên biểu mẫu <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      value={tplName}
                      onChange={(e) => setTplName(e.target.value)}
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label>Số dòng in mẫu</label>
                    <input
                      type="number"
                      min={1}
                      className="form-control"
                      value={tplRows}
                      onChange={(e) => setTplRows(Number(e.target.value))}
                    />
                  </div>
                </div>

                <div
                  style={{
                    background: '#f8fafc',
                    border: '1px solid #e2e8f0',
                    borderRadius: 10,
                    padding: 12,
                    marginBottom: 14,
                  }}
                >
                  <div
                    style={{
                      fontWeight: 700,
                      fontSize: 12.5,
                      color: '#059669',
                      marginBottom: 8,
                    }}
                  >
                    🏢 Tiêu đề đơn vị (in ở góc trên bên trái phiếu)
                  </div>
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: 16, fontSize: 12.5 }}>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Tên công ty
                    </label>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Địa chỉ chi nhánh
                    </label>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Số điện thoại
                    </label>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Mã số thuế
                    </label>
                  </div>
                </div>

                <div
                  style={{
                    background: '#f8fafc',
                    border: '1px solid #e2e8f0',
                    borderRadius: 10,
                    padding: 12,
                    marginBottom: 14,
                  }}
                >
                  <div
                    style={{
                      fontWeight: 700,
                      fontSize: 12.5,
                      color: '#059669',
                      marginBottom: 8,
                    }}
                  >
                    📊 Cột hiển thị trong bảng khi in
                  </div>
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: 16, fontSize: 12.5 }}>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Mã số
                    </label>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> ĐVT
                    </label>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Số lượng
                    </label>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Đơn giá
                    </label>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Thành tiền
                    </label>
                    <label style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <input type="checkbox" defaultChecked /> Ghi chú
                    </label>
                  </div>
                </div>

                <div className="form-group">
                  <label>
                    ✍️ Cấu hình chữ ký cuối phiếu (mỗi chức danh phân cách bởi dấu phẩy)
                  </label>
                  <input
                    className="form-control"
                    value={tplSignatories}
                    onChange={(e) => setTplSignatories(e.target.value)}
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => setEditingTpl(null)}
                >
                  Hủy
                </button>
                <button type="submit" className="btn btn-emerald">
                  💾 Lưu Biểu Mẫu
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
