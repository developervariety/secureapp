using System;
using System.IO;

namespace SecureAppUtil.Model
{
    public static class Location
    {
        public static readonly string SecureApp =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SecureApp");

        public static readonly string FileToken = Path.Combine(SecureApp, "Guid.id");
    }
}