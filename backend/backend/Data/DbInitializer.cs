using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Models.Entities;

namespace backend.Data
{
    public static class DbInitializer
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public static void Initialize(AppDbContext context)
        {
            InitializeAsync(context).GetAwaiter().GetResult();
        }

        public static async Task InitializeAsync(AppDbContext context)
        {
            var now = DateTime.UtcNow;
            var defaultPasswordHash = HashPassword("123456");

            // 1. Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role
                    {
                        RoleName = "Purchase Staff",
                        Description = "Nhân viên mua hàng - Quản lý NCC & tạo PO",
                        Status = "Active",
                        CreatedAt = now
                    },
                    new Role
                    {
                        RoleName = "Warehouse Manager",
                        Description = "Quản lý kho - Duyệt PO và báo cáo vận hành kho",
                        Status = "Active",
                        CreatedAt = now
                    },
                    new Role
                    {
                        RoleName = "Warehouse Staff",
                        Description = "Thủ kho - Nhập hàng, kiểm hàng, cập nhật tồn kho",
                        Status = "Active",
                        CreatedAt = now
                    },
                    new Role
                    {
                        RoleName = "Administrator",
                        Description = "Quản trị hệ thống",
                        Status = "Active",
                        CreatedAt = now
                    }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // 2. Users
            if (!await context.Users.AnyAsync())
            {
                var users = new List<User>
                {
                    new User
                    {
                        EmployeeCode = "EMP-PUR-01",
                        FullName = "Nguyễn Văn Mua",
                        Email = "staff_purchase@ewms.com",
                        Phone = "0912345671",
                        PasswordHash = defaultPasswordHash,
                        Status = "Active",
                        CreatedAt = now
                    },
                    new User
                    {
                        EmployeeCode = "EMP-MGR-01",
                        FullName = "Trần Quản Lý",
                        Email = "manager_kho@ewms.com",
                        Phone = "0912345672",
                        PasswordHash = defaultPasswordHash,
                        Status = "Active",
                        CreatedAt = now
                    },
                    new User
                    {
                        EmployeeCode = "EMP-STF-01",
                        FullName = "Lê Nhân Viên Kho",
                        Email = "staff_kho@ewms.com",
                        Phone = "0912345673",
                        PasswordHash = defaultPasswordHash,
                        Status = "Active",
                        CreatedAt = now
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }

            // 3. User Roles
            if (!await context.UserRoles.AnyAsync())
            {
                var purchaseRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Purchase Staff");
                var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Warehouse Manager");
                var staffRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Warehouse Staff");

                var purchaseUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "staff_purchase@ewms.com");
                var managerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "manager_kho@ewms.com");
                var staffUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "staff_kho@ewms.com");

                var userRoles = new List<UserRole>();

                if (purchaseUser != null && purchaseRole != null)
                {
                    userRoles.Add(new UserRole
                    {
                        UserId = purchaseUser.UserId,
                        RoleId = purchaseRole.RoleId,
                        AssignedAt = now
                    });
                }

                if (managerUser != null && managerRole != null)
                {
                    userRoles.Add(new UserRole
                    {
                        UserId = managerUser.UserId,
                        RoleId = managerRole.RoleId,
                        AssignedAt = now
                    });
                }

                if (staffUser != null && staffRole != null)
                {
                    userRoles.Add(new UserRole
                    {
                        UserId = staffUser.UserId,
                        RoleId = staffRole.RoleId,
                        AssignedAt = now
                    });
                }

                if (userRoles.Count > 0)
                {
                    await context.UserRoles.AddRangeAsync(userRoles);
                    await context.SaveChangesAsync();
                }
            }

            // 4. Suppliers
            if (!await context.Suppliers.AnyAsync())
            {
                var suppliers = new List<Supplier>
                {
                    new Supplier
                    {
                        SupplierCode = "SUP001",
                        SupplierName = "Công ty TNHH Điện tử Samsung Việt Nam",
                        ContactPerson = "Kim Min Soo",
                        Phone = "0243999888",
                        Email = "contact@samsung.vn",
                        Address = "KCN Yên Phong, Bắc Ninh",
                        Status = "Active",
                        CreatedAt = now
                    },
                    new Supplier
                    {
                        SupplierCode = "SUP002",
                        SupplierName = "Công ty Cổ phần Phân phối Synnex FPT",
                        ContactPerson = "Nguyễn Hoàng Long",
                        Phone = "0287300666",
                        Email = "longnh@synnexfpt.com",
                        Address = "Tòa nhà FPT, Cầu Giấy, Hà Nội",
                        Status = "Active",
                        CreatedAt = now
                    },
                    new Supplier
                    {
                        SupplierCode = "SUP003",
                        SupplierName = "Nhà cung cấp Ngừng Hợp Tác Demo",
                        ContactPerson = "Vũ Đình Tạm",
                        Phone = "0988776655",
                        Email = "inactive_sup@demo.com",
                        Address = "Quận 1, TP.HCM",
                        Status = "Inactive",
                        CreatedAt = now
                    }
                };

                await context.Suppliers.AddRangeAsync(suppliers);
                await context.SaveChangesAsync();
            }

