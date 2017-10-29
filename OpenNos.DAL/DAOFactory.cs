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
using System;
using System.Configuration;
using OpenNos.Data;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Entities;
using OpenNos.GameObject;

namespace OpenNos.DAL
{
    public class DAOFactory
    {
        #region Members

        private static readonly bool _useMock;
        private static IGenericDAO<Account, AccountDTO> _accountDAO;
        private static IGenericDAO<BazaarItem, BazaarItemDTO> _bazaarItemDAO;
        private static IGenericDAO<Card, CardDTO> _cardDAO;
        private static IGenericDAO<BCard, BCardDTO> _bcardDAO;
        private static IGenericDAO<RollGeneratedItem, RollGeneratedItemDTO> _rollGeneratedItemDAO;
        private static IGenericDAO<EquipmentOption, EquipmentOptionDTO> _equipmentOptionDAO;
        private static IGenericDAO<Character, CharacterDTO> _characterDAO;
        private static IGenericDAO<CharacterRelation, CharacterRelationDTO> _characterRelationDAO;
        private static IGenericDAO<CharacterSkill, CharacterSkillDTO> _characterskillDAO;
        private static IGenericDAO<Combo, ComboDTO> _comboDAO;
        private static IGenericDAO<Drop, DropDTO> _dropDAO;
        private static IGenericDAO<FamilyCharacter, FamilyCharacterDTO> _familycharacterDAO;
        private static IGenericDAO<Family, FamilyDTO> _familyDAO;
        private static IGenericDAO<FamilyLog, FamilyLogDTO> _familylogDAO;
        private static IGenericDAO<GeneralLog, GeneralLogDTO> _generallogDAO;
        private static IGenericDAO<Item, ItemDTO> _itemDAO;
        private static IGenericDAO<ItemInstance, ItemInstanceDTO> _iteminstanceDAO;
        private static IGenericDAO<LogChat, LogChatDTO> _logChatDAO;
        private static IGenericDAO<LogCommands, LogCommandsDTO> _logCommandsDAO;
        private static IGenericDAO<Mail, MailDTO> _mailDAO;
        private static IGenericDAO<Map, MapDTO> _mapDAO;
        private static IGenericDAO<MapMonster, MapMonsterDTO> _mapmonsterDAO;
        private static IGenericDAO<MapNpc, MapNpcDTO> _mapnpcDAO;
        private static IGenericDAO<MapType, MapTypeDTO> _maptypeDAO;
        private static IGenericDAO<MapTypeMap, MapTypeMapDTO> _maptypemapDAO;
        private static IGenericDAO<Mate, MateDTO> _mateDAO;
        private static IGenericDAO<MinilandObject, MinilandObjectDTO> _minilandobjectDAO;
        private static IGenericDAO<NpcMonster, NpcMonsterDTO> _npcmonsterDAO;
        private static IGenericDAO<NpcMonsterSkill, NpcMonsterSkillDTO> _npcmonsterskillDAO;
        private static IGenericDAO<PenaltyLog, PenaltyLogDTO> _penaltylogDAO;
        private static IGenericDAO<Portal, PortalDTO> _portalDAO;
        private static IGenericDAO<QuicklistEntry, QuicklistEntryDTO> _quicklistDAO;
        private static IGenericDAO<Recipe, RecipeDTO> _recipeDAO;
        private static IGenericDAO<RecipeItem, RecipeItemDTO> _recipeitemDAO;
        private static IGenericDAO<Respawn, RespawnDTO> _respawnDAO;
        private static IGenericDAO<RespawnMapType, RespawnMapTypeDTO> _respawnMapTypeDAO;
        private static IGenericDAO<ScriptedInstance, ScriptedInstanceDTO> _scriptedinstanceDAO;
        private static IGenericDAO<Shop, ShopDTO> _shopDAO;
        private static IGenericDAO<ShopItem, ShopItemDTO> _shopitemDAO;
        private static IGenericDAO<ShopSkill, ShopSkillDTO> _shopskillDAO;
        private static IGenericDAO<Skill, SkillDTO> _skillDAO;
        private static IGenericDAO<StaticBonus, StaticBonusDTO> _staticBonusDAO;
        private static IGenericDAO<StaticBuff, StaticBuffDTO> _staticBuffDAO;
        private static IGenericDAO<Teleporter, TeleporterDTO> _teleporterDAO;

        #endregion

        #region Instantiation

        static DAOFactory()
        {
            try
            {
                _useMock = Convert.ToBoolean(ConfigurationManager.AppSettings["UseMock"]);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Database Error Server", ex);
            }
        }

        #endregion

        #region Properties

        public static IGenericDAO<Account, AccountDTO> AccountDAO
        {
            get { return _accountDAO ?? (_accountDAO = new GenericDAO<Account, AccountDTO>()); }
        }

        public static IGenericDAO<BazaarItem, BazaarItemDTO> BazaarItemDAO
        {
            get { return _bazaarItemDAO ?? (_bazaarItemDAO = new GenericDAO<BazaarItem, BazaarItemDTO>()); }
        }

