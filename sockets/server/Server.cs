using lib;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace server
{
    internal class Server
    {
        private const int Port = 12345;
        private const int PacketSize = 64;

        private static UdpClient listener;
        private static HashSet<IPEndPoint> clientEndpoints;

        static async Task Main(string[] args)
        {
            listener = new UdpClient(Port);
            clientEndpoints = new HashSet<IPEndPoint>();
            Console.WriteLine($"Server is listening on port {Port}.");

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for requests.");
                    UdpReceiveResult receiveResult = await listener.ReceiveAsync();
                    byte[] receivedBytes = receiveResult.Buffer;
                    if (!clientEndpoints.Contains(receiveResult.RemoteEndPoint))
                    {
                        clientEndpoints.Add(receiveResult.RemoteEndPoint);
                        Console.WriteLine($"New client sent request to server: {receiveResult.RemoteEndPoint}.");
                    }

                    string receivedData = Encoding.ASCII.GetString(receivedBytes);
                    Console.WriteLine($"Received data from {receiveResult.RemoteEndPoint}: \n{receivedData}");

                    XmlSerializer serializer = new XmlSerializer(typeof(Request));
                    Request? request;
                    using (StringReader stringReader = new StringReader(receivedData))
                    {
                        request = serializer.Deserialize(stringReader) as Request;
                        if (request is null)
                        {
                            Console.WriteLine($"Invalid request from {receiveResult.RemoteEndPoint}.");
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

                    await TrySendToAllClients(response);
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

        private static async Task TrySendToAllClients(Response response)
        {
            ResponsePacket[] responsePackets = SplitResponseIntoPackets(response, PacketSize);

            foreach (var client in clientEndpoints)
            {
                Console.WriteLine($"Sending response to {client} as {responsePackets.Length} packets.");
                byte[] responseLength = BitConverter.GetBytes(responsePackets.Length);
                await listener.SendAsync(responseLength, responseLength.Length, client);
                foreach (ResponsePacket packet in responsePackets)
                {
                    await listener.SendAsync(packet.Bytes, packet.Bytes.Length, client);
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