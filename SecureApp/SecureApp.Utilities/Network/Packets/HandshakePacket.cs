using System;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.Packets
{
    [Serializable]
    internal class HandshakePacket : IPacket
    {
        public bool Success { get; set; }
        public Guid Id { get; set; }

        public HandshakePacket(bool success, Guid id)
        {
            Success = success;
            Id = id;
        }
    }
}
