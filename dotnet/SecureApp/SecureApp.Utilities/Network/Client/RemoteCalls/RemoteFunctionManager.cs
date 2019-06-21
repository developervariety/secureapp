using System.Collections.Generic;
using SecureApp.Utilities.Network.RemoteCalls;

namespace SecureApp.Utilities.Network.Client.RemoteCalls 
{
    internal class RemoteFunctionManager 
    {

        private readonly Dictionary<string, RemoteFunction> _functionList = new Dictionary<string, RemoteFunction>();
        private readonly object _syncLock = new object();

        public RemoteFunction<T> GetFunction<T>(string name) 
        {
            lock (_syncLock)
            {
                if (_functionList.ContainsKey(name))
                    return _functionList[name] as RemoteFunction<T>;
                return null;
            }
        }

        public RemoteFunction<T> RegisterFunction<T>(SecureSocketClient client, string name) 
        {
            lock (_syncLock)
            {
                if (_functionList.ContainsKey(name))
                {
                    return _functionList[name] as RemoteFunction<T>;
                }

                InternalRemoteFunction<T> f = new InternalRemoteFunction<T>(client, name);
                _functionList.Add(name, f);
                return f;

            }
        }

        public void RaiseFunction(RemoteCallResponse resp) 
        {
            lock (_syncLock)
            {
                if (!_functionList.ContainsKey(resp.Name)) return;
                
                RemoteFunction f = _functionList[resp.Name];
                f.LastStatus = resp.Response;
                f.SetReturnValue(resp.Return, resp.CallId);
            }
        }
    }
}
