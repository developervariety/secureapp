using System;
using System.Net;
using System.Net.Sockets;
using SecureApp.Utilities.Model.Enum;

namespace SecureApp.Utilities.Network.Server 
{
    internal class BaseServerSocket : SecureSocket 
    {
        public event Action<BaseServerSocket, Socket> OnClientConnect;

        public TransportProtocol Protocol { get; }
        public bool Binded => (_networkSocket?.IsBound ?? false) && _successfulBind;
       
        private readonly AsyncCallback _internalAcceptHandler;
        private Socket _networkSocket;
        private bool _successfulBind;

        public BaseServerSocket(TransportProtocol protocol) 
        {
            Protocol = protocol;
            _internalAcceptHandler = HandleAccept;
        }

        public BaseServerSocket() :this(TransportProtocol.IPv4) 
        {
        }
        
        private void CreateNewSocket() 
        {
            _networkSocket?.Dispose();
            _networkSocket = Protocol == TransportProtocol.IPv6 ? new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp) : new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetTcpKeepAlive(_networkSocket);
        }

        public bool BeginAccept (EndPoint ep) 
        {
            CreateNewSocket();
            
            try {
                _networkSocket.Bind(ep);
                _networkSocket.Listen(50);
                EndPoint = (IPEndPoint)_networkSocket.LocalEndPoint;
                _successfulBind = true;
            } catch { 
                _networkSocket = null;
                _successfulBind = false;
                return false;
            } 
           
            _networkSocket.BeginAccept(_internalAcceptHandler, null);
            return true;
        }
        
        public bool BeginAccept(int port) 
        {
            return BeginAccept(new IPEndPoint(IPAddress.Any, port));
        }

        private void HandleAccept(IAsyncResult ar) 
        {
            try {
                Socket s = _networkSocket.EndAccept(ar);
                OnClientConnect?.Invoke(this, s);
            } catch (Exception ex) {
                LastError = ex;
                Close();
                return;
            }
            
            _networkSocket.BeginAccept(_internalAcceptHandler, null);
        }

        protected override void Close() 
        {
            if (!Binded) return;
            
            _networkSocket.Dispose();
            _networkSocket = null;
            _successfulBind = false;
        }
    }
}
