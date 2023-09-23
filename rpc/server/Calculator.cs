using lib;
using System;

namespace server
{
    public class Calculator
    {
        public decimal LastAnswer { get; private set; }

        public decimal Add(decimal a, decimal b) => a + b;
        public decimal Subtract(decimal a, decimal b) => a - b;
        public decimal Multiply(decimal a, decimal b) => a * b;
        public decimal Divide(decimal a, decimal b) => a / b;

        public decimal Calculate(decimal a, decimal b, Operation op)
        {
            switch (op)
            {
                case Operation.Add:
                    LastAnswer = Add(a, b);
                    break;
                case Operation.Subtract:
                    LastAnswer = Subtract(a, b);
                    break;
                case Operation.Multiply:
                    LastAnswer = Multiply(a, b);
                    break;
                case Operation.Divide:
                    LastAnswer = Divide(a, b);
                    break;
                default:
                    throw new ArgumentException("Invalid operator");
            }

            return LastAnswer;
        }
    }
}
