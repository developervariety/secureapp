using System;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.RemoteCalls
{
    [Serializable]
    internal class RemoteCallResponse : IPacket
    {
        public string Name { get; set; }
        public FunctionResponseStatus Reponce { get; set; }
        public object Return { get; set; }
        public Guid CallID { get; set; }

        public RemoteCallResponse(Guid _callId, string _name)
        {
            CallID = _callId;
            Name = _name;
        }
    }


    public enum FunctionResponseStatus : byte
    {
        Success,
        PermissionDenied,
        ExceptionThrown,
        DoesNotExist,
        InvalidParameters
    }
}
