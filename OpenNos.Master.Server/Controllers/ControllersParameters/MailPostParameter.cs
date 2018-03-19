using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Master.Server.Controllers.ControllersParam
{

    public class MailPostParameter
    {
        public long CharacterId { get; set; }
        public string WorldGroup { get; set; }
        public string Title { get; set; }
        public short VNum { get; set; }
        public byte Amount { get; set; }
        public sbyte Rare { get; set; }
        public byte Upgrade { get; set; }
        public bool IsNosmall { get; set; }
    }
}
