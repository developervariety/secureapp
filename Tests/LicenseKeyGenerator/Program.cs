using System;
using SecureAppUtil;
using SecureAppUtil.Extensions;
using SecureAppUtil.Model;
using SecureAppUtil.Model.Enum.License;

namespace LicenseKeyGenerator
{
    internal static class Program
    {
        private static readonly Generator Generator = new Generator("secureapp123!stongpassword");
        
        public static void Main()
        {
            LicenseKey data = new LicenseKey
            {
                Tier = Tier.Bronze,
                Edition = Edition.Professional,
                Expiration = DateTime.Now.AddDays(30)
            };

            string licenseKey = Generator.Generate(data);
            Console.WriteLine(licenseKey);
            
            LicenseKey license = new LicenseKey();
            string status = Generator.Validate(licenseKey, ref license) ? "valid" : "not valid";
                        
            Console.Write($"This license key is {status}.");
            Console.WriteLine(Environment.NewLine);
            
            Console.WriteLine($"Tier: {data.Tier}");
            Console.WriteLine($"Edition: {data.Edition}");
            Console.WriteLine($"Expiration date: {data.Expiration:g}");
        }
    }
}