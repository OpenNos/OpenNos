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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class Inventory : InventoryDTO, IGameObject
    {
        #region Instantiation

        public Inventory()
        {
            Mapper.CreateMap<InventoryDTO, Inventory>();
            Mapper.CreateMap<Inventory, InventoryDTO>();
        }

        #endregion

        #region Methods

        public void UpgradeItem(ClientSession Session,UpgradeMode mode,UpgradeProtection protection)
        {
            if (this.InventoryItem.Upgrade < 10)
            {
                short[] upsuccess = { 100, 100, 100, 95, 80, 60, 40, 30, 20, 11 };
                short[] upfix = { 0, 0, 10, 15, 20, 20, 20, 20, 15, 10 };

                //short itempricevnum1 = 0;
                //short itempricevnum2 = 0;
                int[] goldprice = { 500, 1500, 3000, 10000, 30000, 80000, 150000, 400000, 700000, 1000000 };
                short[] cella = { 20, 50, 80, 120, 160, 220, 280, 380, 480, 600 };
                short[] gem = { 1, 1, 2, 2, 3, 3, 1, 2, 2, 3 };

                int cellaVnum = 1014;
                int gemVnum = 1015;
                int gemFullVnum = 1016;
                double reducedpricefactor = 0.5;

                if (this.InventoryItem.IsFixed)
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_FIXED"), 10));

                    Session.Character.GetStartupInventory(Session);
                    Session.Client.SendPacket("shop_end 1");
                    return;
                }
                switch (mode)
                {
                    case UpgradeMode.Free:
                        break;

                    case UpgradeMode.Reduced:
                        // TODO: Reduced Item Amount
                        if (Session.Character.Gold < goldprice[this.InventoryItem.Upgrade] * reducedpricefactor)
                            return;
                        Session.Character.Gold = Session.Character.Gold - (long)(goldprice[this.InventoryItem.Upgrade] * reducedpricefactor);
                        if (Session.Character.InventoryList.CountItem(cellaVnum) < cella[this.InventoryItem.Upgrade] * reducedpricefactor)
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(cellaVnum, (int)(cella[this.InventoryItem.Upgrade] * reducedpricefactor));
                        if (this.InventoryItem.Upgrade <= 5)
                        {
                            if (Session.Character.InventoryList.CountItem(gemVnum) < gem[this.InventoryItem.Upgrade] * reducedpricefactor)
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemVnum, (int)(gem[this.InventoryItem.Upgrade] * reducedpricefactor));
                        }
                        else
                        {
                            if (Session.Character.InventoryList.CountItem(gemFullVnum) < gem[this.InventoryItem.Upgrade] * reducedpricefactor)
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemFullVnum, (int)(gem[this.InventoryItem.Upgrade] * reducedpricefactor));
                        }
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                        break;

                    case UpgradeMode.Normal:
                        // TODO: Normal Item Amount
                        if (Session.Character.Gold < goldprice[this.InventoryItem.Upgrade])
                            return;
                        Session.Character.Gold = Session.Character.Gold - goldprice[this.InventoryItem.Upgrade];
                        if (Session.Character.InventoryList.CountItem(cellaVnum) < cella[this.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(cellaVnum, (cella[this.InventoryItem.Upgrade]));
                        if (this.InventoryItem.Upgrade < 5)
                        {
                            if (Session.Character.InventoryList.CountItem(gemVnum) < gem[this.InventoryItem.Upgrade])
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemVnum, (gem[this.InventoryItem.Upgrade]));
                        }
                        else
                        {
                            if (Session.Character.InventoryList.CountItem(gemFullVnum) < gem[this.InventoryItem.Upgrade])
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemFullVnum, (gem[this.InventoryItem.Upgrade]));
                        }
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                        break;
                }

                Random r = new Random();
                int rnd = r.Next(100);
                if (rnd <= upfix[this.InventoryItem.Upgrade])
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.IsFixed = true;
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 0));
                }
                else if (rnd <= upsuccess[this.InventoryItem.Upgrade])
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 0));
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Upgrade++;
                }
                else
                {
                    if (protection ==UpgradeProtection.None)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 11));
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 0));
                        Session.Character.DeleteItem(Session, this.Type, this.Slot);
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 11));
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
                    }
                }
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 11));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
            }
            Session.Character.GetStartupInventory(Session);
            Session.Client.SendPacket("shop_end 1");
        }

        public void UpgradeSp(ClientSession Session,UpgradeProtection protect)
        {
            short[] upsuccess = { 100, 100, 95, 90, 85, 80, 75, 70, 65, 60, 55, 50, 45, 40, 30 };
            short[] upfail = { 20, 25, 25, 30, 35, 40, 40, 40, 40, 40, 45, 43, 40, 37, 29 };

            int[] goldprice = { 200000, 200000, 200000, 200000, 200000, 500000, 500000, 500000, 500000, 500000, 1000000, 1000000, 1000000, 1000000, 1000000 };
            short[] feather = { 3, 5, 8, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 70 };
            short[] fullmoon = { 1, 3, 5, 7, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30 };
            short[] soul = { 2, 4, 6, 8, 10, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };
            short featherVnum = 2282;
            short fullmoonVnum = 1030;
            short greenSoulVnum = 2283;
            short redSoulVnum = 2284;
            short blueSoulVnum = 2285;
            short dragonSkinVnum = 2511;
            short dragonBloodVnum = 2512;
            short dragonHeartVnum = 2513;

            if (this.InventoryItem.IsFixed)
                return;
            if (Session.Character.Gold < goldprice[this.InventoryItem.Upgrade])
                return;
            if (Session.Character.InventoryList.CountItem(fullmoonVnum) < fullmoon[this.InventoryItem.Upgrade])
                return;
            if (Session.Character.InventoryList.CountItem(featherVnum) < feather[this.InventoryItem.Upgrade])
                return;

            if (this.InventoryItem.Upgrade < 5)
            {
                if (this.InventoryItem.SpLevel > 20)
                {
                    if (ServerManager.GetItem(this.InventoryItem.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(greenSoulVnum) < soul[this.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(greenSoulVnum, (soul[this.InventoryItem.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonSkinVnum) < soul[this.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonSkinVnum, (soul[this.InventoryItem.Upgrade]));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIERED"), 21), 11));

                    return;
                }
            }
            else if (this.InventoryItem.Upgrade < 10)
            {
                if (this.InventoryItem.SpLevel > 40)
                {
                    if (ServerManager.GetItem(this.InventoryItem.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(redSoulVnum) < soul[this.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(redSoulVnum, (soul[this.InventoryItem.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonBloodVnum) < soul[this.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonBloodVnum, (soul[this.InventoryItem.Upgrade]));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIERED"), 40), 11));

                    return;
                }
            }
            else
            {
                if (this.InventoryItem.SpLevel > 50)
                {
                    if (ServerManager.GetItem(this.InventoryItem.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(blueSoulVnum) < soul[this.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(blueSoulVnum, (soul[this.InventoryItem.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonHeartVnum) < soul[this.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonHeartVnum, (soul[this.InventoryItem.Upgrade]));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIERED"), 51), 11));

                    return;
                }
            }
            Random r = new Random();
            int rnd = r.Next(100);
            if (rnd <= upfail[this.InventoryItem.Upgrade])
            {
                if (protect ==UpgradeProtection.Protected)
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));

                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 11));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 0));
            }
            else if (rnd <= upsuccess[this.InventoryItem.Upgrade])
            {
                if (protect ==UpgradeProtection.Protected)
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 0));
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Upgrade++;
            }
            else
            {
                if (protect ==UpgradeProtection.Protected)
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.IsFixed = true;
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 0));
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROY"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROY"), 0));
                }
            }
            Session.Character.Gold = Session.Character.Gold - goldprice[this.InventoryItem.Upgrade];
            Session.Client.SendPacket(Session.Character.GenerateGold());
            Session.Character.InventoryList.RemoveItemAmount(featherVnum, (feather[this.InventoryItem.Upgrade]));
            Session.Character.InventoryList.RemoveItemAmount(fullmoonVnum, (fullmoon[this.InventoryItem.Upgrade]));
            Session.Character.GetStartupInventory(Session);
            Session.Client.SendPacket("shop_end 1");
        }

        public void PerfectSP(ClientSession Session, UpgradeProtection protect)
        {
            short[] upsuccess = { 50, 40, 30, 20, 10 };

            int[] goldprice = { 5000, 1000, 20000, 50000, 100000 };
            short[] stoneprice = { 1, 2, 3, 4, 5 };
            short stonevnum;
            byte upmode = 1;

            switch (ServerManager.GetItem(this.InventoryItem.ItemVNum).Morph)
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
            if (this.InventoryItem.SpStoneUpgrade > 99)
                return;
            if (this.InventoryItem.SpStoneUpgrade > 80)
            {
                upmode = 5;
            }
            if (this.InventoryItem.SpStoneUpgrade > 60)
            {
                upmode = 4;
            }
            if (this.InventoryItem.SpStoneUpgrade > 40)
            {
                upmode = 3;
            }
            if (this.InventoryItem.SpStoneUpgrade > 20)
            {
                upmode = 2;
            }

            if (this.InventoryItem.IsFixed)
                return;
            if (Session.Character.Gold < goldprice[upmode])
                return;
            if (Session.Character.InventoryList.CountItem(stonevnum) < stoneprice[upmode])
                return;

            Random r = new Random();
            int rnd = r.Next(100);
            if (rnd <= upsuccess[upmode])
            {
                byte type = (byte)r.Next(16), count = 1;

                if (upmode == 4)
                {
                    count = 2;
                }
                if (count == 5)
                {
                    count = (byte)r.Next(3, 6);
                }

                Session.Client.SendPacket(Session.Character.GenerateEff(3005));

                if (type < 3)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpDamage += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 0));
                }
                else if (type < 6)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpDefence += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 0));
                }
                else if (type < 9)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpElement += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 0));
                }
                else if (type < 12)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpHP += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 0));
                }
                else if (type == 12)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpFire += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 0));
                }
                else if (type == 13)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpWater += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 0));
                }
                else if (type == 14)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpLight += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 0));
                }
                else if (type == 15)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpDark += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 0));
                }
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.SpStoneUpgrade++;
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PERFECTSP_FAILED"), 11));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("PERFECTSP_FAILED"), 0));
            }
            Session.Character.Gold = Session.Character.Gold - goldprice[upmode];
            Session.Client.SendPacket(Session.Character.GenerateGold());
            Session.Character.InventoryList.RemoveItemAmount(stonevnum, stoneprice[upmode]);
            Session.Character.GetStartupInventory(Session);
        }

        public void RarifyItem(ClientSession Session, RarifyMode mode, RarifyProtection protection)
        {
            double rare1 = 50;
            double rare2 = 35;
            double rare3 = 25;
            double rare4 = 10;
            double rare5 = 10;
            double rare6 = 5;
            double rare7 = 1;
            double reducedchancefactor = 1.1;

            //short itempricevnum = 0;
            short goldprice = 500;
            double reducedpricefactor = 0.5;

            byte cella = 5;
            int cellavnum = 1014;

            if (protection == RarifyProtection.RedAmulet)
            {
                rare1 = rare1 * reducedchancefactor;
                rare2 = rare2 * reducedchancefactor;
                rare3 = rare3 * reducedchancefactor;
                rare4 = rare4 * reducedchancefactor;
                rare5 = rare5 * reducedchancefactor;
                rare6 = rare6 * reducedchancefactor;
                rare7 = rare7 * reducedchancefactor;
            }
            switch (mode)
            {
                case RarifyMode.Free:
                    break;

                case RarifyMode.Reduced:
                    // TODO: Reduced Item Amount
                    if (Session.Character.Gold < goldprice * reducedpricefactor)
                        return;
                    Session.Character.Gold = Session.Character.Gold - (long)(goldprice * reducedpricefactor);
                    if (Session.Character.InventoryList.CountItem(cellavnum) < cella * reducedpricefactor)
                        return;
                    Session.Character.InventoryList.RemoveItemAmount(cellavnum, (int)(cella * reducedpricefactor));
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    break;

                case RarifyMode.Normal:
                    // TODO: Normal Item Amount
                    if (Session.Character.Gold < goldprice)
                        return;
                    Session.Character.Gold = Session.Character.Gold - goldprice;
                    if (Session.Character.InventoryList.CountItem(cellavnum) < cella)
                        return;
                    Session.Character.InventoryList.RemoveItemAmount(cellavnum, cella);
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    break;
            }

            Random r = new Random();
            int rnd = r.Next(100);
            if (rnd <= rare7 && !(protection == RarifyProtection.Scroll && this.InventoryItem.Rare >= 7))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 7), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 7), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Rare = 7;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare6 && !(protection == RarifyProtection.Scroll && this.InventoryItem.Rare >= 6))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 6), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 6), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Rare = 6;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare5 && !(protection == RarifyProtection.Scroll && this.InventoryItem.Rare >= 5))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 5), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 5), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Rare = 5;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare4 && !(protection == RarifyProtection.Scroll && this.InventoryItem.Rare >= 4))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 4), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 4), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Rare = 4;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare3 && !(protection == RarifyProtection.Scroll && this.InventoryItem.Rare >= 3))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 3), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 3), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Rare = 3;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare2 && !(protection == RarifyProtection.Scroll && this.InventoryItem.Rare >= 2))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 2), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 2), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Rare = 2;

                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare1 && !(protection == RarifyProtection.Scroll && this.InventoryItem.Rare >= 1))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 1), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 1), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.Rare = 1;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else
            {
                if (protection == RarifyProtection.None)
                {
                    Session.Character.DeleteItem(Session, this.Type, this.Slot);
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 0));
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByInventoryItem(this.InventoryItem.InventoryItemId).InventoryItem.IsFixed = true;
                }
            }
            Session.Character.GetStartupInventory(Session);
            Session.Client.SendPacket("shop_end 1");
        }
    
        public void SumItem(ClientSession Session, Inventory item2)
        {
            short[] upsuccess = { 100, 100, 85, 70, 50, 20 };
            int[] goldprice = { 1500, 3000, 6000, 12000, 24000, 48000 };
            short[] sand = { 5, 10, 15, 20, 25, 30 };
            int sandVnum = 1027;
            Item iteminfo = ServerManager.GetItem(this.InventoryItem.ItemVNum);
            Item iteminfo2 = ServerManager.GetItem(item2.InventoryItem.ItemVNum);
            if ((this.InventoryItem.Upgrade + item2.InventoryItem.Upgrade) < 6 && ((iteminfo2.EquipmentSlot == (byte)EquipmentType.Gloves) && (iteminfo2.EquipmentSlot == (byte)EquipmentType.Gloves)) || ((iteminfo.EquipmentSlot == (byte)EquipmentType.Boots) && (iteminfo2.EquipmentSlot == (byte)EquipmentType.Boots)))
            {
                if (Session.Character.Gold < goldprice[this.InventoryItem.Upgrade])
                    return;
                Session.Character.Gold = Session.Character.Gold - (goldprice[this.InventoryItem.Upgrade]);
                if (Session.Character.InventoryList.CountItem(sandVnum) < sand[this.InventoryItem.Upgrade])
                    return;
                Session.Character.InventoryList.RemoveItemAmount(sandVnum, (byte)(sand[this.InventoryItem.Upgrade]));

                Random r = new Random();
                int rnd = r.Next(100);
                if (rnd <= upsuccess[this.InventoryItem.Upgrade + item2.InventoryItem.Upgrade])
                {
                    this.InventoryItem.Upgrade += (byte)(item2.InventoryItem.Upgrade + 1);
                    this.InventoryItem.DarkResistance += (byte)(item2.InventoryItem.DarkResistance + iteminfo2.DarkResistance);
                    this.InventoryItem.LightResistance += (byte)(item2.InventoryItem.LightResistance + iteminfo2.LightResistance);
                    this.InventoryItem.WaterResistance += (byte)(item2.InventoryItem.WaterResistance + iteminfo2.WaterResistance);
                    this.InventoryItem.FireResistance += (byte)(item2.InventoryItem.FireResistance + iteminfo2.FireResistance);
                    Session.Character.DeleteItem(Session, item2.Type, item2.Slot);
                    Session.Client.SendPacket($"pdti 10 {this.InventoryItem.ItemVNum} 1 27 {this.InventoryItem.Upgrade} 0");
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 12));
                    Session.Client.SendPacket($"guri 19 1 {Session.Character.CharacterId} 1324");
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    Session.Character.GetStartupInventory(Session);
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_FAILED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_FAILED"), 11));
                    Session.Client.SendPacket($"guri 19 1 {Session.Character.CharacterId} 1332");
                    Session.Character.DeleteItem(Session, item2.Type, item2.Slot);
                    Session.Character.DeleteItem(Session, this.Type, this.Slot);
                }
                Session.Client.SendPacket("shop_end 1");
            }
        }


        public void Save()
        {
            InventoryDTO tempsave = this;
            SaveResult insertResult = DAOFactory.InventoryDAO.InsertOrUpdate(ref tempsave);
        }

        #endregion
    }
}