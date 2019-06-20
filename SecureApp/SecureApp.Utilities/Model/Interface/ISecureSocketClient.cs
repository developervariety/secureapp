using System;
using SecureApp.Utilities.Network;

namespace SecureApp.Utilities.Model.Interface {
    public interface ISecureSocketClient {
        void Send(params object[] data);
        void Send(IPacket packet);

        void Send(Action<SecureSocketConnectedClient> afterSend, params object[] data);
        void Send(Action<SecureSocketConnectedClient> afterSend, IPacket packet);
    }
}
