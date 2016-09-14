using OpenNos.Core;

namespace OpenNos.Handler
{
    public class WalkPacket : PacketBase
    {
        #region Properties

        [Index(0)]
        public short XCoordinate { get; set; }

        [Index(1)]
        public short YCoordinate { get; set; }

        [Index(3)]
        public short Speed { get; set; }

        #endregion
    }
}