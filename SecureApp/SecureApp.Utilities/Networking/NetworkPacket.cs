namespace SecureApp.Networking
{
    public enum NetworkPacket : byte
    {
        Handshake,
        Initialization,
        ServerCall
    }
}