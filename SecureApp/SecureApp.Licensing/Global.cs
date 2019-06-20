using SecureApp.Utilities.Network.Client;

namespace SecureApp.Licensing
{
    public class Global
    {
        public static SecureSocketClient Socket { get; set; }
        
        public void Init()
        {
            Socket = new SecureSocketClient();
        }
    }
}