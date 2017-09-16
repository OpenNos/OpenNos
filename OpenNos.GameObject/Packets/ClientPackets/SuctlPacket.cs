using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    [PacketHeader("suctl")]
    public class SuctlPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int CastId { get; set; }

        [PacketIndex(1)]
        public int Unknown2 { get; set; }

        [PacketIndex(2)]
        public int MateTransportId { get; set; }

        [PacketIndex(3)]
        public UserType TargetType { get; set; }

        [PacketIndex(4)]
        public int TargetId { get; set; }

        public override string ToString()
        {
            return $"{CastId} {Unknown2} {MateTransportId} {TargetType} {TargetId}";
        }
    }
}
