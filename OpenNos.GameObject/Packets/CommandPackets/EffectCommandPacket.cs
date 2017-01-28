using OpenNos.Core;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    [PacketHeader("$Effect", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class EffectCommandPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public int EffectId { get; set; }

        #endregion
    }
}