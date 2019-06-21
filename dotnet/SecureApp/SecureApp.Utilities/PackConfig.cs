using SecureApp.Utilities.Cryptography;
using SecureApp.Utilities.Model.Enum;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities 
{
    internal class PackConfig 
    {
        public ISecureSocketEncryption Encryption { get; set; }

        public static ISecureSocketEncryption GenerateNewEncryption(KeySize size) 
        {
            return new Aes(Packager.RandomBytes((int)size));
        }
        
        public byte[] PostPacking(byte[] data) 
        {
            if (Encryption != null)
                data = Encryption.Encrypt(data);

            return data;
        }
        
        public byte[] PreUnpacking(byte[] data) 
        {
            if (Encryption != null)
                data = Encryption.Decrypt(data);

            return data;
        }
    }
}
