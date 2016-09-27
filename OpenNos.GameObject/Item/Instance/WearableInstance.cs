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
        #region Instantiation

        public WearableInstance()
        {
        }

        public WearableInstance(Guid id)
        {
            Id = id;
        }

        public WearableInstance(WearableInstanceDTO wearableInstance)
        {
            XP = wearableInstance.XP;
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

        public byte DarkResistance { get; set; }

        public short DefenceDodge { get; set; }

        public short DistanceDefence { get; set; }

        public short DistanceDefenceDodge { get; set; }

        public short ElementRate { get; set; }

        public byte FireElement { get; set; }

        public byte FireResistance { get; set; }

        public short HitRate { get; set; }

        public short HP { get; set; }

        public bool IsEmpty { get; set; }

        public bool IsFixed { get; set; }

        public byte LightElement { get; set; }

        public byte LightResistance { get; set; }

        public short MagicDefence { get; set; }

        public byte MaxElementRate { get; set; }

        public short MP { get; set; }

        public byte WaterElement { get; set; }

        public byte WaterResistance { get; set; }

        public long XP { get; set; }

        #endregion

        #region Methods

        public void RarifyItem(ClientSession Session, RarifyMode mode, RarifyProtection protection)
        {
            double raren2 = 80;
            double raren1 = 70;
            double rare0 = 60;
            double rare1 = 40;
            double rare2 = 30;
            double rare3 = 15;
            double rare4 = 10;
            double rare5 = 5;
            double rare6 = 2;
            double rare7 = 0;

            //double rare8 = 0; //disabled for now
            double reducedchancefactor = 1.1;

            //short itempricevnum = 0;
            short goldprice = 500;
            double reducedpricefactor = 0.5;

            byte cella = 5;
            int cellavnum = 1014;

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

                //rare8 = rare8 * reducedchancefactor;
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
                    Session.SendPacket(Session.Character.GenerateGold());
                    break;

                case RarifyMode.Normal:

                    // TODO: Normal Item Amount
                    if (Session.Character.Gold < goldprice)
                        return;
                    Session.Character.Gold = Session.Character.Gold - goldprice;
                    if (Session.Character.InventoryList.CountItem(cellavnum) < cella)
                        return;
                    Session.Character.InventoryList.RemoveItemAmount(cellavnum, cella);
                    Session.SendPacket(Session.Character.GenerateGold());
                    break;
            }

            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            int rnd = r.Next(0, 100);

            //if (rnd <= rare8 && !(protection == RarifyProtection.Scroll && this.Rare >= 8))
            //{
            //    if (mode != RarifyMode.Drop)
            //    {
            //        Session.Character.NotifyRarifyResult(8);
            //    }

            //    this.Rare = 8;
            //    SetRarityPoint();
            //}
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
            else if (rnd <= rare0 && !(protection == RarifyProtection.Scroll && this.Rare >= 0) && mode == RarifyMode.Drop && this.Item.ItemType != (byte)ItemType.Shell)
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(0);
                }
                this.Rare = 0;
                SetRarityPoint();
            }
            else if (rnd <= raren1 && !(protection == RarifyProtection.Scroll && this.Rare >= -1) && mode == RarifyMode.Drop && this.Item.ItemType != (byte)ItemType.Shell)
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(-1);
                }
                this.Rare = -1;
                SetRarityPoint();
            }
            else if (rnd <= raren2 && !(protection == RarifyProtection.Scroll && this.Rare >= -2) && mode == RarifyMode.Drop && this.Item.ItemType != (byte)ItemType.Shell)
            {
                if (mode != RarifyMode.Drop)
                {
                    Session.Character.NotifyRarifyResult(-2);
                }
                this.Rare = -2;
                SetRarityPoint();
            }
            else if (this.Rare == 0 && this.Item.ItemType == (byte)ItemType.Shell)
            {
                this.Rare = 1;
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
                        ServerManager.Instance.Broadcast(Session.Character.GenerateEff(3004));
                    }
                }

                if (mode != RarifyMode.Drop)
                {
                    Inventory inventory = Session.Character.InventoryList.GetInventoryByItemInstanceId(this.Id);
                    if (inventory != null)
                        Session.SendPacket(Session.Character.GenerateInventoryAdd(this.ItemVNum, 1, inventory.Type, inventory.Slot, inventory.ItemInstance.Rare, 0, inventory.ItemInstance.Upgrade, 0));
                }
            }
        }

        public void SetRarityPoint()
        {
            if (this.Item.EquipmentSlot == (byte)EquipmentType.MainWeapon || this.Item.EquipmentSlot == (byte)EquipmentType.SecondaryWeapon)
            {
                int point = ServersData.RarityPoint(this.Rare, (this.Item.IsHeroic ? (short)(95 + this.Item.LevelMinimum) : this.Item.LevelMinimum));
                Random rnd = new Random();
                this.Concentrate = 0;
                this.HitRate = 0;
                this.DamageMinimum = 0;
                this.DamageMaximum = 0;
                for (int i = 0; i < point; i++)
                {
                    int rndn = rnd.Next(0, 3);
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
            else if (this.Item.EquipmentSlot == (byte)EquipmentType.Armor)
            {
                int point = ServersData.RarityPoint(this.Rare, this.Item.LevelMinimum);
                Random rnd = new Random();
                this.DefenceDodge = 0;
                this.DistanceDefenceDodge = 0;
                this.DistanceDefence = 0;
                this.MagicDefence = 0;
                this.CloseDefence = 0;
                for (int i = 0; i < point; i++)
                {
                    int rndn = rnd.Next(0, 3);
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
        }

        public void SumItem(ClientSession Session, WearableInstance itemToSum)
        {
            short[] upsuccess = { 100, 100, 85, 70, 50, 20 };
            int[] goldprice = { 1500, 3000, 6000, 12000, 24000, 48000 };
            short[] sand = { 5, 10, 15, 20, 25, 30 };
            int sandVnum = 1027;
            if ((this.Upgrade + itemToSum.Upgrade) < 6 && ((((itemToSum.Item.EquipmentSlot == (byte)EquipmentType.Gloves) && (this.Item.EquipmentSlot == (byte)EquipmentType.Gloves)) || ((this.Item.EquipmentSlot == (byte)EquipmentType.Boots) && (itemToSum.Item.EquipmentSlot == (byte)EquipmentType.Boots)))))
            {
                if (Session.Character.Gold < goldprice[this.Upgrade])
                    return;
                Session.Character.Gold = Session.Character.Gold - (goldprice[this.Upgrade]);
                if (Session.Character.InventoryList.CountItem(sandVnum) < sand[this.Upgrade])
                    return;
                Session.Character.InventoryList.RemoveItemAmount(sandVnum, (byte)(sand[this.Upgrade]));

                Random r = new Random();
                int rnd = r.Next(100);
                if (rnd <= upsuccess[this.Upgrade + itemToSum.Upgrade])
                {
                    this.Upgrade += (byte)(itemToSum.Upgrade + 1);
                    this.DarkResistance += (byte)(itemToSum.DarkResistance + itemToSum.Item.DarkResistance);
                    this.LightResistance += (byte)(itemToSum.LightResistance + itemToSum.Item.LightResistance);
                    this.WaterResistance += (byte)(itemToSum.WaterResistance + itemToSum.Item.WaterResistance);
                    this.FireResistance += (byte)(itemToSum.FireResistance + itemToSum.Item.FireResistance);
                    Session.Character.DeleteItemByItemInstanceId(itemToSum.Id);
                    Session.SendPacket($"pdti 10 {this.ItemVNum} 1 27 {this.Upgrade} 0");
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 0));
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 12));
                    Session.SendPacket(Session.Character.GenerateGuri(19, 1, 1324));
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateGuri(6, 1), ReceiverType.All);
                    Inventory inventory = Session.Character.InventoryList.GetInventoryByItemInstanceId(this.Id);
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(inventory.ItemInstance.ItemVNum, 1, inventory.Type, inventory.Slot, 0, 0, this.Upgrade, 0));
                    Session.SendPacket(Session.Character.GenerateGold());
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_FAILED"), 0));
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_FAILED"), 11));
                    Session.SendPacket(Session.Character.GenerateGuri(19, 1, 1332));
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateGuri(6, 1), ReceiverType.All);
                    Session.Character.DeleteItemByItemInstanceId(itemToSum.Id);
                    Session.Character.DeleteItemByItemInstanceId(this.Id);
                }
                Session.SendPacket("shop_end 1");
            }
        }

        public void UpgradeItem(ClientSession Session, UpgradeMode mode, UpgradeProtection protection)
        {
            if (this.Upgrade < 10)
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
                        if (Session.Character.Gold < goldprice[this.Upgrade] * reducedpricefactor)
                            return;
                        Session.Character.Gold = Session.Character.Gold - (long)(goldprice[this.Upgrade] * reducedpricefactor);
                        if (Session.Character.InventoryList.CountItem(cellaVnum) < cella[this.Upgrade] * reducedpricefactor)
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(cellaVnum, (int)(cella[this.Upgrade] * reducedpricefactor));
                        if (this.Upgrade <= 5)
                        {
                            if (Session.Character.InventoryList.CountItem(gemVnum) < gem[this.Upgrade] * reducedpricefactor)
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemVnum, (int)(gem[this.Upgrade] * reducedpricefactor));
                        }
                        else
                        {
                            if (Session.Character.InventoryList.CountItem(gemFullVnum) < gem[this.Upgrade] * reducedpricefactor)
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemFullVnum, (int)(gem[this.Upgrade] * reducedpricefactor));
                        }
                        Session.SendPacket(Session.Character.GenerateGold());
                        break;

                    case UpgradeMode.Normal:

                        // TODO: Normal Item Amount
                        if (Session.Character.Gold < goldprice[this.Upgrade])
                            return;
                        Session.Character.Gold = Session.Character.Gold - goldprice[this.Upgrade];
                        if (Session.Character.InventoryList.CountItem(cellaVnum) < cella[this.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(cellaVnum, (cella[this.Upgrade]));
                        if (this.Upgrade < 5)
                        {
                            if (Session.Character.InventoryList.CountItem(gemVnum) < gem[this.Upgrade])
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemVnum, (gem[this.Upgrade]));
                        }
                        else
                        {
                            if (Session.Character.InventoryList.CountItem(gemFullVnum) < gem[this.Upgrade])
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemFullVnum, (gem[this.Upgrade]));
                        }
                        Session.SendPacket(Session.Character.GenerateGold());
                        break;
                }
                WearableInstance wearable = Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.Id);
                Inventory inventory = Session.Character.InventoryList.GetInventoryByItemInstanceId(this.Id);
                Random r = new Random();
                int rnd = r.Next(100);
                if (rnd <= upfix[this.Upgrade])
                {
                    ServerManager.Instance.Broadcast(Session, Session.Character.GenerateEff(3004), ReceiverType.All);
                    wearable.IsFixed = true;
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 0));
                }
                else if (rnd <= upsuccess[this.Upgrade])
                {
                    ServerManager.Instance.Broadcast(Session, Session.Character.GenerateEff(3005), ReceiverType.All);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 0));
                    wearable.Upgrade++;
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(this.ItemVNum, 1, inventory.Type, inventory.Slot, this.Rare, 0, this.Upgrade, 0));
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
                        ServerManager.Instance.Broadcast(Session, Session.Character.GenerateEff(3004), ReceiverType.All);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SCROLL_PROTECT_USED"), 11));
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
                    }
                }
            }
            else
            {
                ServerManager.Instance.Broadcast(Session, Session.Character.GenerateEff(3004), ReceiverType.All);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SCROLL_PROTECT_USED"), 11));
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
            }
            Session.SendPacket("shop_end 1");
        }

        #endregion
    }
}