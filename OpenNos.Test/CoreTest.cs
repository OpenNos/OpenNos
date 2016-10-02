using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Mock;
using OpenNos.Handler;
using OpenNos.ServiceRef.Internal;
using System.Collections.Generic;
using System.Diagnostics;

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

            // create server items
            CreateServerItems();

            // initialize servermanager
            ServerManager.Initialize();

            // initialize WCF
            ServiceFactory.Instance.Initialize();

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

            AccountDTO account = new AccountDTO()
            {
                AccountId = 1, Authority = AuthorityType.Admin, LastSession = 12345, Name = "test",
                Password = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff"
            };
            DAOFactory.AccountDAO.InsertOrUpdate(ref account);

            // register for account login
            ServiceFactory.Instance.CommunicationService.RegisterAccountLogin("test", 12345);

            // OpenNosEntryPoint -> LoadCharacterList
            client.ReceivePacket("12345");
            client.ReceivePacket("test");
            client.ReceivePacket("test");

            string clistStart = WaitForPacket(client);
            Assert.AreEqual(clistStart, "clist_start 0");

            string clistEnd = WaitForPacket(client);
            Assert.AreEqual(clistEnd, "clist_end");

            client.ReceivePacket("Char_NEW Test 2 1 0 9");

            List<string> clistAfterCreate = WaitForPackets(client, 3);
            ClistPacket cListPacket = PacketFactory.Serialize<ClistPacket>(clistAfterCreate[1]);
        }

        public string WaitForPacket(FakeNetworkClient client)
        {
            while (true)
            {
                if (client.SentPackets.Count > 0)
                {
                    return client.SentPackets.Dequeue();
                }
            }
        }

        public List<string> WaitForPackets(FakeNetworkClient client, int amount)
        {
            int receivedPackets = 0;
            List<string> packets = new List<string>();
            while (receivedPackets < amount)
            {
                if (client.SentPackets.Count > 0)
                {
                    packets.Add(client.SentPackets.Dequeue());
                    receivedPackets++;
                }
            }

            return packets;
        }

        private void CreateServerItems()
        {
            ItemDTO weaponRight = new ItemDTO()
            {
                VNum = 1, Class = 1, CriticalLuckRate = 4, CriticalRate = 70, DamageMaximum = 28, DamageMinimum = 20, HitRate = 20, IsDroppable = true, IsSoldable = true, IsTradable = true, LevelMinimum = 1, MaximumAmmo = 100, Name = "Wooden Stick", Price = 70
            };
            DAOFactory.ItemDAO.Insert(weaponRight);
            ItemDTO weaponLeft = new ItemDTO()
            {
                VNum = 8,
                EquipmentSlot = 5
            };
            DAOFactory.ItemDAO.Insert(weaponLeft);
            ItemDTO armor = new ItemDTO()
            {
                VNum = 12,
                EquipmentSlot = 1
            };
            DAOFactory.ItemDAO.Insert(armor);
        }

        #endregion
    }
}