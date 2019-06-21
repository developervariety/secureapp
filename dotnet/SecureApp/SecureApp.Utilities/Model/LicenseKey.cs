using System;
using SecureApp.Utilities.Model.Enum.License;

namespace SecureApp.Utilities.Model
{
    public class LicenseKey
    {
        public string ApplicationId { get; set; }
        public Tier Tier { get; set; }
        public Edition Edition { get; set; }
        public DateTime Expiration { get; set; }
    }
}