using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SecureApp.Utilities.Model.Enum.RemoteCalls;
using SecureApp.Utilities.Network.RemoteCalls;

namespace SecureApp.Utilities.Network.Server.RemoteCalls 
{
    internal class RemoteCallServerManager 
    {
        private readonly Dictionary<Type, uint> _bindableTypes;
        private readonly Dictionary<string, RemoteFunctionBind> _functionLookup = new Dictionary<string, RemoteFunctionBind>();
        private RemoteFunctionCallAuth _defaultAuthCallback;
        private readonly object _syncLock = new object();

        public RemoteCallServerManager(Packager packer) 
        {
            _bindableTypes = packer.GetTypeDictionary();
        }
        
        public void SetDefaultAuthCallback(RemoteFunctionCallAuth defaultAuthCallback) 
        {
            _defaultAuthCallback = defaultAuthCallback;
        }
        
        public RemoteFunctionBind BindRemoteCall(string name, Delegate a) 
        {
            if (_functionLookup.ContainsKey(name))
                throw new Exception("Function with the same name already exists");

            RemoteFunctionInformation funcInfo = new RemoteFunctionInformation {Name = name};

            if (!_bindableTypes.ContainsKey(a.Method.ReturnType))
                throw new Exception("Return type is not serializable.");

            ParameterInfo[] funcParamInfo = a.Method.GetParameters();
            funcInfo.ReturnType = _bindableTypes[a.Method.ReturnType];
            funcInfo.Parameters = new uint[funcParamInfo.Length];

            for (int i = 0; i < funcParamInfo.Length; i++) {
                Type t = funcParamInfo[i].ParameterType;
                if (!_bindableTypes.ContainsKey(t))
                    throw new Exception($"Parameter type {t} not serializable.");
                funcInfo.Parameters[i] = _bindableTypes[t];
            }

            RemoteFunctionBind remoteFunc = new RemoteFunctionBind(funcInfo, a);
            remoteFunc.SetAuthFunc(_defaultAuthCallback);
            _functionLookup.Add(name, remoteFunc);
            return remoteFunc;
        }

        public void HandleClientFunctionCall(SecureSocketConnectedClient client, RemoteCallRequest request) 
        {
            lock (_syncLock) {
                RemoteCallResponse respPacket = new RemoteCallResponse(request.CallId, request.Name);

                if (_functionLookup.ContainsKey(request.Name)) {
                    RemoteFunctionBind func = _functionLookup[request.Name];

                    if (request.Args.Select(t => t.GetType()).Where((argType, i) => !_bindableTypes.ContainsKey(argType) || !func.ValidParameter(i, _bindableTypes[argType])).Any())
                    {
                        respPacket.Response = RemoteFunctionStatus.InvalidParameters;
                        client.Send(respPacket);
                        return;
                    }

                    func.Invoke(client, respPacket, request.Args);

                } else {
                    respPacket.Response = RemoteFunctionStatus.DoesNotExist;
                }

                client.Send(respPacket);
            }
        }
    }
}
