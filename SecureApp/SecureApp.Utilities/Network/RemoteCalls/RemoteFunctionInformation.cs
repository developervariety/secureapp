using System;
using SecureApp.Utilities.Model.Enum;
using SecureApp.Utilities.Model.Enum.RemoteCalls;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.RemoteCalls 
{
    [Serializable]
    internal class RemoteFunctionInformation : IPacket 
    {
        public RemoteFunctionResponse Response { get; set; }

        public string Name { get; set; }
        public uint[] Parameters { get; set; }
        public uint ReturnType { get; set; }
    }
}
