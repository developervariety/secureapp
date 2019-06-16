using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SecureAppUtil.Extensions;

namespace SecureAppUtil.Networking
{
    /// <summary>
    /// eSock 2.0 by BahNahNah (modified)
    /// uid=2388291
    /// </summary>
    public static class Socket
    {
        #region " Server "

        public class Server
        {
            #region " Delegates "

            public delegate void OnClientConnectCallback(Server sender, SocketClient client);
            public delegate void OnClientDisconnectCallback(Server sender, SocketClient client, SocketError er);
            public delegate bool OnClientConnectingCallback(Server sender, System.Net.Sockets.Socket cSock);
            public delegate void OnDataRetrievedCallback(Server sender, SocketClient client, object[] data);

            #endregion

            #region " Events "

            public event OnClientConnectCallback OnClientConnect;
            public event OnClientDisconnectCallback OnClientDisconnect;
            public event OnClientConnectingCallback OnClientConnecting;
            public event OnDataRetrievedCallback OnDataRetrieved;

            #endregion

            #region " Variables and Properties "

            private readonly System.Net.Sockets.Socket _globalSocket;
            private const int BufferSize = 1000000;

            public bool IsRunning { get; private set; }
            public CryptSettings Encryption { get; private set; }

            #endregion

            #region " Constructors "

            public Server()
            {
                _globalSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Encryption = new CryptSettings();
                IsRunning = false;
            }

            #endregion

            #region " Functions "

            public bool Start(int port)
            {
                if (IsRunning)
                    throw new Exception("Server is already running.");
                try
                {
                    _globalSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                    _globalSocket.Listen(5);
                    _globalSocket.BeginAccept(AcceptCallback, null);
                    IsRunning = true;
                }
                catch
                {
                    IsRunning = false;
                }
                return IsRunning;
            }

            public bool Start(int port, int backlog)
            {
                if (IsRunning)
                    throw new Exception("Server is already running.");
                try
                {
                    _globalSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                    _globalSocket.Listen(backlog);
                    _globalSocket.BeginAccept(AcceptCallback, null);
                    IsRunning = true;
                }
                catch
                {
                    return false;
                }
                return IsRunning;
            }

            public void Stop()
            {
                IsRunning = false;
                _globalSocket.Close();

            }
            #endregion

            #region " Callbacks "

            private void AcceptCallback(IAsyncResult ar)
            {
                if (!IsRunning)
                    return;
                System.Net.Sockets.Socket cSock = _globalSocket.EndAccept(ar);
                if (OnClientConnecting != null)
                {
                    if (!OnClientConnecting(this, cSock))
                        return;
                }

                SocketClient client = new SocketClient(cSock, BufferSize)
                {
                    Encryption =
                    {
                        Key = Encryption.Key
                    }
                };

                OnClientConnect?.Invoke(this, client);
                client.NetworkSocket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None,
                    RetrieveCallback, client);
                _globalSocket.BeginAccept(AcceptCallback, null);
            }

