using System;

namespace Server
{
    public class ClientSendMessageEventArgs
    {
        public const string AllClients = "*";

        public string SourceClient { get; }

        public string TargetClient { get; }

        public string Message { get; }

        public ClientSendMessageEventArgs(string sourceClient, string targetClient, string message)
        {
            if (sourceClient == null)
                throw new ArgumentNullException(nameof(sourceClient));

            if (targetClient == null)
                throw new ArgumentNullException(nameof(targetClient));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            SourceClient = sourceClient;
            TargetClient = targetClient;
            Message = message;
        }

        public ClientSendMessageEventArgs(string sourceClient, string message)
        {
            if (sourceClient == null)
                throw new ArgumentNullException(nameof(sourceClient));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            SourceClient = sourceClient;
            TargetClient = AllClients;
            Message = message;
        }
    }
}