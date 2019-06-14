using System;
using System.IO;

namespace SecureAppUtil.Model
{
    public static class Location
    {

        public static string SecureApp =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SecureApp");
        public static string FileToken = Path.Combine(SecureApp, "Guid.id");
    }
}