namespace lib
{
    [Serializable]
    public class Request
    {
        public decimal A { get; set; }
        public decimal B { get; set; }
        public Operation Operation { get; set; }

        public Request() { }

        public Request(decimal a, decimal b, Operation operation)
        {
            A = a;
            B = b;
            Operation = operation;
        }
    }
}