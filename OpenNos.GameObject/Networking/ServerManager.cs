/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class ServerManager : BroadcastableBase
    {
        #region Members

        public bool ShutdownStop;

        private static ServerManager _instance;
        private static List<Item> _items = new List<Item>();

        private static ConcurrentDictionary<Guid, Map> _maps = new ConcurrentDictionary<Guid, Map>();

        private static List<NpcMonster> _npcs = new List<NpcMonster>();

        private static List<Skill> _skills = new List<Skill>();

        private bool _disposed;

        private List<DropDTO> _generalDrops;
        private ThreadSafeSortedList<long, Group> _groups;
        private ThreadSafeSortedList<short, List<MapNpc>> _mapNpcs;
        private ThreadSafeSortedList<short, List<DropDTO>> _monsterDrops;
        private ThreadSafeSortedList<short, List<NpcMonsterSkill>> _monsterSkills;

        private ThreadSafeSortedList<int, List<Recipe>> _recipes;

        private ThreadSafeSortedList<int, List<ShopItemDTO>> _shopItems;
        private ThreadSafeSortedList<int, Shop> _shops;

        private ThreadSafeSortedList<int, List<ShopSkillDTO>> _shopSkills;
        private ThreadSafeSortedList<int, List<TeleporterDTO>> _teleporters;
        private long _lastGroupId;

        #endregion

        #region Instantiation

        private ServerManager()
        {
        }

        public void LaunchEvents()
        {
            _groups = new ThreadSafeSortedList<long, Group>();

            Observable.Interval(TimeSpan.FromMinutes(5)).Subscribe(x =>
            {
                SaveAllProcess();
            });

            Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(x =>
            {
                GroupProcess();
            });

            Observable.Interval(TimeSpan.FromHours(3)).Subscribe(x =>
            {
                BotProcess();
            });

            Observable.Interval(TimeSpan.FromSeconds(30)).Subscribe(x =>
            {
                MailProcess();
            });

            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(x =>
            {
                RemoveItemProcess();
            });

            foreach (var map in _maps)
            {
                Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(x =>
                {
                    try
                    {
                        if (!map.Value.IsSleeping)
                        {
                            map.Value.RemoveMapItem();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                });

                foreach (MapNpc npc in map.Value.Npcs)
                {
                    npc.StartLife();
                }

                Observable.Interval(TimeSpan.FromMilliseconds(400)).Subscribe(x =>
                {
                    Parallel.ForEach(map.Value.Monsters, monster => { monster.StartLife(); });
                });
            }

            _lastGroupId = 1;
        }

        private void RemoveItemProcess()
        {
            try
            {
                Sessions.Where(c => c.IsConnected).ToList().ForEach(s => s.Character?.RefreshValidity());
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void MailProcess()
        {
            try
            {
                Mails = DAOFactory.MailDAO.LoadAll().ToList();
                Sessions.Where(c => c.IsConnected).ToList().ForEach(s => s.Character?.RefreshMail());
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion

        #region Properties

        public static int DropRate { get; set; }

        public static List<MailDTO> Mails { get; set; }

        public static int FairyXpRate { get; set; }

        public static int GoldDropRate { get; set; }

        public static int GoldRate { get; set; }

        public static int XpRate { get; set; }

        public List<Group> Groups
        {
            get
            {
                return _groups.GetAllItems();
            }
        }

        public static ServerManager Instance => _instance ?? (_instance = new ServerManager());

        public Task TaskShutdown { get; set; }

        #endregion

        #region Methods

        public static ConcurrentDictionary<Guid, Map> GetAllMap()
        {
            return _maps;
        }

        public static IEnumerable<Skill> GetAllSkill()
        {
            return _skills;
        }

        public static Item GetItem(short vnum)
        {
            return _items.FirstOrDefault(m => m.VNum.Equals(vnum));
        }

        public static Map GetMap(short id)
        {
            return _maps.FirstOrDefault(m => m.Value.MapId.Equals(id)).Value;
        }

        public static NpcMonster GetNpc(short npcVNum)
        {
            return _npcs.FirstOrDefault(m => m.NpcMonsterVNum.Equals(npcVNum));
        }

        public static Skill GetSkill(short skillVNum)
        {
            return _skills.FirstOrDefault(m => m.SkillVNum.Equals(skillVNum));
        }

        public void AddGroup(Group group)
        {
            _groups[group.GroupId] = group;
        }

        // PacketHandler -> with Callback?
        public void AskRevive(long characterId)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session != null && session.HasSelectedCharacter)
            {
                if (session.Character.IsVehicled)
                {
                    session.Character.RemoveVehicle();
                }
                session.SendPacket(session.Character.GenerateStat());
                session.SendPacket(session.Character.GenerateCond());
                session.SendPackets(session.Character.GenerateVb());
                if (session.Character.Level > 20)
                {
                    session.Character.Dignity -= (short)(session.Character.Level < 50 ? session.Character.Level : 50);
                    if (session.Character.Dignity < -1000)
                    {
                        session.Character.Dignity = -1000;
                    }
                    session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LOSE_DIGNITY"), (short)(session.Character.Level < 50 ? session.Character.Level : 50)), 11));
                    session.SendPacket(session.Character.GenerateFd());
                    session.CurrentMap?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                }
                session.SendPacket("eff_ob -1 -1 0 4269");
                session.SendPacket(session.Character.GenerateDialog($"#revival^0 #revival^1 {(session.Character.Level > 20 ? Language.Instance.GetMessageFromKey("ASK_REVIVE") : Language.Instance.GetMessageFromKey("ASK_REVIVE_FREE"))}"));

                Parallel.Invoke(delegate
                {
                    for (int i = 1; i <= 30; i++)
                    {
                        Thread.Sleep(1000);
                        if (session.Character.Hp > 0)
                        {
                            return;
                        }
                    }
                    Instance.ReviveFirstPosition(session.Character.CharacterId);
                });
            }
        }

        // Both partly
        public void ChangeMap(long id, short? mapId = null, short? mapX = null, short? mapY = null)
        {
            ClientSession session = GetSessionByCharacterId(id);
            if (session?.Character != null && !session.Character.IsChangingMap)
            {
                try
                {
                    session.Character.IsChangingMap = true;

                    session.CurrentMap.RemoveMonstersTarget(session.Character.CharacterId);
                    session.CurrentMap.UnregisterSession(session.Character.CharacterId);

                    // cleanup sending queue to avoid sending uneccessary packets to it
                    session.ClearLowPriorityQueue();

                    // avoid cleaning new portals
                    if (mapId != null) session.Character.MapId = (short)mapId;
                    if (mapX != null) session.Character.MapX = (short)mapX;
                    if (mapY != null) session.Character.MapY = (short)mapY;

                    session.CurrentMap = GetMap(session.Character.MapId);
                    session.CurrentMap.RegisterSession(session);
                    session.SendPacket(session.Character.GenerateCInfo());
                    session.SendPacket(session.Character.GenerateCMode());
                    session.SendPacket(session.Character.GenerateEq());
                    session.SendPacket(session.Character.GenerateEquipment());
                    session.SendPacket(session.Character.GenerateLev());
                    session.SendPacket(session.Character.GenerateStat());
                    session.SendPacket(session.Character.GenerateAt());
                    session.SendPacket(session.Character.GenerateCond());
                    session.SendPacket(session.Character.GenerateCMap());
                    session.SendPacket(session.Character.GenerateStatChar());
                    session.SendPacket($"gidx 1 {session.Character.CharacterId} -1 - 0"); // family
                    session.SendPacket("rsfp 0 -1");

                    // in 2 // send only when partner present cond 2 // send only when partner present
                    session.SendPacket(session.Character.GeneratePairy());
                    session.SendPacket("pinit 0"); // clear party list
                    session.SendPacket("act6"); // act6 1 0 14 0 0 0 14 0 0 0

                    Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(session.Character.MapId) && s.Character.Name != session.Character.Name && !s.Character.InvisibleGm).ToList().ForEach(s => RequireBroadcastFromUser(session, s.Character.CharacterId, "GenerateIn"));

                    session.SendPackets(session.Character.GenerateGp());

                    // wp 23 124 4 4 12 99
                    session.SendPackets(session.Character.GenerateIn3());
                    session.SendPackets(session.Character.GenerateIn2());
                    session.SendPackets(session.Character.GenerateNpcShopOnMap());
                    session.SendPackets(session.Character.GenerateDroppedItem());
                    session.SendPackets(session.Character.GenerateShopOnMap());
                    session.SendPackets(session.Character.GeneratePlayerShopOnMap());
                    if (mapId == 138)
                    {
                        session.SendPacket("bc 0 0 0");
                    }
                    if (!session.Character.InvisibleGm)
                    {
                        session.CurrentMap?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    }
                    if (session.Character.Size != 10)
                    {
                        session.SendPacket(session.Character.GenerateScal());
                    }
                    if (session.CurrentMap != null && session.CurrentMap.IsDancing && !session.Character.IsDancing)
                    {
                        session.CurrentMap?.Broadcast("dance 2");
                    }
                    else if (session.CurrentMap != null && !session.CurrentMap.IsDancing && session.Character.IsDancing)
                    {
                        session.Character.IsDancing = false;
                        session.CurrentMap?.Broadcast("dance");
                    }
                    if (Groups != null)
                    {
                        foreach (Group g in Groups)
                        {
                            foreach (ClientSession groupSession in g.Characters)
                            {
                                ClientSession chara = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == groupSession.Character.CharacterId && s.CurrentMap.MapId == groupSession.CurrentMap.MapId);
                                if (chara != null)
                                {
                                    groupSession.SendPacket(groupSession.Character.GeneratePinit());
                                }
                                session.CurrentMap?.Broadcast(groupSession, groupSession.Character.GeneratePidx(), ReceiverType.AllExceptMe);
                            }
                        }
                    }

                    session.Character.IsChangingMap = false;
                }
                catch (Exception)
                {
                    Logger.Log.Warn("Character changed while changing map. Do not abuse Commands.");
                    session.Character.IsChangingMap = false;
                }
            }
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public List<DropDTO> GetDropsByMonsterVNum(short monsterVNum)
        {
            if (_monsterDrops.ContainsKey(monsterVNum))
            {
                return _generalDrops.Concat(_monsterDrops[monsterVNum]).ToList();
            }

            return new List<DropDTO>();
        }

        public Group GetGroupByCharacterId(long characterId)
        {
            return Groups?.SingleOrDefault(g => g.IsMemberOfGroup(characterId));
        }

        public long GetNextGroupId()
        {
            _lastGroupId++;
            return _lastGroupId;
        }

        public T GetProperty<T>(string charName, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(charName));
            if (session == null)
            {
                return default(T);
            }
            return (T)session.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public T GetProperty<T>(long charId, string property)
        {
            ClientSession session = GetSessionByCharacterId(charId);
            if (session == null)
            {
                return default(T);
            }
            return (T)session.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public List<Recipe> GetReceipesByMapNpcId(int mapNpcId)
        {
            return _recipes.ContainsKey(mapNpcId) ? _recipes[mapNpcId] : new List<Recipe>();
        }

        public ClientSession GetSessionByCharacterName(string name)
        {
            return Sessions.SingleOrDefault(s => s.Character.Name == name);
        }

        public T GetUserMethod<T>(long characterId, string methodName)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session == null)
            {
                return default(T);
            }
            MethodInfo method = session.Character.GetType().GetMethod(methodName);

            return (T)method.Invoke(session.Character, null);
        }

        public void GroupLeave(ClientSession session)
        {
            if (Groups != null)
            {
                Group grp = Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(session.Character.CharacterId));
                if (grp != null)
                {
                    if (grp.CharacterCount == 3)
                    {
                        if (grp.Characters.ElementAt(0) == session)
                        {
                            Broadcast(session, session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("NEW_LEADER")), ReceiverType.OnlySomeone, String.Empty, grp.Characters.ElementAt(1).Character.CharacterId);
                        }
                        grp.LeaveGroup(session);
                        foreach (ClientSession groupSession in grp.Characters)
                        {
                            ClientSession sess = GetSessionByCharacterId(groupSession.Character.CharacterId);
                            sess.SendPacket(sess.Character.GeneratePinit());
                            sess.SendPacket(sess.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("LEAVE_GROUP"), session.Character.Name), 0));
                        }
                        session.SendPacket("pinit 0");
                        Broadcast(session.Character.GeneratePidx(true));
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_LEFT"), 0));
                    }
                    else
                    {
                        foreach (ClientSession targetSession in grp.Characters)
                        {
                            targetSession.SendPacket("pinit 0");
                            targetSession.SendPacket(targetSession.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_CLOSED"), 0));
                            Broadcast(targetSession.Character.GeneratePidx(true));
                            grp.LeaveGroup(targetSession);
                        }
                        RemoveGroup(grp);
                    }
                    session.Character.Group = null;
                }
            }
        }

        public void Initialize()
        {
            // parse rates
            XpRate = int.Parse(ConfigurationManager.AppSettings["RateXp"]);
            DropRate = int.Parse(ConfigurationManager.AppSettings["RateDrop"]);
            GoldDropRate = int.Parse(ConfigurationManager.AppSettings["GoldRateDrop"]);
            GoldRate = int.Parse(ConfigurationManager.AppSettings["RateGold"]);
            FairyXpRate = int.Parse(ConfigurationManager.AppSettings["RateFairyXp"]);

            Mails = DAOFactory.MailDAO.LoadAll().ToList();

            // load explicite type of ItemDTO
            foreach (ItemDTO item in DAOFactory.ItemDAO.LoadAll())
            {
                Item itemObject;

                switch (item.ItemType)
                {
                    case ItemType.Ammo:
                        itemObject = new NoFunctionItem(item);
                        break;

                    case ItemType.Armor:
                        itemObject = new WearableItem(item);
                        break;

                    case ItemType.Box:
                        itemObject = new BoxItem(item);
                        break;

                    case ItemType.Event:
                        itemObject = new MagicalItem(item);
                        break;

                    case ItemType.Fashion:
                        itemObject = new WearableItem(item);
                        break;

                    case ItemType.Food:
                        itemObject = new FoodItem(item);
                        break;

                    case ItemType.Jewelery:
                        itemObject = new WearableItem(item);
                        break;

                    case ItemType.Magical:
                        itemObject = new MagicalItem(item);
                        break;

                    case ItemType.Main:
                        itemObject = new NoFunctionItem(item);
                        break;

                    case ItemType.Map:
                        itemObject = new NoFunctionItem(item);
                        break;

                    case ItemType.Part:
                        itemObject = new NoFunctionItem(item);
                        break;

                    case ItemType.Potion:
                        itemObject = new PotionItem(item);
                        break;

                    case ItemType.Production:
                        itemObject = new ProduceItem(item);
                        break;

                    case ItemType.Quest1:
                        itemObject = new NoFunctionItem(item);
                        break;

                    case ItemType.Quest2:
                        itemObject = new NoFunctionItem(item);
                        break;

                    case ItemType.Sell:
                        itemObject = new NoFunctionItem(item);
                        break;

                    case ItemType.Shell:
                        itemObject = new MagicalItem(item);
                        break;

                    case ItemType.Snack:
                        itemObject = new SnackItem(item);
                        break;

                    case ItemType.Special:
                        itemObject = new SpecialItem(item);
                        break;

                    case ItemType.Specialist:
                        itemObject = new WearableItem(item);
                        break;

                    case ItemType.Teacher:
                        itemObject = new TeacherItem(item);
                        break;

                    case ItemType.Upgrade:
                        itemObject = new UpgradeItem(item);
                        break;

                    case ItemType.Weapon:
                        itemObject = new WearableItem(item);
                        break;

                    default:
                        itemObject = new NoFunctionItem(item);
                        break;
                }
                _items.Add(itemObject);
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("ITEMS_LOADED"), _items.Count));

            // intialize monsterdrops
            _monsterDrops = new ThreadSafeSortedList<short, List<DropDTO>>();
            foreach (var monsterDropGrouping in DAOFactory.DropDAO.LoadAll().GroupBy(d => d.MonsterVNum))
            {
                if (monsterDropGrouping.Key.HasValue)
                {
                    _monsterDrops[monsterDropGrouping.Key.Value] = monsterDropGrouping.OrderBy(d => d.DropChance).ToList();
                }
                else
                {
                    _generalDrops = monsterDropGrouping.ToList();
                }
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("DROPS_LOADED"), _monsterDrops.GetAllItems().Sum(i => i.Count)));

            // initialiize monsterskills
            _monsterSkills = new ThreadSafeSortedList<short, List<NpcMonsterSkill>>();
            foreach (var monsterSkillGrouping in DAOFactory.NpcMonsterSkillDAO.LoadAll().GroupBy(n => n.NpcMonsterVNum))
            {
                _monsterSkills[monsterSkillGrouping.Key] = monsterSkillGrouping.Select(n => n as NpcMonsterSkill).ToList();
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MONSTERSKILLS_LOADED"), _monsterSkills.GetAllItems().Sum(i => i.Count)));

            // initialize npcmonsters
            foreach (NpcMonsterDTO npcMonster in DAOFactory.NpcMonsterDAO.LoadAll())
            {
                _npcs.Add(npcMonster as NpcMonster);
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("NPCMONSTERS_LOADED"), _npcs.Count));

            // intialize receipes
            _recipes = new ThreadSafeSortedList<int, List<Recipe>>();
            foreach (var recipeGrouping in DAOFactory.RecipeDAO.LoadAll().GroupBy(r => r.MapNpcId))
            {
                _recipes[recipeGrouping.Key] = recipeGrouping.Select(r => r as Recipe).ToList();
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("RECIPES_LOADED"), _recipes.GetAllItems().Sum(i => i.Count)));

            // initialize shopitems
            _shopItems = new ThreadSafeSortedList<int, List<ShopItemDTO>>();
            foreach (var shopItemGrouping in DAOFactory.ShopItemDAO.LoadAll().GroupBy(s => s.ShopId))
            {
                _shopItems[shopItemGrouping.Key] = shopItemGrouping.ToList();
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPITEMS_LOADED"), _shopItems.GetAllItems().Sum(i => i.Count)));

            // initialize shopskills
            _shopSkills = new ThreadSafeSortedList<int, List<ShopSkillDTO>>();
            foreach (var shopSkillGrouping in DAOFactory.ShopSkillDAO.LoadAll().GroupBy(s => s.ShopId))
            {
                _shopSkills[shopSkillGrouping.Key] = shopSkillGrouping.ToList();
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPSKILLS_LOADED"), _shopSkills.GetAllItems().Sum(i => i.Count)));

            // initialize shops
            _shops = new ThreadSafeSortedList<int, Shop>();
            foreach (var shopGrouping in DAOFactory.ShopDAO.LoadAll())
            {
                _shops[shopGrouping.MapNpcId] = (Shop)shopGrouping;
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SHOPS_LOADED"), _shops.GetAllItems().Count));

            // initialize teleporters
            _teleporters = new ThreadSafeSortedList<int, List<TeleporterDTO>>();
            foreach (var teleporterGrouping in DAOFactory.TeleporterDAO.LoadAll().GroupBy(t => t.MapNpcId))
            {
                _teleporters[teleporterGrouping.Key] = teleporterGrouping.Select(t => t).ToList();
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("TELEPORTERS_LOADED"), _teleporters.GetAllItems().Sum(i => i.Count)));

            // initialize skills
            foreach (SkillDTO skill in DAOFactory.SkillDAO.LoadAll())
            {
                Skill skillObject = (Skill)skill;
                skillObject.Combos.AddRange(DAOFactory.ComboDAO.LoadBySkillVnum(skillObject.SkillVNum).ToList());
                _skills.Add(skillObject);
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("SKILLS_LOADED"), _skills.Count));

            // intialize mapnpcs
            _mapNpcs = new ThreadSafeSortedList<short, List<MapNpc>>();
            foreach (var mapNpcGrouping in DAOFactory.MapNpcDAO.LoadAll().GroupBy(t => t.MapId))
            {
                _mapNpcs[mapNpcGrouping.Key] = mapNpcGrouping.Select(t => t as MapNpc).ToList();
            }
            Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPNPCS_LOADED"), _mapNpcs.GetAllItems().Sum(i => i.Count)));

            try
            {
                int i = 0;
                int monstercount = 0;

                foreach (MapDTO map in DAOFactory.MapDAO.LoadAll())
                {
                    Guid guid = Guid.NewGuid();
                    Map newMap = new Map(map.MapId, guid, map.Data)
                    {
                        Music = map.Music,
                        ShopAllowed = map.ShopAllowed
                    };

                    // register for broadcast
                    _maps.TryAdd(guid, newMap);
                    newMap.SetMapMapMonsterReference();
                    newMap.SetMapMapNpcReference();
                    i++;

                    newMap.LoadMonsters();
                    foreach (MapMonster mapMonster in newMap.Monsters)
                    {
                        mapMonster.Map = newMap;
                        newMap.AddMonster(mapMonster);
                    }
                    monstercount += newMap.Monsters.Count;
                }
                if (i != 0)
                {
                    Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPS_LOADED"), i));
                }
                else
                {
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("NO_MAP"));
                }
                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("MAPMONSTERS_LOADED"), monstercount));
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
            LaunchEvents();
        }

        public bool IsCharacterMemberOfGroup(long characterId)
        {
            if (Groups != null)
            {
                return Groups.Any(g => g.IsMemberOfGroup(characterId));
            }
            return false;
        }

        public bool IsCharactersGroupFull(long characterId)
        {
            if (Groups != null)
            {
                return Groups.Any(g => g.IsMemberOfGroup(characterId) && g.CharacterCount == 3);
            }
            return false;
        }

        // Server
        public bool Kick(string characterName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(characterName));
            if (session == null)
            {
                return false;
            }
            session.Disconnect();
            return true;
        }

        // Map
        public void LeaveMap(long id)
        {
            ClientSession session = GetSessionByCharacterId(id);
            if (session == null)
            {
                return;
            }
            session.SendPacket(session.Character.GenerateAt());
            session.SendPacket(session.Character.GenerateCMap());
            session.SendPacket(session.Character.GenerateMapOut());
            session.CurrentMap?.Broadcast(session, session.Character.GenerateOut(), ReceiverType.AllExceptMe);
        }

        public void RequireBroadcastFromUser(ClientSession client, long characterId, string methodName)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session == null)
            {
                return;
            }
            MethodInfo method = session.Character.GetType().GetMethod(methodName);
            string result = (string)method.Invoke(session.Character, null);
            client.SendPacket(result);
        }

        // Map
        public void ReviveFirstPosition(long characterId)
        {
            ClientSession session = GetSessionByCharacterId(characterId);
            if (session != null && session.Character.Hp <= 0)
            {
                Random rnd = new Random();
                LeaveMap(session.Character.CharacterId);
                session.Character.Hp = 1;
                session.Character.Mp = 1;
                RespawnMapTypeDTO resp = session.Character.Respawn;
                short x = (short)(resp.DefaultX + rnd.Next(-5, 5));
                short y = (short)(resp.DefaultY + rnd.Next(-5, 5));
                ChangeMap(session.Character.CharacterId, resp.DefaultMapId, x, y);
                session.CurrentMap?.Broadcast(session, session.Character.GenerateTp());
                session.CurrentMap?.Broadcast(session.Character.GenerateRevive());
                session.SendPacket(session.Character.GenerateStat());
            }
        }

        public void SaveAll()
        {
            List<ClientSession> sessions = Sessions.Where(c => c.IsConnected).ToList();
            sessions.ForEach(s => s.Character?.Save());
        }

        public void SetProperty(long charId, string property, object value)
        {
            ClientSession session = GetSessionByCharacterId(charId);
            if (session == null)
            {
                return;
            }
            PropertyInfo propertyinfo = session.Character.GetType().GetProperties().Single(pi => pi.Name == property);
            propertyinfo.SetValue(session.Character, value, null);
        }

        public void Shout(string message)
        {
            Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Broadcast($"msg 2 {message}");
        }

        // Server
        public void UpdateGroup(long charId)
        {
            try
            {
                if (Groups != null)
                {
                    Group myGroup = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId));
                    if (myGroup == null)
                    {
                        return;
                    }
                    string str = $"pinit {myGroup.Characters.Count}";
                    int i = 0;
                    IList<ClientSession> groupMembers = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId))?.Characters;
                    if (groupMembers != null)
                        foreach (ClientSession session in groupMembers)
                        {
                            i++;
                            str += $" 1|{session.Character.CharacterId}|{i}|{session.Character.Level}|{session.Character.Name}|11|{(byte)session.Character.Gender}|{(byte)session.Character.Class}|{(session.Character.UseSp ? session.Character.Morph : 0)}|{(session.Character.IsVehicled ? 1 : 0)}|{session.Character.HeroLevel}";
                        }

                    foreach (ClientSession session in myGroup.Characters)
                    {
                        session.SendPacket(str);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        internal IEnumerable<MapNpc> GetMapNpcsByMapId(short mapId)
        {
            if (_mapNpcs.ContainsKey(mapId))
            {
                return _mapNpcs[mapId];
            }

            return new List<MapNpc>();
        }

        internal List<NpcMonsterSkill> GetNpcMonsterSkillsByMonsterVNum(short npcMonsterVNum)
        {
            if (_monsterSkills.ContainsKey(npcMonsterVNum))
            {
                return _monsterSkills[npcMonsterVNum];
            }

            return new List<NpcMonsterSkill>();
        }

        internal Shop GetShopByMapNpcId(int mapNpcId)
        {
            if (_shops.ContainsKey(mapNpcId))
            {
                return _shops[mapNpcId];
            }

            return null;
        }

        internal List<ShopItemDTO> GetShopItemsByShopId(int shopId)
        {
            if (_shopItems.ContainsKey(shopId))
            {
                return _shopItems[shopId];
            }

            return new List<ShopItemDTO>();
        }

        internal List<ShopSkillDTO> GetShopSkillsByShopId(int shopId)
        {
            if (_shopSkills.ContainsKey(shopId))
            {
                return _shopSkills[shopId];
            }

            return new List<ShopSkillDTO>();
        }

        internal List<TeleporterDTO> GetTeleportersByNpcVNum(short npcMonsterVNum)
        {
            if (_teleporters != null && _teleporters.ContainsKey(npcMonsterVNum))
            {
                return _teleporters[npcMonsterVNum];
            }
            return new List<TeleporterDTO>();
        }

        internal void StopServer()
        {
            Instance.ShutdownStop = true;
            Instance.TaskShutdown = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monsterDrops.Dispose();
                _groups.Dispose();
                _monsterSkills.Dispose();
                _shopSkills.Dispose();
                _shopItems.Dispose();
                _shops.Dispose();
                _recipes.Dispose();
                _mapNpcs.Dispose();
                _teleporters.Dispose();
            }
        }

        // Server
        private void BotProcess()
        {
            try
            {
                Random rnd = new Random();
                Shout(Language.Instance.GetMessageFromKey($"BOT_MESSAGE_{ rnd.Next(0, 5) }"));
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void GroupProcess()
        {
            try
            {
                if (Groups != null)
                {
                    foreach (Group grp in Groups)
                    {
                        foreach (ClientSession session in grp.Characters)
                        {
                            foreach (string str in grp.GeneratePst())
                            {
                                session.SendPacket(str);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);

            }
        }

        private void RemoveGroup(Group grp)
        {
            _groups.Remove(grp.GroupId);
        }

        // Server
        private void SaveAllProcess()
        {
            try
            {
                Logger.Log.Info(Language.Instance.GetMessageFromKey("SAVING_ALL"));
                SaveAll();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion
    }
}