using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Common;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/receiving")]
    public class ReceivingController : ControllerBase
    {
        private readonly IReceivingService _receivingService;

        public ReceivingController(IReceivingService receivingService)
        {
            _receivingService = receivingService;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetReceivableOrders()
        {
            var result = await _receivingService.GetReceivableOrdersAsync();
            return Ok(ApiResponse<List<PurchaseOrderListResponse>>.Success(result, "Success", "200"));
        }

        [HttpPost("{poId}/check-quantity")]
        public async Task<IActionResult> CheckQuantity(int poId, [FromBody] List<CheckQuantityItemRequest> items)
        {
            var result = await _receivingService.CheckQuantityAsync(poId, items);
            return Ok(ApiResponse<CheckQuantityResponse>.Success(result, "Quantity checked successfully.", "200"));
        }

        [HttpPost("{poId}/start")]
        public async Task<IActionResult> StartReceiving(int poId)
        {
            int staffId = GetCurrentUserId(defaultId: 3);
            var result = await _receivingService.StartReceivingAsync(poId, staffId);
            return Ok(ApiResponse<PurchaseOrderResponse>.Success(result, "Receiving started successfully.", "200"));
        }

        [HttpPost("{poId}/cancel")]
        public async Task<IActionResult> CancelReceiving(int poId)
        {
            int staffId = GetCurrentUserId(defaultId: 3);
            var result = await _receivingService.CancelReceivingAsync(poId, staffId);
            return Ok(ApiResponse<PurchaseOrderResponse>.Success(result, "Receiving cancelled successfully.", "200"));
        }

        [HttpPost("{poId}/confirm")]
        public async Task<IActionResult> ConfirmReceive(int poId, [FromBody] ConfirmReceiveRequest request)
        {
            int staffId = GetCurrentUserId(defaultId: 3);
            var result = await _receivingService.ConfirmReceiveAsync(poId, request, staffId);
            return Ok(ApiResponse<PurchaseOrderResponse>.Success(result, "Products received and inventory updated successfully.", "MSG12"));
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
