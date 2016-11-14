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

            // mtlist packet
            string mtlistPacket = "mtlist 30 3 1698 3 1703 3 1861 3 1865 3 1870 3 1873 3 1874 3 1879 3 1880 3 1882 3 1885 3 1887 3 1890 3 1896 3 1900 3 1901 3 1904 3 1908 3 1915 3 1916 3 1919 3 1923 3 1925 3 1931 3 1934 3 1935 3 1937 3 1941 3 1942 3 1946";
            MultiTargetListPacket deserializedMtlistPacket = PacketFactory.Deserialize<MultiTargetListPacket>(mtlistPacket);
            string serializedMtlistPacket = PacketFactory.Serialize(deserializedMtlistPacket);
            Assert.AreEqual(mtlistPacket, serializedMtlistPacket);

            // Equip Packet
            string equipPacket = "equip 5 0 0.4903.5.0.0 2.340.0.0.0 3.720.0.0.0 5.4912.6.0.0 9.227.0.0.0 10.803.0.0.0 11.347.0.0.0 13.4146.0.0.0 14.4138.0.0.0";
            EquipPacket deserializedEquipPacket = PacketFactory.Deserialize<EquipPacket>(equipPacket);
            string serializedEquipPacket = PacketFactory.Serialize(deserializedEquipPacket);
            Assert.AreEqual(equipPacket, serializedEquipPacket);

            // In Packet
            string inPacket = "in 1 ImACharacter - 1 80 116 0 2 1 0 3 0 -1.12.1.8.-1.-1.-1.-1.-1 100 100 0 -1 0 0 0 0 0 0 0 0 -1 - 1 0 0 0 0 1 0 0 0 10 0";
            InPacket deserializedInPacket = PacketFactory.Deserialize<InPacket>(inPacket);
            string serializedInPacket = PacketFactory.Serialize(deserializedInPacket);
            Assert.AreEqual(inPacket, serializedInPacket);

            // Walk Packet
            string walkPacket = "walk 3 115 1 11";
            WalkPacket deserializedWalkPacket = PacketFactory.Deserialize<WalkPacket>(walkPacket);
            string serializedWalkPacket = PacketFactory.Serialize(deserializedWalkPacket);
            Assert.AreEqual(walkPacket, serializedWalkPacket);

            WalkPacket invalidWalkPacket = PacketFactory.Deserialize<WalkPacket>("walk 3a0 115 1 11");
            Assert.IsNull(invalidWalkPacket);

            // Dialog Packet
            string dialogPacket = "dlg #walk^3^115^1^11 #walk^3^115^1^11 Do you really wanna walk this way?";
            DialogPacket<WalkPacket, WalkPacket> deserializedDialogPacket = PacketFactory.Deserialize<DialogPacket<WalkPacket, WalkPacket>>(dialogPacket);
            string serializedDialogPacket = PacketFactory.Serialize(deserializedDialogPacket);
            Assert.AreEqual(dialogPacket, serializedDialogPacket);
        }

        #endregion
    }
}