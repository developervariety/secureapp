using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SecureApp.Utilities.Model.Interface;
using SecureApp.Utilities.Network.Callbacks;

namespace SecureApp.Utilities.Network 
{
    public delegate void OnClientDisconnectDelegate(SecureSocketConnectedClient client, Exception ex);

    public abstract class SecureSocketConnectedClient : ISecureSocketClient 
    {
        public event OnClientDisconnectDelegate OnDisconnect;
        
        public Guid Id { get; protected set; }
        public object Tag { get; set; }
        public IPEndPoint EndPoint => (IPEndPoint)NetworkSocket.RemoteEndPoint;
        protected Socket NetworkSocket { get; set; }

        internal PackConfig PackagingConfiguration { get; set; }

        public void Send(params object[] data) 
        {
            Send(null, data);
        }

        public void Send(IPacket packet) 
        {
            Send(null, packet);
        } 

        public virtual void Send(Action<SecureSocketConnectedClient> afterSend, params object[] data) 
        {
        }

        public virtual void Send(Action<SecureSocketConnectedClient> afterSend, IPacket packet) 
        {
        }

        public void Disconnect(Exception ex) 
        {
            if (NetworkSocket == null) return;
            NetworkSocket.Shutdown(SocketShutdown.Both);
            NetworkSocket.Dispose();
            NetworkSocket = null;
            OnDisconnect?.Invoke(this, ex);
        }

        public void SetEncryption(ISecureSocketEncryption encryption) 
        {
            if (PackagingConfiguration == null)
                PackagingConfiguration = new PackConfig();

            PackagingConfiguration.Encryption = encryption;
        }
    }
    
    internal class InternalSecureSocketConnectedClient : SecureSocketConnectedClient 
    {
        public Packager Packager { get; }

        private readonly object _syncLock = new object();

        private readonly Queue<QueuedPacket> _sendQueue = new Queue<QueuedPacket>();
        
        private readonly PacketDefragmenter _defragger;
        
        private Action<InternalSecureSocketConnectedClient, IPacket> _receiveCallback;

        public InternalSecureSocketConnectedClient(Socket s, Packager p, int bufferSize = 1024 * 5) {
            NetworkSocket = s ?? throw new Exception("Socket not valid.");
            Packager = p;
            _defragger = new PacketDefragmenter(bufferSize);
        }


        public override void Send(Action<SecureSocketConnectedClient> afterSend, object[] arr) {
            byte[] data = Packager.PackArray(arr, PackagingConfiguration);
            HandleRawBytes(data, afterSend);
        }

        public override void Send(Action<SecureSocketConnectedClient> afterSend, IPacket packet) {
            byte[] data = Packager.Pack(packet);
            HandleRawBytes(data, afterSend);
        }

        private void HandleRawBytes(IReadOnlyCollection<byte> data, Action<SecureSocketConnectedClient> afterSend) {
            byte[] packet = BitConverter.GetBytes(data.Count).Concat(data).ToArray();//Appending length prefix to packet
            lock (_syncLock) {
                _sendQueue.Enqueue(new QueuedPacket(packet, afterSend));
                Task.Factory.StartNew(HandleSendQueue);
            }
        }

        private void HandleSendQueue() {
            QueuedPacket packet;

            lock (_syncLock) {
                packet = _sendQueue.Dequeue();
            }

            if (packet?.Data == null) return;
            SocketError se = SocketError.SocketError;

            NetworkSocket?.Send(packet.Data, 0, packet.Data.Length, SocketFlags.None, out se);

            if (se != SocketError.Success) {
                Disconnect(new SocketException());
                return;
            }
            packet.HasBeenSent(this);
        }
        
        public void BeginReceive(Action<InternalSecureSocketConnectedClient, IPacket> callback) 
        {

            if (_receiveCallback != null)
                throw new Exception("Already listening.");

            _receiveCallback = callback;
            if (_receiveCallback == null)
                throw new Exception("Invalid callback");

            ReceiveWithDefragger();
        }

        private void ReceiveWithDefragger() 
        {
            NetworkSocket.BeginReceive(_defragger.ReceiveBuffer, _defragger.BufferIndex, _defragger.BytesToReceive, SocketFlags.None, out SocketError se, InternalReceive, null);

            if (se != SocketError.Success)
                Disconnect(new SocketException());
        }

        private void InternalReceive(IAsyncResult ar) 
        {
            SocketError se = SocketError.SocketError;
            int bytes = NetworkSocket?.EndReceive(ar, out se) ?? 0;

            if (se != SocketError.Success) {
                Disconnect(new SocketException());
                return;
            }

            byte[] packet = _defragger.Process(bytes);

            ReceiveWithDefragger();

            if (packet == null) return;
            try {
                IPacket pack = Packager.Unpack(packet, PackagingConfiguration);
                _receiveCallback(this, pack);
            } catch (Exception ex){
                Disconnect(ex);
                return;
            }

        }

        public void SetId(Guid id) 
        {
            Id = id;
        }
    }
}