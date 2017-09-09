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

using CloneExtensions;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.GameObject
{
    public class SpecialItem : Item
    {
        #region Instantiation

        public SpecialItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            inv.Item.BCards.ForEach(c => c.ApplyBCards(session.Character));
            
            switch (Effect)
            {
                case 0:
                    switch (VNum)
                    {
                        case 1428:
                            session.SendPacket("guri 18 1");
                            break;
                        case 1429:
                            session.SendPacket("guri 18 0");
                            break;
                        case 1430:
                            if (packetsplit == null)
                            {
                                return;
                            }

                            if (packetsplit.Length < 9)
                            {
                                // MODIFIED PACKET
                                return;
                            }

                            if (!short.TryParse(packetsplit[9], out short eqSlot) || !Enum.TryParse(packetsplit[8], out InventoryType eqType))
                            {
                                return;
                            }
                            WearableInstance eq = session.Character.Inventory.LoadBySlotAndType<WearableInstance>(eqSlot, eqType);
                            if (eq == null)
                            {
                                // PACKET MODIFIED
                                return;
                            }
                            if (eq.Item.ItemType != ItemType.Armor && eq.Item.ItemType != ItemType.Weapon)
                            {
                                return;
                            }
                            eq.EquipmentOptions.Clear();
                            eq.ShellRarity = null;
                            session.Character.Inventory.RemoveItemAmount(1430);
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHELL_ERASED"), 0));
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(17, 1, session.Character.CharacterId));
                            break;
                        case 5916:
                            session.Character.AddStaticBuff(new StaticBuffDTO
                            {
                                CardId = 340,
                                CharacterId = session.Character.CharacterId,
                                RemainingTime = 7200
                            });
                            session.Character.RemoveBuff(339);
                            session.Character.Inventory.RemoveItemAmount(5916);
                            break;
                        case 5929:
                            session.Character.AddStaticBuff(new StaticBuffDTO
                            {
                                CardId = 340,
                                CharacterId = session.Character.CharacterId,
                                RemainingTime = 600
                            });
                            session.Character.RemoveBuff(339);
                            session.Character.Inventory.RemoveItemAmount(5916);
                            break;
                        default:
                            IEnumerable<RollGeneratedItemDTO> roll = DAOFactory.RollGeneratedItemDAO.LoadByItemVNum(VNum);
                            IEnumerable<RollGeneratedItemDTO> rollGeneratedItemDtos = roll as IList<RollGeneratedItemDTO> ?? roll.ToList();
                            if (!rollGeneratedItemDtos.Any())
                            {
                                return;
                            }
                            int probabilities = rollGeneratedItemDtos.Sum(s => s.Probability);
                            int rnd = ServerManager.Instance.RandomNumber(0, probabilities);
                            int currentrnd = 0;
                            foreach (RollGeneratedItemDTO rollitem in rollGeneratedItemDtos)
                            {
                                if (rollitem.Probability == 10000)
                                {
                                    session.Character.GiftAdd(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount);
                                    continue;
                                }
                                currentrnd += rollitem.Probability;
                                if (currentrnd < rnd)
                                {
                                    continue;
                                }
                                if (rollitem.IsSuperReward)
                                {
                                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                                    {
                                        DestinationCharacterId = null,
                                        SourceCharacterId = session.Character.CharacterId,
                                        SourceWorldId = ServerManager.Instance.WorldId,
                                        Message = Language.Instance.GetMessageFromKey("SUPER_REWARD"),
                                        Type = MessageType.Shout
                                    });
                                }
                                session.SendPacket(session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.Instance.GetItem(rollitem.ItemGeneratedVNum)?.Name} x {rollitem.ItemGeneratedAmount}", 12));
                                session.Character.GiftAdd(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount);
                                break;
                            }
                            session.Character.Inventory.RemoveItemAmount(VNum);
                            break;
                    }
                    break;
                // sp point potions
                case 150:
                case 151:
                    session.Character.SpAdditionPoint += EffectValue;
                    if (session.Character.SpAdditionPoint > 1000000)
                    {
                        session.Character.SpAdditionPoint = 1000000;
                    }
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDED"), EffectValue), 0));
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.SendPacket(session.Character.GenerateSpPoint());
                    break;

                case 204:
                    session.Character.SpPoint += EffectValue;
                    session.Character.SpAdditionPoint += EffectValue * 3;
                    if (session.Character.SpAdditionPoint > 1000000)
                    {
                        session.Character.SpAdditionPoint = 1000000;
                    }
                    if (session.Character.SpPoint > 10000)
                    {
                        session.Character.SpPoint = 10000;
                    }
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDEDBOTH"), EffectValue, EffectValue * 3), 0));
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.SendPacket(session.Character.GenerateSpPoint());
                    break;

                case 250:
                    if (session.Character.Buff.Any(s => s.Card.CardId == 131))
                    {
                        //TODO ADD MESSAGE ALREADY GOT BUFF
                        return;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.Character.AddStaticBuff(new StaticBuffDTO { CardId = 131 });
                    session.CurrentMapInstance.Broadcast(session.Character.GeneratePairy());
                    session.SendPacket(session.Character.GeneratePairy());
                    break;

                case 208:
                    if (session.Character.Buff.Any(s => s.Card.CardId == 121))
                    {
                        //TODO ADD MESSAGE ALREADY GOT BUFF
                        return;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.Character.AddStaticBuff(new StaticBuffDTO {CardId = 121, CharacterId = session.Character.CharacterId, RemainingTime = 3600});
                    break;

                case 301:
                    if (ServerManager.Instance.IsCharacterMemberOfGroup(session.Character.CharacterId))
                    {
                        //TODO you are in group
                        return;
                    }
                    ItemInstance raidSeal = session.Character.Inventory.LoadBySlotAndType<ItemInstance>(inv.Slot, InventoryType.Main);

                    ScriptedInstance raid = ServerManager.Instance.Raids.FirstOrDefault(s => s.RequieredItems.Any(obj => obj.VNum == raidSeal.ItemVNum))?.GetClone();
                    if (raid != null)
                    {
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, raidSeal.Id);
                        Group group = new Group(GroupType.Team);
                        group.Raid = raid;
                        group.JoinGroup(session.Character.CharacterId);
                        ServerManager.Instance.AddGroup(group);
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("YOU_ARE_RAID_CHIEF"), session.Character.Name), 0));
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("YOU_ARE_RAID_CHIEF"), session.Character.Name), 10));
                        if (session.Character.Level > raid.LevelMaximum || session.Character.Level < raid.LevelMinimum)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RAID_LEVEL_INCORRECT"), 10));
                        }
                        session.SendPacket(session.Character.GenerateRaid(2, false));
                        session.SendPacket(session.Character.GenerateRaid(0, false));
                        session.SendPacket(session.Character.GenerateRaid(1, false));
                        session.SendPacket(group.GenerateRdlst());
                    }
                    break;

                case 305:
                    if (packetsplit == null || packetsplit.Length < 3)
                    {
                        return;
                    }
                    Mate mate = session.Character.Mates.FirstOrDefault(s => s.MateTransportId == int.Parse(packetsplit[3]));
                    if (mate != null && EffectValue == mate.NpcMonsterVNum && mate.Skin == 0)
                    {
                        mate.Skin = Morph;
                        session.SendPacket(mate.GenerateCMode(mate.Skin));
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    break;

                //Atk/Def/HP/Exp potions
                case 6600:
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    break;
                // Divorce letter
                case 6969: // this is imaginary number I = √(-1)
                    CharacterRelationDTO rel = session.Character.CharacterRelations.FirstOrDefault(s => s.RelationType == CharacterRelationType.Spouse);
                    if (rel != null)
                    {
                        session.Character.DeleteRelation(rel.CharacterId == session.Character.CharacterId ? rel.RelatedCharacterId : rel.CharacterId);
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("DIVORCED")));
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    break;

                // Cupid's arrow
                case 34: // this is imaginary number I = √(-1)
                    if (packetsplit != null && packetsplit.Length > 3)
                    {
                        if (long.TryParse(packetsplit[3], out long characterId))
                        {
                            if (session.Character.CharacterRelations.Any(s => s.RelationType == CharacterRelationType.Spouse))
                            {
                                session.SendPacket($"info {Language.Instance.GetMessageFromKey("ALREADY_MARRIED")}");
                                return;
                            }
                            if (session.Character.IsFriendOfCharacter(characterId))
                            {
                                ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                                if (otherSession != null)
                                {
                                    otherSession.SendPacket(UserInterfaceHelper.Instance.GenerateDialog(
                                        $"#fins^-34^{session.Character.CharacterId} #fins^-69^{session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("MARRY_REQUEST"), session.Character.Name)}"));
                                    session.Character.FriendRequestCharacters.Add(characterId);
                                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);

                                }
                            }
                            else
                            {
                                session.SendPacket($"info {Language.Instance.GetMessageFromKey("NOT_FRIEND")}");
                            }
                        }
                    }
                    break;

                case 570:
                    if (session.Character.Faction == (FactionType)EffectValue)
                    {
                        return;
                    }
                    session.SendPacket(session.Character.Family == null
                        ? $"qna #guri^750^{EffectValue} {Language.Instance.GetMessageFromKey($"ASK_CHANGE_FACTION{EffectValue}")}"
                        : UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("IN_FAMILY"), 0));
                    break;

                // wings
                case 650:
                    if (session.Character.UseSp && session.Character.SpInstance != null)
                    {
                        if (option == 0)
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte) inv.Type}^{inv.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_WINGS_CHANGE")}");
                        }
                        else
                        {
                            session.Character.SpInstance.Design = (byte) EffectValue;
                            session.Character.MorphUpgrade2 = EffectValue;
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                            session.SendPacket(session.Character.GenerateStat());
                            session.SendPacket(session.Character.GenerateStatChar());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    }
                    break;

                // presentation messages
                case 203:
                    if (!session.Character.IsVehicled)
                    {
                        if (option == 0)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(10, 2, session.Character.CharacterId, 1));
                        }
                    }
                    break;

                // magic lamps
                case 651:
                    if (session.Character.Inventory.All(i => i.Value.Type != InventoryType.Wear))
                    {
                        if (option == 0)
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_USE")}");
                        }
                        else
                        {
                            session.Character.ChangeSex();
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                // vehicles
                case 1000:
                    if (session.Character.HasShopOpened)
                    {
                        return;
                    }
                    if (Morph > 0)
                    {
                        if (option == 0 && !session.Character.IsVehicled)
                        {
                            if (session.Character.IsSitting)
                            {
                                session.Character.IsSitting = false;
                                session.CurrentMapInstance?.Broadcast(session.Character.GenerateRest());
                            }
                            session.Character.LastDelay = DateTime.Now;
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 3, $"#u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^2"));
                        }
                        else
                        {
                            if (!session.Character.IsVehicled && option != 0)
                            {
                                DateTime delay = DateTime.Now.AddSeconds(-4);
                                if (session.Character.LastDelay > delay && session.Character.LastDelay < delay.AddSeconds(2))
                                {
                                    session.Character.Speed = Speed;
                                    session.Character.IsVehicled = true;
                                    session.Character.VehicleSpeed = Speed;
                                    session.Character.MorphUpgrade = 0;
                                    session.Character.MorphUpgrade2 = 0;
                                    session.Character.Morph = Morph + (byte)session.Character.Gender;
                                    session.Character.Mates?.ForEach(x => session.CurrentMapInstance?.Broadcast(x.GenerateOut()));
                                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(196), session.Character.MapX, session.Character.MapY);
                                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                                    session.SendPacket(session.Character.GenerateCond());
                                    session.Character.LastSpeedChange = DateTime.Now;
                                }
                            }
                            else if (session.Character.IsVehicled)
                            {
                                session.Character.Mates?.ForEach(x =>
                                {
                                    x.PositionX = session.Character.PositionX;
                                    x.PositionY = session.Character.PositionY;
                                    session.CurrentMapInstance?.Broadcast(x.GenerateIn());
                                });
                                session.Character.RemoveVehicle();
                            }
                        }
                    }
                    break;

                case 1002:
                    // TODO REVIEW THIS
                    /*
                    if (EffectValue == 69)
                    {
                        int rnd = ServerManager.Instance.RandomNumber(0, 1000);
                        if (rnd < 5)
                        {
                            short[] vnums =
                            {
                                5560, 5591, 4099, 907, 1160, 4705, 4706, 4707, 4708, 4709, 4710, 4711, 4712, 4713, 4714,
                                4715, 4716
                            };
                            byte[] counts = {1, 1, 1, 1, 10, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
                            int item = ServerManager.Instance.RandomNumber(0, 17);

                            session.Character.GiftAdd(vnums[item], counts[item]);
                        }
                        else if (rnd < 30)
                        {
                            short[] vnums = {361, 362, 363, 366, 367, 368, 371, 372, 373};
                            session.Character.GiftAdd(vnums[ServerManager.Instance.RandomNumber(0, 9)], 1);
                        }
                        else
                        {
                            short[] vnums =
                            {
                                1161, 2282, 1030, 1244, 1218, 5369, 1012, 1363, 1364, 2160, 2173, 5959, 5983, 2514,
                                2515, 2516, 2517, 2518, 2519, 2520, 2521, 1685, 1686, 5087, 5203, 2418, 2310, 2303,
                                2169, 2280, 5892, 5893, 5894, 5895, 5896, 5897, 5898, 5899, 5332, 5105, 2161, 2162
                            };
                            byte[] counts =
                            {
                                10, 10, 20, 5, 1, 1, 99, 1, 1, 5, 5, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 5, 20,
                                20, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
                            };
                            int item = ServerManager.Instance.RandomNumber(0, 42);
                            session.Character.GiftAdd(vnums[item], counts[item]);
                        }
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    */
                    if (session.HasCurrentMapInstance)
                    {
                        if (session.CurrentMapInstance.Map.MapTypes.All(m => m.MapTypeId != (short) MapTypeEnum.Act4))
                        {
                            short[] vnums =
                            {
                                1386, 1387, 1388, 1389, 1390, 1391, 1392, 1393, 1394, 1395, 1396, 1397, 1398, 1399,
                                1400, 1401, 1402, 1403, 1404, 1405
                            };
                            short vnum = vnums[ServerManager.Instance.RandomNumber(0, 20)];

                            NpcMonster npcmonster = ServerManager.Instance.GetNpc(vnum);
                            if (npcmonster == null)
                            {
                                return;
                            }
                            MapMonster monster = new MapMonster
                            {
                                MonsterVNum = vnum,
                                MapY = session.Character.MapY,
                                MapX = session.Character.MapX,
                                MapId = session.Character.MapInstance.Map.MapId,
                                Position = (byte) session.Character.Direction,
                                IsMoving = true,
                                MapMonsterId = session.CurrentMapInstance.GetNextMonsterId(),
                                ShouldRespawn = false
                            };
                            monster.Initialize(session.CurrentMapInstance);
                            session.CurrentMapInstance.AddMonster(monster);
                            session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    break;

                case 69:
                    session.Character.GetReput(ReputPrice);
                    session.SendPacket(session.Character.GenerateFd());
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    break;

                case 1003:
                    if (!session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.BazaarMedalGold
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1004:
                    if (!session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalGold))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.BazaarMedalSilver
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1005:
                    if (session.Character.StaticBonusList.All(s => s.StaticBonusType != StaticBonusType.BackPack))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.BackPack
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateExts());
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1006:
                    if (option == 0)
                    {
                        session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^2 {Language.Instance.GetMessageFromKey("ASK_PET_MAX")}");
                    }
                    else
                    {
                        if (session.Character.MaxMateCount < 30)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GET_PET_PLACES"), 10));
                            session.SendPacket(session.Character.GenerateScpStc());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    break;

                case 1007:
                    if (session.Character.StaticBonusList.All(s => s.StaticBonusType != StaticBonusType.PetBasket))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.PetBasket
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateExts());
                        session.SendPacket("ib 1278 1");
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1008:
                    if (session.Character.StaticBonusList.All(s => s.StaticBonusType != StaticBonusType.PetBackPack))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.PetBackPack
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateExts());
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType()));
                    break;
            }
        }




        #endregion
    }
}