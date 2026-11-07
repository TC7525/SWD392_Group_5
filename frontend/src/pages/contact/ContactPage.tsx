import React, { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { masterDataService } from '../../services/masterDataService';
import { type CustomerItem, type SupplierItem } from '../../types/master.types';

export const ContactPage: React.FC = () => {
  const { notifyError, notifySuccess } = useAuth();
  const [subTab, setSubTab] = useState<'SUPPLIER' | 'CUSTOMER'>('SUPPLIER');
  const [suppliers, setSuppliers] = useState<SupplierItem[]>([]);
  const [customers, setCustomers] = useState<CustomerItem[]>([]);
  const [keyword, setKeyword] = useState('');

  // Modal state
  const [showModal, setShowModal] = useState(false);
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [contactPerson, setContactPerson] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [address, setAddress] = useState('');

  const loadData = useCallback(() => {
    masterDataService
      .searchSuppliers()
      .then((res) => setSuppliers(res.items || []))
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải Nhà cung cấp')
      );
    masterDataService
      .searchCustomers()
      .then((res) => setCustomers(res.items || []))
      .catch((err: unknown) =>
        notifyError(err instanceof Error ? err.message : 'Lỗi tải Khách hàng')
      );
  }, [notifyError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const openCreateModal = () => {
    setCode('');
    setName('');
    setContactPerson('');
    setPhone('');
    setEmail('');
    setAddress('');
    setShowModal(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (subTab === 'SUPPLIER') {
        await masterDataService.createSupplier({
          supplier_code: code || undefined,
          supplier_name: name,
          contact_person: contactPerson,
          phone,
          email,
          address,
        });
        notifySuccess(`Đã thêm Nhà cung cấp '${name}'!`);
      } else {
        await masterDataService.createCustomer({
          customer_code: code || undefined,
          customer_name: name,
          phone,
          email,
          address,
        });
        notifySuccess(`Đã thêm Khách hàng '${name}'!`);
      }
      setShowModal(false);
      loadData();
    } catch (err: unknown) {
      notifyError(err instanceof Error ? err.message : 'Lỗi lưu Đối tượng');
    }
  };

  const filteredSuppliers = suppliers.filter(
    (s) =>
      !keyword ||
      s.supplier_code.toLowerCase().includes(keyword.toLowerCase()) ||
      s.supplier_name.toLowerCase().includes(keyword.toLowerCase()) ||
      (s.phone && s.phone.includes(keyword))
  );

  const filteredCustomers = customers.filter(
    (c) =>
      !keyword ||
      c.customer_code.toLowerCase().includes(keyword.toLowerCase()) ||
      c.customer_name.toLowerCase().includes(keyword.toLowerCase()) ||
      (c.phone && c.phone.includes(keyword))
  );

  return (
    <div>
      <div className="page-header">
        <div className="page-title-group">
          <h1>
            <span style={{ color: '#f59e0b' }}>👥</span> Danh Mục Đối Tượng (NCC & Khách Hàng)
          </h1>
          <p>Quản lý thông tin Nhà cung cấp đầu vào và Khách hàng / Đại lý phân phối đầu ra</p>
        </div>
        <div className="page-actions">
          <button
            className={`btn ${subTab === 'SUPPLIER' ? 'btn-emerald' : 'btn-outline'}`}
            onClick={() => setSubTab('SUPPLIER')}
          >
            🏢 Nhà cung cấp ({suppliers.length})
          </button>
          <button
            className={`btn ${subTab === 'CUSTOMER' ? 'btn-emerald' : 'btn-outline'}`}
            onClick={() => setSubTab('CUSTOMER')}
          >
            🤝 Khách hàng ({customers.length})
          </button>
          <button className="btn btn-emerald" onClick={openCreateModal}>
            + Thêm {subTab === 'SUPPLIER' ? 'Nhà cung cấp' : 'Khách hàng'}
          </button>
        </div>
      </div>

      <div className="surface-card">
        <div className="filter-bar">
          <div className="search-box-wrapper">
            <span className="search-box-icon">🔍</span>
            <input
              className="search-box-input"
              placeholder="Tìm theo mã, tên đối tượng, số điện thoại, email..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
            />
          </div>
        </div>

        {subTab === 'SUPPLIER' ? (
          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>MÃ NCC</th>
                  <th>TÊN NHÀ CUNG CẤP</th>
                  <th>NGƯỜI LIÊN HỆ</th>
                  <th>SỐ ĐIỆN THOẠI</th>
                  <th>EMAIL</th>
                  <th>ĐỊA CHỈ</th>
                  <th style={{ textAlign: 'center' }}>SỐ PHIẾU NHẬP</th>
                  <th>TRẠNG THÁI</th>
                </tr>
              </thead>
              <tbody>
                {filteredSuppliers.map((s, idx) => (
                  <tr key={s.id}>
                    <td>{idx + 1}</td>
                    <td className="voucher-code">{s.supplier_code}</td>
                    <td style={{ fontWeight: 700 }}>{s.supplier_name}</td>
                    <td>{s.contact_person || '—'}</td>
                    <td>{s.phone || '—'}</td>
                    <td>{s.email || '—'}</td>
                    <td>{s.address || 'Việt Nam'}</td>
                    <td style={{ textAlign: 'center' }}>
                      <span className="status-pill blue">
                        {s.total_purchase_orders} phiếu
                      </span>
                    </td>
                    <td>
                      <span className="status-pill emerald">{s.status_name}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>MÃ KH</th>
                  <th>TÊN KHÁCH HÀNG / ĐẠI LÝ</th>
                  <th>SỐ ĐIỆN THOẠI</th>
                  <th>EMAIL</th>
                  <th>ĐỊA CHỈ</th>
                  <th style={{ textAlign: 'center' }}>SỐ PHIẾU XUẤT</th>
                  <th>TRẠNG THÁI</th>
                </tr>
              </thead>
              <tbody>
                {filteredCustomers.map((c, idx) => (
                  <tr key={c.id}>
                    <td>{idx + 1}</td>
                    <td className="voucher-code">{c.customer_code}</td>
                    <td style={{ fontWeight: 700 }}>{c.customer_name}</td>
                    <td>{c.phone || '—'}</td>
                    <td>{c.email || '—'}</td>
                    <td>{c.address || 'Việt Nam'}</td>
                    <td style={{ textAlign: 'center' }}>
                      <span className="status-pill blue">{c.total_orders} phiếu</span>
                    </td>
                    <td>
                      <span className="status-pill emerald">{c.status_name}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {showModal && (
        <div className="modal-backdrop" onClick={() => setShowModal(false)}>
          <div className="modal-dialog md" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div className="modal-title">
                Thêm mới {subTab === 'SUPPLIER' ? 'Nhà Cung Cấp' : 'Khách Hàng'}
              </div>
              <button className="modal-close-btn" onClick={() => setShowModal(false)}>
                ✕
              </button>
            </div>
            <form onSubmit={handleSave}>
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label>Mã đối tượng (Tự sinh nếu để trống)</label>
                    <input
                      className="form-control"
                      value={code}
                      onChange={(e) => setCode(e.target.value)}
                      placeholder={subTab === 'SUPPLIER' ? 'SUP-005' : 'CUS-005'}
                    />
                  </div>
                  <div className="form-group">
                    <label>
                      Tên đối tượng <span className="req">*</span>
                    </label>
                    <input
                      className="form-control"
                      value={name}
                      onChange={(e) => setName(e.target.value)}
                      required
                    />
                  </div>
                </div>
                <div className="form-grid-2">
                  <div className="form-group">
                    <label>Số điện thoại</label>
                    <input
                      className="form-control"
                      value={phone}
                      onChange={(e) => setPhone(e.target.value)}
                    />
                  </div>
                  <div className="form-group">
                    <label>Email liên hệ</label>
                    <input
                      type="email"
                      className="form-control"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                    />
                  </div>
                </div>
                {subTab === 'SUPPLIER' && (
                  <div className="form-group" style={{ marginBottom: 12 }}>
                    <label>Người đại diện liên hệ</label>
                    <input
                      className="form-control"
                      value={contactPerson}
                      onChange={(e) => setContactPerson(e.target.value)}
                    />
                  </div>
                )}
                <div className="form-group">
                  <label>Địa chỉ trụ sở / giao hàng</label>
                  <input
                    className="form-control"
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
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
                <button type="submit" className="btn btn-emerald">
                  💾 Lưu Đối Tượng
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
