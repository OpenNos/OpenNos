using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;

namespace OpenNos.GameObject.Packets.ClientPackets
{
    [PacketHeader("psl")]
    public class PslPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int Type { get; set; }
    }
}
