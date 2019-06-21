using SecureApp.Utilities.Network.Client;

namespace SecureApp.Licensing
{
    public class Global
    {
        public static SecureSocketClient Socket { get; set; }
        
        public static void Init()
        {
            Socket = new SecureSocketClient();
            
            Socket.Connect("localhost", 100);
        }
    }
}