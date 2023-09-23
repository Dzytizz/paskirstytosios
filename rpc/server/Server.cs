using lib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;

namespace server
{
    internal class Server
    {
        static void Main(string[] args)
        {
            SoapServerFormatterSinkProvider provider = new SoapServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary props = new Hashtable();
            props["port"] = 1234;
            props["name"] = "tcp";
            props["typeFilterLevel"] = TypeFilterLevel.Full;

            SoapServerFormatterSinkProvider serverProvider = new SoapServerFormatterSinkProvider();
            SoapClientFormatterSinkProvider clientProvider = new SoapClientFormatterSinkProvider();

            TcpChannel channel = new TcpChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteObject),
                "RemoteObject",
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Server is running. Press Enter to exit.");
            Console.ReadLine();
        }
    }

    public class RemoteObject : MarshalByRefObject
    {
        private Dictionary<string, int> counters;
        int totalCounter;
        Calculator calculator;

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

        public decimal? GetLastAnswer()
        {
            return calculator.LastAnswer;
        }

        private void TryAddClient(string name)
        {
            if (!counters.ContainsKey(name))
            {
                counters.Add(name, 0);
            }
        }
    }
}
