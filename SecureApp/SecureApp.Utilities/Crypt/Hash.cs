using System.Security.Cryptography;
using System.Text;

namespace SecureApp.Crypt
{
    public static class Hash
    {
        public static byte[] Sha256Hash(string val)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(val);
            using (SHA256CryptoServiceProvider cryptoServiceProvider = new SHA256CryptoServiceProvider())
            {
                return cryptoServiceProvider.ComputeHash(strBytes);
            }
        }
    }
}