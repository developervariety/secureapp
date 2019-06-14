using SecureApp.Model.Networking;
using SecureAppUtil.Extensions.Networking;

namespace SecureApp.Model.Interface
{
    public interface ICommand
    {
        void Work(object[] payload, ClientSession clientSession, Socket.Server.SocketClient socketClient);
    }
}