using System;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Network.Callbacks 
{
    internal class PacketCallback<TSenderType>  
    {
        public static PacketCallback<TSenderType> Create<T>(Action<TSenderType, T> handler) 
            where T : class, IPacket 
        {
            return new InnerPacketCallback<TSenderType, T>(handler);
        }

        public virtual void Raise(TSenderType t, IPacket packet)
        {
        }

        public virtual Type Type { get; protected set; }
        public virtual bool IsType(Type t)
        {
            return false;
        }
    }
    
    internal class InnerPacketCallback<TSt, TRt> : PacketCallback<TSt>
        where TRt : class, IPacket 
    {

        private readonly Action<TSt, TRt> _callback;
        public sealed override Type Type {get; protected set; }
        public InnerPacketCallback(Action<TSt, TRt> cb)
        {
            _callback = cb;
            Type = typeof(TRt);
        }

        public override void Raise(TSt t, IPacket packet)
        {
            TRt pass = packet as TRt;
            _callback(t, pass);
        }

        public override bool IsType(Type t)
        {
            return Type == t;
        }
    }
}
