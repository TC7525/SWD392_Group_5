using ewms_api.Route.User;
using ewms_common.Controllers;
using ewms_common.Wrappers;
using ewms_service.DTOs.User.Authentication.Request;
using ewms_service.DTOs.User.Authentication.Response;
using ewms_service.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(AuthenticationRoute.INDEX)]
    public class AuthenticationController : AuthorizeController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            ILogger<AuthenticationController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _authenticationService = authenticationService;
        }

        [AllowAnonymous]
        [HttpPost(AuthenticationRoute.Action.Login)]
        public async Task<ActionResult<Response<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _authenticationService.LoginAsync(request, ipAddress);
                return Ok(new Response<LoginResponse>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(AuthenticationRoute.Action.Profile)]
        public async Task<ActionResult<Response<UserProfileView>>> GetProfile()
        {
            try
            {
                var res = await _authenticationService.GetProfileAsync(UserHeader);
                return Ok(new Response<UserProfileView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(AuthenticationRoute.Action.UpdateProfile)]
        public async Task<ActionResult<Response<UserProfileView>>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _authenticationService.UpdateProfileAsync(request, UserHeader, ipAddress);
                return Ok(new Response<UserProfileView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(AuthenticationRoute.Action.ChangePassword)]
        public async Task<ActionResult<Response<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _authenticationService.ChangePasswordAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(AuthenticationRoute.Action.SwitchWarehouse)]
        public async Task<ActionResult<Response<LoginResponse>>> SwitchWarehouse([FromBody] SwitchWarehouseRequest request)
        {
            try
            {
                var res = await _authenticationService.SwitchWarehouseAsync(request, UserHeader);
                return Ok(new Response<LoginResponse>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
