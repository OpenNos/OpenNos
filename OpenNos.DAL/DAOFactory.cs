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
using OpenNos.DAL.Interface;
using System;
using MySQL = OpenNos.DAL.EF.MySQL;

namespace OpenNos.DAL
{
    public class DAOFactory
    {
        #region Members

        private static IAccountDAO _accountDAO;
        private static ICellonOptionDAO _cellonoptionDAO;
        private static ICharacterDAO _characterDAO;
        private static ICharacterSkillDAO _characterskillDAO;
        private static IComboDAO _comboDAO;
        private static IDropDAO _dropDAO;
        private static IGeneralLogDAO _generallogDAO;
        private static IInventoryDAO _inventoryDAO;
        private static IItemDAO _itemDAO;
        private static IMailDAO _mailDAO;
        private static IMapDAO _mapDAO;
        private static IMapMonsterDAO _mapmonsterDAO;
        private static IMapNpcDAO _mapnpcDAO;
        private static IMapTypeDAO _maptypeDAO;
        private static IMapTypeMapDAO _maptypemapDAO;
        private static INpcMonsterDAO _npcmonsterDAO;
        private static INpcMonsterSkillDAO _npcmonsterskillDAO;
        private static IPenaltyLogDAO _penaltylogDAO;
        private static IPortalDAO _portalDAO;
        private static IQuicklistEntryDAO _quicklistDAO;
        private static IRecipeDAO _recipeDAO;
        private static IRecipeItemDAO _recipeitemDAO;
        private static IRespawnDAO _respawnDAO;
        private static IShopDAO _shopDAO;
        private static IShopItemDAO _shopitemDAO;
        private static IShopSkillDAO _shopskillDAO;
        private static ISkillDAO _skillDAO;
        private static ITeleporterDAO _teleporterDAO;
        private static bool _useMock;

        #endregion

        #region Instantiation

        static DAOFactory()
        {
            try
            {
                _useMock = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseMock"]);

                if (!_useMock)
                {
                    MySQL.Helpers.MigrationHelper.GenerateSQLScript();
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Database Error Server", ex);
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
                    if (_useMock)
                    {
                        _accountDAO = new Mock.AccountDAO();
                    }
                    else
                    {
                        _accountDAO = new MySQL.AccountDAO();
                    }
                }

                return _accountDAO;
            }
        }

        public static ICellonOptionDAO CellonOptionDAO
        {
            get
            {
                if (_cellonoptionDAO == null)
                {
                    if (_useMock)
                    {
                        _cellonoptionDAO = new Mock.CellonOptionDAO();
                    }
                    else
                    {
                        _cellonoptionDAO = new MySQL.CellonOptionDAO();
                    }
                }

                return _cellonoptionDAO;
            }
        }

        public static ICharacterDAO CharacterDAO
        {
            get
            {
                if (_characterDAO == null)
                {
                    if (_useMock)
                    {
                        _characterDAO = new Mock.CharacterDAO();
                    }
                    else
                    {
                        _characterDAO = new MySQL.CharacterDAO();
                    }
                }

                return _characterDAO;
            }
        }

        public static ICharacterSkillDAO CharacterSkillDAO
        {
            get
            {
                if (_characterskillDAO == null)
                {
                    if (_useMock)
                    {
                        _characterskillDAO = new Mock.CharacterSkillDAO();
                    }
                    else
                    {
                        _characterskillDAO = new MySQL.CharacterSkillDAO();
                    }
                }

                return _characterskillDAO;
            }
        }

        public static IComboDAO ComboDAO
        {
            get
            {
                if (_comboDAO == null)
                {
                    if (_useMock)
                    {
                        _comboDAO = new Mock.ComboDAO();
                    }
                    else
                    {
                        _comboDAO = new MySQL.ComboDAO();
                    }
                }

                return _comboDAO;
            }
        }

        public static IDropDAO DropDAO
        {
            get
            {
                if (_dropDAO == null)
                {
                    if (_useMock)
                    {
                        _dropDAO = new Mock.DropDAO();
                    }
                    else
                    {
                        _dropDAO = new MySQL.DropDAO();
                    }
                }

                return _dropDAO;
            }
        }

