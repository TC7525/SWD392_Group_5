using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;
using backend.Common;
using backend.Controllers;
using backend.Enums;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Tests
{
    public class ControllerTests
    {
        #region AuthController Tests

        [Fact]
        public async Task AuthController_Login_ReturnsOkWithApiResponse()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();
            var loginResponse = new LoginResponse
            {
                Token = "fake-jwt-token",
                User = new UserDto
                {
                    UserId = 1,
                    FullName = "Test Staff",
                    Role = "Purchase Staff"
                }
            };
            var request = new LoginRequest { Email = "staff@example.com", Password = "123" };
            mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(loginResponse);

            var controller = new AuthController(mockAuthService.Object);

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<LoginResponse>>(okResult.Value);
            Assert.Equal("200", apiResponse.Code);
            Assert.Equal("Login successful", apiResponse.Message);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal("fake-jwt-token", apiResponse.Data.Token);
            Assert.Equal(1, apiResponse.Data.User.UserId);
        }

        #endregion

        #region PurchaseOrderController Tests

        [Fact]
        public async Task PurchaseOrderController_GetAll_ReturnsOkWithList()
        {
            // Arrange
            var mockPoService = new Mock<IPurchaseOrderService>();
            var list = new List<PurchaseOrderListResponse>
            {
                new PurchaseOrderListResponse { PurchaseOrderId = 1, PurchaseOrderCode = "PO-2026-0001" }
            };
            mockPoService.Setup(s => s.GetAllAsync(null)).ReturnsAsync(list);

            var controller = new PurchaseOrderController(mockPoService.Object);

            // Act
            var result = await controller.GetAll(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<PurchaseOrderListResponse>>>(okResult.Value);
            Assert.Equal("200", apiResponse.Code);
            Assert.Single(apiResponse.Data!);
            Assert.Equal("PO-2026-0001", apiResponse.Data![0].PurchaseOrderCode);
        }

        [Fact]
        public async Task PurchaseOrderController_GetById_ReturnsOkWithOrder()
        {
            var mockPoService = new Mock<IPurchaseOrderService>();
            var poResponse = new PurchaseOrderResponse { PurchaseOrderId = 5, PurchaseOrderCode = "PO-2026-0005" };
            mockPoService.Setup(s => s.GetOrderDetailAsync(5)).ReturnsAsync(poResponse);

            var controller = new PurchaseOrderController(mockPoService.Object);

            var result = await controller.GetById(5);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderResponse>>(okResult.Value);
            Assert.Equal(5, apiResponse.Data!.PurchaseOrderId);
        }

        [Fact]
        public async Task PurchaseOrderController_GetPendingOrders_ReturnsOkWithList()
        {
            var mockPoService = new Mock<IPurchaseOrderService>();
            mockPoService.Setup(s => s.GetPendingOrdersAsync()).ReturnsAsync(new List<PurchaseOrderListResponse>());

            var controller = new PurchaseOrderController(mockPoService.Object);

            var result = await controller.GetPendingOrders();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<PurchaseOrderListResponse>>>(okResult.Value);
            Assert.NotNull(apiResponse.Data);
        }

        [Fact]
        public async Task PurchaseOrderController_GetFormData_ReturnsOkWithFormData()
        {
            var mockPoService = new Mock<IPurchaseOrderService>();
            var formData = new PurchaseOrderFormDataResponse
            {
                Suppliers = new List<SupplierOptionDto> { new SupplierOptionDto { SupplierId = 1, SupplierName = "Supplier A" } },
                Products = new List<ProductOptionDto> { new ProductOptionDto { ProductId = 1, ProductName = "Product A" } }
            };
            mockPoService.Setup(s => s.GetFormDataAsync()).ReturnsAsync(formData);

            var controller = new PurchaseOrderController(mockPoService.Object);

            var result = await controller.GetFormData();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderFormDataResponse>>(okResult.Value);
            Assert.Single(apiResponse.Data!.Suppliers);
            Assert.Single(apiResponse.Data!.Products);
        }

        [Fact]
        public async Task PurchaseOrderController_CreateOrder_WithClaim_ExtractsUserIdAndReturnsMSG10()
        {
            var mockPoService = new Mock<IPurchaseOrderService>();
            var request = new CreatePurchaseOrderRequest { SupplierId = 1 };
            var poResponse = new PurchaseOrderResponse { PurchaseOrderId = 10, PurchaseOrderCode = "PO-0010" };

            mockPoService.Setup(s => s.CreateOrderAsync(request, 99)).ReturnsAsync(poResponse);

            var controller = new PurchaseOrderController(mockPoService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "99")
            }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await controller.CreateOrder(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderResponse>>(okResult.Value);
            Assert.Equal("MSG10", apiResponse.Code);
            Assert.Equal("Purchase order created successfully.", apiResponse.Message);
            Assert.Equal(10, apiResponse.Data!.PurchaseOrderId);
            mockPoService.Verify(s => s.CreateOrderAsync(request, 99), Times.Once);
        }

        [Fact]
        public async Task PurchaseOrderController_CreateOrder_WithoutClaim_DefaultsToUserId1()
        {
            var mockPoService = new Mock<IPurchaseOrderService>();
            var request = new CreatePurchaseOrderRequest { SupplierId = 1 };
            var poResponse = new PurchaseOrderResponse { PurchaseOrderId = 11, PurchaseOrderCode = "PO-0011" };

            mockPoService.Setup(s => s.CreateOrderAsync(request, 1)).ReturnsAsync(poResponse);

            var controller = new PurchaseOrderController(mockPoService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.CreateOrder(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderResponse>>(okResult.Value);
            Assert.Equal("MSG10", apiResponse.Code);
            mockPoService.Verify(s => s.CreateOrderAsync(request, 1), Times.Once);
        }

        [Fact]
        public async Task PurchaseOrderController_ApproveOrder_ExtractsManagerIdAndReturnsMSG11()
        {
            var mockPoService = new Mock<IPurchaseOrderService>();
            var poResponse = new PurchaseOrderResponse { PurchaseOrderId = 12, Status = POStatus.Approved };

            mockPoService.Setup(s => s.ApproveOrderAsync(12, 77)).ReturnsAsync(poResponse);

            var controller = new PurchaseOrderController(mockPoService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "77")
            }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await controller.ApproveOrder(12);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderResponse>>(okResult.Value);
            Assert.Equal("MSG11", apiResponse.Code);
            Assert.Equal("Purchase order approved successfully.", apiResponse.Message);
            mockPoService.Verify(s => s.ApproveOrderAsync(12, 77), Times.Once);
        }

        [Fact]
        public async Task PurchaseOrderController_RejectOrder_ExtractsManagerIdAndReturnsMSG11()
        {
            var mockPoService = new Mock<IPurchaseOrderService>();
            var poResponse = new PurchaseOrderResponse { PurchaseOrderId = 13, Status = POStatus.Rejected };

            mockPoService.Setup(s => s.RejectOrderAsync(13, 2, "Out of budget")).ReturnsAsync(poResponse);

            var controller = new PurchaseOrderController(mockPoService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.RejectOrder(13, new ReviewPurchaseOrderRequest { Reason = "Out of budget" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderResponse>>(okResult.Value);
            Assert.Equal("MSG11", apiResponse.Code);
            Assert.Equal("Purchase order rejected successfully.", apiResponse.Message);
            mockPoService.Verify(s => s.RejectOrderAsync(13, 2, "Out of budget"), Times.Once);
        }

        #endregion

        #region ReceivingController Tests

        [Fact]
        public async Task ReceivingController_GetReceivableOrders_ReturnsOkWithList()
        {
            var mockReceivingService = new Mock<IReceivingService>();
            mockReceivingService.Setup(s => s.GetReceivableOrdersAsync()).ReturnsAsync(new List<PurchaseOrderListResponse>());

            var controller = new ReceivingController(mockReceivingService.Object);

            var result = await controller.GetReceivableOrders();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<PurchaseOrderListResponse>>>(okResult.Value);
            Assert.Equal("200", apiResponse.Code);
        }

        [Fact]
        public async Task ReceivingController_CheckQuantity_ReturnsOkWithResult()
        {
            var mockReceivingService = new Mock<IReceivingService>();
            var checkResponse = new CheckQuantityResponse { Valid = true, Message = "Valid" };
            var items = new List<CheckQuantityItemRequest> { new CheckQuantityItemRequest { ProductId = 1, Quantity = 5 } };
            mockReceivingService.Setup(s => s.CheckQuantityAsync(1, items)).ReturnsAsync(checkResponse);

            var controller = new ReceivingController(mockReceivingService.Object);

            var result = await controller.CheckQuantity(1, items);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<CheckQuantityResponse>>(okResult.Value);
            Assert.True(apiResponse.Data!.Valid);
        }

        [Fact]
        public async Task ReceivingController_StartReceiving_ReturnsOk()
        {
            var mockReceivingService = new Mock<IReceivingService>();
            var poResponse = new PurchaseOrderResponse { PurchaseOrderId = 20, Status = POStatus.Processing };
            mockReceivingService.Setup(s => s.StartReceivingAsync(20, 88)).ReturnsAsync(poResponse);

            var controller = new ReceivingController(mockReceivingService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "88")
            }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await controller.StartReceiving(20);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderResponse>>(okResult.Value);
            Assert.Equal("200", apiResponse.Code);
            Assert.Equal(POStatus.Processing, apiResponse.Data!.Status);
            mockReceivingService.Verify(s => s.StartReceivingAsync(20, 88), Times.Once);
        }

        [Fact]
        public async Task ReceivingController_CancelReceiving_ReturnsOk()
        {
            var mockReceivingService = new Mock<IReceivingService>();
            var poResponse = new PurchaseOrderResponse { PurchaseOrderId = 20, Status = POStatus.Approved };
            mockReceivingService.Setup(s => s.CancelReceivingAsync(20, 3)).ReturnsAsync(poResponse);

            var controller = new ReceivingController(mockReceivingService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.CancelReceiving(20);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderResponse>>(okResult.Value);
            Assert.Equal("200", apiResponse.Code);
            mockReceivingService.Verify(s => s.CancelReceivingAsync(20, 3), Times.Once);
        }

        [Fact]
        public async Task ReceivingController_ConfirmReceive_ReturnsOkWithMSG12()
        {
            var mockReceivingService = new Mock<IReceivingService>();
            var request = new ConfirmReceiveRequest
            {
                WarehouseId = 1,
                Items = new List<ConfirmReceiveItemRequest> { new ConfirmReceiveItemRequest { PurchaseOrderItemId = 1, ReceivedQuantity = 5 } }
            };
            var poResponse = new PurchaseOrderResponse { PurchaseOrderId = 20, Status = POStatus.Completed };
            mockReceivingService.Setup(s => s.ConfirmReceiveAsync(20, request, 3)).ReturnsAsync(poResponse);

            var controller = new ReceivingController(mockReceivingService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.ConfirmReceive(20, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<PurchaseOrderResponse>>(okResult.Value);
            Assert.Equal("MSG12", apiResponse.Code);
            Assert.Equal("Products received and inventory updated successfully.", apiResponse.Message);
            Assert.Equal(POStatus.Completed, apiResponse.Data!.Status);
            mockReceivingService.Verify(s => s.ConfirmReceiveAsync(20, request, 3), Times.Once);
        }

        #endregion

        #region InventoryController Tests

        [Fact]
        public async Task InventoryController_GetStockOverview_ReturnsOkWithList()
        {
            var mockInventoryService = new Mock<IInventoryService>();
            var stockList = new List<InventoryStockResponse>
            {
                new InventoryStockResponse { ProductId = 1, ProductName = "iPhone 16", Quantity = 25 }
            };
            mockInventoryService.Setup(s => s.GetStockOverviewAsync()).ReturnsAsync(stockList);

            var controller = new InventoryController(mockInventoryService.Object);

            var result = await controller.GetStockOverview();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<InventoryStockResponse>>>(okResult.Value);
            Assert.Equal("200", apiResponse.Code);
            Assert.Single(apiResponse.Data!);
            Assert.Equal("iPhone 16", apiResponse.Data![0].ProductName);
        }

        [Fact]
        public async Task InventoryController_GetRecentTransactions_ReturnsOkWithList()
        {
            var mockInventoryService = new Mock<IInventoryService>();
            var txList = new List<InventoryTransactionResponse>
            {
                new InventoryTransactionResponse { InventoryTransactionId = 1, Quantity = 10, TransactionType = TransactionType.IN }
            };
            mockInventoryService.Setup(s => s.GetRecentTransactionsAsync(25)).ReturnsAsync(txList);

            var controller = new InventoryController(mockInventoryService.Object);

            var result = await controller.GetRecentTransactions(25);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<InventoryTransactionResponse>>>(okResult.Value);
            Assert.Equal("200", apiResponse.Code);
            Assert.Single(apiResponse.Data!);
            Assert.Equal(TransactionType.IN, apiResponse.Data![0].TransactionType);
            mockInventoryService.Verify(s => s.GetRecentTransactionsAsync(25), Times.Once);
        }

        #endregion

        #region GlobalExceptionFilter Tests

        [Fact]
        public void GlobalExceptionFilter_AppException_ReturnsCustomCodeAndStatus()
        {
            var filter = new GlobalExceptionFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new AppException("MSG01", "Please fill in all required fields: Supplier.", 400)
            };

            filter.OnException(exceptionContext);

            Assert.True(exceptionContext.ExceptionHandled);
            var objectResult = Assert.IsType<ObjectResult>(exceptionContext.Result);
            Assert.Equal(400, objectResult.StatusCode);
            var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);
            Assert.Equal("MSG01", response.Code);
            Assert.Equal("Please fill in all required fields: Supplier.", response.Message);
        }

        [Fact]
        public void GlobalExceptionFilter_UnhandledException_Returns500AndMSG17()
        {
            var filter = new GlobalExceptionFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new InvalidOperationException("Unexpected database failure")
            };

            filter.OnException(exceptionContext);

            Assert.True(exceptionContext.ExceptionHandled);
            var objectResult = Assert.IsType<ObjectResult>(exceptionContext.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);
            Assert.Equal("MSG17", response.Code);
            Assert.Equal("The operation could not be completed. No inventory changes were saved.", response.Message);
        }

        #endregion
    }
}
