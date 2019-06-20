using System;
using SecureApp.Utilities.Model.Interface;
using SecureApp.Utilities.Network.RemoteCalls;

namespace SecureApp.Utilities.Network.Server.RemoteCalls 
{
    public delegate bool RemoteFunctionCallAuth(SecureSocketConnectedClient client, RemoteFunctionBind func);
    
    public class RemoteFunctionBind 
    {
        public string Name => _functionInfo.Name;
        public object Tag { get; set; }

        private readonly RemoteFunctionInfomation _functionInfo;
        private RemoteFunctionCallAuth _authCallback;

        private readonly Delegate _functionCall;


        internal RemoteFunctionBind(RemoteFunctionInfomation info, Delegate functionCall) 
        {
            _functionInfo = info;
            SetAuthFunc(null);

            _functionCall = functionCall;
            
            if (_functionCall == null)
                throw new ArgumentNullException(nameof(functionCall));
        }

        public void SetAuthFunc(RemoteFunctionCallAuth callback) 
        {
            _authCallback = callback;
        }

        internal void Invoke(SecureSocketConnectedClient client, RemoteCallResponse resp, object[] param) 
        {
            try {
                if(_authCallback?.Invoke(client, this) ?? true) {
                    resp.Return = _functionCall.DynamicInvoke(param);
                    resp.Reponce = FunctionResponseStatus.Success;
                } else {
                    resp.Reponce = FunctionResponseStatus.PermissionDenied;
                }
            }catch {
                resp.Reponce = FunctionResponseStatus.ExceptionThrown;
            }
        }

        public bool ValidParameter(int index, uint id)
        {
            if (index < _functionInfo.Parameters.Length)
                return _functionInfo.Parameters[index] == id;
            return false;
        }

        public IPacket GetFunctionInfo() 
        {
            return _functionInfo;
        }
    }
}
