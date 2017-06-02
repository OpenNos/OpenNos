using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using System;
using System.Collections.Generic;

namespace OpenNos.Data
{
    public class WorldServerDTO
    {
        #region Instantiation

        public WorldServerDTO(Guid id, ScsTcpEndPoint endpoint, int accountLimit)
        {
            ConnectedAccounts = new Dictionary<string, long>();
            ConnectedCharacters = new Dictionary<string, long>();
            Id = id;
            Endpoint = endpoint;
            AccountLimit = accountLimit;
        }

        #endregion

        #region Properties

        public int AccountLimit { get; set; }

        public int ChannelId { get; set; }

        public Dictionary<string, long> ConnectedAccounts { get; set; }

        public Dictionary<string, long> ConnectedCharacters { get; set; }

        public ScsTcpEndPoint Endpoint { get; set; }

        public Guid Id { get; set; }

        #endregion
    }
}