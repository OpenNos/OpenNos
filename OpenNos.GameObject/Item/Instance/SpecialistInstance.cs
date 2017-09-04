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
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class SpecialistInstance : WearableInstance, ISpecialistInstance
    {
        #region Members

        private Random _random;

        private long _transportId;

        #endregion

        #region Instantiation

        public SpecialistInstance()
        {
            _random = new Random();
        }

        public SpecialistInstance(Guid id)
        {
            Id = id;
            _random = new Random();
        }

        public SpecialistInstance(SpecialistInstanceDTO specialistInstance)
        {
            _random = new Random();
            SpDamage = specialistInstance.SpDamage;
            SpDark = specialistInstance.SpDark;
            SpDefence = specialistInstance.SpDefence;
            SpElement = specialistInstance.SpElement;
            SpFire = specialistInstance.SpFire;
            SpHP = specialistInstance.SpHP;
            SpLight = specialistInstance.SpLight;
            SpStoneUpgrade = specialistInstance.SpStoneUpgrade;
            SpWater = specialistInstance.SpWater;
            SpLevel = specialistInstance.SpLevel;
            SlDefence = specialistInstance.SlDefence;
            SlElement = specialistInstance.SlElement;
            SlDamage = specialistInstance.SlDamage;
            SlHP = specialistInstance.SlHP;
        }

        #endregion

        #region Properties

        public short SlDamage { get; set; }

        public short SlDefence { get; set; }

        public short SlElement { get; set; }

        public short SlHP { get; set; }

        public byte SpDamage { get; set; }

        public byte SpDark { get; set; }

        public byte SpDefence { get; set; }

        public byte SpElement { get; set; }

        public byte SpFire { get; set; }

        public byte SpHP { get; set; }

        public byte SpLevel { get; set; }

        public byte SpLight { get; set; }

        public byte SpStoneUpgrade { get; set; }

        public byte SpWater { get; set; }

        public long TransportId
        {
            get
            {
                if (_transportId == 0)
                {
                    // create transportId thru factory
                    _transportId = TransportFactory.Instance.GenerateTransportId();
                }

                return _transportId;
            }
        }

        #endregion

        #region Methods

        public string GeneratePslInfo()
        {
            // 1235.3 1237.4 1239.5 <= skills SkillVNum.Grade
            return $"pslinfo {Item.VNum} {Item.Element} {Item.ElementRate} {Item.LevelJobMinimum} {Item.Speed} {Item.FireResistance} {Item.WaterResistance} {Item.LightResistance} {Item.DarkResistance} 0.0 0.0 0.0";
        }

        public string GenerateSlInfo()
        {
            int freepoint = CharacterHelper.SPPoint(SpLevel, Upgrade) - SlDamage - SlHP - SlElement - SlDefence;

            int slHit = CharacterHelper.SlPoint(SlDamage, 0);
            int slDefence = CharacterHelper.SlPoint(SlDefence, 0);
            int slElement = CharacterHelper.SlPoint(SlElement, 0);
            int slHp = CharacterHelper.SlPoint(SlHP, 0);

            if (CharacterSession != null)
            {
                slHit += CharacterSession.Character.GetMostValueEquipmentBuff(BCardType.CardType.SPSL, (byte)AdditionalTypes.SPSL.Attack) +
                         CharacterSession.Character.GetMostValueEquipmentBuff(BCardType.CardType.SPSL, (byte)AdditionalTypes.SPSL.All);

                slDefence += CharacterSession.Character.GetMostValueEquipmentBuff(BCardType.CardType.SPSL, (byte)AdditionalTypes.SPSL.Defense) +
                             CharacterSession.Character.GetMostValueEquipmentBuff(BCardType.CardType.SPSL, (byte)AdditionalTypes.SPSL.All);

                slElement += CharacterSession.Character.GetMostValueEquipmentBuff(BCardType.CardType.SPSL, (byte)AdditionalTypes.SPSL.Element) +
                             CharacterSession.Character.GetMostValueEquipmentBuff(BCardType.CardType.SPSL, (byte)AdditionalTypes.SPSL.All);

                slHp += CharacterSession.Character.GetMostValueEquipmentBuff(BCardType.CardType.SPSL, (byte)AdditionalTypes.SPSL.HPMP) +
                        CharacterSession.Character.GetMostValueEquipmentBuff(BCardType.CardType.SPSL, (byte)AdditionalTypes.SPSL.All);

                slHit = slHit > 100 ? 100 : slHit;
                slDefence = slDefence > 100 ? 100 : slDefence;
                slElement = slElement > 100 ? 100 : slElement;
                slHp = slHp > 100 ? 100 : slHp;
            }

            string skill = string.Empty;
            List<CharacterSkill> skillsSp = ServerManager.Instance.GetAllSkill().Where(ski => ski.Class == Item.Morph + 31 && ski.LevelMinimum <= SpLevel).Select(ski => new CharacterSkill
                {
                    SkillVNum = ski.SkillVNum,
                    CharacterId = CharacterId
                })
                .ToList();
            byte spdestroyed = 0;
            if (Rare == -2)
            {
                spdestroyed = 1;
            }
            if (!skillsSp.Any())
            {
                skill = "-1";
            }
            short firstskillvnum = skillsSp[0].SkillVNum;

            for (int i = 1; i < 11; i++)
            {
                if (skillsSp.Count < i + 1)
                {
                    continue;
                }
                if (skillsSp[i].SkillVNum <= firstskillvnum + 10)
                {
                    skill += $"{skillsSp[i].SkillVNum}.";
                }
            }

            // 10 9 8 '0 0 0 0'<- bonusdamage bonusarmor bonuselement bonushpmp its after upgrade and
            // 3 first values are not important
            skill = skill.TrimEnd('.');
            return
                $"slinfo {(Type == InventoryType.Wear || Type == InventoryType.Specialist || Type == InventoryType.Equipment ? "0" : "2")} {ItemVNum} {Item.Morph} {SpLevel} {Item.LevelJobMinimum} {Item.ReputationMinimum} 0 0 0 0 0 0 0 {Item.SpType} {Item.FireResistance} {Item.WaterResistance} {Item.LightResistance} {Item.DarkResistance} {XP} {CharacterHelper.SPXPData[SpLevel - 1]} {skill} {TransportId} {freepoint} {slHit} {slDefence} {slElement} {slHp} {Upgrade} 0 0 {spdestroyed} 0 0 0 0 {SpStoneUpgrade} {SpDamage} {SpDefence} {SpElement} {SpHP} {SpFire} {SpWater} {SpLight} {SpDark}";
        }

        public void PerfectSP()
        {
            short[] upsuccess = { 50, 40, 30, 20, 10 };

            int[] goldprice = { 5000, 10000, 20000, 50000, 100000 };
            short[] stoneprice = { 1, 2, 3, 4, 5 };
            short stonevnum;
            byte upmode = 1;

            switch (Item.Morph)
            {
                case 2:
                    stonevnum = 2514;
                    break;

                case 6:
                    stonevnum = 2514;
                    break;

                case 9:
                    stonevnum = 2514;
                    break;

                case 12:
                    stonevnum = 2514;
                    break;

                case 3:
                    stonevnum = 2515;
                    break;

                case 4:
                    stonevnum = 2515;
                    break;

                case 14:
                    stonevnum = 2515;
                    break;

                case 5:
                    stonevnum = 2516;
                    break;

                case 11:
                    stonevnum = 2516;
                    break;

                case 15:
                    stonevnum = 2516;
                    break;

                case 10:
                    stonevnum = 2517;
                    break;

                case 13:
                    stonevnum = 2517;
                    break;

                case 7:
                    stonevnum = 2517;
                    break;

                case 17:
                    stonevnum = 2518;
                    break;

                case 18:
                    stonevnum = 2518;
                    break;

                case 19:
                    stonevnum = 2518;
                    break;

                case 20:
                    stonevnum = 2519;
                    break;

                case 21:
                    stonevnum = 2519;
                    break;

                case 22:
                    stonevnum = 2519;
                    break;

                case 23:
                    stonevnum = 2520;
                    break;

                case 24:
                    stonevnum = 2520;
                    break;

                case 25:
                    stonevnum = 2520;
                    break;

                case 26:
                    stonevnum = 2521;
                    break;

                case 27:
                    stonevnum = 2521;
                    break;

                case 28:
                    stonevnum = 2521;
                    break;

                default:
                    return;
            }
            if (SpStoneUpgrade > 99)
            {
                return;
            }
            if (SpStoneUpgrade > 80)
            {
                upmode = 5;
            }
            else if (SpStoneUpgrade > 60)
            {
                upmode = 4;
            }
            else if (SpStoneUpgrade > 40)
            {
                upmode = 3;
            }
            else if (SpStoneUpgrade > 20)
            {
                upmode = 2;
            }

            if (IsFixed)
            {
                return;
            }
            if (CharacterSession.Character.Gold < goldprice[upmode - 1])
            {
                return;
            }
            if (CharacterSession.Character.Inventory.CountItem(stonevnum) < stoneprice[upmode - 1])
            {
                return;
            }

            SpecialistInstance specialist = CharacterSession.Character.Inventory.LoadByItemInstance<SpecialistInstance>(Id);

            int rnd = ServerManager.Instance.RandomNumber();
            if (rnd < upsuccess[upmode - 1])
            {
                byte type = (byte)ServerManager.Instance.RandomNumber(0, 16), count = 1;
                if (upmode == 4)
                {
                    count = 2;
                }
                if (upmode == 5)
                {
                    count = (byte)ServerManager.Instance.RandomNumber(3, 6);
                }

                CharacterSession.CurrentMapInstance.Broadcast(CharacterSession.Character.GenerateEff(3005), CharacterSession.Character.MapX, CharacterSession.Character.MapY);

                if (type < 3)
                {
                    specialist.SpDamage += count;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 12));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 0));
                }
                else if (type < 6)
                {
                    specialist.SpDefence += count;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 12));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 0));
                }
                else if (type < 9)
                {
                    specialist.SpElement += count;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 12));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 0));
                }
                else if (type < 12)
                {
                    specialist.SpHP += count;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 12));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 0));
                }
                else if (type == 12)
                {
                    specialist.SpFire += count;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 12));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 0));
                }
                else if (type == 13)
                {
                    specialist.SpWater += count;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 12));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 0));
                }
                else if (type == 14)
                {
                    specialist.SpLight += count;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 12));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 0));
                }
                else if (type == 15)
                {
                    specialist.SpDark += count;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 12));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 0));
                }
                specialist.SpStoneUpgrade++;
            }
            else
            {
                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("PERFECTSP_FAILURE"), 11));
                CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("PERFECTSP_FAILURE"), 0));
            }
            CharacterSession.SendPacket(specialist.GenerateInventoryAdd());
            CharacterSession.Character.Gold -= goldprice[upmode - 1];
            CharacterSession.SendPacket(CharacterSession.Character.GenerateGold());
            CharacterSession.Character.Inventory.RemoveItemAmount(stonevnum, stoneprice[upmode - 1]);
            CharacterSession.SendPacket("shop_end 1");
        }

        public void UpgradeSp(UpgradeProtection protect)
        {
            if (Upgrade >= 15)
            {
                return;
            }

            short[] upfail = { 20, 25, 30, 40, 50, 60, 65, 70, 75, 80, 90, 93, 95, 97, 99 };
            short[] destroy = { 0, 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 70 };

            int[] goldprice = { 200000, 200000, 200000, 200000, 200000, 500000, 500000, 500000, 500000, 500000, 1000000, 1000000, 1000000, 1000000, 1000000 };
            short[] feather = { 3, 5, 8, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 70 };
            short[] fullmoon = { 1, 3, 5, 7, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30 };
            short[] soul = { 2, 4, 6, 8, 10, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };
            const short featherVnum = 2282;
            const short fullmoonVnum = 1030;
            const short greenSoulVnum = 2283;
            const short redSoulVnum = 2284;
            const short blueSoulVnum = 2285;
            const short dragonSkinVnum = 2511;
            const short dragonBloodVnum = 2512;
            const short dragonHeartVnum = 2513;
            const short blueScrollVnum = 1363;
            const short redScrollVnum = 1364;

            if (CharacterSession == null)
            {
                return;
            }
            if (!CharacterSession.HasCurrentMapInstance)
            {
                return;
            }
            if (CharacterSession.Character.Inventory.CountItem(fullmoonVnum) < fullmoon[Upgrade])
            {
                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(fullmoonVnum).Name, fullmoon[Upgrade])), 10));
                return;
            }
            if (CharacterSession.Character.Inventory.CountItem(featherVnum) < feather[Upgrade])
            {
                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(featherVnum).Name, feather[Upgrade])), 10));
                return;
            }
            if (CharacterSession.Character.Gold < goldprice[Upgrade])
            {
                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                return;
            }

            if (Upgrade < 5)
            {
                if (SpLevel > 20)
                {
                    if (Item.Morph <= 16)
                    {
                        if (CharacterSession.Character.Inventory.CountItem(greenSoulVnum) < soul[Upgrade])
                        {
                            CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(greenSoulVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (CharacterSession.Character.Inventory.CountItem(blueScrollVnum) < 1)
                            {
                                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(blueScrollVnum).Name, 1)), 10));
                                return;
                            }
                            CharacterSession.Character.Inventory.RemoveItemAmount(blueScrollVnum);
                            CharacterSession.SendPacket("shop_end 2");
                        }
                    }
                    else
                    {
                        if (CharacterSession.Character.Inventory.CountItem(dragonSkinVnum) < soul[Upgrade])
                        {
                            CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(dragonSkinVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (CharacterSession.Character.Inventory.CountItem(blueScrollVnum) < 1)
                            {
                                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(blueScrollVnum).Name, 1)), 10));
                                return;
                            }
                            CharacterSession.Character.Inventory.RemoveItemAmount(blueScrollVnum);
                            CharacterSession.SendPacket("shop_end 2");
                        }
                    }
                }
                else
                {
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIRED"), 21), 11));
                    return;
                }
            }
            else if (Upgrade < 10)
            {
                if (SpLevel > 40)
                {
                    if (Item.Morph <= 16)
                    {
                        if (CharacterSession.Character.Inventory.CountItem(redSoulVnum) < soul[Upgrade])
                        {
                            CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(redSoulVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (CharacterSession.Character.Inventory.CountItem(blueScrollVnum) < 1)
                            {
                                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(blueScrollVnum).Name, 1)), 10));
                                return;
                            }
                            CharacterSession.Character.Inventory.RemoveItemAmount(blueScrollVnum);
                            CharacterSession.SendPacket("shop_end 2");
                        }
                    }
                    else
                    {
                        if (CharacterSession.Character.Inventory.CountItem(dragonBloodVnum) < soul[Upgrade])
                        {
                            CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(dragonBloodVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (CharacterSession.Character.Inventory.CountItem(blueScrollVnum) < 1)
                            {
                                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(blueScrollVnum).Name, 1)), 10));
                                return;
                            }
                            CharacterSession.Character.Inventory.RemoveItemAmount(blueScrollVnum);
                            CharacterSession.SendPacket("shop_end 2");
                        }
                    }
                }
                else
                {
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIRED"), 41), 11));
                    return;
                }
            }
            else if (Upgrade < 15)
            {
                if (SpLevel > 50)
                {
                    if (Item.Morph <= 16)
                    {
                        if (CharacterSession.Character.Inventory.CountItem(blueSoulVnum) < soul[Upgrade])
                        {
                            CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(blueSoulVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (CharacterSession.Character.Inventory.CountItem(redScrollVnum) < 1)
                            {
                                return;
                            }
                            CharacterSession.Character.Inventory.RemoveItemAmount(redScrollVnum);
                            CharacterSession.SendPacket("shop_end 2");
                        }
                    }
                    else
                    {
                        if (CharacterSession.Character.Inventory.CountItem(dragonHeartVnum) < soul[Upgrade])
                        {
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (CharacterSession.Character.Inventory.CountItem(redScrollVnum) < 1)
                            {
                                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.Instance.GetItem(redScrollVnum).Name, 1)), 10));
                                return;
                            }
                            CharacterSession.Character.Inventory.RemoveItemAmount(redScrollVnum);
                            CharacterSession.SendPacket("shop_end 2");
                        }
                    }
                }
                else
                {
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIRED"), 51), 11));
                    return;
                }
            }

            CharacterSession.Character.Gold -= goldprice[Upgrade];

            // remove feather and fullmoon before upgrading
            CharacterSession.Character.Inventory.RemoveItemAmount(featherVnum, feather[Upgrade]);
            CharacterSession.Character.Inventory.RemoveItemAmount(fullmoonVnum, fullmoon[Upgrade]);

            WearableInstance wearable = CharacterSession.Character.Inventory.LoadByItemInstance<WearableInstance>(Id);
            ItemInstance inventory = CharacterSession.Character.Inventory.GetItemInstanceById(Id);
            int rnd = ServerManager.Instance.RandomNumber();
            if (rnd < destroy[Upgrade])
            {
                if (protect == UpgradeProtection.Protected)
                {
                    CharacterSession.CurrentMapInstance.Broadcast(CharacterSession.Character.GenerateEff(3004), CharacterSession.Character.MapX, CharacterSession.Character.MapY);
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 11));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 0));
                }
                else
                {
                    wearable.Rare = -2;
                    CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROYED"), 11));
                    CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROYED"), 0));
                    CharacterSession.SendPacket(wearable.GenerateInventoryAdd());
                }
            }
            else if (rnd < upfail[Upgrade])
            {
                if (protect == UpgradeProtection.Protected)
                {
                    CharacterSession.CurrentMapInstance.Broadcast(CharacterSession.Character.GenerateEff(3004), CharacterSession.Character.MapX, CharacterSession.Character.MapY);
                }
                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 11));
                CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 0));
            }
            else
            {
                if (protect == UpgradeProtection.Protected)
                {
                    CharacterSession.CurrentMapInstance.Broadcast(CharacterSession.Character.GenerateEff(3004), CharacterSession.Character.MapX, CharacterSession.Character.MapY);
                }
                CharacterSession.CurrentMapInstance.Broadcast(CharacterSession.Character.GenerateEff(3005), CharacterSession.Character.MapX, CharacterSession.Character.MapY);
                CharacterSession.SendPacket(CharacterSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 12));
                CharacterSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 0));
                if (Upgrade < 5)
                {
                    CharacterSession.Character.Inventory.RemoveItemAmount(Item.Morph <= 16 ? greenSoulVnum : dragonSkinVnum, soul[Upgrade]);
                }
                else if (Upgrade < 10)
                {
                    CharacterSession.Character.Inventory.RemoveItemAmount(Item.Morph <= 16 ? redSoulVnum : dragonBloodVnum, soul[Upgrade]);
                }
                else if (Upgrade < 15)
                {
                    CharacterSession.Character.Inventory.RemoveItemAmount(Item.Morph <= 16 ? blueSoulVnum : dragonHeartVnum, soul[Upgrade]);
                }
                wearable.Upgrade++;
                if (wearable.Upgrade > 8)
                {
                    CharacterSession.Character.Family?.InsertFamilyLog(FamilyLogType.ItemUpgraded, CharacterSession.Character.Name, itemVNum: wearable.ItemVNum, upgrade: wearable.Upgrade);
                }
                CharacterSession.SendPacket(wearable.GenerateInventoryAdd());
            }
            CharacterSession.SendPacket(CharacterSession.Character.GenerateGold());
            CharacterSession.SendPacket(CharacterSession.Character.GenerateEq());
            CharacterSession.SendPacket("shop_end 1");
        }

        #endregion
    }
}