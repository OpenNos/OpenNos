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
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Buff.Indicators;
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

        public override void Use(ClientSession session, ref ItemInstance inv, bool delay = false, string[] packetsplit = null)
        {
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
                    session.SendPacket(session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDED"), EffectValue), 0));
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
                    session.SendPacket(session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDEDBOTH"), EffectValue, EffectValue * 3), 0));
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.SendPacket(session.Character.GenerateSpPoint());
                    break;
                //Atk/Def/HP/Exp potions
                case 6600:
                    switch (EffectValue)
                    {
                        case 1:
                            IndicatorBase buff1 = new Buff.Indicators.Item.AttackPotion(session.Character.Level);
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(203));
                            session.Character.Buff.Add(buff1);
                            break;
                        case 2:
                            IndicatorBase buff2 = new Buff.Indicators.Item.DefensePotion(session.Character.Level);
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(203));
                            session.Character.Buff.Add(buff2);
                            break;
                        case 3:
                            IndicatorBase buff3 = new Buff.Indicators.Item.EnergyPotion(session.Character.Level);
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(203));
                            session.Character.Buff.Add(buff3);
                            break;
                        case 4:
                            IndicatorBase buff4 = new Buff.Indicators.Item.ExperiencePotion(session.Character.Level);
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(203));
                            session.Character.Buff.Add(buff4);
                            break;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
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
                        : session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("IN_FAMILY"), 0));
                    break;

                // wings
                case 650:
                    SpecialistInstance specialistInstance = session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                    if (session.Character.UseSp && specialistInstance != null)
                    {
                        if (!delay)
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
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    }
                    break;

                // presentation messages
                case 203:
                    if (!session.Character.IsVehicled)
                    {
                        if (!delay)
                        {
                            session.SendPacket(session.Character.GenerateGuri(10, 2, 1));
                        }
                    }
                    break;

                // magic lamps
                case 651:
                    if (session.Character.Inventory.GetAllItems().All(i => i.Type != InventoryType.Wear))
                    {
                        if (!delay)
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
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                // vehicles
                case 1000:
                    if (Morph > 0)
                    {
                        if (!delay && !session.Character.IsVehicled)
                        {
                            if (session.Character.IsSitting)
                            {
                                session.Character.IsSitting = false;
                                session.CurrentMapInstance?.Broadcast(session.Character.GenerateRest());
                            }
                            session.SendPacket(session.Character.GenerateDelay(3000, 3, $"#u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^2"));
                        }
                        else
                        {
                            if (!session.Character.IsVehicled && delay)
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
                        int rnd = ServerManager.RandomNumber(0, 1000);
                        if (rnd < 5)
                        {
                            short[] vnums = { 5560, 5591, 4099, 907, 1160, 4705, 4706, 4707, 4708, 4709, 4710, 4711, 4712, 4713, 4714, 4715, 4716 };
                            byte[] counts = { 1, 1, 1, 1, 10, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                            int item = ServerManager.RandomNumber(0, 17);

                            session.Character.GiftAdd(vnums[item], counts[item]);
                        }
                        else if (rnd < 30)
                        {
                            short[] vnums = { 361, 362, 363, 366, 367, 368, 371, 372, 373 };
                            session.Character.GiftAdd(vnums[ServerManager.RandomNumber(0, 9)], 1);
                        }
                        else
                        {
                            short[] vnums = { 1161, 2282, 1030, 1244, 1218, 5369, 1012, 1363, 1364, 2160, 2173, 5959, 5983, 2514, 2515, 2516, 2517, 2518, 2519, 2520, 2521, 1685, 1686, 5087, 5203, 2418, 2310, 2303, 2169, 2280, 5892, 5893, 5894, 5895, 5896, 5897, 5898, 5899, 5332, 5105, 2161, 2162 };
                            byte[] counts = { 10, 10, 20, 5, 1, 1, 99, 1, 1, 5, 5, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 5, 20, 20, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                            int item = ServerManager.RandomNumber(0, 42);
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
                                short[] vnums = { 1386, 1387, 1388, 1389, 1390, 1391, 1392, 1393, 1394, 1395, 1396, 1397, 1398, 1399, 1400, 1401, 1402, 1403, 1404, 1405 };
                                short vnum = vnums[ServerManager.RandomNumber(0, 20)];

                                NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                                if (npcmonster == null)
                                {
                                    return;
                                }
                                // ReSharper disable once PossibleNullReferenceException HasCurrentMapInstance NullCheck
                                MapMonster monster = new MapMonster { MonsterVNum = vnum, MapY = session.Character.MapY, MapX = session.Character.MapX, MapId = session.Character.MapInstance.Map.MapId, Position = (byte)session.Character.Direction, IsMoving = true, MapMonsterId = session.CurrentMapInstance.GetNextMonsterId(), ShouldRespawn = false };
                                monster.Initialize(session.CurrentMapInstance);
                                monster.StartLife();
                                session.CurrentMapInstance.AddMonster(monster);
                                session.CurrentMapInstance.Broadcast(monster.GenerateIn3());
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
                        session.Character.StaticBonusList.Add(new StaticBonusDTO { CharacterId = session.Character.CharacterId, DateEnd = DateTime.Now.AddDays(EffectValue), StaticBonusType = StaticBonusType.BazaarMedalGold });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1004:
                    if (!session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalGold))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO { CharacterId = session.Character.CharacterId, DateEnd = DateTime.Now.AddDays(EffectValue), StaticBonusType = StaticBonusType.BazaarMedalSilver });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1005:
                    if (session.Character.StaticBonusList.All(s => s.StaticBonusType != StaticBonusType.BackPack))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO { CharacterId = session.Character.CharacterId, DateEnd = DateTime.Now.AddDays(EffectValue), StaticBonusType = StaticBonusType.BackPack });
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