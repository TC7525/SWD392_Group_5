using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using backend.Enums;
using backend.Models;
using backend.Models.DTOs;
using backend.Models.Entities;
using backend.Repositories;

namespace backend.Tests
{
    public class RepositoryAndDtoTests
    {
        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        #region DTO Tests

        [Fact]
        public void PurchaseOrderItemResponse_CalculatedProperties_AreCorrect()
        {
            var item = new PurchaseOrderItemResponse
            {
                PurchaseOrderItemId = 1,
                PurchaseOrderId = 10,
                ProductId = 100,
                ProductCode = "PRD-01",
                ProductName = "Product 1",
                OrderedQuantity = 10,
                ReceivedQuantity = 3,
                UnitPrice = 250000m
            };

            Assert.Equal(7, item.RemainingQuantity);
            Assert.Equal(2500000m, item.LineTotal);
        }

        [Fact]
        public void PurchaseOrderResponse_CalculatedProperties_AreCorrect()
        {
            var response = new PurchaseOrderResponse
            {
                PurchaseOrderId = 1,
                PurchaseOrderCode = "PO-20261106-0001",
                SupplierId = 1,
                Status = POStatus.Processing,
                Items = new List<PurchaseOrderItemResponse>
                {
                    new PurchaseOrderItemResponse
                    {
                        PurchaseOrderItemId = 1,
                        ProductId = 1,
                        OrderedQuantity = 10,
                        ReceivedQuantity = 4,
                        UnitPrice = 1000m
                    },
                    new PurchaseOrderItemResponse
                    {
                        PurchaseOrderItemId = 2,
                        ProductId = 2,
                        OrderedQuantity = 5,
                        ReceivedQuantity = 5,
                        UnitPrice = 2000m
                    }
                }
            };

            Assert.Equal("Processing", response.StatusName);
            Assert.Equal(15, response.TotalOrderedQuantity);
            Assert.Equal(9, response.TotalReceivedQuantity);
            Assert.Equal(6, response.TotalRemainingQuantity);
            Assert.Equal(20000m, response.GrandTotal); // 10*1000 + 5*2000 = 10000 + 10000
        }

        [Fact]
        public void PurchaseOrderListResponse_GrandTotal_Alias_Works()
        {
            var listDto = new PurchaseOrderListResponse
            {
                PurchaseOrderId = 1,
                TotalAmount = 50000m
            };

            Assert.Equal(50000m, listDto.GrandTotal);

            listDto.GrandTotal = 75000m;
            Assert.Equal(75000m, listDto.TotalAmount);
        }

        #endregion

        #region PurchaseOrderRepository Tests

