using OpenNos.Core;

namespace OpenNos.GameObject
{
    [PacketHeader("$Effect")]
    public class EffectCommandPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public int EffectId { get; set; }

        #endregion
    }
}