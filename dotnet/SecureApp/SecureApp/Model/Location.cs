using System;
using System.IO;

namespace SecureApp.Model
{
    public class Location
    {
        public static string Plugins = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"));
    }
}