using System.Linq;
using SecureAppUtil.Crypt;
using SecureAppUtil.Model.Interface;

namespace SecureApp.Core
{
    public class Global
    {
        public static readonly Polymorphic Polymorphic = new Polymorphic();
        public static readonly Rsa Rsa = new Rsa();
        
        public static readonly IPlugin Logger = Settings.Plugins.FirstOrDefault(p => p.Name == "Logger");
    }
}