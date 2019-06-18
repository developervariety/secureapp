using System.Security.Cryptography;
using System.Text;
using SecureAppUtil.Extensions;

namespace SecureAppUtil.Crypt
{
    public class Hash
    {
        public static byte[] Sha256(string value)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(value);
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                return sha256.ComputeHash(strBytes);
            }
        }

        public static string HmacSha256(string value, string key)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(value);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] bytes = hmac.ComputeHash(strBytes);
                return Base32.Encode(bytes);
            }
        }
    }
}