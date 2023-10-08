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
        private const int PacketSize = 128;

        private static UdpClient listener;
        private static HashSet<IPEndPoint> clientEndpoints;
        private static ICalculator calculator;

        static void Main(string[] args)
        {
            // Initialize
            listener = new UdpClient(Port);
            clientEndpoints = new HashSet<IPEndPoint>();
            calculator = new Calculator();
            Console.WriteLine($"Server is listening on port {Port}.");
            Console.WriteLine("Waiting for requests.");
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, Port);

            try
            {
                while (true)
                {
                    byte[] receivedBytes = listener.Receive(ref clientEndPoint);
                    if (receivedBytes.Length == 0 && !clientEndpoints.Contains(clientEndPoint)) // If new client, add to list
                    {
                        clientEndpoints.Add(clientEndPoint);
                        Console.WriteLine($"New client connected to server: {clientEndPoint}.");
                        continue;
                    }
                    else // Else do calculation
                    {
                        string receivedData = Encoding.ASCII.GetString(receivedBytes);
                        Console.WriteLine($"Received data from {clientEndPoint}: \n{receivedData}");

                        Response response = new Response();
                        decimal result = 0;
                        try
                        {
                            Request? request = XMLParser.DeserializeFromString<Request>(receivedData);  // Try form response from request
                            if (request is null)
                            {
                                Console.WriteLine($"Wrong request from client {clientEndPoint}.");
                                continue;
                            }
                            result = calculator.Calculate(request.A, request.B, request.Operation);
                            response.Result = result;
                        }
                        catch (Exception e) // If calculation fails add error message to response
                        {
                            Console.WriteLine(e.Message);
                            response.ExceptionMessage = e.Message;
                        }

                        TrySendToAllClients(listener, response);    // Send response to all connected clients
                    }
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
            // Split response into packets and randomize (to test udp mechanism in client)
            ResponsePacket[] responsePackets = SplitResponseIntoPackets(response, PacketSize);  
            responsePackets = responsePackets.OrderBy(x => Guid.NewGuid()).ToArray();

            foreach (var client in clientEndpoints)
            {
                Console.WriteLine($"Sending response to {client} as {responsePackets.Length} packets.");
                byte[] responseLength = BitConverter.GetBytes(responsePackets.Length);
                listener.Send(responseLength, responseLength.Length, client);
                foreach (ResponsePacket packet in responsePackets)
                {
                    string xmlText = XMLParser.SerializeToString(packet);
                    byte[] bytes = Encoding.ASCII.GetBytes(xmlText);

                    listener.Send(bytes, bytes.Length, client);
                    Console.WriteLine($"Packet {packet.SequenceNumber} with contents \n{Encoding.ASCII.GetString(packet.Bytes)} \nsent as: \n{xmlText}");
                }
            }
        }

        private static ResponsePacket[] SplitResponseIntoPackets(Response response, int packetSize)
        {
            string xmlText = XMLParser.SerializeToString(response);

            string emptyPacket = XMLParser.SerializeToString(new ResponsePacket(99, new byte[0]));
            int charCount = 8;
            int remainingLength = packetSize - emptyPacket.Length - charCount;

            ResponsePacket[] packets = new ResponsePacket[xmlText.Length / remainingLength + 1];
            for (int i = 0; i < packets.Length; i++)
            {
                packets[i] = new ResponsePacket(i, Encoding.ASCII.GetBytes(xmlText.Substring(i * remainingLength, Math.Min(remainingLength, xmlText.Length - i * remainingLength))));
            }

            return packets;
        }
    }
}