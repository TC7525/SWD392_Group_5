using ewms_api.Route.Contact;
using ewms_common.Controllers;
using ewms_common.DTOs.Request;
using ewms_common.Wrappers;
using ewms_service.DTOs.Contact.Customer.Request;
using ewms_service.DTOs.Contact.Customer.Response;
using ewms_service.Services.Contact;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(CustomerRoute.INDEX)]
    public class CustomerController : AuthorizeController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(
            ICustomerService customerService,
            ILogger<CustomerController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _customerService = customerService;
        }

        [HttpPost(CustomerRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<CustomerTableView>>>> Search([FromBody] CustomerTableFilter filter)
        {
            try
            {
                var res = await _customerService.SearchAsync(filter);
                return Ok(new Response<ExtendedPageResponse<CustomerTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(CustomerRoute.Action.GetById)]
        public async Task<ActionResult<Response<CustomerDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _customerService.GetByIdAsync(id);
                return Ok(new Response<CustomerDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(CustomerRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] CustomerCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _customerService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(CustomerRoute.Action.Update)]
        public async Task<ActionResult<Response<bool>>> Update([FromBody] CustomerUpdating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _customerService.UpdateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(CustomerRoute.Action.Delete)]
        public async Task<ActionResult<Response<bool>>> Delete([FromBody] DeleteRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _customerService.DeleteAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
