using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class SpecialistInstance : WearableInstance, ISpecialistInstance
    {
        #region Instantiation

        public SpecialistInstance()
        {

        }

        public SpecialistInstance(long itemInstanceId)
        {
            ItemInstanceId = itemInstanceId;
        }

        public SpecialistInstance(SpecialistInstanceDTO specialistInstance)
        {
            SpDamage = specialistInstance.SpDamage;
            SpDark = specialistInstance.SpDark;
            SpDefence = specialistInstance.SpDefence;
            SpElement = specialistInstance.SpElement;
            SpFire = specialistInstance.SpFire;
            SpHP = specialistInstance.SpHP;
            SpLight = specialistInstance.SpLight;
            SpStoneUpgrade = specialistInstance.SpStoneUpgrade;
            SpWater = specialistInstance.SpWater;
            SpXp = specialistInstance.SpXp;
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

        public long SpXp { get; set; }

        #endregion

        #region Methods

        public void PerfectSP(ClientSession Session, UpgradeProtection protect)
        {
            short[] upsuccess = { 50, 40, 30, 20, 10 };

            int[] goldprice = { 5000, 1000, 20000, 50000, 100000 };
            short[] stoneprice = { 1, 2, 3, 4, 5 };
            short stonevnum;
            byte upmode = 1;

            switch (ServerManager.GetItem(this.ItemVNum).Morph)
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
            if (this.SpStoneUpgrade > 99)
                return;
            if (this.SpStoneUpgrade > 80)
            {
                upmode = 5;
            }
            if (this.SpStoneUpgrade > 60)
            {
                upmode = 4;
            }
            if (this.SpStoneUpgrade > 40)
            {
                upmode = 3;
            }
            if (this.SpStoneUpgrade > 20)
            {
                upmode = 2;
            }

            if (this.IsFixed)
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
                    Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpDamage += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 0));
                }
                else if (type < 6)
                {
                    Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpDefence += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 0));
                }
                else if (type < 9)
                {
                    Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpElement += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 0));
                }
                else if (type < 12)
                {
                    Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpHP += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 0));
                }
                else if (type == 12)
                {
                    Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpFire += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 0));
                }
                else if (type == 13)
                {
                    Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpWater += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 0));
                }
                else if (type == 14)
                {
                    Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpLight += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 0));
                }
                else if (type == 15)
                {
                    Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpDark += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 0));
                }
                Session.Character.InventoryList.LoadByItemInstance<SpecialistInstance>(this.ItemInstanceId).SpStoneUpgrade++;
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PERFECTSP_FAILURE"), 11));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("PERFECTSP_FAILURE"), 0));
            }
            Session.Character.Gold = Session.Character.Gold - goldprice[upmode];
            Session.Client.SendPacket(Session.Character.GenerateGold());
            Session.Character.InventoryList.RemoveItemAmount(stonevnum, stoneprice[upmode]);
            Session.Character.GenerateStartupInventory();
        }

        public void UpgradeSp(ClientSession Session, UpgradeProtection protect)
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

            if (this.IsFixed)
                return;
            if (Session.Character.Gold < goldprice[this.Upgrade])
                return;
            if (Session.Character.InventoryList.CountItem(fullmoonVnum) < fullmoon[this.Upgrade])
                return;
            if (Session.Character.InventoryList.CountItem(featherVnum) < feather[this.Upgrade])
                return;

            if (this.Upgrade < 5)
            {
                if (this.SpLevel > 20)
                {
                    if (ServerManager.GetItem(this.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(greenSoulVnum) < soul[this.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(greenSoulVnum, (soul[this.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonSkinVnum) < soul[this.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonSkinVnum, (soul[this.Upgrade]));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIERED"), 21), 11));

                    return;
                }
            }
            else if (this.Upgrade < 10)
            {
                if (this.SpLevel > 40)
                {
                    if (ServerManager.GetItem(this.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(redSoulVnum) < soul[this.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(redSoulVnum, (soul[this.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonBloodVnum) < soul[this.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonBloodVnum, (soul[this.Upgrade]));
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
                if (this.SpLevel > 50)
                {
                    if (ServerManager.GetItem(this.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(blueSoulVnum) < soul[this.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(blueSoulVnum, (soul[this.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonHeartVnum) < soul[this.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonHeartVnum, (soul[this.Upgrade]));
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
            if (rnd <= upfail[this.Upgrade])
            {
                if (protect == UpgradeProtection.Protected)
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));

                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 11));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 0));
            }
            else if (rnd <= upsuccess[this.Upgrade])
            {
                if (protect == UpgradeProtection.Protected)
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 0));
                Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).Upgrade++;
            }
            else
            {
                if (protect == UpgradeProtection.Protected)
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByItemInstance<WearableInstance>(this.ItemInstanceId).IsFixed = true;
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 0));
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROY"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROY"), 0));
                }
            }
            Session.Character.Gold = Session.Character.Gold - goldprice[this.Upgrade];
            Session.Client.SendPacket(Session.Character.GenerateGold());
            Session.Character.InventoryList.RemoveItemAmount(featherVnum, (feather[this.Upgrade]));
            Session.Character.InventoryList.RemoveItemAmount(fullmoonVnum, (fullmoon[this.Upgrade]));
            Session.Character.GenerateStartupInventory();
            Session.Client.SendPacket("shop_end 1");
        }

        #endregion
    }
}
