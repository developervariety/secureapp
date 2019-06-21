using System;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.Packets 
{
    [Serializable]
    internal class ObjectArrayPacket : IPacket
    {
        public object[] Data { get; set; }
        public ObjectArrayPacket(object[] d) 
        {
            Data = d;
        }
    }
}
