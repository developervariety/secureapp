using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using SecureApp.Model.Networking;
using Random = SecureAppUtil.Extensions.Random;

namespace SecureApp
{
    internal static class Program
    {
        private static readonly Dictionary<Guid, ClientSession> ConnectedClients = new Dictionary<Guid, ClientSession>();
        private static readonly SecureAppUtil.Networking.Socket.Server Server = new SecureAppUtil.Networking.Socket.Server();

        public static void Main()
        {
            Server.OnClientConnect += OnClientConnect;
            Server.OnClientConnecting += OnClientConnecting;
            Server.OnClientDisconnect += OnClientDisconnect;
            Server.OnDataRetrieved += OnDataRetrieved;

            Console.WriteLine(Server.Start(0709) ? "Server is live!" : "Server failed to go live!");
        }


        #region " Network Callbacks "
        
        private static void OnDataRetrieved(SecureAppUtil.Networking.Socket.Server sender, SecureAppUtil.Networking.Socket.Server.SocketClient socketClient, object[] data)
        {            
            lock (socketClient)
            {
                ClientSession clientSession = (ClientSession) socketClient.Tag;
                Guid guid = (Guid) data[0];

                if (guid != Guid.Empty) return;
                
                // TODO:: find a better way to do this..
                Type type = Type.GetType($"SecureApp.Core.Command.{data[1]}");
                object obj = Activator.CreateInstance(type ?? throw new InvalidOperationException());
                MethodInfo methodInfo = type.GetMethod("Work");
                methodInfo?.Invoke(obj, new object[] {data, clientSession, socketClient});
            }
        }

        private static void OnClientDisconnect(SecureAppUtil.Networking.Socket.Server sender, SecureAppUtil.Networking.Socket.Server.SocketClient socketClient, SocketError er)
        {            
            ClientSession clientData = (ClientSession)socketClient.Tag;
            
            if (ConnectedClients.ContainsKey(clientData.Id))
                ConnectedClients.Remove(clientData.Id);
        }

        private static bool OnClientConnecting(SecureAppUtil.Networking.Socket.Server sender, Socket csock)
        {
            return true;
        }

        private static void OnClientConnect(SecureAppUtil.Networking.Socket.Server sender, SecureAppUtil.Networking.Socket.Server.SocketClient socketClient)
        {
            ClientSession clientSession = new ClientSession(Random.Guid(), socketClient);
            
            ConnectedClients.Add(clientSession.Id, clientSession);
            socketClient.Tag = clientSession;
        }
        
        #endregion
    }
}