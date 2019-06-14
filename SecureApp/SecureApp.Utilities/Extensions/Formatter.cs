using System;
using System.IO;
using System.IO.Compression;
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
                return Compress(ms.ToArray());
            }
        }

        public static TT Deserialize<TT>(byte[] input)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(Decompress(input)))
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

        private static byte[] Compress(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
                {
                    gz.Write(input, 0, input.Length);
                }
                return ms.ToArray();
            }
        }

        private static byte[] Decompress(byte[] input)
        {
            using (MemoryStream decompressed = new MemoryStream())
            {
                using (MemoryStream ms = new MemoryStream(input))
                {
                    using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        byte[] byteBuffer = new byte[1024];
                        int bytesRead;
                        while ((bytesRead = gz.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                        {
                            decompressed.Write(byteBuffer, 0, bytesRead);
                        }
                    }
                    return decompressed.ToArray();
                }
            }
        }
    }
}