using log4net;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Mock;
using OpenNos.Handler;
using OpenNos.ServiceRef.Internal;
using System;
using System.Collections.Generic;

namespace OpenNos.Test
{
    public static class HandlerTestHelper
    {
        #region Methods

        private static SessionManager _sessionManager;

        public static FakeNetworkClient InitializeTestEnvironment()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            // initialize Logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(BasicPacketHandlerTest)));

            // create server entities (this values would have been imported)
            CreateServerItems();
            CreateServerMaps();
            CreateServerSkills();

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
            _sessionManager = new NetworkManager<TestEncryption>("127.0.0.1", 1234, typeof(CharacterScreenPacketHandler), typeof(TestEncryption), true);
            FakeNetworkClient client = new FakeNetworkClient();
            _sessionManager.AddSession(client);

            AccountDTO account = new AccountDTO()
            {
                AccountId = 1,
                Authority = AuthorityType.Admin,
                LastSession = 12345,
                Name = "test",
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

            string clistEnd = WaitForPacket(client);

            // creation of character
            client.ReceivePacket("Char_NEW Test 2 1 0 9");

            List<string> clistAfterCreate = WaitForPackets(client, 3);
            CListPacket cListPacket = PacketFactory.Serialize<CListPacket>(clistAfterCreate[1]);

            // select character
            client.ReceivePacket($"select {cListPacket.Slot}");
            string okPacket = WaitForPacket(client);

            // start game
            client.ReceivePacket("game_start");
            List<string> gameStartPacketsFirstPart = WaitForPackets(client, "p_clear");
            List<string> gameStartPacketsSecondPart = WaitForPackets(client, "p_clear");

            return client;
        }

        public static void ShutdownTestingEnvironment()
        {
            _sessionManager.StopServer();
        }

        public static string WaitForPacket(FakeNetworkClient client)
        {
            while (true)
            {
                if (client.SentPackets.Count > 0)
                {
                    return client.SentPackets.Dequeue();
                }
            }
        }

        public static string WaitForPacket(FakeNetworkClient client, string packetHeader)
        {
            while (true)
            {
                if (client.SentPackets.Count > 0)
                {
                    string packet = client.SentPackets.Dequeue();
                    if (packet != null && packet.StartsWith(packetHeader))
                    {
                        return packet;
                    }
                }
            }
        }

        public static List<string> WaitForPackets(FakeNetworkClient client, int amount)
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

        public static List<string> WaitForPackets(FakeNetworkClient client, string lastPacketHeader, int timeout = 5)
        {
            DateTime startTime = DateTime.Now;
            List<string> packets = new List<string>();

            while (true)
            {
                if (client.SentPackets.Count > 0)
                {
                    string packet = client.SentPackets.Dequeue();
                    if (packet != null)
                    {
                        packets.Add(packet);
                        if (packet.StartsWith(lastPacketHeader))
                        {
                            return packets;
                        }
                    }
                }

                if (startTime.AddSeconds(timeout) <= DateTime.Now)
                {
                    return packets; // timing out
                }
            }
        }

        private static void CreateServerItems()
        {
            DAOFactory.ItemDAO.Insert(new ItemDTO()
            {
                VNum = 1,
                Class = 1,
                CriticalLuckRate = 4,
                CriticalRate = 70,
                DamageMaximum = 28,
                DamageMinimum = 20,
                HitRate = 20,
                IsDroppable = true,
                IsSoldable = true,
                IsTradable = true,
                LevelMinimum = 1,
                MaximumAmmo = 100,
                Name = "Wooden Stick",
                Price = 70
            });
            DAOFactory.ItemDAO.Insert(new ItemDTO()
            {
                VNum = 8,
                EquipmentSlot = 5
            });
            DAOFactory.ItemDAO.Insert(new ItemDTO()
            {
                VNum = 12,
                EquipmentSlot = 1
            });
        }

        private static void CreateServerMaps()
        {
            MapDTO testingMap = new MapDTO()
            {
                MapId = 1,
                Music = 1,
                Name = "Testing-Map",
                ShopAllowed = true
            };
            List<byte> mapData = new List<byte>();

            // create map grid
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    // we can go everywhere
                    mapData.Add(1);
                }
            }
            testingMap.Data = mapData.ToArray();
            DAOFactory.MapDAO.Insert(testingMap);
        }

        private static void CreateServerSkills()
        {
            DAOFactory.SkillDAO.Insert(new SkillDTO()
            {
                SkillVNum = 200
            });
            DAOFactory.SkillDAO.Insert(new SkillDTO()
            {
                SkillVNum = 201
            });
            DAOFactory.SkillDAO.Insert(new SkillDTO()
            {
                SkillVNum = 209
            });
        }

        #endregion
    }
}