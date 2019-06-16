using System.IO;
using System.Security.Cryptography;
using System.Text;
using SecureAppUtil.Model.Interface;

namespace SecureAppUtil.Crypt
{
    public class Aes : ISocketEncryption
    {
        public byte[] Encrypt(byte[] input, string key)
        {
            return Encrypt(input, key, "5AL7NEUA");
        }

        public byte[] Decrypt(byte[] input, string key)
        {
            return Decrypt(input, key, "5AL7NEUA");
        }
        
        #region " Encryption "

        private static byte[] Encrypt(byte[] payload, string password, string salt)
        {
            using (PasswordDeriveBytes pwd = new PasswordDeriveBytes(password, Encoding.UTF8.GetBytes(salt)))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (System.Security.Cryptography.Aes aes = new AesManaged())
                    {
                        aes.Key = pwd.GetBytes(aes.KeySize / 8);
                        aes.IV = pwd.GetBytes(aes.BlockSize / 8);

                        //aes.Padding = PaddingMode.PKCS7;
                        
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(payload, 0, payload.Length);
                            cs.Close();
                        }
                        return ms.ToArray();
                    }
                }
            }
        }
 
        #endregion
  
        #region " Decryption "

        private static byte[] Decrypt(byte[] payload, string password, string salt)
        {
            using (PasswordDeriveBytes pwd = new PasswordDeriveBytes(password, Encoding.UTF8.GetBytes(salt)))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (System.Security.Cryptography.Aes aes = new AesManaged())
                    {
                        aes.Key = pwd.GetBytes(aes.KeySize / 8);
                        aes.IV = pwd.GetBytes(aes.BlockSize / 8);

                        //aes.Padding = PaddingMode.PKCS7;
                        
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(payload, 0, payload.Length);
                            cs.Close();
                        }
                        return ms.ToArray();
                    }
                }
            }
        }
 
        #endregion
    }
}