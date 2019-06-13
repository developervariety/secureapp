using System;

namespace SecureApp.Networking.Model
{
    public class ClientData : IDisposable
    {
        public Guid Id { get; set; }
        public ESock.ESockEncryptionSettings Encryption => ClientSocket.Encryption;
        public ESock.Server.ESockClient ClientSocket { get; set; }

        public string ProductId { get; set; }
        
        public byte[] HardwareId { get; set; }
        public byte[] LicenseKey { get; set; }
        
        public bool Handshaken { get; set; }
        
        public ClientData(Guid id, ESock.Server.ESockClient socket)
        {
            Id = id;
            Handshaken = false;
            ClientSocket = socket;
        }

        public void Dispose()
        {
            ClientSocket.Dispose();
        }
    }
}