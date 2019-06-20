using System;
using System.Net;
using System.Net.Sockets;

namespace SecureApp.Utilities.Network 
{
    public delegate void OnSecureSocketClose(SecureSocket sender, Exception e);
    public abstract class SecureSocket : IDisposable 
    {
        public event OnSecureSocketClose OnClose;

        public virtual IPEndPoint EndPoint { get; protected set; }
        public bool HasUdp { get; internal set; }

        protected Exception LastError = null;

        private readonly byte[] _socketOptions;

        public SecureSocket() 
        {
            const int keepAliveTime = 30000;
            const int keepAliveInterval = 30000;

            _socketOptions = new byte[sizeof(uint) * 3];
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(_socketOptions, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(_socketOptions, sizeof(uint));
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(_socketOptions, sizeof(uint) * 2);
        }

        protected virtual void Close() 
        {
        }

        public void Dispose() 
        {
            OnClose?.Invoke(this, LastError);
            Close();
        }

        protected void SetTcpKeepAlive(Socket socket) 
        {
            socket?.IOControl(IOControlCode.KeepAliveValues, _socketOptions, null);
        }

        public override string ToString() 
        {
            return EndPoint.ToString();
        }
    }
}
