using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;
using backend.Common;
using backend.Enums;
using backend.Models;
using backend.Models.DTOs;
using backend.Models.Entities;
using backend.Repositories;
using backend.Services;

namespace backend.Tests
{
    public class ReceivingServiceTests
    {
        private AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }

        private async Task<(AppDbContext Context, ReceivingService Service, InventoryService InvService, PurchaseOrder PO, Warehouse Warehouse, User Staff)> SeedBaseScenarioAsync()
        {
            var context = CreateInMemoryDbContext();

            var staff = new User
            {
                UserId = 1,
                EmployeeCode = "NV001",
                FullName = "Warehouse Staff 1",
                Email = "staff1@inventory.vn",
                PasswordHash = "hash",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(staff);

            var supplier = new Supplier
            {
                SupplierId = 1,
                SupplierCode = "SUP001",
                SupplierName = "Công ty TNHH Apple Việt Nam",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            await context.Suppliers.AddAsync(supplier);

            var warehouse = new Warehouse
            {
                WarehouseId = 1,
                WarehouseCode = "WH-HN",
                WarehouseName = "Kho Tổng Hà Nội",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            await context.Warehouses.AddAsync(warehouse);

            var product1 = new Product
            {
                ProductId = 1,
                ProductCode = "IP15PM",
                ProductName = "iPhone 15 Pro Max 256GB",
                UnitPrice = 34990000m,
                MinimumStockLevel = 10,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            var product2 = new Product
            {
                ProductId = 2,
                ProductCode = "IP15",
                ProductName = "iPhone 15 128GB",
                UnitPrice = 22990000m,
                MinimumStockLevel = 5,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            await context.Products.AddRangeAsync(product1, product2);

            var po = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                PurchaseOrderCode = "PO-20261106-0001",
                SupplierId = supplier.SupplierId,
                Supplier = supplier,
                CreatedBy = staff.UserId,
                Creator = staff,
                Status = POStatus.Approved,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Items = new List<PurchaseOrderItem>
                {
                    new()
                    {
                        PurchaseOrderItemId = 1,
                        PurchaseOrderId = 1,
                        ProductId = product1.ProductId,
                        Product = product1,
                        OrderedQuantity = 10,
                        ReceivedQuantity = 0,
                        UnitPrice = product1.UnitPrice
                    },
                    new()
                    {
                        PurchaseOrderItemId = 2,
                        PurchaseOrderId = 1,
                        ProductId = product2.ProductId,
                        Product = product2,
                        OrderedQuantity = 5,
                        ReceivedQuantity = 0,
                        UnitPrice = product2.UnitPrice
                    }
                }
            };
            await context.PurchaseOrders.AddAsync(po);
            await context.SaveChangesAsync();

            var poRepo = new PurchaseOrderRepository(context);
            var invRepo = new InventoryRepository(context);
            var txRepo = new InventoryTransactionRepository(context);
            var statusService = new POStatusService();

            var receivingService = new ReceivingService(context, poRepo, invRepo, txRepo, statusService);
            var inventoryService = new InventoryService(invRepo, txRepo);

            return (context, receivingService, inventoryService, po, warehouse, staff);
        }

        #region UC12: CheckQuantityAsync Tests

        [Fact]
        public async Task CheckQuantityAsync_QuantityExceedsRemaining_ReturnsInvalidAndMSG09()
        {
            // Arrange
            var (_, service, _, po, _, _) = await SeedBaseScenarioAsync();
            var checkItems = new List<CheckQuantityItemRequest>
            {
                new() { ProductId = 1, Quantity = 15 } // ordered: 10, received: 0 -> remaining: 10 < 15
            };

            // Act
            var result = await service.CheckQuantityAsync(po.PurchaseOrderId, checkItems);

            // Assert
            Assert.False(result.Valid);
            Assert.Contains("MSG09", result.Message);
            var itemResult = Assert.Single(result.Items);
            Assert.False(itemResult.IsValid);
            Assert.Contains("MSG09", itemResult.Message);
            Assert.Equal(10, itemResult.RemainingQuantity);
        }

        [Fact]
        public async Task CheckQuantityAsync_QuantityWithinRemaining_ReturnsValid()
        {
            // Arrange
            var (_, service, _, po, _, _) = await SeedBaseScenarioAsync();
            var checkItems = new List<CheckQuantityItemRequest>
            {
                new() { ProductId = 1, Quantity = 5 },
                new() { ProductId = 2, Quantity = 5 }
            };

            // Act
            var result = await service.CheckQuantityAsync(po.PurchaseOrderId, checkItems);

            // Assert
            Assert.True(result.Valid);
            Assert.All(result.Items, i => Assert.True(i.IsValid));
        }

        [Fact]
        public async Task CheckQuantityAsync_ZeroOrNegativeQuantity_ReturnsInvalidAndMSG02()
        {
            // Arrange
            var (_, service, _, po, _, _) = await SeedBaseScenarioAsync();
            var checkItems = new List<CheckQuantityItemRequest>
            {
                new() { ProductId = 1, Quantity = 0 }
            };

            // Act
            var result = await service.CheckQuantityAsync(po.PurchaseOrderId, checkItems);

            // Assert
            Assert.False(result.Valid);
            Assert.Contains("MSG02", result.Message);
            var itemResult = Assert.Single(result.Items);
            Assert.False(itemResult.IsValid);
            Assert.Contains("MSG02", itemResult.Message);
        }

        [Fact]
        public async Task CheckQuantityAsync_ProductNotInPO_ReturnsInvalidAndMSG05()
        {
            // Arrange
            var (_, service, _, po, _, _) = await SeedBaseScenarioAsync();
            var checkItems = new List<CheckQuantityItemRequest>
            {
                new() { ProductId = 999, Quantity = 5 }
            };

            // Act
            var result = await service.CheckQuantityAsync(po.PurchaseOrderId, checkItems);

            // Assert
            Assert.False(result.Valid);
            Assert.Contains("MSG05", result.Message);
        }

        #endregion

        #region UC13: StartReceivingAsync & CancelReceivingAsync Tests

        [Fact]
        public async Task StartReceivingAsync_ApprovedPO_SetsStatusToProcessing()
        {
            // Arrange
            var (_, service, _, po, _, staff) = await SeedBaseScenarioAsync();

            // Act
            var response = await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            // Assert
            Assert.Equal(POStatus.Processing, response.Status);
        }

        [Fact]
        public async Task StartReceivingAsync_PartiallyReceivedPO_SetsStatusToProcessing()
        {
            // Arrange
            var (context, service, _, po, _, staff) = await SeedBaseScenarioAsync();
            po.Status = POStatus.PartiallyReceived;
            context.PurchaseOrders.Update(po);
            await context.SaveChangesAsync();

            // Act
            var response = await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            // Assert
            Assert.Equal(POStatus.Processing, response.Status);
        }

        [Fact]
        public async Task StartReceivingAsync_NonApprovedPO_ThrowsMSG16()
        {
            // Arrange
            var (context, service, _, po, _, staff) = await SeedBaseScenarioAsync();
            po.Status = POStatus.Pending;
            context.PurchaseOrders.Update(po);
            await context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId));
            Assert.Equal("MSG16", ex.Code);
        }

        [Fact]
        public async Task CancelReceivingAsync_RevertsProcessingToApproved_WhenNoItemsReceived()
        {
            // Arrange
            var (_, service, _, po, _, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            // Act
            var response = await service.CancelReceivingAsync(po.PurchaseOrderId, staff.UserId);

            // Assert
            Assert.Equal(POStatus.Approved, response.Status);
        }

        [Fact]
        public async Task CancelReceivingAsync_RevertsProcessingToPartiallyReceived_WhenSomeItemsReceived()
        {
            // Arrange
            var (context, service, _, po, _, staff) = await SeedBaseScenarioAsync();
            po.Items.First().ReceivedQuantity = 3;
            po.Status = POStatus.Processing;
            context.PurchaseOrders.Update(po);
            await context.SaveChangesAsync();

            // Act
            var response = await service.CancelReceivingAsync(po.PurchaseOrderId, staff.UserId);

            // Assert
            Assert.Equal(POStatus.PartiallyReceived, response.Status);
        }

        #endregion

        #region UC13 & UC14: ConfirmReceiveAsync Tests (Atomic Inventory Update)

        [Fact]
        public async Task ConfirmReceiveAsync_UpdatesReceivedQuantity_IncreasesWarehouseStock_AndCreatesTransaction()
        {
            // Arrange
            var (context, service, _, po, warehouse, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = warehouse.WarehouseId,
                Note = "Received partial shipment batch 1",
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 1, ReceivedQuantity = 4 }
                }
            };

            // Act
            var response = await service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId);

            // Assert PO response & status
            Assert.Equal(POStatus.PartiallyReceived, response.Status);
            var item1 = response.Items.First(i => i.PurchaseOrderItemId == 1);
            Assert.Equal(4, item1.ReceivedQuantity);
            Assert.Equal(6, item1.RemainingQuantity);

            // Assert Inventory stock increased
            var stock = await context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == 1 && i.WarehouseId == warehouse.WarehouseId);
            Assert.NotNull(stock);
            Assert.Equal(4, stock.Quantity);

