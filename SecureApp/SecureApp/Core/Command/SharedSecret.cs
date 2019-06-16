using System;
using SecureApp.Model.Interface;
using SecureApp.Model.Networking;
using SecureAppUtil.Networking;

namespace SecureApp.Core.Command
{
    public class SharedSecret : ICommand
    {
        public void Work(object[] payload, ClientSession clientSession, Socket.Server.SocketClient socketClient)
        {
            // TODO:: create a method to check if handshake is valid and if not remove the client and add to a blacklist

            clientSession.Encryption.Key = (string) payload[2];
            clientSession.Encryption.Enabled = true;

            socketClient.Encryption = clientSession.Encryption;
            
            socketClient.Send(Guid.Empty, "SharedSecret", "Successfully shared the key.");
        }
    }
}