using System;
using SecureApp.Core;
using SecureApp.Utilities.Network;
using SecureApp.Utilities.Network.Server;

namespace SecureApp
{
    public class Callbacks
    {
        public Callbacks()
        {
            Global.Socket.OnClientConnect += OnClientConnect;
        }

        private static void OnClientConnect(SecureSocketServer sender, SecureSocketConnectedClient client)
        {
            Global.Logger?.Execute($"New client connection, IP Address: {client.EndPoint}.");
            
            client.OnDisconnect += OnClientDisconnect;
        }

        private static void OnClientDisconnect(SecureSocketConnectedClient client, Exception ex)
        {
            Global.Logger?.Execute($"Client disconnected. IP Address: {client.EndPoint}, Reason: {ex}");
        }
    }
}