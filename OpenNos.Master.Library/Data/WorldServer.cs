using Hik.Communication.ScsServices.Service;
using System;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;

namespace OpenNos.Master.Library.Data
{
    public class WorldServer
    {
        #region Instantiation

        public WorldServer(Guid id, ScsTcpEndPoint endpoint, int accountLimit, string worldGroup)
        {
            Id = id;
            Endpoint = endpoint;
            AccountLimit = accountLimit;
            WorldGroup = worldGroup;
        }

        #endregion

        #region Properties

        public int AccountLimit { get; set; }

        public int ChannelId { get; set; }

        public ScsTcpEndPoint Endpoint { get; set; }

        public IScsServiceClient ServiceClient { get; set; }

        public Guid Id { get; set; }

        public string WorldGroup { get; set; }

        public SerializableWorldServer Serializable { get; private set; }

        #endregion
    }
}