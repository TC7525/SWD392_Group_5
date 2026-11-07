using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ewms_common.Exceptions;
using ewms_common.Authentications;

namespace ewms_common.Controllers
{
    [ApiController]
    public class BaseController : Controller
    {
        protected ILogger _logger;
        private readonly IWebHostEnvironment _env;

        protected ActionResult HandleException(Exception ex)
        {
            var userId = HttpContext.User
                .FindFirst(CustomJwtClaimType.UserId)?.Value ?? "anonymous";
            var warehouseId = HttpContext.User
                .FindFirst(CustomJwtClaimType.WarehouseId)?.Value ?? "";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            _logger.LogWarning(
                ex,
                "Request failed | UserId: {UserId} | WarehouseId: {WarehouseId} | TraceId: {TraceId} | Method: {Method} | Path: {Path} | IpAddress: {IpAddress}",
                userId,
                warehouseId,
                HttpContext.TraceIdentifier,
                HttpContext.Request.Method,
                HttpContext.Request.Path,
                ipAddress
            );
            if (ex is ForbiddenException)
            {
                return StatusCode(403, new { Message = ex.Message });
            }
            if (ex is NotFoundException)
            {
                return NotFound(((NotFoundException)ex).Message);
            }
            if (ex is BadException)
            {
                return BadRequest(((BadException)ex).Message);
            }

            if (ex is UnexpectedException unexpectedEx && unexpectedEx.ErrorObject != null)
            {
                return StatusCode(500, new
                {
                    unexpectedEx.Message,
                    Data = unexpectedEx.ErrorObject
                });
            }

            _logger.LogError(
                ex,
                "Request failed | UserId: {UserId} | WarehouseId: {WarehouseId} | TraceId: {TraceId} | Method: {Method} | Path: {Path} | IpAddress: {IpAddress}",
                userId,
                warehouseId,
                HttpContext.TraceIdentifier,
                HttpContext.Request.Method,
                HttpContext.Request.Path,
                ipAddress
            );
            if (_env.EnvironmentName == "Production")
            {
                return Problem(detail: "Có lỗi xảy ra trong quá trình xử lý", statusCode: 500);
            }
            return Problem(detail: ex.Message, statusCode: 500);
        }

        public BaseController(ILogger logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthorizeController : BaseController
    {
        protected UserHeader UserHeader
        {
            get
            {
                try
                {
                    return new UserHeader
                    {
                        phone = User.FindFirst(CustomJwtClaimType.Phone)?.Value ?? "",
                        email = User.FindFirst(CustomJwtClaimType.Email)?.Value ?? "",
                        employee_code = User.FindFirst(CustomJwtClaimType.EmployeeCode)?.Value ?? "",
                        user_id = User.FindFirst(CustomJwtClaimType.UserId)?.Value ?? "",
                        warehouse_id = User.FindFirst(CustomJwtClaimType.WarehouseId)?.Value ?? "",
                        role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst(CustomJwtClaimType.Role)?.Value ?? "",
                        fullname = User.FindFirst(CustomJwtClaimType.Fullname)?.Value ?? "",
                        role_id = User.FindFirst(CustomJwtClaimType.RoleId)?.Value ?? "",
                        warehouse_name = User.FindFirst(CustomJwtClaimType.WarehouseName)?.Value ?? "",
                        warehouse_code = User.FindFirst(CustomJwtClaimType.WarehouseCode)?.Value ?? "",
                        system_role = User.FindFirst(CustomJwtClaimType.SystemRole)?.Value ?? "",
                    };
                }
                catch
                {
                    return new UserHeader();
                }
            }
        }

        public AuthorizeController(ILogger logger, IWebHostEnvironment env) : base(logger, env)
        {
        }
    }
}
