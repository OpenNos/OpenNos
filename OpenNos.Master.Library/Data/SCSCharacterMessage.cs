using OpenNos.Domain;
using System;

namespace OpenNos.Master.Library.Data
{
    [Serializable]
    public class SCSCharacterMessage
    {
        public SCSCharacterMessage()
        {

        }

        #region Properties

        public long SourceCharacterId { get; set; }

        public Guid SourceWorldId { get; set; }

        public long? DestinationCharacterId { get; set; }

        public string Message { get; set; }

        public MessageType Type { get; set; }

        #endregion
    }
}
