using OpenNos.Core;
using OpenNos.Core.Networking.Communication.ScsServices.Service;
using OpenNos.Master.Interface;
using OpenNos.Master.Library.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.Master.Server
{
    internal class MSManager
    {
        #region Members
        private static MSManager _instance;
        #endregion

        #region Instantiation

        public MSManager()
        {
            WorldServers = new List<WorldServer>();
            LoginServers = new List<IScsServiceClient>();
            ConnectedAccounts = new List<AccountConnection>();
            AuthentificatedClients = new List<long>();
        }
        #endregion

        #region Properties

        public List<long> AuthentificatedClients { get; set; }

        public static MSManager Instance => _instance ?? (_instance = new MSManager());

        public List<WorldServer> WorldServers { get; set; }

        public List<IScsServiceClient> LoginServers { get; set; }

        public List<AccountConnection> ConnectedAccounts { get; set; }

        #endregion

        #region Methods



        #endregion
    }
}
