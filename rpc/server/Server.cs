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
