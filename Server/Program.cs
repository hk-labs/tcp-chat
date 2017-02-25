using ChatProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public static class Program
    {
        private static readonly IList<ClientChannel> Clients = new List<ClientChannel>();

        public static void Main()
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

            var clientChannel = new ClientChannel(args.Socket);
            clientChannel.ClientJoined += OnClientJoined;
            clientChannel.ClientSentPublicMessage += OnClientSentPublicMessage;
            clientChannel.ClientSentPrivateMessage += OnClientSentPrivateMessage;
            clientChannel.ClientDisconnected += OnClientDisconnected;

            Clients.Add(clientChannel);

            clientChannel.Start();
        }

        private static void OnClientSentPublicMessage(object sender, PublicMessage message)
        {
            var incomingMessage = new IncomingMessage(message.Source, message.Message);

            foreach (var client in Clients.Where(c => c != sender))
            {
                client.Send(incomingMessage);
            }
        }

        private static void OnClientSentPrivateMessage(object sender, PrivateMessage message)
        {
            var targetClient = Clients.FirstOrDefault(c => c.Name == message.Target);
            if (targetClient == null)
            {
                Console.WriteLine($"Cannot find '{message.Target}' client");
                return;
            }

            targetClient.Send(new IncomingMessage(message.Source, message.Message));
        }

        private static void OnClientJoined(object sender, JoinChat message)
        {
            var clientJoined = new ClientJoinedChat(message.Name);

            foreach (var client in Clients.Where(c => c != sender))
            {
                client.Send(clientJoined);
            }
        }

        private static void OnClientDisconnected(object sender, EventArgs args)
        {
            var clientChannel = (ClientChannel)sender;
            Clients.Remove(clientChannel);

            var message = new ClientLeftChat(clientChannel.Name);

            foreach (var client in Clients)
            {
                client.Send(message);
            }

            Console.WriteLine($"Client '{clientChannel.Socket.RemoteEndPoint}' disconnected");
        }
    }
}
