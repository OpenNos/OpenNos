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

using AutoMapper;
using OpenNos.Core;
using OpenNos.Core.Collections;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ServerManager : BroadcastableBase
    {
        #region Members

        public Boolean ShutdownStop = false;

        private static ServerManager _instance;
        private static List<Item> _items = new List<Item>();

        private static IMapper _mapper;

        private static ConcurrentDictionary<Guid, Map> _maps = new ConcurrentDictionary<Guid, Map>();

        private static List<NpcMonster> _npcs = new List<NpcMonster>();

        private static List<Skill> _skills = new List<Skill>();

        private ThreadSafeSortedList<short, List<DropDTO>> _dropsByMonster;

        private List<DropDTO> _generalDrops;

        private ThreadSafeSortedList<long, Group> _groups;

        private ThreadSafeSortedList<short, List<NpcMonsterSkill>> _monsterSkills;

        private long lastGroupId;

        #endregion

        #region Instantiation

        static ServerManager()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemDTO, NoFunctionItem>();
                cfg.CreateMap<ItemDTO, WearableItem>();
                cfg.CreateMap<ItemDTO, BoxItem>();
                cfg.CreateMap<ItemDTO, MagicalItem>();
                cfg.CreateMap<ItemDTO, FoodItem>();
                cfg.CreateMap<ItemDTO, PotionItem>();
                cfg.CreateMap<ItemDTO, ProduceItem>();
                cfg.CreateMap<ItemDTO, SnackItem>();
                cfg.CreateMap<ItemDTO, SpecialItem>();
                cfg.CreateMap<ItemDTO, TeacherItem>();
                cfg.CreateMap<ItemDTO, UpgradeItem>();
                cfg.CreateMap<SkillDTO, Skill>();
                cfg.CreateMap<NpcMonsterDTO, NpcMonster>();
                cfg.CreateMap<NpcMonsterSkillDTO, NpcMonsterSkill>();
            });

            _mapper = config.CreateMapper();
        }

        private ServerManager()
        {
            _groups = new ThreadSafeSortedList<long, Group>();

            Task autosave = new Task(SaveAllProcess);
            autosave.Start();

            Task GroupTask = new Task(() => GroupProcess());
            GroupTask.Start();

            Task TaskController = new Task(() => TaskLauncherProcess());
            TaskController.Start();

            lastGroupId = 1;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ItemDTO, NoFunctionItem>();
                cfg.CreateMap<ItemDTO, WearableItem>();
                cfg.CreateMap<ItemDTO, BoxItem>();
                cfg.CreateMap<ItemDTO, MagicalItem>();
                cfg.CreateMap<ItemDTO, FoodItem>();
                cfg.CreateMap<ItemDTO, PotionItem>();
                cfg.CreateMap<ItemDTO, ProduceItem>();
                cfg.CreateMap<ItemDTO, SnackItem>();
                cfg.CreateMap<ItemDTO, SpecialItem>();
                cfg.CreateMap<ItemDTO, TeacherItem>();
                cfg.CreateMap<ItemDTO, UpgradeItem>();
                cfg.CreateMap<SkillDTO, Skill>();
                cfg.CreateMap<NpcMonsterDTO, NpcMonster>();
                cfg.CreateMap<NpcMonsterSkillDTO, NpcMonsterSkill>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Properties

        public static int DropRate { get; set; }

        public static int FairyXpRate { get; set; }

        public static int GoldRate { get; set; }

        public static int XPRate { get; set; }

        public List<Group> Groups
        {
            get
            {
                return _groups.GetAllItems();
            }
        }

        public static ServerManager Instance => _instance ?? (_instance = new ServerManager());

        public Task TaskShutdown { get; set; }

        private bool _disposed;

        #endregion

        #region Methods

        public override void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dropsByMonster.Dispose();
                _groups.Dispose();
                _monsterSkills.Dispose();
            }
        }

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
            ClientSession Session = GetSessionByCharacterId(characterId);
            if (Session != null && Session.HasSelectedCharacter)
            {
                if (Session.Character.IsVehicled)
                {
                    Session.Character.RemoveVehicle();
                }
                Session.SendPacket(Session.Character.GenerateStat());
                Session.SendPacket(Session.Character.GenerateCond());
                Session.SendPackets(Session.Character.GenerateVb());
                if (Session.Character.Level > 20)
                {
                    Session.Character.Dignity -= (short)(Session.Character.Level < 50 ? Session.Character.Level : 50);
                    if (Session.Character.Dignity < -1000)
                    {
                        Session.Character.Dignity = -1000;
                    }
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LOSE_DIGNITY"), (short)(Session.Character.Level < 50 ? Session.Character.Level : 50)), 11));
                    Session.SendPacket(Session.Character.GenerateFd());
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                }
                Session.SendPacket("eff_ob -1 -1 0 4269");
                Session.SendPacket(Session.Character.GenerateDialog($"#revival^0 #revival^1 {(Session.Character.Level > 20 ? Language.Instance.GetMessageFromKey("ASK_REVIVE") : Language.Instance.GetMessageFromKey("ASK_REVIVE_FREE"))}"));

                Task.Factory.StartNew(async () =>
                {
                    for (int i = 1; i <= 30; i++)
                    {
                        await Task.Delay(1000);
                        if (Session.Character.Hp > 0)
                        {
                            return;
                        }
                    }
                    Instance.ReviveFirstPosition(Session.Character.CharacterId);
                });
            }
        }

        // PacketHandler
        public void BuyValidate(ClientSession clientSession, KeyValuePair<long, MapShop> shop, short slot, byte amount)
        {
            PersonalShopItem shopitem = clientSession.CurrentMap.UserShops[shop.Key].Items.FirstOrDefault(i => i.ShopSlot.Equals(slot));
            if (shopitem == null)
            {
                return;
            }
            Guid id = shopitem.Id;
            shopitem.Amount -= amount;
            if (shopitem.Amount <= 0)
            {
                clientSession.CurrentMap.UserShops[shop.Key].Items.Remove(shopitem);
            }

            ClientSession shopOwnerSession = GetSessionByCharacterId(shop.Value.OwnerId);
            if (shopOwnerSession == null)
            {
                return;
            }

            shopOwnerSession.Character.Gold += shopitem.Price * amount;
            shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateGold());
            shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), shopOwnerSession.Character.Name, shopitem.Item.Name, amount)));
            clientSession.CurrentMap.UserShops[shop.Key].Sell += shopitem.Price * amount;
            shopOwnerSession.SendPacket($"sell_list {shop.Value.Sell} {slot}.{amount}.{shopitem.Amount}");

            ItemInstance inv = shopOwnerSession.Character.Inventory.RemoveItemAmountFromInventory(amount, id);

            if (inv != null)
            {
                // Send reduced-amount to owners inventory
                shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
            }
            else
            {
                // Send empty slot to owners inventory
                shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(-1, 0, shopitem.Type, shopitem.Slot, 0, 0, 0, 0));
                if (!clientSession.CurrentMap.UserShops[shop.Key].Items.Any(s => s.Amount > 0))
                {
                    clientSession.SendPacket("shop_end 0");
                    shopOwnerSession.CurrentMap?.Broadcast(shopOwnerSession, shopOwnerSession.Character.GenerateShopEnd(), ReceiverType.All);
                    shopOwnerSession.CurrentMap?.Broadcast(shopOwnerSession, shopOwnerSession.Character.GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                    shopOwnerSession.Character.LoadSpeed();
                    shopOwnerSession.Character.IsSitting = false;
                    shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateCond());
                    shopOwnerSession.CurrentMap?.Broadcast(shopOwnerSession, shopOwnerSession.Character.GenerateRest(), ReceiverType.All);
                }
            }
        }

        // Both partly
        public void ChangeMap(long id)
        {
            ClientSession session = GetSessionByCharacterId(id);
            if (session != null && session.Character != null && !session.Character.IsChangingMap)
            {
                try
                {
                    session.Character.IsChangingMap = true;
                    session.CurrentMap.UnregisterSession(session.Character.CharacterId);
                    session.CurrentMap = GetMap(session.Character.MapId);
                    session.CurrentMap.RegisterSession(session);
                    session.SendPacket(session.Character.GenerateCInfo());
                    session.SendPacket(session.Character.GenerateCMode());
                    session.SendPacket(session.Character.GenerateEq());
                    session.SendPacket(session.Character.GenerateEquipment());
                    session.SendPacket(session.Character.GenerateLev());
                    session.SendPacket(session.Character.GenerateStat());
                    session.SendPacket(session.Character.GenerateAt());
                    session.SendPacket(session.Character.GenerateCMap());
                    session.SendPacket(session.Character.GenerateStatChar());
                    session.SendPacket(session.Character.GenerateCond());
                    session.SendPacket($"gidx 1 {session.Character.CharacterId} -1 - 0"); // family
                    session.SendPacket("rsfp 0 -1");

                    // in 2 // send only when partner present cond 2 // send only when partner present
                    session.SendPacket("pinit 0"); // clear party list
                    session.SendPacket(session.Character.GeneratePairy());
                    session.SendPacket("act6"); // act6 1 0 14 0 0 0 14 0 0 0

                    Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(session.Character.MapId) && s.Character.Name != session.Character.Name && !s.Character.InvisibleGm).ToList().ForEach(s => RequireBroadcastFromUser(session, s.Character.CharacterId, "GenerateIn"));

                    session.SendPackets(session.Character.GenerateGp());

                    // wp 23 124 4 4 12 99
                    session.SendPackets(session.Character.GenerateIn3());
                    session.SendPackets(session.Character.GenerateIn2());
                    session.SendPackets(session.Character.GenerateNPCShopOnMap());
                    session.SendPackets(session.Character.GenerateDroppedItem());
                    session.SendPackets(session.Character.GenerateShopOnMap());
                    session.SendPackets(session.Character.GeneratePlayerShopOnMap());
                    if (!session.Character.InvisibleGm)
                    {
                        session.CurrentMap?.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    }
                    if (session.Character.Size != 10)
                    {
                        session.SendPacket(session.Character.GenerateScal());
                    }
                    if (session.CurrentMap.IsDancing && !session.Character.IsDancing)
                    {
                        session.CurrentMap?.Broadcast("dance 2");
                    }
                    else if (!session.CurrentMap.IsDancing && session.Character.IsDancing)
                    {
                        session.Character.IsDancing = false;
                        session.CurrentMap?.Broadcast("dance");
                    }
                    foreach (Group g in Groups)
                    {
                        foreach (ClientSession groupSession in g.Characters)
                        {
                            ClientSession chara = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == groupSession.Character.CharacterId && s.CurrentMap.MapId == groupSession.CurrentMap.MapId);
                            if (chara != null)
                            {
                                groupSession.SendPacket(groupSession.Character.GeneratePinit());
                            }
                            if (groupSession.Character.CharacterId == groupSession.Character.CharacterId)
                            {
                                session.CurrentMap?.Broadcast(groupSession, groupSession.Character.GeneratePidx(), ReceiverType.AllExceptMe);
                            }
                        }
                    }

                    //cleanup sending queue to avoid sending uneccessary packets to it
                    session.ClearLowpriorityQueue();
                    session.Character.IsChangingMap = false;
                }
                catch (Exception)
                {
                    Logger.Log.Warn("Character changed while changing map. Do not abuse Commands.");
                    session.Character.IsChangingMap = false;
                }
            }
        }

        // PacketHandler
        public void ExchangeValidate(ClientSession c1Session, long charId)
        {
            ClientSession c2Session = GetSessionByCharacterId(charId);

            if (c2Session == null || c2Session.Character.ExchangeInfo == null)
            {
                return;
            }
            foreach (ItemInstance item in c2Session.Character.ExchangeInfo.ExchangeList)
            {
                ItemInstance invtemp = c2Session.Character.Inventory.FirstOrDefault(s => s.Id == item.Id);
                short slot = invtemp.Slot;
                InventoryType type = invtemp.Type;

                ItemInstance inv = c2Session.Character.Inventory.RemoveItemAmountFromInventory((byte)item.Amount, invtemp.Id);
                if (inv != null)
                {
                    // Send reduced-amount to owners inventory
                    c2Session.SendPacket(c2Session.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
                }
                else
                {
                    // Send empty slot to owners inventory
                    c2Session.SendPacket(c2Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
                }
            }

            foreach (ItemInstance item in c1Session.Character.ExchangeInfo.ExchangeList)
            {
                ItemInstance item2 = item.DeepCopy();
                item2.Id = Guid.NewGuid();
                ItemInstance inv = c2Session.Character.Inventory.AddToInventory(item2);
                if (inv == null || inv.Slot == -1)
                {
                    continue;
                }
                c2Session.SendPacket(c2Session.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
            }

            c2Session.Character.Gold = c2Session.Character.Gold - c2Session.Character.ExchangeInfo.Gold + c1Session.Character.ExchangeInfo.Gold;
            c2Session.SendPacket(c2Session.Character.GenerateGold());
            c1Session.Character.ExchangeInfo = null;
            c2Session.Character.ExchangeInfo = null;
        }

        public List<DropDTO> GetDropsByMonsterVNum(short monsterVNum)
        {
            if (_dropsByMonster.ContainsKey(monsterVNum))
            {
                return _generalDrops.Concat(_dropsByMonster[monsterVNum]).ToList();
            }

            return new List<DropDTO>();
        }

        public Group GetGroupByCharacterId(long characterId)
        {
            return Groups.SingleOrDefault(g => g.IsMemberOfGroup(characterId));
        }

        public long GetNextGroupId()
        {
            lastGroupId++;
            return lastGroupId;
        }

        public T GetProperty<T>(string charName, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(charName));
            if (session == null)
            {
                return default(T);
            }
            return (T)session?.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public T GetProperty<T>(long charId, string property)
        {
            ClientSession session = GetSessionByCharacterId(charId);
            if (session == null)
            {
                return default(T);
            }
            return (T)session?.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
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
            Group grp = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(session.Character.CharacterId));
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

        public void Initialize()
        {
            XPRate = int.Parse(System.Configuration.ConfigurationManager.AppSettings["RateXp"]);

            DropRate = int.Parse(System.Configuration.ConfigurationManager.AppSettings["RateDrop"]);

            GoldRate = int.Parse(System.Configuration.ConfigurationManager.AppSettings["RateGold"]);

            FairyXpRate = int.Parse(System.Configuration.ConfigurationManager.AppSettings["RateFairyXp"]);

            foreach (ItemDTO itemDTO in DAOFactory.ItemDAO.LoadAll())
            {
                Item ItemGO = null;

                switch (itemDTO.ItemType)
                {
                    case Domain.ItemType.Ammo:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case Domain.ItemType.Armor:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    case Domain.ItemType.Box:
                        ItemGO = _mapper.Map<BoxItem>(itemDTO);
                        break;

                    case Domain.ItemType.Event:
                        ItemGO = _mapper.Map<MagicalItem>(itemDTO);
                        break;

                    case Domain.ItemType.Fashion:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    case Domain.ItemType.Food:
                        ItemGO = _mapper.Map<FoodItem>(itemDTO);
                        break;

                    case Domain.ItemType.Jewelery:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    case Domain.ItemType.Magical:
                        ItemGO = _mapper.Map<MagicalItem>(itemDTO);
                        break;

                    case Domain.ItemType.Main:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case Domain.ItemType.Map:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case Domain.ItemType.Part:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case Domain.ItemType.Potion:
                        ItemGO = _mapper.Map<PotionItem>(itemDTO);
                        break;

                    case Domain.ItemType.Production:
                        ItemGO = _mapper.Map<ProduceItem>(itemDTO);
                        break;

                    case Domain.ItemType.Quest1:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case Domain.ItemType.Quest2:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case Domain.ItemType.Sell:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;

                    case Domain.ItemType.Shell:
                        ItemGO = _mapper.Map<MagicalItem>(itemDTO);
                        break;

                    case Domain.ItemType.Snack:
                        ItemGO = _mapper.Map<SnackItem>(itemDTO);
                        break;

                    case Domain.ItemType.Special:
                        ItemGO = _mapper.Map<SpecialItem>(itemDTO);
                        break;

                    case Domain.ItemType.Specialist:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    case Domain.ItemType.Teacher:
                        ItemGO = _mapper.Map<TeacherItem>(itemDTO);
                        break;

                    case Domain.ItemType.Upgrade:
                        ItemGO = _mapper.Map<UpgradeItem>(itemDTO);
                        break;

                    case Domain.ItemType.Weapon:
                        ItemGO = _mapper.Map<WearableItem>(itemDTO);
                        break;

                    default:
                        ItemGO = _mapper.Map<NoFunctionItem>(itemDTO);
                        break;
                }
                _items.Add(ItemGO);
            }

            // intialize monster drops
            _dropsByMonster = new ThreadSafeSortedList<short, List<DropDTO>>();
            foreach (var monsterDropGrouping in DAOFactory.DropDAO.LoadAll().GroupBy(d => d.MonsterVNum))
            {
                if (monsterDropGrouping.Key.HasValue)
                {
                    _dropsByMonster[monsterDropGrouping.Key.Value] = monsterDropGrouping.OrderBy(d => d.DropChance).ToList();
                }
                else
                {
                    _generalDrops = monsterDropGrouping.ToList();
                }
            }

            // initialiize monster skills
            _monsterSkills = new ThreadSafeSortedList<short, List<NpcMonsterSkill>>();
            foreach (var monsterSkillGrouping in DAOFactory.NpcMonsterSkillDAO.LoadAll().GroupBy(n => n.NpcMonsterVNum))
            {
                _monsterSkills[monsterSkillGrouping.Key] = monsterSkillGrouping.Select(n => _mapper.Map<NpcMonsterSkill>(n)).ToList();
            }

            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("ITEM_LOADED"), _items.Count()));
            foreach (SkillDTO skillDTO in DAOFactory.SkillDAO.LoadAll())
            {
                Skill skill = _mapper.Map<Skill>(skillDTO);
                foreach (ComboDTO com in DAOFactory.ComboDAO.LoadBySkillVnum(skill.SkillVNum).ToList())
                {
                    skill.Combos.Add(_mapper.Map<ComboDTO>(com));
                }
                _skills.Add(skill);
            }
            foreach (NpcMonsterDTO npcmonsterDTO in DAOFactory.NpcMonsterDAO.LoadAll())
            {
                _npcs.Add(_mapper.Map<NpcMonster>(npcmonsterDTO));
            }
            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("NPCMONSTERS_LOADED"), _npcs.Count()));

            try
            {
                int i = 0;
                int npccount = 0;
                int recipescount = 0;
                int shopcount = 0;
                int monstercount = 0;
                foreach (MapDTO map in DAOFactory.MapDAO.LoadAll())
                {
                    Guid guid = Guid.NewGuid();
                    Map newMap = new Map(Convert.ToInt16(map.MapId), guid, map.Data);
                    newMap.Music = map.Music;
                    newMap.ShopAllowed = map.ShopAllowed;

                    // register for broadcast
                    _maps.TryAdd(guid, newMap);
                    i++;
                    npccount += newMap.Npcs.Count();

                    foreach (MapMonster n in newMap.Monsters)
                    {
                        newMap.AddMonster(n);
                    }
                    monstercount += newMap.Monsters.Count();
                    foreach (MapNpc n in newMap.Npcs.Where(n => n.Shop != null))
                    {
                        shopcount++;
                    }
                    foreach (MapNpc npcs in newMap.Npcs)
                    {
                        foreach (Recipe recipies in npcs.Recipes)
                        {
                            recipescount++;
                        }
                    }
                }
                if (i != 0)
                {
                    Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MAP_LOADED"), i));
                }
                else
                {
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("NO_MAP"));
                }
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("SKILLS_LOADED"), _skills.Count()));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MONSTERS_LOADED"), monstercount));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("NPCS_LOADED"), npccount));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("SHOPS_LOADED"), shopcount));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("RECIPES_LOADED"), recipescount));
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        public bool IsCharacterMemberOfGroup(long characterId)
        {
            return Groups.Any(g => g.IsMemberOfGroup(characterId));
        }

        public bool IsCharactersGroupFull(long characterId)
        {
            return (Groups.Any(g => g.IsMemberOfGroup(characterId) && g.CharacterCount == 3));
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
        public void MapOut(long id)
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
                MapOut(session.Character.CharacterId);
                session.Character.MapId = 1;
                session.Character.MapX = 80;
                session.Character.MapY = 116;
                session.Character.Hp = 1;
                session.Character.Mp = 1;
                ChangeMap(session.Character.CharacterId);
                session.CurrentMap?.Broadcast(session, session.Character.GenerateTp(), ReceiverType.All);
                session.CurrentMap?.Broadcast(session.Character.GenerateRevive(), 200);
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
                Group myGroup = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId));
                if (myGroup == null)
                {
                    return;
                }
                string str = $"pinit { myGroup.Characters.Count()}";
                int i = 0;
                IList<ClientSession> groupMembers = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId))?.Characters;
                foreach (ClientSession session in groupMembers)
                {
                    i++;
                    str += $" 1|{session.Character.CharacterId}|{i}|{session.Character.Level}|{session.Character.Name}|11|{session.Character.Gender}|{session.Character.Class}|{(session.Character.UseSp ? session.Character.Morph : 0)}|{session.Character.HeroLevel}";
                }

                foreach (ClientSession session in myGroup.Characters)
                {
                    session.SendPacket(str);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        internal List<NpcMonsterSkill> GetNpcMonsterSkillsByMonsterVNum(short npcMonsterVNum)
        {
            if (_monsterSkills.ContainsKey(npcMonsterVNum))
            {
                return _monsterSkills[npcMonsterVNum];
            }

            return new List<NpcMonsterSkill>();
        }

        internal void StopServer()
        {
            ServerManager.Instance.ShutdownStop = true;
            ServerManager.Instance.TaskShutdown = null;
        }

        // Server
        private async void GroupProcess()
        {
            while (true)
            {
                try
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
                catch (Exception)
                {
                }

                await Task.Delay(2000);
            }
        }

        private void RemoveGroup(Group grp)
        {
            _groups.Remove(grp.GroupId);
        }

        // Server
        private async void SaveAllProcess()
        {
            while (true)
            {
                await Task.Delay(60000 * 4);
                Logger.Log.Info(Language.Instance.GetMessageFromKey("SAVING_ALL"));
                SaveAll();
            }
        }

        // Map ??
        private void TaskLauncherProcess()
        {
            List<Task> TaskMaps = null;
            while (true)
            {
                TaskMaps = new List<Task>();
                foreach (var map in _maps.Where(s => s.Value.Sessions.Any() || s.Value.LastUnregister.AddSeconds(30) > DateTime.Now))
                {
                    TaskMaps.Add(new Task(() => map.Value.MapTaskManager()));
                    map.Value.Disabled = false;
                }
                foreach (var map in _maps.Where(s => !s.Value.Disabled && (!s.Value.Sessions.Any() && s.Value.LastUnregister.AddSeconds(30) < DateTime.Now)))
                {
                    map.Value.Disabled = true;
                }
                TaskMaps.ForEach(s => s.Start());
                Task.WaitAll(TaskMaps.ToArray());
                Thread.Sleep(300);
            }
        }

        #endregion
    }
}