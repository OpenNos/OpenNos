using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class PortalDTO
    {
        public int PortalId { get; set; }

        public short SrcMap { get; set; }

        public short SrcX { get; set; }

        public short SrcY { get; set; }

        public short DestMap { get; set; }

        public short DestX { get; set; }

        public short DestY { get; set; }

        public short Type { get; set; }

    }
}
