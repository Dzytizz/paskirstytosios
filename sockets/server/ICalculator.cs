using lib;

namespace server
{
    public interface ICalculator
    {
        public decimal Add(decimal a, decimal b);
        public decimal Subtract(decimal a, decimal b);
        public decimal Multiply(decimal a, decimal b);
        public decimal Divide(decimal a, decimal b);

        public decimal Calculate(decimal a, decimal b, Operation op);
    }
}
