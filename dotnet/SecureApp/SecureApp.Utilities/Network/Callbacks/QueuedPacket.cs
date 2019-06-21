using System;

namespace SecureApp.Utilities.Network.Callbacks 
{
    internal class QueuedPacket 
    {
        public byte[] Data { get; }
        private readonly Action<SecureSocketConnectedClient> _afterSend;
        
        public QueuedPacket(byte[] data, Action<SecureSocketConnectedClient> after = null)
        {
            Data = data;
            _afterSend = after;
        }

        public void HasBeenSent(SecureSocketConnectedClient sender)
        {
            _afterSend?.Invoke(sender);
        }
    }
}
