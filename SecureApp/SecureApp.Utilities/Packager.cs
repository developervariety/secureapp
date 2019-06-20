using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using NetSerializer;
using SecureApp.Utilities.Model.Interface;
using SecureApp.Utilities.Network.Packets;
using SecureApp.Utilities.Network.RemoteCalls;
using Aes = SecureApp.Utilities.Cryptography.Aes;

namespace SecureApp.Utilities 
{
    public class Packager 
    {
        private readonly Serializer _nsSerializer;
        private static readonly RNGCryptoServiceProvider Rnd = new RNGCryptoServiceProvider();

        #region " Constructors "
        
        public Packager(Type[] manualTypes)
        {
            List<Type> addTypes = new List<Type>(new[] {
                    typeof(HandshakePacket),
                    typeof(SharedSecretPacket),
                    typeof(ObjectArrayPacket),
                    typeof(RemoteFunctionInfomation),
                    typeof(RemoteCallRequest),
                    typeof(RemoteCallResponse),
                   
                    typeof(Guid),
                    typeof(Guid[]),

                    typeof(byte),
                    typeof(int),
                    typeof(uint),
                    typeof(short),
                    typeof(ushort),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(char),
                    typeof(bool),
                    typeof(decimal),
                    typeof(object),
                    typeof(string),

                    typeof(byte[]),
                    typeof(int[]),
                    typeof(uint[]),
                    typeof(short[]),
                    typeof(ushort[]),
                    typeof(long[]),
                    typeof(ulong[]),
                    typeof(float[]),
                    typeof(double[]),
                    typeof(char[]),
                    typeof(bool[]),
                    typeof(decimal[]),
                    typeof(string[])
             });
            
            if(manualTypes != null) {
                addTypes.AddRange(manualTypes);
            }
            
            _nsSerializer = new Serializer(addTypes);
        }
        
        public Packager() : this(null) 
        {
        }
        
        #endregion
        
        public static byte[] RandomBytes(int size) 
        {
            byte[] b = new byte[size];
            Rnd.GetNonZeroBytes(b);
            return b;
        }
        
        public byte[] Pack(IPacket p) 
        {
            return Pack(p, null);
        }

        public IPacket Unpack(byte[] data) 
        {
            return Unpack(data, null);
        }
        
        internal byte[] Pack(IPacket p, PackConfig cfg) 
        {
            using (MemoryStream ms = new MemoryStream()) 
            {
                _nsSerializer.Serialize(ms, p);
                byte[] data = ms.ToArray();
                
                if (cfg != null)
                    data = cfg.PostPacking(data);
                
                return data;
            }
        }

        internal IPacket Unpack(byte[] data, PackConfig cfg) 
        {
            if (cfg != null)
                data = cfg.PreUnpacking(data);

            using (MemoryStream ms = new MemoryStream(data))
                return (IPacket)_nsSerializer.Deserialize(ms);
        }

        internal byte[] PackArray(object[] arr, PackConfig cfg) 
        {
            return Pack(new ObjectArrayPacket(arr), cfg);
        }
        
        public ISecureSocketEncryption NewRijndaelEncryption(byte[] key) 
        {
            if (key.Length != 16)
                throw new ArgumentException("key needs to be 16 bytes long.", nameof(key));

            return new Aes(key);
        }

        public Dictionary<Type, uint> GetTypeDictionary() 
        {
            return _nsSerializer.GetTypeMap();
        }
    }
}
