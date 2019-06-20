using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SecureApp.Utilities.Model.Enum;
using SecureApp.Utilities.Model.Interface;
using SecureApp.Utilities.Network.Callbacks;
using SecureApp.Utilities.Network.Client.RemoteCalls;
using SecureApp.Utilities.Network.Packets;
using SecureApp.Utilities.Network.RemoteCalls;

namespace SecureApp.Utilities.Network.Client 
{
    public delegate void OnHandshakeDelegate(SecureSocketClient sender, Guid id, bool success);
    public delegate void OnDisconnectDelegate(SecureSocketClient sender, Exception ex);

    public class SecureSocketClient : SecureSocket, ISecureSocketClient 
    {
        public event OnHandshakeDelegate OnHandshake;
        public event OnDisconnectDelegate OnDisconnect;

        public Guid Id => _connection?.Id ?? Guid.Empty;
        public TransportProtocol Protocol { get; }
        public bool Connected => Id != Guid.Empty;

        private readonly CallbackManager<SecureSocketClient> _callbacks;
        private readonly RemoteFunctionManager _remoteFunctions;
        private InternalSecureSocketConnectedClient _connection;
        private readonly Packager _packager;
        private ManualResetEvent _handshakeEvent = new ManualResetEvent(false);
        private readonly TimeSpan _handshakeTimeout = TimeSpan.FromSeconds(15);

        public SecureSocketClient(TransportProtocol protocol, Packager packager) 
        {
            Protocol = protocol;
            _packager = packager;
            _callbacks = new CallbackManager<SecureSocketClient>();
            _remoteFunctions = new RemoteFunctionManager();

            _callbacks.SetHandler<HandshakePacket>((c, p) => {
                bool handshakeComplete = p.Success;
                _connection.SetId(p.Id);
                _handshakeEvent?.Set();
                _handshakeEvent?.Dispose();
                _handshakeEvent = null;
                OnHandshake?.Invoke(this, Id, handshakeComplete);
                _callbacks.RemoveHandler<HandshakePacket>();
            });

            _callbacks.SetHandler<RemoteCallResponse>((c, p) => {
                _remoteFunctions.RaiseFunction(p);
            });
        }

        public SecureSocketClient(): this(TransportProtocol.IPv4, new Packager()) 
        {
        }

        private Socket NewSocket() 
        {
            _connection?.Disconnect(null);
            _connection = null;
            
            return Protocol == TransportProtocol.IPv6 ? new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp) : new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void SetupConnection(Socket s) 
        {
            SetTcpKeepAlive(s);
            
            EndPoint = (IPEndPoint)s.RemoteEndPoint;
            _connection = new InternalSecureSocketConnectedClient(s, _packager);
            _connection.BeginReceive(ReceiveHandler);
            _connection.OnDisconnect += Connection_OnDisconnect;
        }

        private void Connection_OnDisconnect(SecureSocketConnectedClient client, Exception ex) 
        {
            OnDisconnect?.Invoke(this, ex);
        }

        private void ReceiveHandler(InternalSecureSocketConnectedClient client, IPacket data) 
        {
            _callbacks.Handle(this, data);
        }

        public bool Connect(string host, int port) 
        {
            Socket sock = NewSocket();
            
            try {
                sock.Connect(host, port);
                SetupConnection(sock);
                return true;
            } catch {
                _connection = null;
                return false;
            }
        }

        public void SetHandler(Action<SecureSocketClient, object[]> callback) 
        {
            _callbacks.SetArrayHandler(callback);
        }

        public void SetHandler<T>(Action<SecureSocketClient, T> callback) where T : class, IPacket 
        {
            _callbacks.SetHandler(callback);
        }

        public void SetHandler(Action<SecureSocketClient, IPacket> callback) 
        {
            _callbacks.SetPacketHandler(callback);
        }

        public void Send(Action<SecureSocketConnectedClient> afterSend, params object[] data) 
        {
            if (Connected)
            {
                _connection.Send(afterSend, data);
            }
        }

        public void Send(Action<SecureSocketConnectedClient> afterSend, IPacket packet) 
        {
            if (Connected)
            {
                _connection.Send(afterSend, packet);
            }
        }

        public void Send(params object[] data) 
        {
            Send(null, data);
        }

        public void Send(IPacket packet) 
        {
            Send(null, packet);
        }

        public void SetEncryption(ISecureSocketEncryption encryption) 
        {
            if (_connection == null)
                return;

            if (_connection.PackagingConfiguration == null)
                _connection.PackagingConfiguration = new PackConfig();

            _connection.PackagingConfiguration.Encryption = encryption;
        }
        
        public bool WaitForHandshake() 
        {
            if (Connected)
                return true;

            _handshakeEvent?.WaitOne(_handshakeTimeout);

            return Connected;
        }
        
        public RemoteFunction<T> GetRemoteFunction<T>(string name) 
        {
            return _remoteFunctions.RegisterFunction<T>(this, name);
        }
    }
}
