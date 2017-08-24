using System;

namespace OpenNos.Master.Library.Data
{
    [Serializable]
    public class SerializableWorldServer
    {
        #region Instantiation

        public SerializableWorldServer(Guid id, string epIp, short epPort, int accountLimit, string worldGroup)
        {
            Id = id;
            EndPointIp = epIp;
            EndPointPort = epPort;
            AccountLimit = accountLimit;
            WorldGroup = worldGroup;
        }

        #endregion

        #region Properties

        public int AccountLimit { get; set; }

        public int ChannelId { get; set; }

        public string EndPointIp { get; set; }

        public short EndPointPort { get; set; }

        public Guid Id { get; set; }

        public string WorldGroup { get; set; }

        #endregion
    }
}