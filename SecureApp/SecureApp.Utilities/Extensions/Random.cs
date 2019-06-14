using System;

namespace SecureAppUtil.Extensions
{
    public class Random
    {
        public static Guid Guid()
        {
            return System.Guid.NewGuid();
        }
    }
}