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
            
            string server = "\u1803\u6003\u4803\u2803\u7003\uA003";
 
            for (int LwloM = 0, LuZMc = 0; LwloM < 6; LwloM++)
            {
                LuZMc = server[LwloM];
                LuZMc = ((LuZMc << 5) | ( (LuZMc & 0xFFFF) >> 11)) & 0xFFFF;
                server = server.Substring(0, LwloM) + (char)(LuZMc & 0xFFFF) + server.Substring(LwloM + 1);
            }
            socketClient.Send(Guid.Empty, "Handshake", server);
        }
    }
}