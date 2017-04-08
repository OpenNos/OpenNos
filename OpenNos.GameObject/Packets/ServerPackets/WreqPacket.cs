using OpenNos.Core;

namespace OpenNos.GameObject.Packets.ServerPackets
{
    [PacketHeader("wreq")]
    public class WreqPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(1)]
        public long? Param { get; set; }

        [PacketIndex(0)]
        public byte Value { get; set; }

        #endregion
    }
}