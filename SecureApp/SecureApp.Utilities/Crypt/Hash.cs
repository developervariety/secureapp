using System.Security.Cryptography;
using System.Text;

namespace SecureAppUtil.Crypt
{
    public static class Hash
    {
        public static byte[] Sha256(string value)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(value);
            using (SHA256CryptoServiceProvider cryptoServiceProvider = new SHA256CryptoServiceProvider())
            {
                return cryptoServiceProvider.ComputeHash(strBytes);
            }
        }
    }
}