        public static IGeneralLogDAO GeneralLogDAO
        {
            get
            {
                if (_generallogDAO == null)
                {
                    if (_useMock)
                    {
                        _generallogDAO = new Mock.GeneralLogDAO();
                    }
                    else
                    {
                        _generallogDAO = new MySQL.GeneralLogDAO();
                    }
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
                    if (_useMock)
                    {
                        _inventoryDAO = new Mock.InventoryDAO();
                    }
                    else
                    {
                        _inventoryDAO = new MySQL.InventoryDAO();
                    }
                }

                return _inventoryDAO;
            }
        }

        public static IItemDAO ItemDAO
        {
            get
            {
                if (_itemDAO == null)
                {
                    if (_useMock)
                    {
                        _itemDAO = new Mock.ItemDAO();
                    }
                    else
                    {
                        _itemDAO = new MySQL.ItemDAO();
                    }
                }

                return _itemDAO;
            }
        }

        public static IMailDAO MailDAO
        {
            get
            {
                if (_mailDAO == null)
                {
                    if (_useMock)
                    {
                        _mailDAO = new Mock.MailDAO();
                    }
                    else
                    {
                        _mailDAO = new MySQL.MailDAO();
                    }
                }

                return _mailDAO;
            }
        }

        public static IMapDAO MapDAO
        {
            get
            {
                if (_mapDAO == null)
                {
                    if (_useMock)
                    {
                        _mapDAO = new Mock.MapDAO();
                    }
                    else
                    {
                        _mapDAO = new MySQL.MapDAO();
                    }
                }

                return _mapDAO;
            }
        }

        public static IMapMonsterDAO MapMonsterDAO
        {
            get
            {
                if (_mapmonsterDAO == null)
                {
                    if (_useMock)
                    {
                        _mapmonsterDAO = new Mock.MapMonsterDAO();
                    }
                    else
                    {
                        _mapmonsterDAO = new MySQL.MapMonsterDAO();
                    }
                }

                return _mapmonsterDAO;
            }
        }

        public static IMapNpcDAO MapNpcDAO
        {
            get
            {
                if (_mapnpcDAO == null)
                {
                    if (_useMock)
                    {
                        _mapnpcDAO = new Mock.MapNpcDAO();
                    }
                    else
                    {
                        _mapnpcDAO = new MySQL.MapNpcDAO();
                    }
                }

                return _mapnpcDAO;
            }
        }

        public static IMapTypeDAO MapTypeDAO
        {
            get
            {
                if (_maptypeDAO == null)
                {
                    if (_useMock)
                    {
                        _maptypeDAO = new Mock.MapTypeDAO();
                    }
                    else
                    {
                        _maptypeDAO = new MySQL.MapTypeDAO();
                    }
                }

                return _maptypeDAO;
            }
        }

        public static IMapTypeMapDAO MapTypeMapDAO
        {
            get
            {
                if (_maptypemapDAO == null)
                {
                    if (_useMock)
                    {
                        _maptypemapDAO = new Mock.MapTypeMapDAO();
                    }
                    else
                    {
                        _maptypemapDAO = new MySQL.MapTypeMapDAO();
                    }
                }

                return _maptypemapDAO;
            }
        }

        public static INpcMonsterDAO NpcMonsterDAO
        {
            get
            {
                if (_npcmonsterDAO == null)
                {
                    if (_useMock)
                    {
                        _npcmonsterDAO = new Mock.NpcMonsterDAO();
                    }
                    else
                    {
                        _npcmonsterDAO = new MySQL.NpcMonsterDAO();
                    }
                }

                return _npcmonsterDAO;
            }
        }

        public static INpcMonsterSkillDAO NpcMonsterSkillDAO
        {
            get
            {
                if (_npcmonsterskillDAO == null)
                {
                    if (_useMock)
                    {
                        _npcmonsterskillDAO = new Mock.NpcMonsterSkillDAO();
                    }
                    else
                    {
                        _npcmonsterskillDAO = new MySQL.NpcMonsterSkillDAO();
                    }
                }

                return _npcmonsterskillDAO;
            }
        }

