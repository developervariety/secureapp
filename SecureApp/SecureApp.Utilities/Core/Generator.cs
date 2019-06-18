using System;
using System.IO;
using SecureAppUtil.Crypt;
using SecureAppUtil.Extensions;
using SecureAppUtil.Model;
using SecureAppUtil.Model.Enum.License;
using Random = SecureAppUtil.Extensions.Random;

namespace SecureAppUtil.Core
{
    public class Generator
    {
        private static readonly Aes Aes = new Aes();

        public Generator(string applicationSecret)
        {
            ApplicationSecret = applicationSecret;
        }

        private string ApplicationSecret { get; }

        public string Generate(LicenseKey data)
        {
            byte[] appIdBytes = BitConverter.GetBytes(data.ApplicationId.Length);
            byte[] tierBytes = BitConverter.GetBytes((byte) data.Tier);
            byte[] editionBytes = BitConverter.GetBytes((byte) data.Edition);
            byte[] expirationBytes = BitConverter.GetBytes(Convert.ToUInt32(
                data.Expiration.Day.ToString().PadLeft(2, '0') + data.Expiration.Month.ToString().PadLeft(2, '0') +
                data.Expiration.Year));
            byte[] rndBytes = BitConverter.GetBytes(Random.Int());

            byte[] memArray;
            using (MemoryStream memStream = new MemoryStream())
            {
                memStream.Write(appIdBytes, 0, 1);
                memStream.Write(tierBytes, 0, 1);
                memStream.Write(editionBytes, 0, 1);
                memStream.Write(expirationBytes, 0, 4);
                memStream.Write(rndBytes, 0, 1);
                memArray = memStream.ToArray();
            }

            byte[] aesArray = Aes.Encrypt(memArray, ApplicationSecret);
            string text = Base32.Encode(aesArray);

            return
                $"{text.Substring(0, 5)}-{text.Substring(5, 5)}-{text.Substring(10, 6)}-{text.Substring(16, 5)}-{text.Substring(21, 5)}"
                    .ToUpperInvariant();
        }

        public bool Validate(string licenseKey, ref LicenseKey data)
        {
            try
            {
                if (string.IsNullOrEmpty(licenseKey))
                    throw new ArgumentNullException(licenseKey, "License Key is null or empty.");

                if (licenseKey.Length != 30)
                    throw new ArgumentException("License Key is invalid.", licenseKey);

                licenseKey = licenseKey.ToLowerInvariant().Replace("-", string.Empty);
                byte[] keyBytes = Base32.Decode(licenseKey);

                byte[] aesBytes = Aes.Decrypt(keyBytes, ApplicationSecret);

                byte[] appIdBytes = new byte[2];
                byte[] tierBytes = new byte[2];
                byte[] editionBytes = new byte[2];
                byte[] expirationBytes = new byte[4];

                using (MemoryStream memStream = new MemoryStream(aesBytes))
                {
                    memStream.Read(appIdBytes, 0, 1);
                    memStream.Read(tierBytes, 0, 1);
                    memStream.Read(editionBytes, 0, 1);
                    memStream.Read(expirationBytes, 0, 4);
                }

                LicenseKey licenseKeyData = new LicenseKey
                {
                    ApplicationId = BitConverter.ToString(appIdBytes, 0),
                    Tier = (Tier) BitConverter.ToInt16(tierBytes, 0),
                    Edition = (Edition) BitConverter.ToInt16(editionBytes, 0)
                };

                string text = BitConverter.ToUInt32(expirationBytes, 0).ToString().PadLeft(8, '0');
                licenseKeyData.Expiration = new DateTime(Convert.ToInt16(text.Substring(4, 4)),
                    Convert.ToInt16(text.Substring(2, 2)), Convert.ToInt16(text.Substring(0, 2)));

                data = licenseKeyData;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}