using OpenNos.Core;
using OpenNos.Domain;
using System;

namespace OpenNos.Master.Library.Data
{
    [Serializable]
    public class SCSCharacterMessage
    {
        #region Instantiation

        public SCSCharacterMessage()
        {
        }

        #endregion

        #region Properties

        public long? DestinationCharacterId { get; set; }

        public string Message { get; set; }

        public long SourceCharacterId { get; set; }

        public Guid SourceWorldId { get; set; }

        public MessageType Type { get; set; }

        #endregion
    }
}