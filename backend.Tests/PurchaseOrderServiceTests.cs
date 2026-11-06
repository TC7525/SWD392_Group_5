using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using backend.Common;
using backend.Enums;
using backend.Models.DTOs;
using backend.Models.Entities;
using backend.Repositories;
using backend.Services;

namespace backend.Tests
{
    public class PurchaseOrderServiceTests
    {
        private readonly Mock<IPurchaseOrderRepository> _mockPoRepo;
        private readonly Mock<ISupplierRepository> _mockSupplierRepo;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly IPOStatusService _statusService;
        private readonly PurchaseOrderService _service;

        public PurchaseOrderServiceTests()
        {
            _mockPoRepo = new Mock<IPurchaseOrderRepository>();
            _mockSupplierRepo = new Mock<ISupplierRepository>();
            _mockProductRepo = new Mock<IProductRepository>();
            _statusService = new POStatusService();

            _service = new PurchaseOrderService(
                _mockPoRepo.Object,
                _mockSupplierRepo.Object,
                _mockProductRepo.Object,
                _statusService);
        }

        #region CreateOrderAsync Tests (UC09)

        [Fact]
        public async Task CreateOrderAsync_NullRequest_ThrowsMSG01()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateOrderAsync(null!, 10));
            Assert.Equal("MSG01", ex.Code);
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_InactiveSupplier_ThrowsMSG15()
        {
            // Arrange
            var inactiveSupplier = new Supplier
            {
                SupplierId = 1,
                SupplierName = "Inactive Supplier",
                Status = "Inactive"
            };
            _mockSupplierRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inactiveSupplier);

            var request = new CreatePurchaseOrderRequest
            {
                SupplierId = 1,
                Items = new List<CreatePurchaseOrderItemRequest>
                {
                    new() { ProductId = 1, OrderedQuantity = 5, UnitPrice = 1000 }
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateOrderAsync(request, 10));
            Assert.Equal("MSG15", ex.Code);
            Assert.Equal(400, ex.StatusCode);
            Assert.Equal("The selected supplier is inactive and cannot be used.", ex.Message);
        }

