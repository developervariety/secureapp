using System;

namespace SecureApp.Utilities.Model.Interface {
    public interface ISecureSocketEncryption : IDisposable {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }
}
