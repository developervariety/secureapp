using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SecureAppUtil.Extensions.Compression;

namespace SecureAppUtil.Extensions
{
    public static class Formatter
    {
        public static byte[] Serialize(object input)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, input);
                return Gzip.Compress(ms.ToArray());
            }
        }

        public static TT Deserialize<TT>(byte[] input)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(Gzip.Decompress(input)))
                {
                    return (TT)bf.Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[eSock] {0}", ex);
                return default;
            }
        }
    }
}