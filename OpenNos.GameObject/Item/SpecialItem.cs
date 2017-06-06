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
using System.Linq;

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

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
        {
            inv.Item.BCards.ForEach(c => c.ApplyBCards(session.Character));

            switch (Effect)
            {
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

                case 301:
                    if (ServerManager.Instance.IsCharacterMemberOfGroup(session.Character.CharacterId))
                    {
                        //TODO you are in group
                        return;
                    }
                    ItemInstance raidSeal = session.Character.Inventory.LoadBySlotAndType<ItemInstance>(inv.Slot, InventoryType.Main);
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, raidSeal.Id);

                    ScriptedInstance raid = ServerManager.Instance.Raids.FirstOrDefault(s => s.RequieredItems.Any(obj => obj.VNum == raidSeal.ItemVNum)).GetClone();
                    if (raid != null)
                    {
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

                case 208:
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.Character.AddStaticBuff(new StaticBuffDTO() { CardId = 121 });
                    break;
                // Divorce letter
                case 6969: // this is imaginary number I = √(-1)
                    break;

                // Cupid's arrow
                case 34: // this is imaginary number I = √(-1)
                    break;

                case 570:
                    if (session.Character.Faction == EffectValue)
                    {
                        return;
                    }
                    session.SendPacket(session.Character.Family == null
                        ? $"qna #guri^750^{EffectValue} {Language.Instance.GetMessageFromKey($"ASK_CHANGE_FACTION{EffectValue}")}"
                        : UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("IN_FAMILY"),
                            0));
                    break;

                // wings
                case 650:
                    SpecialistInstance specialistInstance = session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                    if (session.Character.UseSp && specialistInstance != null)
                    {
                        if (Option == 0)
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_WINGS_CHANGE")}");
                        }
                        else
                        {
                            specialistInstance.Design = (byte)EffectValue;
                            session.Character.MorphUpgrade2 = EffectValue;
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                            session.SendPacket(session.Character.GenerateStat());
                            session.SendPacket(session.Character.GenerateStatChar());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    }
                    break;

                // presentation messages
                case 203:
                    if (!session.Character.IsVehicled)
                    {
                        if (Option == 0)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(10, 2, session.Character.CharacterId, 1));
                        }
                    }
                    break;

                // magic lamps
                case 651:
                    if (session.Character.Inventory.GetAllItems().All(i => i.Type != InventoryType.Wear))
                    {
                        if (Option == 0)
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
                    if (Morph > 0)
                    {
                        if (Option == 0 && !session.Character.IsVehicled)
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
                            if (!session.Character.IsVehicled && Option != 0)
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
                                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(196), session.Character.MapX, session.Character.MapY);
                                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                                    session.SendPacket(session.Character.GenerateCond());
                                    session.Character.LastSpeedChange = DateTime.Now;
                                }
                            }
                            else if (session.Character.IsVehicled)
                            {
                                session.Character.RemoveVehicle();
                            }
                        }
                    }
                    break;

                case 1002:
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
                            byte[] counts = { 1, 1, 1, 1, 10, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                            int item = ServerManager.Instance.RandomNumber(0, 17);

                            session.Character.GiftAdd(vnums[item], counts[item]);
                        }
                        else if (rnd < 30)
                        {
                            short[] vnums = { 361, 362, 363, 366, 367, 368, 371, 372, 373 };
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
                    else
                    {
                        if (session.HasCurrentMapInstance)
                        {
                            if (session.CurrentMapInstance.Map.MapTypes.All(m => m.MapTypeId != (short)MapTypeEnum.Act4))
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
                                    Position = (byte)session.Character.Direction,
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
                    }
                    break;

                case 69:
                    session.Character.Reput += ReputPrice;
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
                    if (Option == 0)
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