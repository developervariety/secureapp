using System;
using System.Security.Cryptography;
using SecureAppUtil.Model.Interface;

namespace SecureAppUtil.Crypt
{
    public class Rsa : ISocketEncryption
    {
        public byte[] Encrypt(byte[] input, string key)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider {PersistKeyInCsp = false})
            {
                rsa.FromXmlString(key);

                byte[] encryptedBytes = rsa.Encrypt(input, true);
                return encryptedBytes;
            }
        }

        public byte[] Decrypt(byte[] input, string key)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider {PersistKeyInCsp = false})
            {
                rsa.FromXmlString(key);

                byte[] decryptedBytes = rsa.Decrypt(input, true);
                return decryptedBytes;
            }
        }

        public Tuple<string, string> GenerateKeypair()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider {PersistKeyInCsp = false})
            {
                string publicKey = rsa.ToXmlString(false);
                string privateKey = rsa.ToXmlString(true);

                return new Tuple<string, string>(publicKey, privateKey);
            }
        }
    }
}