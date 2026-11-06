using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace backend.Common
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter>? _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter>? logger = null)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is AppException appEx)
            {
                _logger?.LogWarning(appEx, "Handled application exception: {Code} - {Message}", appEx.Code, appEx.Message);

                context.Result = new ObjectResult(new ApiResponse<object>
                {
                    Code = appEx.Code,
                    Message = appEx.Message,
                    Data = null
                })
                {
                    StatusCode = appEx.StatusCode
                };

                context.ExceptionHandled = true;
            }
            else
            {
                _logger?.LogError(context.Exception, "Unhandled exception occurred: {Message}", context.Exception.Message);

                context.Result = new ObjectResult(new ApiResponse<object>
                {
                    Code = "MSG17",
                    Message = "The operation could not be completed. No inventory changes were saved.",
                    Data = null
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };

                context.ExceptionHandled = true;
            }
        }
    }
}
