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

using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class CommandPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public CommandPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session { get { return _session; } }

        #endregion

        #region Methods

        [Packet("$AddMonster")]
        public void AddMonster(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short vnum = 0, isMoving = 0;

            Random rnd = new Random();

            if (packetsplit.Length == 4 && short.TryParse(packetsplit[2], out vnum) && short.TryParse(packetsplit[3], out isMoving))
            {
                NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                if (npcmonster == null)
                    return;

                MapMonsterDTO monst = new MapMonsterDTO() { MonsterVNum = vnum, MapY = Session.Character.MapY, MapX = Session.Character.MapX, MapId = Session.Character.MapId, Position = (byte)Session.Character.Direction, IsMoving = isMoving == 1 ? true : false, MapMonsterId = MapMonster.GenerateMapMonsterId() };
                MapMonster monster = null;
                Map map = ServerManager.GetMap(monst.MapId);
                if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) == null)
                {
                    DAOFactory.MapMonsterDAO.Insert(monst);
                    monster = new MapMonster(map, vnum) { MapY = monst.MapY, Alive = true, CurrentHp = npcmonster.MaxHP, CurrentMp = npcmonster.MaxMP, MapX = monst.MapX, MapId = Session.Character.MapId, firstX = monst.MapX, firstY = monst.MapY, MapMonsterId = monst.MapMonsterId, Position = 1, IsMoving = isMoving == 1 ? true : false };
                    ServerManager.Monsters.Add(monster);
                    Session.CurrentMap.Monsters.Add(monster);
                    Session.CurrentMap?.Broadcast(monster.GenerateIn3());
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 10));
        }

        [Packet("$Ban")]
        public void Ban(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int duration = 0;
            if (packetsplit.Length >= 4)
            {
                string name = packetsplit[2];
                string reason = packetsplit[3];
                if (packetsplit.Length > 4)
                {
                    Int32.TryParse(packetsplit[4], out duration);
                }
                ServerManager.Instance.Kick(packetsplit[2]);
                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                {
                    DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO()
                    {
                        AccountId = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId,
                        Reason = reason,
                        Penalty = PenaltyType.Banned,
                        DateStart = DateTime.Now,
                        DateEnd = duration == 0 ? DateTime.Now.AddYears(15) : DateTime.Now.AddDays(duration)
                    });
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME REASON TIME", 10));
                Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME REASON", 10));
            }
        }

        [Packet("$ChangeClass")]
        public void ChangeClass(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte Class;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out Class) && Class < 4)
                {
                    Session.Character.ChangeClass(Class);
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 10));
        }

        [Packet("$Guri")]
        public void TestGuri(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte type = 0, argument = 0;
            short value = 0;
            if (packetsplit.Length > 3)
            {
                if (byte.TryParse(packetsplit[2], out type) && byte.TryParse(packetsplit[3], out argument) && short.TryParse(packetsplit[4], out value))
                {
                    Session.SendPacket(Session.Character.GenerateGuri(type, argument, value));
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Guri TYPE ARGUMENT VALUE", 10));
        }

        [Packet("$FLvl")]
        public void ChangeFairyLevel(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short fairylevel;
            WearableInstance fairy = Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((short)EquipmentType.Fairy, InventoryType.Equipment);
            if (fairy != null && packetsplit.Length > 2)
            {
                if (short.TryParse(packetsplit[2], out fairylevel) && fairylevel <= 25565)
                {
                    fairylevel -= fairy.Item.ElementRate;
                    fairy.ElementRate = fairylevel;
                    fairy.XP = 0;
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVEL_CHANGED"), fairy.Item.Name), 10));
                    Session.SendPacket(Session.Character.GeneratePairy());
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$FLvl FAIRYLEVEL", 10));
        }

        [Packet("$ChangeSex")]
        public void ChangeGender(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.ChangeSex();
        }

        [Packet("$HeroLvl")]
        public void ChangeHeroLevel(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte hlevel;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out hlevel) && hlevel < 51 && hlevel > 0)
                {
                    Session.Character.HeroLevel = hlevel;
                    Session.Character.HeroXp = 0;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HEROLEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(Session.Character.GenerateStatInfo());
                    Session.SendPacket(Session.Character.GenerateStatChar());
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(6));
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(198));
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$HeroLvl HEROLEVEL", 10));
        }

        [Packet("$JLvl")]
        public void ChangeJobLevel(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte joblevel;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out joblevel) && ((Session.Character.Class == 0 && joblevel <= 20) || (Session.Character.Class != 0 && joblevel <= 80)) && joblevel > 0)
                {
                    Session.Character.JobLevel = joblevel;
                    Session.Character.JobLevelXp = 0;
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(8));
                    Session.SendPacket(Session.Character.GenerateSki());
                    Session.Character.LearnAdventurerSkill();
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 10));
        }

        [Packet("$Lvl")]
        public void ChangeLevel(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte level;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out level) && level < 100 && level > 0)
                {
                    Session.Character.Level = level;
                    Session.Character.LevelXp = 0;
                    Session.Character.Hp = (int)Session.Character.HPLoad();
                    Session.Character.Mp = (int)Session.Character.MPLoad();
                    Session.SendPacket(Session.Character.GenerateStat());
                    Session.SendPacket(Session.Character.GenerateStatInfo());
                    Session.SendPacket(Session.Character.GenerateStatChar());
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(6));
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(198));
                    ServerManager.Instance.UpdateGroup(Session.Character.CharacterId);
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 10));
        }

        [Packet("$ChangeRep")]
        public void ChangeReputation(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            long reput;
            if (packetsplit.Length != 3)
            {
                Session.SendPacket(Session.Character.GenerateSay("$ChangeRep REPUTATION", 10));
                return;
            }

            if (Int64.TryParse(packetsplit[2], out reput) && reput > 0)
            {
                Session.Character.Reput = reput;
                Session.SendPacket(Session.Character.GenerateFd());
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REP_CHANGED"), 0));
                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
            }
        }

        [Packet("$SPLvl")]
        public void ChangeSpecialistLevel(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte splevel;
            SpecialistInstance sp = Session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Equipment);
            if (sp != null && packetsplit.Length > 2 && Session.Character.UseSp)
            {
                if (Byte.TryParse(packetsplit[2], out splevel) && splevel <= 99 && splevel > 0)
                {
                    sp.SpLevel = splevel;
                    sp.XP = 0;
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateSki());
                    Session.Character.LearnSPSkill();
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(8));
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 10));
        }

        [Packet("$Help")]
        public void Command(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.SendPacket(Session.Character.GenerateSay("-------------Commands Info-------------", 11));
            Session.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME REASON TIME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME REASON", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeRep REPUTATION", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeSex", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SearchMonster NAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SearchItem NAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Effect EFFECTID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$FLvl FAIRYLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$GodMode", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Guri TYPE ARGUMENT VALUE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$HairColor COLORID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$HairStyle STYLEID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$HeroLvl HEROLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Invisible", 12));
            Session.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Kick CHARACTERNAME REASON", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Kill CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$MapDance", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME REASON TIME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME REASON", 12));
            Session.SendPacket(Session.Character.GenerateSay("$PlayMusic MUSIC", 12));
            Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Position", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RateDrop RATE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RateFairyXp RATE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RateGold RATE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RateXp RATE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SkillAdd SKILLID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SPRefill", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Shout MESSAGE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Shutdown", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Stat", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Teleport Map X Y", 12));
            Session.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Unban CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Unmute CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 12));
            Session.SendPacket(Session.Character.GenerateSay("$WigColor COLORID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Zoom VALUE", 12));
            Session.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 11));
        }

        [Packet("$SearchMonster")]
        public void SearchMonster(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                IEnumerable<NpcMonsterDTO> monsterlist = DAOFactory.NpcMonsterDAO.FindByName(packetsplit[2]).OrderBy(s => s.NpcMonsterVNum).ToList();
                if (monsterlist.Any())
                {
                    foreach (NpcMonsterDTO NpcMonster in monsterlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay($"Monster : {NpcMonster.Name} VNum {NpcMonster.NpcMonsterVNum}", 12));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MONSTER_NOT_FOUND"), 11));
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$SearchMonster NAME", 10));
        }

        [Packet("$SearchItem")]
        public void SearchItem(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                IEnumerable<ItemDTO> itemlist = DAOFactory.ItemDAO.FindByName(packetsplit[2]).OrderBy(s => s.VNum).ToList();
                if (itemlist.Any())
                {
                    foreach (ItemDTO item in itemlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay($"Item : {item.Name} VNum {item.VNum}", 12));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_NOT_FOUND"), 11));
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$SearchItem NAME", 10));
        }

        [Packet("$CreateItem")]
        public void CreateItem(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte amount = 1, upgrade = 0, design = 0;
            sbyte rare = 0;
            short vnum;
            Item iteminfo = null;
            if (packetsplit.Length != 5 && packetsplit.Length != 4 && packetsplit.Length != 3)
            {
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 10));
            }
            else if (short.TryParse(packetsplit[2], out vnum))
            {
                iteminfo = ServerManager.GetItem(vnum);
                if (iteminfo != null)
                {
                    if (iteminfo.IsColored)
                    {
                        if (packetsplit.Count() > 3)
                            byte.TryParse(packetsplit[3], out design);
                    }
                    else if (iteminfo.Type == 0)
                    {
                        if (packetsplit.Length == 4)
                        {
                            sbyte.TryParse(packetsplit[3], out rare);
                        }
                        else if (packetsplit.Length == 5)
                        {
                            if (iteminfo.EquipmentSlot == Convert.ToByte((byte)EquipmentType.Sp))
                            {
                                byte.TryParse(packetsplit[3], out upgrade);
                                upgrade = upgrade > 15 ? (byte)15 : upgrade;
                                byte.TryParse(packetsplit[4], out design);
                            }
                            else
                            {
                                sbyte.TryParse(packetsplit[3], out rare);
                                byte.TryParse(packetsplit[4], out upgrade);
                                upgrade = upgrade > 10 ? (byte)10 : upgrade;
                                if (upgrade == 0)
                                    if (iteminfo.BasicUpgrade != 0)
                                    {
                                        upgrade = iteminfo.BasicUpgrade;
                                    }
                            }
                        }
                    }
                    else
                    {
                        if (packetsplit.Length > 3 && !byte.TryParse(packetsplit[3], out amount))
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                            return;
                        }
                    }
                    amount = amount > 99 ? (byte)99 : amount;
                    Inventory inv = Session.Character.InventoryList.AddNewItemToInventory(vnum, amount);
                    if (inv != null)
                    {
                        inv.ItemInstance.Rare = rare;
                        inv.ItemInstance.Upgrade = upgrade;
                        inv.ItemInstance.Design = design;

                        WearableInstance wearable = Session.Character.InventoryList.LoadBySlotAndType<WearableInstance>(inv.Slot, inv.Type);

                        if (wearable != null && (wearable.Item.EquipmentSlot == (byte)EquipmentType.Armor || wearable.Item.EquipmentSlot == (byte)EquipmentType.MainWeapon || wearable.Item.EquipmentSlot == (byte)EquipmentType.SecondaryWeapon))
                        {
                            wearable.SetRarityPoint();
                        }

                        short Slot = inv.Slot;
                        if (Slot != -1)
                        {
                            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {amount}", 12));
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(vnum, inv.ItemInstance.Amount, iteminfo.Type, Slot, rare, design, upgrade, 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                    }
                }
                else
                {
                    Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_ITEM"), 0);
                }
            }
        }

        [Packet("$PortalTo")]
        public void CreatePortal(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short mapid, destx, desty = 0;
            sbyte portaltype = -1;
            if (packetsplit.Length > 4 && short.TryParse(packetsplit[2], out mapid) && short.TryParse(packetsplit[3], out destx) && short.TryParse(packetsplit[4], out desty))
            {
                if (ServerManager.GetMap(mapid) == null)
                    return;

                short mapId = Session.Character.MapId;
                short mapX = Session.Character.MapX;
                short mapY = Session.Character.MapY;
                if (packetsplit.Length > 5)
                    sbyte.TryParse(packetsplit[5], out portaltype);
                Portal portal = new Portal() { SourceMapId = mapId, SourceX = mapX, SourceY = mapY, DestinationMapId = mapid, DestinationX = destx, DestinationY = desty, Type = portaltype };
                Session.CurrentMap.Portals.Add(portal);
                Session.CurrentMap?.Broadcast(Session.Character.GenerateGp(portal));
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 10));
        }

        [Packet("$Effect")]
        public void Effect(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short arg = 0;
            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);
                Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(arg));
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Effect EFFECT", 10));
        }

        [Packet("$GodMode")]
        public void GodMode(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.HasGodMode = Session.Character.HasGodMode == true ? false : true;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        [Packet("$Gold")]
        public void Gold(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            long gold;
            if (packetsplit.Length > 2)
            {
                if (Int64.TryParse(packetsplit[2], out gold))
                {
                    gold = gold > 1000000000 ? 1000000000 : gold;
                    if (gold >= 0)
                    {
                        Session.Character.Gold = gold;
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_SET"), 0));
                        Session.SendPacket(Session.Character.GenerateGold());
                    }
                    else
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 10));
        }

        [Packet("$HairColor")]
        public void Haircolor(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte haircolor;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out haircolor) && haircolor < 128)
                {
                    Session.Character.HairColor = haircolor;
                    Session.SendPacket(Session.Character.GenerateEq());
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateIn());
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$HairColor COLORID", 10));
        }

        [Packet("$HairStyle")]
        public void Hairstyle(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte hairstyle;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out hairstyle))
                {
                    Session.Character.HairStyle = hairstyle;
                    Session.SendPacket(Session.Character.GenerateEq());
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateIn());
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$HairStyle STYLEID", 10));
        }

        [Packet("$Invisible")]
        public void Invisible(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.Invisible = Session.Character.Invisible ? false : true;
            Session.CurrentMap?.Broadcast(Session.Character.GenerateInvisible());
            Session.Character.InvisibleGm = Session.Character.InvisibleGm ? false : true;
            Session.SendPacket(Session.Character.GenerateEq());
            if (Session.Character.InvisibleGm == true)
            {
                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllExceptMe);
            }
            else
            {
                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
            }
        }

        [Packet("$Kick")]
        public void Kick(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
                ServerManager.Instance.Kick(packetsplit[2]);
            else
                Session.SendPacket(Session.Character.GenerateSay("$Kick CHARACTERNAME", 10));
        }

        [Packet("$Kill")]
        public void Kill(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                string name = packetsplit[2];

                long? id = ServerManager.Instance.GetProperty<long?>(name, nameof(Character.CharacterId));

                if (id != null)
                {
                    int? Hp = ServerManager.Instance.GetProperty<int?>((long)id, nameof(Character.Hp));
                    if (Hp == 0)
                        return;
                    ServerManager.Instance.SetProperty((long)id, nameof(Character.Hp), 0);
                    ServerManager.Instance.SetProperty((long)id, nameof(Character.LastDefence), DateTime.Now);
                    Session.CurrentMap?.Broadcast($"su 1 {Session.Character.CharacterId} 1 {id} 1114 4 11 4260 0 0 0 0 {60000} 3 0");
                    Session.CurrentMap?.Broadcast(null, ServerManager.Instance.GetUserMethod<string>((long)id, nameof(Character.GenerateStat)), ReceiverType.OnlySomeone, String.Empty, (long)id);
                    ServerManager.Instance.AskRevive((long)id);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Kill CHARACTERNAME", 10));
        }

        [Packet("$MapDance")]
        public void MapDance(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.CurrentMap.IsDancing = Session.CurrentMap.IsDancing == 0 ? 2 : 0;
            if (Session.CurrentMap.IsDancing == 2)
            {
                Session.Character.Dance();
                ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(Session.Character.MapId) && s.Character.Name != Session.Character.Name).ToList().ForEach(s => ServerManager.Instance.RequireBroadcastFromUser(Session, s.Character.CharacterId, "Dance"));
                Session.CurrentMap?.Broadcast("dance 2");
            }
            else
            {
                Session.Character.Dance();
                ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(Session.Character.MapId) && s.Character.Name != Session.Character.Name).ToList().ForEach(s => ServerManager.Instance.RequireBroadcastFromUser(Session, s.Character.CharacterId, "GenerateIn"));
                Session.CurrentMap?.Broadcast("dance");
            }
        }

        [Packet("$Morph")]
        public void Morph(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[4];
            bool verify = false;
            if (packetsplit.Length > 5)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && short.TryParse(packetsplit[5], out arg[3]));
            }
            switch (packetsplit.Length)
            {
                case 6:
                    if (verify)
                    {
                        if (arg[0] < 30 && arg[0] > 0)
                        {
                            Session.Character.UseSp = true;
                            Session.Character.Morph = arg[0];
                            Session.Character.MorphUpgrade = arg[1];
                            Session.Character.MorphUpgrade2 = arg[2];
                            Session.Character.ArenaWinner = arg[3];
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                        }
                        else if (arg[0] > 30)
                        {
                            Session.Character.IsVehicled = true;
                            Session.Character.Morph = arg[0];
                            Session.Character.ArenaWinner = arg[3];
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                        }
                        else
                        {
                            Session.Character.IsVehicled = false;
                            Session.Character.UseSp = false;
                            Session.Character.ArenaWinner = 0;
                            Session.SendPacket(Session.Character.GenerateCond());
                            Session.SendPacket(Session.Character.GenerateLev());
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                        }
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 10));
                    break;
            }
        }

        [Packet("$Mute")]
        public void Mute(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte duration;
            if (packetsplit.Length > 3)
            {
                string name = packetsplit[2];
                string reason = packetsplit[3];

                if (packetsplit.Length <= 4)
                    duration = 1;
                else
                    Byte.TryParse(packetsplit[4], out duration);

                ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                if (duration != 0)
                {
                    if (session != null)
                    {
                        session.Account.PenaltyLogs.Add(new PenaltyLog()
                        {
                            AccountId = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId,
                            Reason = reason,
                            Penalty = PenaltyType.Muted,
                            DateStart = DateTime.Now,
                            DateEnd = DateTime.Now.AddHours(duration)
                        });
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                        if (duration == 1)
                            ServerManager.Instance.Broadcast(Session, Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), reason)), ReceiverType.OnlySomeone, name);
                        else
                            ServerManager.Instance.Broadcast(Session, Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)), ReceiverType.OnlySomeone, name);
                    }
                    else if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO()
                        {
                            AccountId = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId,
                            Reason = reason,
                            Penalty = (byte)PenaltyType.Muted,
                            DateStart = DateTime.Now,
                            DateEnd = DateTime.Now.AddHours(duration)
                        });
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                        ServerManager.Instance.Broadcast(Session, Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("MUTED"), reason, duration)), ReceiverType.OnlySomeone, name);
                    }
                    else
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME REASON TIME", 10));
                Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME REASON", 10));
            }
        }

        [Packet("$PlayMusic")]
        public void PlayMusic(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                if (packetsplit.Length <= 1) return;

                short arg;
                short.TryParse(packetsplit[2], out arg);
                if (arg > -1)
                    Session.CurrentMap?.Broadcast($"bgm {arg}");
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$PlayMusic BGMUSIC", 10));
        }

        [Packet("$Position")]
        public void Position(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.SendPacket(Session.Character.GenerateSay($"Map:{Session.Character.MapId} - X:{Session.Character.MapX} - Y:{Session.Character.MapY}", 12));
        }

        [Packet("$Rarify")]
        public void Rarify(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 10));
            }
            else
            {
                short itemslot = -1;
                short mode = -1;
                short protection = -1;
                short.TryParse(packetsplit[2], out itemslot);
                short.TryParse(packetsplit[3], out mode);
                short.TryParse(packetsplit[4], out protection);

                if (itemslot > -1 && mode > -1 && protection > -1)
                {
                    WearableInstance wearableInstance = Session.Character.InventoryList.LoadBySlotAndType<WearableInstance>(itemslot, 0);
                    if (wearableInstance != null)
                    {
                        wearableInstance.RarifyItem(Session, (RarifyMode)mode, (RarifyProtection)protection);
                    }
                }
            }
        }

        [Packet("$RateDrop")]
        public void RateDrop(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int rate;
            if (packetsplit.Length > 2)
            {
                if (int.TryParse(packetsplit[2], out rate) && rate <= 1000)
                {
                    ServerManager.DropRate = rate;

                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_RATE_CHANGED"), 0));
                }
                else
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$RateDrop RATE", 10));
        }

        [Packet("$RateFairyXp")]
        public void RateFairyXp(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int rate;
            if (packetsplit.Length > 2)
            {
                if (int.TryParse(packetsplit[2], out rate) && rate <= 1000)
                {
                    ServerManager.FairyXpRate = rate;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("FAIRYXP_RATE_CHANGED"), 0));
                }
                else
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$RateFairyXp RATE", 10));
        }

        [Packet("$RateGold")]
        public void RateGold(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int rate;
            if (packetsplit.Length > 2)
            {
                if (int.TryParse(packetsplit[2], out rate) && rate <= 1000)
                {
                    ServerManager.GoldRate = rate;

                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_RATE_CHANGED"), 0));
                }
                else
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$RateGold RATE", 10));
        }

        [Packet("$RateXp")]
        public void RateXp(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int rate;
            if (packetsplit.Length > 2)
            {
                if (int.TryParse(packetsplit[2], out rate) && rate <= 1000)
                {
                    ServerManager.XPRate = rate;

                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("XP_RATE_CHANGED"), 0));
                }
                else
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$RateXp RATE", 10));
        }

        [Packet("$Resize")]
        public void Resize(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short arg = -1;

            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);

                if (arg > -1)

                {
                    Session.Character.Size = arg;
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateScal());
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 10));
        }

        [Packet("$SkillAdd")]
        public void SkillAdd(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short vnum = 0;
            if (packetsplit.Length > 2 && short.TryParse(packetsplit[2], out vnum))
            {
                Skill skillinfo = ServerManager.GetSkill(vnum);
                if (skillinfo == null)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_DOES_NOT_EXIST"), 11));
                    return;
                }

                if (skillinfo.SkillVNum < 200)
                {
                    for (int i = Session.Character.Skills.Count - 1; i >= 0; i--)
                    {
                        if ((skillinfo.CastId == Session.Character.Skills[i].Skill.CastId) && (Session.Character.Skills[i].Skill.SkillVNum < 200))
                        {
                            Session.Character.Skills.Remove(Session.Character.Skills[i]);
                        }
                    }
                }
                else
                {
                    if (Session.Character.Skills.Any(s => s.SkillVNum == vnum))
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_ALREADY_EXIST"), 11));
                        return;
                    }
                    //if (Session.Character.Class != skillinfo.Class)
                    //{
                    //    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                    //    return;
                    //}
                    if (skillinfo.UpgradeSkill != 0)
                    {
                        CharacterSkill oldupgrade = Session.Character.Skills.FirstOrDefault(s => s.Skill.UpgradeSkill == skillinfo.UpgradeSkill && s.Skill.UpgradeType == skillinfo.UpgradeType && s.Skill.UpgradeSkill != 0);
                        if (oldupgrade != null)
                        {
                            Session.Character.Skills.Remove(oldupgrade);
                        }
                    }
                }

                Session.Character.Skills.Add(new CharacterSkill() { SkillVNum = vnum, CharacterId = Session.Character.CharacterId });

                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                Session.SendPacket(Session.Character.GenerateLev());
            }
            else
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
        }

        [Packet("$Shout")]
        public void Shout(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            if (packetsplit.Length > 2)
                for (int i = 2; i < packetsplit.Length; i++)
                    message += packetsplit[i] + " ";
            message.Trim();

            ServerManager.Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            ServerManager.Instance.Broadcast(Session.Character.GenerateMsg(message, 2));
        }

        [Packet("$Shutdown")]
        public void Shutdown(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            if (ServerManager.Instance.TaskShutdown != null)
            {
                ServerManager.Instance.ShutdownStop = true;
                ServerManager.Instance.TaskShutdown = null;
            }
            else
            {
                ServerManager.Instance.TaskShutdown = new Task(ShutdownTask);
                ServerManager.Instance.TaskShutdown.Start();
            }
        }

        [Packet("$Speed")]
        public void Speed(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte arg = 0;
            bool verify = false;
            if (packetsplit.Length > 2)
            {
                verify = (byte.TryParse(packetsplit[2], out arg));
            }
            switch (packetsplit.Length)
            {
                case 3:
                    if (verify && arg < 60)
                    {
                        Session.Character.Speed = arg;
                        Session.Character.IsCustomSpeed = true;
                        Session.SendPacket(Session.Character.GenerateCond());
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 10));
                    break;
            }
        }

        [Packet("$SPRefill")]
        public void SPRefill(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.SpPoint = 10000;
            Session.Character.SpAdditionPoint = 1000000;
            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SP_REFILL"), 0));
            Session.SendPacket(Session.Character.GenerateSpPoint());
        }

        [Packet("$Stat")]
        public void Stat(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("TOTAL_SESSION")}: {ServerManager.Instance.Sessions.Count()} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("XP_RATE_NOW")}: {ServerManager.XPRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("DROP_RATE_NOW")}: {ServerManager.DropRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("GOLD_RATE_NOW")}: {ServerManager.GoldRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("FAIRYXP_RATE_NOW")}: {ServerManager.FairyXpRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("SERVER_WORKING_TIME")}: {(Process.GetCurrentProcess().StartTime - DateTime.Now).ToString("d\\ hh\\:mm\\:ss")} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("MEMORY")}: {(GC.GetTotalMemory(true) / (1024 * 1024))}MB ", 13));
        }

        [Packet("$Summon")]
        public void Summon(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short vnum = 0;
            byte qty = 1, move = 0;
            Random rnd = new Random();
            if (packetsplit.Length == 5 && short.TryParse(packetsplit[2], out vnum) && byte.TryParse(packetsplit[3], out qty) && byte.TryParse(packetsplit[4], out move))
            {
                NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                if (npcmonster == null)
                    return;
                Map map = Session.CurrentMap;
                for (int i = 0; i < qty; i++)
                {
                    short mapx = (short)rnd.Next(Session.Character.MapX - 4, Session.Character.MapX + 4);
                    short mapy = (short)rnd.Next(Session.Character.MapY - 4, Session.Character.MapY + 4);
                    while (Session.CurrentMap.IsBlockedZone(mapx, mapy))
                    {
                        mapx = (short)rnd.Next(Session.Character.MapX - 4, Session.Character.MapX + 4);
                        mapy = (short)rnd.Next(Session.Character.MapY - 4, Session.Character.MapY + 4);
                    }
                    MapMonster monst = new MapMonster(map, vnum) { Alive = true, CurrentHp = npcmonster.MaxHP, CurrentMp = npcmonster.MaxMP, MapY = mapy, MapX = mapx, MapId = Session.Character.MapId, firstX = mapx, firstY = mapy, MapMonsterId = MapMonster.GenerateMapMonsterId(), Position = 1, IsMoving = move != 0 ? true : false };
                    Session.CurrentMap.Monsters.Add(monst);
                    ServerManager.Monsters.Add(monst);
                    Session.CurrentMap?.Broadcast(monst.GenerateIn3());
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 10));
        }

        [Packet("$Teleport")]
        public void Teleport(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[3];
            bool verify = false;

            if (packetsplit.Length > 4)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && DAOFactory.MapDAO.LoadById(arg[0]) != null);
            }
            switch (packetsplit.Length)
            {
                case 3:
                    string name = packetsplit[2];
                    short? mapId = ServerManager.Instance.GetProperty<short?>(name, nameof(Character.MapId));
                    short? mapx = ServerManager.Instance.GetProperty<short?>(name, nameof(Character.MapX));
                    short? mapy = ServerManager.Instance.GetProperty<short?>(name, nameof(Character.MapY));
                    if (mapy != null && mapx != null && mapId != null)
                    {
                        ServerManager.Instance.MapOut(Session.Character.CharacterId);
                        Session.Character.MapId = (short)mapId;
                        Session.Character.MapX = (short)((short)(mapx) + 1);
                        Session.Character.MapY = (short)((short)(mapy) + 1);

                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                    break;

                case 5:
                    if (verify)
                    {
                        ServerManager.Instance.MapOut(Session.Character.CharacterId);
                        Session.Character.MapId = arg[0];
                        Session.Character.MapX = arg[1];
                        Session.Character.MapY = arg[2];

                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Teleport MAP X Y", 10));
                    Session.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 10));
                    break;
            }
        }

        [Packet("$TeleportToMe")]
        public void TeleportToMe(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');

            if (packetsplit.Length == 3)
            {
                string name = packetsplit[2];

                long? id = ServerManager.Instance.GetProperty<long?>(name, nameof(Character.CharacterId));

                if (id != null)
                {
                    ServerManager.Instance.MapOut((long)id);
                    ServerManager.Instance.SetProperty((long)id, nameof(Character.IsSitting), false);
                    ServerManager.Instance.SetProperty((long)id, nameof(Character.MapId), Session.Character.MapId);
                    ServerManager.Instance.SetProperty((long)id, nameof(Character.MapX), (short)((Session.Character.MapX) + (short)1));
                    ServerManager.Instance.SetProperty((long)id, nameof(Character.MapY), (short)((Session.Character.MapY) + (short)1));
                    ServerManager.Instance.ChangeMap((long)id);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME", 10));
        }

        [Packet("$Unban")]
        public void Unban(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                string name = packetsplit[2];
                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                {
                    if (DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId).Any(s => s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId).FirstOrDefault(s => s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now);
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        DAOFactory.PenaltyLogDAO.Update(log);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_BANNED"), 10));
                }
                else
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Unban CHARACTERNAME", 10));
        }

        [Packet("$Unmute")]
        public void Unmute(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                string name = packetsplit[2];
                ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);

                if (session != null)
                {
                    if (session.Account.PenaltyLogs.Where(s => s.AccountId == session.Account.AccountId && s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now).Any())
                    {
                        PenaltyLog log = session.Account.PenaltyLogs.Where(s => s.AccountId == session.Account.AccountId && s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now).FirstOrDefault();
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_MUTED"), 10));
                }
                else if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                {
                    if (DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId).Any(s => s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId).FirstOrDefault(s => s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now);
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        DAOFactory.PenaltyLogDAO.Update(log);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_MUTED"), 10));
                }
                else
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$Unmute CHARACTERNAME", 10));
        }

        [Packet("$Upgrade")]
        public void Upgrade(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 10));
            }
            else
            {
                short itemslot = -1;
                short mode = -1;
                short protection = -1;
                short.TryParse(packetsplit[2], out itemslot);
                short.TryParse(packetsplit[3], out mode);
                short.TryParse(packetsplit[4], out protection);

                if (itemslot > -1 && mode > -1 && protection > -1)
                {
                    WearableInstance wearableInstance = Session.Character.InventoryList.LoadBySlotAndType<WearableInstance>(itemslot, 0);
                    if (wearableInstance != null)
                    {
                        wearableInstance.UpgradeItem(Session, (UpgradeMode)mode, (UpgradeProtection)protection);
                    }
                }
            }
        }

        [Packet("$WigColor")]
        public void WigColor(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte wigcolor = 0;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out wigcolor))
                {
                    WearableInstance wig = Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Equipment);
                    if (wig != null)
                    {
                        wig.Design = wigcolor;
                        Session.SendPacket(Session.Character.GenerateEq());
                        Session.SendPacket(Session.Character.GenerateEquipment());
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateIn());
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                        return;
                    }
                }
            }
            else
                Session.SendPacket(Session.Character.GenerateSay("$WigColor COLORID", 10));
        }

        [Packet("$Zoom")]
        public void Zoom(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte arg = 0;
            if (packetsplit.Length > 2 && byte.TryParse(packetsplit[2], out arg))
            {
                Session.SendPacket(Session.Character.GenerateGuri(15, arg));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Zoom VALUE", 0));
            }
        }

        private async void ShutdownTask()
        {
            string message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 5);
            Session.CurrentMap?.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Session.CurrentMap?.Broadcast(Session.Character.GenerateMsg(message, 2));
            for (int i = 0; i < 60 * 4; i++)
            {
                await Task.Delay(1000);
                if (ServerManager.Instance.ShutdownStop == true)
                {
                    ServerManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
            Session.CurrentMap?.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Session.CurrentMap?.Broadcast(Session.Character.GenerateMsg(message, 2));
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (ServerManager.Instance.ShutdownStop == true)
                {
                    ServerManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
            Session.CurrentMap?.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            Session.CurrentMap?.Broadcast(Session.Character.GenerateMsg(message, 2));
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (ServerManager.Instance.ShutdownStop == true)
                {
                    ServerManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            ServerManager.Instance.SaveAll();
            Environment.Exit(0);
        }

        #endregion
    }
}