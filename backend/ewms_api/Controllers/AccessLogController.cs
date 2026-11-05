using ewms_api.Route.AccessLog;
using ewms_common.Controllers;
using ewms_common.Wrappers;
using ewms_service.DTOs.AccessLog.Request;
using ewms_service.DTOs.AccessLog.Response;
using ewms_service.Services.AccessLog;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(AccessLogRoute.INDEX)]
    public class AccessLogController : AuthorizeController
    {
        private readonly IAccessLogService _accessLogService;

        public AccessLogController(
            IAccessLogService accessLogService,
            ILogger<AccessLogController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _accessLogService = accessLogService;
        }

        [HttpPost(AccessLogRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<AccessLogTableView>>>> Search([FromBody] AccessLogTableFilter filter)
        {
            try
            {
                var res = await _accessLogService.SearchAsync(filter);
                return Ok(new Response<ExtendedPageResponse<AccessLogTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
