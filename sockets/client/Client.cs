using lib;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace client
{
    internal class Client
    {
        private const string ServerIP = "127.0.0.1";
        private const int Port = 12345;
        private const char ExitChar = 'q';

        private static UdpClient udpClient;
        private static IPEndPoint serverEndPoint;

        static void Main(string[] args)
        {
            // Initialize
            udpClient = new UdpClient();
            udpClient.Connect(ServerIP, Port);
            serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), Port);

            udpClient.Send(new byte[0]);    // Register new client

            Console.WriteLine("Enter 'q' to Quit at any time. Enters response reading mode.");

            try
            {
                while (true)
                {
                    SendRequests();

                    ReadResponse();
                }
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine("Since math operation was cancelled, switching to response reading mode instead.");
                while (true) 
                {
                    ReadResponse();
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }

        private static void SendRequests()
        {
            decimal a = ReadDecimal("Enter first number: ");
            Operation op = ReadOperator("Enter operator: ");
            decimal b = ReadDecimal("Enter second number: ");

            Request request = new Request(a, b, op);
            string xmlText = XMLParser.SerializeToString(request);
            byte[] sendData = Encoding.ASCII.GetBytes(xmlText);
            udpClient.Send(sendData, sendData.Length);
            Console.WriteLine($"Sent data: \n{xmlText}");
        }

        private static void ReadResponse()
        {
            byte[] receivedData = udpClient.Receive(ref serverEndPoint);
            int packetCount = BitConverter.ToInt32(receivedData);
            Console.WriteLine($"Receiving amount of packets: {packetCount}");

            ResponsePacket[] responsePackets = new ResponsePacket[packetCount];
            for (int i = 0; i < packetCount; i++)
            {
                receivedData = udpClient.Receive(ref serverEndPoint);
                string xmlText = Encoding.ASCII.GetString(receivedData);
                responsePackets[i] = XMLParser.DeserializeFromString<ResponsePacket>(xmlText);

                Console.WriteLine($"Received packet: \n{xmlText}");
            }

            responsePackets = responsePackets.OrderBy(packet => packet.SequenceNumber).ToArray();
            Response? response = GetResponseFromPackets(responsePackets);
            if (response is null)
            {
                Console.WriteLine("Invalid response from server.");
                return;
            }
            else if (response.ExceptionMessage is not null)
            {
                Console.WriteLine($"Received exception from server: {response.ExceptionMessage}");
            }
            else
            {
                Console.WriteLine($"Received parsed response from server: {response.Result}");
            }
        }

        // Tries to add all response packet contents into a string and parse it as Response object
        private static Response? GetResponseFromPackets(ResponsePacket[] packets)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Response));
            StringBuilder responseData = new StringBuilder();
            foreach (ResponsePacket packet in packets)
            {
                responseData.Append(Encoding.ASCII.GetString(packet.Bytes));
            }

            try
            {
                using (StringReader stringReader = new StringReader(responseData.ToString()))
                {
                    return serializer.Deserialize(stringReader) as Response;
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
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