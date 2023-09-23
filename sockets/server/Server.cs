using lib;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace server
{
    internal class Server
    {
        private const int PacketSize = 64;
        private static HashSet<IPEndPoint> connectedClients = new();

        static void Main(string[] args)
        {
            int port = 12345;
            UdpClient listener = new UdpClient(port);
            Console.WriteLine($"Server is listening on port {port}.");
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, port);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for requests.");
                    byte[] receivedBytes = listener.Receive(ref clientEndPoint);
                    if (!connectedClients.Contains(clientEndPoint))
                    {
                        connectedClients.Add(clientEndPoint);
                        Console.WriteLine($"New client sent request to server: {clientEndPoint}.");
                    }

                    string receivedData = Encoding.ASCII.GetString(receivedBytes);
                    Console.WriteLine($"Received data from {clientEndPoint}: {receivedData}");

                    XmlSerializer serializer = new XmlSerializer(typeof(Request));
                    Request? request;
                    using (StringReader stringReader = new StringReader(receivedData))
                    {
                        request = serializer.Deserialize(stringReader) as Request;
                        if (request is null)
                        {
                            Console.WriteLine($"Invalid request from {clientEndPoint}.");
                            continue;
                        }
                    }

                    Response response = new Response();

                    decimal result = 0;
                    Calculator calculator = new Calculator();
                    try
                    {
                        result = calculator.Calculate(request.A, request.B, request.Operation);
                        response.Result = result;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        response.ExceptionMessage = e.Message;
                    }

                    TrySendToAllClients(listener, response);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }

        private static void TrySendToAllClients(UdpClient listener, Response response)
        {
            ResponsePacket[] responsePackets = SplitResponseIntoPackets(response, PacketSize);

            foreach (var client in connectedClients)
            {
                Console.WriteLine($"Sending response to {client} as {responsePackets.Length} packets.");
                byte[] responseLength = BitConverter.GetBytes(responsePackets.Length);
                listener.Send(responseLength, responseLength.Length, client);
                foreach (ResponsePacket packet in responsePackets)
                {
                    listener.Send(packet.Bytes, packet.Bytes.Length, client);
                    Console.WriteLine($"Packet {packet.SequenceNumber}: {Encoding.ASCII.GetString(packet.Bytes)}");
                }
            }
        }

        private static ResponsePacket[] SplitResponseIntoPackets(Response response, int packetSize)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Response));
            string responseData;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, response);
                responseData = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            ResponsePacket[] packets = new ResponsePacket[responseData.Length / packetSize + 1];
            for (int i = 0; i < packets.Length; i++)
            {
                packets[i] = new ResponsePacket(i, Encoding.ASCII.GetBytes(responseData.Substring(i * packetSize, Math.Min(packetSize, responseData.Length - i * packetSize))));
            }

            return packets;
        }
    }
}