        public static IGenericDAO<Card, CardDTO> CardDAO
        {
            get { return _cardDAO ?? (_cardDAO = new GenericDAO<Card, CardDTO>()); }
        }

        public static IGenericDAO<EquipmentOption, EquipmentOptionDTO> EquipmentOptionDAO
        {
            get { return _equipmentOptionDAO ?? (_equipmentOptionDAO = new GenericDAO<EquipmentOption, EquipmentOptionDTO>()); }
        }

        public static IGenericDAO<Character, CharacterDTO> CharacterDAO
        {
            get { return _characterDAO ?? (_characterDAO = new GenericDAO<Character, CharacterDTO>()); }
        }

        public static IGenericDAO<CharacterRelation, CharacterRelationDTO> CharacterRelationDAO
        {
            get { return _characterRelationDAO ?? (_characterRelationDAO = new GenericDAO<CharacterRelation, CharacterRelationDTO>()); }
        }

        public static IGenericDAO<CharacterSkill, CharacterSkillDTO> CharacterSkillDAO
        {
            get { return _characterskillDAO ?? (_characterskillDAO = new GenericDAO<CharacterSkill, CharacterSkillDTO>()); }
        }

        public static IGenericDAO<Combo, ComboDTO> ComboDAO
        {
            get { return _comboDAO ?? (_comboDAO = new GenericDAO<Combo, ComboDTO>()); }
        }

        public static IGenericDAO<Drop, DropDTO> DropDAO
        {
            get { return _dropDAO ?? (_dropDAO = new GenericDAO<Drop, DropDTO>()); }
        }

        public static IGenericDAO<FamilyCharacter, FamilyCharacterDTO> FamilyCharacterDAO
        {
            get { return _familycharacterDAO ?? (_familycharacterDAO = new GenericDAO<FamilyCharacter, FamilyCharacterDTO>()); }
        }

        public static IGenericDAO<Family, FamilyDTO> FamilyDAO
        {
            get { return _familyDAO ?? (_familyDAO = new GenericDAO<Family, FamilyDTO>()); }
        }

        public static IGenericDAO<FamilyLog, FamilyLogDTO> FamilyLogDAO
        {
            get { return _familylogDAO ?? (_familylogDAO = new GenericDAO<FamilyLog, FamilyLogDTO>()); }
        }

        public static IGenericDAO<GeneralLog, GeneralLogDTO> GeneralLogDAO
        {
            get { return _generallogDAO ?? (_generallogDAO = new GenericDAO<GeneralLog, GeneralLogDTO>()); }
        }

        public static IGenericDAO<Item, ItemDTO> ItemDAO
        {
            get { return _itemDAO ?? (_itemDAO = new GenericDAO<Item, ItemDTO>()); }
        }

        public static IGenericDAO<ItemInstance, ItemInstanceDTO> IteminstanceDAO
        {
            get { return _iteminstanceDAO ?? (_iteminstanceDAO = new GenericDAO<ItemInstance, ItemInstanceDTO>()); }
        }

        public static IGenericDAO<LogChat, LogChatDTO> LogChatDAO
        {
            get { return _logChatDAO ?? (_logChatDAO = new GenericDAO<LogChat, LogChatDTO>()); }
        }

        public static IGenericDAO<LogCommands, LogCommandsDTO> LogCommandsDAO
        {
            get { return _logCommandsDAO ?? (_logCommandsDAO = new GenericDAO<LogCommands, LogCommandsDTO>()); }
        }

        public static IGenericDAO<Mail, MailDTO> MailDAO
        {
            get { return _mailDAO ?? (_mailDAO = new GenericDAO<Mail, MailDTO>()); }
        }

        public static IGenericDAO<Map, MapDTO> MapDAO
        {
            get { return _mapDAO ?? (_mapDAO = new GenericDAO<Map, MapDTO>()); }
        }

        public static IGenericDAO<MapMonster, MapMonsterDTO> MapMonsterDAO
        {
            get { return _mapmonsterDAO ?? (_mapmonsterDAO = new GenericDAO<MapMonster, MapMonsterDTO>()); }
        }

        public static IGenericDAO<MapNpc, MapNpcDTO> MapNpcDAO
        {
            get { return _mapnpcDAO ?? (_mapnpcDAO = new GenericDAO<MapNpc, MapNpcDTO>()); }
        }

        public static IGenericDAO<MapType, MapTypeDTO> MapTypeDAO
        {
            get { return _maptypeDAO ?? (_maptypeDAO = new GenericDAO<MapType, MapTypeDTO>()); }
        }

        public static IGenericDAO<MapTypeMap, MapTypeMapDTO> MapTypeMapDAO
        {
            get { return _maptypemapDAO ?? (_maptypemapDAO = new GenericDAO<MapTypeMap, MapTypeMapDTO>()); }
        }

        public static IGenericDAO<Mate, MateDTO> MateDAO
        {
            get { return _mateDAO ?? (_mateDAO = new GenericDAO<Mate, MateDTO>()); }
        }

