using SecureAppUtil.Networking;

namespace SecureApp.Licensing.Model.Interface
{
    public interface ICommand
    {
        void Work(object[] payload, SecureSocket.Client socket);
    }
}