using System;
using SecureAppUtil.Crypt;
using SecureAppUtil.Model.Interface;

namespace SecureAppUtil.Networking
{
    public class SocketEncryptionSettings
    {
        public ISocketEncryption Method { get; set; }
        public bool Enabled { get; set; }
        public string Key { get; set; }

        public SocketEncryptionSettings()
        {
            Enabled = false;
            Key = string.Empty;
            Method = new Aes();
        }

        public SocketEncryptionSettings(ISocketEncryption method)
        {
            Enabled = false;
            Key = string.Empty;
            Method = method;
        }

        public byte[] Encrypt(byte[] input)
        {
            try
            {
                if (Enabled)
                {
                    if (Method == null)
                        throw new Exception("No method");
                    return Method.Encrypt(input, Key);
                }
            }
            catch
            {

            }

            return input;
        }

        public byte[] Decrypt(byte[] input)
        {
            try
            {
                if (Enabled)
                {
                    if (Method == null)
                        throw new Exception("No method");
                    return Method.Decrypt(input, Key);
                }
            }
            catch
            {

            }

            return input;
        }
    }
}