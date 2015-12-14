using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class GeneralLogDTO
    {
        public long LogId { get; set; }
        public long AccountId { get; set; }
        public string IpAddress { get; set; }
        public System.DateTime Timestamp { get; set; }
        public Nullable<long> CharacterId { get; set; }
        public string LogType { get; set; }
        public string LogData { get; set; }
        public Nullable<long> CharacterCharacterId { get; set; }
    }
}
