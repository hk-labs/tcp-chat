using ChatProtocol;
using System;
using System.Net.Sockets;
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
        private readonly PDUParser _parser = new PDUParser();

        public event EventHandler<PublicMessage> ClientSentPublicMessage;
        public event EventHandler<PrivateMessage> ClientSentPrivateMessage;
        public event EventHandler<JoinChat> ClientJoined;
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

                    _parser.AddChunk(buffer, readCount);

                    ChatPDU chatPdu;
                    while (_parser.TryParse(out chatPdu))
                    {
                        var joinChat = chatPdu as JoinChat;
                        if (joinChat != null)
                        {
                            Name = joinChat.Name;
                            ClientJoined?.Invoke(this, joinChat);
                            _authorized = true;
                        }

                        var publicMessage = chatPdu as PublicMessage;
                        if (publicMessage != null)
                        {
                            ClientSentPublicMessage?.Invoke(this, publicMessage);
                        }

                        var privateMessage = chatPdu as PrivateMessage;
                        if (privateMessage != null)
                        {
                            ClientSentPrivateMessage?.Invoke(this, privateMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    break;
                }

                Thread.Sleep(1);
            }

            if (_authorized)
                ClientDisconnected?.Invoke(this, EventArgs.Empty);

            Socket.Dispose();
        }

        public void Send(ChatPDU pdu)
        {
            if (pdu == null)
                throw new ArgumentNullException(nameof(pdu));

            var bytes = pdu.Serialize();

            try
            {
                var position = 0;
                do
                {
                    position += Socket.Send(bytes, position, bytes.Length - position, SocketFlags.None);
                } while (position < bytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}