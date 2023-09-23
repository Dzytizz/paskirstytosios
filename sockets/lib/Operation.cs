namespace lib
{
    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public static class OperationHelper
    {
        public static bool IsValidOperationChar(string op)
        {
            return op.Length == 1 && IsValidOperationChar(op[0]);
        }

        public static bool IsValidOperationChar(char op)
        {
            return op == '+' || op == '-' || op == '*' || op == '/';
        }

        public static Operation GetOperation(char op)
        {
            switch (op)
            {
                case '+':
                    return Operation.Add;
                case '-':
                    return Operation.Subtract;
                case '*':
                    return Operation.Multiply;
                case '/':
                    return Operation.Divide;
                default:
                    throw new ArgumentException("Invalid operator");
            }
        }
    }
}
