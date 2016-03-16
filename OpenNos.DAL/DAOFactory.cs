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

using OpenNos.DAL.Interface;
using MySQL = OpenNos.DAL.EF.MySQL;

namespace OpenNos.DAL
{
    public class DAOFactory
    {
        #region Members

        private static IAccountDAO _accountDAO;
        private static ICharacterDAO _characterDAO;
        private static IGeneralLogDAO _generallogDAO;
        private static IInventoryDAO _inventoryDAO;
        private static IInventoryItemDAO _inventoryitemDAO;
        private static IItemDAO _itemDAO;
        private static IMapDAO _mapDAO;
        private static INpcMonsterDAO _npcmonsterDAO;
        private static IPortalDAO _portalDAO;
        private static IRespawnDAO _respawnDAO;
        private static IShopDAO _shopDAO;
        private static ITeleporterDAO _teleporterDAO;
        private static IShopItemDAO _shopitemDAO;
        private static IMapMonsterDAO _mapmonsterDAO;
        private static IMapNpcDAO _mapnpcDAO;
        #endregion

        #region Instantiation

        public DAOFactory()
        {
            if (_accountDAO == null)
            {
                _accountDAO = new MySQL.AccountDAO();
            }
        }

        #endregion

        #region Properties

        public static IAccountDAO AccountDAO
        {
            get
            {
                if (_accountDAO == null)
                {
                    _accountDAO = new MySQL.AccountDAO();
                }

                return _accountDAO;
            }
        }
        public static IMapNpcDAO MapNpcDAO
        {
            get
            {
                if (_mapnpcDAO == null)
                {
                    _mapnpcDAO = new MySQL.MapNpcDAO();
                }

                return _mapnpcDAO;
            }
        }
        public static IMapMonsterDAO MapMonsterDAO
        {
            get
            {
                if (_mapmonsterDAO == null)
                {
                    _mapmonsterDAO = new MySQL.MapMonsterDAO();
                }

                return _mapmonsterDAO;
            }
        }

  

        public static ICharacterDAO CharacterDAO
        {
            get
            {
                if (_characterDAO == null)
                {
                    _characterDAO = new MySQL.CharacterDAO();
                }

                return _characterDAO;
            }
        }

        public static IGeneralLogDAO GeneralLogDAO
        {
            get
            {
                if (_generallogDAO == null)
                {
                    _generallogDAO = new MySQL.GeneralLogDAO();
                }

                return _generallogDAO;
            }
        }

        public static IInventoryDAO InventoryDAO
        {
            get
            {
                if (_inventoryDAO == null)
                {
                    _inventoryDAO = new MySQL.InventoryDAO();
                }

                return _inventoryDAO;
            }
        }

        public static IInventoryItemDAO InventoryItemDAO
        {
            get
            {
                if (_inventoryitemDAO == null)
                {
                    _inventoryitemDAO = new MySQL.InventoryItemDAO();
                }

                return _inventoryitemDAO;
            }
        }

        public static IItemDAO ItemDAO
        {
            get
            {
                if (_itemDAO == null)
                {
                    _itemDAO = new MySQL.ItemDAO();
                }

                return _itemDAO;
            }
        }

        public static IMapDAO MapDAO
        {
            get
            {
                if (_mapDAO == null)
                {
                    _mapDAO = new MySQL.MapDAO();
                }

                return _mapDAO;
            }
        }

        public static INpcMonsterDAO NpcMonsterDAO
        {
            get
            {
                if (_npcmonsterDAO == null)
                {
                    _npcmonsterDAO = new MySQL.NpcMonsterDAO();
                }

                return _npcmonsterDAO;
            }
        }

        public static IPortalDAO PortalDAO
        {
            get
            {
                if (_portalDAO == null)
                {
                    _portalDAO = new MySQL.PortalDAO();
                }

                return _portalDAO;
            }
        }

        public static IRespawnDAO RespawnDAO
        {
            get
            {
                if (_respawnDAO == null)
                {
                    _respawnDAO = new MySQL.RespawnDAO();
                }

                return _respawnDAO;
            }
        }

        public static IShopDAO ShopDAO
        {
            get
            {
                if (_shopDAO == null)
                {
                    _shopDAO = new MySQL.ShopDAO();
                }

                return _shopDAO;
            }
        }
        public static ITeleporterDAO TeleporterDAO
        {
            get
            {
                if (_teleporterDAO == null)
                {
                    _teleporterDAO = new MySQL.TeleporterDAO();
                }

                return _teleporterDAO;
            }
        }
        public static IShopItemDAO ShopItemDAO
        {
            get
            {
                if (_shopitemDAO == null)
                {
                    _shopitemDAO = new MySQL.ShopItemDAO();
                }

                return _shopitemDAO;
            }
        }

        #endregion
    }
}