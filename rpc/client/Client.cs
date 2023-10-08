using lib;
using System;
using System.Globalization;
using System.Runtime.Remoting;

namespace client
{
    internal class Client
    {
        private static readonly char ExitChar = 'q';

        static void Main(string[] args)
        {
            Console.WriteLine("Configuring remoting.");
            RemotingConfiguration.Configure("Client.exe.config", true);

            IRemoteObject remoteObject = (IRemoteObject)Activator.GetObject(
                typeof(IRemoteObject),
                "tcp://localhost:12345/RemoteObject");
            if (remoteObject == null)
            {
                Console.WriteLine("Could not locate TCP server");
                return;
            }

            string sessionId = Guid.NewGuid().ToString();
            Console.WriteLine("Enter 'q' to Quit at any time.");
            try
            {
                while (true)
                {
                    decimal a = ReadDecimal("Enter first number: ");
                    Operation op = ReadOperator("Enter operator (+,-,*,/): ");
                    decimal b = ReadDecimal("Enter second number: ");

                    string message = remoteObject.Calculate(sessionId, a, b, op, out decimal answer);
                    Console.WriteLine($"Message from server: {message}");
                    Console.WriteLine($"Answer is: {answer}");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Math actions canceled. Exiting.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        private static decimal ReadDecimal(string title)
        {
            Console.Write(title);
            string input;
            decimal result;
            while (true)
            {
                input = Console.ReadLine();

                if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                {
                    break;
                }
                else if (char.TryParse(input, out char character) && character.Equals(ExitChar))
                {
                    throw new OperationCanceledException();
                }
                else
                {
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.Write(title);
                }
            }

            return result;
        }

        private static Operation ReadOperator(string title)
        {
            Console.Write(title);
            char result;
            while (true)
            {
                char.TryParse(Console.ReadLine(), out result);

                if (OperationHelper.IsValidOperationChar(result))
                {
                    break;
                }
                else if (result.Equals(ExitChar))
                {
                    throw new OperationCanceledException();
                }
                else
                {
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.Write(title);
                }
            }

            return OperationHelper.GetOperation(result);
        }
    }
}
