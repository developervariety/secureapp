using System;
using System.Collections.Generic;
using SecureApp.Utilities.Model.Interface;
using SecureApp.Utilities.Network;

namespace SecureApp
{
    public class Settings
    {
        public static List<IPlugin> Plugins { get; set; }
        
        public static List<Tuple<Guid, SecureSocketConnectedClient>> Clients = new List<Tuple<Guid, SecureSocketConnectedClient>>();
    }
}