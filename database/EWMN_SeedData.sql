-- ============================================================================
-- EWMS (Electronic Warehouse Management System) - Database Setup & Seed Data
-- Database Engine: Microsoft SQL Server (T-SQL)
-- Generated for SWD392 - SE1972-NET - Group 5
-- ============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'EWMN')
BEGIN
    CREATE DATABASE EWMN;
END
GO

USE EWMN;
GO

-- 1. Drop existing tables if re-running
IF OBJECT_ID('inventory_transactions', 'U') IS NOT NULL DROP TABLE inventory_transactions;
IF OBJECT_ID('stock_transfer_items', 'U') IS NOT NULL DROP TABLE stock_transfer_items;
IF OBJECT_ID('stock_transfers', 'U') IS NOT NULL DROP TABLE stock_transfers;
IF OBJECT_ID('outbound_order_details', 'U') IS NOT NULL DROP TABLE outbound_order_details;
IF OBJECT_ID('outbound_orders', 'U') IS NOT NULL DROP TABLE outbound_orders;
IF OBJECT_ID('purchase_order_items', 'U') IS NOT NULL DROP TABLE purchase_order_items;
IF OBJECT_ID('purchase_orders', 'U') IS NOT NULL DROP TABLE purchase_orders;
IF OBJECT_ID('inventories', 'U') IS NOT NULL DROP TABLE inventories;
IF OBJECT_ID('user_warehouses', 'U') IS NOT NULL DROP TABLE user_warehouses;
IF OBJECT_ID('warehouses', 'U') IS NOT NULL DROP TABLE warehouses;
IF OBJECT_ID('products', 'U') IS NOT NULL DROP TABLE products;
IF OBJECT_ID('categories', 'U') IS NOT NULL DROP TABLE categories;
IF OBJECT_ID('suppliers', 'U') IS NOT NULL DROP TABLE suppliers;
IF OBJECT_ID('customers', 'U') IS NOT NULL DROP TABLE customers;
IF OBJECT_ID('activity_histories', 'U') IS NOT NULL DROP TABLE activity_histories;
IF OBJECT_ID('role_permissions', 'U') IS NOT NULL DROP TABLE role_permissions;
IF OBJECT_ID('user_roles', 'U') IS NOT NULL DROP TABLE user_roles;
IF OBJECT_ID('permissions', 'U') IS NOT NULL DROP TABLE permissions;
IF OBJECT_ID('roles', 'U') IS NOT NULL DROP TABLE roles;
IF OBJECT_ID('users', 'U') IS NOT NULL DROP TABLE users;
GO

-- 2. DDL - Create Tables
CREATE TABLE users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    employee_code VARCHAR(50) NOT NULL UNIQUE,
    full_name NVARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    phone VARCHAR(20) UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    status VARCHAR(20) NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL
);

CREATE TABLE roles (
    role_id INT IDENTITY(1,1) PRIMARY KEY,
    role_name NVARCHAR(50) NOT NULL UNIQUE,
    description NVARCHAR(255),
    status VARCHAR(20) NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL
);

CREATE TABLE permissions (
    permission_id INT IDENTITY(1,1) PRIMARY KEY,
    permission_code VARCHAR(100) NOT NULL UNIQUE,
    permission_name NVARCHAR(100) NOT NULL,
    module VARCHAR(50) NOT NULL,
    action VARCHAR(30) NOT NULL,
    description NVARCHAR(255),
    status VARCHAR(20) NOT NULL
);

CREATE TABLE user_roles (
    user_role_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    role_id INT NOT NULL,
    assigned_at DATETIME NOT NULL,
    assigned_by INT NULL,
    CONSTRAINT UQ_user_role UNIQUE (user_id, role_id),
    CONSTRAINT FK_user_roles_users FOREIGN KEY (user_id) REFERENCES users(user_id),
    CONSTRAINT FK_user_roles_roles FOREIGN KEY (role_id) REFERENCES roles(role_id)
);

