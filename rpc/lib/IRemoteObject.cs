using System.Collections.Generic;

namespace lib
{
    public interface IRemoteObject
    {
        Dictionary<string, int> counters { get; set; }
        int totalCounter { get; set; }
        ICalculator calculator { get; set; }

        string Calculate(string sessionId, decimal a, decimal b, Operation op, out decimal answer);

        void TryAddClient(string sessionId);
    }
}
