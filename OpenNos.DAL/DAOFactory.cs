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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySQL = OpenNos.DAL.EF.MySQL;

namespace OpenNos.DAL
{
    public class DAOFactory
    {
        #region Members

        private static IAccountDAO _accountDAO;
        private static ICharacterDAO _characterDAO;
        private static IPortalDAO _portalDAO;
        private static IMapDAO _mapDAO;
        private static INpcDAO _npcDAO;
        private static IGeneralLogDAO _generallogDAO;
        private static IItemDAO _itemDAO;
        private static IItemListDAO _itemlistDAO;
        private static IInventoryDAO _inventoryDAO;
        #endregion

        #region Properties
        public DAOFactory()
        {
             
                if (_accountDAO == null)
                {
                    _accountDAO = new MySQL.AccountDAO();
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

        public static IItemListDAO ItemListDAO
        {
            get
            {
                if (_itemlistDAO == null)
                {
                    _itemlistDAO = new MySQL.ItemListDAO();
                }

                return _itemlistDAO;
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
        public static INpcDAO NpcDAO
        {
            get
            {
                if (_npcDAO == null)
                {
                    _npcDAO = new MySQL.NpcDAO();
                }

                return _npcDAO;
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

        #endregion
    }
}
