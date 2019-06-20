using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using SecureApp.Utilities.Model.Enum;
using SecureApp.Utilities.Model.Interface;
using SecureApp.Utilities.Network.Callbacks;
using SecureApp.Utilities.Network.Packets;
using SecureApp.Utilities.Network.RemoteCalls;
using SecureApp.Utilities.Network.Server.RemoteCalls;

namespace SecureApp.Utilities.Network.Server 
{
    public delegate void OnClientConnectDelegate(SecureSocketServer sender, SecureSocketConnectedClient client);
    
    public class SecureSocketServer : IEnumerable<SecureSocket>
    {
        public event OnClientConnectDelegate OnClientConnect;
        public TransportProtocol Protocol { get; }
        public ClientManager Clients { get; private set; }

        private readonly Packager _packager;
        private readonly CallbackManager<SecureSocketConnectedClient> _callbacks;
        private readonly RemoteCallServerManager _remoteFuncs;
        private Func<Guid> _guidGenerator = Guid.NewGuid;
        private readonly List<SecureSocket> _openSockets = new List<SecureSocket>();

        private SecureSocketServer(TransportProtocol protocol, Packager packager) 
        {
            Protocol = protocol;
            _packager = packager;
            _callbacks = new CallbackManager<SecureSocketConnectedClient>();
            Clients = new ClientManager();
            _remoteFuncs = new RemoteCallServerManager(_packager);

            SetHandler<RemoteCallRequest>((cl, att) => _remoteFuncs.HandleClientFunctionCall(cl, att));
        }

        public SecureSocketServer() : this(TransportProtocol.IPv4, new Packager()) 
        {
        }

        public SecureSocket StartServer(int port) 
        {
            BaseServerSocket baseSock = new BaseServerSocket(Protocol);
            baseSock.OnClientConnect += TcpSock_OnClientConnect;
            
            if (!baseSock.BeginAccept(port))
                return null;

            _openSockets.Add(baseSock);
            baseSock.OnClose += (s, err) => {
                _openSockets.Remove(s);
            };
            
            return baseSock;
        }

        private void TcpSock_OnClientConnect(BaseServerSocket sender, Socket s) 
        {
            InternalSecureSocketConnectedClient client = new InternalSecureSocketConnectedClient(s, _packager);

            client.SetId(_guidGenerator());
            client.BeginReceive(ReceiveHandler);
            client.Send(cl => {
                Clients.Add(cl);
                client.OnDisconnect += (c, err) => {
                    Clients.Remove(c);
                };

                OnClientConnect?.Invoke(this, cl);
            }, new HandshakePacket(true, client.Id));
        }

        private void ReceiveHandler(InternalSecureSocketConnectedClient client, IPacket data) 
        {
            _callbacks.Handle(client, data);
        }
        
        public void SetGuidGenerator(Func<Guid> call) 
        {
            if (call == null)
                return;
            
            _guidGenerator = call;
        }
        
        public void SetHandler(Action<SecureSocketConnectedClient, object[]> callback) 
        {
            _callbacks.SetArrayHandler(callback);
        }

        public void SetHandler<T>(Action<SecureSocketConnectedClient, T> callback) where T : class, IPacket 
        {
            _callbacks.SetHandler(callback);
        }

        public void SetHandler(Action<SecureSocketConnectedClient, IPacket> callback) 
        {
            _callbacks.SetPacketHandler(callback);
        }

        public RemoteFunctionBind RegisterRemoteFunction(string name, Delegate func) 
        {
            return _remoteFuncs.BindRemoteCall(name, func);
        }

        public void SetDefaultRemoteFunctionAuthCallback(RemoteFunctionCallAuth defaultAuthCallback) 
        {
            _remoteFuncs.SetDefaultAuthCallback(defaultAuthCallback);
        }

        public IEnumerator<SecureSocket> GetEnumerator() 
        {
            return _openSockets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() 
        {
            return _openSockets.GetEnumerator();
        }
    }
}