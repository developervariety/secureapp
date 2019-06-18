using System.IO;
using System.Security.Cryptography;
using DeviceId;
using DeviceId.Encoders;
using DeviceId.Formatters;
using SecureAppUtil.Model;

namespace SecureAppUtil.Core
{
    public class Fingerprint
    {
        public string Generate()
        {
            if (!Directory.Exists(Location.SecureApp))
                Directory.CreateDirectory(Location.SecureApp);

            return new DeviceIdBuilder()
                .AddProcessorId()
                .AddMotherboardSerialNumber()
                .AddSystemDriveSerialNumber()
                .AddFileToken(Location.FileToken)
                .UseFormatter(new HashDeviceIdFormatter(SHA256.Create, new HexByteArrayEncoder()))
                .ToString();
        }
    }
}