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

        public void CharacterOptionChange(CharacterOptionPacket characteroptionpacket)
        {
            switch (characteroptionpacket.Option)
            {
                case CharacterOption.BuffBlocked:
                    Session.Character.BuffBlocked = characteroptionpacket.IsActive == true;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.BuffBlocked ? "BUFF_BLOCKED" : "BUFF_UNLOCKED"), 0));
                    break;

                case CharacterOption.EmoticonsBlocked:
                    Session.Character.EmoticonsBlocked = characteroptionpacket.IsActive == true;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.EmoticonsBlocked ? "EMO_BLOCKED" : "EMO_UNLOCKED"), 0));
                    break;

                case CharacterOption.ExchangeBlocked:
                    Session.Character.ExchangeBlocked = characteroptionpacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.ExchangeBlocked ? "EXCHANGE_BLOCKED" : "EXCHANGE_UNLOCKED"), 0));
                    break;

                case CharacterOption.FriendRequestBlocked:
                    Session.Character.FriendRequestBlocked = characteroptionpacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.FriendRequestBlocked ? "FRIEND_REQ_BLOCKED" : "FRIEND_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupRequestBlocked:
                    Session.Character.GroupRequestBlocked = characteroptionpacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.GroupRequestBlocked ? "GROUP_REQ_BLOCKED" : "GROUP_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.HeroChatBlocked:
                    Session.Character.HeroChatBlocked = characteroptionpacket.IsActive == true;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.HeroChatBlocked ? "HERO_CHAT_BLOCKED" : "HERO_CHAT_UNLOCKED"), 0));
                    break;

                case CharacterOption.HpBlocked:
                    Session.Character.HpBlocked = characteroptionpacket.IsActive == true;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.HpBlocked ? "HP_BLOCKED" : "HP_UNLOCKED"), 0));
                    break;

                case CharacterOption.MinilandInviteBlocked:
                    Session.Character.MinilandInviteBlocked = characteroptionpacket.IsActive == true;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.MinilandInviteBlocked ? "MINI_INV_BLOCKED" : "MINI_INV_UNLOCKED"), 0));
                    break;

                case CharacterOption.MouseAimLock:
                    Session.Character.MouseAimLock = characteroptionpacket.IsActive == true;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.MouseAimLock ? "MOUSE_LOCKED" : "MOUSE_UNLOCKED"), 0));
                    break;

                case CharacterOption.QuickGetUp:
                    Session.Character.QuickGetUp = characteroptionpacket.IsActive == true;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.QuickGetUp ? "QUICK_GET_UP_ENABLED" : "QUICK_GET_UP_DISABLED"), 0));
                    break;

                case CharacterOption.WhisperBlocked:
                    Session.Character.WhisperBlocked = characteroptionpacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.WhisperBlocked ? "WHISPER_BLOCKED" : "WHISPER_UNLOCKED"), 0));
                    break;

                case CharacterOption.FamilyRequestBlocked:
                    Session.Character.FamilyRequestBlocked = characteroptionpacket.IsActive == false;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.FamilyRequestBlocked ? "FAMILY_REQ_LOCKED" : "FAMILY_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupSharing:
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
                    if (characteroptionpacket.IsActive == false)
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
            Session.SendPacket(Session.Character.GenerateStat());
        }

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
                        if (Session.Character.Inventory.CanAddItem((short)mail.AttachmentVNum))
                        {
                            ItemInstance newInv = Session.Character.Inventory.AddNewToInventory((short)mail.AttachmentVNum, mail.AttachmentAmount);

                            if (newInv != null)
                            {
                                newInv.Upgrade = mail.AttachmentUpgrade;
                                newInv.Rare = (sbyte)mail.AttachmentRarity;
                                if (newInv.Rare != 0)
                                {
                                    (newInv as WearableInstance).SetRarityPoint();
                                }
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                                Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_GIFTED")}: {newInv.Item.Name} x {mail.AttachmentAmount}", 12));

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

        public void GroupJoin(PJoinPacket pjoinPacket)
        {
            Logger.Debug("Joining group", Session.SessionId);

            if (pjoinPacket.RequestType.Equals(GroupRequestType.Requested) || pjoinPacket.RequestType.Equals(GroupRequestType.Invited))
            {
                if (pjoinPacket.CharacterId == 0)
                {
                    return;
                }

                if (ServerManager.Instance.IsCharactersGroupFull(pjoinPacket.CharacterId))
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                    return;
                }

                if (ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId) &&
                    ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId))
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_GROUP")));
                    return;
                }

                if (Session.Character.CharacterId != pjoinPacket.CharacterId)
                {
                    ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(pjoinPacket.CharacterId);
                    if (targetSession != null)
                    {
                        if (targetSession.Character.GroupRequestBlocked)
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_BLOCKED"), 0));
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("GROUP_REQUEST"), targetSession.Character.Name)));
                            Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateDialog($"#pjoin^3^{ Session.Character.CharacterId} #pjoin^4^{Session.Character.CharacterId} {String.Format(Language.Instance.GetMessageFromKey("INVITED_YOU"), Session.Character.Name)}"), ReceiverType.OnlySomeone, targetSession.Character.Name);
                        }
                    }
                }
            }
        }

        [Packet("#pjoin")]
        public void GroupJoinValid(string packet)
        {
            Logger.Debug(packet, Session.SessionId);

            // serialization hack -> dialog answer packet isnt supported by PacketFactory atm
            PJoinPacket pjoinPacket = PacketFactory.Deserialize<PJoinPacket>(packet.Replace('^', ' ').Replace('#', ' '), true);
            bool createNewGroup = true;

            if (pjoinPacket != null)
            {
                if (pjoinPacket.CharacterId == 0)
                {
                    return;
                }

                ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(pjoinPacket.CharacterId);

                if (targetSession == null)
                {
                    // target session with character id does not exist
                    return;
                }

                // accepted, join the group
                if (pjoinPacket.RequestType.Equals(GroupRequestType.Accepted))
                {
                    if (ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId) &&
                        ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId))
                    {
                        // everyone is in group, return
                        return;
                    }

                    if (ServerManager.Instance.IsCharactersGroupFull(pjoinPacket.CharacterId)
                        || ServerManager.Instance.IsCharactersGroupFull(Session.Character.CharacterId))
                    {
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                        targetSession.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                        return;
                    }

                    // get group and add to group
                    if (ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId))
                    {
                        // target joins source
                        Group currentGroup = ServerManager.Instance.GetGroupByCharacterId(Session.Character.CharacterId);

                        if (currentGroup != null)
                        {
                            currentGroup.JoinGroup(targetSession);
                            targetSession.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOINED_GROUP"), 10));
                            createNewGroup = false;
                        }
                    }
                    else if (ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId))
                    {
                        // source joins target
                        Group currentGroup = ServerManager.Instance.GetGroupByCharacterId(pjoinPacket.CharacterId);

                        if (currentGroup != null)
                        {
                            currentGroup.JoinGroup(Session);
                            createNewGroup = false;
                        }
                    }

                    if (createNewGroup)
                    {
                        Group group = new Group();
                        group.JoinGroup(pjoinPacket.CharacterId);
                        Session.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("GROUP_JOIN"), targetSession.Character.Name), 10));
                        group.JoinGroup(Session.Character.CharacterId);
                        ServerManager.Instance.AddGroup(group);
                        targetSession.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_ADMIN")));

                        // set back reference to group
                        Session.Character.Group = group;
                        targetSession.Character.Group = group;
                    }

                    // player join group
                    ServerManager.Instance.UpdateGroup(pjoinPacket.CharacterId);
                    Session.CurrentMap?.Broadcast(Session.Character.GeneratePidx());
                }
                else if (pjoinPacket.RequestType == GroupRequestType.Declined)
                {
                    targetSession.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("REFUSED_GROUP_REQUEST"), Session.Character.Name), 10));
                }
            }
        }

        [Packet("pleave")]
        public void GroupLeave(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            ServerManager.Instance.GroupLeave(Session);
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
            if (message.Length > 60)
            {
                message = message.Substring(0, 60);
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
                Session.CurrentMap?.Broadcast(Session.Character.GenerateGuri(2, 1), Session.Character.MapX, Session.Character.MapY);
            }
            else if (guriPacket[2] == "4")
            {
                int speakerVNum = 2173;

                // presentation message
                if (guriPacket[3] == "2")
                {
                    int presentationVNum = (Session.Character.Inventory.CountItem(1117) > 0 ? 1117 : (Session.Character.Inventory.CountItem(9013) > 0 ? 9013 : -1));
                    if (presentationVNum != -1)
                    {
                        string message = String.Empty;

                        // message = $" ";
                        for (int i = 6; i < guriPacket.Length; i++)
                        {
                            message += guriPacket[i] + "^";
                        }
                        message = message.Substring(0, message.Length - 1); // Remove the last ^
                        message.Trim();
                        if (message.Length > 60)
                        {
                            message = message.Substring(0, 60);
                        }

                        Session.Character.Biography = message;
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("INTRODUCTION_SET"), 10));
                        Session.Character.Inventory.RemoveItemAmount(presentationVNum);
                    }
                }

                // Speaker
                if (guriPacket[3] == "3")
                {
                    if (Session.Character.Inventory.CountItem(speakerVNum) > 0)
                    {
                        string message = String.Empty;
                        message = $"<{Language.Instance.GetMessageFromKey("SPEAKER")}> [{Session.Character.Name}]:";
                        for (int i = 6; i < guriPacket.Length; i++)
                        {
                            message += guriPacket[i] + " ";
                        }
                        if (message.Length > 120)
                        {
                            message = message.Substring(0, 120);
                        }

                        message.Trim();

                        if (Session.Character.IsMuted())
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SPEAKER_CANT_BE_USED"), 10));
                            return;
                        }
                        Session.Character.Inventory.RemoveItemAmount(speakerVNum);
                        ServerManager.Instance.Broadcast(Session.Character.GenerateSay(message, 13));
                    }
                }
            }
            else if (guriPacket[2] == "203" && guriPacket[3] == "0")
            {
                // SP points initialization
                int[] listPotionResetVNums = new int[4] { 1366, 1427, 5115, 9040 };
                int vnumToUse = -1;
                foreach (int vnum in listPotionResetVNums)
                {
                    if (Session.Character.Inventory.CountItem(vnum) > 0)
                    {
                        vnumToUse = vnum;
                    }
                }
                if (vnumToUse != -1)
                {
                    if (Session.Character.UseSp)
                    {
                        SpecialistInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
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

                            Session.Character.Inventory.RemoveItemAmount(vnumToUse);
                            Session.Character.Inventory.DeleteFromSlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                            Session.Character.Inventory.AddToInventoryWithSlotAndType(specialistInstance, InventoryType.Wear, (byte)EquipmentType.Sp);
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
                        int delay = (int)Math.Round((3 + mapobject.RespawnTime / 1000d) * Session.Character.TimesUsed);
                        delay = delay > 11 ? 8 : delay;
                        if (Session.Character.LastMapObject.AddSeconds(delay) < DateTime.Now)
                        {
                            if (mapobject.Drops.Any(s => s.MonsterVNum != null))
                            {
                                if (mapobject.VNumRequired > 10 && Session.Character.Inventory.CountItem(mapobject.VNumRequired) < mapobject.AmountRequired)
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
                                ItemInstance newInv = Session.Character.Inventory.AddNewToInventory(vnum);
                                Session.Character.LastMapObject = DateTime.Now;
                                Session.Character.TimesUsed++;
                                if (Session.Character.TimesUsed >= 4)
                                {
                                    Session.Character.TimesUsed = 0;
                                }
                                if (newInv != null)
                                {
                                    Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                                    Session.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), newInv.Item.Name), 0));
                                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), newInv.Item.Name), 11));
                                    return;
                                }
                                else
                                {
                                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                    return;
                                }
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
                            Session.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("TRY_FAILED_WAIT"), (int)(Session.Character.LastMapObject.AddSeconds(delay) - DateTime.Now).TotalSeconds), 0));
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
                message = message.Trim();

                ServerManager.Instance.Broadcast(Session, $"msg 5 [{Session.Character.Name}]:{message}", ReceiverType.AllNoHeroBlocked);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_HERO"), 11));
            }
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
                    ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                    Session.Character.LastPortal = currentRunningSeconds;
                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId, portal.DestinationMapId, portal.DestinationX, portal.DestinationY);
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
                        if (Session.Character.Inventory.CountItem(seed) < 10 && Session.Character.Level > 20)
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POWER_SEED"), 0));
                            ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_SEED_SAY"), 0));
                        }
                        else
                        {
                            if (Session.Character.Level > 20)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("SEED_USED"), 10), 10));
                                Session.Character.Inventory.RemoveItemAmount(seed, 10);
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
            if (message.Length > 60)
            {
                message = message.Substring(0, 60);
            }

            if (Session.Character.IsMuted())
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1), ReceiverType.All);
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
                else
                {
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1), ReceiverType.All);
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
                        WearableInstance headWearable = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);
                        byte color = (headWearable != null && headWearable.Item.IsColored) ? headWearable.Design : (byte)Session.Character.HairColor;
                        MailDTO mailcopy = new MailDTO()
                        {
                            AttachmentAmount = 0,
                            IsOpened = false,
                            Date = DateTime.Now,
                            Title = packetsplit[8],
                            Message = packetsplit[9],
                            ReceiverId = Receiver.CharacterId,
                            SenderId = Session.Character.CharacterId,
                            IsSenderCopy = true,
                            SenderClass = Session.Character.Class,
                            SenderGender = Session.Character.Gender,
                            SenderHairColor = Enum.IsDefined(typeof(HairColorType), (byte)color) ? (HairColorType)color : 0,
                            SenderHairStyle = Session.Character.HairStyle,
                            EqPacket = Session.Character.GenerateEqListForPacket(),
                            SenderMorphId = Session.Character.Morph == 0 ? (short)-1 : (short)((Session.Character.Morph > short.MaxValue) ? 0 : Session.Character.Morph)
                        };
                        MailDTO mail = new MailDTO()
                        {
                            AttachmentAmount = 0,
                            IsOpened = false,
                            Date = DateTime.Now,
                            Title = packetsplit[8],
                            Message = packetsplit[9],
                            ReceiverId = Receiver.CharacterId,
                            SenderId = Session.Character.CharacterId,
                            IsSenderCopy = false,
                            SenderClass = Session.Character.Class,
                            SenderGender = Session.Character.Gender,
                            SenderHairColor = Enum.IsDefined(typeof(HairColorType), (byte)color) ? (HairColorType)color : 0,
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
                        QuicklistEntryDTO qlFrom = Session.Character.QuicklistEntries.SingleOrDefault(n => n.Q1 == data1 && n.Q2 == data2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                        if (qlFrom != null)
                        {
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
            if (Session.IsOnMap || !Session.HasSelectedCharacter)
            {
                // character should have been selected in SelectCharacter
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
            SpecialistInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((short)8, InventoryType.Wear);
            if (specialistInstance != null)
            {
                Session.SendPacket(Session.Character.GenerateSpPoint());
            }
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

            // blinit
            Session.SendPacket("zzim");
            Session.SendPacket($"twk 2 {Session.Character.CharacterId} {Session.Account.Name} {Session.Character.Name} shtmxpdlfeoqkr");

            // qstlist target sqst bf
            Session.SendPacket("act6");
            Session.SendPacket(Session.Character.GenerateFaction());

            // sc_p pet again sc_n nospartner again
#pragma warning disable 618
            Session.Character.GenerateStartupInventory();
#pragma warning restore 618

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
            Session.Character.LoadSentMail();
            Session.Character.DeleteTimeout();
        }

        public void Walk(WalkPacket walkPacket)
        {
            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
            double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
            int distance = Map.GetDistance(new MapCell() { X = Session.Character.MapX, Y = Session.Character.MapY }, new MapCell() { X = walkPacket.XCoordinate, Y = walkPacket.YCoordinate });

            if (!Session.CurrentMap.IsBlockedZone(walkPacket.XCoordinate, walkPacket.YCoordinate) && !Session.Character.IsChangingMap)
            {
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

            ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterName(packetsplit[(packetsplit[1] == "/GM" ? 2 : 1)].Substring((packetsplit[1] == "/GM" ? 0 : 1)));
            if (targetSession == null)
            {
                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")));
                return;
            }

            if (packetsplit[1] == "/GM" && targetSession.Account.Authority != AuthorityType.Admin)
            {
                Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("USER_IS_NOT_AN_ADMIN"), targetSession.Character.Name), 10));
                return;
            }

            for (int i = (packetsplit[1] == "/GM" ? 3 : 2); i < packetsplit.Length; i++)
            {
                message += packetsplit[i] + " ";
            }
            if (message.Length > 60)
            {
                message = message.Substring(0, 60);
            }

            message.Trim();

            Session.SendPacket(Session.Character.GenerateSpk(message, 5));

            if (targetSession.Character.GmPvtBlock)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_CHAT_BLOCKED"), 10));
                return;
            }

            if (!targetSession.Character.WhisperBlocked)
            {
                ServerManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, (Session.Account.Authority == AuthorityType.Admin ? 15 : 5)), ReceiverType.OnlySomeone, packetsplit[(packetsplit[1] == "/GM" ? 2 : 1)].Substring((packetsplit[1] == "/GM" ? 0 : 1)));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_WHISPER_BLOCKED"), 0));
            }
        }
        #endregion
    }
}