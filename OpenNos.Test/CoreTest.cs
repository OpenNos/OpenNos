using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.GameObject;
using OpenNos.GameObject.Mock;
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
            // this test only tests the factory, not the packets
            // intialize factory
            PacketFactory.Initialize<WalkPacket>();

            Logger.InitializeLogger(LogManager.GetLogger(typeof(CoreTest)));

            // 1234 simulates packet header iterative number
            string equipPacket = "1234 equip 5 0 0.4903.5.0.0 2.340.0.0.0 3.720.0.0.0 5.4912.6.0.0 9.227.0.0.0 10.803.0.0.0 11.347.0.0.0 13.4146.0.0.0 14.4138.0.0.0";
            EquipPacket serializedPacket = PacketFactory.Serialize<EquipPacket>(equipPacket);
            string deserializedEquipPacket = PacketFactory.Deserialize(serializedPacket);
            Assert.AreEqual(equipPacket, $"1234 {deserializedEquipPacket}");

            // 1234 simulates packet header iterative number
            string inPacket = "1234 in 1 ImACharacter - 1 80 116 0 2 1 0 3 0 -1.12.1.8.-1.-1.-1.-1.-1 100 100 0 -1 0 0 0 0 0 0 0 0 -1 - 1 0 0 0 0 1 0 0 0 10 0";
            InPacket serializedInPacket = PacketFactory.Serialize<InPacket>(inPacket);
            string deserializedInPacket = PacketFactory.Deserialize(serializedInPacket);
            Assert.AreEqual(inPacket, $"1234 {deserializedInPacket}");

            WalkPacket walkPacket = PacketFactory.Serialize<WalkPacket>("123 walk 3a0 115 1 11");
            Assert.IsNull(walkPacket);
        }

        [TestMethod]
        public void SessionManagerTest()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            // initialize Logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(CoreTest)));

            // initialilize maps
            ServerManager.Initialize();

            // register mappings for items
            DAOFactory.InventoryDAO.RegisterMapping(typeof(SpecialistInstance));
            DAOFactory.InventoryDAO.RegisterMapping(typeof(WearableInstance));
            DAOFactory.InventoryDAO.RegisterMapping(typeof(UsableInstance));
            DAOFactory.InventoryDAO.InitializeMapper(typeof(ItemInstance));

            // initialize PacketSerialization
            PacketFactory.Initialize<WalkPacket>();

            // initialize new manager
            SessionManager manager = new NetworkManager<TestEncryption>("127.0.0.1", 1234, typeof(CharacterScreenPacketHandler), typeof(TestEncryption), true);
            FakeNetworkClient client = new FakeNetworkClient();
            manager.AddSession(client);
            client.ReceivePacket("test 12345");

        }

        #endregion
    }
}