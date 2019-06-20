using System;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.RemoteCalls {

    [Serializable]
    internal class RemoteFunctionInfomation : IPacket {

        public RemoteFunctionResponce Reponce { get; set; }

        public string Name { get; set; }
        public uint[] Parameters { get; set; }
        public uint ReturnType { get; set; }

    }

    enum RemoteFunctionResponce : byte {
        DoesNotExist,
        Success,
        PermissionDenied
    }
}
