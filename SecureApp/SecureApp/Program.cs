using System.Threading.Tasks;
using SecureApp.Core;

namespace SecureApp
{
    internal static class Program
    {
        public static async Task Main()
        {
            Global.Init();
            
            
            await Task.Delay(-1);
        }
    }
}