        [Fact]
        public async Task PurchaseOrderRepository_AddAndGetById_IncludesRelatedEntities()
        {
            using var context = CreateInMemoryDbContext();

            var supplier = new Supplier { SupplierId = 1, SupplierCode = "SUP-01", SupplierName = "Supplier One", Status = "Active" };
            var creator = new User { UserId = 1, EmployeeCode = "EMP-01", FullName = "Creator User", Email = "creator@test.com", PasswordHash = "hash" };
            var approver = new User { UserId = 2, EmployeeCode = "EMP-02", FullName = "Approver User", Email = "approver@test.com", PasswordHash = "hash" };
            var category = new Category { CategoryId = 1, CategoryName = "Cat1" };
            var product = new Product { ProductId = 1, ProductCode = "P01", ProductName = "Prod1", CategoryId = 1, UnitPrice = 100, MinimumStockLevel = 5, Status = "Active" };

            context.Suppliers.Add(supplier);
            context.Users.AddRange(creator, approver);
            context.Categories.Add(category);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var po = new PurchaseOrder
            {
                PurchaseOrderId = 10,
                PurchaseOrderCode = "PO-0001",
                SupplierId = 1,
                CreatedBy = 1,
                ApprovedBy = 2,
                Status = POStatus.Approved,
                Items = new List<PurchaseOrderItem>
                {
                    new PurchaseOrderItem
                    {
                        PurchaseOrderItemId = 100,
                        PurchaseOrderId = 10,
                        ProductId = 1,
                        OrderedQuantity = 10,
                        ReceivedQuantity = 0,
                        UnitPrice = 100
                    }
                }
            };

            var repo = new PurchaseOrderRepository(context);
            await repo.AddAsync(po);

            var retrieved = await repo.GetByIdAsync(10);
            Assert.NotNull(retrieved);
            Assert.Equal("PO-0001", retrieved!.PurchaseOrderCode);
            Assert.NotNull(retrieved.Supplier);
            Assert.Equal("Supplier One", retrieved.Supplier!.SupplierName);
            Assert.NotNull(retrieved.Creator);
            Assert.Equal("Creator User", retrieved.Creator!.FullName);
            Assert.NotNull(retrieved.Approver);
            Assert.Equal("Approver User", retrieved.Approver!.FullName);
            Assert.Single(retrieved.Items);
            Assert.NotNull(retrieved.Items.First().Product);
            Assert.Equal("Prod1", retrieved.Items.First().Product!.ProductName);

            var byCode = await repo.GetByCodeAsync("PO-0001");
            Assert.NotNull(byCode);

            var exists = await repo.ExistsCodeAsync("PO-0001");
            Assert.True(exists);

            var count = await repo.GetCountAsync();
            Assert.Equal(1, count);

            var byStatus = await repo.GetByStatusAsync(POStatus.Approved);
            Assert.Single(byStatus);

            var emptyStatus = await repo.GetByStatusAsync(POStatus.Pending);
            Assert.Empty(emptyStatus);
        }

        #endregion

        #region SupplierRepository Tests

        [Fact]
        public async Task SupplierRepository_GetActiveSuppliers_FiltersCorrectly()
        {
            using var context = CreateInMemoryDbContext();
            context.Suppliers.AddRange(
                new Supplier { SupplierId = 1, SupplierCode = "S1", SupplierName = "Beta Active", Status = "Active" },
                new Supplier { SupplierId = 2, SupplierCode = "S2", SupplierName = "Alpha Active", Status = "Active" },
                new Supplier { SupplierId = 3, SupplierCode = "S3", SupplierName = "Inactive Supplier", Status = "Inactive" }
            );
            await context.SaveChangesAsync();

            var repo = new SupplierRepository(context);
            var active = await repo.GetActiveSuppliersAsync();

            Assert.Equal(2, active.Count);
            Assert.Equal("Alpha Active", active[0].SupplierName);
            Assert.Equal("Beta Active", active[1].SupplierName);

            var all = await repo.GetAllAsync();
            Assert.Equal(3, all.Count);
        }

        #endregion

        #region ProductRepository Tests

        [Fact]
        public async Task ProductRepository_GetByIds_ReturnsMatchingProducts()
        {
            using var context = CreateInMemoryDbContext();
            var category = new Category { CategoryId = 1, CategoryName = "Cat1" };
            context.Categories.Add(category);
            context.Products.AddRange(
                new Product { ProductId = 1, ProductCode = "P1", ProductName = "Prod 1", CategoryId = 1, UnitPrice = 10, MinimumStockLevel = 5, Status = "Active" },
                new Product { ProductId = 2, ProductCode = "P2", ProductName = "Prod 2", CategoryId = 1, UnitPrice = 20, MinimumStockLevel = 5, Status = "Active" },
                new Product { ProductId = 3, ProductCode = "P3", ProductName = "Prod 3", CategoryId = 1, UnitPrice = 30, MinimumStockLevel = 5, Status = "Inactive" }
            );
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var result = await repo.GetByIdsAsync(new[] { 1, 3 });

            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.ProductId == 1);
            Assert.Contains(result, p => p.ProductId == 3);

            var active = await repo.GetActiveProductsAsync();
            Assert.Equal(2, active.Count);
        }

        #endregion

        #region InventoryRepository Tests

