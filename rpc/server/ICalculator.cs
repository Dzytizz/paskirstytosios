using lib;

namespace server
{
    public interface ICalculator
    {
        decimal Add(decimal a, decimal b);
        decimal Subtract(decimal a, decimal b);
        decimal Multiply(decimal a, decimal b);
        decimal Divide(decimal a, decimal b);

        decimal Calculate(decimal a, decimal b, Operation op);
    }
}
