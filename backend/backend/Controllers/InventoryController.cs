using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Common;
using backend.Models.DTOs;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/inventories")]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStockOverview()
        {
            var result = await _inventoryService.GetStockOverviewAsync();
            return Ok(ApiResponse<List<InventoryStockResponse>>.Success(result, "Success", "200"));
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int count = 50)
        {
            var result = await _inventoryService.GetRecentTransactionsAsync(count);
            return Ok(ApiResponse<List<InventoryTransactionResponse>>.Success(result, "Success", "200"));
        }
    }
}
