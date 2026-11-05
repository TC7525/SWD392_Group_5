using ewms_api.Route.Category;
using ewms_common.Controllers;
using ewms_common.DTOs.Request;
using ewms_common.Wrappers;
using ewms_service.DTOs.Category.Request;
using ewms_service.DTOs.Category.Response;
using ewms_service.Services.Category;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(CategoryRoute.INDEX)]
    public class CategoryController : AuthorizeController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(
            ICategoryService categoryService,
            ILogger<CategoryController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _categoryService = categoryService;
        }

        [HttpPost(CategoryRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<CategoryTableView>>>> Search([FromBody] CategoryTableFilter filter)
        {
            try
            {
                var res = await _categoryService.SearchAsync(filter);
                return Ok(new Response<ExtendedPageResponse<CategoryTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(CategoryRoute.Action.GetById)]
        public async Task<ActionResult<Response<CategoryDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _categoryService.GetByIdAsync(id);
                return Ok(new Response<CategoryDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(CategoryRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] CategoryCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _categoryService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(CategoryRoute.Action.Update)]
        public async Task<ActionResult<Response<bool>>> Update([FromBody] CategoryUpdating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _categoryService.UpdateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(CategoryRoute.Action.Delete)]
        public async Task<ActionResult<Response<bool>>> Delete([FromBody] DeleteRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _categoryService.DeleteAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
