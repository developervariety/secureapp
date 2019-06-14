using System;
using SecureApp.Model.Interface;
using SecureApp.Model.Networking;
using SecureAppUtil.Extensions.Networking;

namespace SecureApp.Core.Command
{
    public class Handshake : ICommand
    {
        public void Work(object[] payload, ClientSession clientSession, Socket.Server.SocketClient socketClient)
        {
            string decryptionCode = (string)payload[2];

            if (!decryptionCode.Equals("app"))
                // do something, maybe a blacklist for x minutes?
                Console.WriteLine("waiting..");

            clientSession.Handshake = true;
            
            string server = "\x9E\x8D\x9A\x81\x8E\x91";
 
            for (int ONXjG = 0, KnCmq = 0; ONXjG < 6; ONXjG++)
            {
                KnCmq = server[ONXjG];
                KnCmq ^= 0xE8;
                server = server.Substring(0, ONXjG) + (char)(KnCmq & 0xFF) + server.Substring(ONXjG + 1);
            }

            socketClient.Send(Guid.Empty, "Handshake", server);
        }
    }
}