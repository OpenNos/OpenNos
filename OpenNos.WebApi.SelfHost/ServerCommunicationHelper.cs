using OpenNos.Core;
using OpenNos.Data;

namespace OpenNos.WebApi.SelfHost
{
    public class ServerCommunicationHelper
    {
        #region Members

        private static ServerCommunicationHelper _instance;

        private ThreadSafeSortedList<string, int> _connectedAccounts;

        private ThreadSafeSortedList<string, string> _connectedCharacters;

        private ThreadSafeSortedList<string, long> _registeredAccountLogins;

        private ThreadSafeSortedList<string, WorldserverGroupDTO> _worldservers;

        #endregion

        #region Properties

        public static ServerCommunicationHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServerCommunicationHelper();
                }

                return _instance;
            }
        }

        public ThreadSafeSortedList<string, int> ConnectedAccounts
        {
            get
            {
                if (_connectedAccounts == null)
                {
                    _connectedAccounts = new ThreadSafeSortedList<string, int>();
                }

                return _connectedAccounts;
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
                if (_connectedCharacters == null)
                {
                    _connectedCharacters = new ThreadSafeSortedList<string, string>();
                }

                return _connectedCharacters;
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
                if (_registeredAccountLogins == null)
                {
                    _registeredAccountLogins = new ThreadSafeSortedList<string, long>();
                }

                return _registeredAccountLogins;
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
                if (_worldservers == null)
                {
                    _worldservers = new ThreadSafeSortedList<string, WorldserverGroupDTO>();
                }

                return _worldservers;
            }
            set
            {
                _worldservers = value;
            }
        }

        #endregion
    }
}