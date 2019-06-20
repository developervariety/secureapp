namespace SecureApp.Licensing
{
    public class Base
    {
        private static readonly Global Global = new Global();
        
        public Base()
        {
            Global.Init();
            
            Global.Socket.Connect("localhost", 100);
        }
    }
}