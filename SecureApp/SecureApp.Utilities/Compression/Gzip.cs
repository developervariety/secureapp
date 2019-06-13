using System.IO;
using System.IO.Compression;

namespace SecureApp.Compression
{
    public static class Gzip
    {
        public static byte[] Compress(byte[] input)
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

        public static byte[] Decompress(byte[] input)
        {
            using (MemoryStream decompressed = new MemoryStream())
            {
                using (MemoryStream ms = new MemoryStream(input))
                {
                    using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        byte[] byteBuffer = new byte[1024];
                        int bytesRead = 0;
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