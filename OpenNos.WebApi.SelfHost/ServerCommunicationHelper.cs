using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.WebApi.SelfHost
{
    public class ServerCommunicationHelper
    {
        #region Members

        private static ServerCommunicationHelper _instance;

        private Dictionary<string, long> _registeredAccountLogins;

        private List<WorldserverGroupDTO> _worldserverGroups;

        private List<WorldserverDTO> _worldservers;

        #endregion

        #region Properties

        public static ServerCommunicationHelper Instance => _instance ?? (_instance = new ServerCommunicationHelper());

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

        public List<WorldserverGroupDTO> WorldserverGroups
        {
            get
            {
                return _worldserverGroups ?? (_worldserverGroups = new List<WorldserverGroupDTO>());
            }
            set
            {
                _worldserverGroups = value;
            }
        }

        public List<WorldserverDTO> Worldservers
        {
            get
            {
                return _worldservers ?? (_worldservers = new List<WorldserverDTO>());
            }
            set
            {
                _worldservers = value;
            }
        }

        #endregion

        #region Methods

        public WorldserverDTO GetWorldserverById(Guid id)
        {
            return Worldservers.SingleOrDefault(w => w.Id == id);
        }

        #endregion
    }
}