            // 5. Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        CategoryName = "Điện tử & Di động",
                        Description = "Điện thoại thông minh, máy tính bảng và phụ kiện cao cấp",
                        Status = "Active"
                    },
                    new Category
                    {
                        CategoryName = "Máy tính & Laptop",
                        Description = "Máy tính xách tay văn phòng, đồ họa và gaming",
                        Status = "Active"
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 6. Products
            if (!await context.Products.AnyAsync())
            {
                var mobileCat = await context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Điện tử & Di động");
                var laptopCat = await context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Máy tính & Laptop");
                var laptopCategoryId = laptopCat?.CategoryId ?? mobileCat!.CategoryId;

                if (mobileCat != null)
                {
                    var products = new List<Product>
                    {
                        new Product
                        {
                            ProductCode = "PRD-IP16",
                            ProductName = "iPhone 16 Pro Max 256GB Desert Titanium",
                            CategoryId = mobileCat.CategoryId,
                            Brand = "Apple",
                            Description = "Chip A18 Pro, màn hình Super Retina XDR 6.9 inch",
                            UnitPrice = 34990000m,
                            MinimumStockLevel = 10,
                            Status = "Active",
                            CreatedAt = now
                        },
                        new Product
                        {
                            ProductCode = "PRD-S24",
                            ProductName = "Samsung Galaxy S24 Ultra 512GB Titanium Gray",
                            CategoryId = mobileCat.CategoryId,
                            Brand = "Samsung",
                            Description = "Snapdragon 8 Gen 3, Camera 200MP, Galaxy AI",
                            UnitPrice = 29990000m,
                            MinimumStockLevel = 10,
                            Status = "Active",
                            CreatedAt = now
                        },
                        new Product
                        {
                            ProductCode = "PRD-MBA",
                            ProductName = "MacBook Air 13 inch M3 16GB 512GB Space Gray",
                            CategoryId = laptopCategoryId,
                            Brand = "Apple",
                            Description = "Apple M3 8-core CPU, 10-core GPU, Liquid Retina",
                            UnitPrice = 27500000m,
                            MinimumStockLevel = 5,
                            Status = "Active",
                            CreatedAt = now
                        },
                        new Product
                        {
                            ProductCode = "PRD-SNY",
                            ProductName = "Tai nghe Sony WH-1000XM5 Wireless Noise Canceling",
                            CategoryId = mobileCat.CategoryId,
                            Brand = "Sony",
                            Description = "Chống ồn đỉnh cao, thời lượng pin 30 giờ",
                            UnitPrice = 7490000m,
                            MinimumStockLevel = 8,
                            Status = "Active",
                            CreatedAt = now
                        }
                    };

                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                }
            }

            // 7. Warehouses
            if (!await context.Warehouses.AnyAsync())
            {
                var warehouses = new List<Warehouse>
                {
                    new Warehouse
                    {
                        WarehouseCode = "WH-HN-01",
                        WarehouseName = "Kho Tổng Hà Nội",
                        Address = "Số 8 Tôn Thất Thuyết, Mỹ Đình, Hà Nội",
                        Status = "Active",
                        CreatedAt = now
                    },
                    new Warehouse
                    {
                        WarehouseCode = "WH-HCM-01",
                        WarehouseName = "Kho Tổng TP. Hồ Chí Minh",
                        Address = "Khu Công nghệ cao, TP. Thủ Đức, TP.HCM",
                        Status = "Active",
                        CreatedAt = now
                    }
                };

                await context.Warehouses.AddRangeAsync(warehouses);
                await context.SaveChangesAsync();
            }

            // 8. Inventories (Baseline stock: 0)
            if (!await context.Inventories.AnyAsync())
            {
                var allProducts = await context.Products.ToListAsync();
                var allWarehouses = await context.Warehouses.ToListAsync();

                var inventories = new List<Inventory>();
                foreach (var warehouse in allWarehouses)
                {
                    foreach (var product in allProducts)
                    {
                        inventories.Add(new Inventory
                        {
                            ProductId = product.ProductId,
                            WarehouseId = warehouse.WarehouseId,
                            Quantity = 0,
                            LastUpdatedAt = now
                        });
                    }
                }

                if (inventories.Count > 0)
                {
                    await context.Inventories.AddRangeAsync(inventories);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
