using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class WearableInstance : ItemInstance, IWearableInstance, IGameObject
    {
        #region Instantiation

        public WearableInstance()
        {
        }

        public WearableInstance(long itemInstanceId)
        {
            ItemInstanceId = itemInstanceId;
        }

        public WearableInstance(WearableInstanceDTO wearableInstance)
        {
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

        public short MP { get; set; }

        public byte WaterElement { get; set; }

        public byte WaterResistance { get; set; }

        #endregion

        #region Methods

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
            //TODO inventoryitem
            if (rnd <= rare7 && !(protection == RarifyProtection.Scroll && this.Rare >= 7))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 7), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 7), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Rare = 7;
                WearableInstance inv = Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId);
                ServersData.SetRarityPoint(ref inv);
                
            }
            else if (rnd <= rare6 && !(protection == RarifyProtection.Scroll && this.Rare >= 6))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 6), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 6), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Rare = 6;
                WearableInstance inv = Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId);
                ServersData.SetRarityPoint(ref inv);
            }
            else if (rnd <= rare5 && !(protection == RarifyProtection.Scroll && this.Rare >= 5))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 5), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 5), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Rare = 5;
                WearableInstance inv = Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId);
                ServersData.SetRarityPoint(ref inv);
            }
            else if (rnd <= rare4 && !(protection == RarifyProtection.Scroll && this.Rare >= 4))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 4), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 4), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Rare = 4;
                WearableInstance inv = Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId);
                ServersData.SetRarityPoint(ref inv);
            }
            else if (rnd <= rare3 && !(protection == RarifyProtection.Scroll && this.Rare >= 3))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 3), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 3), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Rare = 3;
                WearableInstance inv = Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId);
                ServersData.SetRarityPoint(ref inv);
            }
            else if (rnd <= rare2 && !(protection == RarifyProtection.Scroll && this.Rare >= 2))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 2), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 2), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Rare = 2;
                WearableInstance inv = Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId);
                ServersData.SetRarityPoint(ref inv);
            }
            else if (rnd <= rare1 && !(protection == RarifyProtection.Scroll && this.Rare >= 1))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 1), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 1), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Rare = 1;
                WearableInstance inv = Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId);
                ServersData.SetRarityPoint(ref inv);
            }
            else
            {
                if (protection == RarifyProtection.None)
                {
                    Session.Character.DeleteItemByItemInstanceId (this.ItemInstanceId);
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 0));
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).IsFixed = true;
                }
            }
            Session.Character.GetStartupInventory();
            Session.Client.SendPacket("shop_end 1");
        }

        public void SumItem(ClientSession Session, WearableInstance itemToSum)
        {
            short[] upsuccess = { 100, 100, 85, 70, 50, 20 };
            int[] goldprice = { 1500, 3000, 6000, 12000, 24000, 48000 };
            short[] sand = { 5, 10, 15, 20, 25, 30 };
            int sandVnum = 1027;
            Item iteminfo = ServerManager.GetItem(this.ItemVNum);
            Item iteminfo2 = ServerManager.GetItem(itemToSum.ItemVNum);
            if ((this.Upgrade + itemToSum.Upgrade) < 6 && ((iteminfo2.EquipmentSlot == (byte)EquipmentType.Gloves) && (iteminfo2.EquipmentSlot == (byte)EquipmentType.Gloves)) || ((iteminfo.EquipmentSlot == (byte)EquipmentType.Boots) && (iteminfo2.EquipmentSlot == (byte)EquipmentType.Boots)))
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
                    this.DarkResistance += (byte)(itemToSum.DarkResistance + iteminfo2.DarkResistance);
                    this.LightResistance += (byte)(itemToSum.LightResistance + iteminfo2.LightResistance);
                    this.WaterResistance += (byte)(itemToSum.WaterResistance + iteminfo2.WaterResistance);
                    this.FireResistance += (byte)(itemToSum.FireResistance + iteminfo2.FireResistance);
                    Session.Character.DeleteItemByItemInstanceId(itemToSum.ItemInstanceId);
                    Session.Client.SendPacket($"pdti 10 {this.ItemVNum} 1 27 {this.Upgrade} 0");
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 12));
                    Session.Client.SendPacket($"guri 19 1 {Session.Character.CharacterId} 1324");
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    Session.Character.GetStartupInventory();
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_FAILED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_FAILED"), 11));
                    Session.Client.SendPacket($"guri 19 1 {Session.Character.CharacterId} 1332");
                    Session.Character.DeleteItemByItemInstanceId(itemToSum.ItemInstanceId);
                    Session.Character.DeleteItemByItemInstanceId(this.ItemInstanceId);
                }
                Session.Client.SendPacket("shop_end 1");
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
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_FIXED"), 10));

                    Session.Character.GetStartupInventory();
                    Session.Client.SendPacket("shop_end 1");
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
                        Session.Client.SendPacket(Session.Character.GenerateGold());
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
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                        break;
                }

                Random r = new Random();
                int rnd = r.Next(100);
                if (rnd <= upfix[this.Upgrade])
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).IsFixed = true;
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 0));
                }
                else if (rnd <= upsuccess[this.Upgrade])
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 0));
                    Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Upgrade++;
                }
                else
                {
                    if (protection == UpgradeProtection.None)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 11));
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 0));
                        Session.Character.DeleteItemByItemInstanceId(this.ItemInstanceId);
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
            Session.Character.GetStartupInventory();
            Session.Client.SendPacket("shop_end 1");
        }

        #endregion
    }
}