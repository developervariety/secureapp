using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using SecureApp.Core;
using SecureApp.Model.Networking;
using SecureAppUtil.Model.Interface;
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
            Global.Logger?.Execute("Generated RSA keypair.");
            
            Plugins.LoadPlugins();
            Global.Logger?.Execute($"Loaded {Settings.Plugins.Count} plugin(s).");
            
            foreach (IPlugin plugin in Settings.Plugins)
            {
                plugin.Init();
            }
            
            if (Server.Start(0709))
                Global.Logger?.Execute("Server successfully started.");
            else
            {
                Global.Logger?.Execute("Server failed to successfully start.");
            }

            for (;;)
            {
            }
        }

        #region " Network Callbacks "

        private static void OnDataRetrieved(SecureSocket.Server sender, SecureSocket.Server.SocketClient socket,
            object[] data)
        {
            lock (socket)
            {
                ClientSession client = (ClientSession) socket.Tag;

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

        private static void OnClientDisconnect(SecureSocket.Server sender, SecureSocket.Server.SocketClient socket,
            SocketError er)
        {
            ClientSession client = (ClientSession) socket.Tag;

            if (ConnectedClients.ContainsKey(client.Id))
                ConnectedClients.Remove(client.Id);
        }

        private static bool OnClientConnecting(SecureSocket.Server sender, Socket socket)
        {
            return true;
        }

        private static void OnClientConnect(SecureSocket.Server sender, SecureSocket.Server.SocketClient socket)
        {
            ClientSession clientSession = new ClientSession(Random.Guid(), socket);

            ConnectedClients.Add(clientSession.Id, clientSession);
            socket.Tag = clientSession;
        }

        #endregion
    }
}