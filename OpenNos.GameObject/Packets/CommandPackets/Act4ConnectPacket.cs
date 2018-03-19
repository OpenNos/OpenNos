using OpenNos.Core;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    [PacketHeader("$Act4Connect", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class Act4ConnectPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string Name { get; set; }

        public override string ToString()
        {
            return "Act4Connect Name";
        }

        #endregion
    }
}
