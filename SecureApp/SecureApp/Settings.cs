using System.Collections.Generic;
using SecureAppUtil.Model.Interface;

namespace SecureApp
{
    public class Settings
    {
        public static class Rsa
        {
            public static string PublicKey { get; set; }
            public static string PrivateKey { get; set; }
        }
        
        public static List<IPlugin> Plugins { get; set; }
    }
}