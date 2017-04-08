using OpenNos.Core;

namespace OpenNos.GameObject.Packets.ServerPackets
{
    [PacketHeader("treq")]
    public class TreqPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(3)]
        public byte? RecordPress { get; set; }

        [PacketIndex(2)]
        public byte? StartPress { get; set; }

        [PacketIndex(0)]
        public int X { get; set; }

        [PacketIndex(1)]
        public int Y { get; set; }

        #endregion
    }
}