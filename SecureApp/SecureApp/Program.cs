using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SecureApp.Crypt;
using SecureApp.Networking;
using SecureApp.Networking.Model;

namespace SecureApp
{
    internal static class Program
    {
        private static ESock.Server _server;
        private static readonly Dictionary<Guid, ClientData> ConnectedClients = new Dictionary<Guid, ClientData>();

        public static void Main()
        {
            _server = new ESock.Server {BufferSize = 8192};
            _server.OnClientConnect += _server_OnClientConnect;
            _server.OnClientConnecting += _server_OnClientConnecting;
            _server.OnClientDisconnect += _server_OnClientDisconnect;
            _server.OnDataRetrieved += _server_OnDataRetrieved;

            Console.WriteLine(_server.Start(0709) ? "Server is live!" : "Server is not live!");

            Console.Read();
        }

        #region " Network Callbacks "
        
        private static void _server_OnDataRetrieved(ESock.Server sender, ESock.Server.ESockClient client, object[] data)
        {
            lock (client)
            {
                try
                {
                    ClientData clientData = (ClientData) client.Tag;
                    Guid guid = (Guid) data[0];
                    
                    if (guid == Guid.Empty)
                    {
                        NetworkPacket command = (NetworkPacket)data[1];

                        if (!clientData.Handshaken)
                        {
                            if (command == NetworkPacket.Handshake)
                            {
                                clientData.Handshaken = true;
                                client.Send(Guid.Empty, (byte)NetworkPacket.Handshake, "Handshake Approved");
                            }
                            else
                            {
                                if (ConnectedClients.ContainsKey(clientData.Id))
                                    ConnectedClients.Remove(clientData.Id);
                            }
                        }
                        else
                        {
                            switch (command)
                            {
                                    case NetworkPacket.Initialization:

                                        string productId = (string) data[2];
                                        
                                        
                                        break;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static void _server_OnClientDisconnect(ESock.Server sender, ESock.Server.ESockClient client, SocketError er)
        {
            ClientData clientData = (ClientData)client.Tag;
            
            if (ConnectedClients.ContainsKey(clientData.Id))
                ConnectedClients.Remove(clientData.Id);
        }

        private static bool _server_OnClientConnecting(ESock.Server sender, Socket csock)
        {
            return true;
        }

        private static void _server_OnClientConnect(ESock.Server sender, ESock.Server.ESockClient client)
        {
            ClientData clientData = new ClientData(Extensions.Random.UniqueId(ConnectedClients), client);
            
            ConnectedClients.Add(clientData.Id, clientData);
            
            client.Tag = clientData;
        }
        
        #endregion
    }
}