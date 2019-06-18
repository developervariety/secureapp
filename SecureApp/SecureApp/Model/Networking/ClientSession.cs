using System;
using SecureAppUtil.Networking;

namespace SecureApp.Model.Networking
{
    public class ClientSession : IDisposable
    {
        public ClientSession(Guid id, SecureSocket.Server.SocketClient socket)
        {
            Id = id;
            ClientSocket = socket;
        }

        public Guid Id { get; set; }
        public SocketEncryptionSettings Encryption => ClientSocket.Encryption;
        public SecureSocket.Server.SocketClient ClientSocket { get; set; }

        public bool Handshake { get; set; }

        public void Dispose()
        {
            ClientSocket.Dispose();
        }
    }
}