using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Program
    {
        private static readonly IList<ClientChannel> Clients = new List<ClientChannel>();

        public static void Main(string[] args)
        {
            Console.WindowHeight = 10;

            const string listeningAddress = "127.0.0.1";
            const int port = 9000;

            using (var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(listeningAddress), port));
                serverSocket.Listen(1);

                Console.WriteLine($"Listening on {listeningAddress}:{port}");

                var server = new Server(serverSocket);
                server.Start();

                server.ClientConnected += OnClientConnected;

                Console.ReadLine();
            }
        }

        private static void OnClientConnected(object sender, ClientConnectedEventArgs args)
        {
            Console.WriteLine($"Client '{args.Socket.RemoteEndPoint}' connected");

            var acceptedClient = new ClientChannel(args.Socket);
            acceptedClient.ClientAuthorized += OnClientAuthorized;
            acceptedClient.ClientSentMessage += OnClientSentMessage;
            acceptedClient.ClientDisconnected += OnClientDisconnected;

            Clients.Add(acceptedClient);

            acceptedClient.Start();
        }

        private static void OnClientSentMessage(object sender, ClientSendMessageEventArgs args)
        {
            var message = $"{args.SourceClient}:{args.Message}";

            if (args.TargetClient == ClientSendMessageEventArgs.AllClients)
            {
                foreach (var client in Clients.Where(c => c != sender))
                {
                    client.Send(message);
                }
            }
            else
            {
                var targetClient = Clients.FirstOrDefault(c => c.Name == args.TargetClient);
                if (targetClient == null)
                {
                    Console.WriteLine($"Cannot find '{args.TargetClient}' client");
                    return;
                }

                targetClient.Send(message);
            }
        }

        private static void OnClientAuthorized(object sender, ClientAuthorizedEventArgs args)
        {
            var message = $"server:client {args.Client} has joined chat";

            foreach (var client in Clients.Where(c => c != sender))
            {
                client.Send(message);
            }
        }

        private static void OnClientDisconnected(object sender, EventArgs args)
        {
            var acceptedClient = (ClientChannel)sender;
            Clients.Remove(acceptedClient);

            var message = $"server:client {acceptedClient.Name} has left chat";

            foreach (var client in Clients)
            {
                client.Send(message);
            }

            Console.WriteLine($"Client '{acceptedClient.Socket.RemoteEndPoint}' disconnected");
        }
    }
}
