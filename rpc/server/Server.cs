using System;
using System.Runtime.Remoting;

namespace server
{
    public class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Configuring remoting.");
            RemotingConfiguration.Configure("Server.exe.config", true);

            Console.WriteLine("Server ready. Press any key to exit.");
            Console.ReadLine();
        }
    }
}