        public static IGenericDAO<MinilandObject, MinilandObjectDTO> MinilandObjectDAO
        {
            get { return _minilandobjectDAO ?? (_minilandobjectDAO = new GenericDAO<MinilandObject, MinilandObjectDTO>()); }
        }

        public static IGenericDAO<NpcMonster, NpcMonsterDTO> NpcMonsterDAO
        {
            get { return _npcmonsterDAO ?? (_npcmonsterDAO = new GenericDAO<NpcMonster, NpcMonsterDTO>()); }
        }

        public static IGenericDAO<NpcMonsterSkill, NpcMonsterSkillDTO> NpcMonsterSkillDAO
        {
            get { return _npcmonsterskillDAO ?? (_npcmonsterskillDAO = new GenericDAO<NpcMonsterSkill, NpcMonsterSkillDTO>()); }
        }

        public static IGenericDAO<PenaltyLog, PenaltyLogDTO> PenaltyLogDAO
        {
            get { return _penaltylogDAO ?? (_penaltylogDAO = new GenericDAO<PenaltyLog, PenaltyLogDTO>()); }
        }

        public static IGenericDAO<Portal, PortalDTO> PortalDAO
        {
            get { return _portalDAO ?? (_portalDAO = new GenericDAO<Portal, PortalDTO>()); }
        }

        public static IGenericDAO<QuicklistEntry, QuicklistEntryDTO> QuicklistEntryDAO
        {
            get { return _quicklistDAO ?? (_quicklistDAO = new GenericDAO<QuicklistEntry, QuicklistEntryDTO>()); }
        }

        public static IGenericDAO<Recipe, RecipeDTO> RecipeDAO
        {
            get { return _recipeDAO ?? (_recipeDAO = new GenericDAO<Recipe, RecipeDTO>()); }
        }

        public static IGenericDAO<RecipeItem, RecipeItemDTO> RecipeItemDAO
        {
            get { return _recipeitemDAO ?? (_recipeitemDAO = new GenericDAO<RecipeItem, RecipeItemDTO>()); }
        }

        public static IGenericDAO<Respawn, RespawnDTO> RespawnDAO
        {
            get { return _respawnDAO ?? (_respawnDAO = new GenericDAO<Respawn, RespawnDTO>()); }
        }

        public static IGenericDAO<RespawnMapType, RespawnMapTypeDTO> RespawnMapTypeDAO
        {
            get { return _respawnMapTypeDAO ?? (_respawnMapTypeDAO = new GenericDAO<RespawnMapType, RespawnMapTypeDTO>()); }
        }

        public static IGenericDAO<Shop, ShopDTO> ShopDAO
        {
            get { return _shopDAO ?? (_shopDAO = new GenericDAO<Shop, ShopDTO>()); }
        }

        public static IGenericDAO<ShopItem, ShopItemDTO> ShopItemDAO
        {
            get { return _shopitemDAO ?? (_shopitemDAO = new GenericDAO<ShopItem, ShopItemDTO>()); }
        }

        public static IGenericDAO<ShopSkill, ShopSkillDTO> ShopSkillDAO
        {
            get { return _shopskillDAO ?? (_shopskillDAO = new GenericDAO<ShopSkill, ShopSkillDTO>()); }
        }

        public static IGenericDAO<Skill, SkillDTO> SkillDAO
        {
            get { return _skillDAO ?? (_skillDAO = new GenericDAO<Skill, SkillDTO>()); }
        }

        public static IGenericDAO<StaticBonus, StaticBonusDTO> StaticBonusDAO
        {
            get { return _staticBonusDAO ?? (_staticBonusDAO = new GenericDAO<StaticBonus, StaticBonusDTO>()); }
        }

        public static IGenericDAO<StaticBuff, StaticBuffDTO> StaticBuffDAO
        {
            get { return _staticBuffDAO ?? (_staticBuffDAO = new GenericDAO<StaticBuff, StaticBuffDTO>()); }
        }

        public static IGenericDAO<Teleporter, TeleporterDTO> TeleporterDAO
        {
            get { return _teleporterDAO ?? (_teleporterDAO = new GenericDAO<Teleporter, TeleporterDTO>()); }
        }

        public static IGenericDAO<ScriptedInstance, ScriptedInstanceDTO> ScriptedInstanceDAO
        {
            get { return _scriptedinstanceDAO ?? (_scriptedinstanceDAO = new GenericDAO<ScriptedInstance, ScriptedInstanceDTO>()); }
        }

        public static IGenericDAO<BCard, BCardDTO> BCardDAO
        {
            get { return _bcardDAO ?? (_bcardDAO = new GenericDAO<BCard, BCardDTO>()); }
        }

        public static IGenericDAO<RollGeneratedItem, RollGeneratedItemDTO> RollGeneratedItemDAO
        {
            get { return _rollGeneratedItemDAO ?? (_rollGeneratedItemDAO = new GenericDAO<RollGeneratedItem, RollGeneratedItemDTO>()); }
        }

        #endregion
    }
}