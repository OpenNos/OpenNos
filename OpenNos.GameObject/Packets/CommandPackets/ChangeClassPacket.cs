using OpenNos.Core;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    [PacketHeader("$ChangeClass")]
    public class ChangeClassPacket : PacketBase
    {
        #region Properties

        [PacketIndex(0)]
        public ClassType ClassType { get; set; }

        #endregion
    }
}