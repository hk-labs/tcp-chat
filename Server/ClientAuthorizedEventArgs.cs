using System;

namespace Server
{
    public class ClientAuthorizedEventArgs
    {
        public string Client { get; }

        public ClientAuthorizedEventArgs(string client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Client = client;
        }
    }
}