            private void RetrieveCallback(IAsyncResult ar)
            {
                if (!IsRunning)
                    return;
                SocketClient client = (SocketClient)ar.AsyncState;
                int packetLength = client.NetworkSocket.EndReceive(ar, out SocketError se);
                if (se != SocketError.Success)
                {
                    OnClientDisconnect?.Invoke(this, client, se);
                    return;
                }
                byte[] packetCluster = new byte[packetLength];
                Buffer.BlockCopy(client.Buffer, 0, packetCluster, 0, packetLength);

                using (MemoryStream bufferStream = new MemoryStream(packetCluster))
                using (BinaryReader packetReader = new BinaryReader(bufferStream))
                {
                    try
                    {
                        while (bufferStream.Position < bufferStream.Length)
                        {
                            int length = packetReader.ReadInt32();

                            byte[] packet;
                            if (length > bufferStream.Length - bufferStream.Position)
                            {
                                using (MemoryStream receivePacketChunks = new MemoryStream(length))
                                {
                                    byte[] buffer = new byte[bufferStream.Length - bufferStream.Position];

                                    buffer = packetReader.ReadBytes(buffer.Length);
                                    receivePacketChunks.Write(buffer, 0, buffer.Length);

                                    while (receivePacketChunks.Position != length)
                                    {
                                        packetLength = client.NetworkSocket.Receive(client.Buffer);
                                        buffer = new byte[packetLength];
                                        Buffer.BlockCopy(client.Buffer, 0, buffer, 0, packetLength);
                                        receivePacketChunks.Write(buffer, 0, buffer.Length);
                                    }
                                    packet = receivePacketChunks.ToArray();
                                }
                            }
                            else
                            {
                                packet = packetReader.ReadBytes(length);
                            }

 
                            if (client.Encryption != null)
                                packet = client.Encryption.Decrypt(packet);

                            object[] retrievedData = Formatter.Deserialize<object[]>(packet);
                            if (OnDataRetrieved != null && retrievedData != null)
                                OnDataRetrieved(this, client, retrievedData);

                            client.NetworkSocket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, RetrieveCallback, client);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            #endregion

            public class SocketClient : IDisposable
            {
                public byte[] Buffer { get; set; }
                public object Tag { get; set; }
                public System.Net.Sockets.Socket NetworkSocket { get; private set; }
                public CryptSettings Encryption { get; set; }
                
                public SocketClient(System.Net.Sockets.Socket cSock)
                {
                    NetworkSocket = cSock;
                    Buffer = new byte[8192];
                }

                public SocketClient(System.Net.Sockets.Socket cSock, int bufferSize)
                {
                    Encryption = new CryptSettings();
                    NetworkSocket = cSock;
                    Buffer = new byte[bufferSize];
                }

                public void Send(params object[] args)
                {
                    try
                    {
                        byte[] serializedData = Formatter.Serialize(args);
                        if (Encryption != null)
                        {
                            serializedData = Encryption.Encrypt(serializedData);
                        }

                        byte[] packet;
                        using (MemoryStream packetStream = new MemoryStream())
                        using (BinaryWriter packetWriter = new BinaryWriter(packetStream))
                        {
                            packetWriter.Write(serializedData.Length);
                            packetWriter.Write(serializedData);
                            packet = packetStream.ToArray();
                        }

                        NetworkSocket.BeginSend(packet, 0, packet.Length, SocketFlags.None, EndSend, null);
                    }
                    catch
                    {
                        //Not connected
                    }
                }
                private void EndSend(IAsyncResult ar)
                {
                    SocketError se;
                    NetworkSocket.EndSend(ar, out se);
                }

                public void Dispose()
                {
                    if (NetworkSocket.Connected)
                    {
                        NetworkSocket.Shutdown(SocketShutdown.Both);
                        NetworkSocket.Disconnect(true);
                    }
                    NetworkSocket.Close(1000);
                }
            }
        }

        #endregion

        #region " Client "

        public class Client
        {
            #region " Delegates "

            public delegate void OnConnectAsyncCallback(Client sender, bool success);
            public delegate void OnDisconnectCallback(Client sender, SocketError er);
            public delegate void OnDataRetrievedCallback(Client sender, object[] data);

            #endregion

            #region " Events "

            public event OnConnectAsyncCallback OnConnect;
            public event OnDisconnectCallback OnDisconnect;
            public event OnDataRetrievedCallback OnDataRetrieved;

            #endregion

            #region " Variables and Properties "

            private readonly System.Net.Sockets.Socket _globalSocket;
            private int _bufferSize = 1000000;
            public bool Connected { get; private set; }
            public byte[] PacketBuffer { get; private set; }
            public CryptSettings Encryption { get; set; }
            public int BufferSize
            {
                get => _bufferSize;
                set
                {
                    if (Connected)
                        throw new Exception("Can not change buffer size while connected");
                    if (value < 5)
                        throw new ArgumentOutOfRangeException("BufferSize");
                    _bufferSize = value;
                }
            }

            #endregion

            #region " Constructor "

            public Client()
            {
                _globalSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Connected = false;
                Encryption = new CryptSettings();
            }
            
            #endregion"

            #region " Connect "

            public bool Connect(string ip, int port)
            {
                try
                {
                    _globalSocket.Connect(ip, port);
                    OnConnected();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private void OnConnected()
            {
                PacketBuffer = new byte[_bufferSize];
                _globalSocket.BeginReceive(PacketBuffer, 0, PacketBuffer.Length, SocketFlags.None, EndRetrieve, null);
            }

            #endregion

            #region " Functions "

            public void Send(params object[] data)
            {
                byte[] serializedData = Formatter.Serialize(data);
                if (Encryption != null)
                    serializedData = Encryption.Encrypt(serializedData);

                byte[] packet;

                using (MemoryStream packetStream = new MemoryStream())
                using (BinaryWriter packetWriter = new BinaryWriter(packetStream))
                {
                    packetWriter.Write(serializedData.Length);
                    packetWriter.Write(serializedData);
                    packet = packetStream.ToArray();
                }

                _globalSocket.BeginSend(packet, 0, packet.Length, SocketFlags.None, EndSend, null);
            }

            public void SendWait(params object[] data)
            {
                lock (this)
                {
                    byte[] serializedData = Formatter.Serialize(data);
                    if (Encryption != null)
                        serializedData = Encryption.Encrypt(serializedData);
                    byte[] packet;

                    using (MemoryStream packetStream = new MemoryStream())
                    using (BinaryWriter packetWriter = new BinaryWriter(packetStream))
                    {
                        packetWriter.Write(serializedData.Length);
                        packetWriter.Write(serializedData);
                        packet = packetStream.ToArray();
                    }

                    _globalSocket.Send(packet);
                    Thread.Sleep(10);
                }
            }

            private void EndSend(IAsyncResult ar)
            {
                _globalSocket.EndSend(ar, out SocketError _);
            }

            #endregion

            #region " Callbacks "

            private void EndRetrieve(IAsyncResult ar)
            {
                int packetLength = _globalSocket.EndReceive(ar, out SocketError se);
                if (se != SocketError.Success)
                {
                    OnDisconnect?.Invoke(this, se);
                    return;
                }
                byte[] packetCluster = new byte[packetLength];
                Buffer.BlockCopy(PacketBuffer, 0, packetCluster, 0, packetLength);
            

                using (MemoryStream bufferStream = new MemoryStream(packetCluster))
                using (BinaryReader packetReader = new BinaryReader(bufferStream))
                {
                    try
                    {
                        while (bufferStream.Position < bufferStream.Length)
                        {
                            int length = packetReader.ReadInt32();
                            byte[] packet;
                            if (length > bufferStream.Length - bufferStream.Position)
                            {
                                using (MemoryStream receivePacketChunks = new MemoryStream(length))
                                {
                                    byte[] buffer = new byte[bufferStream.Length - bufferStream.Position];

                                    buffer = packetReader.ReadBytes(buffer.Length);
                                    receivePacketChunks.Write(buffer, 0, buffer.Length);

                                    while (receivePacketChunks.Position != length)
                                    {
                                        packetLength = _globalSocket.Receive(PacketBuffer);
                                        buffer = new byte[packetLength];
                                        Buffer.BlockCopy(PacketBuffer, 0, buffer, 0, packetLength);
                                        receivePacketChunks.Write(buffer, 0, buffer.Length);
                                    }
                                    packet = receivePacketChunks.ToArray();
                                }
                            }
                            else
                            {
                                packet = packetReader.ReadBytes(length);
                            }

                            if (Encryption != null)
                                packet = Encryption.Decrypt(packet);

                            object[] data = Formatter.Deserialize<object[]>(packet);
                            if (OnDataRetrieved != null && data != null)
                                OnDataRetrieved(this, data);

                            _globalSocket.BeginReceive(PacketBuffer, 0, PacketBuffer.Length, SocketFlags.None, EndRetrieve, null);
                        }
                    }
                    catch (Exception ex) { throw ex; }
                }   
            }

            #endregion
        }


        #endregion
    }
}