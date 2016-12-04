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
using System.Configuration;
using OpenNos.Core;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.DAL.Mock;

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
        private static IItemDAO _itemDAO;
        private static IItemInstanceDAO _itemInstanceDAO;
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
        private static IRespawnMapTypeDAO _respawnMapTypeDAO;
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
                _useMock = Convert.ToBoolean(ConfigurationManager.AppSettings["UseMock"]);

                if (!_useMock)
                {
                    MigrationHelper.GenerateSqlScript();
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
                        _accountDAO = new AccountDAO();
                    }
                    else
                    {
                        _accountDAO = new AccountDao();
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
                        _cellonoptionDAO = new CellonOptionDAO();
                    }
                    else
                    {
                        _cellonoptionDAO = new CellonOptionDao();
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
                        _characterDAO = new CharacterDAO();
                    }
                    else
                    {
                        _characterDAO = new CharacterDao();
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
                        _characterskillDAO = new CharacterSkillDAO();
                    }
                    else
                    {
                        _characterskillDAO = new CharacterSkillDao();
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
                        _comboDAO = new ComboDAO();
                    }
                    else
                    {
                        _comboDAO = new ComboDao();
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
                        _dropDAO = new DropDAO();
                    }
                    else
                    {
                        _dropDAO = new DropDao();
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
                        _generallogDAO = new GeneralLogDAO();
                    }
                    else
                    {
                        _generallogDAO = new GeneralLogDao();
                    }
                }

                return _generallogDAO;
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
                        _itemDAO = new ItemDAO();
                    }
                    else
                    {
                        _itemDAO = new ItemDao();
                    }
                }

                return _itemDAO;
            }
        }

        public static IItemInstanceDAO ItemInstanceDAO
        {
            get
            {
                if (_itemInstanceDAO == null)
                {
                    if (_useMock)
                    {
                        _itemInstanceDAO = new ItemInstanceDAO();
                    }
                    else
                    {
                        _itemInstanceDAO = new ItemInstanceDao();
                    }
                }

                return _itemInstanceDAO;
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
                        _mailDAO = new MailDAO();
                    }
                    else
                    {
                        _mailDAO = new MailDao();
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
                        _mapDAO = new MapDAO();
                    }
                    else
                    {
                        _mapDAO = new MapDao();
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
                        _mapmonsterDAO = new MapMonsterDAO();
                    }
                    else
                    {
                        _mapmonsterDAO = new MapMonsterDao();
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
                        _mapnpcDAO = new MapNpcDAO();
                    }
                    else
                    {
                        _mapnpcDAO = new MapNpcDao();
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
                        _maptypeDAO = new MapTypeDAO();
                    }
                    else
                    {
                        _maptypeDAO = new MapTypeDao();
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
                        _maptypemapDAO = new MapTypeMapDAO();
                    }
                    else
                    {
                        _maptypemapDAO = new MapTypeMapDao();
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
                        _npcmonsterDAO = new NpcMonsterDAO();
                    }
                    else
                    {
                        _npcmonsterDAO = new NpcMonsterDao();
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
                        _npcmonsterskillDAO = new NpcMonsterSkillDAO();
                    }
                    else
                    {
                        _npcmonsterskillDAO = new NpcMonsterSkillDao();
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
                        _penaltylogDAO = new PenaltyLogDAO();
                    }
                    else
                    {
                        _penaltylogDAO = new PenaltyLogDao();
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
                        _portalDAO = new PortalDAO();
                    }
                    else
                    {
                        _portalDAO = new PortalDao();
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
                        _quicklistDAO = new QuicklistEntryDAO();
                    }
                    else
                    {
                        _quicklistDAO = new QuicklistEntryDao();
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
                        _recipeDAO = new RecipeDAO();
                    }
                    else
                    {
                        _recipeDAO = new RecipeDao();
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
                        _recipeitemDAO = new RecipeItemDAO();
                    }
                    else
                    {
                        _recipeitemDAO = new RecipeItemDao();
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
                        _respawnDAO = new RespawnDAO();
                    }
                    else
                    {
                        _respawnDAO = new RespawnDao();
                    }
                }

                return _respawnDAO;
            }
        }

        public static IRespawnMapTypeDAO RespawnMapTypeDAO
        {
            get
            {
                if (_respawnMapTypeDAO == null)
                {
                    if (_useMock)
                    {
                        _respawnMapTypeDAO = new RespawnMapTypeDAO();
                    }
                    else
                    {
                        _respawnMapTypeDAO = new RespawnMapTypeDao();
                    }
                }

                return _respawnMapTypeDAO;
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
                        _shopDAO = new ShopDAO();
                    }
                    else
                    {
                        _shopDAO = new ShopDao();
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
                        _shopitemDAO = new ShopItemDAO();
                    }
                    else
                    {
                        _shopitemDAO = new ShopItemDao();
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
                        _shopskillDAO = new ShopSkillDAO();
                    }
                    else
                    {
                        _shopskillDAO = new ShopSkillDao();
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
                        _skillDAO = new SkillDAO();
                    }
                    else
                    {
                        _skillDAO = new SkillDao();
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
                        _teleporterDAO = new TeleporterDAO();
                    }
                    else
                    {
                        _teleporterDAO = new TeleporterDao();
                    }
                }

                return _teleporterDAO;
            }
        }

        #endregion
    }
}