        public static IPenaltyLogDAO PenaltyLogDAO
        {
            get
            {
                if (_penaltylogDAO == null)
                {
                    if (_useMock)
                    {
                        _penaltylogDAO = new Mock.PenaltyLogDAO();
                    }
                    else
                    {
                        _penaltylogDAO = new MySQL.PenaltyLogDAO();
                    }
                }

                return _penaltylogDAO;
            }
        }

        public static IPortalDAO PortalDAO
        {
            get
            {
                if (_portalDAO == null)
                {
                    if (_useMock)
                    {
                        _portalDAO = new Mock.PortalDAO();
                    }
                    else
                    {
                        _portalDAO = new MySQL.PortalDAO();
                    }
                }

                return _portalDAO;
            }
        }

        public static IQuicklistEntryDAO QuicklistEntryDAO
        {
            get
            {
                if (_quicklistDAO == null)
                {
                    if (_useMock)
                    {
                        _quicklistDAO = new Mock.QuicklistEntryDAO();
                    }
                    else
                    {
                        _quicklistDAO = new MySQL.QuicklistEntryDAO();
                    }
                }

                return _quicklistDAO;
            }
        }

        public static IRecipeDAO RecipeDAO
        {
            get
            {
                if (_recipeDAO == null)
                {
                    if (_useMock)
                    {
                        _recipeDAO = new Mock.RecipeDAO();
                    }
                    else
                    {
                        _recipeDAO = new MySQL.RecipeDAO();
                    }
                }

                return _recipeDAO;
            }
        }

        public static IRecipeItemDAO RecipeItemDAO
        {
            get
            {
                if (_recipeitemDAO == null)
                {
                    if (_useMock)
                    {
                        _recipeitemDAO = new Mock.RecipeItemDAO();
                    }
                    else
                    {
                        _recipeitemDAO = new MySQL.RecipeItemDAO();
                    }
                }

                return _recipeitemDAO;
            }
        }

        public static IRespawnDAO RespawnDAO
        {
            get
            {
                if (_respawnDAO == null)
                {
                    if (_useMock)
                    {
                        _respawnDAO = new Mock.RespawnDAO();
                    }
                    else
                    {
                        _respawnDAO = new MySQL.RespawnDAO();
                    }
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
                    if (_useMock)
                    {
                        _shopDAO = new Mock.ShopDAO();
                    }
                    else
                    {
                        _shopDAO = new MySQL.ShopDAO();
                    }
                }

                return _shopDAO;
            }
        }

        public static IShopItemDAO ShopItemDAO
        {
            get
            {
                if (_shopitemDAO == null)
                {
                    if (_useMock)
                    {
                        _shopitemDAO = new Mock.ShopItemDAO();
                    }
                    else
                    {
                        _shopitemDAO = new MySQL.ShopItemDAO();
                    }
                }

                return _shopitemDAO;
            }
        }

        public static IShopSkillDAO ShopSkillDAO
        {
            get
            {
                if (_shopskillDAO == null)
                {
                    if (_useMock)
                    {
                        _shopskillDAO = new Mock.ShopSkillDAO();
                    }
                    else
                    {
                        _shopskillDAO = new MySQL.ShopSkillDAO();
                    }
                }

                return _shopskillDAO;
            }
        }

        public static ISkillDAO SkillDAO
        {
            get
            {
                if (_skillDAO == null)
                {
                    if (_useMock)
                    {
                        _skillDAO = new Mock.SkillDAO();
                    }
                    else
                    {
                        _skillDAO = new MySQL.SkillDAO();
                    }
                }

                return _skillDAO;
            }
        }

        public static ITeleporterDAO TeleporterDAO
        {
            get
            {
                if (_teleporterDAO == null)
                {
                    if (_useMock)
                    {
                        _teleporterDAO = new Mock.TeleporterDAO();
                    }
                    else
                    {
                        _teleporterDAO = new MySQL.TeleporterDAO();
                    }
                }

                return _teleporterDAO;
            }
        }

        #endregion
    }
}