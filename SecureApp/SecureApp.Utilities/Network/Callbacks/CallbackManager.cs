using System;
using System.Collections.Generic;
using SecureApp.Utilities.Model.Interface;
using SecureApp.Utilities.Network.Packets;

namespace SecureApp.Utilities.Network.Callbacks 
{
    internal class CallbackManager<TClientType> where TClientType : ISecureSocketClient 
    {
        private Action<TClientType, object[]> _arrayHandler;
        private Action<TClientType, IPacket> _genericHandler;
        private readonly Dictionary<Type, PacketCallback<TClientType>> _packetCallbacks = new Dictionary<Type, PacketCallback<TClientType>>();
        private readonly object _callbackLock = new object();

        public void SetArrayHandler(Action<TClientType, object[]> callback)
        {
            lock (_callbackLock) {
                _arrayHandler = callback;
            }
        }
        
        public void RemoveHandler<T>()
        {
            lock (_callbackLock) {
                Type fType = typeof(T);
                if (_packetCallbacks.ContainsKey(fType))
                    _packetCallbacks.Remove(fType);
            }
        }
        
        public void SetHandler<T>(Action<TClientType, T> callback) where T : class, IPacket
        {
            PacketCallback<TClientType> cb = PacketCallback<TClientType>.Create(callback);
            
            lock (_callbackLock) {
                if (_packetCallbacks.ContainsKey(cb.Type))
                    _packetCallbacks[cb.Type] = cb;
                else
                    _packetCallbacks.Add(cb.Type, cb);
            }
        }

        public void SetPacketHandler(Action<TClientType, IPacket> callback)
        {
            lock (_callbackLock) {
                _genericHandler = callback;
            }
        }

        private bool RaisePacketHandler(Type packetType, TClientType client, IPacket data)
        {
            PacketCallback<TClientType> callback = null;
            
            lock (_callbackLock) {
                if (_packetCallbacks.ContainsKey(packetType))
                    callback = _packetCallbacks[packetType];
            }
            
            if (callback == null) {
                return false;
            }

            callback.Raise(client, data);
            return true;
        }

        public void Handle(TClientType client, IPacket data)
        {
            Type packetType = data.GetType();

            if (packetType == typeof(ObjectArrayPacket)) {
                _arrayHandler?.Invoke(client, ((ObjectArrayPacket)data).Data);
            } else {
                if (!RaisePacketHandler(packetType, client, data))
                    _genericHandler?.Invoke(client, data);
            }
        }
    }
}