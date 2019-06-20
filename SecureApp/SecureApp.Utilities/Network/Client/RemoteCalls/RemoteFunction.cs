using System;
using System.Threading;
using SecureApp.Utilities.Model.Enum.RemoteCalls;
using SecureApp.Utilities.Network.RemoteCalls;

namespace SecureApp.Utilities.Network.Client.RemoteCalls 
{
    public delegate void RemoteFunctionCallback<T>(RemoteFunction<T> function, T returnValue, Guid callId);
    
    public abstract class RemoteFunction
    {
        public RemoteFunctionStatus LastStatus { get; internal set; }
        public abstract void SetReturnValue(object val, Guid callId);
    }

    public abstract class RemoteFunction<T> : RemoteFunction
    {
        public T LastValue { get; internal set; }

        public event RemoteFunctionCallback<T> ReturnCallback;
        public abstract Guid Call(params object[] args);
        public abstract T CallWait(params object[] args);

        protected void RaiseReturn(T val, Guid callId)
        {
            LastValue = val;
            ReturnCallback?.Invoke(this, val, callId);
        }
    }

    internal class InternalRemoteFunction<T> : RemoteFunction<T>
    {
        private readonly SecureSocketClient _client;
        private readonly object _syncLock = new object();
        private readonly string _name;

        private Guid _callId;
        private T _returnValue;
       

        public InternalRemoteFunction(SecureSocketClient client, string name)
        {
            _name = name;
            _client = client;
        }

        public override void SetReturnValue(object val, Guid callId)
        {
            lock (_syncLock)
            {
                if (val == null)
                    _returnValue = default;
                else
                    _returnValue = (T)val;

                _callId = callId;
                RaiseReturn(_returnValue, _callId);
                Monitor.PulseAll(_syncLock);
            }
        }

        public override Guid Call(object[] args)
        {
            Guid id = Guid.NewGuid();
            
            _client.Send(new RemoteCallRequest
            {
                Name = _name,
                CallId = id,
                Args = args
            });
            
            return id;
        }
        
        public override T CallWait(object[] args)
        {
            lock (_syncLock)
            {
                Guid id = Call(args);
                do {
                    Monitor.Wait(_syncLock);
                } while (_callId != id);
                return _returnValue;
            }
        }
    }
}
