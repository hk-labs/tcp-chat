using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
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

                var clientMessageListener = new ClientMessageListener(clientSocket);
                clientMessageListener.Start();

                var bytes = Encoding.UTF8.GetBytes(name);
                clientSocket.Send(bytes);

                Console.WriteLine("Start typing and press enter to send message...");

                while (true)
                {
                    var text = Console.ReadLine();

                    if (string.IsNullOrEmpty(text))
                    {
                        clientMessageListener.Stop();
                        break;
                    }

                    bytes = Encoding.UTF8.GetBytes(text);

                    clientSocket.Send(bytes);
                }
            }
        }
    }
}
