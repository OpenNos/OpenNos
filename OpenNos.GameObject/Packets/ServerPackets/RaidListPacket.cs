using OpenNos.Core;

namespace OpenNos.GameObject
{
    [PacketHeader("rl")]
    public class RaidListPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public short MonsterVNum { get; set; }

        #endregion
    }
}