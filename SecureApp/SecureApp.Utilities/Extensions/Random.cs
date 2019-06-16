using System;
using System.Security.Cryptography;

namespace SecureAppUtil.Extensions
{
    public class Random
    {
        private static readonly System.Random Rnd = new System.Random();
        
        public static Guid Guid()
        {
            return System.Guid.NewGuid();
        }

        public static int Int()
        {
            return Rnd.Next(int.MaxValue);
        }
    }
}