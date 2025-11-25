namespace APIPMSoftware.Src.Application.Common
{
    public class ResponseApiDynamic<T>
    {
        public int status { get; set; } 
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ResponseApiDynamic<T> Success(T data, string message = "Success")
        {
            return new ResponseApiDynamic<T> { status = 1, Message = message, Data = data };
        }   

        public static ResponseApiDynamic<T> Fail(string message)
        {
            return new ResponseApiDynamic<T> { status = 0, Message = message, Data = default };
        }
    }
}