CREATE TABLE role_permissions (
    role_permission_id INT IDENTITY(1,1) PRIMARY KEY,
    role_id INT NOT NULL,
    permission_id INT NOT NULL,
    created_at DATETIME NOT NULL,
    CONSTRAINT UQ_role_permission UNIQUE (role_id, permission_id),
    CONSTRAINT FK_role_permissions_roles FOREIGN KEY (role_id) REFERENCES roles(role_id),
    CONSTRAINT FK_role_permissions_permissions FOREIGN KEY (permission_id) REFERENCES permissions(permission_id)
);

CREATE TABLE activity_histories (
    activity_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    module VARCHAR(50) NOT NULL,
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50),
    entity_id INT,
    description NVARCHAR(500),
    created_at DATETIME NOT NULL,
    CONSTRAINT FK_activity_histories_users FOREIGN KEY (user_id) REFERENCES users(user_id)
);

CREATE TABLE customers (
    customer_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_name NVARCHAR(100) NOT NULL,
    phone VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(100),
    address NVARCHAR(255),
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL
);

CREATE TABLE suppliers (
    supplier_id INT IDENTITY(1,1) PRIMARY KEY,
    supplier_code VARCHAR(50) NOT NULL UNIQUE,
    supplier_name NVARCHAR(100) NOT NULL,
    contact_person NVARCHAR(100),
    phone VARCHAR(20),
    email VARCHAR(100),
    address NVARCHAR(255),
    status VARCHAR(20) NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL
);

CREATE TABLE categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    category_name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(500),
    status VARCHAR(20) NOT NULL
);

CREATE TABLE products (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    product_code VARCHAR(50) NOT NULL UNIQUE,
    product_name NVARCHAR(255) NOT NULL,
    category_id INT NOT NULL,
    brand NVARCHAR(100),
    description NVARCHAR(500),
    unit_price DECIMAL(18,2) NOT NULL,
    minimum_stock_level INT NOT NULL,
    status VARCHAR(20) NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    CONSTRAINT FK_products_categories FOREIGN KEY (category_id) REFERENCES categories(category_id)
);

CREATE TABLE warehouses (
    warehouse_id INT IDENTITY(1,1) PRIMARY KEY,
    warehouse_code VARCHAR(50) NOT NULL UNIQUE,
    warehouse_name NVARCHAR(100) NOT NULL,
    address NVARCHAR(255),
    status VARCHAR(20) NOT NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL
);

CREATE TABLE user_warehouses (
    user_warehouse_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    warehouse_id INT NOT NULL,
    assigned_at DATETIME NOT NULL,
    CONSTRAINT UQ_user_warehouse UNIQUE (user_id, warehouse_id),
    CONSTRAINT FK_user_warehouses_users FOREIGN KEY (user_id) REFERENCES users(user_id),
    CONSTRAINT FK_user_warehouses_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id)
);

CREATE TABLE inventories (
    inventory_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    warehouse_id INT NOT NULL,
    quantity INT NOT NULL,
    last_updated_at DATETIME NOT NULL,
    CONSTRAINT UQ_product_warehouse UNIQUE (product_id, warehouse_id),
    CONSTRAINT FK_inventories_products FOREIGN KEY (product_id) REFERENCES products(product_id),
    CONSTRAINT FK_inventories_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id)
);

CREATE TABLE purchase_orders (
    purchase_order_id INT IDENTITY(1,1) PRIMARY KEY,
    purchase_order_code VARCHAR(50) NOT NULL UNIQUE,
    supplier_id INT NOT NULL,
    created_by INT NOT NULL,
    approved_by INT NULL,
    status VARCHAR(30) NOT NULL,
    order_date DATETIME NOT NULL,
    expected_delivery_date DATETIME NULL,
    approved_date DATETIME NULL,
    completed_date DATETIME NULL,
    rejection_reason NVARCHAR(255) NULL,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    CONSTRAINT FK_purchase_orders_suppliers FOREIGN KEY (supplier_id) REFERENCES suppliers(supplier_id),
    CONSTRAINT FK_purchase_orders_created_by FOREIGN KEY (created_by) REFERENCES users(user_id),
    CONSTRAINT FK_purchase_orders_approved_by FOREIGN KEY (approved_by) REFERENCES users(user_id)
);

