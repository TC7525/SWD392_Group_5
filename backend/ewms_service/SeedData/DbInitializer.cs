using ewms_common.Helpers;
using ewms_common.Infras.Models;
using ewms_infra.Context;
using ewms_infra.Enums;
using ewms_infra.Models.Accounts;
using ewms_infra.Models.Category;
using ewms_infra.Models.Contact;
using ewms_infra.Models.Permission;
using ewms_infra.Models.Product;
using ewms_infra.Models.Repository;
using ewms_service.Consts;
using Microsoft.EntityFrameworkCore;

namespace ewms_service.SeedData
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ewms_context context)
        {
            await context.Database.MigrateAsync();

            // 1. Seed Roles (5 Roles according to SRS Report 1)
            if (!await context.roles.AnyAsync())
            {
                var roles = new List<role>
                {
                    new role
                    {
                        id = "role-admin",
                        role_code = SystemRoles.Administrator,
                        role_name = "Administrator",
                        description = "Quản trị viên hệ thống toàn quyền",
                        status = eStatus_Common.Active
                    }.Created("SYSTEM"),
                    new role
                    {
                        id = "role-wm",
                        role_code = SystemRoles.WarehouseManager,
                        role_name = "Warehouse Manager",
                        description = "Quản lý kho - duyệt đơn mua, đơn xuất, phiếu chuyển kho và xem báo cáo",
                        status = eStatus_Common.Active
                    }.Created("SYSTEM"),
                    new role
                    {
                        id = "role-ws",
                        role_code = SystemRoles.WarehouseStaff,
                        role_name = "Warehouse Staff",
                        description = "Nhân viên kho - thực nhập, thực xuất, xuất/nhận chuyển kho",
                        status = eStatus_Common.Active
                    }.Created("SYSTEM"),
                    new role
                    {
                        id = "role-sales",
                        role_code = SystemRoles.SalesStaff,
                        role_name = "Sales Staff",
                        description = "Nhân viên kinh doanh - quản lý khách hàng và tạo đơn xuất kho",
                        status = eStatus_Common.Active
                    }.Created("SYSTEM"),
                    new role
                    {
                        id = "role-purchase",
                        role_code = SystemRoles.PurchaseStaff,
                        role_name = "Purchase Staff",
                        description = "Nhân viên mua hàng - quản lý nhà cung cấp và tạo đơn mua hàng",
                        status = eStatus_Common.Active
                    }.Created("SYSTEM")
                };

                await context.roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // 2. Seed / Sync Permissions
            var definedPermissions = new List<permission>
            {
                // Dashboard
                new permission { id = "perm-dashboard-view", permission_code = "DASHBOARD.VIEW", permission_name = "Xem bảng tổng quan", module = "DASHBOARD", action = "VIEW" }.Created("SYSTEM"),

                // Hàng hóa
                new permission { id = "perm-product-view", permission_code = "PRODUCT.VIEW", permission_name = "Xem danh mục hàng hóa", module = "PRODUCT", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-product-manage", permission_code = "PRODUCT.MANAGE", permission_name = "Thêm, sửa, xóa hàng hóa", module = "PRODUCT", action = "MANAGE" }.Created("SYSTEM"),
                new permission { id = "perm-product-export", permission_code = "PRODUCT.EXPORT", permission_name = "In & Xuất Excel hàng hóa", module = "PRODUCT", action = "EXPORT" }.Created("SYSTEM"),

                // Kho hàng
                new permission { id = "perm-warehouse-view", permission_code = "WAREHOUSE.VIEW", permission_name = "Xem danh sách kho & khu vực", module = "WAREHOUSE", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-warehouse-manage", permission_code = "WAREHOUSE.MANAGE", permission_name = "Thêm, sửa, xóa kho hàng", module = "WAREHOUSE", action = "MANAGE" }.Created("SYSTEM"),
                new permission { id = "perm-warehouse-export", permission_code = "WAREHOUSE.EXPORT", permission_name = "In & Xuất Excel kho hàng", module = "WAREHOUSE", action = "EXPORT" }.Created("SYSTEM"),

                // Đối tượng (NCC & Khách hàng)
                new permission { id = "perm-partner-view", permission_code = "PARTNER.VIEW", permission_name = "Xem NCC & Khách hàng", module = "PARTNER", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-partner-manage", permission_code = "PARTNER.MANAGE", permission_name = "Thêm, sửa, xóa NCC & Khách hàng", module = "PARTNER", action = "MANAGE" }.Created("SYSTEM"),
                new permission { id = "perm-partner-export", permission_code = "PARTNER.EXPORT", permission_name = "In & Xuất Excel NCC & Khách hàng", module = "PARTNER", action = "EXPORT" }.Created("SYSTEM"),

                // Nhập kho (PO)
                new permission { id = "perm-po-view", permission_code = "PO.VIEW", permission_name = "Xem đơn nhập kho", module = "PURCHASE_ORDER", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-po-manage", permission_code = "PO.MANAGE", permission_name = "Tạo, sửa, xóa đơn nhập kho", module = "PURCHASE_ORDER", action = "MANAGE" }.Created("SYSTEM"),
                new permission { id = "perm-po-export", permission_code = "PO.EXPORT", permission_name = "In & Xuất Excel đơn nhập kho", module = "PURCHASE_ORDER", action = "EXPORT" }.Created("SYSTEM"),
                new permission { id = "perm-po-approve", permission_code = "PO.APPROVE", permission_name = "Phê duyệt đơn nhập kho", module = "PURCHASE_ORDER", action = "APPROVE" }.Created("SYSTEM"),
                new permission { id = "perm-po-create", permission_code = "PO.CREATE", permission_name = "Tạo đơn mua hàng", module = "PURCHASE_ORDER", action = "CREATE" }.Created("SYSTEM"),
                new permission { id = "perm-po-receive", permission_code = "PO.RECEIVE", permission_name = "Thực nhập hàng vào kho", module = "PURCHASE_ORDER", action = "RECEIVE" }.Created("SYSTEM"),

                // Xuất kho (SO)
                new permission { id = "perm-so-view", permission_code = "SO.VIEW", permission_name = "Xem đơn xuất kho", module = "OUTBOUND_ORDER", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-so-manage", permission_code = "SO.MANAGE", permission_name = "Tạo, sửa, xóa đơn xuất kho", module = "OUTBOUND_ORDER", action = "MANAGE" }.Created("SYSTEM"),
                new permission { id = "perm-so-export", permission_code = "SO.EXPORT", permission_name = "In & Xuất Excel đơn xuất kho", module = "OUTBOUND_ORDER", action = "EXPORT" }.Created("SYSTEM"),
                new permission { id = "perm-so-approve", permission_code = "SO.APPROVE", permission_name = "Phê duyệt đơn xuất kho", module = "OUTBOUND_ORDER", action = "APPROVE" }.Created("SYSTEM"),
                new permission { id = "perm-so-create", permission_code = "SO.CREATE", permission_name = "Tạo đơn xuất kho", module = "OUTBOUND_ORDER", action = "CREATE" }.Created("SYSTEM"),
                new permission { id = "perm-so-issue", permission_code = "SO.ISSUE", permission_name = "Thực xuất hàng khỏi kho", module = "OUTBOUND_ORDER", action = "ISSUE" }.Created("SYSTEM"),

                // Chuyển kho (TRF)
                new permission { id = "perm-trf-view", permission_code = "TRF.VIEW", permission_name = "Xem yêu cầu chuyển kho", module = "STOCK_TRANSFER", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-trf-manage", permission_code = "TRF.MANAGE", permission_name = "Tạo, sửa, xóa yêu cầu chuyển kho", module = "STOCK_TRANSFER", action = "MANAGE" }.Created("SYSTEM"),
                new permission { id = "perm-trf-export", permission_code = "TRF.EXPORT", permission_name = "In & Xuất Excel phiếu chuyển kho", module = "STOCK_TRANSFER", action = "EXPORT" }.Created("SYSTEM"),
                new permission { id = "perm-trf-approve", permission_code = "TRF.APPROVE", permission_name = "Phê duyệt chuyển kho", module = "STOCK_TRANSFER", action = "APPROVE" }.Created("SYSTEM"),
                new permission { id = "perm-trf-create", permission_code = "TRF.CREATE", permission_name = "Tạo yêu cầu chuyển kho", module = "STOCK_TRANSFER", action = "CREATE" }.Created("SYSTEM"),
                new permission { id = "perm-trf-dispatch", permission_code = "TRF.DISPATCH", permission_name = "Xuất chuyển kho", module = "STOCK_TRANSFER", action = "DISPATCH" }.Created("SYSTEM"),
                new permission { id = "perm-trf-receive", permission_code = "TRF.RECEIVE", permission_name = "Nhận chuyển kho", module = "STOCK_TRANSFER", action = "RECEIVE" }.Created("SYSTEM"),

                // Tồn kho (INVENTORY)
                new permission { id = "perm-inventory-view", permission_code = "INVENTORY.VIEW", permission_name = "Xem thông tin tồn kho & thẻ kho", module = "INVENTORY", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-inventory-manage", permission_code = "INVENTORY.MANAGE", permission_name = "Điều chỉnh & kiểm kê tồn kho", module = "INVENTORY", action = "MANAGE" }.Created("SYSTEM"),
                new permission { id = "perm-inventory-export", permission_code = "INVENTORY.EXPORT", permission_name = "In & Xuất Excel tồn kho", module = "INVENTORY", action = "EXPORT" }.Created("SYSTEM"),

                // Báo cáo N-X-T (REPORT)
                new permission { id = "perm-report-view", permission_code = "REPORT.VIEW", permission_name = "Xem thống kê & báo cáo N-X-T", module = "REPORT", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-report-export", permission_code = "REPORT.EXPORT", permission_name = "Xuất Excel báo cáo N-X-T", module = "REPORT", action = "EXPORT" }.Created("SYSTEM"),

                // Lịch sử & Nhật ký (AUDIT_LOG)
                new permission { id = "perm-audit-view", permission_code = "AUDIT_LOG.VIEW", permission_name = "Xem lịch sử thao tác & nhật ký truy cập", module = "AUDIT_LOG", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-audit-export", permission_code = "AUDIT_LOG.EXPORT", permission_name = "Xuất Excel nhật ký truy cập", module = "AUDIT_LOG", action = "EXPORT" }.Created("SYSTEM"),

                // Người dùng & Danh mục
                new permission { id = "perm-user-view", permission_code = "USER.VIEW", permission_name = "Xem danh sách người dùng & vai trò", module = "USER", action = "VIEW" }.Created("SYSTEM"),
                new permission { id = "perm-user-manage", permission_code = "USER.MANAGE", permission_name = "Quản lý người dùng & phân quyền", module = "USER", action = "MANAGE" }.Created("SYSTEM"),
                new permission { id = "perm-master-manage", permission_code = "MASTER.MANAGE", permission_name = "Quản lý dữ liệu danh mục", module = "MASTER", action = "MANAGE" }.Created("SYSTEM")
            };

            var existingPermCodes = await context.permissions.Select(p => p.permission_code).ToListAsync();
            foreach (var p in definedPermissions)
            {
                if (!existingPermCodes.Contains(p.permission_code))
                {
                    await context.permissions.AddAsync(p);
                }
            }
            await context.SaveChangesAsync();

            // 2.1. Seed Default Role Permissions
            if (!await context.role_permissions.AnyAsync())
            {
                var allDbPerms = await context.permissions.AsNoTracking().ToListAsync();
                var permMap = allDbPerms.ToDictionary(p => p.permission_code, p => p.id);

                var rolePermsToSeed = new List<role_permission>();

                // Helper to add role permissions safely
                void AddPermsToRole(string roleId, IEnumerable<string> codes)
                {
                    foreach (var code in codes)
                    {
                        if (permMap.TryGetValue(code, out var permId))
                        {
                            rolePermsToSeed.Add(new role_permission
                            {
                                role_id = roleId,
                                permission_id = permId
                            }.Created("SYSTEM"));
                        }
                    }
                }

                // 1. ADMIN - All permissions
                AddPermsToRole("role-admin", allDbPerms.Select(p => p.permission_code));

                // 2. WAREHOUSE MANAGER - Toàn quyền quản lý kho, duyệt phiếu, xuất báo cáo
                AddPermsToRole("role-wm", new[]
                {
                    "DASHBOARD.VIEW",
                    "PRODUCT.VIEW", "PRODUCT.MANAGE", "PRODUCT.EXPORT",
                    "WAREHOUSE.VIEW", "WAREHOUSE.MANAGE", "WAREHOUSE.EXPORT",
                    "PARTNER.VIEW", "PARTNER.EXPORT",
                    "PO.VIEW", "PO.MANAGE", "PO.EXPORT", "PO.APPROVE", "PO.CREATE", "PO.RECEIVE",
                    "SO.VIEW", "SO.MANAGE", "SO.EXPORT", "SO.APPROVE", "SO.CREATE", "SO.ISSUE",
                    "TRF.VIEW", "TRF.MANAGE", "TRF.EXPORT", "TRF.APPROVE", "TRF.CREATE", "TRF.DISPATCH", "TRF.RECEIVE",
                    "INVENTORY.VIEW", "INVENTORY.MANAGE", "INVENTORY.EXPORT",
                    "REPORT.VIEW", "REPORT.EXPORT",
                    "AUDIT_LOG.VIEW", "AUDIT_LOG.EXPORT",
                    "USER.VIEW"
                });

                // 3. WAREHOUSE STAFF - Nghiệp vụ kho, kiểm đếm, in phiếu, không có quyền duyệt
                AddPermsToRole("role-ws", new[]
                {
                    "DASHBOARD.VIEW",
                    "PRODUCT.VIEW", "PRODUCT.EXPORT",
                    "WAREHOUSE.VIEW",
                    "PARTNER.VIEW",
                    "PO.VIEW", "PO.MANAGE", "PO.EXPORT", "PO.RECEIVE",
                    "SO.VIEW", "SO.MANAGE", "SO.EXPORT", "SO.ISSUE",
                    "TRF.VIEW", "TRF.MANAGE", "TRF.EXPORT", "TRF.DISPATCH", "TRF.RECEIVE",
                    "INVENTORY.VIEW", "INVENTORY.MANAGE", "INVENTORY.EXPORT",
                    "REPORT.VIEW", "REPORT.EXPORT"
                });

                // 4. SALES STAFF - Quản lý khách hàng, tạo đơn xuất kho
                AddPermsToRole("role-sales", new[]
                {
                    "DASHBOARD.VIEW",
                    "PRODUCT.VIEW", "PRODUCT.EXPORT",
                    "PARTNER.VIEW", "PARTNER.MANAGE", "PARTNER.EXPORT",
                    "SO.VIEW", "SO.MANAGE", "SO.EXPORT", "SO.CREATE",
                    "INVENTORY.VIEW", "INVENTORY.EXPORT",
                    "REPORT.VIEW", "REPORT.EXPORT"
                });

                // 5. PURCHASE STAFF - Quản lý nhà cung cấp, tạo đơn mua hàng
                AddPermsToRole("role-purchase", new[]
                {
                    "DASHBOARD.VIEW",
                    "PRODUCT.VIEW", "PRODUCT.EXPORT",
                    "PARTNER.VIEW", "PARTNER.MANAGE", "PARTNER.EXPORT",
                    "PO.VIEW", "PO.MANAGE", "PO.EXPORT", "PO.CREATE",
                    "INVENTORY.VIEW", "INVENTORY.EXPORT",
                    "REPORT.VIEW", "REPORT.EXPORT"
                });

                if (rolePermsToSeed.Any())
                {
                    await context.role_permissions.AddRangeAsync(rolePermsToSeed);
                    await context.SaveChangesAsync();
                }
            }

            // 3. Seed Default Warehouses (Multi-warehouse: Hà Nội & TP.HCM)
            if (!await context.warehouses.AnyAsync())
            {
                var warehouses = new List<warehouse>
                {
                    new warehouse
                    {
                        id = "wh-hn-01",
                        warehouse_code = "WH-HN",
                        warehouse_name = "Kho Trung Tâm Hà Nội",
                        address = "Khu Công Nghệ Cao Hòa Lạc, Thạch Thất, Hà Nội",
                        status = eStatus_Warehouse.Active
                    }.Created("SYSTEM"),
                    new warehouse
                    {
                        id = "wh-hcm-01",
                        warehouse_code = "WH-HCM",
                        warehouse_name = "Kho Trung Tâm TP.HCM",
                        address = "Khu Công Nghệ Cao Quận 9, TP. Thủ Đức, TP.HCM",
                        status = eStatus_Warehouse.Active
                    }.Created("SYSTEM")
                };

                await context.warehouses.AddRangeAsync(warehouses);
                await context.SaveChangesAsync();
            }

            // 4. Seed Default Users for each of the 5 Roles (Password: Admin@123)
            if (!await context.users.AnyAsync())
            {
                var salt = HashingHelper.GenerateSalt();
                var passwordHash = HashingHelper.HashPassword("Admin@123", salt);

                var users = new List<user>
                {
                    new user
                    {
                        id = "user-admin",
                        employee_code = "EMP001",
                        full_name = "Quản Trị Hệ Thống",
                        email = "admin@ewms.vn",
                        phone = "0901000001",
                        password_hash = passwordHash,
                        password_salt = salt,
                        status = eStatus_Account.Active
                    }.Created("SYSTEM"),
                    new user
                    {
                        id = "user-wm",
                        employee_code = "EMP002",
                        full_name = "Nguyễn Văn Quản Lý Kho",
                        email = "manager@ewms.vn",
                        phone = "0901000002",
                        password_hash = passwordHash,
                        password_salt = salt,
                        status = eStatus_Account.Active
                    }.Created("SYSTEM"),
                    new user
                    {
                        id = "user-ws",
                        employee_code = "EMP003",
                        full_name = "Trần Thị Nhân Viên Kho",
                        email = "staff@ewms.vn",
                        phone = "0901000003",
                        password_hash = passwordHash,
                        password_salt = salt,
                        status = eStatus_Account.Active
                    }.Created("SYSTEM"),
                    new user
                    {
                        id = "user-sales",
                        employee_code = "EMP004",
                        full_name = "Lê Hoàng Kinh Doanh",
                        email = "sales@ewms.vn",
                        phone = "0901000004",
                        password_hash = passwordHash,
                        password_salt = salt,
                        status = eStatus_Account.Active
                    }.Created("SYSTEM"),
                    new user
                    {
                        id = "user-purchase",
                        employee_code = "EMP005",
                        full_name = "Phạm Minh Mua Hàng",
                        email = "purchase@ewms.vn",
                        phone = "0901000005",
                        password_hash = passwordHash,
                        password_salt = salt,
                        status = eStatus_Account.Active
                    }.Created("SYSTEM")
                };

                await context.users.AddRangeAsync(users);

                var userRoles = new List<user_role>
                {
                    new user_role { user_id = "user-admin", role_id = "role-admin", assigned_by = "SYSTEM" }.Created("SYSTEM"),
                    new user_role { user_id = "user-wm", role_id = "role-wm", assigned_by = "SYSTEM" }.Created("SYSTEM"),
                    new user_role { user_id = "user-ws", role_id = "role-ws", assigned_by = "SYSTEM" }.Created("SYSTEM"),
                    new user_role { user_id = "user-sales", role_id = "role-sales", assigned_by = "SYSTEM" }.Created("SYSTEM"),
                    new user_role { user_id = "user-purchase", role_id = "role-purchase", assigned_by = "SYSTEM" }.Created("SYSTEM")
                };

                await context.user_roles.AddRangeAsync(userRoles);

                // Assign both warehouses to default accounts so testing works seamlessly out-of-the-box
                var userWarehouses = new List<user_warehouse>();
                foreach (var u in users)
                {
                    userWarehouses.Add(new user_warehouse { user_id = u.id, warehouse_id = "wh-hn-01" }.Created("SYSTEM"));
                    userWarehouses.Add(new user_warehouse { user_id = u.id, warehouse_id = "wh-hcm-01" }.Created("SYSTEM"));
                }

                await context.user_warehouses.AddRangeAsync(userWarehouses);
                await context.SaveChangesAsync();
            }

            // 5. Seed Sample Electronics Categories, Products, Suppliers, Customers & Initial Inventory
            if (!await context.categories.AnyAsync())
            {
                var categories = new List<category>
                {
                    new category { id = "cat-laptop", category_code = "CAT-LAPTOP", category_name = "Laptop & Macbook", description = "Máy tính xách tay văn phòng, gaming, đồ họa", status = eStatus_Category.Active }.Created("SYSTEM"),
                    new category { id = "cat-comp", category_code = "CAT-COMP", category_name = "Linh kiện máy tính", description = "CPU, RAM, SSD, VGA, Mainboard", status = eStatus_Category.Active }.Created("SYSTEM"),
                    new category { id = "cat-mon", category_code = "CAT-MON", category_name = "Màn hình & Thiết bị ngoại vi", description = "Màn hình máy tính, bàn phím cơ, chuột gaming", status = eStatus_Category.Active }.Created("SYSTEM")
                };
                await context.categories.AddRangeAsync(categories);

                var products = new List<product>
                {
                    new product { id = "prd-01", product_code = "PRD-DELL-5530", product_name = "Laptop Dell Inspiron 15 5530 i7-1360P/16GB/512GB", category_id = "cat-laptop", brand = "Dell", unit_price = 21500000m, minimum_stock_level = 10, status = eStatus_Product.Active }.Created("SYSTEM"),
                    new product { id = "prd-02", product_code = "PRD-ASUS-ROG", product_name = "Laptop ASUS ROG Strix G16 i9-14900HX/32GB/1TB/RTX4070", category_id = "cat-laptop", brand = "ASUS", unit_price = 45900000m, minimum_stock_level = 5, status = eStatus_Product.Active }.Created("SYSTEM"),
                    new product { id = "prd-03", product_code = "PRD-SS-990PRO", product_name = "Ổ cứng SSD Samsung 990 PRO PCIe Gen 4.0 NVMe 1TB", category_id = "cat-comp", brand = "Samsung", unit_price = 2850000m, minimum_stock_level = 20, status = eStatus_Product.Active }.Created("SYSTEM"),
                    new product { id = "prd-04", product_code = "PRD-LG-27GP850", product_name = "Màn hình LG UltraGear 27GP850-B 27 inch 2K Nano IPS 180Hz", category_id = "cat-mon", brand = "LG", unit_price = 7900000m, minimum_stock_level = 8, status = eStatus_Product.Active }.Created("SYSTEM")
                };
                await context.products.AddRangeAsync(products);

                var suppliers = new List<supplier>
                {
                    new supplier { id = "sup-01", supplier_code = "SUP-SYNNEX", supplier_name = "Công ty Cổ phần Phân phối Synnex FPT", contact_person = "Nguyễn Hoàng Nam", phone = "02473006666", email = "contact@synnexfpt.com", address = "Số 17 Duy Tân, Cầu Giấy, Hà Nội", status = eStatus_Supplier.Active }.Created("SYSTEM"),
                    new supplier { id = "sup-02", supplier_code = "SUP-DIGIWORLD", supplier_name = "Công ty Cổ phần Thế Giới Số (Digiworld)", contact_person = "Trần Minh Tuấn", phone = "02839290059", email = "info@dgw.com.vn", address = "195-197 Nguyễn Thái Bình, Quận 1, TP.HCM", status = eStatus_Supplier.Active }.Created("SYSTEM")
                };
                await context.suppliers.AddRangeAsync(suppliers);

                var customers = new List<customer>
                {
                    new customer { id = "cus-01", customer_code = "CUS-PHONGVU", customer_name = "Hệ thống Bán lẻ Điện máy Phong Vũ", phone = "18006867", email = "purchasing@phongvu.vn", address = "2A Nguyễn Thị Minh Khai, Quận 1, TP.HCM", status = eStatus_Common.Active }.Created("SYSTEM"),
                    new customer { id = "cus-02", customer_code = "CUS-HACOM", customer_name = "Công ty Cổ phần Máy tính Hà Nội (HACOM)", phone = "19001903", email = "b2b@hacom.vn", address = "131 Lê Thanh Nghị, Hai Bà Trưng, Hà Nội", status = eStatus_Common.Active }.Created("SYSTEM")
                };
                await context.customers.AddRangeAsync(customers);

                var inventories = new List<inventory>
                {
                    new inventory { product_id = "prd-01", warehouse_id = "wh-hn-01", quantity = 25, last_updated_at = DateTime.Now }.Created("SYSTEM"),
                    new inventory { product_id = "prd-02", warehouse_id = "wh-hn-01", quantity = 3, last_updated_at = DateTime.Now }.Created("SYSTEM"), // Below minimum_stock_level (5) for low-stock alert testing
                    new inventory { product_id = "prd-03", warehouse_id = "wh-hn-01", quantity = 50, last_updated_at = DateTime.Now }.Created("SYSTEM"),
                    new inventory { product_id = "prd-04", warehouse_id = "wh-hn-01", quantity = 15, last_updated_at = DateTime.Now }.Created("SYSTEM"),
                    new inventory { product_id = "prd-01", warehouse_id = "wh-hcm-01", quantity = 18, last_updated_at = DateTime.Now }.Created("SYSTEM"),
                    new inventory { product_id = "prd-03", warehouse_id = "wh-hcm-01", quantity = 12, last_updated_at = DateTime.Now }.Created("SYSTEM") // Below minimum_stock_level (20)
                };
                await context.inventories.AddRangeAsync(inventories);

                await context.SaveChangesAsync();
            }
        }
    }
}