        [Fact]
        public async Task InventoryRepository_IncreaseStock_CreatesOrUpdates()
        {
            using var context = CreateInMemoryDbContext();
            var warehouse = new Warehouse { WarehouseId = 1, WarehouseCode = "WH01", WarehouseName = "Main WH", Status = "Active" };
            var product = new Product { ProductId = 1, ProductCode = "P1", ProductName = "Prod 1", CategoryId = 1, UnitPrice = 10, MinimumStockLevel = 5, Status = "Active" };
            context.Warehouses.Add(warehouse);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new InventoryRepository(context);

            var initialQty = await repo.GetQuantityAsync(1, 1);
            Assert.Equal(0, initialQty);

            // First increase creates inventory row
            var created = await repo.IncreaseStockAsync(1, 1, 15);
            Assert.Equal(15, created.Quantity);

            var qtyAfterFirst = await repo.GetQuantityAsync(1, 1);
            Assert.Equal(15, qtyAfterFirst);

            // Second increase increments existing row
            var updated = await repo.IncreaseAsync(1, 1, 10);
            Assert.Equal(25, updated.Quantity);

            var qtyAfterSecond = await repo.GetQuantityAsync(1, 1);
            Assert.Equal(25, qtyAfterSecond);

            var details = await repo.GetAllWithDetailsAsync();
            Assert.Single(details);
            Assert.NotNull(details[0].Product);
            Assert.NotNull(details[0].Warehouse);
        }

        #endregion

        #region InventoryTransactionRepository Tests

        [Fact]
        public async Task InventoryTransactionRepository_AddAndQuery_Works()
        {
            using var context = CreateInMemoryDbContext();
            var warehouse = new Warehouse { WarehouseId = 1, WarehouseCode = "WH01", WarehouseName = "Main WH", Status = "Active" };
            var product = new Product { ProductId = 1, ProductCode = "P1", ProductName = "Prod 1", CategoryId = 1, UnitPrice = 10, MinimumStockLevel = 5, Status = "Active" };
            var user = new User { UserId = 1, EmployeeCode = "EMP01", FullName = "Test User", Email = "u@test.com", PasswordHash = "hash" };

            context.Warehouses.Add(warehouse);
            context.Products.Add(product);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new InventoryTransactionRepository(context);

            var tx = new InventoryTransaction
            {
                ProductId = 1,
                WarehouseId = 1,
                UserId = 1,
                TransactionType = TransactionType.IN,
                Quantity = 10,
                TransactionDate = DateTime.UtcNow,
                Note = "Receiving shipment 1"
            };

            await repo.AddAsync(tx);

            var all = await repo.GetAllAsync(10);
            Assert.Single(all);
            Assert.Equal(TransactionType.IN, all[0].TransactionType);
            Assert.Equal(10, all[0].Quantity);
            Assert.NotNull(all[0].Product);
            Assert.NotNull(all[0].Warehouse);
            Assert.NotNull(all[0].User);

            var byProd = await repo.GetByProductAsync(1);
            Assert.Single(byProd);
        }

        #endregion

        #region UserRepository Tests

        [Fact]
        public async Task UserRepository_GetByEmail_IncludesUserRolesAndRole()
        {
            using var context = CreateInMemoryDbContext();

            var role = new Role { RoleId = 1, RoleName = "Warehouse Staff", Status = "Active" };
            var user = new User
            {
                UserId = 1,
                EmployeeCode = "WH-STAFF-01",
                FullName = "Staff Kho",
                Email = "staff_kho@ewms.com",
                PasswordHash = "hash",
                Status = "Active"
            };
            var userRole = new UserRole { UserRoleId = 1, UserId = 1, RoleId = 1 };

            context.Roles.Add(role);
            context.Users.Add(user);
            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var retrieved = await repo.GetByEmailAsync("STAFF_KHO@EWMS.COM");
            Assert.NotNull(retrieved);
            Assert.Equal("Staff Kho", retrieved!.FullName);
            Assert.Single(retrieved.UserRoles);
            Assert.NotNull(retrieved.UserRoles.First().Role);
            Assert.Equal("Warehouse Staff", retrieved.UserRoles.First().Role!.RoleName);

            var retrievedByCode = await repo.GetByEmployeeCodeAsync("WH-STAFF-01");
            Assert.NotNull(retrievedByCode);
        }

        #endregion
    }
}
