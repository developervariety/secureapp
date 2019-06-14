using System;
using System.IO;
using System.Security.Cryptography;
using SecureAppUtil.Model.Interface;

// ReSharper disable IdentifierTypo

namespace SecureAppUtil.Extensions.Crypt
{
    public class Aes : ISocketEncryption
    {
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        private const int BlockBitSize = 128;
        private const int KeyBitSize = 256;

        private const int SaltBitSize = 64;
        private const int Iterations = 10000;
        private const int MinPasswordLength = 12;

        public static byte[] NewKey()
        {
            byte[] key = new byte[KeyBitSize / 8];
            Random.GetBytes(key);
            return key;
        }
        
        public byte[] Encrypt(byte[] input, string key)
        {
            byte[] nonSecretPayload = null;

            if (string.IsNullOrWhiteSpace(key) || key.Length < MinPasswordLength)
                throw new ArgumentException($"Must have a password of at least {MinPasswordLength} characters!", nameof(input));

            if (input == null || input.Length ==0)
                throw new ArgumentException("Secret Message Required!", nameof(input));

            byte[] payload = new byte[SaltBitSize / 8 * 2 + nonSecretPayload.Length];

            Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
            int payloadIndex = nonSecretPayload.Length;

            byte[] cryptKey;
            byte[] authKey;
            using (Rfc2898DeriveBytes generator = new Rfc2898DeriveBytes(key, SaltBitSize / 8, Iterations))
            {
                byte[] salt = generator.Salt;

                cryptKey = generator.GetBytes(KeyBitSize / 8);

                Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
                payloadIndex += salt.Length;
            }

            using (Rfc2898DeriveBytes generator = new Rfc2898DeriveBytes(key, SaltBitSize / 8, Iterations))
            {
                byte[] salt = generator.Salt;

                authKey = generator.GetBytes(KeyBitSize / 8);

                Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
            }

            return SimpleEncrypt(input, cryptKey, authKey, payload);
        }

        public byte[] Decrypt(byte[] input, string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length < MinPasswordLength)
                throw new ArgumentException($"Must have a password of at least {MinPasswordLength} characters!", nameof(key));

            if (input == null || input.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", nameof(input));

            byte[] cryptSalt = new byte[SaltBitSize / 8];
            byte[] authSalt = new byte[SaltBitSize / 8];

            Array.Copy(input, 0, cryptSalt, 0, cryptSalt.Length);
            Array.Copy(input, 0 + cryptSalt.Length, authSalt, 0, authSalt.Length);

            byte[] cryptKey;
            byte[] authKey;

            using (Rfc2898DeriveBytes generator = new Rfc2898DeriveBytes(key, cryptSalt, Iterations))
            {
                cryptKey = generator.GetBytes(KeyBitSize / 8);
            }
            using (Rfc2898DeriveBytes generator = new Rfc2898DeriveBytes(key, authSalt, Iterations))
            {
                authKey = generator.GetBytes(KeyBitSize / 8);
            }

            return SimpleDecrypt(input, cryptKey, authKey, cryptSalt.Length + authSalt.Length);
        }

        private static byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
        {
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException($"Key needs to be {KeyBitSize} bit!", nameof(cryptKey));

            if (authKey == null || authKey.Length != KeyBitSize / 8)
                throw new ArgumentException($"Key needs to be {KeyBitSize} bit!", nameof(authKey));

            if (secretMessage == null || secretMessage.Length < 1)
                throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            byte[] cipherText;
            byte[] iv;

            using (AesManaged aes = new AesManaged
            {
                KeySize = KeyBitSize,
                BlockSize = BlockBitSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {

                aes.GenerateIV();
                iv = aes.IV;

                using (ICryptoTransform encrypter = aes.CreateEncryptor(cryptKey, iv))
                using (MemoryStream cipherStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                    using (BinaryWriter binaryWriter = new BinaryWriter(cryptoStream))
                    {
                        binaryWriter.Write(secretMessage);
                    }

                    cipherText = cipherStream.ToArray();
                }

            }

            using (HMACSHA256 hmac = new HMACSHA256(authKey))
            using (MemoryStream encryptedStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(encryptedStream))
                {
                    binaryWriter.Write(nonSecretPayload);
                    binaryWriter.Write(iv);
                    binaryWriter.Write(cipherText);
                    binaryWriter.Flush();

                    byte[] tag = hmac.ComputeHash(encryptedStream.ToArray());
                    binaryWriter.Write(tag);
                }
                return encryptedStream.ToArray();
            }

        }

        private static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
        {

            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException($"CryptKey needs to be {KeyBitSize} bit!", nameof(cryptKey));

            if (authKey == null || authKey.Length != KeyBitSize / 8)
                throw new ArgumentException($"AuthKey needs to be {KeyBitSize} bit!", nameof(authKey));

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

            using (HMACSHA256 hmac = new HMACSHA256(authKey))
            {
                byte[] sentTag = new byte[hmac.HashSize / 8];
                byte[] calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
                const int ivLength = BlockBitSize / 8;

                if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
                    return null;

                Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

                int compare = 0;
                for (int i = 0; i < sentTag.Length; i++)
                    compare |= sentTag[i] ^ calcTag[i]; 

                if (compare != 0)
                    return null;

                using (AesManaged aes = new AesManaged
                {
                    KeySize = KeyBitSize,
                    BlockSize = BlockBitSize,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                })
                {

                    byte[] iv = new byte[ivLength];
                    Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

                    using (ICryptoTransform decrypter = aes.CreateDecryptor(cryptKey, iv))
                    using (MemoryStream plainTextStream = new MemoryStream())
                    {
                        using (CryptoStream decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
                        using (BinaryWriter binaryWriter = new BinaryWriter(decrypterStream))
                        {
                            binaryWriter.Write(
                                encryptedMessage,
                                nonSecretPayloadLength + iv.Length,
                                encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
                            );
                        }
                        return plainTextStream.ToArray();
                    }
                }
            }
        }
    }
}