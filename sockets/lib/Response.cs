namespace lib
{
    public class Response
    {
        public decimal Result { get; set; }
        public string? ExceptionMessage { get; set; }

        public Response() { }

        public Response(decimal result)
        {
            Result = result;
        }

        public Response(Exception exception)
        {
            ExceptionMessage = exception.Message;
        }

    }
}
