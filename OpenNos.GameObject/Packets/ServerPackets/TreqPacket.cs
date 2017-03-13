using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;

namespace OpenNos.GameObject.Packets.ServerPackets
{
    public class TreqPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public int X { get; set; }

        [PacketIndex(1)]
        public int Y { get; set; }
        
        #endregion
    }
}
