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

namespace OpenNos.DAL
{
    public class DAOFactory
    {
        #region Members

        private static IAccountDAO _accountDAO;
        private static IBazaarItemDAO _bazaarItemDAO;
        private static ICellonOptionDAO _cellonoptionDAO;
        private static ICharacterDAO _characterDAO;
        private static ICharacterRelationDAO _characterRelationDAO;
        private static ICharacterSkillDAO _characterskillDAO;
        private static IComboDAO _comboDAO;
        private static IDropDAO _dropDAO;
        private static IFamilyCharacterDAO _familycharacterDAO;
        private static IFamilyDAO _familyDAO;
        private static IFamilyLogDAO _familylogDAO;
        private static IGeneralLogDAO _generallogDAO;
        private static IItemDAO _itemDAO;
        private static IItemInstanceDAO _iteminstanceDAO;
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
        private static IStaticBonusDAO _staticBonusDAO;
        private static ITeleporterDAO _teleporterDAO;
        private static readonly bool _useMock;

        #endregion

        #region Instantiation

        static DAOFactory()
        {
            try
            {
                _useMock = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseMock"]);

                if (!_useMock)
                {
                    EF.Helpers.MigrationHelper.GenerateSQLScript();
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
                        _accountDAO = new EF.AccountDAO();
                    }
                }

                return _accountDAO;
            }
        }

        public static IBazaarItemDAO BazaarItemDAO
        {
            get
            {
                if (_bazaarItemDAO == null)
                {
                    if (_useMock)
                    {
                        _bazaarItemDAO = new Mock.BazaarItemDAO();
                    }
                    else
                    {
                        _bazaarItemDAO = new EF.BazaarItemDAO();
                    }
                }

                return _bazaarItemDAO;
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
                        _cellonoptionDAO = new EF.CellonOptionDAO();
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
                        _characterDAO = new EF.CharacterDAO();
                    }
                }

                return _characterDAO;
            }
        }

        public static ICharacterRelationDAO CharacterRelationDAO
        {
            get
            {
                if (_characterRelationDAO == null)
                {
                    if (_useMock)
                    {
                        _characterRelationDAO = new Mock.CharacterRelationDAO();
                    }
                    else
                    {
                        _characterRelationDAO = new EF.CharacterRelationDAO();
                    }
                }

                return _characterRelationDAO;
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
                        _characterskillDAO = new EF.CharacterSkillDAO();
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
                        _comboDAO = new EF.ComboDAO();
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
                        _dropDAO = new EF.DropDAO();
                    }
                }

                return _dropDAO;
            }
        }

        public static IFamilyCharacterDAO FamilyCharacterDAO
        {
            get
            {
                if (_familycharacterDAO == null)
                {
                    if (_useMock)
                    {
                        _familycharacterDAO = new Mock.FamilyCharacterDAO();
                    }
                    else
                    {
                        _familycharacterDAO = new EF.FamilyCharacterDAO();
                    }
                }

                return _familycharacterDAO;
            }
        }

        public static IFamilyDAO FamilyDAO
        {
            get
            {
                if (_familyDAO == null)
                {
                    if (_useMock)
                    {
                        _familyDAO = new Mock.FamilyDAO();
                    }
                    else
                    {
                        _familyDAO = new EF.FamilyDAO();
                    }
                }

                return _familyDAO;
            }
        }

        public static IFamilyLogDAO FamilyLogDAO
        {
            get
            {
                if (_familylogDAO == null)
                {
                    if (_useMock)
                    {
                        _familylogDAO = new Mock.FamilyLogDAO();
                    }
                    else
                    {
                        _familylogDAO = new EF.FamilyLogDAO();
                    }
                }

                return _familylogDAO;
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
                        _generallogDAO = new EF.GeneralLogDAO();
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
                        _itemDAO = new Mock.ItemDAO();
                    }
                    else
                    {
                        _itemDAO = new EF.ItemDAO();
                    }
                }

                return _itemDAO;
            }
        }

        public static IItemInstanceDAO IteminstanceDAO
        {
            get
            {
                if (_iteminstanceDAO == null)
                {
                    if (_useMock)
                    {
                        _iteminstanceDAO = new Mock.ItemInstanceDAO();
                    }
                    else
                    {
                        _iteminstanceDAO = new EF.ItemInstanceDAO();
                    }
                }

                return _iteminstanceDAO;
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
                        _mailDAO = new EF.MailDAO();
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
                        _mapDAO = new EF.MapDAO();
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
                        _mapmonsterDAO = new EF.MapMonsterDAO();
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
                        _mapnpcDAO = new EF.MapNpcDAO();
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
                        _maptypeDAO = new EF.MapTypeDAO();
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
                        _maptypemapDAO = new EF.MapTypeMapDAO();
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
                        _npcmonsterDAO = new EF.NpcMonsterDAO();
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
                        _npcmonsterskillDAO = new EF.NpcMonsterSkillDAO();
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
                        _penaltylogDAO = new EF.PenaltyLogDAO();
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
                        _portalDAO = new EF.PortalDAO();
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
                        _quicklistDAO = new EF.QuicklistEntryDAO();
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
                        _recipeDAO = new EF.RecipeDAO();
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
                        _recipeitemDAO = new EF.RecipeItemDAO();
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
                        _respawnDAO = new EF.RespawnDAO();
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
                        _respawnMapTypeDAO = new Mock.RespawnMapTypeDAO();
                    }
                    else
                    {
                        _respawnMapTypeDAO = new EF.RespawnMapTypeDAO();
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
                        _shopDAO = new Mock.ShopDAO();
                    }
                    else
                    {
                        _shopDAO = new EF.ShopDAO();
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
                        _shopitemDAO = new EF.ShopItemDAO();
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
                        _shopskillDAO = new EF.ShopSkillDAO();
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
                        _skillDAO = new EF.SkillDAO();
                    }
                }

                return _skillDAO;
            }
        }

        public static IStaticBonusDAO StaticBonusDAO
        {
            get
            {
                if (_staticBonusDAO == null)
                {
                    if (_useMock)
                    {
                        _staticBonusDAO = new Mock.StaticBonusDAO();
                    }
                    else
                    {
                        _staticBonusDAO = new EF.StaticBonusDAO();
                    }
                }

                return _staticBonusDAO;
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
                        _teleporterDAO = new EF.TeleporterDAO();
                    }
                }

                return _teleporterDAO;
            }
        }

        #endregion
    }
}