using OpenNos.Core;
using OpenNos.Data;

namespace OpenNos.WebApi.SelfHost
{
    public class ServerCommunicationHelper
    {
        #region Members

        private static ServerCommunicationHelper _instance;

        private ThreadSafeSortedList<string, long> _connectedAccounts;

        private ThreadSafeSortedList<string, string> _connectedCharacters;

        private ThreadSafeSortedList<string, long> _registeredAccountLogins;

        private ThreadSafeSortedList<string, WorldserverGroupDTO> _worldservers;

        #endregion

        #region Properties

        public static ServerCommunicationHelper Instance
        {
            get
            {
                return _instance ?? (_instance = new ServerCommunicationHelper());
            }
        }

        public ThreadSafeSortedList<string, long> ConnectedAccounts
        {
            get
            {
                return _connectedAccounts ?? (_connectedAccounts = new ThreadSafeSortedList<string, long>());
            }
            set
            {
                _connectedAccounts = value;
            }
        }

        public ThreadSafeSortedList<string, string> ConnectedCharacters
        {
            get
            {
                return _connectedCharacters ?? (_connectedCharacters = new ThreadSafeSortedList<string, string>());
            }
            set
            {
                _connectedCharacters = value;
            }
        }

        public ThreadSafeSortedList<string, long> RegisteredAccountLogins
        {
            get
            {
                return _registeredAccountLogins ?? (_registeredAccountLogins = new ThreadSafeSortedList<string, long>());
            }
            set
            {
                _registeredAccountLogins = value;
            }
        }

        public ThreadSafeSortedList<string, WorldserverGroupDTO> Worldservers
        {
            get
            {
                return _worldservers ?? (_worldservers = new ThreadSafeSortedList<string, WorldserverGroupDTO>());
            }
            set
            {
                _worldservers = value;
            }
        }

        #endregion
    }
}