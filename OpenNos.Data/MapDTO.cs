using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class MapDTO
    {
        public short MapId { get; set; }

        public string Name { get; set; }

        public byte[] Data { get; set; }

        public int Music { get; set; }
    }
}
