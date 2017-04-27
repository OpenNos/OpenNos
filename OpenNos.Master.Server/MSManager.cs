using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Server;
using OpenNos.Master.Interface;
using OpenNos.Master.Library;
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
            ConnectedAccounts = new List<AccountConnection>();

        }
        #endregion

        #region Properties

        public static MSManager Instance => _instance ?? (_instance = new MSManager());

        public List<WorldServer> WorldServers { get; set; }

        public List<AccountConnection> ConnectedAccounts { get; set; }


        #endregion

        #region Methods



        #endregion
    }
}
