using System.Security.Cryptography;
using SecureApp.Utilities.Model.Interface;

namespace SecureApp.Utilities.Cryptography
{
    public class Aes : ISecureSocketEncryption
    {
        private readonly Rijndael _rijObject;
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;
        
        public Aes(byte[] key)
        {
            _rijObject = new RijndaelManaged {Key = key, IV = key};
            _encryptor = _rijObject.CreateEncryptor();
            _decryptor = _rijObject.CreateDecryptor();
        }

        public Aes(Rijndael rijObject)
        {
            _rijObject = rijObject;
            _encryptor = _rijObject.CreateEncryptor();
            _decryptor = _rijObject.CreateDecryptor();
        }

        public byte[] Decrypt(byte[] data)
        {
            return _decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public void Dispose()
        {
            _encryptor.Dispose();
            _decryptor.Dispose();
            _rijObject.Dispose();
        }
    }
}