CREATE TABLE purchase_order_items (
    purchase_order_item_id INT IDENTITY(1,1) PRIMARY KEY,
    purchase_order_id INT NOT NULL,
    product_id INT NOT NULL,
    ordered_quantity INT NOT NULL,
    received_quantity INT NOT NULL DEFAULT 0,
    unit_price DECIMAL(18,2) NOT NULL,
    CONSTRAINT UQ_po_product UNIQUE (purchase_order_id, product_id),
    CONSTRAINT FK_purchase_order_items_po FOREIGN KEY (purchase_order_id) REFERENCES purchase_orders(purchase_order_id),
    CONSTRAINT FK_purchase_order_items_product FOREIGN KEY (product_id) REFERENCES products(product_id)
);

CREATE TABLE outbound_orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL,
    created_by INT NOT NULL,
    order_date DATETIME NOT NULL,
    status VARCHAR(30) NOT NULL,
    total_quantity INT NOT NULL,
    note NVARCHAR(255),
    CONSTRAINT FK_outbound_orders_customers FOREIGN KEY (customer_id) REFERENCES customers(customer_id),
    CONSTRAINT FK_outbound_orders_users FOREIGN KEY (created_by) REFERENCES users(user_id)
);

CREATE TABLE outbound_order_details (
    order_detail_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    issued_quantity INT NOT NULL,
    CONSTRAINT FK_outbound_order_details_orders FOREIGN KEY (order_id) REFERENCES outbound_orders(order_id),
    CONSTRAINT FK_outbound_order_details_products FOREIGN KEY (product_id) REFERENCES products(product_id)
);

CREATE TABLE stock_transfers (
    stock_transfer_id INT IDENTITY(1,1) PRIMARY KEY,
    stock_transfer_code VARCHAR(50) NOT NULL UNIQUE,
    source_warehouse_id INT NOT NULL,
    destination_warehouse_id INT NOT NULL,
    created_by INT NOT NULL,
    status VARCHAR(30) NOT NULL,
    transfer_date DATETIME NOT NULL,
    completed_date DATETIME NULL,
    note NVARCHAR(255),
    created_at DATETIME NOT NULL,
    updated_at DATETIME NULL,
    CONSTRAINT FK_stock_transfers_source FOREIGN KEY (source_warehouse_id) REFERENCES warehouses(warehouse_id),
    CONSTRAINT FK_stock_transfers_destination FOREIGN KEY (destination_warehouse_id) REFERENCES warehouses(warehouse_id),
    CONSTRAINT FK_stock_transfers_users FOREIGN KEY (created_by) REFERENCES users(user_id)
);

CREATE TABLE stock_transfer_items (
    stock_transfer_item_id INT IDENTITY(1,1) PRIMARY KEY,
    stock_transfer_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    CONSTRAINT UQ_transfer_product UNIQUE (stock_transfer_id, product_id),
    CONSTRAINT FK_stock_transfer_items_transfer FOREIGN KEY (stock_transfer_id) REFERENCES stock_transfers(stock_transfer_id),
    CONSTRAINT FK_stock_transfer_items_product FOREIGN KEY (product_id) REFERENCES products(product_id)
);

