using ewms_api.Route.Repository;
using ewms_common.Controllers;
using ewms_common.Wrappers;
using ewms_service.DTOs.Repository.PurchaseOrder.Request;
using ewms_service.DTOs.Repository.PurchaseOrder.Response;
using ewms_service.Services.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(PurchaseOrderRoute.INDEX)]
    public class PurchaseOrderController : AuthorizeController
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            ILogger<PurchaseOrderController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpPost(PurchaseOrderRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<PurchaseOrderTableView>>>> Search([FromBody] PurchaseOrderTableFilter filter)
        {
            try
            {
                var res = await _purchaseOrderService.SearchAsync(filter, UserHeader);
                return Ok(new Response<ExtendedPageResponse<PurchaseOrderTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(PurchaseOrderRoute.Action.GetById)]
        public async Task<ActionResult<Response<PurchaseOrderDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _purchaseOrderService.GetByIdAsync(id);
                return Ok(new Response<PurchaseOrderDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(PurchaseOrderRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] PurchaseOrderCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _purchaseOrderService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(PurchaseOrderRoute.Action.Approve)]
        public async Task<ActionResult<Response<bool>>> Approve([FromBody] PurchaseOrderApproveRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _purchaseOrderService.ApproveAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(PurchaseOrderRoute.Action.Reject)]
        public async Task<ActionResult<Response<bool>>> Reject([FromBody] PurchaseOrderRejectRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _purchaseOrderService.RejectAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(PurchaseOrderRoute.Action.Cancel)]
        public async Task<ActionResult<Response<bool>>> Cancel([FromBody] PurchaseOrderCancelRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _purchaseOrderService.CancelAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(PurchaseOrderRoute.Action.ReceiveGoods)]
        public async Task<ActionResult<Response<bool>>> ReceiveGoods([FromBody] PurchaseOrderReceiveRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _purchaseOrderService.ReceiveGoodsAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
