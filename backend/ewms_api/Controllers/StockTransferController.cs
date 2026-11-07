using ewms_api.Route.Repository;
using ewms_common.Controllers;
using ewms_common.Wrappers;
using ewms_service.DTOs.Repository.StockTransfer.Request;
using ewms_service.DTOs.Repository.StockTransfer.Response;
using ewms_service.Services.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(StockTransferRoute.INDEX)]
    public class StockTransferController : AuthorizeController
    {
        private readonly IStockTransferService _stockTransferService;

        public StockTransferController(
            IStockTransferService stockTransferService,
            ILogger<StockTransferController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _stockTransferService = stockTransferService;
        }

        [HttpPost(StockTransferRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<StockTransferTableView>>>> Search([FromBody] StockTransferTableFilter filter)
        {
            try
            {
                var res = await _stockTransferService.SearchAsync(filter, UserHeader);
                return Ok(new Response<ExtendedPageResponse<StockTransferTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(StockTransferRoute.Action.GetById)]
        public async Task<ActionResult<Response<StockTransferDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _stockTransferService.GetByIdAsync(id);
                return Ok(new Response<StockTransferDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(StockTransferRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] StockTransferCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _stockTransferService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(StockTransferRoute.Action.Approve)]
        public async Task<ActionResult<Response<bool>>> Approve([FromBody] StockTransferApproveRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _stockTransferService.ApproveAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(StockTransferRoute.Action.Reject)]
        public async Task<ActionResult<Response<bool>>> Reject([FromBody] StockTransferRejectRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _stockTransferService.RejectAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(StockTransferRoute.Action.Cancel)]
        public async Task<ActionResult<Response<bool>>> Cancel([FromBody] StockTransferCancelRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _stockTransferService.CancelAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(StockTransferRoute.Action.Dispatch)]
        public async Task<ActionResult<Response<bool>>> Dispatch([FromBody] StockTransferDispatchRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _stockTransferService.DispatchTransferAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(StockTransferRoute.Action.Receive)]
        public async Task<ActionResult<Response<bool>>> Receive([FromBody] StockTransferReceiveRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _stockTransferService.ReceiveTransferAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
