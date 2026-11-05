using ewms_api.Route.Repository;
using ewms_common.Controllers;
using ewms_common.Wrappers;
using ewms_service.DTOs.Repository.Inventory.Request;
using ewms_service.DTOs.Repository.Inventory.Response;
using ewms_service.Services.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(InventoryRoute.INDEX)]
    public class InventoryController : AuthorizeController
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(
            IInventoryService inventoryService,
            ILogger<InventoryController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost(InventoryRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<InventoryTableView>>>> Search([FromBody] InventoryTableFilter filter)
        {
            try
            {
                var res = await _inventoryService.SearchInventoryAsync(filter, UserHeader);
                return Ok(new Response<ExtendedPageResponse<InventoryTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(InventoryRoute.Action.Adjust)]
        public async Task<ActionResult<Response<bool>>> Adjust([FromBody] InventoryAdjustmentRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _inventoryService.AdjustInventoryAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(InventoryRoute.Action.SearchTransactions)]
        public async Task<ActionResult<Response<ExtendedPageResponse<InventoryTransactionTableView>>>> SearchTransactions([FromBody] InventoryTransactionTableFilter filter)
        {
            try
            {
                var res = await _inventoryService.SearchTransactionsAsync(filter, UserHeader);
                return Ok(new Response<ExtendedPageResponse<InventoryTransactionTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(InventoryRoute.Action.DashboardSummary)]
        public async Task<ActionResult<Response<DashboardSummaryView>>> GetDashboardSummary([FromQuery] string? warehouse_id = null)
        {
            try
            {
                var res = await _inventoryService.GetDashboardSummaryAsync(warehouse_id, UserHeader);
                return Ok(new Response<DashboardSummaryView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
