using System;
using SecureAppUtil.Extensions.Crypt;
using SecureAppUtil.Model.Interface;

namespace SecureAppUtil.Extensions.Networking
{
    public class CryptSettings
    {
        public ISocketEncryption Method { get; set; }
        public bool Enabled { get; set; }
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