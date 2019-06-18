namespace SecureAppUtil.Model.Json
{
    public class Contents
    {
        public string command { get; set; }
        public string body { get; set; }
    }

    public class Packet
    {
        public Contents packet { get; set; }
        public string signature { get; set; }
    }
}