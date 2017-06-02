using log4net;
using NUnit.Framework;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Mock;
using OpenNos.Handler;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace OpenNos.Test
{
    public static class HandlerTestHelper
    {
        #region Members

        private static SessionManager _sessionManager;

        #endregion

        #region Methods

        public static FakeNetworkClient CreateFakeNetworkClient()
        {
            FakeNetworkClient client = new FakeNetworkClient();
            _sessionManager.AddSession(client);

            long id = ServerManager.Instance.RandomNumber(0, 999999);
            AccountDTO account = new AccountDTO
            {
                AccountId = id,
                Authority = AuthorityType.GameMaster,
                Name = "test" + id,
                Password = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff"
            };
            DAOFactory.AccountDAO.InsertOrUpdate(ref account);

            // register for account login
            CommunicationServiceClient.Instance.RegisterAccountLogin(account.AccountId, 12345);

            // OpenNosEntryPoint -> LoadCharacterList
            client.ReceivePacket("12345");
            client.ReceivePacket(account.Name);
            client.ReceivePacket("test");

            string clistStart = WaitForPacket(client);

            string clistEnd = WaitForPacket(client);

            // creation of character
            client.ReceivePacket($"Char_NEW {account.Name} 2 1 0 9");

            List<string> clistAfterCreate = WaitForPackets(client, 3);
            CListPacket cListPacket = PacketFactory.Deserialize<CListPacket>(clistAfterCreate[1]);

            // select character
            client.ReceivePacket($"select {cListPacket.Slot}");
            string okPacket = WaitForPacket(client);

            // start game
            client.ReceivePacket("game_start");
            List<string> gameStartPacketsFirstPart = WaitForPackets(client, "p_clear");
            List<string> gameStartPacketsSecondPart = WaitForPackets(client, "p_clear");

            // wait 100 milliseconds to be sure initialization has been finished
            Thread.Sleep(100);

            return client;
        }

        public static FakeNetworkClient InitializeTestEnvironment()
        {
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            // initialize Logger
            Logger.InitializeLogger(LogManager.GetLogger(typeof(BasicPacketHandlerTest)));

            // register mappings for items
            RegisterMappings();

            // create server entities (this values would have been imported)
            CreateServerItems();
            CreateServerMaps();
            CreateServerSkills();

            // initialize servermanager
            ServerManager.Instance.Initialize();

            // initialize PacketSerialization
            PacketFactory.Initialize<WalkPacket>();

            // initialize new manager
            _sessionManager = new NetworkManager<TestEncryption>("127.0.0.1", 1234, typeof(CharacterScreenPacketHandler), typeof(TestEncryption), true);

            return CreateFakeNetworkClient();
        }

        public static void ShutdownTestingEnvironment()
        {
            _sessionManager.StopServer();
        }

        public static string WaitForPacket(FakeNetworkClient client)
        {
            DateTime startTime = DateTime.Now;

            while (true)
            {
                if (client.SentPackets.Count > 0)
                {
                    string packet = client.SentPackets.Dequeue();
                    Debug.WriteLine($"Dequeued {packet}");
                    return packet;
                }

                // exit token
                if (startTime.AddSeconds(10) < DateTime.Now)
                {
                    Assert.Fail($"Timed out while waiting for a Packet.");
                    return string.Empty;
                }
            }
        }

        public static string WaitForPacket(FakeNetworkClient client, string packetHeader)
        {
            DateTime startTime = DateTime.Now;

            while (true)
            {
                if (client.SentPackets.Count > 0)
                {
                    string packet = client.SentPackets.Dequeue();
                    Debug.WriteLine($"Dequeued {packet}");
                    if (packet != null && packet.StartsWith(packetHeader))
                    {
                        return packet;
                    }
                }

                // exit token
                if (startTime.AddSeconds(10) < DateTime.Now)
                {
                    Assert.Fail($"Timed out while waiting for {packetHeader}");
                    return string.Empty;
                }
            }
        }

        public static List<string> WaitForPackets(FakeNetworkClient client, int amount)
        {
            DateTime startTime = DateTime.Now;

            int receivedPackets = 0;
            List<string> packets = new List<string>();
            while (receivedPackets < amount)
            {
                if (client.SentPackets.Count > 0)
                {
                    packets.Add(client.SentPackets.Dequeue());
                    receivedPackets++;
                }

                // exit token
                if (startTime.AddSeconds(10) < DateTime.Now)
                {
                    Assert.Fail($"Timed out while waiting for {amount} Packets");
                    return new List<string>();
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
            DAOFactory.ItemDAO.Insert(new ItemDTO
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
            DAOFactory.ItemDAO.Insert(new ItemDTO
            {
                VNum = 8,
                EquipmentSlot = EquipmentType.SecondaryWeapon
            });
            DAOFactory.ItemDAO.Insert(new ItemDTO
            {
                VNum = 12,
                EquipmentSlot = EquipmentType.Armor
            });
        }

        private static void CreateServerMaps()
        {
            MapDTO testingMap = new MapDTO
            {
                MapId = 1,
                Music = 1,
                Name = "Testing-Map",
                ShopAllowed = true
            };
            List<byte> mapData = new List<byte>
            {
                255, // x length
                0, // x length
                255, // y length
                0 // y length
            };

            // create map grid
            for (int i = 0; i < 255; i++)
            {
                for (int j = 0; j < 255; j++)
                {
                    // we can go everywhere
                    mapData.Add(0);
                }
            }
            testingMap.Data = mapData.ToArray();
            DAOFactory.MapDAO.Insert(testingMap);
        }

        private static void CreateServerSkills()
        {
            DAOFactory.SkillDAO.Insert(new SkillDTO
            {
                SkillVNum = 200,
                CastId = 0
            });
            DAOFactory.SkillDAO.Insert(new SkillDTO
            {
                SkillVNum = 201,
                CastId = 1
            });
            DAOFactory.SkillDAO.Insert(new SkillDTO
            {
                SkillVNum = 209,
                CastId = 2
            });
        }

        private static void RegisterMappings()
        {
            // register mappings for items
            DAOFactory.IteminstanceDAO.RegisterMapping(typeof(SpecialistInstance));
            DAOFactory.IteminstanceDAO.RegisterMapping(typeof(WearableInstance));
            DAOFactory.IteminstanceDAO.InitializeMapper(typeof(ItemInstance));

            // entities
            DAOFactory.AccountDAO.RegisterMapping(typeof(Account)).InitializeMapper();
            DAOFactory.CellonOptionDAO.RegisterMapping(typeof(CellonOptionDTO)).InitializeMapper();
            DAOFactory.CharacterDAO.RegisterMapping(typeof(Character)).InitializeMapper();
            DAOFactory.CharacterSkillDAO.RegisterMapping(typeof(CharacterSkill)).InitializeMapper();
            DAOFactory.ComboDAO.RegisterMapping(typeof(ComboDTO)).InitializeMapper();
            DAOFactory.DropDAO.RegisterMapping(typeof(DropDTO)).InitializeMapper();
            DAOFactory.GeneralLogDAO.RegisterMapping(typeof(GeneralLogDTO)).InitializeMapper();
            DAOFactory.ItemDAO.RegisterMapping(typeof(ItemDTO)).InitializeMapper();
            DAOFactory.MailDAO.RegisterMapping(typeof(MailDTO)).InitializeMapper();
            DAOFactory.MapDAO.RegisterMapping(typeof(MapDTO)).InitializeMapper();
            DAOFactory.MapMonsterDAO.RegisterMapping(typeof(MapMonster)).InitializeMapper();
            DAOFactory.MapNpcDAO.RegisterMapping(typeof(MapNpc)).InitializeMapper();
            DAOFactory.MapTypeDAO.RegisterMapping(typeof(MapTypeDTO)).InitializeMapper();
            DAOFactory.MapTypeMapDAO.RegisterMapping(typeof(MapTypeMapDTO)).InitializeMapper();
            DAOFactory.NpcMonsterDAO.RegisterMapping(typeof(NpcMonster)).InitializeMapper();
            DAOFactory.NpcMonsterSkillDAO.RegisterMapping(typeof(NpcMonsterSkill)).InitializeMapper();
            DAOFactory.PenaltyLogDAO.RegisterMapping(typeof(PenaltyLogDTO)).InitializeMapper();
            DAOFactory.PortalDAO.RegisterMapping(typeof(PortalDTO)).InitializeMapper();
            DAOFactory.QuicklistEntryDAO.RegisterMapping(typeof(QuicklistEntryDTO)).InitializeMapper();
            DAOFactory.RecipeDAO.RegisterMapping(typeof(Recipe)).InitializeMapper();
            DAOFactory.RecipeItemDAO.RegisterMapping(typeof(RecipeItemDTO)).InitializeMapper();
            DAOFactory.RespawnDAO.RegisterMapping(typeof(RespawnDTO)).InitializeMapper();
            DAOFactory.ShopDAO.RegisterMapping(typeof(Shop)).InitializeMapper();
            DAOFactory.ShopItemDAO.RegisterMapping(typeof(ShopItemDTO)).InitializeMapper();
            DAOFactory.ShopSkillDAO.RegisterMapping(typeof(ShopSkillDTO)).InitializeMapper();
            DAOFactory.SkillDAO.RegisterMapping(typeof(Skill)).InitializeMapper();
            DAOFactory.TeleporterDAO.RegisterMapping(typeof(TeleporterDTO)).InitializeMapper();
        }

        #endregion
    }
}