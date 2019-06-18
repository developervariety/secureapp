using System;
using SecureApp.Licensing.Model.Interface;
using SecureAppUtil.Networking;

namespace SecureApp.Licensing.Core.Command
{
    public class Handshake : ICommand
    {
        public void Work(object[] payload, SecureSocket.Client socket)
        {
            throw new NotImplementedException();
        }
    }
}