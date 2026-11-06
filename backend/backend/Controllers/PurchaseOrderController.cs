using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Common;
using backend.Enums;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/purchase-orders")]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] POStatus? status = null)
        {
            var result = await _purchaseOrderService.GetAllAsync(status);
            return Ok(ApiResponse<List<PurchaseOrderListResponse>>.Success(result, "Success", "200"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _purchaseOrderService.GetOrderDetailAsync(id);
            return Ok(ApiResponse<PurchaseOrderResponse>.Success(result, "Success", "200"));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var result = await _purchaseOrderService.GetPendingOrdersAsync();
            return Ok(ApiResponse<List<PurchaseOrderListResponse>>.Success(result, "Success", "200"));
        }

        [HttpGet("form-data")]
        public async Task<IActionResult> GetFormData()
        {
            var result = await _purchaseOrderService.GetFormDataAsync();
            return Ok(ApiResponse<PurchaseOrderFormDataResponse>.Success(result, "Success", "200"));
        }

        [HttpPost("products/quick-create")]
        public async Task<IActionResult> QuickCreateProduct([FromBody] QuickCreateProductRequest request)
        {
            var result = await _purchaseOrderService.QuickCreateProductAsync(request);
            return Ok(ApiResponse<ProductOptionDto>.Success(result, "Tạo sản phẩm mới thành công.", "MSG10"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreatePurchaseOrderRequest request)
        {
            int userId = GetCurrentUserId(defaultId: 1);
            var result = await _purchaseOrderService.CreateOrderAsync(request, userId);
            return Ok(ApiResponse<PurchaseOrderResponse>.Success(result, "Purchase order created successfully.", "MSG10"));
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            int managerId = GetCurrentUserId(defaultId: 2);
            var result = await _purchaseOrderService.ApproveOrderAsync(id, managerId);
            return Ok(ApiResponse<PurchaseOrderResponse>.Success(result, "Purchase order approved successfully.", "MSG11"));
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectOrder(int id, [FromBody] ReviewPurchaseOrderRequest? request = null)
        {
            int managerId = GetCurrentUserId(defaultId: 2);
            var result = await _purchaseOrderService.RejectOrderAsync(id, managerId, request?.Reason ?? string.Empty);
            return Ok(ApiResponse<PurchaseOrderResponse>.Success(result, "Purchase order rejected successfully.", "MSG11"));
        }

        private int GetCurrentUserId(int defaultId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out int parsedId))
            {
                return parsedId;
            }
            return defaultId;
        }
    }
}
