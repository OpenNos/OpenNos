using System;

namespace OpenNos.Data
{
    public class GeneralLogDTO
    {
        #region Properties

        public long AccountId { get; set; }

        public Nullable<long> CharacterCharacterId { get; set; }

        public Nullable<long> CharacterId { get; set; }

        public string IpAddress { get; set; }

        public string LogData { get; set; }

        public long LogId { get; set; }

        public string LogType { get; set; }

        public System.DateTime Timestamp { get; set; }

        #endregion
    }
}