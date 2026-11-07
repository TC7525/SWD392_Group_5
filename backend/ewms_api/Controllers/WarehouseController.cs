using ewms_api.Route.Repository;
using ewms_common.Controllers;
using ewms_common.DTOs.Request;
using ewms_common.Wrappers;
using ewms_service.DTOs.Repository.Warehouse.Request;
using ewms_service.DTOs.Repository.Warehouse.Response;
using ewms_service.Services.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(WarehouseRoute.INDEX)]
    public class WarehouseController : AuthorizeController
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(
            IWarehouseService warehouseService,
            ILogger<WarehouseController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost(WarehouseRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<WarehouseTableView>>>> Search([FromBody] WarehouseTableFilter filter)
        {
            try
            {
                var res = await _warehouseService.SearchAsync(filter);
                return Ok(new Response<ExtendedPageResponse<WarehouseTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(WarehouseRoute.Action.GetById)]
        public async Task<ActionResult<Response<WarehouseDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _warehouseService.GetByIdAsync(id);
                return Ok(new Response<WarehouseDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(WarehouseRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] WarehouseCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _warehouseService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(WarehouseRoute.Action.Update)]
        public async Task<ActionResult<Response<bool>>> Update([FromBody] WarehouseUpdating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _warehouseService.UpdateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(WarehouseRoute.Action.Delete)]
        public async Task<ActionResult<Response<bool>>> Delete([FromBody] DeleteRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _warehouseService.DeleteAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
