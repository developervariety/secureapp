using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SecureApp.Crypt;
using SecureApp.Extensions;

namespace SecureApp.Networking
{
    public static class ESock
    {
        #region " eSock Server "

        public class Server
        {
            #region " Delegates "

            public delegate void OnClientConnectCallback(Server sender, ESockClient client);
            public delegate void OnClientDisconnectCallback(Server sender, ESockClient client, SocketError er);
            public delegate bool OnClientConnectingCallback(Server sender, Socket cSock);
            public delegate void OnDataRetrievedCallback(Server sender, ESockClient client, object[] data);

            #endregion

            #region " Events "

            public event OnClientConnectCallback OnClientConnect;
            public event OnClientDisconnectCallback OnClientDisconnect;
            public event OnClientConnectingCallback OnClientConnecting;
            public event OnDataRetrievedCallback OnDataRetrieved;

            #endregion

            #region " Variables and Properties "

            private readonly Socket _globalSocket;
            private int _bufferSize = 1000000;

            public int BufferSize
            {
                private get => _bufferSize;
                set
                {
                    if (value < 5)
                        throw new ArgumentOutOfRangeException("BufferSize");
                    if (IsRunning)
                        throw new Exception("Cannot set buffer size while server is running.");
                    _bufferSize = value;
                }
            }
            
            public bool IsRunning { get; private set; }
            public ESockServerEncryptionSettings Encryption { get; private set; }

            #endregion

            #region " Constructors "

