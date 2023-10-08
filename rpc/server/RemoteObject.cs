using lib;
using System;
using System.Collections.Generic;

namespace server
{
    public class RemoteObject : MarshalByRefObject, IRemoteObject
    {
        public Dictionary<string, int> counters { get; set; }
        public int totalCounter { get; set; }
        public ICalculator calculator { get; set; }

        public RemoteObject()
        {
            counters = new Dictionary<string, int>();
            totalCounter = 0;
            calculator = new Calculator();
        }

        public string Calculate(string sessionId, decimal a, decimal b, Operation op, out decimal answer)
        {
            TryAddClient(sessionId);
            counters[sessionId]++;

            totalCounter++;

            answer = 0;
            try
            {
                answer = calculator.Calculate(a, b, op);
                string message = $"'{sessionId}' client called: {counters[sessionId]} times, Total counter: {totalCounter}";
                Console.WriteLine(message);

                return message;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message} '{sessionId}' client called: {counters[sessionId]} times, Total counter: {totalCounter}");
                string message = $"Error: {e.Message} Default answer of 0 is returned.";

                return message;
            }
        }

        public void TryAddClient(string sessionId)
        {
            if (!counters.ContainsKey(sessionId))
            {
                counters.Add(sessionId, 0);
            }
        }
    }
}
