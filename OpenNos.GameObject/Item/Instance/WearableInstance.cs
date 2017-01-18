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

using System;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;

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

        public void RarifyItem(ClientSession session, RarifyMode mode, RarifyProtection protection,
            bool isCommand = false)
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
            const short goldprice = 500;
            const double reducedpricefactor = 0.5;
            const double reducedchancefactor = 1.1;
            const byte cella = 5;
            const int cellaVnum = 1014;
            const int scrollVnum = 1218;
            int rnd;

            if (session != null && !session.HasCurrentMapInstance)
            {
                return;
            }
            if (mode != RarifyMode.Drop || Item.ItemType == ItemType.Shell)
            {
                raren2 = 0;
                raren1 = 0;
                rare0 = 0;
                rnd = ServerManager.RandomNumber(0, 80);
            }
            else
            {
                rnd = ServerManager.RandomNumber();
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
            if (session != null)
            {
                switch (mode)
                {
                    case RarifyMode.Free:
                        break;

                    case RarifyMode.Reduced:

                        // TODO: Reduced Item Amount
                        if (session.Character.Gold < (long)(goldprice * reducedpricefactor))
                        {
                            return;
                        }
                        if (session.Character.Inventory.CountItem(cellaVnum) < cella * reducedpricefactor)
                        {
                            return;
                        }
                        session.Character.Inventory.RemoveItemAmount(cellaVnum, (int)(cella * reducedpricefactor));
                        session.Character.Gold -= (long)(goldprice * reducedpricefactor);
                        session.SendPacket(session.Character.GenerateGold());
                        break;

                    case RarifyMode.Normal:

                        // TODO: Normal Item Amount
                        if (session.Character.Gold < goldprice)
                        {
                            return;
                        }
                        if (session.Character.Inventory.CountItem(cellaVnum) < cella)
                        {
                            return;
                        }
                        if (protection == RarifyProtection.Scroll && !isCommand &&
                            session.Character.Inventory.CountItem(scrollVnum) < 1)
                        {
                            return;
                        }

                        if (protection == RarifyProtection.Scroll && !isCommand)
                        {
                            session.Character.Inventory.RemoveItemAmount(scrollVnum);
                            session.SendPacket("shop_end 2");
                        }
                        session.Character.Gold -= goldprice;
                        session.Character.Inventory.RemoveItemAmount(cellaVnum, cella);
                        session.SendPacket(session.Character.GenerateGold());
                        break;
                }
            }
            if (Item.IsHeroic && protection == RarifyProtection.Scroll)
            {
                if (rnd <= rare8 && !(protection == RarifyProtection.Scroll && Rare >= 8))
                {
                    if (mode != RarifyMode.Drop)
                    {
                        session?.Character.NotifyRarifyResult(8);
                    }

                    Rare = 8;
                    SetRarityPoint();
                    ItemInstance inventory = session?.Character.Inventory.GetItemInstanceById(Id);
                    if (inventory != null)
                    {
                        session.SendPacket(session.Character.GenerateInventoryAdd(ItemVNum, 1, inventory.Type, inventory.Slot, inventory.Rare, 0, inventory.Upgrade, 0));
                    }
                    return;
                }
            }
            if (rnd <= rare7 && !(protection == RarifyProtection.Scroll && Rare >= 7))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(7);
                }

                Rare = 7;
                SetRarityPoint();
            }
            else if (rnd <= rare6 && !(protection == RarifyProtection.Scroll && Rare >= 6))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(6);
                }
                Rare = 6;
                SetRarityPoint();
            }
            else if (rnd <= rare5 && !(protection == RarifyProtection.Scroll && Rare >= 5))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(5);
                }
                Rare = 5;
                SetRarityPoint();
            }
            else if (rnd <= rare4 && !(protection == RarifyProtection.Scroll && Rare >= 4))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(4);
                }
                Rare = 4;
                SetRarityPoint();
            }
            else if (rnd <= rare3 && !(protection == RarifyProtection.Scroll && Rare >= 3))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(3);
                }
                Rare = 3;
                SetRarityPoint();
            }
            else if (rnd <= rare2 && !(protection == RarifyProtection.Scroll && Rare >= 2))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(2);
                }
                Rare = 2;
                SetRarityPoint();
            }
            else if (rnd <= rare1 && !(protection == RarifyProtection.Scroll && Rare >= 1))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(1);
                }
                Rare = 1;
                SetRarityPoint();
            }
            else if (rnd <= rare0 && !(protection == RarifyProtection.Scroll && Rare >= 0))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(0);
                }
                Rare = 0;
                SetRarityPoint();
            }
            else if (rnd <= raren1 && !(protection == RarifyProtection.Scroll && Rare >= -1))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(-1);
                }
                Rare = -1;
                SetRarityPoint();
            }
            else if (rnd <= raren2 && !(protection == RarifyProtection.Scroll && Rare >= -2))
            {
                if (mode != RarifyMode.Drop)
                {
                    session?.Character.NotifyRarifyResult(-2);
                }
                Rare = -2;
                SetRarityPoint();
            }
            else
            {
                if (mode != RarifyMode.Drop && session != null)
                {
                    if (protection == RarifyProtection.None)
                    {
                        session.Character.DeleteItemByItemInstanceId(Id);
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 11));
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 0));
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 11));
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 0));
                        session.CurrentMapInstance.Broadcast(session.Character.GenerateEff(3004), session.Character.MapX, session.Character.MapY);
                    }
                }
            }

            // don't place under else.
            if (mode != RarifyMode.Drop && session != null)
            {
                ItemInstance inventory = session.Character.Inventory.GetItemInstanceById(Id);
                if (inventory != null)
                {
                    session.SendPacket(session.Character.GenerateInventoryAdd(ItemVNum, 1, inventory.Type, inventory.Slot, inventory.Rare, 0, inventory.Upgrade, 0));
                }
            }
        }

        public void SetRarityPoint()
        {
            switch (Item.EquipmentSlot)
            {
                case EquipmentType.MainWeapon:
                case EquipmentType.SecondaryWeapon:
                    {
                        int point = CharacterHelper.RarityPoint(Rare, Item.IsHeroic ? (short)(95 + Item.LevelMinimum) : Item.LevelMinimum);
                        Concentrate = 0;
                        HitRate = 0;
                        DamageMinimum = 0;
                        DamageMaximum = 0;
                        if (Rare >= 0)
                        {
                            for (int i = 0; i < point; i++)
                            {
                                int rndn = ServerManager.RandomNumber(0, 3);
                                if (rndn == 0)
                                {
                                    Concentrate++;
                                    HitRate++;
                                }
                                else
                                {
                                    DamageMinimum++;
                                    DamageMaximum++;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i > point; i--)
                            {
                                int rndn = ServerManager.RandomNumber(0, 3);
                                if (rndn == 0)
                                {
                                    Concentrate--;
                                    HitRate--;
                                }
                                else
                                {
                                    DamageMinimum--;
                                    DamageMaximum--;
                                }
                            }
                        }
                    }
                    break;
                case EquipmentType.Armor:
                    {
                        int point = CharacterHelper.RarityPoint(Rare, Item.IsHeroic ? (short)(95 + Item.LevelMinimum) : Item.LevelMinimum);
                        DefenceDodge = 0;
                        DistanceDefenceDodge = 0;
                        DistanceDefence = 0;
                        MagicDefence = 0;
                        CloseDefence = 0;
                        if (Rare >= 0)
                        {
                            for (int i = 0; i < point; i++)
                            {
                                int rndn = ServerManager.RandomNumber(0, 3);
                                if (rndn == 0)
                                {
                                    DefenceDodge++;
                                    DistanceDefenceDodge++;
                                }
                                else
                                {
                                    DistanceDefence++;
                                    MagicDefence++;
                                    CloseDefence++;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i > point; i--)
                            {
                                int rndn = ServerManager.RandomNumber(0, 3);
                                if (rndn == 0)
                                {
                                    DefenceDodge--;
                                    DistanceDefenceDodge--;
                                }
                                else
                                {
                                    DistanceDefence--;
                                    MagicDefence--;
                                    CloseDefence--;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        public void Sum(ClientSession session, WearableInstance itemToSum)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }

            // cannot sum higher than 5
            if (Upgrade < 6)
            {
                short[] upsuccess = { 100, 100, 85, 70, 50, 20 };
                int[] goldprice = { 1500, 3000, 6000, 12000, 24000, 48000 };
                short[] sand = { 5, 10, 15, 20, 25, 30 };
                const int sandVnum = 1027;
                if (Upgrade + itemToSum.Upgrade < 6 && (itemToSum.Item.EquipmentSlot == EquipmentType.Gloves && Item.EquipmentSlot == EquipmentType.Gloves || Item.EquipmentSlot == EquipmentType.Boots && itemToSum.Item.EquipmentSlot == EquipmentType.Boots))
                {
                    if (session.Character.Gold < goldprice[Upgrade])
                    {
                        return;
                    }
                    if (session.Character.Inventory.CountItem(sandVnum) < sand[Upgrade])
                    {
                        return;
                    }
                    session.Character.Inventory.RemoveItemAmount(sandVnum, (byte)sand[Upgrade]);
                    session.Character.Gold -= goldprice[Upgrade];

                    int rnd = ServerManager.RandomNumber();
                    if (rnd <= upsuccess[Upgrade + itemToSum.Upgrade])
                    {
                        Upgrade += (byte)(itemToSum.Upgrade + 1);
                        DarkResistance += (short)(itemToSum.DarkResistance + itemToSum.Item.DarkResistance);
                        LightResistance += (short)(itemToSum.LightResistance + itemToSum.Item.LightResistance);
                        WaterResistance += (short)(itemToSum.WaterResistance + itemToSum.Item.WaterResistance);
                        FireResistance += (short)(itemToSum.FireResistance + itemToSum.Item.FireResistance);
                        session.Character.DeleteItemByItemInstanceId(itemToSum.Id);
                        session.SendPacket($"pdti 10 {ItemVNum} 1 27 {Upgrade} 0");
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 0));
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 12));
                        session.SendPacket(session.Character.GenerateGuri(19, 1, 1324));
                        ItemInstance itemInstnace = session.Character.Inventory.GetItemInstanceById(Id);
                        session.SendPacket(session.Character.GenerateInventoryAdd(itemInstnace.ItemVNum, 1, itemInstnace.Type, itemInstnace.Slot, 0, 0, Upgrade, 0));
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_FAILED"), 0));
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_FAILED"), 11));
                        session.SendPacket(session.Character.GenerateGuri(19, 1, 1332));
                        session.Character.DeleteItemByItemInstanceId(itemToSum.Id);
                        session.Character.DeleteItemByItemInstanceId(Id);
                    }
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateGuri(6, 1), session.Character.MapX, session.Character.MapY);
                    session.SendPacket(session.Character.GenerateGold());
                    session.SendPacket("shop_end 1");
                }
            }
        }

        public void UpgradeItem(ClientSession session, UpgradeMode mode, UpgradeProtection protection, bool isCommand = false)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }
            if (Upgrade < 10)
            {
                short[] upsuccess;
                short[] upfix;
                int[] goldprice;
                short[] cella;
                short[] gem;

                if (Rare == 8)
                {
                    upsuccess = new short[] { 50, 50, 45, 30, 20, 10, 5, 3, 2, 1 };
                    upfix = new short[] { 0, 0, 10, 15, 20, 20, 20, 20, 15, 10 };
                    goldprice = new[] { 5000, 15000, 30000, 100000, 300000, 800000, 1500000, 4000000, 7000000, 10000000 };
                    cella = new short[] { 40, 100, 160, 240, 320, 440, 560, 760, 960, 1200 };
                    gem = new short[] { 2, 2, 4, 4, 6, 2, 2, 4, 4, 6 };
                }
                else
                {
                    upsuccess = new short[] { 100, 100, 100, 95, 80, 60, 40, 30, 20, 11 };
                    upfix = new short[] { 0, 0, 10, 15, 20, 20, 20, 20, 15, 10 };
                    goldprice = new[] { 500, 1500, 3000, 10000, 30000, 80000, 150000, 400000, 700000, 1000000 };
                    cella = new short[] { 20, 50, 80, 120, 160, 220, 280, 380, 480, 600 };
                    gem = new short[] { 1, 1, 2, 2, 3, 1, 1, 2, 2, 3 };
                }

                const short cellaVnum = 1014;
                const short gemVnum = 1015;
                const short gemFullVnum = 1016;
                const double reducedpricefactor = 0.5;
                const short normalScrollVnum = 1218;
                const short goldScrollVnum = 5369;

                if (IsFixed)
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_FIXED"), 10));
                    session.SendPacket("shop_end 1");
                    return;
                }
                switch (mode)
                {
                    case UpgradeMode.Free:
                        break;

                    case UpgradeMode.Reduced:

                        // TODO: Reduced Item Amount
                        if (session.Character.Gold < (long)(goldprice[Upgrade] * reducedpricefactor))
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            return;
                        }
                        if (session.Character.Inventory.CountItem(cellaVnum) < cella[Upgrade] * reducedpricefactor)
                        {
                            session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(cellaVnum).Name, cella[Upgrade] * reducedpricefactor), 10));
                            return;
                        }
                        if (protection == UpgradeProtection.Protected && !isCommand && session.Character.Inventory.CountItem(goldScrollVnum) < 1)
                        {
                            session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(goldScrollVnum).Name, cella[Upgrade] * reducedpricefactor), 10));
                            return;
                        }
                        if (Upgrade < 5)
                        {
                            if (session.Character.Inventory.CountItem(gemVnum) < gem[Upgrade])
                            {
                                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(gemVnum).Name, gem[Upgrade]), 10));
                                return;
                            }
                            session.Character.Inventory.RemoveItemAmount(gemVnum, gem[Upgrade]);
                        }
                        else
                        {
                            if (session.Character.Inventory.CountItem(gemFullVnum) < gem[Upgrade])
                            {
                                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(gemFullVnum).Name, gem[Upgrade]), 10));
                                return;
                            }
                            session.Character.Inventory.RemoveItemAmount(gemFullVnum, gem[Upgrade]);
                        }
                        if (protection == UpgradeProtection.Protected && !isCommand)
                        {
                            session.Character.Inventory.RemoveItemAmount(goldScrollVnum);
                            session.SendPacket("shop_end 2");
                        }
                        session.Character.Gold -= (long)(goldprice[Upgrade] * reducedpricefactor);
                        session.Character.Inventory.RemoveItemAmount(cellaVnum, (int)(cella[Upgrade] * reducedpricefactor));
                        session.SendPacket(session.Character.GenerateGold());
                        break;

                    case UpgradeMode.Normal:

                        // TODO: Normal Item Amount
                        if (session.Character.Inventory.CountItem(cellaVnum) < cella[Upgrade])
                        {
                            return;
                        }
                        if (session.Character.Gold < goldprice[Upgrade])
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            return;
                        }
                        if (protection == UpgradeProtection.Protected && !isCommand && session.Character.Inventory.CountItem(normalScrollVnum) < 1)
                        {
                            session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(normalScrollVnum).Name, 1), 10));
                            return;
                        }
                        if (Upgrade < 5)
                        {
                            if (session.Character.Inventory.CountItem(gemVnum) < gem[Upgrade])
                            {
                                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(gemVnum).Name, gem[Upgrade]), 10));
                                return;
                            }
                            session.Character.Inventory.RemoveItemAmount(gemVnum, gem[Upgrade]);
                        }
                        else
                        {
                            if (session.Character.Inventory.CountItem(gemFullVnum) < gem[Upgrade])
                            {
                                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), ServerManager.GetItem(gemFullVnum).Name, gem[Upgrade]), 10));
                                return;
                            }
                            session.Character.Inventory.RemoveItemAmount(gemFullVnum, gem[Upgrade]);
                        }
                        if (protection == UpgradeProtection.Protected && !isCommand)
                        {
                            session.Character.Inventory.RemoveItemAmount(normalScrollVnum);
                            session.SendPacket("shop_end 2");
                        }
                        session.Character.Inventory.RemoveItemAmount(cellaVnum, cella[Upgrade]);
                        session.Character.Gold -= goldprice[Upgrade];
                        session.SendPacket(session.Character.GenerateGold());
                        break;
                }
                WearableInstance wearable = session.Character.Inventory.LoadByItemInstance<WearableInstance>(Id);
                ItemInstance inventory = session.Character.Inventory.GetItemInstanceById(Id);

                int rnd = ServerManager.RandomNumber();

                if (Rare == 8)
                {
                    if (rnd <= upsuccess[Upgrade])
                    {
                        session.CurrentMapInstance.Broadcast(session.Character.GenerateEff(3005), session.Character.MapX, session.Character.MapY);
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 0));
                        wearable.Upgrade++;
                        if (session.Character.Family != null)
                        {
                            session.Character.Family.InsertFamilyLog(FamilyLogType.Upgrade, session.Character.Name, "", "", "", 0, 0, wearable.ItemVNum, wearable.Upgrade, 0);
                        }
                        session.SendPacket(session.Character.GenerateInventoryAdd(ItemVNum, 1, inventory.Type, inventory.Slot, wearable.Rare, 0, wearable.Upgrade, 0));
                    }
                    else if (rnd <= upfix[Upgrade])
                    {
                        session.CurrentMapInstance.Broadcast(session.Character.GenerateEff(3004), session.Character.MapX, session.Character.MapY);
                        wearable.IsFixed = true;
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 0));
                    }
                    else
                    {
                        if (protection == UpgradeProtection.None)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 11));
                            session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 0));
                            session.Character.DeleteItemByItemInstanceId(Id);
                        }
                        else
                        {
                            session.CurrentMapInstance.Broadcast(session.Character.GenerateEff(3004), session.Character.MapX, session.Character.MapY);
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SCROLL_PROTECT_USED"), 11));
                            session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
                        }
                    }
                }
                else
                {
                    if (rnd <= upfix[Upgrade])
                    {
                        session.CurrentMapInstance.Broadcast(session.Character.GenerateEff(3004), session.Character.MapX, session.Character.MapY);
                        wearable.IsFixed = true;
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 0));
                    }
                    else if (rnd <= upsuccess[Upgrade])
                    {
                        session.CurrentMapInstance.Broadcast(session.Character.GenerateEff(3005), session.Character.MapX, session.Character.MapY);
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 0));
                        wearable.Upgrade++;
                        if (session.Character.Family != null)
                        {
                            session.Character.Family.InsertFamilyLog(FamilyLogType.Upgrade, session.Character.Name, "", "", "", 0, 0, wearable.ItemVNum, wearable.Upgrade, 0);
                        }
                        session.SendPacket(session.Character.GenerateInventoryAdd(ItemVNum, 1, inventory.Type, inventory.Slot, wearable.Rare, 0, wearable.Upgrade, 0));
                    }
                    else
                    {
                        if (protection == UpgradeProtection.None)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 11));
                            session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 0));
                            session.Character.DeleteItemByItemInstanceId(Id);
                        }
                        else
                        {
                            session.CurrentMapInstance.Broadcast(session.Character.GenerateEff(3004), session.Character.MapX, session.Character.MapY);
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SCROLL_PROTECT_USED"), 11));
                            session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
                        }
                    }
                }
            }
            else
            {
                session.CurrentMapInstance.Broadcast(session.Character.GenerateEff(3004), session.Character.MapX, session.Character.MapY);
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SCROLL_PROTECT_USED"), 11));
                session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
            }
            session.SendPacket("shop_end 1");
        }

        #endregion
    }
}