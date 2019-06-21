using System;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.Packets
{
    [Serializable]
    internal class SharedSecretPacket : IPacket
    {
        public string Secret { get; set; }
        public ISecureSocketEncryption Encryption { get; set; }

        public SharedSecretPacket(string secret, ISecureSocketEncryption encryption)
        {
            Secret = secret;
            Encryption = encryption;
        }
    }
}