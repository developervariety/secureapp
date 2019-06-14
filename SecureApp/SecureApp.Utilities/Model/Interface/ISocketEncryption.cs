namespace SecureAppUtil.Model.Interface
{
    public interface ISocketEncryption
    {
        byte[] Encrypt(byte[] input, string key);
        byte[] Decrypt(byte[] input, string key);
    }
}