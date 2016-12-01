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
    public class WearableInstance : ItemInstance, IWearableInstance
    {
        #region Members

        private Random _random;

        #endregion

        #region Instantiation

        public WearableInstance()
        {
            _random = new Random();
        }

        public WearableInstance(Guid id)
        {
            Id = id;
            _random = new Random();
        }

        public WearableInstance(short vNum, byte amount) : base(vNum, amount)
        {
            _random = new Random();
        }

        #endregion

        #region Properties

        public byte Ammo { get; set; }

        public byte Cellon { get; set; }

        public short CloseDefence { get; set; }

        public short Concentrate { get; set; }

        public short CriticalDodge { get; set; }

        public byte CriticalLuckRate { get; set; }

        public short CriticalRate { get; set; }

        public short DamageMaximum { get; set; }

        public short DamageMinimum { get; set; }

        public byte DarkElement { get; set; }

        public short DarkResistance { get; set; }

        public short DefenceDodge { get; set; }

        public short DistanceDefence { get; set; }

        public short DistanceDefenceDodge { get; set; }

        public short ElementRate { get; set; }

        public byte FireElement { get; set; }

        public short FireResistance { get; set; }

        public short HitRate { get; set; }

        public short HP { get; set; }

        public bool IsEmpty { get; set; }

        public bool IsFixed { get; set; }

        public byte LightElement { get; set; }

        public short LightResistance { get; set; }

        public short MagicDefence { get; set; }

        public byte MaxElementRate { get; set; }

        public short MP { get; set; }

        public byte WaterElement { get; set; }

        public short WaterResistance { get; set; }

        public long XP { get; set; }

        #endregion

        #region Methods

        public override void Initialize()
        {
            _random = new Random();
        }

        public void RarifyItem(ClientSession Session, RarifyMode mode, RarifyProtection protection, bool isCommand = false)
        {
            double raren2 = 80;
            double raren1 = 70;
            double rare0 = 60;
            double rare1 = 40;
            double rare2 = 30;
            double rare3 = 15;
            double rare4 = 10;
            double rare5 = 5;
            double rare6 = 3;
            double rare7 = 2;
            double rare8 = 1;
            short goldprice = 500;
            double reducedpricefactor = 0.5;
            double reducedchancefactor = 1.1;
            byte cella = 5;
            int cellaVnum = 1014;
            int scrollVnum = 1218;
            int rnd;

            if (!Session.HasCurrentMap)
            {
                return;
            }
            if (mode != RarifyMode.Drop || this.Item.ItemType == ItemType.Shell)
            {
                raren2 = 0;
                raren1 = 0;
                rare0 = 0;
                rnd = _random.Next(0, 80);
            }
            else
            {
                rnd = _random.Next(0, 100);
            }
            if (protection == RarifyProtection.RedAmulet)
            {
                raren2 = raren1 * reducedchancefactor;
                raren1 = raren1 * reducedchancefactor;
                rare0 = rare0 * reducedchancefactor;
                rare1 = rare1 * reducedchancefactor;
                rare2 = rare2 * reducedchancefactor;
                rare3 = rare3 * reducedchancefactor;
                rare4 = rare4 * reducedchancefactor;
                rare5 = rare5 * reducedchancefactor;
                rare6 = rare6 * reducedchancefactor;
                rare7 = rare7 * reducedchancefactor;
                rare8 = rare8 * reducedchancefactor;

                // rare8 = rare8 * reducedchancefactor;
            }
            switch (mode)
            {
                case RarifyMode.Free:
                    break;

                case RarifyMode.Reduced:

                    // TODO: Reduced Item Amount
                    if (Session.Character.Gold < (long)(goldprice * reducedpricefactor))
                    {
                        return;
                    }
                    if (Session.Character.Inventory.CountItem(cellaVnum) < cella * reducedpricefactor)
                    {
                        return;
                    }
                    Session.Character.Inventory.RemoveItemAmount(cellaVnum, (int)(cella * reducedpricefactor));
                    Session.Character.Gold -= (long)(goldprice * reducedpricefactor);
                    Session.SendPacket(Session.Character.GenerateGold());
                    break;

                case RarifyMode.Normal:

                    // TODO: Normal Item Amount
                    if (Session.Character.Gold < goldprice)
                    {
                        return;
                    }
                    if (Session.Character.Inventory.CountItem(cellaVnum) < cella)
                    {
                        return;
                    }
                    if (protection == RarifyProtection.Scroll && !isCommand && Session.Character.Inventory.CountItem(scrollVnum) < 1)
                    {
                        return;
                    }

                    if (protection == RarifyProtection.Scroll && !isCommand)
                    {
                        Session.Character.Inventory.RemoveItemAmount(scrollVnum);
                        Session.SendPacket("shop_end 2");
                    }
                    Session.Character.Gold -= goldprice;
                    Session.Character.Inventory.RemoveItemAmount(cellaVnum, cella);
                    Session.SendPacket(Session.Character.GenerateGold());
                    break;
            }
            if (this.Item.IsHeroic && protection == RarifyProtection.Scroll)
            {
                if (rnd <= rare8 && !(protection == RarifyProtection.Scroll && this.Rare >= 8))
                {
                    if (mode != RarifyMode.Drop)
                    {
                        Session.Character.NotifyRarifyResult(8);
                    }

                    this.Rare = 8;
                    SetRarityPoint();
                    ItemInstance inventory = Session.Character.Inventory.GetItemInstanceById(this.Id);
                    if (inventory != null)
                    {
                        Session.SendPacket(Session.Character.GenerateInventoryAdd(this.ItemVNum, 1, inventory.Type, inventory.Slot, inventory.Rare, 0, inventory.Upgrade, 0));
                    }
                    return;
                }
            }
            if (rnd <= rare7 && !(protection == RarifyProtection.Scroll && this.Rare >= 7))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(7);
                }

                this.Rare = 7;
                SetRarityPoint();
            }
            else if (rnd <= rare6 && !(protection == RarifyProtection.Scroll && this.Rare >= 6))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(6);
                }
                this.Rare = 6;
                SetRarityPoint();
            }
            else if (rnd <= rare5 && !(protection == RarifyProtection.Scroll && this.Rare >= 5))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(5);
                }
                this.Rare = 5;
                SetRarityPoint();
            }
            else if (rnd <= rare4 && !(protection == RarifyProtection.Scroll && this.Rare >= 4))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(4);
                }
                this.Rare = 4;
                SetRarityPoint();
            }
            else if (rnd <= rare3 && !(protection == RarifyProtection.Scroll && this.Rare >= 3))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(3);
                }
                this.Rare = 3;
                SetRarityPoint();
            }
            else if (rnd <= rare2 && !(protection == RarifyProtection.Scroll && this.Rare >= 2))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(2);
                }
                this.Rare = 2;
                SetRarityPoint();
            }
            else if (rnd <= rare1 && !(protection == RarifyProtection.Scroll && this.Rare >= 1))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(1);
                }
                this.Rare = 1;
                SetRarityPoint();
            }
            else if (rnd <= rare0 && !(protection == RarifyProtection.Scroll && this.Rare >= 0))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(0);
                }
                this.Rare = 0;
                SetRarityPoint();
            }
            else if (rnd <= raren1 && !(protection == RarifyProtection.Scroll && this.Rare >= -1))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(-1);
                }
                this.Rare = -1;
                SetRarityPoint();
            }
            else if (rnd <= raren2 && !(protection == RarifyProtection.Scroll && this.Rare >= -2))
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(-2);
                }
                this.Rare = -2;
                SetRarityPoint();
            }
            else
            {
                if (mode != RarifyMode.Drop)
                {
                    if (protection == RarifyProtection.None)
                    {
                        Session.Character.DeleteItemByItemInstanceId(this.Id);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 11));
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 0));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 11));
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 0));
                        Session.CurrentMap.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                    }
                }
            }

            // don't place under else.
            if (mode != RarifyMode.Drop)
            {
                ItemInstance inventory = Session.Character.Inventory.GetItemInstanceById(this.Id);
                if (inventory != null)
                {
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(this.ItemVNum, 1, inventory.Type, inventory.Slot, inventory.Rare, 0, inventory.Upgrade, 0));
                }
            }
        }

        public void SetRarityPoint()
        {
            if (this.Item.EquipmentSlot == EquipmentType.MainWeapon || this.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
            {
                int point = CharacterHelper.RarityPoint(this.Rare, (this.Item.IsHeroic ? (short)(95 + this.Item.LevelMinimum) : this.Item.LevelMinimum));
                this.Concentrate = 0;
                this.HitRate = 0;
                this.DamageMinimum = 0;
                this.DamageMaximum = 0;
                if (this.Rare >= 0)
                {
                    for (int i = 0; i < point; i++)
                    {
                        int rndn = _random.Next(0, 3);
                        if (rndn == 0)
                        {
                            this.Concentrate++;
                            this.HitRate++;
                        }
                        else
                        {
                            this.DamageMinimum++;
                            this.DamageMaximum++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i > point; i--)
                    {
                        int rndn = _random.Next(0, 3);
                        if (rndn == 0)
                        {
                            this.Concentrate--;
                            this.HitRate--;
                        }
                        else
                        {
                            this.DamageMinimum--;
                            this.DamageMaximum--;
                        }
                    }
                }
            }
            else if (this.Item.EquipmentSlot == EquipmentType.Armor)
            {
                int point = CharacterHelper.RarityPoint(this.Rare, (this.Item.IsHeroic ? (short)(95 + this.Item.LevelMinimum) : this.Item.LevelMinimum));
                this.DefenceDodge = 0;
                this.DistanceDefenceDodge = 0;
                this.DistanceDefence = 0;
                this.MagicDefence = 0;
                this.CloseDefence = 0;
                if (this.Rare >= 0)
                {
                    for (int i = 0; i < point; i++)
                    {
                        int rndn = _random.Next(0, 3);
                        if (rndn == 0)
                        {
                            this.DefenceDodge++;
                            this.DistanceDefenceDodge++;
                        }
                        else
                        {
                            this.DistanceDefence++;
                            this.MagicDefence++;
                            this.CloseDefence++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i > point; i--)
                    {
                        int rndn = _random.Next(0, 3);
                        if (rndn == 0)
                        {
                            this.DefenceDodge--;
                            this.DistanceDefenceDodge--;
                        }
                        else
                        {
                            this.DistanceDefence--;
                            this.MagicDefence--;
                            this.CloseDefence--;
                        }
                    }
                }
            }
        }

        public void Sum(ClientSession Session, WearableInstance itemToSum)
        {
            if (!Session.HasCurrentMap)
            {
                return;
            }
            // cannot sum higher than 5
            if (this.Upgrade < 6)
            {
                short[] upsuccess = { 100, 100, 85, 70, 50, 20 };
                int[] goldprice = { 1500, 3000, 6000, 12000, 24000, 48000 };
                short[] sand = { 5, 10, 15, 20, 25, 30 };
                int sandVnum = 1027;
                if ((this.Upgrade + itemToSum.Upgrade) < 6 && ((((itemToSum.Item.EquipmentSlot == EquipmentType.Gloves) && (this.Item.EquipmentSlot == EquipmentType.Gloves)) || ((this.Item.EquipmentSlot == EquipmentType.Boots) && (itemToSum.Item.EquipmentSlot == EquipmentType.Boots)))))
                {
                    if (Session.Character.Gold < goldprice[this.Upgrade])
                    {
                        return;
                    }
                    if (Session.Character.Inventory.CountItem(sandVnum) < sand[this.Upgrade])
                    {
                        return;
                    }
                    Session.Character.Inventory.RemoveItemAmount(sandVnum, (byte)(sand[this.Upgrade]));
                    Session.Character.Gold -= goldprice[this.Upgrade];

                    int rnd = _random.Next(100);
                    if (rnd <= upsuccess[this.Upgrade + itemToSum.Upgrade])
                    {
                        this.Upgrade += (byte)(itemToSum.Upgrade + 1);
                        this.DarkResistance += (short)(itemToSum.DarkResistance + itemToSum.Item.DarkResistance);
                        this.LightResistance += (short)(itemToSum.LightResistance + itemToSum.Item.LightResistance);
                        this.WaterResistance += (short)(itemToSum.WaterResistance + itemToSum.Item.WaterResistance);
                        this.FireResistance += (short)(itemToSum.FireResistance + itemToSum.Item.FireResistance);
                        Session.Character.DeleteItemByItemInstanceId(itemToSum.Id);
                        Session.SendPacket($"pdti 10 {this.ItemVNum} 1 27 {this.Upgrade} 0");
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 0));
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 12));
                        Session.SendPacket(Session.Character.GenerateGuri(19, 1, 1324));
                        ItemInstance itemInstnace = Session.Character.Inventory.GetItemInstanceById(this.Id);
                        Session.SendPacket(Session.Character.GenerateInventoryAdd(itemInstnace.ItemVNum, 1, itemInstnace.Type, itemInstnace.Slot, 0, 0, this.Upgrade, 0));                        
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_FAILED"), 0));
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_FAILED"), 11));
                        Session.SendPacket(Session.Character.GenerateGuri(19, 1, 1332));
                        Session.Character.DeleteItemByItemInstanceId(itemToSum.Id);
                        Session.Character.DeleteItemByItemInstanceId(this.Id);
                    }
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateGuri(6, 1), Session.Character.MapX, Session.Character.MapY); 
                    Session.SendPacket(Session.Character.GenerateGold());
                    Session.SendPacket("shop_end 1");
                }
            }
        }

        public void UpgradeItem(ClientSession Session, UpgradeMode mode, UpgradeProtection protection, bool isCommand = false)
        {
            if (!Session.HasCurrentMap)
            {
                return;
            }
            if (this.Upgrade < 10)
            {
                short[] upsuccess;
                short[] upfix;
                int[] goldprice;
                short[] cella;
                short[] gem;

                if (this.Rare == 8)
                {
                    upsuccess = new short[] { 50, 50, 45, 30, 20, 10, 5, 3, 2, 1 };
                    upfix = new short[] { 0, 0, 10, 15, 20, 20, 20, 20, 15, 10 };
                    goldprice = new int[] { 5000, 15000, 30000, 100000, 300000, 800000, 1500000, 4000000, 7000000, 10000000 };
                    cella = new short[] { 40, 100, 160, 240, 320, 440, 560, 760, 960, 1200 };
                    gem = new short[] { 2, 2, 4, 4, 6, 2, 2, 4, 4, 6 };
                }
                else
                {
                    upsuccess = new short[] { 100, 100, 100, 95, 80, 60, 40, 30, 20, 11 };
                    upfix = new short[] { 0, 0, 10, 15, 20, 20, 20, 20, 15, 10 };
                    goldprice = new int[] { 500, 1500, 3000, 10000, 30000, 80000, 150000, 400000, 700000, 1000000 };
                    cella = new short[] { 20, 50, 80, 120, 160, 220, 280, 380, 480, 600 };
                    gem = new short[] { 1, 1, 2, 2, 3, 1, 1, 2, 2, 3 };
                }

                short cellaVnum = 1014;
                short gemVnum = 1015;
                short gemFullVnum = 1016;
                double reducedpricefactor = 0.5;
                short normalScrollVnum = 1218;
                short goldScrollVnum = 5369;

                if (this.IsFixed)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_FIXED"), 10));
                    Session.SendPacket("shop_end 1");
                    return;
                }
                switch (mode)
                {
                    case UpgradeMode.Free:
                        break;

                    case UpgradeMode.Reduced:

                        // TODO: Reduced Item Amount
                        if (Session.Character.Gold < (long)(goldprice[this.Upgrade] * reducedpricefactor))
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            return;
                        }
                        if (Session.Character.Inventory.CountItem(cellaVnum) < cella[this.Upgrade] * reducedpricefactor)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(String.Format("NOT_ENOUGH_ITEMS", ServerManager.GetItem(cellaVnum).Name, cella[this.Upgrade] * reducedpricefactor)), 10));
                            return;
                        }
                        if (protection == UpgradeProtection.Protected && !isCommand && Session.Character.Inventory.CountItem(goldScrollVnum) < 1)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(String.Format("NOT_ENOUGH_ITEMS", ServerManager.GetItem(goldScrollVnum).Name, cella[this.Upgrade] * reducedpricefactor)), 10));
                            return;
                        }
                        if (this.Upgrade < 5)
                        {
                            if (Session.Character.Inventory.CountItem(gemVnum) < gem[this.Upgrade])
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(String.Format("NOT_ENOUGH_ITEMS", ServerManager.GetItem(gemVnum).Name, gem[this.Upgrade])), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(gemVnum, gem[this.Upgrade]);
                        }
                        else
                        {
                            if (Session.Character.Inventory.CountItem(gemFullVnum) < gem[this.Upgrade])
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(String.Format("NOT_ENOUGH_ITEMS", ServerManager.GetItem(gemFullVnum).Name, gem[this.Upgrade])), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(gemFullVnum, gem[this.Upgrade]);
                        }
                        if (protection == UpgradeProtection.Protected && !isCommand)
                        {
                            Session.Character.Inventory.RemoveItemAmount(goldScrollVnum);
                            Session.SendPacket("shop_end 2");
                        }
                        Session.Character.Gold -= (long)(goldprice[this.Upgrade] * reducedpricefactor);
                        Session.Character.Inventory.RemoveItemAmount(cellaVnum, (int)(cella[this.Upgrade] * reducedpricefactor));
                        Session.SendPacket(Session.Character.GenerateGold());
                        break;

                    case UpgradeMode.Normal:

                        // TODO: Normal Item Amount
                        if (Session.Character.Inventory.CountItem(cellaVnum) < cella[this.Upgrade])
                        {
                            return;
                        }
                        if (Session.Character.Gold < goldprice[this.Upgrade])
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            return;
                        }
                        if (protection == UpgradeProtection.Protected && !isCommand && Session.Character.Inventory.CountItem(normalScrollVnum) < 1)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(String.Format("NOT_ENOUGH_ITEMS", ServerManager.GetItem(normalScrollVnum).Name, 1)), 10));
                            return;
                        }
                        if (this.Upgrade < 5)
                        {
                            if (Session.Character.Inventory.CountItem(gemVnum) < gem[this.Upgrade])
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(String.Format("NOT_ENOUGH_ITEMS", ServerManager.GetItem(gemVnum).Name, gem[this.Upgrade])), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(gemVnum, (gem[this.Upgrade]));
                        }
                        else
                        {
                            if (Session.Character.Inventory.CountItem(gemFullVnum) < gem[this.Upgrade])
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey(String.Format("NOT_ENOUGH_ITEMS", ServerManager.GetItem(gemFullVnum).Name, gem[this.Upgrade])), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(gemFullVnum, (gem[this.Upgrade]));
                        }
                        if (protection == UpgradeProtection.Protected && !isCommand)
                        {
                            Session.Character.Inventory.RemoveItemAmount(normalScrollVnum);
                            Session.SendPacket("shop_end 2");
                        }
                        Session.Character.Inventory.RemoveItemAmount(cellaVnum, (cella[this.Upgrade]));
                        Session.Character.Gold -= goldprice[this.Upgrade];
                        Session.SendPacket(Session.Character.GenerateGold());
                        break;
                }
                WearableInstance wearable = Session.Character.Inventory.LoadByItemInstance<WearableInstance>(this.Id);
                ItemInstance inventory = Session.Character.Inventory.GetItemInstanceById(this.Id);

                int rnd = _random.Next(1, 100);

                if (this.Rare == 8)
                {                    
                    if (rnd <= upsuccess[this.Upgrade])
                    {
                        Session.CurrentMap.Broadcast(Session.Character.GenerateEff(3005), Session.Character.MapX, Session.Character.MapY);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 0));
                        wearable.Upgrade++;
                        Session.SendPacket(Session.Character.GenerateInventoryAdd(this.ItemVNum, 1, inventory.Type, inventory.Slot, wearable.Rare, 0, wearable.Upgrade, 0));
                    }
                    else if (rnd <= upfix[this.Upgrade])
                    {
                        Session.CurrentMap.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                        wearable.IsFixed = true;
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 0));
                    }
                    else
                    {
                        if (protection == UpgradeProtection.None)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 11));
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 0));
                            Session.Character.DeleteItemByItemInstanceId(this.Id);
                        }
                        else
                        {
                            Session.CurrentMap.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SCROLL_PROTECT_USED"), 11));
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
                        }
                    }
                }
                else
                {
                    if (rnd <= upfix[this.Upgrade])
                    {
                        Session.CurrentMap.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                        wearable.IsFixed = true;
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 0));
                    }
                    else if (rnd <= upsuccess[this.Upgrade])
                    {
                        Session.CurrentMap.Broadcast(Session.Character.GenerateEff(3005), Session.Character.MapX, Session.Character.MapY);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 0));
                        wearable.Upgrade++;
                        Session.SendPacket(Session.Character.GenerateInventoryAdd(this.ItemVNum, 1, inventory.Type, inventory.Slot, wearable.Rare, 0, wearable.Upgrade, 0));
                    }
                    else
                    {
                        if (protection == UpgradeProtection.None)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 11));
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 0));
                            Session.Character.DeleteItemByItemInstanceId(this.Id);
                        }
                        else
                        {
                            Session.CurrentMap.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SCROLL_PROTECT_USED"), 11));
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
                        }
                    }
                }
            }
            else
            {
                Session.CurrentMap.Broadcast(Session.Character.GenerateEff(3004), Session.Character.MapX, Session.Character.MapY);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SCROLL_PROTECT_USED"), 11));
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
            }
            Session.SendPacket("shop_end 1");
        }

        #endregion
    }
}