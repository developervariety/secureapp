using System;
using SecureAppUtil.Crypt;
using SecureAppUtil.Model.Interface;

namespace SecureAppUtil.Networking
{
    public class CryptSettings
    {
        private ISocketEncryption Method { get; }
        public bool Enabled { private get; set; }
        public string Key { get; set; }

        public CryptSettings()
        {
            Enabled = false;
            Key = string.Empty;
            Method = new Aes();
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