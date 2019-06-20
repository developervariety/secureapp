using System;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.RemoteCalls {
    [Serializable]
    internal class RemoteCallRequest : IPacket {
        public string Name { get; set; }
        public object[] Args { get; set; }
        public Guid CallId { get; set; }
    }
}
