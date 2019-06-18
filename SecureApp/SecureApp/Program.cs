using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SecureApp.Core;
using SecureApp.Model.Networking;
using SecureAppUtil.Networking;
using Random = SecureAppUtil.Extensions.Random;

namespace SecureApp
{
    internal static class Program
    {
        private static readonly Dictionary<Guid, ClientSession>
            ConnectedClients = new Dictionary<Guid, ClientSession>();

        private static readonly SecureSocket.Server Server = new SecureSocket.Server();

        public static void Main()
        {
            Server.OnClientConnect += OnClientConnect;
            Server.OnClientConnecting += OnClientConnecting;
            Server.OnClientDisconnect += OnClientDisconnect;
            Server.OnDataRetrieved += OnDataRetrieved;

            Tuple<string, string> rsaKeypair = Global.Rsa.GenerateKeypair();
            Settings.Rsa.PublicKey = rsaKeypair.Item1;
            Settings.Rsa.PrivateKey = rsaKeypair.Item2;

            Console.WriteLine(Server.Start(0709) ? "Server is live!" : "Server failed to go live!");

            for (;;)
            {
            }
        }

        #region " Network Callbacks "

        private static void OnDataRetrieved(SecureSocket.Server sender, SecureSocket.Server.SocketClient socketClient,
            object[] data)
        {
            lock (socketClient)
            {
                ClientSession clientSession = (ClientSession) socketClient.Tag;

//                Packet packet = JsonConvert.DeserializeObject<Packet>((string) data[0]);
//                
//                Console.WriteLine(packet.packet.command);

                // TODO:: find a better way to do this..
//                Type type = Type.GetType($"SecureApp.Core.Command.{data[1]}");
//                object obj = Activator.CreateInstance(type ?? throw new InvalidOperationException());
//                MethodInfo methodInfo = type.GetMethod("Work");
//                methodInfo?.Invoke(obj, new object[] {data, clientSession, socketClient});
            }
        }

        private static void OnClientDisconnect(SecureSocket.Server sender, SecureSocket.Server.SocketClient socketClient,
            SocketError er)
        {
            ClientSession clientSession = (ClientSession) socketClient.Tag;

            if (ConnectedClients.ContainsKey(clientSession.Id))
                ConnectedClients.Remove(clientSession.Id);
        }

        private static bool OnClientConnecting(SecureSocket.Server sender, System.Net.Sockets.Socket csock)
        {
            return true;
        }

        private static void OnClientConnect(SecureSocket.Server sender, SecureSocket.Server.SocketClient socketClient)
        {
            ClientSession clientSession = new ClientSession(Random.Guid(), socketClient);

            ConnectedClients.Add(clientSession.Id, clientSession);
            socketClient.Tag = clientSession;
        }

        #endregion
    }
}