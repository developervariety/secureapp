using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SecureApp.Utilities.Network.Server 
{
    public class ClientManager : IEnumerable<SecureSocketConnectedClient> 
    {
        private readonly Dictionary<Guid, SecureSocketConnectedClient> _clientList = new Dictionary<Guid, SecureSocketConnectedClient>();
        private readonly object _syncLock = new object();
        
        internal void Add(SecureSocketConnectedClient client) 
        {
            lock (_syncLock) {
                if (_clientList.ContainsKey(client.Id))
                    _clientList[client.Id] = client;
                else
                    _clientList.Add(client.Id, client);
            }
        }

        internal void Remove(SecureSocketConnectedClient client) 
        {
            lock (_syncLock) {
                if (_clientList.ContainsKey(client.Id))
                    _clientList.Remove(client.Id);
            }
        }

        public SecureSocketConnectedClient this[Guid id] 
        {
            get {
                lock (_syncLock)
                {
                    return _clientList.ContainsKey(id) ? _clientList[id] : null;
                }
            }
        }

        public IEnumerator<SecureSocketConnectedClient> GetEnumerator() 
        {
            lock (_syncLock) {
                return _clientList.Select(x => x.Value).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() 
        {
            lock (_syncLock) {
                return _clientList.Select(x => x.Value).GetEnumerator();
            }
        }
    }
}
