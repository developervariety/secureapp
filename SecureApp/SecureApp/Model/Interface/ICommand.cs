using SecureApp.Model.Networking;
using SecureAppUtil.Networking;

namespace SecureApp.Model.Interface
{
    public interface ICommand
    {
        void Work(object[] payload, ClientSession clientSession, SecureSocket.Server.SocketClient socket);
    }
}