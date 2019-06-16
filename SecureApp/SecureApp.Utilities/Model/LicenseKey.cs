using System;
using SecureAppUtil.Model.Enum.License;

namespace SecureAppUtil.Model
{
    public class LicenseKey
    {
        public string ApplicationId { get; set; }
        public Tier Tier { get; set; }
        public Edition Edition { get; set; }
        public DateTime Expiration { get; set; }
    }
}