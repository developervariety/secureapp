using System;
using SecureApp.Exceptions;
using SecureApp.Networking;

namespace SecureApp
{
    public class Base
    {
        private static readonly ESock.Client Client = new ESock.Client();

        public Base()
        {
            Client.OnDataRetrieved += _client_OnDataRetrieved;
            Client.OnDisconnect += _client_OnDisconnect;

            if (Client.Connect("127.0.0.1", 0709))
            {
                Client.Send(Guid.Empty, (byte) NetworkPacket.Handshake, "Handshake Pending");
            }
            else
            {
                throw new ServerUnavailableException();
            }
        }

        #region " Network Methods "

        public static object ServerCall(string function, object[] args)
        {
            try
            {
                object[] objectArray = Client.ServerCall((byte)NetworkPacket.ServerCall, function, args);
                return objectArray[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        #endregion

        public static void Initialize(string productId)
        {
            if (Client.Connected)
            {
                Client.Send(Guid.Empty, (byte)NetworkPacket.Initialization, "Username", $"{Environment.UserName}/{Environment.MachineName}");
            }
            else
            {
                throw new NotConnectedToServerException();
            }
        }
        
        #region " Network Callbacks "
        
        private static void _client_OnDisconnect(ESock.Client sender, System.Net.Sockets.SocketError er)
        {
            
        }

        private static void _client_OnDataRetrieved(ESock.Client sender, object[] data)
        {
            try
            {
                Guid id = (Guid)data[0];
                if (id != Guid.Empty) return;
                NetworkPacket command = (NetworkPacket) data[1];

                switch (command)
                {
                    case NetworkPacket.Handshake:

                        if ((string) data[2] != "Handshake Approved")
                        {
                            throw new HandshakeDeclinedException();
                        }
                        
                        break;
                    
                    case NetworkPacket.Initialization:
                        
                        
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        
        #endregion
    }
}