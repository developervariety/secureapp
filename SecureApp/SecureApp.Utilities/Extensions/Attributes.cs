using System;

namespace SecureAppUtil.Extensions
{
    public class Attributes
    {
        [AttributeUsage(AttributeTargets.Method)]
        public class ServerCall : Attribute
        { 
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class ServerExecution : Attribute
        {
        }
    }
}