using System;
using SecureApp.Utilities.Model.Enum.RemoteCalls;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.RemoteCalls
{
    [Serializable]
    internal class RemoteCallResponse : IPacket
    {
        public string Name { get; set; }
        public RemoteFunctionStatus Response { get; set; }
        public object Return { get; set; }
        public Guid CallId { get; set; }

        public RemoteCallResponse(Guid callId, string name)
        {
            CallId = callId;
            Name = name;
        }
    }
}
