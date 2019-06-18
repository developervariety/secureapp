using System;
using SecureAppUtil.Networking;

namespace SecureApp.Licensing
{
    public class Base
    {
        private static readonly SecureSocket.Client Client = new SecureSocket.Client();

        public Base()
        {
            Console.WriteLine(Client.Connect("localhost", 0709)
                ? "Connected to the server."
                : "Failed to connect to server.");

            Client.Send("LOL");
        }
    }
}