CREATE TABLE inventory_transactions (
    inventory_transaction_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    warehouse_id INT NOT NULL,
    user_id INT NOT NULL,
    purchase_order_item_id INT NULL,
    outbound_order_detail_id INT NULL,
    stock_transfer_id INT NULL,
    transaction_type VARCHAR(20) NOT NULL,
    quantity INT NOT NULL,
    transaction_date DATETIME NOT NULL,
    note NVARCHAR(255),
    CONSTRAINT FK_inv_trans_products FOREIGN KEY (product_id) REFERENCES products(product_id),
    CONSTRAINT FK_inv_trans_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(warehouse_id),
    CONSTRAINT FK_inv_trans_users FOREIGN KEY (user_id) REFERENCES users(user_id),
    CONSTRAINT FK_inv_trans_po_item FOREIGN KEY (purchase_order_item_id) REFERENCES purchase_order_items(purchase_order_item_id),
    CONSTRAINT FK_inv_trans_outbound_detail FOREIGN KEY (outbound_order_detail_id) REFERENCES outbound_order_details(order_detail_id),
    CONSTRAINT FK_inv_trans_transfer FOREIGN KEY (stock_transfer_id) REFERENCES stock_transfers(stock_transfer_id)
);
GO

-- 3. Seed Base Data
-- Password for all seed users is '123456' (SHA256: 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92)
SET IDENTITY_INSERT roles ON;
INSERT INTO roles (role_id, role_name, description, status, created_at) VALUES
(1, N'Purchase Staff', N'Nhân viên mua hàng - Quản lý NCC & tạo PO', 'Active', GETDATE()),
(2, N'Warehouse Manager', N'Quản lý kho - Duyệt PO và báo cáo vận hành kho', 'Active', GETDATE()),
(3, N'Warehouse Staff', N'Thủ kho - Nhập hàng, kiểm hàng, cập nhật tồn kho', 'Active', GETDATE()),
(4, N'Administrator', N'Quản trị hệ thống', 'Active', GETDATE());
SET IDENTITY_INSERT roles OFF;

