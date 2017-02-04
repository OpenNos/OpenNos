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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.WebApi.Reference;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OpenNos.Handler
{
    public class BazaarPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public BazaarPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session => _session;

        #endregion

        #region Methods

        /// <summary>
        /// c_blist cbListPacket
        /// </summary>
        /// <param name="cbListPacket"></param>
        public void RefreshBazarList(CBListPacket cbListPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            Session.SendPacket(Session.Character.GenerateRCBList(cbListPacket));
        }

        /// <summary>
        /// c_slist csListPacket
        /// </summary>
        /// <param name="csListPacket"></param>
        public void RefreshPersonalBazarList(CSListPacket csListPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            Session.SendPacket(Session.Character.GenerateRCSList(csListPacket));
        }

        /// <summary>
        /// c_skill cSkillPacket
        /// </summary>
        /// <param name="cSkillPacket"></param>
        public void OpenBazaar(CSkillPacket cSkillPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            StaticBonusDTO medal = Session.Character.StaticBonusList.FirstOrDefault(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
            if (medal != null)
            {
                byte Medal = medal.StaticBonusType == StaticBonusType.BazaarMedalGold ? (byte)MedalType.Gold : (byte)MedalType.Silver;
                int Time = (int)(medal.DateEnd - DateTime.Now).TotalHours;
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOTICE_BAZAAR"), 0));
                Session.SendPacket($"wopen 32 {Medal} {Time}");
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("INFO_BAZAAR")));
            }
        }

        /// <summary>
        /// c_buy cBuyPacket
        /// </summary>
        /// <param name="cBuyPacket"></param>
        public void BuyBazaar(CBuyPacket cBuyPacket)
        {
            BazaarItemDTO bz = DAOFactory.BazaarItemDAO.LoadAll().FirstOrDefault(s => s.BazaarItemId == cBuyPacket.BazaarId);
            if (bz != null && cBuyPacket.Amount > 0)
            {
                long price = cBuyPacket.Amount * bz.Price;

                if (Session.Character.Gold >= price)
                {
                    BazaarItemLink bzcree = new BazaarItemLink { BazaarItem = bz };
                    if (DAOFactory.CharacterDAO.LoadById(bz.SellerId) != null)
                    {
                        bzcree.Owner = DAOFactory.CharacterDAO.LoadById(bz.SellerId)?.Name;
                        bzcree.Item = (ItemInstance)DAOFactory.IteminstanceDAO.LoadById(bz.ItemInstanceId);
                    }
                    if (cBuyPacket.Amount <= bzcree.Item.Amount)
                    {

                        if (!Session.Character.Inventory.CanAddItem(bzcree.Item.ItemVNum))
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                            return;
                        }

                        if (bzcree.Item != null)
                        {
                            if (bz.IsPackage && cBuyPacket.Amount != bz.Amount)
                            {
                                return;
                            }
                            ItemInstanceDTO bzitemdto = DAOFactory.IteminstanceDAO.LoadById(bzcree.BazaarItem.ItemInstanceId);
                            if (bzitemdto.Amount < cBuyPacket.Amount)
                            {
                                return;
                            }
                            bzitemdto.Amount -= cBuyPacket.Amount;
                            Session.Character.Gold -= price;
                            Session.SendPacket(Session.Character.GenerateGold());
                            DAOFactory.IteminstanceDAO.InsertOrUpdate(bzitemdto);
                            ServerManager.Instance.BazaarRefresh(bzcree.BazaarItem.BazaarItemId);
                            Session.SendPacket($"rc_buy 1 {bzcree.Item.Item.VNum} {bzcree.Owner} {cBuyPacket.Amount} {cBuyPacket.Price} 0 0 0");
                            ItemInstance newBz = bzcree.Item.DeepCopy();
                            newBz.Id = Guid.NewGuid();
                            newBz.Amount = cBuyPacket.Amount;
                            newBz.Type = newBz.Item.Type;

                            ItemInstance newInv = Session.Character.Inventory.AddToInventory(newBz);
                            if (newInv != null)
                            {
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                                Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: { bzcree.Item.Item.Name} x {cBuyPacket.Amount}", 10));
                            }

                        }

                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateModal(Language.Instance.GetMessageFromKey("STATE_CHANGED"), 1));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                    Session.SendPacket(Session.Character.GenerateModal(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 1));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateModal(Language.Instance.GetMessageFromKey("STATE_CHANGED"), 1));
            }

        }

        /// <summary>
        /// c_scalc cScalcPacket
        /// </summary>
        /// <param name="cScalcPacket"></param>
        public void GetBazaar(CScalcPacket cScalcPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            BazaarItemDTO bz = DAOFactory.BazaarItemDAO.LoadAll().FirstOrDefault(s => s.BazaarItemId == cScalcPacket.BazaarId);
            if (bz != null)
            {
                ItemInstance Item = (ItemInstance)DAOFactory.IteminstanceDAO.LoadById(bz.ItemInstanceId);
                if (Item == null || Item.CharacterId != Session.Character.CharacterId)
                    return;
                int soldedamount = bz.Amount - Item.Amount;
                long taxes = bz.MedalUsed ? 0 : (long)(bz.Price * 0.10 * soldedamount);
                long price = bz.Price * soldedamount - taxes;
                if (Session.Character.Gold + price <= 1000000000)
                {
                    Session.Character.Gold += price;
                    Session.SendPacket(Session.Character.GenerateGold());
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REMOVE_FROM_BAZAAR"), price), 10));
                    if (Item.Amount != 0)
                    {
                        ItemInstance newBz = Item.DeepCopy();
                        newBz.Id = Guid.NewGuid();
                        newBz.Type = newBz.Item.Type;

                        ItemInstance newInv = Session.Character.Inventory.AddToInventory(newBz);
                        if (newInv != null)
                        {
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                        }
                    }
                    Session.SendPacket($"rc_scalc 1 {bz.Price} {bz.Amount - Item.Amount} {bz.Amount} {taxes} {price + taxes}");

                    if (DAOFactory.BazaarItemDAO.LoadById(bz.BazaarItemId) != null)
                    {
                        DAOFactory.BazaarItemDAO.Delete(bz.BazaarItemId);
                    }

                    DAOFactory.IteminstanceDAO.Delete(Item.Id);

                    ServerManager.Instance.BazaarRefresh(bz.BazaarItemId);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                    Session.SendPacket($"rc_scalc 1 {bz.Price} 0 {bz.Amount} 0 0");
                }
            }
            else
            {
                Session.SendPacket($"rc_scalc 1 0 0 0 0 0");
            }
        }

        /// <summary>
        /// c_reg packet
        /// </summary>
        /// <param name="cRegPacket"></param>
        public void SellBazaar(CRegPacket cRegPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.inBazaarRefreshMode);
            StaticBonusDTO medal = Session.Character.StaticBonusList.FirstOrDefault(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);

            long price = cRegPacket.Price * cRegPacket.Amount;
            long taxmax = price > 100000 ? price / 200 : 500;
            long taxmin = price >= 4000 ? (60 + (price - 4000) / 2000 * 30 > 10000 ? 10000 : 60 + (price - 4000) / 2000 * 30) : 50;
            long tax = medal == null ? taxmax : taxmin;
            if (Session.Character.Gold < tax || cRegPacket.Amount <= 0 || Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.ExchangeList.Any() || Session.Character.IsShopping)
            {
                return;
            }
            ItemInstance it = Session.Character.Inventory.LoadBySlotAndType(cRegPacket.Slot, cRegPacket.Inventory == 4 ? 0 : (InventoryType)cRegPacket.Inventory);
            if (it == null || !it.Item.IsSoldable || it.IsBound)
            {
                return;
            }
            if (Session.Character.Inventory.CountItemInAnInventory(InventoryType.Bazaar) > 10 * (medal == null ? 1 : 10))
            {
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LIMIT_EXCEEDED"), 0));
                return;
            }
            if (price >= (medal == null ? 1000000 : 1000000000))
            {
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("PRICE_EXCEEDED"), 0));
                return;
            }
            if (cRegPacket.Price < 0)
            {
                return;
            }
            ItemInstance bazar = Session.Character.Inventory.AddIntoBazaarInventory(cRegPacket.Inventory == 4 ? 0 : (InventoryType)cRegPacket.Inventory, cRegPacket.Slot, cRegPacket.Amount);
            if (bazar == null)
            {
                return;
            }
            short duration;
            switch (cRegPacket.Durability)
            {
                case 1:
                    duration = 24;
                    break;
                case 2:
                    duration = 168;
                    break;
                case 3:
                    duration = 360;
                    break;
                case 4:
                    duration = 720;
                    break;
                default:
                    return;
            }

            DAOFactory.IteminstanceDAO.InsertOrUpdate(bazar);

            BazaarItemDTO bz = new BazaarItemDTO
            {
                Amount = bazar.Amount,
                DateStart = DateTime.Now,
                Duration = duration,
                IsPackage = cRegPacket.IsPackage != 0,
                MedalUsed = medal != null,
                Price = cRegPacket.Price,
                SellerId = Session.Character.CharacterId,
                ItemInstanceId = bazar.Id,
            };


            DAOFactory.BazaarItemDAO.InsertOrUpdate(ref bz);
            ServerManager.Instance.BazaarRefresh(bz.BazaarItemId);

            Session.Character.Gold -= tax;
            Session.SendPacket(Session.Character.GenerateGold());

            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("OBJECT_IN_BAZAAR"), 10));
            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("OBJECT_IN_BAZAAR"), 0));

            Session.SendPacket("rc_reg 1");

        }

        #endregion
    }
}