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
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace OpenNos.Handler
{
    public class BasicPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public BasicPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session
        {
            get
            {
                return _session;
            }
        }

        #endregion

        #region Methods

        [Packet("compl")]
        public void Compliment(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] complimentPacket = packet.Split(' ');
            long complimentedCharacterId = 0;
            if (long.TryParse(complimentPacket[3], out complimentedCharacterId))
            {
                if (Session.Character.Level >= 30)
                {
                    if (Session.Character.LastLogin.AddMinutes(60) <= DateTime.Now)
                    {
                        if (Session.Account.LastCompliment.Date.AddDays(1) <= DateTime.Now.Date)
                        {
                            short? compliment = ServerManager.Instance.GetProperty<short?>(complimentedCharacterId, nameof(Character.Compliment));
                            compliment++;
                            ServerManager.Instance.SetProperty(complimentedCharacterId, nameof(Character.Compliment), compliment);
                            Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_GIVEN"), ServerManager.Instance.GetProperty<string>(complimentedCharacterId, nameof(Character.Name))), 12));
                            AccountDTO account = Session.Account;
                            account.LastCompliment = DateTime.Now;
                            DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_RECEIVED"), Session.Character.Name), 12), ReceiverType.OnlySomeone, complimentPacket[1].Substring(1));
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_COOLDOWN"), 11));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_LOGIN_COOLDOWN"), (Session.Character.LastLogin.AddMinutes(60) - DateTime.Now).Minutes), 11));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_NOT_MINLVL"), 11));
                }
            }
        }

        [Packet("dir")]
        public void Dir(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] dirPacket = packet.Split(' ');

            if (Convert.ToInt32(dirPacket[4]) == Session.Character.CharacterId)
            {
                Session.Character.Direction = Convert.ToInt32(dirPacket[2]);
                Session.CurrentMap?.Broadcast(Session.Character.GenerateDir());
            }
        }

        [Packet("pcl")]
        public void GetGift(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                int id;
                if (!int.TryParse(packetsplit[3], out id))
                {
                    return;
                }

                if (Session.Character.MailList.ContainsKey(id))
                {
                    MailDTO mail = Session.Character.MailList[id];
                    if (packetsplit[2] == "4")
                    {
                        Inventory newInv = Session.Character.InventoryList.AddNewItemToInventory((short)mail.ItemVNum, mail.Amount);
                        if (newInv != null)
                        {
                            if ((newInv.ItemInstance as ItemInstance).Item.ItemType == (byte)ItemType.Armor || (newInv.ItemInstance as ItemInstance).Item.ItemType == (byte)ItemType.Weapon || (newInv.ItemInstance as ItemInstance).Item.ItemType == (byte)ItemType.Shell)
                            {
                                (newInv.ItemInstance as WearableInstance).RarifyItem(Session, RarifyMode.Drop, RarifyProtection.None);
                            }
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemInstance.ItemVNum, newInv.ItemInstance.Amount, newInv.Type, newInv.Slot, newInv.ItemInstance.Rare, newInv.ItemInstance.Design, newInv.ItemInstance.Upgrade, 0));
                            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_GIFTED")}: {(newInv.ItemInstance as ItemInstance).Item.Name} x {mail.Amount}", 12));

                            if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                            {
                                DAOFactory.MailDAO.DeleteById(mail.MailId);
                            }
                            Session.SendPacket($"parcel 2 1 {packetsplit[3]}");
                            if (Session.Character.MailList.ContainsKey(id))
                            {
                                Session.Character.MailList.Remove(id);
                            }
                        }
                        else
                        {
                            Session.SendPacket($"parcel 5 1 0");
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                        }
                    }
                    else if (packetsplit[2] == "5")
                    {
                        Session.SendPacket($"parcel 7 1 {packetsplit[3]}");

                        if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                        {
                            DAOFactory.MailDAO.DeleteById(mail.MailId);
                        }
                        if (Session.Character.MailList.ContainsKey(id))
                        {
                            Session.Character.MailList.Remove(id);
                        }
                    }
                }
            }
        }

        [Packet("ncif")]
        public void GetNamedCharacterInformation(string packet)
        {
            string[] characterInformationPacket = packet.Split(' ');
            long charId = 0;
            if (characterInformationPacket[2] == "1")
            {
                if (Int64.TryParse(characterInformationPacket[3], out charId))
                {
                    ServerManager.Instance.RequireBroadcastFromUser(Session, charId, "GenerateStatInfo");
                }
            }
            if (characterInformationPacket[2] == "2")
            {
                foreach (MapNpc npc in Session.CurrentMap.Npcs)
                {
                    if (npc.MapNpcId == Convert.ToInt32(characterInformationPacket[3]))
                    {
                        NpcMonster npcinfo = ServerManager.GetNpc(npc.NpcVNum);
                        if (npcinfo == null)
                        {
                            return;
                        }
                        Session.SendPacket($"st 2 {characterInformationPacket[3]} {npcinfo.Level} {npcinfo.HeroLevel} 100 100 50000 50000");
                    }
                }
            }
            if (characterInformationPacket[2] == "3")
            {
                foreach (MapMonster monster in Session.CurrentMap.Monsters)
                {
                    if (monster.MapMonsterId == Convert.ToInt32(characterInformationPacket[3]))
                    {
                        NpcMonster monsterinfo = ServerManager.GetNpc(monster.MonsterVNum);
                        if (monsterinfo == null)
                        {
                            return;
                        }
                        Session.Character.LastMonsterId = monster.MapMonsterId;
                        Session.SendPacket($"st 3 {characterInformationPacket[3]} {monsterinfo.Level} {monsterinfo.HeroLevel} {((int)((float)monster.CurrentHp / (float)monsterinfo.MaxHP * 100))} {((int)((float)monster.CurrentMp / (float)monsterinfo.MaxMP * 100))} {monster.CurrentHp} {monster.CurrentMp}");
                    }
                }
            }
        }

        [Packet("npinfo")]
        public void GetStats(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.SendPacket(Session.Character.GenerateStatChar());
        }

        [Packet(";")]
        public void GroupTalk(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 1; i < packetsplit.Length; i++)
            {
                message += packetsplit[i] + " ";
            }
            message = message.Substring(1).Trim();

            ServerManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 3), ReceiverType.Group);
        }

        [Packet("guri")]
        public void Guri(string packet)
        {
            string[] guriPacket = packet.Split(' ');
            if (guriPacket[2] == "10" && Convert.ToInt32(guriPacket[5]) >= 973 && Convert.ToInt32(guriPacket[5]) <= 999 && !Session.Character.EmoticonsBlocked)
            {
                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateEff(Convert.ToInt32(guriPacket[5]) + 4099), ReceiverType.AllNoEmoBlocked);
            }
            if (guriPacket[2] == "2")
            {
                Session.CurrentMap?.Broadcast(Session.Character.GenerateGuri(2, 1));
            }
            else if (guriPacket[2] == "4")
            {
                int speakerVNum = 2173;

                // Speaker
                if (guriPacket[3] == "3")
                {
                    if (Session.Character.InventoryList.CountItem(speakerVNum) > 0)
                    {
                        string message = String.Empty;
                        message = $"<{Language.Instance.GetMessageFromKey("SPEAKER")}> [{Session.Character.Name}]:";
                        for (int i = 6; i < guriPacket.Length; i++)
                        {
                            message += guriPacket[i] + " ";
                        }
                        message.Trim();

                        Session.Character.InventoryList.RemoveItemAmount(speakerVNum, 1);
                        ServerManager.Instance.Broadcast(Session.Character.GenerateSay(message, 13));
                    }
                }
            }
            else if (guriPacket[2] == "203" && guriPacket[3] == "0")
            {
                // SP points initialization
                int[] listPotionResetVNums = new int[3] { 1366, 1427, 5115 };
                int vnumToUse = -1;
                foreach (int vnum in listPotionResetVNums)
                {
                    if (Session.Character.InventoryList.CountItem(vnum) > 0)
                    {
                        vnumToUse = vnum;
                    }
                }
                if (vnumToUse != -1)
                {
                    if (Session.Character.UseSp)
                    {
                        SpecialistInstance specialistInstance = Session.Character.EquipmentList.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Equipment);
                        if (specialistInstance != null)
                        {
                            specialistInstance.SlDamage = 0;
                            specialistInstance.SlDefence = 0;
                            specialistInstance.SlElement = 0;
                            specialistInstance.SlHP = 0;

                            specialistInstance.DamageMinimum = 0;
                            specialistInstance.DamageMaximum = 0;
                            specialistInstance.HitRate = 0;
                            specialistInstance.CriticalLuckRate = 0;
                            specialistInstance.CriticalRate = 0;
                            specialistInstance.DefenceDodge = 0;
                            specialistInstance.DistanceDefenceDodge = 0;
                            specialistInstance.ElementRate = 0;
                            specialistInstance.DarkResistance = 0;
                            specialistInstance.LightResistance = 0;
                            specialistInstance.FireResistance = 0;
                            specialistInstance.WaterResistance = 0;
                            specialistInstance.CriticalDodge = 0;
                            specialistInstance.MagicDefence = 0;
                            specialistInstance.HP = 0;
                            specialistInstance.MP = 0;

                            Session.Character.InventoryList.RemoveItemAmount(vnumToUse, 1);
                            Session.Character.EquipmentList.DeleteFromSlotAndType((byte)EquipmentType.Sp, InventoryType.Equipment);
                            Session.Character.EquipmentList.AddToInventoryWithSlotAndType(specialistInstance, InventoryType.Equipment, (byte)EquipmentType.Sp);
                            Session.SendPacket(Session.Character.GenerateCond());
                            Session.SendPacket(Session.Character.GenerateSlInfo(specialistInstance, 2));
                            Session.SendPacket(Session.Character.GenerateLev());
                            Session.SendPacket(Session.Character.GenerateStatChar());
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_RESET"), 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORMATION_NEEDED"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POINTS"), 10));
                }
            }
        }

        [Packet("#guri")]
        public void GuriAnswer(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            switch (packetsplit[2])
            {
                case "400":
                    if (packetsplit.Length > 3)
                    {
                        short MapNpcId = -1;
                        if (!short.TryParse(packetsplit[3], out MapNpcId))
                        {
                            return;
                        }
                        MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.MapNpcId.Equals(MapNpcId));
                        NpcMonster mapobject = ServerManager.GetNpc(npc.NpcVNum);

                        int RateDrop = ServerManager.DropRate;
                        if (Session.Character.LastMapObject.AddSeconds(6) < DateTime.Now)
                        {
                            if (mapobject.Drops.Any(s => s.MonsterVNum != null))
                            {
                                if (mapobject.VNumRequired > 10 && Session.Character.InventoryList.CountItem(mapobject.VNumRequired) < mapobject.AmountRequired)
                                {
                                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEM"), 0));
                                    return;
                                }
                            }
                            Random random = new Random();
                            double randomAmount = random.Next(0, 100) * random.NextDouble();
                            int dropChance = mapobject.Drops.FirstOrDefault(s => s.MonsterVNum == npc.NpcVNum).DropChance;
                            if (randomAmount <= ((double)dropChance * RateDrop) / 5000.000)
                            {
                                short vnum = mapobject.Drops.FirstOrDefault(s => s.MonsterVNum == npc.NpcVNum).ItemVNum;
                                Session.Character.InventoryList.AddNewItemToInventory(vnum);
                                Session.Character.LastMapObject = DateTime.Now;
                                Session.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), ServerManager.GetItem(vnum).Name), 0));
                                Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), ServerManager.GetItem(vnum).Name), 11));
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("TRY_FAILED"), 0));
                            }
                        }
                        else
                        {
                            // make it like official, more than 6 seconds propably multiplied by
                            // amount of tries
                            Session.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("TRY_FAILED_WAIT"), (int)(Session.Character.LastMapObject.AddSeconds(6) - DateTime.Now).TotalSeconds), 0));
                        }
                    }
                    break;

                case "710":
                    if (packetsplit.Length > 5)
                    {
                        // MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(n =>
                        // n.MapNpcId.Equals(Convert.ToInt16(packetsplit[5]))); NpcMonster mapObject
                        // = ServerManager.GetNpc(npc.NpcVNum); teleport free
                    }
                    break;
            }
        }

        [Packet("hero")]
        public void Hero(string packet)
        {
            if (DAOFactory.CharacterDAO.IsReputHero(Session.Character.CharacterId) >= 3)
            {
                string[] packetsplit = packet.Split(' ');
                string message = String.Empty;
                for (int i = 2; i < packetsplit.Length; i++)
                {
                    message += packetsplit[i] + " ";
                }
                message.Trim();

                Session.CurrentMap?.Broadcast(Session, $"msg 5 [{Session.Character.Name}]:{message}", ReceiverType.AllNoHeroBlocked);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_HERO"), 11));
            }
        }

        [Packet("gop")]
        public void Option(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 4)
            {
                int TypeOption = -1;
                int OptionValue = -1;

                if (int.TryParse(packetsplit[2], out TypeOption) && int.TryParse(packetsplit[3], out OptionValue))
                {
                    switch (TypeOption)
                    {
                        case (int)ConfigType.BuffBlocked:
                            Session.Character.BuffBlocked = OptionValue == 1;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.BuffBlocked ? "BUFF_BLOCKED" : "BUFF_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.EmoticonsBlocked:
                            Session.Character.EmoticonsBlocked = OptionValue == 1;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.EmoticonsBlocked ? "EMO_BLOCKED" : "EMO_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.ExchangeBlocked:
                            Session.Character.ExchangeBlocked = OptionValue == 0;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.ExchangeBlocked ? "EXCHANGE_BLOCKED" : "EXCHANGE_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.FriendRequestBlocked:
                            Session.Character.FriendRequestBlocked = OptionValue == 0;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.FriendRequestBlocked ? "FRIEND_REQ_BLOCKED" : "FRIEND_REQ_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.GroupRequestBlocked:
                            Session.Character.GroupRequestBlocked = OptionValue == 0;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.GroupRequestBlocked ? "GROUP_REQ_BLOCKED" : "GROUP_REQ_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.HeroChatBlocked:
                            Session.Character.HeroChatBlocked = OptionValue == 1;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.HeroChatBlocked ? "HERO_CHAT_BLOCKED" : "HERO_CHAT_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.HpBlocked:
                            Session.Character.HpBlocked = OptionValue == 1;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.HpBlocked ? "HP_BLOCKED" : "HP_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.MinilandInviteBlocked:
                            Session.Character.MinilandInviteBlocked = OptionValue == 1;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.MinilandInviteBlocked ? "MINI_INV_BLOCKED" : "MINI_INV_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.MouseAimLock:
                            Session.Character.MouseAimLock = OptionValue == 1;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.MouseAimLock ? "MOUSE_LOCKED" : "MOUSE_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.QuickGetUp:
                            Session.Character.QuickGetUp = OptionValue == 1;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.QuickGetUp ? "QUICK_GET_UP_ENABLED" : "QUICK_GET_UP_DISABLED"), 0));
                            break;

                        case (int)ConfigType.WhisperBlocked:
                            Session.Character.WhisperBlocked = OptionValue == 0;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.WhisperBlocked ? "WHISPER_BLOCKED" : "WHISPER_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.FamilyRequestBlocked:
                            Session.Character.FamilyRequestBlocked = OptionValue == 0;
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.FamilyRequestBlocked ? "FAMILY_REQ_LOCKED" : "FAMILY_REQ_UNLOCKED"), 0));
                            break;

                        case (int)ConfigType.GroupSharing:
                            Group grp = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup(Session.Character.CharacterId));
                            if (grp == null)
                            {
                                return;
                            }
                            if (grp.Characters.ElementAt(0) != Session)
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_MASTER"), 0));
                                return;
                            }
                            if (OptionValue == 0)
                            {
                                ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(Session.Character.CharacterId)).SharingMode = 1;
                                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING"), 0), ReceiverType.Group);
                            }
                            else
                            {
                                ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(Session.Character.CharacterId)).SharingMode = 0;
                                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING_BY_ORDER"), 0), ReceiverType.Group);
                            }
                            break;
                    }
                }
            }
            Session.SendPacket(Session.Character.GenerateStat());
        }

        [Packet("pjoin")]
        public void PlayerJoin(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                string charName;
                long charId = -1;
                bool grouped1 = false, grouped2 = false, isBlocked = false;
                if (!long.TryParse(packetsplit[3], out charId))
                {
                    return;
                }
                foreach (Group group in ServerManager.Instance.Groups)
                {
                    if ((group.IsMemberOfGroup(charId) || group.IsMemberOfGroup(Session.Character.CharacterId)) && group.CharacterCount == 3)
                    {
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                        return;
                    }
                    if (group.IsMemberOfGroup(charId))
                    {
                        grouped1 = true;
                    }
                    if (group.IsMemberOfGroup(Session.Character.CharacterId))
                    {
                        grouped2 = true;
                    }
                }

                if (grouped1 && grouped2)
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_GROUP")));
                    return;
                }
                if (Convert.ToInt32(packetsplit[2]) == 0 || Convert.ToInt32(packetsplit[2]) == 1)
                {
                    if (Session.Character.CharacterId != charId)
                    {
                        if (!long.TryParse(packetsplit[3], out charId))
                        {
                            return;
                        }
                        isBlocked = ServerManager.Instance.GetProperty<bool>(charId, nameof(Character.GroupRequestBlocked));
                        if (isBlocked)
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_BLOCKED"), 0));
                        }
                        else
                        {
                            charName = ServerManager.Instance.GetProperty<string>(charId, nameof(Character.Name));
                            Session.SendPacket(Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("GROUP_REQUEST"), charName)));
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateDialog($"#pjoin^3^{ Session.Character.CharacterId} #pjoin^4^{Session.Character.CharacterId} {String.Format(Language.Instance.GetMessageFromKey("INVITED_YOU"), Session.Character.Name)}"), ReceiverType.OnlySomeone, charName);
                        }
                    }
                }
            }
        }

        [Packet("pleave")]
        public void PlayerLeave(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            ServerManager.Instance.GroupLeave(Session);
        }

        [Packet("preq")]
        public void Preq(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
            double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
            if (!(timeSpanSinceLastPortal >= 4))
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                return;
            }
            foreach (PortalDTO portal in Session.CurrentMap.Portals)
            {
                if (Session.Character.MapY >= portal.SourceY - 1 && Session.Character.MapY <= portal.SourceY + 1
                    && Session.Character.MapX >= portal.SourceX - 1 && Session.Character.MapX <= portal.SourceX + 1)
                {
                    switch (portal.Type)
                    {
                        case (sbyte)PortalType.MapPortal:
                        case (sbyte)PortalType.TSNormal:
                        case (sbyte)PortalType.Open:
                        case (sbyte)PortalType.Miniland:
                        case (sbyte)PortalType.TSEnd:
                        case (sbyte)PortalType.End:
                        case (sbyte)PortalType.Effect:
                        case (sbyte)PortalType.ShopTeleport:
                            break;

                        default:
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                            return;
                    }
                    ServerManager.Instance.MapOut(Session.Character.CharacterId);

                    Session.Character.MapId = portal.DestinationMapId;
                    Session.Character.MapX = portal.DestinationX;
                    Session.Character.MapY = portal.DestinationY;
                    Session.Character.LastPortal = currentRunningSeconds;

                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                    break;
                }
            }
        }

        [Packet("pulse")]
        public void Pulse(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            Session.Character.LastPulse += 60;
            if (Convert.ToInt32(packetsplit[2]) != Session.Character.LastPulse)
            {
                Session.Disconnect();
            }
            Session.Character.DeleteTimeout();
        }

        [Packet("req_info")]
        public void ReqInfo(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2] == "5")
            {
                short VNumNpc = -1;
                if (short.TryParse(packetsplit[3], out VNumNpc))
                {
                    NpcMonster npc = ServerManager.GetNpc(VNumNpc);
                    if (npc != null)
                    {
                        Session.SendPacket(npc.GenerateEInfo());
                    }
                }
            }
            else
            {
                ServerManager.Instance.RequireBroadcastFromUser(Session, Convert.ToInt64(packetsplit[3]), "GenerateReqInfo");
            }
        }

        [Packet("rest")]
        public void Rest(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.Rest();
        }

        [Packet("#revival")]
        public void Revive(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            byte type;
            if (packetsplit.Length > 2)
            {
                if (!byte.TryParse(packetsplit[2], out type))
                {
                    return;
                }
                if (Session.Character.Hp > 0)
                {
                    return;
                }
                switch (type)
                {
                    case 0:
                        int seed = 1012;
                        if (Session.Character.InventoryList.CountItem(seed) < 10 && Session.Character.Level > 20)
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POWER_SEED"), 0));
                            ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_SEED_SAY"), 0));
                        }
                        else
                        {
                            if (Session.Character.Level > 20)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("SEED_USED"), 10), 10));
                                Session.Character.InventoryList.RemoveItemAmount(seed, 10);
                                Session.Character.Hp = (int)(Session.Character.HPLoad() / 2);
                                Session.Character.Mp = (int)(Session.Character.MPLoad() / 2);
                            }
                            else
                            {
                                Session.Character.Hp = (int)Session.Character.HPLoad();
                                Session.Character.Mp = (int)Session.Character.MPLoad();
                            }
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateTp(), ReceiverType.All);
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateRevive(), ReceiverType.All);
                            Session.SendPacket(Session.Character.GenerateStat());
                        }
                        break;

                    case 1:
                        ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                        break;
                }
            }
        }

        [Packet("say")]
        public void Say(string packet)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 2; i < packetsplit.Length; i++)
            {
                message += packetsplit[i] + " ";
            }
            if (Session.Character.IsMuted())
            {
                if (Session.Character.Gender == 1)
                {
                    ServerManager.Instance.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
                else
                {
                    ServerManager.Instance.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
            }
            else
            {
                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateSay(message.Trim(), 0), ReceiverType.AllExceptMe);
            }
        }

        [Packet("pst")]
        public void SendMail(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            switch (packetsplit.Count())
            {
                case 10:
                    CharacterDTO Receiver = DAOFactory.CharacterDAO.LoadByName(packetsplit[7]);
                    if (Receiver != null)
                    {
                        WearableInstance headWearable = Session.Character.EquipmentList.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Equipment);
                        byte color = (headWearable != null && headWearable.Item.IsColored) ? headWearable.Design : Session.Character.HairColor;
                        MailDTO mailcopy = new MailDTO()
                        {
                            Amount = 0,
                            IsOpened = false,
                            Date = DateTime.Now,
                            Title = packetsplit[8],
                            Message = packetsplit[9],
                            ReceiverId = Receiver.CharacterId,
                            SenderId = Session.Character.CharacterId,
                            IsSenderCopy = true,
                            SenderClass = Session.Character.Class,
                            SenderGender = Session.Character.Gender,
                            SenderHairColor = color,
                            SenderHairStyle = Session.Character.HairStyle,
                            EqPacket = Session.Character.GenerateEqListForPacket(),
                            SenderMorphId = Session.Character.Morph == 0 ? (short)-1 : (short)((Session.Character.Morph > short.MaxValue) ? 0 : Session.Character.Morph)
                        };
                        MailDTO mail = new MailDTO()
                        {
                            Amount = 0,
                            IsOpened = false,
                            Date = DateTime.Now,
                            Title = packetsplit[8],
                            Message = packetsplit[9],
                            ReceiverId = Receiver.CharacterId,
                            SenderId = Session.Character.CharacterId,
                            IsSenderCopy = false,
                            SenderClass = Session.Character.Class,
                            SenderGender = Session.Character.Gender,
                            SenderHairColor = color,
                            SenderHairStyle = Session.Character.HairStyle,
                            EqPacket = Session.Character.GenerateEqListForPacket(),
                            SenderMorphId = Session.Character.Morph == 0 ? (short)-1 : (short)((Session.Character.Morph > short.MaxValue) ? 0 : Session.Character.Morph)
                        };
                        if (mailcopy.SenderId != mail.SenderId)
                        {
                            DAOFactory.MailDAO.InsertOrUpdate(ref mailcopy);
                        }
                        DAOFactory.MailDAO.InsertOrUpdate(ref mail);

                        Session.Character.MailList.Add((Session.Character.MailList.Any() ? Session.Character.MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mailcopy);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAILED"), 11));
                        Session.SendPacket(Session.Character.GeneratePost(mailcopy, 2));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                    break;

                case 5:
                    int id;
                    byte type;
                    if (int.TryParse(packetsplit[4], out id) && byte.TryParse(packetsplit[3], out type))
                    {
                        if (packetsplit[2] == "3")
                        {
                            if (Session.Character.MailList.ContainsKey(id))
                            {
                                if (!Session.Character.MailList[id].IsOpened)
                                {
                                    Session.Character.MailList[id].IsOpened = true;
                                    MailDTO mailupdate = Session.Character.MailList[id];
                                    DAOFactory.MailDAO.InsertOrUpdate(ref mailupdate);
                                }
                                Session.SendPacket(Session.Character.GeneratePostMessage(Session.Character.MailList[id], type));
                            }
                        }
                        else if (packetsplit[2] == "2")
                        {
                            if (Session.Character.MailList.ContainsKey(id))
                            {
                                MailDTO mail = Session.Character.MailList[id];
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAIL_DELETED"), 11));
                                Session.SendPacket($"post 2 {type} {id}");
                                if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                                {
                                    DAOFactory.MailDAO.DeleteById(mail.MailId);
                                }
                                if (Session.Character.MailList.ContainsKey(id))
                                {
                                    Session.Character.MailList.Remove(id);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        [Packet("qset")]
        public void SetQuicklist(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 4)
            {
                short type, q1, q2, data1 = 0, data2 = 0;
                if (!short.TryParse(packetsplit[2], out type) || !short.TryParse(packetsplit[3], out q1) || !short.TryParse(packetsplit[4], out q2))
                {
                    return;
                }
                if (packetsplit.Length > 6)
                {
                    short.TryParse(packetsplit[5], out data1);
                    short.TryParse(packetsplit[6], out data2);
                }
                switch (type)
                {
                    case 0:
                    case 1:

                        // client says qset 0 1 3 2 6 answer -> qset 1 3 0.2.6.0
                        Session.Character.QuicklistEntries.RemoveAll(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                        Session.Character.QuicklistEntries.Add(new QuicklistEntryDTO
                        {
                            CharacterId = Session.Character.CharacterId,
                            Type = type,
                            Q1 = q1,
                            Q2 = q2,
                            Slot = data1,
                            Pos = data2,
                            Morph = Session.Character.UseSp ? (short)Session.Character.Morph : (short)0
                        });

                        Session.SendPacket($"qset {q1} {q2} {type}.{data1}.{data2}.0");
                        break;

                    case 2:

                        // DragDrop / Reorder qset type to1 to2 from1 from2 vars -> q1 q2 data1 data2
                        QuicklistEntryDTO qlFrom = Session.Character.QuicklistEntries.Single(n => n.Q1 == data1 && n.Q2 == data2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));
                        QuicklistEntryDTO qlTo = Session.Character.QuicklistEntries.SingleOrDefault(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                        qlFrom.Q1 = q1;
                        qlFrom.Q2 = q2;

                        if (qlTo == null)
                        {
                            // Put 'from' to new position (datax)
                            Session.SendPacket($"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                            // old 'from' is now empty.
                            Session.SendPacket($"qset {data1} {data2} 7.7.-1.0");
                        }
                        else
                        {
                            // Put 'from' to new position (datax)
                            Session.SendPacket($"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                            // 'from' is now 'to' because they exchanged
                            qlTo.Q1 = data1;
                            qlTo.Q2 = data2;
                            Session.SendPacket($"qset {qlTo.Q1} {qlTo.Q2} {qlTo.Type}.{qlTo.Slot}.{qlTo.Pos}.0");
                        }
                        break;

                    case 3:

                        // Remove from Quicklist
                        Session.Character.QuicklistEntries.RemoveAll(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));
                        Session.SendPacket($"qset {q1} {q2} 7.7.-1.0");
                        break;

                    default:
                        return;
                }
            }
        }

        [Packet("game_start")]
        public void StartGame(string packet)
        {
            if (Session.IsOnMap || !Session.HasSelectedCharacter) //character should have been selected in SelectCharacter
            {
                return;
            }

            Session.CurrentMap = ServerManager.GetMap(Session.Character.MapId);
            if (System.Configuration.ConfigurationManager.AppSettings["SceneOnCreate"].ToLower() == "true" & DAOFactory.GeneralLogDAO.LoadByLogType("Connection", Session.Character.CharacterId).Count() == 1)
            {
                Session.SendPacket("scene 40");
            }
            if (System.Configuration.ConfigurationManager.AppSettings["WorldInformation"].ToLower() == "true")
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                string productVersion = assembly != null ? FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion : "1337";
                Session.SendPacket(Session.Character.GenerateSay("----------[World Information]----------", 10));
                Session.SendPacket(Session.Character.GenerateSay($"OpenNos by OpenNos Team\nVersion : v{productVersion}", 11));
                Session.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));
            }
            Session.Character.LoadSpeed();
            Session.Character.LoadSkills();
            Session.SendPacket(Session.Character.GenerateTit());
            Session.SendPacket($"rsfi 1 1 0 9 0 9");
            if (Session.Character.Hp <= 0)
            {
                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
            }
            else
            {
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
            }
            Session.SendPacket(Session.Character.GenerateSki());
            Session.SendPacket($"fd {Session.Character.Reput} 0 {(int)Session.Character.Dignity} {Math.Abs(Session.Character.GetDignityIco())}");
            Session.SendPacket(Session.Character.GenerateFd());
            Session.SendPacket("rage 0 250000");
            Session.SendPacket("rank_cool 0 0 18000");
            Session.SendPacket(Session.Character.GenerateSpPoint());
            Session.SendPacket("scr 0 0 0 0 0 0");
            for (int i = 0; i < 10; i++)
            {
                Session.SendPacket($"bn {i} {Language.Instance.GetMessageFromKey($"BN{i}")}");
            }
            Session.SendPacket(Session.Character.GenerateExts());
            Session.SendPacket($"mlinfo 3800 2000 100 0 0 10 0 {Language.Instance.GetMessageFromKey("WELCOME_MUSIC_INFO")} {Language.Instance.GetMessageFromKey("MINILAND_WELCOME_MESSAGE")}"); // 0 before 10 = visitors
            Session.SendPacket("p_clear");

            // sc_p pet sc_n nospartner Session.SendPacket("sc_p_stc 0"); // end pet and partner
            Session.SendPacket("pinit 0"); // clean party list
            Session.Character.DeleteTimeout();

            // blinit
            Session.SendPacket("zzim");
            Session.SendPacket($"twk 2 {Session.Character.CharacterId} {Session.Account.Name} {Session.Character.Name} shtmxpdlfeoqkr");

            // qstlist target sqst bf
            Session.SendPacket("act6");
            Session.SendPacket(Session.Character.GenerateFaction());

            // sc_p pet again sc_n nospartner again
            Session.Character.GenerateStartupInventory();

            // mlobjlst - miniland object list
            Session.SendPacket(Session.Character.GenerateGold());
            Session.SendPackets(Session.Character.GenerateQuicklist());

            // string finit = "finit"; string blinit = "blinit";
            string clinit = "clinit";
            string flinit = "flinit";
            string kdlinit = "kdlinit";
            foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopComplimented())
            {
                clinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Compliment}|{character.Name}";
            }
            foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopReputation())
            {
                flinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Reput}|{character.Name}";
            }
            foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopPoints())
            {
                kdlinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Act4Points}|{character.Name}";
            }

            // finit - friend list
            Session.SendPacket(clinit);
            Session.SendPacket(flinit);
            Session.SendPacket(kdlinit);

            // finfo - friends info
            Session.SendPacket("p_clear");
            Session.Character.RefreshMail();
        }

        [Packet("#pjoin")]
        public void ValidPJoin(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ', '^');
            int type = -1, newgroup = 1;
            long charId = -1;
            bool blocked1 = false, blocked2 = false;
            if (packetsplit.Length > 3)
            {
                if (!(int.TryParse(packetsplit[2], out type) && long.TryParse(packetsplit[3], out charId)))
                {
                    return;
                }
                if (type == 3 && ServerManager.Instance.GetProperty<string>(charId, nameof(Character.Name)) != null)
                {
                    foreach (Group group in ServerManager.Instance.Groups)
                    {
                        if (group.IsMemberOfGroup(Session))
                        {
                            blocked1 = true;
                        }
                        if (group.IsMemberOfGroup(charId))
                        {
                            blocked2 = true;
                        }
                    }
                    foreach (Group group in ServerManager.Instance.Groups)
                    {
                        if (group.CharacterCount == 3)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));

                            return;
                        }
                        else if (blocked2 && blocked1)
                        {
                            return;
                        }
                        else if (group.IsMemberOfGroup(charId))
                        {
                            group.JoinGroup(Session);
                            newgroup = 0;
                        }
                        else if (group.IsMemberOfGroup(Session.Character.CharacterId))
                        {
                            group.JoinGroup(charId);
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOINED_GROUP"), 10), ReceiverType.OnlySomeone, String.Empty, charId);
                            newgroup = 0;
                        }
                    }

                    if (newgroup == 1)
                    {
                        Group group = new Group();
                        group.JoinGroup(charId);
                        Session.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("GROUP_JOIN"), ServerManager.Instance.GetProperty<string>(charId, nameof(Character.Name))), 10));
                        group.JoinGroup(Session.Character.CharacterId);
                        ServerManager.Instance.AddGroup(group);
                        Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_ADMIN")), ReceiverType.OnlySomeone, String.Empty, charId);

                        // set back reference to group
                        Session.Character.Group = group;
                        ClientSession session = ServerManager.Instance.GetSessionByCharacterId(charId);
                        if(session != null && session.Character != null)
                        {
                            ClientSession loadedSession = ServerManager.Instance.GetSessionByCharacterId(charId);

                            if(loadedSession != null)
                            {
                                loadedSession.Character.Group = group;
                            }
                        }
                    }

                    // player join group
                    ServerManager.Instance.UpdateGroup(charId);
                    Session.CurrentMap?.Broadcast(Session.Character.GeneratePidx());
                }
                else if (type == 4)
                {
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("REFUSED_GROUP_REQUEST"), Session.Character.Name), 10), ReceiverType.OnlySomeone, String.Empty, charId);
                }
            }
        }

        [Packet("walk")]
        public void Walk(string packet)
        {
            WalkPacket walkPacket = PacketFactory.Serialize<WalkPacket>(packet, true);

            if (walkPacket != null)
            {
                double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
                int distance = Map.GetDistance(new MapCell() { X = Session.Character.MapX, Y = Session.Character.MapY }, new MapCell() { X = walkPacket.XCoordinate, Y = walkPacket.YCoordinate });

                if (Session.Character.Speed >= walkPacket.Speed && !(distance > 60 && timeSpanSinceLastPortal > 5))
                {
                    Session.Character.MapX = walkPacket.XCoordinate;
                    Session.Character.MapY = walkPacket.YCoordinate;
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateMv());
                    Session.SendPacket(Session.Character.GenerateCond());
                    Session.Character.LastMove = DateTime.Now;
                }
                else
                {
                    Session.Disconnect();
                }
            }
        }

        [Packet("/")]
        public void Whisper(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 2; i < packetsplit.Length; i++)
            {
                message += packetsplit[i] + " ";
            }
            message.Trim();

            Session.SendPacket(Session.Character.GenerateSpk(message, 5));

            bool? whisperBlocked = ServerManager.Instance.GetProperty<bool?>(packetsplit[1].Substring(1), nameof(Character.WhisperBlocked));
            if (whisperBlocked.HasValue)
            {
                if (!whisperBlocked.Value)
                {
                    ServerManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 5), ReceiverType.OnlySomeone, packetsplit[1].Substring(1));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_WHISPER_BLOCKED"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")));
            }
        }

        #endregion
    }
}