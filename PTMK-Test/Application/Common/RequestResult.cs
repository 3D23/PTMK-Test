namespace PTMK_Test.Application.Common
{
    public readonly struct RequestResult
    {
        public bool IsSuccess { get; private init; }
        public string ErrorMessage { get; private init; }
        public int StatusCode { get; private init; }

        private RequestResult(
            bool isSuccess, 
            string errorMessage = "", 
            int statusCode = 400)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        public static RequestResult Success() 
            => new(true);
        
        public static RequestResult Failure(string message) 
            => new(false, message, 400);

        public static RequestResult NotFound(string message)
            => new(false, message, 404);
    }

    public readonly struct RequestResult<T>
    {
        public bool IsSuccess { get; private init; }
        public string ErrorMessage { get; private init; }
        public int StatusCode { get; private init; }
        public T? Value { get; private init; }

        private RequestResult(bool isSuccess, T? value, string errorMessage = "", int statusCode = 400)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
        }

        public static RequestResult<T> Success(T value) => new(true, value, statusCode: 200);
        public static RequestResult<T> Failure(string message) => new(false, default, message, 400);
        public static RequestResult<T> NotFound(string message) => new(false, default, message, 404);
    }
}
