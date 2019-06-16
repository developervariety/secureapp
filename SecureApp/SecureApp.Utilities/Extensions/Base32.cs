using System;
using System.Collections.Generic;
using System.Text;

namespace SecureAppUtil.Extensions
{
    public class Base32
    {
        private const string EncodingTable = "ybndrfg8ejkmcpqxot1uwisza345h769";

        private static readonly byte[] DecodingTable = new byte[128];

        static Base32()
        {
            for (int i = 0; i < DecodingTable.Length; ++i)
            {
                DecodingTable[i] = byte.MaxValue;
            }

            for (int i = 0; i < EncodingTable.Length; ++i)
            {
                DecodingTable[EncodingTable[i]] = (byte)i;
            }
        }

        public static string Encode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            StringBuilder encodedResult = new StringBuilder((int)Math.Ceiling(data.Length * 8.0 / 5.0));

            for (int i = 0; i < data.Length; i += 5)
            {
                int byteCount = Math.Min(5, data.Length - i);
                
                ulong buffer = 0;
                for (int j = 0; j < byteCount; ++j)
                {
                    buffer = (buffer << 8) | data[i + j];
                }

                int bitCount = byteCount * 8;
                while (bitCount > 0)
                {                    
                    int index = bitCount >= 5
                        ? (int)(buffer >> (bitCount - 5)) & 0x1f
                        : (int)(buffer & (ulong)(0x1f >> (5 - bitCount))) << (5 - bitCount);

                    encodedResult.Append(EncodingTable[index]);
                    bitCount -= 5;
                }
            }

            return encodedResult.ToString();
        }

        public static byte[] Decode(string data)
        {
            if (data == string.Empty)
            {
                return new byte[0];
            }

            List<byte> result = new List<byte>((int)Math.Ceiling(data.Length * 5.0 / 8.0));

            int[] index = new int[8];
            for (int i = 0; i < data.Length;)
            {
                i = CreateIndexByOctetAndMovePosition(ref data, i, ref index);

                int shortByteCount = 0;
                ulong buffer = 0;
                for (int j = 0; j < 8 && index[j] != -1; ++j)
                {
                    buffer = (buffer << 5) | (ulong)(DecodingTable[index[j]] & 0x1f);
                    shortByteCount++;
                }

                int bitCount = shortByteCount * 5;
                while (bitCount >= 8)
                {
                    result.Add((byte)((buffer >> (bitCount - 8)) & 0xff));
                    bitCount -= 8;
                }
            }

            return result.ToArray();
        }

        private static int CreateIndexByOctetAndMovePosition(ref string data, int currentPosition, ref int[] index)
        {
            int j = 0;
            while (j < 8)
            {
                if (currentPosition >= data.Length)
                {
                    index[j++] = -1;
                    continue;
                }

                if (IgnoredSymbol(data[currentPosition]))
                {
                    currentPosition++;
                    continue;
                }

                index[j] = data[currentPosition];
                j++;
                currentPosition++;
            }

            return currentPosition;
        }

        private static bool IgnoredSymbol(char checkedSymbol)
        {
            return checkedSymbol >= DecodingTable.Length || DecodingTable[checkedSymbol] == byte.MaxValue;
        }
    }
}