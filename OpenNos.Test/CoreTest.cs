using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNos.Core;
using OpenNos.Handler;

namespace OpenNos.Test
{
    [TestClass]
    public class CoreTest
    {
        #region Methods

        [TestMethod]
        public void PacketFactoryTest()
        {
            //this test only tests the factory, not the packets

            //intialize factory
            PacketFactory.Initialize<WalkPacket>();

            //1234 simulates pulse
            string equipPacket = "1234 equip 5 0 0.4903.5.0.0 2.340.0.0.0 3.720.0.0.0 5.4912.6.0.0 9.227.0.0.0 10.803.0.0.0 11.347.0.0.0 13.4146.0.0.0 14.4138.0.0.0";
            EquipPacket serializedPacket = PacketFactory.Serialize<EquipPacket>(equipPacket);

            string inPacket = "1234 in 1 ChuckTheRipper - 1 80 116 0 2 1 0 3 0 -1.12.1.8.-1.-1.-1.-1.-1 100 100 0 -1 0 0 0 0 0 0 00 00 -1 - 1 0 0 0 0 1 0 0 0 10 0";
            InPacket serializedInPacket = PacketFactory.Serialize<InPacket>(inPacket);
            string deserializedInPacket = PacketFactory.Deserialize(serializedInPacket);

        }

        #endregion
    }
}