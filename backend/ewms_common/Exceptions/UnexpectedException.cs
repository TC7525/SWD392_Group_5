namespace ewms_common.Exceptions
{
    public class UnexpectedException : Exception
    {
        public object? ErrorObject { get; }

        public UnexpectedException()
        {
        }

        public UnexpectedException(string message) : base(message)
        {
        }

        public UnexpectedException(string message, object errorObject) : base(message)
        {
            ErrorObject = errorObject;
        }

        public UnexpectedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnexpectedException(string message, object errorObject, Exception innerException) : base(message, innerException)
        {
            ErrorObject = errorObject;
        }
    }
}
