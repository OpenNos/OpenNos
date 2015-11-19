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

        public int SrcMap { get; set; }

        public int SrcX { get; set; }

        public int SrcY { get; set; }


        public int DestMap { get; set; }

        public int DestX { get; set; }

        public int DestY { get; set; }

    }
}
