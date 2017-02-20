using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class ClientMessageListener
    {
        private readonly Socket _socket;
        private readonly Thread _thread;
        private bool _stop;

        public ClientMessageListener(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            _socket = socket;
            _thread = new Thread(Run);
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Stop()
        {
            _stop = true;
            _thread.Join();
        }

        private void Run()
        {
            var buffer = new byte[1024];

            while (!_stop)
            {
                if (_socket.Available > 0)
                {
                    var readBytes = _socket.Receive(buffer);

                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, readBytes);

                    Console.WriteLine($"{receivedMessage}");
                }

                Thread.Sleep(1);
            }
        }
    }
}
