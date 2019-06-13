using System;
using System.Collections.Generic;
using SecureApp.Networking.Model;

namespace SecureApp.Extensions
{
    public static class Random
    {
        public static Guid UniqueId(IReadOnlyDictionary<Guid, ClientData> connectedClients)
        {
            Guid id = Guid.NewGuid();
            
            while(connectedClients.ContainsKey(id))
                id = Guid.NewGuid();
            
            return id;
        }
    }
}