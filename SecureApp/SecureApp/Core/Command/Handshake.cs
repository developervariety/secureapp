using System;
using SecureApp.Model.Interface;
using SecureApp.Model.Networking;
using SecureAppUtil.Networking;

namespace SecureApp.Core.Command
{
    public class Handshake : ICommand
    {
        public void Work(object[] payload, ClientSession clientSession, SecureSocket.Server.SocketClient socketClient)
        {
            string polyString = Global.Polymorphic.Decrypt((string) payload[2], "dasdasd");

            if (!polyString.Equals("app"))
                // do something, maybe a blacklist for x minutes?
                Console.WriteLine("waiting..");

            clientSession.Handshake = true;

            socketClient.Send(Guid.Empty, "Handshake");
        }
    }
}