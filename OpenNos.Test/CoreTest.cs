using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNos.Core;
using OpenNos.GameObject;

namespace OpenNos.Test
{
    [TestClass]
    public class CoreTest
    {
        #region Methods

        [TestMethod]
        public void PacketFactoryTest()
        {
            // this test only tests the factory, not the packets intialize factory
            PacketFactory.Initialize<WalkPacket>();

            Logger.InitializeLogger(LogManager.GetLogger(typeof(CoreTest)));

            string equipPacket = "equip 5 0 0.4903.5.0.0 2.340.0.0.0 3.720.0.0.0 5.4912.6.0.0 9.227.0.0.0 10.803.0.0.0 11.347.0.0.0 13.4146.0.0.0 14.4138.0.0.0";
            EquipPacket deserializedEquipPacket = PacketFactory.Deserialize<EquipPacket>(equipPacket);
            string serializedEquipPacket = PacketFactory.Serialize(deserializedEquipPacket);
            Assert.AreEqual(equipPacket, serializedEquipPacket);

            string inPacket = "in 1 ImACharacter - 1 80 116 0 2 1 0 3 0 -1.12.1.8.-1.-1.-1.-1.-1 100 100 0 -1 0 0 0 0 0 0 0 0 -1 - 1 0 0 0 0 1 0 0 0 10 0";
            InPacket deserializedInPacket = PacketFactory.Deserialize<InPacket>(inPacket);
            string serializedInPacket = PacketFactory.Serialize(deserializedInPacket);
            Assert.AreEqual(inPacket, serializedInPacket);

            WalkPacket invalidWalkPacket = PacketFactory.Deserialize<WalkPacket>("walk 3a0 115 1 11");
            Assert.IsNull(invalidWalkPacket);

            //Dialog Packet
            WalkPacket validWalkPacket = PacketFactory.Deserialize<WalkPacket>("walk 3 115 1 11");
            string dialogPacket = "dlg #walk^3^115^1^11 #walk^3^115^1^11 Do you really wanna walk this way?";
            DialogPacket<WalkPacket, WalkPacket> deserializedDialogPacket = PacketFactory.Deserialize<DialogPacket<WalkPacket, WalkPacket>>(dialogPacket);
            string serializedDialogPacket = PacketFactory.Serialize(deserializedDialogPacket);
            Assert.AreEqual(dialogPacket, serializedDialogPacket);

        }

        #endregion
    }
}