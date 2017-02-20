using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class ClientChannel
    {
        public Socket Socket { get; }
        public string Name { get; private set; }

        private readonly Thread _thread;
        private bool _stop;
        private bool _authorized;

        public event EventHandler<ClientSendMessageEventArgs> ClientSentMessage;
        public event EventHandler<ClientAuthorizedEventArgs> ClientAuthorized;
        public event EventHandler ClientDisconnected;

        public ClientChannel(Socket socket)
        {
            Socket = socket;
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
                try
                {
                    var readCount = Socket.Receive(buffer);
                    if (readCount == 0)
                        break;

                    var message = Encoding.UTF8.GetString(buffer, 0, readCount);

                    if (!_authorized)
                    {
                        Name = message;
                        ClientAuthorized?.Invoke(this, new ClientAuthorizedEventArgs(Name));
                        _authorized = true;
                    }
                    else if (message.IndexOf(':') == -1)
                    {
                        ClientSentMessage?.Invoke(this, new ClientSendMessageEventArgs(Name, message));
                    }
                    else
                    {
                        var parts = message.Split(':');
                        var targetClient = parts[0];

                        ClientSentMessage?.Invoke(this, new ClientSendMessageEventArgs(Name, targetClient, parts[1]));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    break;
                }

                Thread.Sleep(1);
            }

            ClientDisconnected?.Invoke(this, EventArgs.Empty);
        }

        public void Send(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var bytes = Encoding.UTF8.GetBytes(message);

            try
            {
                Socket.Send(bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Close()
        {
            try
            {
                Socket.Close();
            }
            catch
            {
                // ignored
            }
        }
    }
}