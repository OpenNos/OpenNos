using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class NpcDTO
    {

        public short NpcId { get; set; }
        public string Name { get; set; }
        public short Vnum { get; set; }
        public short Dialog { get; set; }
        public short MapId { get; set; }
        public short MapX { get; set; }
        public short MapY { get; set; }
        public short Position { get; set; }
        public short Level { get; set; }
        public short MenuType { get; set; }

    }
}
