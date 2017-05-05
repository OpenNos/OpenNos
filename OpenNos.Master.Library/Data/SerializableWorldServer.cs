using System;

namespace OpenNos.Master.Library.Data
{
    [Serializable]
    public class SerializableWorldServer
    {
        #region Instantiation

        public SerializableWorldServer(Guid id, string epIP, int epPort, int accountLimit, string worldGroup)
        {
            Id = id;
            EndPointIP = epIP;
            EndPointPort = epPort;
            AccountLimit = accountLimit;
            WorldGroup = worldGroup;
        }

        #endregion

        #region Properties

        public int AccountLimit { get; set; }

        public int ChannelId { get; set; }

        public string EndPointIP { get; set; }

        public int EndPointPort { get; set; }

        public Guid Id { get; set; }

        public string WorldGroup { get; set; }

        #endregion
    }
}