namespace Common.Helper
{
    public class ReturnMessage
    {
        public string Message { get; set; }
        public string Status { get; set; }
        public bool HasError { get; set; }
        public object Data { get; set; }    

        public static ReturnMessage GenerateError(string error, string status = "-1")
        {
            return new ReturnMessage()
            {
                Message = error,
                Status = status,
                HasError = true,
                Data = null
            };
        }

        public static ReturnMessage GenerateMessage(string info)
        {
            return new ReturnMessage()
            {
                Message = info,
                Status = "0",
                HasError = false
            };
        }

        public static ReturnMessage GenerateData(object data)
        {
            return new ReturnMessage()
            {
                Data = data,
                HasError = false,
                Message = null,
                Status = "0"
            };
        }
    }

}
