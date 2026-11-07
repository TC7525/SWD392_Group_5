using ewms_api.Route.User;
using ewms_common.Controllers;
using ewms_common.DTOs.Request;
using ewms_common.Wrappers;
using ewms_service.DTOs.User.Role.Request;
using ewms_service.DTOs.User.Role.Response;
using ewms_service.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(RoleRoute.INDEX)]
    public class RoleController : AuthorizeController
    {
        private readonly IRoleService _roleService;

        public RoleController(
            IRoleService roleService,
            ILogger<RoleController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _roleService = roleService;
        }

        [HttpPost(RoleRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<RoleTableView>>>> Search([FromBody] RoleTableFilter filter)
        {
            try
            {
                var res = await _roleService.SearchAsync(filter);
                return Ok(new Response<ExtendedPageResponse<RoleTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(RoleRoute.Action.GetById)]
        public async Task<ActionResult<Response<RoleDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _roleService.GetByIdAsync(id);
                return Ok(new Response<RoleDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(RoleRoute.Action.Permissions)]
        public async Task<ActionResult<Response<List<PermissionView>>>> GetPermissions()
        {
            try
            {
                var res = await _roleService.GetAllPermissionsAsync();
                return Ok(new Response<List<PermissionView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(RoleRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] RoleCreating request)
        {
            try
            {
                var res = await _roleService.CreateAsync(request, UserHeader);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(RoleRoute.Action.Update)]
        public async Task<ActionResult<Response<bool>>> Update([FromBody] RoleUpdating request)
        {
            try
            {
                var res = await _roleService.UpdateAsync(request, UserHeader);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(RoleRoute.Action.Delete)]
        public async Task<ActionResult<Response<bool>>> Delete([FromBody] DeleteRequest request)
        {
            try
            {
                var res = await _roleService.DeleteAsync(request, UserHeader);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
