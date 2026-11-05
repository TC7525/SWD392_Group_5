using ewms_api.Route.Repository;
using ewms_common.Controllers;
using ewms_common.Wrappers;
using ewms_service.DTOs.Repository.OutboundOrder.Request;
using ewms_service.DTOs.Repository.OutboundOrder.Response;
using ewms_service.Services.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(OutboundOrderRoute.INDEX)]
    public class OutboundOrderController : AuthorizeController
    {
        private readonly IOutboundOrderService _outboundOrderService;

        public OutboundOrderController(
            IOutboundOrderService outboundOrderService,
            ILogger<OutboundOrderController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _outboundOrderService = outboundOrderService;
        }

        [HttpPost(OutboundOrderRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<OutboundOrderTableView>>>> Search([FromBody] OutboundOrderTableFilter filter)
        {
            try
            {
                var res = await _outboundOrderService.SearchAsync(filter, UserHeader);
                return Ok(new Response<ExtendedPageResponse<OutboundOrderTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(OutboundOrderRoute.Action.GetById)]
        public async Task<ActionResult<Response<OutboundOrderDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _outboundOrderService.GetByIdAsync(id);
                return Ok(new Response<OutboundOrderDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(OutboundOrderRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] OutboundOrderCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _outboundOrderService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(OutboundOrderRoute.Action.Approve)]
        public async Task<ActionResult<Response<bool>>> Approve([FromBody] OutboundOrderApproveRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _outboundOrderService.ApproveAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(OutboundOrderRoute.Action.Reject)]
        public async Task<ActionResult<Response<bool>>> Reject([FromBody] OutboundOrderRejectRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _outboundOrderService.RejectAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(OutboundOrderRoute.Action.Cancel)]
        public async Task<ActionResult<Response<bool>>> Cancel([FromBody] OutboundOrderCancelRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _outboundOrderService.CancelAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(OutboundOrderRoute.Action.IssueGoods)]
        public async Task<ActionResult<Response<bool>>> IssueGoods([FromBody] OutboundOrderIssueRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _outboundOrderService.IssueGoodsAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
