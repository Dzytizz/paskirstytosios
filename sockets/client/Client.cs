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
        private static readonly char ExitChar = 'q';

        static void Main(string[] args)
        {
            string serverIP = "127.0.0.1"; // Replace with the server's IP address
            int serverPort = 12345;
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(serverIP, serverPort);

            decimal a = ReadDecimal("Enter first number: ");
            Operation op = ReadOperator("Enter operator: ");
            decimal b = ReadDecimal("Enter second number: ");

            Request request = new Request(a, b, op);

            XmlSerializer serializer = new XmlSerializer(typeof(Request));
            string serializedData;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, request);
                serializedData = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            byte[] sendData = Encoding.ASCII.GetBytes(serializedData);
            udpClient.Send(sendData, sendData.Length);
            Console.WriteLine($"Sent data: {serializedData}");

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            while (true)
            {
                byte[] receivedData = udpClient.Receive(ref serverEndPoint);
                int responseLength = BitConverter.ToInt32(receivedData);
                Console.WriteLine($"Received response length: {responseLength}");
                ResponsePacket[] responsePackets = new ResponsePacket[responseLength];
                for (int i = 0; i < responseLength; i++)
                {
                    receivedData = udpClient.Receive(ref serverEndPoint);
                    responsePackets[i] = new ResponsePacket(i, receivedData);
                    Console.WriteLine($"Received packet: {Encoding.ASCII.GetString(receivedData)}");
                }
                responsePackets = responsePackets.OrderBy(packet => packet.SequenceNumber).ToArray();
                Response? response = GetResponseFromPackets(responsePackets);
                if (response is null)
                {
                    Console.WriteLine("Invalid response from server.");
                    continue;
                }
                else if (response.ExceptionMessage is not null)
                {
                    Console.WriteLine($"Received exception from server: {response.ExceptionMessage}");
                }
                else
                {
                    Console.WriteLine($"Received response from server: {response.Result}");
                }
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

        private static Response? GetResponseFromPackets(ResponsePacket[] packets)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Response));
            StringBuilder responseData = new StringBuilder();
            foreach (ResponsePacket packet in packets)
            {
                responseData.Append(Encoding.UTF8.GetString(packet.Bytes));
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
    }
}