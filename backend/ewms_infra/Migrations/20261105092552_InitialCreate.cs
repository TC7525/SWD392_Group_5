using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ewms_infra.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    category_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Mã danh mục"),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Tên danh mục"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả danh mục"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái danh mục")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                },
                comment: "Bảng danh mục sản phẩm điện tử");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    customer_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Mã khách hàng"),
                    customer_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Tên khách hàng"),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Số điện thoại"),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Địa chỉ email"),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Địa chỉ khách hàng"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái khách hàng")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                },
                comment: "Bảng danh mục khách hàng");

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    permission_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false, comment: "Mã quyền"),
                    permission_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Tên quyền"),
                    module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Phân hệ chức năng"),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Hành động"),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Mô tả quyền"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái quyền")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                },
                comment: "Bảng danh mục quyền hạn");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Mã vai trò"),
                    role_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Tên vai trò"),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Mô tả vai trò"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái vai trò")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                },
                comment: "Bảng vai trò người dùng");

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    supplier_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Mã nhà cung cấp"),
                    supplier_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Tên nhà cung cấp"),
                    contact_person = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Người liên hệ"),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Số điện thoại"),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Địa chỉ email"),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Địa chỉ nhà cung cấp"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái nhà cung cấp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.id);
                },
                comment: "Bảng danh mục nhà cung cấp");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    employee_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Mã nhân viên"),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Họ và tên"),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Địa chỉ email"),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Số điện thoại"),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Mật khẩu đã mã hóa"),
                    password_salt = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Chuỗi salt mật khẩu"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái tài khoản")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                },
                comment: "Bảng người dùng hệ thống");

            migrationBuilder.CreateTable(
                name: "warehouses",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    warehouse_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Mã kho"),
                    warehouse_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Tên kho"),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Địa chỉ kho"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái kho")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouses", x => x.id);
                },
                comment: "Bảng danh mục kho hàng");

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    product_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Mã sản phẩm"),
                    product_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false, comment: "Tên sản phẩm"),
                    category_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id danh mục"),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Thương hiệu"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả sản phẩm"),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Đơn giá niêm yết"),
                    minimum_stock_level = table.Column<int>(type: "integer", nullable: false, comment: "Mức tồn kho tối thiểu (cảnh báo)"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái sản phẩm")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng sản phẩm thiết bị điện tử");

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    role_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id vai trò"),
                    permission_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id quyền")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Bảng phân quyền cho vai trò");

            migrationBuilder.CreateTable(
                name: "activity_histories",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id người dùng thực hiện"),
                    module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Phân hệ thực hiện"),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Hành động thực hiện"),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Loại đối tượng tác động"),
                    entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id đối tượng tác động"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả chi tiết hoạt động"),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true, comment: "Địa chỉ IP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_histories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Bảng lịch sử hoạt động hệ thống");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id người dùng"),
                    role_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id vai trò"),
                    assigned_at = table.Column<DateTime>(type: "timestamp", nullable: false, comment: "Thời gian gán vai trò"),
                    assigned_by = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Người gán vai trò")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Bảng phân quyền vai trò cho người dùng");

            migrationBuilder.CreateTable(
                name: "outbound_orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    warehouse_id = table.Column<string>(type: "text", nullable: false, comment: "Mã kho"),
                    order_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Mã đơn xuất kho"),
                    customer_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id khách hàng"),
                    approved_by = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id người phê duyệt"),
                    order_date = table.Column<DateTime>(type: "timestamp", nullable: false, comment: "Ngày tạo đơn xuất"),
                    approved_date = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Ngày phê duyệt"),
                    completed_date = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Ngày hoàn thành xuất kho"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái đơn xuất kho"),
                    total_quantity = table.Column<int>(type: "integer", nullable: false, comment: "Tổng số lượng sản phẩm"),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Tổng tiền đơn xuất"),
                    rejection_reason = table.Column<string>(type: "text", nullable: true, comment: "Lý do từ chối"),
                    note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_outbound_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_outbound_orders_users_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_outbound_orders_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng đơn xuất kho bán hàng");

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    warehouse_id = table.Column<string>(type: "text", nullable: false, comment: "Mã kho"),
                    purchase_order_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Mã đơn mua hàng"),
                    supplier_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id nhà cung cấp"),
                    approved_by = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id người phê duyệt"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái đơn mua hàng"),
                    order_date = table.Column<DateTime>(type: "timestamp", nullable: false, comment: "Ngày đặt hàng"),
                    expected_delivery_date = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Ngày dự kiến giao hàng"),
                    approved_date = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Ngày phê duyệt"),
                    completed_date = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Ngày hoàn thành nhập kho"),
                    rejection_reason = table.Column<string>(type: "text", nullable: true, comment: "Lý do từ chối"),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Tổng tiền đơn mua hàng"),
                    note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_orders_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_users_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_purchase_orders_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng đơn mua hàng (nhập từ nhà cung cấp)");

            migrationBuilder.CreateTable(
                name: "stock_transfers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    stock_transfer_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, comment: "Mã phiếu chuyển kho"),
                    source_warehouse_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id kho xuất (nguồn)"),
                    destination_warehouse_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id kho nhận (đích)"),
                    approved_by = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id người phê duyệt"),
                    dispatched_by = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id nhân viên xuất chuyển kho"),
                    received_by = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id nhân viên nhận chuyển kho"),
                    status = table.Column<int>(type: "integer", nullable: false, comment: "Trạng thái phiếu chuyển kho"),
                    transfer_date = table.Column<DateTime>(type: "timestamp", nullable: false, comment: "Ngày tạo yêu cầu chuyển kho"),
                    approved_date = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Ngày phê duyệt"),
                    dispatched_date = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Ngày xuất kho nguồn"),
                    completed_date = table.Column<DateTime>(type: "timestamp", nullable: true, comment: "Ngày hoàn thành nhận kho đích"),
                    rejection_reason = table.Column<string>(type: "text", nullable: true, comment: "Lý do từ chối"),
                    cancel_reason = table.Column<string>(type: "text", nullable: true, comment: "Lý do hủy"),
                    note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfers", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_transfers_users_approved_by",
                        column: x => x.approved_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stock_transfers_users_dispatched_by",
                        column: x => x.dispatched_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stock_transfers_users_received_by",
                        column: x => x.received_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stock_transfers_warehouses_destination_warehouse_id",
                        column: x => x.destination_warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfers_warehouses_source_warehouse_id",
                        column: x => x.source_warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng phiếu chuyển kho nội bộ");

            migrationBuilder.CreateTable(
                name: "user_warehouses",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id người dùng"),
                    warehouse_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id kho hàng"),
                    assigned_at = table.Column<DateTime>(type: "timestamp", nullable: false, comment: "Thời gian phân công kho")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_warehouses", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_warehouses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_warehouses_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Bảng phân công kho cho nhân sự");

            migrationBuilder.CreateTable(
                name: "inventories",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    warehouse_id = table.Column<string>(type: "text", nullable: false, comment: "Mã kho"),
                    product_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id sản phẩm"),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng tồn kho hiện tại"),
                    last_updated_at = table.Column<DateTime>(type: "timestamp", nullable: false, comment: "Thời gian cập nhật tồn kho gần nhất")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventories", x => x.id);
                    table.ForeignKey(
                        name: "FK_inventories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventories_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng tồn kho theo từng kho và sản phẩm");

            migrationBuilder.CreateTable(
                name: "outbound_order_details",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    order_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id đơn xuất kho"),
                    product_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id sản phẩm"),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng yêu cầu xuất"),
                    issued_quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng đã thực xuất"),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Đơn giá bán")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_order_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_outbound_order_details_outbound_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "outbound_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_outbound_order_details_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng chi tiết đơn xuất kho");

            migrationBuilder.CreateTable(
                name: "purchase_order_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    purchase_order_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id đơn mua hàng"),
                    product_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id sản phẩm"),
                    ordered_quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng đặt mua"),
                    received_quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng đã thực nhập"),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Đơn giá mua")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_order_items_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Bảng chi tiết đơn mua hàng");

            migrationBuilder.CreateTable(
                name: "stock_transfer_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    stock_transfer_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id phiếu chuyển kho"),
                    product_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id sản phẩm"),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng yêu cầu chuyển"),
                    shipped_quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng đã xuất từ kho nguồn"),
                    received_quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng đã nhận tại kho đích"),
                    note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú dòng sản phẩm")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_items_stock_transfers_stock_transfer_id",
                        column: x => x.stock_transfer_id,
                        principalTable: "stock_transfers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Bảng chi tiết phiếu chuyển kho");

            migrationBuilder.CreateTable(
                name: "inventory_transactions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_by = table.Column<string>(type: "text", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    modified_by = table.Column<string>(type: "text", nullable: true),
                    modified_on = table.Column<DateTime>(type: "timestamp", nullable: true),
                    warehouse_id = table.Column<string>(type: "text", nullable: false, comment: "Mã kho"),
                    product_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id sản phẩm"),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Id người thực hiện giao dịch"),
                    purchase_order_item_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id dòng đơn mua hàng (nếu là giao dịch nhập mua)"),
                    outbound_order_detail_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id dòng đơn xuất kho (nếu là giao dịch xuất bán)"),
                    stock_transfer_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true, comment: "Id phiếu chuyển kho (nếu là giao dịch chuyển kho)"),
                    transaction_type = table.Column<int>(type: "integer", nullable: false, comment: "Loại giao dịch kho"),
                    quantity = table.Column<int>(type: "integer", nullable: false, comment: "Số lượng biến động (+ nhập / - xuất)"),
                    transaction_date = table.Column<DateTime>(type: "timestamp", nullable: false, comment: "Thời gian giao dịch"),
                    note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú giao dịch")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_outbound_order_details_outbound_orde~",
                        column: x => x.outbound_order_detail_id,
                        principalTable: "outbound_order_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_purchase_order_items_purchase_order_~",
                        column: x => x.purchase_order_item_id,
                        principalTable: "purchase_order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_stock_transfers_stock_transfer_id",
                        column: x => x.stock_transfer_id,
                        principalTable: "stock_transfers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_transactions_warehouses_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Bảng lịch sử biến động tồn kho");

            migrationBuilder.CreateIndex(
                name: "IX_activity_histories_created_on",
                table: "activity_histories",
                column: "created_on");

            migrationBuilder.CreateIndex(
                name: "IX_activity_histories_deleted",
                table: "activity_histories",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_activity_histories_module",
                table: "activity_histories",
                column: "module");

            migrationBuilder.CreateIndex(
                name: "IX_activity_histories_user_id",
                table: "activity_histories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_category_code",
                table: "categories",
                column: "category_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_category_name",
                table: "categories",
                column: "category_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_deleted",
                table: "categories",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_categories_status",
                table: "categories",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_customers_customer_code",
                table: "customers",
                column: "customer_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_customer_name",
                table: "customers",
                column: "customer_name");

            migrationBuilder.CreateIndex(
                name: "IX_customers_deleted",
                table: "customers",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_customers_status",
                table: "customers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_inventories_deleted",
                table: "inventories",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_inventories_product_id",
                table: "inventories",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventories_product_id_warehouse_id",
                table: "inventories",
                columns: new[] { "product_id", "warehouse_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventories_warehouse_id",
                table: "inventories",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_deleted",
                table: "inventory_transactions",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_outbound_order_detail_id",
                table: "inventory_transactions",
                column: "outbound_order_detail_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_product_id",
                table: "inventory_transactions",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_purchase_order_item_id",
                table: "inventory_transactions",
                column: "purchase_order_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_stock_transfer_id",
                table: "inventory_transactions",
                column: "stock_transfer_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_transaction_date",
                table: "inventory_transactions",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_transaction_type",
                table: "inventory_transactions",
                column: "transaction_type");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_user_id",
                table: "inventory_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_warehouse_id",
                table: "inventory_transactions",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_order_details_deleted",
                table: "outbound_order_details",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_order_details_order_id",
                table: "outbound_order_details",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_order_details_order_id_product_id",
                table: "outbound_order_details",
                columns: new[] { "order_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbound_order_details_product_id",
                table: "outbound_order_details",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_orders_approved_by",
                table: "outbound_orders",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_orders_customer_id",
                table: "outbound_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_orders_deleted",
                table: "outbound_orders",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_orders_order_code",
                table: "outbound_orders",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbound_orders_status",
                table: "outbound_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_orders_warehouse_id",
                table: "outbound_orders",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_deleted",
                table: "permissions",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_module",
                table: "permissions",
                column: "module");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_permission_code",
                table: "permissions",
                column: "permission_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permissions_status",
                table: "permissions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_deleted",
                table: "products",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_products_product_code",
                table: "products",
                column: "product_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_status",
                table: "products",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_deleted",
                table: "purchase_order_items",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_product_id",
                table: "purchase_order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_purchase_order_id",
                table: "purchase_order_items",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_items_purchase_order_id_product_id",
                table: "purchase_order_items",
                columns: new[] { "purchase_order_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_approved_by",
                table: "purchase_orders",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_deleted",
                table: "purchase_orders",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_purchase_order_code",
                table: "purchase_orders",
                column: "purchase_order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_status",
                table: "purchase_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_supplier_id",
                table: "purchase_orders",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_warehouse_id",
                table: "purchase_orders",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_deleted",
                table: "role_permissions",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_id",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_id_permission_id",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_deleted",
                table: "roles",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_roles_role_code",
                table: "roles",
                column: "role_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_role_name",
                table: "roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_status",
                table: "roles",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_items_deleted",
                table: "stock_transfer_items",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_items_product_id",
                table: "stock_transfer_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_items_stock_transfer_id",
                table: "stock_transfer_items",
                column: "stock_transfer_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_items_stock_transfer_id_product_id",
                table: "stock_transfer_items",
                columns: new[] { "stock_transfer_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_approved_by",
                table: "stock_transfers",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_deleted",
                table: "stock_transfers",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_destination_warehouse_id",
                table: "stock_transfers",
                column: "destination_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_dispatched_by",
                table: "stock_transfers",
                column: "dispatched_by");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_received_by",
                table: "stock_transfers",
                column: "received_by");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_source_warehouse_id",
                table: "stock_transfers",
                column: "source_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_status",
                table: "stock_transfers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfers_stock_transfer_code",
                table: "stock_transfers",
                column: "stock_transfer_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_deleted",
                table: "suppliers",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_status",
                table: "suppliers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_supplier_code",
                table: "suppliers",
                column: "supplier_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_supplier_name",
                table: "suppliers",
                column: "supplier_name");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_deleted",
                table: "user_roles",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id_role_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_warehouses_deleted",
                table: "user_warehouses",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_user_warehouses_user_id",
                table: "user_warehouses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_warehouses_user_id_warehouse_id",
                table: "user_warehouses",
                columns: new[] { "user_id", "warehouse_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_warehouses_warehouse_id",
                table: "user_warehouses",
                column: "warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_deleted",
                table: "users",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_employee_code",
                table: "users",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_status",
                table: "users",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_deleted",
                table: "warehouses",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_status",
                table: "warehouses",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_warehouse_code",
                table: "warehouses",
                column: "warehouse_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_warehouse_name",
                table: "warehouses",
                column: "warehouse_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_histories");

            migrationBuilder.DropTable(
                name: "inventories");

            migrationBuilder.DropTable(
                name: "inventory_transactions");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "stock_transfer_items");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_warehouses");

            migrationBuilder.DropTable(
                name: "outbound_order_details");

            migrationBuilder.DropTable(
                name: "purchase_order_items");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "stock_transfers");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "outbound_orders");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "purchase_orders");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "warehouses");
        }
    }
}