            public Server()
            {
                _globalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Encryption = new ESockServerEncryptionSettings();
                IsRunning = false;
            }
            public Server(AddressFamily socketaddressFamily)
                : this()
            {
                _globalSocket = new Socket(socketaddressFamily, SocketType.Stream, ProtocolType.Tcp);
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

            #region " Encryption "

            public class ESockServerEncryptionSettings
            {
                public ESockServerEncryptionSettings()
                {
                    DefaultClientEncryption = new Aes();
                    DefaultEncryptionKey = string.Empty;
                    EnableEncryptionOnConnect = false;
                }
            
                public IESockEncryption DefaultClientEncryption { get; set; }
                public string DefaultEncryptionKey { get; set; }
                public bool EnableEncryptionOnConnect { get; set; }
            }

            #endregion

            #region " Callbacks "

            private void AcceptCallback(IAsyncResult ar)
            {
                if (!IsRunning)
                    return;
                Socket cSock = _globalSocket.EndAccept(ar);
                if (OnClientConnecting != null)
                {
                    if (!OnClientConnecting(this, cSock))
                        return;
                }

                ESockClient client = new ESockClient(cSock, BufferSize, Encryption.DefaultClientEncryption)
                {
                    Encryption =
                    {
                        Key = Encryption.DefaultEncryptionKey, Enabled = Encryption.EnableEncryptionOnConnect
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
                ESockClient client = (ESockClient)ar.AsyncState;
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
                    catch
                    {
                        // ignored
                    }
                }
            }

            #endregion

            public class ESockClient : IDisposable
            {
                public byte[] Buffer { get; set; }
                public object Tag { get; set; }
                public Socket NetworkSocket { get; private set; }
                public ESockEncryptionSettings Encryption { get; set; }
                public ESockClient(Socket cSock)
                {
                    NetworkSocket = cSock;
                    Buffer = new byte[8192];
                }

                public ESockClient(Socket cSock, int bufferSize)
                {
                    Encryption = new ESockEncryptionSettings();
                    NetworkSocket = cSock;
                    Buffer = new byte[bufferSize];
                }

                public ESockClient(Socket cSock, int bufferSize, IESockEncryption method)
                {
                    Encryption = new ESockEncryptionSettings(method);
                    NetworkSocket = cSock;
                    Buffer = new byte[bufferSize];
                }

                public void Send(params object[] args)
                {
                    try
                    {
                        byte[] serializedData = Formatter.Serialize(args);
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

                        NetworkSocket.BeginSend(packet, 0, packet.Length, SocketFlags.None, EndSend, null);
                    }
                    catch
                    {
                        //Not connected
                    }
                }
                private void EndSend(IAsyncResult ar)
                {
                    NetworkSocket.EndSend(ar, out SocketError _);
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

        #region " eSock Client "

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

            private readonly Socket _globalSocket;
            private int _bufferSize = 1000000;
            public bool Connected { get; private set; }
            private byte[] PacketBuffer { get; set; }
            private ESockEncryptionSettings Encryption { get; set; }
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
                _globalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Connected = false;
                Encryption = new ESockEncryptionSettings();
            }

            public Client(AddressFamily socketAddressFamily)
                : this()
            {
                _globalSocket = new Socket(socketAddressFamily, SocketType.Stream, ProtocolType.Tcp);
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

            public bool Connect(IPEndPoint endpoint)
            {
                try
                {
                    _globalSocket.Connect(endpoint);
                    OnConnected();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public void ConnectAsync(string ip, int port)
            {
                _globalSocket.BeginConnect(ip, port, OnConnectAsync, null);
            }

            public void ConnectAsync(IPEndPoint endpoint)
            {
                _globalSocket.BeginConnect(endpoint, OnConnectAsync, null);
            }

            private void OnConnectAsync(IAsyncResult ar)
            {
                try
                {
                    _globalSocket.EndConnect(ar);
                    if (OnConnect != null)
                        OnConnect(this, true);
                    OnConnected();
                }
                catch
                {
                    if (OnConnect != null)
                        OnConnect(this, false);
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

                byte[] packet = null;

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
                    byte[] packet = null;

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

            public object[] ServerCall(params object[] args)
            {
                byte[] serializedData = Formatter.Serialize(args);
                if (Encryption != null)
                    serializedData = Encryption.Encrypt(serializedData);
                byte[] packet = null;

                using (MemoryStream packetStream = new MemoryStream())
                using (BinaryWriter packetWriter = new BinaryWriter(packetStream))
                {
                    packetWriter.Write(serializedData.Length);
                    packetWriter.Write(serializedData);
                    packet = packetStream.ToArray();
                }

                _globalSocket.Send(packet);
                int packetLength = _globalSocket.Receive(PacketBuffer);
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
                            packet = null;
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

                            return Formatter.Deserialize<object[]>(packet);
                        }
                    }
                    catch
                    {
                        return new object[0];
                    }
                }
                return new object[0];
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
                            byte[] packet = null;
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
                    catch
                    {
                        // ignored
                    }
                }

                       
            }

            #endregion
        }


        #endregion

        #region " eSock Encryption "

        public class ESockEncryptionSettings
        {
            private IESockEncryption Method { get; set; }
            public bool Enabled { get; set; }
            public string Key { get; set; }

            private bool UseSeparateEncryptionDecryptionKeys { get; set; }
            private string EncryptionKey { get; set; }
            private string DecryptionKey { get; set; }
            
            public ESockEncryptionSettings()
            {
                Enabled = false;
                Key = string.Empty;
                Method = new Aes();
            }
            
            public ESockEncryptionSettings(IESockEncryption method)
            {
                Enabled = false;
                Key = string.Empty;
                Method = method;
            }
            
            public byte[] Encrypt(byte[] input)
            {
                try
                {
                    if (Enabled)
                    {
                        if (Method == null)
                            throw new Exception("No method");
                        return Method.Encrypt(input, UseSeparateEncryptionDecryptionKeys ? EncryptionKey : Key);
                    }
                }
                catch
                {
                    // ignored
                }

                return input;
            }
            
            public byte[] Decrypt(byte[] input)
            {
                try
                {
                    if (Enabled)
                    {
                        if (Method == null)
                            throw new Exception("No method");
                        return Method.Decrypt(input, UseSeparateEncryptionDecryptionKeys ? DecryptionKey : Key);
                    }
                }
                catch
                {
                    // ignored
                }

                return input;
            }
        }

        public interface IESockEncryption
        {
            byte[] Encrypt(byte[] input, string key);
            byte[] Decrypt(byte[] input, string key);
        }

        #endregion
    }
}