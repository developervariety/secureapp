using System;
using SecureAppUtil.Networking;

namespace SecureApp.Model.Networking
{
    public class ClientSession : IDisposable
    {
        public Guid Id { get; set; }
        public CryptSettings Encryption => ClientSocket.Encryption;
        public Socket.Server.SocketClient ClientSocket { get; set; }

        public string HardwareId { get; set; }
        public string ProductId { get; set; }
        public string LicenseKey { get; set; }

        public bool Handshake { get; set; }
        
        public ClientSession(Guid id,Socket.Server.SocketClient socket)
        {
            Id = id;
            ClientSocket = socket;
        }
        
        public void Dispose()
        {
            ClientSocket.Dispose();
        }
    }
}