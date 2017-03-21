using ChatProtocol;
using System;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public static class Program
    {
        public static void Main()
        {
            Console.WindowHeight = 10;

            const string address = "127.0.0.1";
            const int port = 9000;

            using (var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse(address), port));
                Console.WriteLine($"Connected to {address}:{port}");

                Console.Write("Introduce yourself:");
                var name = Console.ReadLine();

                if (string.IsNullOrEmpty(name))
                    return;

                Console.Title = name;

                Send(clientSocket, new JoinChat(name));

                var clientMessageListener = new ClientMessageListener(clientSocket);
                clientMessageListener.Start();

                Console.WriteLine("Start typing and press enter to send message...");

                while (true)
                {
                    var text = Console.ReadLine();

                    if (string.IsNullOrEmpty(text))
                    {
                        clientMessageListener.Stop();
                        break;
                    }

                    ChatPDU chatPdu;
                    if (text.Contains(":"))
                    {
                        var parts = text.Split(':');
                        var target = parts[0];
                        var message = parts[1];
                        chatPdu = new PrivateMessage(name, target, message);
                    }
                    else
                    {
                        chatPdu = new PublicMessage(name, text);
                    }

                    Send(clientSocket, chatPdu);
                }
            }
        }

        private static void Send(Socket socket, ChatPDU chatPdu)
        {
            var bytes = chatPdu.Serialize();

            try
            {
                var position = 0;
                do
                {
                    position += socket.Send(bytes, position, bytes.Length - position, SocketFlags.None);
                } while (position < bytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
