using ewms_api.Route.User;
using ewms_common.Controllers;
using ewms_common.DTOs.Request;
using ewms_common.Wrappers;
using ewms_service.DTOs.User.Account.Request;
using ewms_service.DTOs.User.Account.Response;
using ewms_service.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(AccountRoute.INDEX)]
    public class AccountController : AuthorizeController
    {
        private readonly IAccountService _accountService;

        public AccountController(
            IAccountService accountService,
            ILogger<AccountController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _accountService = accountService;
        }

        [HttpPost(AccountRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<AccountTableView>>>> Search([FromBody] AccountTableFilter filter)
        {
            try
            {
                var res = await _accountService.SearchAsync(filter);
                return Ok(new Response<ExtendedPageResponse<AccountTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(AccountRoute.Action.GetById)]
        public async Task<ActionResult<Response<AccountDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _accountService.GetByIdAsync(id);
                return Ok(new Response<AccountDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(AccountRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] AccountCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _accountService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(AccountRoute.Action.Update)]
        public async Task<ActionResult<Response<bool>>> Update([FromBody] AccountUpdating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _accountService.UpdateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(AccountRoute.Action.UpdateStatus)]
        public async Task<ActionResult<Response<bool>>> UpdateStatus([FromBody] AccountStatusUpdating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _accountService.UpdateStatusAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(AccountRoute.Action.ResetPassword)]
        public async Task<ActionResult<Response<bool>>> ResetPassword([FromBody] AccountResetPasswordRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _accountService.ResetPasswordAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(AccountRoute.Action.Delete)]
        public async Task<ActionResult<Response<bool>>> Delete([FromBody] DeleteRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _accountService.DeleteAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
