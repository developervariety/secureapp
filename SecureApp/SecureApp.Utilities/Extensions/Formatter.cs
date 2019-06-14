using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
                return Compression.Gzip.Compress(ms.ToArray());
            }
        }

        public static TT Deserialize<TT>(byte[] input)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(Compression.Gzip.Decompress(input)))
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