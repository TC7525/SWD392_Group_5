using ewms_api.Route.Contact;
using ewms_common.Controllers;
using ewms_common.DTOs.Request;
using ewms_common.Wrappers;
using ewms_service.DTOs.Contact.Supplier.Request;
using ewms_service.DTOs.Contact.Supplier.Response;
using ewms_service.Services.Contact;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(SupplierRoute.INDEX)]
    public class SupplierController : AuthorizeController
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(
            ISupplierService supplierService,
            ILogger<SupplierController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _supplierService = supplierService;
        }

        [HttpPost(SupplierRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<SupplierTableView>>>> Search([FromBody] SupplierTableFilter filter)
        {
            try
            {
                var res = await _supplierService.SearchAsync(filter);
                return Ok(new Response<ExtendedPageResponse<SupplierTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(SupplierRoute.Action.GetById)]
        public async Task<ActionResult<Response<SupplierDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _supplierService.GetByIdAsync(id);
                return Ok(new Response<SupplierDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(SupplierRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] SupplierCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _supplierService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(SupplierRoute.Action.Update)]
        public async Task<ActionResult<Response<bool>>> Update([FromBody] SupplierUpdating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _supplierService.UpdateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(SupplierRoute.Action.Delete)]
        public async Task<ActionResult<Response<bool>>> Delete([FromBody] DeleteRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _supplierService.DeleteAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