            // Assert Inventory Transaction logged
            var tx = await context.InventoryTransactions
                .FirstOrDefaultAsync(t => t.PurchaseOrderItemId == 1);
            Assert.NotNull(tx);
            Assert.Equal(TransactionType.IN, tx.TransactionType);
            Assert.Equal(4, tx.Quantity);
            Assert.Equal(warehouse.WarehouseId, tx.WarehouseId);
            Assert.Equal(staff.UserId, tx.UserId);
            Assert.Equal("Received partial shipment batch 1", tx.Note);
        }

        [Fact]
        public async Task ConfirmReceiveAsync_PartialReceive_SetsStatusToPartiallyReceived()
        {
            // Arrange
            var (_, service, _, po, warehouse, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = warehouse.WarehouseId,
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 1, ReceivedQuantity = 5 } // Ordered 10, remaining 5
                }
            };

            // Act
            var response = await service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId);

            // Assert
            Assert.Equal(POStatus.PartiallyReceived, response.Status);
        }

        [Fact]
        public async Task ConfirmReceiveAsync_FullReceive_SetsStatusToCompleted()
        {
            // Arrange
            var (context, service, _, po, warehouse, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = warehouse.WarehouseId,
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 1, ReceivedQuantity = 10 },
                    new() { PurchaseOrderItemId = 2, ReceivedQuantity = 5 }
                }
            };

            // Act
            var response = await service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId);

            // Assert
            Assert.Equal(POStatus.Completed, response.Status);
            Assert.NotNull(response.CompletedDate);

            // Verify both items in inventory
            var stock1 = await context.Inventories.FirstOrDefaultAsync(i => i.ProductId == 1 && i.WarehouseId == warehouse.WarehouseId);
            var stock2 = await context.Inventories.FirstOrDefaultAsync(i => i.ProductId == 2 && i.WarehouseId == warehouse.WarehouseId);
            Assert.Equal(10, stock1!.Quantity);
            Assert.Equal(5, stock2!.Quantity);
        }

        [Fact]
        public async Task ConfirmReceiveAsync_OverReceiving_TriggersRollbackAndThrowsMSG09()
        {
            // Arrange
            var (context, service, _, po, warehouse, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = warehouse.WarehouseId,
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 1, ReceivedQuantity = 4 },
                    new() { PurchaseOrderItemId = 2, ReceivedQuantity = 20 } // Ordered 5, remaining 5 -> over-receive
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId));
            Assert.Equal("MSG09", ex.Code);

            // Verify atomic rollback: NO changes saved to stock or transactions
            var stock1 = await context.Inventories.FirstOrDefaultAsync(i => i.ProductId == 1);
            Assert.Null(stock1); // Item 1 was processed before item 2 threw error; must be rolled back!

            var txCount = await context.InventoryTransactions.CountAsync();
            Assert.Equal(0, txCount);

            var poAfter = await context.PurchaseOrders.Include(p => p.Items).FirstAsync(p => p.PurchaseOrderId == po.PurchaseOrderId);
            Assert.Equal(0, poAfter.Items.First(i => i.PurchaseOrderItemId == 1).ReceivedQuantity);
            Assert.Equal(POStatus.Processing, poAfter.Status);
        }

        [Fact]
        public async Task ConfirmReceiveAsync_ExistingStock_IncrementsStockQuantity()
        {
            // Arrange
            var (context, service, _, po, warehouse, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            // Pre-seed stock of 10 items
            var existingStock = new Inventory
            {
                ProductId = 1,
                WarehouseId = warehouse.WarehouseId,
                Quantity = 10,
                LastUpdatedAt = DateTime.UtcNow.AddDays(-1)
            };
            await context.Inventories.AddAsync(existingStock);
            await context.SaveChangesAsync();

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = warehouse.WarehouseId,
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 1, ReceivedQuantity = 4 }
                }
            };

            // Act
            var response = await service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId);

            // Assert
            var updatedStock = await context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == 1 && i.WarehouseId == warehouse.WarehouseId);
            Assert.NotNull(updatedStock);
            Assert.Equal(14, updatedStock.Quantity);
        }

        [Fact]
        public async Task ConfirmReceiveAsync_EmptyItems_ThrowsMSG01()
        {
            // Arrange
            var (_, service, _, po, warehouse, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = warehouse.WarehouseId,
                Items = new List<ConfirmReceiveItemRequest>()
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId));
            Assert.Equal("MSG01", ex.Code);
        }

        [Fact]
        public async Task ConfirmReceiveAsync_ProductNotFound_ThrowsMSG05()
        {
            // Arrange
            var (_, service, _, po, warehouse, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = warehouse.WarehouseId,
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 999, ReceivedQuantity = 2 }
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId));
            Assert.Equal("MSG05", ex.Code);
        }

        public class FailingDbContext : AppDbContext
        {
            public FailingDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new DbUpdateException("Simulated database failure");
            }
        }

        [Fact]
        public async Task ConfirmReceiveAsync_UnexpectedException_RollsBackAndThrowsMSG17With500()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            using (var initContext = new AppDbContext(options))
            {
                var staff = new User
                {
                    UserId = 1,
                    EmployeeCode = "NV001",
                    FullName = "Warehouse Staff 1",
                    Email = "staff1@inventory.vn",
                    PasswordHash = "hash",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };
                var supplier = new Supplier
                {
                    SupplierId = 1,
                    SupplierCode = "SUP001",
                    SupplierName = "Apple VN",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };
                var warehouse = new Warehouse
                {
                    WarehouseId = 1,
                    WarehouseCode = "WH-HN",
                    WarehouseName = "Kho HN",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };
                var product = new Product
                {
                    ProductId = 1,
                    ProductCode = "IP15PM",
                    ProductName = "iPhone 15",
                    UnitPrice = 1000m,
                    MinimumStockLevel = 1,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };
                var po = new PurchaseOrder
                {
                    PurchaseOrderId = 1,
                    PurchaseOrderCode = "PO-001",
                    SupplierId = 1,
                    CreatedBy = 1,
                    Status = POStatus.Processing,
                    OrderDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<PurchaseOrderItem>
                    {
                        new()
                        {
                            PurchaseOrderItemId = 1,
                            PurchaseOrderId = 1,
                            ProductId = 1,
                            OrderedQuantity = 10,
                            ReceivedQuantity = 0,
                            UnitPrice = 1000m
                        }
                    }
                };
                await initContext.Users.AddAsync(staff);
                await initContext.Suppliers.AddAsync(supplier);
                await initContext.Warehouses.AddAsync(warehouse);
                await initContext.Products.AddAsync(product);
                await initContext.PurchaseOrders.AddAsync(po);
                await initContext.SaveChangesAsync();
            }

            using var failingContext = new FailingDbContext(options);
            var poRepo = new PurchaseOrderRepository(failingContext);
            var invRepo = new InventoryRepository(failingContext);
            var txRepo = new InventoryTransactionRepository(failingContext);
            var statusService = new POStatusService();

            var customService = new ReceivingService(failingContext, poRepo, invRepo, txRepo, statusService);

            var request = new ConfirmReceiveRequest
            {
                WarehouseId = 1,
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 1, ReceivedQuantity = 2 }
                }
            };

            var ex = await Assert.ThrowsAsync<AppException>(() =>
                customService.ConfirmReceiveAsync(1, request, 1));

            Assert.Equal("MSG17", ex.Code);
            Assert.Equal(500, ex.StatusCode);
            Assert.Contains("No inventory changes were saved", ex.Message);
        }

        [Fact]
        public async Task ConfirmReceiveAsync_InvalidWarehouse_RollsBackAndThrowsMSG17()
        {
            // Arrange
            var (context, service, _, po, _, staff) = await SeedBaseScenarioAsync();
            await service.StartReceivingAsync(po.PurchaseOrderId, staff.UserId);

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = 9999, // Non-existent warehouse
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 1, ReceivedQuantity = 5 }
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId));
            Assert.Equal("MSG17", ex.Code);

            var txCount = await context.InventoryTransactions.CountAsync();
            Assert.Equal(0, txCount);
        }

        [Fact]
        public async Task ConfirmReceiveAsync_NotInProcessingStatus_ThrowsMSG04()
        {
            // Arrange
            var (_, service, _, po, warehouse, staff) = await SeedBaseScenarioAsync();
            // Still in Approved, not Processing

            var confirmRequest = new ConfirmReceiveRequest
            {
                WarehouseId = warehouse.WarehouseId,
                Items = new List<ConfirmReceiveItemRequest>
                {
                    new() { PurchaseOrderItemId = 1, ReceivedQuantity = 2 }
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() =>
                service.ConfirmReceiveAsync(po.PurchaseOrderId, confirmRequest, staff.UserId));
            Assert.Equal("MSG04", ex.Code);
        }

        #endregion

        #region GetReceivableOrdersAsync Tests

        [Fact]
        public async Task GetReceivableOrdersAsync_ReturnsApprovedAndPartiallyReceivedOnly()
        {
            // Arrange
            var (context, service, _, po, _, _) = await SeedBaseScenarioAsync();

            var poPending = new PurchaseOrder
            {
                PurchaseOrderId = 2,
                PurchaseOrderCode = "PO-PENDING",
                SupplierId = 1,
                CreatedBy = 1,
                Status = POStatus.Pending,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            var poPartial = new PurchaseOrder
            {
                PurchaseOrderId = 3,
                PurchaseOrderCode = "PO-PARTIAL",
                SupplierId = 1,
                CreatedBy = 1,
                Status = POStatus.PartiallyReceived,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            var poCompleted = new PurchaseOrder
            {
                PurchaseOrderId = 4,
                PurchaseOrderCode = "PO-COMPLETED",
                SupplierId = 1,
                CreatedBy = 1,
                Status = POStatus.Completed,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await context.PurchaseOrders.AddRangeAsync(poPending, poPartial, poCompleted);
            await context.SaveChangesAsync();

            // Act
            var receivable = await service.GetReceivableOrdersAsync();

            // Assert
            Assert.Equal(2, receivable.Count);
            Assert.Contains(receivable, r => r.PurchaseOrderId == po.PurchaseOrderId && r.Status == POStatus.Approved);
            Assert.Contains(receivable, r => r.PurchaseOrderId == poPartial.PurchaseOrderId && r.Status == POStatus.PartiallyReceived);
            Assert.DoesNotContain(receivable, r => r.Status == POStatus.Pending);
            Assert.DoesNotContain(receivable, r => r.Status == POStatus.Completed);
        }

        #endregion

        #region InventoryService Tests

        [Fact]
        public async Task InventoryService_GetStockOverviewAndRecentTransactions_ReturnsMappedData()
        {
            // Arrange
            var (context, _, invService, _, warehouse, staff) = await SeedBaseScenarioAsync();

            var inv = new Inventory
            {
                ProductId = 1,
                WarehouseId = warehouse.WarehouseId,
                Quantity = 15,
                LastUpdatedAt = DateTime.UtcNow
            };
            await context.Inventories.AddAsync(inv);

            var tx = new InventoryTransaction
            {
                ProductId = 1,
                WarehouseId = warehouse.WarehouseId,
                UserId = staff.UserId,
                PurchaseOrderItemId = 1,
                TransactionType = TransactionType.IN,
                Quantity = 15,
                TransactionDate = DateTime.UtcNow,
                Note = "Initial stock"
            };
            await context.InventoryTransactions.AddAsync(tx);
            await context.SaveChangesAsync();

            // Act
            var stockList = await invService.GetStockOverviewAsync();
            var txList = await invService.GetRecentTransactionsAsync(10);

            // Assert
            var stockItem = Assert.Single(stockList);
            Assert.Equal("IP15PM", stockItem.ProductCode);
            Assert.Equal("WH-HN", stockItem.WarehouseCode);
            Assert.Equal(15, stockItem.Quantity);

            var txItem = Assert.Single(txList);
            Assert.Equal("IN", txItem.TransactionTypeName);
            Assert.Equal(15, txItem.Quantity);
            Assert.Equal(staff.FullName, txItem.UserName);
        }

        #endregion
    }
}