SET IDENTITY_INSERT users ON;
INSERT INTO users (user_id, employee_code, full_name, email, phone, password_hash, status, created_at) VALUES
(1, 'EMP-PUR-01', N'Nguyễn Văn Mua', 'staff_purchase@ewms.com', '0912345671', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Active', GETDATE()),
(2, 'EMP-MGR-01', N'Trần Quản Lý', 'manager_kho@ewms.com', '0912345672', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Active', GETDATE()),
(3, 'EMP-STF-01', N'Lê Nhân Viên Kho', 'staff_kho@ewms.com', '0912345673', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Active', GETDATE());
SET IDENTITY_INSERT users OFF;

INSERT INTO user_roles (user_id, role_id, assigned_at) VALUES
(1, 1, GETDATE()), -- Purchase Staff
(2, 2, GETDATE()), -- Warehouse Manager
(3, 3, GETDATE()); -- Warehouse Staff

SET IDENTITY_INSERT warehouses ON;
INSERT INTO warehouses (warehouse_id, warehouse_code, warehouse_name, address, status, created_at) VALUES
(1, 'WH-HN-01', N'Kho Tổng Hà Nội', N'Số 8 Tôn Thất Thuyết, Mỹ Đình, Hà Nội', 'Active', GETDATE()),
(2, 'WH-HCM-01', N'Kho Tổng TP. Hồ Chí Minh', N'Khu Công nghệ cao, TP. Thủ Đức, TP.HCM', 'Active', GETDATE());
SET IDENTITY_INSERT warehouses OFF;

INSERT INTO user_warehouses (user_id, warehouse_id, assigned_at) VALUES
(3, 1, GETDATE()),
(3, 2, GETDATE());

SET IDENTITY_INSERT suppliers ON;
INSERT INTO suppliers (supplier_id, supplier_code, supplier_name, contact_person, phone, email, address, status, created_at) VALUES
(1, 'SUP001', N'Công ty TNHH Điện tử Samsung Việt Nam', N'Kim Min Soo', '0243999888', 'contact@samsung.vn', N'KCN Yên Phong, Bắc Ninh', 'Active', GETDATE()),
(2, 'SUP002', N'Công ty Cổ phần Phân phối Synnex FPT', N'Nguyễn Hoàng Long', '0287300666', 'longnh@synnexfpt.com', N'Tòa nhà FPT, Cầu Giấy, Hà Nội', 'Active', GETDATE()),
(3, 'SUP003', N'Nhà cung cấp Ngừng Hợp Tác Demo', N'Vũ Đình Tạm', '0988776655', 'inactive_sup@demo.com', N'Quận 1, TP.HCM', 'Inactive', GETDATE());
SET IDENTITY_INSERT suppliers OFF;

SET IDENTITY_INSERT categories ON;
INSERT INTO categories (category_id, category_name, description, status) VALUES
(1, N'Điện tử & Di động', N'Điện thoại thông minh, máy tính bảng và phụ kiện cao cấp', 'Active'),
(2, N'Máy tính & Laptop', N'Máy tính xách tay văn phòng, đồ họa và gaming', 'Active');
SET IDENTITY_INSERT categories OFF;

SET IDENTITY_INSERT products ON;
INSERT INTO products (product_id, product_code, product_name, category_id, brand, description, unit_price, minimum_stock_level, status, created_at) VALUES
(1, 'PRD-IP16', N'iPhone 16 Pro Max 256GB Desert Titanium', 1, 'Apple', N'Chip A18 Pro, màn hình Super Retina XDR 6.9 inch', 34990000, 10, 'Active', GETDATE()),
(2, 'PRD-S24', N'Samsung Galaxy S24 Ultra 512GB Titanium Gray', 1, 'Samsung', N'Snapdragon 8 Gen 3, Camera 200MP, Galaxy AI', 29990000, 10, 'Active', GETDATE()),
(3, 'PRD-MBA', N'MacBook Air 13 inch M3 16GB 512GB Space Gray', 2, 'Apple', N'Apple M3 8-core CPU, 10-core GPU, Liquid Retina', 27500000, 5, 'Active', GETDATE()),
(4, 'PRD-SNY', N'Tai nghe Sony WH-1000XM5 Wireless Noise Canceling', 1, 'Sony', N'Chống ồn đỉnh cao, thời lượng pin 30 giờ', 7490000, 8, 'Active', GETDATE()),
(5, 'PRD-IPAD', N'iPad Pro 11 inch M4 256GB Wi-Fi', 1, 'Apple', N'Máy tính bảng Apple chip M4 thế hệ mới', 28990000, 5, 'Active', GETDATE()),
(6, 'PRD-LOGI', N'Chuột Logitech MX Master 3S Wireless', 2, 'Logitech', N'Chuột công thái học cao cấp cho văn phòng và đồ họa', 2490000, 10, 'Active', GETDATE()),
(7, 'PRD-DELL', N'Màn hình Dell UltraSharp U2724D 27 inch 2K 120Hz', 2, 'Dell', N'Màn hình chuyên đồ họa chuẩn màu IPS Black', 11500000, 5, 'Active', GETDATE()),
(8, 'PRD-KEY', N'Bàn phím cơ Keychron K2 Pro QMK/VIA Wireless', 2, 'Keychron', N'Bàn phím cơ không dây layout 75%', 2190000, 10, 'Active', GETDATE());
SET IDENTITY_INSERT products OFF;

-- Baseline initial stock (0 for fresh demo)
INSERT INTO inventories (product_id, warehouse_id, quantity, last_updated_at) VALUES
(1, 1, 0, GETDATE()),
(2, 1, 0, GETDATE()),
(3, 1, 0, GETDATE()),
(4, 1, 0, GETDATE()),
(5, 1, 0, GETDATE()),
(6, 1, 0, GETDATE()),
(7, 1, 0, GETDATE()),
(8, 1, 0, GETDATE()),
(1, 2, 0, GETDATE()),
(2, 2, 0, GETDATE()),
(3, 2, 0, GETDATE()),
(4, 2, 0, GETDATE()),
(5, 2, 0, GETDATE()),
(6, 2, 0, GETDATE()),
(7, 2, 0, GETDATE()),
(8, 2, 0, GETDATE());

PRINT N'EWMS Database schema and seed data created successfully!';
GO

