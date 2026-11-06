namespace backend.Common
{
    public class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(string code, string message, T? data = default)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T> Success(T? data, string message = "Success", string code = "200")
        {
            return new ApiResponse<T>(code, message, data);
        }

        public static ApiResponse<T> Fail(string code, string message, T? data = default)
        {
            return new ApiResponse<T>(code, message, data);
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse()
        {
        }

        public ApiResponse(string code, string message, object? data = null)
            : base(code, message, data)
        {
        }
    }
}
