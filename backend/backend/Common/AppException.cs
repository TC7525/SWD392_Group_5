namespace backend.Common
{
    public class AppException : Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        public AppException(string code, string message, int statusCode = 400)
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }

        public AppException(string code, string message, Exception? innerException, int statusCode = 400)
            : base(message, innerException)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }
}