        [Fact]
        public async Task CreateOrderAsync_SupplierNotFound_ThrowsMSG15()
        {
            // Arrange
            _mockSupplierRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Supplier?)null);

            var request = new CreatePurchaseOrderRequest
            {
                SupplierId = 999,
                Items = new List<CreatePurchaseOrderItemRequest>
                {
                    new() { ProductId = 1, OrderedQuantity = 5, UnitPrice = 1000 }
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateOrderAsync(request, 10));
            Assert.Equal("MSG15", ex.Code);
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_EmptyItems_ThrowsMSG01()
        {
            // Arrange
            var activeSupplier = new Supplier
            {
                SupplierId = 1,
                SupplierName = "Active Supplier",
                Status = "Active"
            };
            _mockSupplierRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeSupplier);

            var request = new CreatePurchaseOrderRequest
            {
                SupplierId = 1,
                Items = new List<CreatePurchaseOrderItemRequest>()
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateOrderAsync(request, 10));
            Assert.Equal("MSG01", ex.Code);
            Assert.Equal(400, ex.StatusCode);
            Assert.Equal("Required information is missing.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task CreateOrderAsync_OrderedQuantityZeroOrNegative_ThrowsMSG02(int qty)
        {
            // Arrange
            var activeSupplier = new Supplier
            {
                SupplierId = 1,
                SupplierName = "Active Supplier",
                Status = "Active"
            };
            _mockSupplierRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeSupplier);

            var request = new CreatePurchaseOrderRequest
            {
                SupplierId = 1,
                Items = new List<CreatePurchaseOrderItemRequest>
                {
                    new() { ProductId = 1, OrderedQuantity = qty, UnitPrice = 1000 }
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateOrderAsync(request, 10));
            Assert.Equal("MSG02", ex.Code);
            Assert.Equal(400, ex.StatusCode);
            Assert.Equal("Quantity must be greater than 0.", ex.Message);
        }

        [Fact]
        public async Task CreateOrderAsync_NegativeUnitPrice_ThrowsMSG01()
        {
            // Arrange
            var activeSupplier = new Supplier
            {
                SupplierId = 1,
                SupplierName = "Active Supplier",
                Status = "Active"
            };
            _mockSupplierRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeSupplier);

            var request = new CreatePurchaseOrderRequest
            {
                SupplierId = 1,
                Items = new List<CreatePurchaseOrderItemRequest>
                {
                    new() { ProductId = 1, OrderedQuantity = 5, UnitPrice = -100m }
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateOrderAsync(request, 10));
            Assert.Equal("MSG01", ex.Code);
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_ProductNotFound_ThrowsMSG05()
        {
            // Arrange
            var activeSupplier = new Supplier
            {
                SupplierId = 1,
                SupplierName = "Active Supplier",
                Status = "Active"
            };
            _mockSupplierRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeSupplier);
            _mockProductRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            var request = new CreatePurchaseOrderRequest
            {
                SupplierId = 1,
                Items = new List<CreatePurchaseOrderItemRequest>
                {
                    new() { ProductId = 999, OrderedQuantity = 10, UnitPrice = 1000 }
                }
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateOrderAsync(request, 10));
            Assert.Equal("MSG05", ex.Code);
            Assert.Equal(400, ex.StatusCode);
            Assert.Equal("The selected product was not found.", ex.Message);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidRequest_SetsStatusPendingAndGeneratesCode()
        {
            // Arrange
            var activeSupplier = new Supplier
            {
                SupplierId = 1,
                SupplierCode = "SUP-01",
                SupplierName = "Active Supplier",
                Status = "Active"
            };
            var product = new Product
            {
                ProductId = 1,
                ProductCode = "PRD-01",
                ProductName = "iPhone 16",
                UnitPrice = 30000000m,
                Status = "Active"
            };

            _mockSupplierRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeSupplier);
            _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _mockPoRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(0);
            _mockPoRepo.Setup(r => r.ExistsCodeAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockPoRepo.Setup(r => r.AddAsync(It.IsAny<PurchaseOrder>()))
                .ReturnsAsync((PurchaseOrder po) =>
                {
                    po.PurchaseOrderId = 101;
                    return po;
                });

            var request = new CreatePurchaseOrderRequest
            {
                SupplierId = 1,
                ExpectedDeliveryDate = DateTime.UtcNow.AddDays(7),
                Items = new List<CreatePurchaseOrderItemRequest>
                {
                    new() { ProductId = 1, OrderedQuantity = 10, UnitPrice = 30000000m }
                }
            };

            // Act
            var result = await _service.CreateOrderAsync(request, 42);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(POStatus.Pending, result.Status);
            Assert.Equal("Pending", result.StatusName);
            Assert.StartsWith("PO-", result.PurchaseOrderCode);
            Assert.Equal(42, result.CreatedBy);
            Assert.Equal(1, result.SupplierId);
            Assert.Single(result.Items);
            Assert.Equal(10, result.TotalOrderedQuantity);
            Assert.Equal(300000000m, result.GrandTotal);

            _mockPoRepo.Verify(r => r.AddAsync(It.Is<PurchaseOrder>(po =>
                po.Status == POStatus.Pending &&
                po.CreatedBy == 42 &&
                po.SupplierId == 1 &&
                po.Items.Count == 1
            )), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_CodeCollision_GeneratesNextCode()
        {
            // Arrange
            var activeSupplier = new Supplier
            {
                SupplierId = 1,
                SupplierName = "Active Supplier",
                Status = "Active"
            };
            var product = new Product
            {
                ProductId = 1,
                ProductCode = "PRD-01",
                ProductName = "iPhone 16",
                UnitPrice = 30000000m,
                Status = "Active"
            };

            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var collidingCode = $"PO-{dateStr}-0001";
            var nextCode = $"PO-{dateStr}-0002";

            _mockSupplierRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeSupplier);
            _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _mockPoRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(0);
            _mockPoRepo.Setup(r => r.ExistsCodeAsync(collidingCode)).ReturnsAsync(true);
            _mockPoRepo.Setup(r => r.ExistsCodeAsync(nextCode)).ReturnsAsync(false);
            _mockPoRepo.Setup(r => r.AddAsync(It.IsAny<PurchaseOrder>()))
                .ReturnsAsync((PurchaseOrder po) => po);

            var request = new CreatePurchaseOrderRequest
            {
                SupplierId = 1,
                Items = new List<CreatePurchaseOrderItemRequest>
                {
                    new() { ProductId = 1, OrderedQuantity = 1, UnitPrice = 1000m }
                }
            };

            // Act
            var result = await _service.CreateOrderAsync(request, 1);

            // Assert
            Assert.Equal(nextCode, result.PurchaseOrderCode);
        }

        #endregion

        #region ApproveOrderAsync Tests (UC10)

        [Fact]
        public async Task ApproveOrderAsync_PendingPO_UpdatesStatusToApproved()
        {
            // Arrange
            var po = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                PurchaseOrderCode = "PO-20261106-0001",
                SupplierId = 1,
                CreatedBy = 10,
                Status = POStatus.Pending,
                Items = new List<PurchaseOrderItem>
                {
                    new() { ProductId = 1, OrderedQuantity = 5, UnitPrice = 1000m }
                }
            };

            _mockPoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(po);
            _mockPoRepo.Setup(r => r.UpdateAsync(It.IsAny<PurchaseOrder>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.ApproveOrderAsync(1, 99);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(POStatus.Approved, result.Status);
            Assert.Equal(99, result.ApprovedBy);
            Assert.NotNull(result.ApprovedDate);
            _mockPoRepo.Verify(r => r.UpdateAsync(It.Is<PurchaseOrder>(p =>
                p.Status == POStatus.Approved &&
                p.ApprovedBy == 99 &&
                p.ApprovedDate != null
            )), Times.Once);
        }

        [Fact]
        public async Task ApproveOrderAsync_OrderNotFound_Throws404()
        {
            // Arrange
            _mockPoRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PurchaseOrder?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.ApproveOrderAsync(999, 99));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task ApproveOrderAsync_InvalidTransition_ThrowsMSG16()
        {
            // Arrange: PO already approved
            var po = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                PurchaseOrderCode = "PO-20261106-0001",
                SupplierId = 1,
                Status = POStatus.Approved
            };

            _mockPoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(po);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.ApproveOrderAsync(1, 99));
            Assert.Equal("MSG16", ex.Code);
            Assert.Equal(400, ex.StatusCode);
        }

        #endregion

        #region RejectOrderAsync Tests (UC10)

        [Fact]
        public async Task RejectOrderAsync_PendingPO_RecordsReasonAndStatusRejected()
        {
            // Arrange
            var po = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                PurchaseOrderCode = "PO-20261106-0001",
                SupplierId = 1,
                CreatedBy = 10,
                Status = POStatus.Pending
            };

            _mockPoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(po);
            _mockPoRepo.Setup(r => r.UpdateAsync(It.IsAny<PurchaseOrder>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RejectOrderAsync(1, 99, "Gia ca bat thuong");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(POStatus.Rejected, result.Status);
            Assert.Equal(99, result.ApprovedBy);
            Assert.Equal("Gia ca bat thuong", result.RejectionReason);
            _mockPoRepo.Verify(r => r.UpdateAsync(It.Is<PurchaseOrder>(p =>
                p.Status == POStatus.Rejected &&
                p.ApprovedBy == 99 &&
                p.RejectionReason == "Gia ca bat thuong"
            )), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task RejectOrderAsync_EmptyReason_ThrowsMSG01(string? reason)
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.RejectOrderAsync(1, 99, reason!));
            Assert.Equal("MSG01", ex.Code);
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RejectOrderAsync_OrderNotFound_Throws404()
        {
            // Arrange
            _mockPoRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PurchaseOrder?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.RejectOrderAsync(999, 99, "Reason"));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task RejectOrderAsync_InvalidTransition_ThrowsMSG16()
        {
            // Arrange: PO already completed
            var po = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                Status = POStatus.Completed
            };

            _mockPoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(po);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.RejectOrderAsync(1, 99, "Valid reason"));
            Assert.Equal("MSG16", ex.Code);
            Assert.Equal(400, ex.StatusCode);
        }

        #endregion

        #region UC11 & Form Helper Tests

        [Fact]
        public async Task GetAllAsync_ReturnsMappedList()
        {
            // Arrange
            var poList = new List<PurchaseOrder>
            {
                new()
                {
                    PurchaseOrderId = 1,
                    PurchaseOrderCode = "PO-1",
                    Supplier = new Supplier { SupplierName = "Supplier A" },
                    Creator = new User { FullName = "Creator A" },
                    Status = POStatus.Pending,
                    Items = new List<PurchaseOrderItem>
                    {
                        new() { OrderedQuantity = 2, UnitPrice = 100 }
                    }
                }
            };
            _mockPoRepo.Setup(r => r.GetAllAsync(null)).ReturnsAsync(poList);

            // Act
            var result = await _service.GetAllAsync(null);

            // Assert
            Assert.Single(result);
            Assert.Equal("PO-1", result[0].PurchaseOrderCode);
            Assert.Equal("Supplier A", result[0].SupplierName);
            Assert.Equal("Creator A", result[0].CreatedByName);
            Assert.Equal(1, result[0].TotalItems);
            Assert.Equal(2, result[0].TotalQuantity);
            Assert.Equal(200m, result[0].TotalAmount);
        }

        [Fact]
        public async Task GetPendingOrdersAsync_ReturnsOnlyPending()
        {
            // Arrange
            var poList = new List<PurchaseOrder>
            {
                new()
                {
                    PurchaseOrderId = 2,
                    PurchaseOrderCode = "PO-2",
                    Status = POStatus.Pending
                }
            };
            _mockPoRepo.Setup(r => r.GetByStatusAsync(POStatus.Pending)).ReturnsAsync(poList);

            // Act
            var result = await _service.GetPendingOrdersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(POStatus.Pending, result[0].Status);
        }

        [Fact]
        public async Task GetOrderDetailAsync_WhenFound_ReturnsResponse()
        {
            // Arrange
            var po = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                PurchaseOrderCode = "PO-1",
                Status = POStatus.Pending,
                Supplier = new Supplier { SupplierName = "Supplier A" },
                Items = new List<PurchaseOrderItem>
                {
                    new()
                    {
                        ProductId = 10,
                        Product = new Product { ProductCode = "P10", ProductName = "Item 10" },
                        OrderedQuantity = 5,
                        UnitPrice = 200m
                    }
                }
            };
            _mockPoRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(po);

            // Act
            var result = await _service.GetOrderDetailAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PurchaseOrderId);
            Assert.Equal("Supplier A", result.SupplierName);
            Assert.Single(result.Items);
            Assert.Equal("Item 10", result.Items[0].ProductName);
        }

        [Fact]
        public async Task GetOrderDetailAsync_WhenNotFound_Throws404()
        {
            // Arrange
            _mockPoRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PurchaseOrder?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.GetOrderDetailAsync(999));
            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task GetFormDataAsync_ReturnsActiveSuppliersAndProducts()
        {
            // Arrange
            var suppliers = new List<Supplier>
            {
                new() { SupplierId = 1, SupplierCode = "S1", SupplierName = "Supplier 1", Status = "Active" }
            };
            var products = new List<Product>
            {
                new() { ProductId = 1, ProductCode = "P1", ProductName = "Product 1", UnitPrice = 500m, MinimumStockLevel = 5, Status = "Active" }
            };

            _mockSupplierRepo.Setup(r => r.GetActiveSuppliersAsync()).ReturnsAsync(suppliers);
            _mockProductRepo.Setup(r => r.GetActiveProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await _service.GetFormDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Suppliers);
            Assert.Equal("Supplier 1", result.Suppliers[0].SupplierName);
            Assert.Single(result.Products);
            Assert.Equal("Product 1", result.Products[0].ProductName);
        }

        #endregion
    }
}
