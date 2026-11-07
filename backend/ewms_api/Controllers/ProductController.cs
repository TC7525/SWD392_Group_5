using ewms_api.Route.Product;
using ewms_common.Controllers;
using ewms_common.DTOs.Request;
using ewms_common.Wrappers;
using ewms_service.DTOs.Product.Request;
using ewms_service.DTOs.Product.Response;
using ewms_service.Services.Product;
using Microsoft.AspNetCore.Mvc;

namespace ewms_api.Controllers
{
    [Route(ProductRoute.INDEX)]
    public class ProductController : AuthorizeController
    {
        private readonly IProductService _productService;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger,
            IWebHostEnvironment env) : base(logger, env)
        {
            _productService = productService;
        }

        [HttpPost(ProductRoute.Action.Search)]
        public async Task<ActionResult<Response<ExtendedPageResponse<ProductTableView>>>> Search([FromBody] ProductTableFilter filter)
        {
            try
            {
                var res = await _productService.SearchAsync(filter, UserHeader);
                return Ok(new Response<ExtendedPageResponse<ProductTableView>>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet(ProductRoute.Action.GetById)]
        public async Task<ActionResult<Response<ProductDetailView>>> GetById([FromRoute] string id)
        {
            try
            {
                var res = await _productService.GetByIdAsync(id);
                return Ok(new Response<ProductDetailView>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(ProductRoute.Action.Create)]
        public async Task<ActionResult<Response<string>>> Create([FromBody] ProductCreating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _productService.CreateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<string>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut(ProductRoute.Action.Update)]
        public async Task<ActionResult<Response<bool>>> Update([FromBody] ProductUpdating request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _productService.UpdateAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost(ProductRoute.Action.Delete)]
        public async Task<ActionResult<Response<bool>>> Delete([FromBody] DeleteRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var res = await _productService.DeleteAsync(request, UserHeader, ipAddress);
                return Ok(new Response<bool>(res));
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
