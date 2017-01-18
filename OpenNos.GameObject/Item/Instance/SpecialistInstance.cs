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
using System;

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

        public void PerfectSP(ClientSession Session)
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
            else if (SpStoneUpgrade > 80)
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
            if (Session.Character.Gold < goldprice[upmode - 1])
            {
                return;
            }
            if (Session.Character.Inventory.CountItem(stonevnum) < stoneprice[upmode - 1])
            {
                return;
            }

            SpecialistInstance specialist = Session.Character.Inventory.LoadByItemInstance<SpecialistInstance>(Id);

            int rnd = ServerManager.RandomNumber();
            if (rnd <= upsuccess[upmode - 1])
            {
                byte type = (byte)ServerManager.RandomNumber(0, 16), count = 1;
                if (upmode == 4)
                {
                    count = 2;
                }
                if (upmode == 5)
                {
                    count = (byte)ServerManager.RandomNumber(3, 6);
                }

                Session.CurrentMapInstance.Broadcast(Session.Character.GenerateEff(3005), Session.Character.MapX, Session.Character.MapY);

                if (type < 3)
                {
                    specialist.SpDamage += count;
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 0));
                }
                else if (type < 6)
                {
                    specialist.SpDefence += count;
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 0));
                }
                else if (type < 9)
                {
                    specialist.SpElement += count;
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 0));
                }
                else if (type < 12)
                {
                    specialist.SpHP += count;
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 0));
                }
                else if (type == 12)
                {
                    specialist.SpFire += count;
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 0));
                }
                else if (type == 13)
                {
                    specialist.SpWater += count;
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 0));
                }
                else if (type == 14)
                {
                    specialist.SpLight += count;
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 0));
                }
                else if (type == 15)
                {
                    specialist.SpDark += count;
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 0));
                }
                specialist.SpStoneUpgrade++;
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PERFECTSP_FAILURE"), 11));
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("PERFECTSP_FAILURE"), 0));
            }
            Session.SendPacket(Session.Character.GenerateInventoryAdd(ItemVNum, Amount, InventoryType.Equipment, Slot, Rare, Design, Upgrade, SpStoneUpgrade));
            Session.Character.Gold -= goldprice[upmode - 1];
            Session.SendPacket(Session.Character.GenerateGold());
            Session.Character.Inventory.RemoveItemAmount(stonevnum, stoneprice[upmode - 1]);
            Session.SendPacket("shop_end 1");
        }

        public void UpgradeSp(ClientSession Session, UpgradeProtection protect)
        {
            short[] upsuccess = { 100, 100, 95, 90, 85, 80, 75, 70, 65, 60, 55, 50, 45, 40, 30 };
            short[] upfail = { 20, 25, 25, 30, 35, 40, 40, 40, 40, 40, 45, 43, 40, 37, 29 };

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

            if (!Session.HasCurrentMapInstance)
            {
                return;
            }
            if (IsFixed)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_FIXED"), 10));
                return;
            }
            if (Session.Character.Inventory.CountItem(fullmoonVnum) < fullmoon[Upgrade])
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(fullmoonVnum).Name, fullmoon[Upgrade])), 10));
                return;
            }
            if (Session.Character.Inventory.CountItem(featherVnum) < feather[Upgrade])
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(featherVnum).Name, feather[Upgrade])), 10));
                return;
            }
            if (Session.Character.Gold < goldprice[Upgrade])
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                return;
            }

            if (Upgrade < 5)
            {
                if (SpLevel > 20)
                {
                    if (Item.Morph <= 16)
                    {
                        if (Session.Character.Inventory.CountItem(greenSoulVnum) < soul[Upgrade])
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(greenSoulVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (Session.Character.Inventory.CountItem(blueScrollVnum) < 1)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(blueScrollVnum).Name, 1)), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(blueScrollVnum);
                            Session.SendPacket("shop_end 2");
                        }
                        Session.Character.Inventory.RemoveItemAmount(greenSoulVnum, soul[Upgrade]);
                    }
                    else
                    {
                        if (Session.Character.Inventory.CountItem(dragonSkinVnum) < soul[Upgrade])
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(dragonSkinVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (Session.Character.Inventory.CountItem(blueScrollVnum) < 1)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(blueScrollVnum).Name, 1)), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(blueScrollVnum);
                            Session.SendPacket("shop_end 2");
                        }
                        Session.Character.Inventory.RemoveItemAmount(dragonSkinVnum, soul[Upgrade]);
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIRED"), 21), 11));
                    return;
                }
            }
            else if (Upgrade < 10)
            {
                if (SpLevel > 40)
                {
                    if (Item.Morph <= 16)
                    {
                        if (Session.Character.Inventory.CountItem(redSoulVnum) < soul[Upgrade])
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(redSoulVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (Session.Character.Inventory.CountItem(blueScrollVnum) < 1)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(blueScrollVnum).Name, 1)), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(blueScrollVnum);
                            Session.SendPacket("shop_end 2");
                        }
                        Session.Character.Inventory.RemoveItemAmount(redSoulVnum, soul[Upgrade]);
                    }
                    else
                    {
                        if (Session.Character.Inventory.CountItem(dragonBloodVnum) < soul[Upgrade])
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(dragonBloodVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected)
                        {
                            if (Session.Character.Inventory.CountItem(blueScrollVnum) < 1)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(blueScrollVnum).Name, 1)), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(blueScrollVnum);
                            Session.SendPacket("shop_end 2");
                        }
                        Session.Character.Inventory.RemoveItemAmount(dragonBloodVnum, soul[Upgrade]);
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIRED"), 41), 11));
                    return;
                }
            }
            else if (Upgrade < 15)
            {
                if (SpLevel > 50)
                {
                    if (Item.Morph <= 16)
                    {
                        if (Session.Character.Inventory.CountItem(blueSoulVnum) < soul[Upgrade])
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(blueSoulVnum).Name, soul[Upgrade])), 10));
                            return;
                        }
                        if (protect == UpgradeProtection.Protected && Upgrade > 9)
                        {
                            if (Session.Character.Inventory.CountItem(redScrollVnum) < 1)
                            {
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(redScrollVnum);
                            Session.SendPacket("shop_end 2");
                        }
                        Session.Character.Inventory.RemoveItemAmount(blueSoulVnum, soul[Upgrade]);
                    }
                    else
                    {
                        if (Session.Character.Inventory.CountItem(dragonHeartVnum) < soul[Upgrade])
                        {
                            return;
                        }
                        if (protect == UpgradeProtection.Protected && Upgrade > 9)
                        {
                            if (Session.Character.Inventory.CountItem(redScrollVnum) < 1)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(redScrollVnum).Name, 1)), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(redScrollVnum);
                            Session.SendPacket("shop_end 2");
                        }
                        Session.Character.Inventory.RemoveItemAmount(dragonHeartVnum, soul[Upgrade]);
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIRED"), 51), 11));
                    return;
                }
            }

            Session.Character.Gold -= goldprice[Upgrade];

            // remove feather and fullmoon before upgrading
            Session.Character.Inventory.RemoveItemAmount(featherVnum, feather[Upgrade]);
            Session.Character.Inventory.RemoveItemAmount(fullmoonVnum, fullmoon[Upgrade]);

            WearableInstance wearable = Session.Character.Inventory.LoadByItemInstance<WearableInstance>(Id);
            ItemInstance inventory = Session.Character.Inventory.GetItemInstanceById(Id);
            int rnd = ServerManager.RandomNumber();
            if (rnd <= upfail[Upgrade])
            {
                if (protect == UpgradeProtection.Protected)
                {
                    Session.CurrentMapInstance.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                }
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 11));
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 0));
            }
            else if (rnd <= upsuccess[Upgrade])
            {
                if (protect == UpgradeProtection.Protected)
                {
                    Session.CurrentMapInstance.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                }
                Session.CurrentMapInstance.Broadcast(Session.Character.GenerateEff(3005), Session.Character.MapX, Session.Character.MapY);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 12));
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 0));
                wearable.Upgrade++;
                if (Session.Character.Family != null)
                {
                    Session.Character.Family.InsertFamilyLog(FamilyLogType.Upgrade, Session.Character.Name, "", "", "", 0, 0, wearable.ItemVNum, wearable.Upgrade, 0);
                }
                Session.SendPacket(Session.Character.GenerateInventoryAdd(ItemVNum, 1, inventory.Type, inventory.Slot, wearable.Rare, wearable.Design, wearable.Upgrade, SpStoneUpgrade));
            }
            else
            {
                if (protect == UpgradeProtection.Protected)
                {
                    Session.CurrentMapInstance.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 11));
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 0));
                }
                else
                {
                    wearable.Rare = -2;
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROYED"), 11));
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROYED"), 0));
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(ItemVNum, 1, inventory.Type, inventory.Slot, wearable.Rare, wearable.Design, wearable.Upgrade, SpStoneUpgrade));
                }
            }
            Session.SendPacket(Session.Character.GenerateGold());
            Session.SendPacket(Session.Character.GenerateEq());
            Session.SendPacket("shop_end 1");
        }

        #endregion
    }
}