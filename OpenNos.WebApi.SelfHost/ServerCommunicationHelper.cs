using OpenNos.Core;
using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.WebApi.SelfHost
{
    public class ServerCommunicationHelper
    {
        #region Members

        private static ServerCommunicationHelper _instance;

        private Dictionary<string, long> _connectedAccounts;

        private Dictionary<string, string> _connectedCharacters;

        private Dictionary<string, long> _registeredAccountLogins;

        private Dictionary<string, WorldserverGroupDTO> _worldservers;

        #endregion

        #region Properties

        public static ServerCommunicationHelper Instance
        {
            get
            {
                return _instance ?? (_instance = new ServerCommunicationHelper());
            }
        }

        public Dictionary<string, long> ConnectedAccounts
        {
            get
            {
                return _connectedAccounts ?? (_connectedAccounts = new Dictionary<string, long>());
            }
            set
            {
                _connectedAccounts = value;
            }
        }

        public Dictionary<string, string> ConnectedCharacters
        {
            get
            {
                return _connectedCharacters ?? (_connectedCharacters = new Dictionary<string, string>());
            }
            set
            {
                _connectedCharacters = value;
            }
        }

        public Dictionary<string, long> RegisteredAccountLogins
        {
            get
            {
                return _registeredAccountLogins ?? (_registeredAccountLogins = new Dictionary<string, long>());
            }
            set
            {
                _registeredAccountLogins = value;
            }
        }

        public Dictionary<string, WorldserverGroupDTO> Worldservers
        {
            get
            {
                return _worldservers ?? (_worldservers = new Dictionary<string, WorldserverGroupDTO>());
            }
            set
            {
                _worldservers = value;
            }
        }

        #endregion
    }
}