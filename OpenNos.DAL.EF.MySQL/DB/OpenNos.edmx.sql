










































-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 03/29/2016 23:36:20

-- Generated from EDMX file: C:\Users\Dominik\Source\Repos\OpenNos\OpenNos.DAL.EF.MySQL\DB\OpenNos.edmx
-- Target version: 3.0.0.0

-- --------------------------------------------------



-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------


--    ALTER TABLE `character` DROP CONSTRAINT `FK_AccountCharacter`;

--    ALTER TABLE `generallog` DROP CONSTRAINT `FK_accountlog`;

--    ALTER TABLE `portal` DROP CONSTRAINT `FK_portalMap`;

--    ALTER TABLE `portal` DROP CONSTRAINT `FK_portalMap1`;

--    ALTER TABLE `generallog` DROP CONSTRAINT `FK_GeneralLogCharacter`;

--    ALTER TABLE `character` DROP CONSTRAINT `FK_CharacterMap`;

--    ALTER TABLE `inventory` DROP CONSTRAINT `FK_CharacterInventory`;

--    ALTER TABLE `inventoryitem` DROP CONSTRAINT `FK_InventoryItemItem`;

--    ALTER TABLE `shopitem` DROP CONSTRAINT `FK_ShopItemItem`;

--    ALTER TABLE `shopitem` DROP CONSTRAINT `FK_ShopShopItem`;

--    ALTER TABLE `respawn` DROP CONSTRAINT `FK_CharacterRespawn`;

--    ALTER TABLE `inventory` DROP CONSTRAINT `FK_InventoryInventoryItem`;

--    ALTER TABLE `teleporter` DROP CONSTRAINT `FK_TeleporterMap`;

--    ALTER TABLE `mapmonster` DROP CONSTRAINT `FK_MapMapMonster`;

--    ALTER TABLE `mapnpc` DROP CONSTRAINT `FK_MapNpcNpc`;

--    ALTER TABLE `mapnpc` DROP CONSTRAINT `FK_MapMapNpc`;

--    ALTER TABLE `teleporter` DROP CONSTRAINT `FK_TeleporterMapNpc`;

--    ALTER TABLE `mapmonster` DROP CONSTRAINT `FK_MapMonsterNpcMonster`;

--    ALTER TABLE `shop` DROP CONSTRAINT `FK_ShopMapNpc`;

--    ALTER TABLE `recipe` DROP CONSTRAINT `FK_MapNpcRecipe`;

--    ALTER TABLE `recipeitem` DROP CONSTRAINT `FK_RecipeItemItem`;

--    ALTER TABLE `recipeitem` DROP CONSTRAINT `FK_RecipeRecipeItem1`;

--    ALTER TABLE `recipe` DROP CONSTRAINT `FK_ItemRecipe`;

--    ALTER TABLE `drop` DROP CONSTRAINT `FK_DropItem`;

--    ALTER TABLE `drop` DROP CONSTRAINT `FK_DropNpcMonster`;


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
SET foreign_key_checks = 0;

    DROP TABLE IF EXISTS `account`;

    DROP TABLE IF EXISTS `character`;

    DROP TABLE IF EXISTS `portal`;

    DROP TABLE IF EXISTS `generallog`;

    DROP TABLE IF EXISTS `map`;

    DROP TABLE IF EXISTS `item`;

    DROP TABLE IF EXISTS `npcmonster`;

    DROP TABLE IF EXISTS `inventoryitem`;

    DROP TABLE IF EXISTS `inventory`;

    DROP TABLE IF EXISTS `shopitem`;

    DROP TABLE IF EXISTS `shop`;

    DROP TABLE IF EXISTS `respawn`;

    DROP TABLE IF EXISTS `teleporter`;

    DROP TABLE IF EXISTS `mapmonster`;

    DROP TABLE IF EXISTS `mapnpc`;

    DROP TABLE IF EXISTS `recipe`;

    DROP TABLE IF EXISTS `recipeitem`;

    DROP TABLE IF EXISTS `drop`;

SET foreign_key_checks = 1;

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------


CREATE TABLE `account`(
	`AccountId` bigint NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` varchar (255) NOT NULL, 
	`Password` varchar (255) NOT NULL, 
	`Authority` TINYINT UNSIGNED NOT NULL, 
	`LastSession` int NOT NULL, 
	`LastCompliment` datetime NOT NULL);

ALTER TABLE `account` ADD PRIMARY KEY (AccountId);





CREATE TABLE `character`(
	`CharacterId` bigint NOT NULL AUTO_INCREMENT UNIQUE, 
	`AccountId` bigint NOT NULL, 
	`MapId` smallint NOT NULL, 
	`Name` varchar (255) NOT NULL, 
	`Slot` TINYINT UNSIGNED NOT NULL, 
	`Gender` TINYINT UNSIGNED NOT NULL, 
	`Class` TINYINT UNSIGNED NOT NULL, 
	`HairStyle` TINYINT UNSIGNED NOT NULL, 
	`HairColor` TINYINT UNSIGNED NOT NULL, 
	`MapX` smallint NOT NULL, 
	`MapY` smallint NOT NULL, 
	`Hp` int NOT NULL, 
	`Mp` int NOT NULL, 
	`ArenaWinner` int NOT NULL, 
	`Reput` bigint NOT NULL, 
	`Dignite` smallint NOT NULL, 
	`Gold` bigint NOT NULL, 
	`Backpack` int NOT NULL, 
	`Level` TINYINT UNSIGNED NOT NULL, 
	`LevelXp` int NOT NULL, 
	`JobLevel` TINYINT UNSIGNED NOT NULL, 
	`JobLevelXp` int NOT NULL, 
	`Act4Dead` int NOT NULL, 
	`Act4Kill` int NOT NULL, 
	`Faction` int NOT NULL, 
	`SpPoint` int NOT NULL, 
	`SpAdditionPoint` int NOT NULL, 
	`State` TINYINT UNSIGNED NOT NULL, 
	`Compliment` smallint NOT NULL, 
	`ExchangeBlocked` bool NOT NULL, 
	`FriendRequestBlocked` bool NOT NULL, 
	`WhisperBlocked` bool NOT NULL, 
	`GroupRequestBlocked` bool NOT NULL, 
	`MouseAimLock` bool NOT NULL, 
	`HeroChatBlocked` bool NOT NULL, 
	`EmoticonsBlocked` bool NOT NULL, 
	`QuickGetUp` bool NOT NULL, 
	`HpBlocked` bool NOT NULL, 
	`BuffBlocked` bool NOT NULL, 
	`MinilandInviteBlocked` bool NOT NULL, 
	`FamilyRequestBlocked` bool NOT NULL, 
	`Act4Points` int NOT NULL, 
	`TalentWin` int NOT NULL, 
	`TalentLose` int NOT NULL, 
	`TalentSurrender` int NOT NULL, 
	`MasterPoints` int NOT NULL, 
	`MasterTicket` int NOT NULL);

ALTER TABLE `character` ADD PRIMARY KEY (CharacterId);





CREATE TABLE `portal`(
	`PortalId` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`SourceX` smallint NOT NULL, 
	`SourceY` smallint NOT NULL, 
	`DestinationX` smallint NOT NULL, 
	`DestinationY` smallint NOT NULL, 
	`Type` tinyint NOT NULL, 
	`DestinationMapId` smallint NOT NULL, 
	`SourceMapId` smallint NOT NULL, 
	`IsDisabled` bool NOT NULL);

ALTER TABLE `portal` ADD PRIMARY KEY (PortalId);





CREATE TABLE `generallog`(
	`LogId` bigint NOT NULL AUTO_INCREMENT UNIQUE, 
	`AccountId` bigint NOT NULL, 
	`IpAddress` longtext NOT NULL, 
	`Timestamp` datetime NOT NULL, 
	`LogType` longtext NOT NULL, 
	`LogData` longtext NOT NULL, 
	`CharacterId` bigint);

ALTER TABLE `generallog` ADD PRIMARY KEY (LogId);





CREATE TABLE `map`(
	`MapId` smallint NOT NULL, 
	`Name` longtext NOT NULL, 
	`Data` longblob NOT NULL, 
	`Music` int NOT NULL);

ALTER TABLE `map` ADD PRIMARY KEY (MapId);





CREATE TABLE `item`(
	`VNum` smallint NOT NULL, 
	`Price` bigint NOT NULL, 
	`ReputPrice` bigint NOT NULL, 
	`Name` longtext NOT NULL, 
	`ItemType` TINYINT UNSIGNED NOT NULL, 
	`ItemSubType` TINYINT UNSIGNED NOT NULL, 
	`EquipmentSlot` TINYINT UNSIGNED NOT NULL, 
	`Morph` smallint NOT NULL, 
	`Type` TINYINT UNSIGNED NOT NULL, 
	`Class` TINYINT UNSIGNED NOT NULL, 
	`IsBlocked` bool NOT NULL, 
	`IsDroppable` bool NOT NULL, 
	`IsTradable` bool NOT NULL, 
	`IsSoldable` bool NOT NULL, 
	`IsMinilandObject` bool NOT NULL, 
	`IsWarehouse` bool NOT NULL, 
	`IsColored` bool NOT NULL, 
	`IsConsumable` bool NOT NULL, 
	`LevelMinimum` TINYINT UNSIGNED NOT NULL, 
	`DamageMinimum` smallint NOT NULL, 
	`DamageMaximum` smallint NOT NULL, 
	`Concentrate` smallint NOT NULL, 
	`HitRate` smallint NOT NULL, 
	`CriticalRate` smallint NOT NULL, 
	`CriticalLuckRate` TINYINT UNSIGNED NOT NULL, 
	`CloseDefence` smallint NOT NULL, 
	`DistanceDefence` smallint NOT NULL, 
	`MagicDefence` smallint NOT NULL, 
	`DistanceDefenceDodge` smallint NOT NULL, 
	`DefenceDodge` smallint NOT NULL, 
	`Hp` smallint NOT NULL, 
	`Mp` smallint NOT NULL, 
	`LevelJobMinimum` TINYINT UNSIGNED NOT NULL, 
	`MaxCellon` TINYINT UNSIGNED NOT NULL, 
	`MaxCellonLvl` TINYINT UNSIGNED NOT NULL, 
	`FireResistance` TINYINT UNSIGNED NOT NULL, 
	`WaterResistance` TINYINT UNSIGNED NOT NULL, 
	`LightResistance` TINYINT UNSIGNED NOT NULL, 
	`DarkResistance` TINYINT UNSIGNED NOT NULL, 
	`DarkElement` TINYINT UNSIGNED NOT NULL, 
	`LightElement` TINYINT UNSIGNED NOT NULL, 
	`FireElement` TINYINT UNSIGNED NOT NULL, 
	`WaterElement` TINYINT UNSIGNED NOT NULL, 
	`PvpStrength` TINYINT UNSIGNED NOT NULL, 
	`Speed` TINYINT UNSIGNED NOT NULL, 
	`Element` TINYINT UNSIGNED NOT NULL, 
	`ElementRate` smallint NOT NULL, 
	`PvpDefence` smallint NOT NULL, 
	`ReduceOposantResistance` smallint NOT NULL, 
	`HpRegeneration` smallint NOT NULL, 
	`MpRegeneration` smallint NOT NULL, 
	`MoreHp` smallint NOT NULL, 
	`MoreMp` smallint NOT NULL, 
	`ReputationMinimum` TINYINT UNSIGNED NOT NULL, 
	`FairyMaximumLevel` TINYINT UNSIGNED NOT NULL, 
	`MaximumAmmo` TINYINT UNSIGNED NOT NULL, 
	`BasicUpgrade` TINYINT UNSIGNED NOT NULL, 
	`Color` TINYINT UNSIGNED NOT NULL, 
	`ItemValidTime` bigint NOT NULL, 
	`Effect` smallint NOT NULL, 
	`EffectValue` int NOT NULL, 
	`CellonLvl` TINYINT UNSIGNED NOT NULL, 
	`SpType` TINYINT UNSIGNED NOT NULL, 
	`Sex` TINYINT UNSIGNED NOT NULL, 
	`SecondaryElement` TINYINT UNSIGNED NOT NULL);

ALTER TABLE `item` ADD PRIMARY KEY (VNum);





CREATE TABLE `npcmonster`(
	`NpcMonsterVNum` smallint NOT NULL, 
	`Name` longtext NOT NULL, 
	`Speed` TINYINT UNSIGNED NOT NULL, 
	`Level` TINYINT UNSIGNED NOT NULL, 
	`AttackClass` TINYINT UNSIGNED NOT NULL, 
	`AttackUpgrade` TINYINT UNSIGNED NOT NULL, 
	`DamageMinimum` smallint NOT NULL, 
	`DamageMaximum` smallint NOT NULL, 
	`Concentrate` smallint NOT NULL, 
	`Element` TINYINT UNSIGNED NOT NULL, 
	`ElementRate` smallint NOT NULL, 
	`CriticalRate` smallint NOT NULL, 
	`CriticalLuckRate` TINYINT UNSIGNED NOT NULL, 
	`CloseDefence` smallint NOT NULL, 
	`DefenceDodge` smallint NOT NULL, 
	`MagicDefence` smallint NOT NULL, 
	`DefenceUpgrade` TINYINT UNSIGNED NOT NULL, 
	`DistanceDefence` smallint NOT NULL, 
	`DistanceDefenceDodge` smallint NOT NULL, 
	`FireResistance` tinyint NOT NULL, 
	`WaterResistance` tinyint NOT NULL, 
	`LightResistance` tinyint NOT NULL, 
	`DarkResistance` tinyint NOT NULL, 
	`MaxHP` smallint NOT NULL, 
	`MaxMP` smallint NOT NULL);

ALTER TABLE `npcmonster` ADD PRIMARY KEY (NpcMonsterVNum);





CREATE TABLE `inventoryitem`(
	`InventoryItemId` bigint NOT NULL AUTO_INCREMENT UNIQUE, 
	`ItemVNum` smallint NOT NULL, 
	`Amount` TINYINT UNSIGNED NOT NULL, 
	`Rare` TINYINT UNSIGNED NOT NULL, 
	`Upgrade` TINYINT UNSIGNED NOT NULL, 
	`Design` smallint NOT NULL, 
	`DamageMinimum` smallint NOT NULL, 
	`DamageMaximum` smallint NOT NULL, 
	`Concentrate` smallint NOT NULL, 
	`HitRate` smallint NOT NULL, 
	`ElementRate` smallint NOT NULL, 
	`CriticalRate` smallint NOT NULL, 
	`CriticalLuckRate` TINYINT UNSIGNED NOT NULL, 
	`CloseDefence` smallint NOT NULL, 
	`DistanceDefence` smallint NOT NULL, 
	`MagicDefence` smallint NOT NULL, 
	`DistanceDefenceDodge` smallint NOT NULL, 
	`DefenceDodge` smallint NOT NULL, 
	`CriticalDodge` smallint NOT NULL, 
	`HP` smallint NOT NULL, 
	`MP` smallint NOT NULL, 
	`DarkElement` TINYINT UNSIGNED NOT NULL, 
	`LightElement` TINYINT UNSIGNED NOT NULL, 
	`WaterElement` TINYINT UNSIGNED NOT NULL, 
	`FireElement` TINYINT UNSIGNED NOT NULL, 
	`Ammo` TINYINT UNSIGNED NOT NULL, 
	`IsEmpty` bool NOT NULL, 
	`IsFixed` bool NOT NULL, 
	`IsUsed` bool NOT NULL, 
	`ItemDeleteTime` datetime, 
	`Cellon` TINYINT UNSIGNED NOT NULL, 
	`FireResistance` TINYINT UNSIGNED NOT NULL, 
	`WaterResistance` TINYINT UNSIGNED NOT NULL, 
	`LightResistance` TINYINT UNSIGNED NOT NULL, 
	`DarkResistance` TINYINT UNSIGNED NOT NULL, 
	`SpLevel` TINYINT UNSIGNED NOT NULL, 
	`SpXp` smallint NOT NULL, 
	`SlHP` smallint NOT NULL, 
	`SlDamage` smallint NOT NULL, 
	`SlElement` smallint NOT NULL, 
	`SlDefence` smallint NOT NULL, 
	`SpHP` TINYINT UNSIGNED NOT NULL, 
	`SpDamage` TINYINT UNSIGNED NOT NULL, 
	`SpElement` TINYINT UNSIGNED NOT NULL, 
	`SpDefence` TINYINT UNSIGNED NOT NULL, 
	`SpFire` TINYINT UNSIGNED NOT NULL, 
	`SpWater` TINYINT UNSIGNED NOT NULL, 
	`SpLight` TINYINT UNSIGNED NOT NULL, 
	`SpDark` TINYINT UNSIGNED NOT NULL, 
	`SpStoneUpgrade` TINYINT UNSIGNED NOT NULL);

ALTER TABLE `inventoryitem` ADD PRIMARY KEY (InventoryItemId);





CREATE TABLE `inventory`(
	`InventoryId` bigint NOT NULL AUTO_INCREMENT UNIQUE, 
	`CharacterId` bigint NOT NULL, 
	`Type` TINYINT UNSIGNED NOT NULL, 
	`Slot` smallint NOT NULL, 
	`inventoryitem_InventoryItemId` bigint NOT NULL);

ALTER TABLE `inventory` ADD PRIMARY KEY (InventoryId);





CREATE TABLE `shopitem`(
	`ShopItemId` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Type` TINYINT UNSIGNED NOT NULL, 
	`Slot` TINYINT UNSIGNED NOT NULL, 
	`ItemVNum` smallint NOT NULL, 
	`Upgrade` TINYINT UNSIGNED NOT NULL, 
	`Rare` TINYINT UNSIGNED NOT NULL, 
	`Color` TINYINT UNSIGNED NOT NULL, 
	`ShopId` int NOT NULL);

ALTER TABLE `shopitem` ADD PRIMARY KEY (ShopItemId);





CREATE TABLE `shop`(
	`ShopId` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` longtext NOT NULL, 
	`MenuType` TINYINT UNSIGNED NOT NULL, 
	`ShopType` TINYINT UNSIGNED NOT NULL, 
	`MapNpcId` int NOT NULL);

ALTER TABLE `shop` ADD PRIMARY KEY (ShopId);





CREATE TABLE `respawn`(
	`RespawnId` bigint NOT NULL AUTO_INCREMENT UNIQUE, 
	`X` smallint NOT NULL, 
	`Y` smallint NOT NULL, 
	`MapId` smallint NOT NULL, 
	`RespawnType` TINYINT UNSIGNED NOT NULL, 
	`CharacterId` bigint NOT NULL);

ALTER TABLE `respawn` ADD PRIMARY KEY (RespawnId);





CREATE TABLE `teleporter`(
	`TeleporterId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`Index` smallint NOT NULL, 
	`MapX` smallint NOT NULL, 
	`MapY` smallint NOT NULL, 
	`MapId` smallint NOT NULL, 
	`MapNpcId` int NOT NULL);

ALTER TABLE `teleporter` ADD PRIMARY KEY (TeleporterId);





CREATE TABLE `mapmonster`(
	`MapMonsterId` int NOT NULL, 
	`MonsterVNum` smallint NOT NULL, 
	`MapId` smallint NOT NULL, 
	`MapX` smallint NOT NULL, 
	`MapY` smallint NOT NULL, 
	`Position` TINYINT UNSIGNED NOT NULL, 
	`Move` bool NOT NULL);

ALTER TABLE `mapmonster` ADD PRIMARY KEY (MapMonsterId);





CREATE TABLE `mapnpc`(
	`MapNpcId` int NOT NULL, 
	`NpcVNum` smallint NOT NULL, 
	`MapId` smallint NOT NULL, 
	`MapX` smallint NOT NULL, 
	`MapY` smallint NOT NULL, 
	`Move` bool NOT NULL, 
	`Position` TINYINT UNSIGNED NOT NULL, 
	`IsSitting` bool NOT NULL, 
	`EffectDelay` smallint NOT NULL, 
	`Effect` smallint NOT NULL, 
	`Dialog` smallint NOT NULL);

ALTER TABLE `mapnpc` ADD PRIMARY KEY (MapNpcId);





CREATE TABLE `recipe`(
	`RecipeId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`MapNpcId` int NOT NULL, 
	`ItemVNum` smallint NOT NULL, 
	`Amount` TINYINT UNSIGNED NOT NULL);

ALTER TABLE `recipe` ADD PRIMARY KEY (RecipeId);





CREATE TABLE `recipeitem`(
	`RecipeItemId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`ItemVNum` smallint NOT NULL, 
	`Amount` TINYINT UNSIGNED NOT NULL, 
	`RecipeId` smallint NOT NULL);

ALTER TABLE `recipeitem` ADD PRIMARY KEY (RecipeItemId);





CREATE TABLE `drop`(
	`DropId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`DropChance` int NOT NULL, 
	`Amount` int NOT NULL, 
	`ItemVNum` smallint NOT NULL, 
	`MonsterVNum` smallint NOT NULL);

ALTER TABLE `drop` ADD PRIMARY KEY (DropId);







-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------


-- Creating foreign key on `AccountId` in table 'character'

ALTER TABLE `character`
ADD CONSTRAINT `FK_AccountCharacter`
    FOREIGN KEY (`AccountId`)
    REFERENCES `account`
        (`AccountId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_AccountCharacter'

CREATE INDEX `IX_FK_AccountCharacter`
    ON `character`
    (`AccountId`);



-- Creating foreign key on `AccountId` in table 'generallog'

ALTER TABLE `generallog`
ADD CONSTRAINT `FK_accountlog`
    FOREIGN KEY (`AccountId`)
    REFERENCES `account`
        (`AccountId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_accountlog'

CREATE INDEX `IX_FK_accountlog`
    ON `generallog`
    (`AccountId`);



-- Creating foreign key on `DestinationMapId` in table 'portal'

ALTER TABLE `portal`
ADD CONSTRAINT `FK_portalMap`
    FOREIGN KEY (`DestinationMapId`)
    REFERENCES `map`
        (`MapId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_portalMap'

CREATE INDEX `IX_FK_portalMap`
    ON `portal`
    (`DestinationMapId`);



-- Creating foreign key on `SourceMapId` in table 'portal'

ALTER TABLE `portal`
ADD CONSTRAINT `FK_portalMap1`
    FOREIGN KEY (`SourceMapId`)
    REFERENCES `map`
        (`MapId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_portalMap1'

CREATE INDEX `IX_FK_portalMap1`
    ON `portal`
    (`SourceMapId`);



-- Creating foreign key on `CharacterId` in table 'generallog'

ALTER TABLE `generallog`
ADD CONSTRAINT `FK_GeneralLogCharacter`
    FOREIGN KEY (`CharacterId`)
    REFERENCES `character`
        (`CharacterId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_GeneralLogCharacter'

CREATE INDEX `IX_FK_GeneralLogCharacter`
    ON `generallog`
    (`CharacterId`);



-- Creating foreign key on `MapId` in table 'character'

ALTER TABLE `character`
ADD CONSTRAINT `FK_CharacterMap`
    FOREIGN KEY (`MapId`)
    REFERENCES `map`
        (`MapId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_CharacterMap'

CREATE INDEX `IX_FK_CharacterMap`
    ON `character`
    (`MapId`);



-- Creating foreign key on `CharacterId` in table 'inventory'

ALTER TABLE `inventory`
ADD CONSTRAINT `FK_CharacterInventory`
    FOREIGN KEY (`CharacterId`)
    REFERENCES `character`
        (`CharacterId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_CharacterInventory'

CREATE INDEX `IX_FK_CharacterInventory`
    ON `inventory`
    (`CharacterId`);



-- Creating foreign key on `ItemVNum` in table 'inventoryitem'

ALTER TABLE `inventoryitem`
ADD CONSTRAINT `FK_InventoryItemItem`
    FOREIGN KEY (`ItemVNum`)
    REFERENCES `item`
        (`VNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_InventoryItemItem'

CREATE INDEX `IX_FK_InventoryItemItem`
    ON `inventoryitem`
    (`ItemVNum`);



-- Creating foreign key on `ItemVNum` in table 'shopitem'

ALTER TABLE `shopitem`
ADD CONSTRAINT `FK_ShopItemItem`
    FOREIGN KEY (`ItemVNum`)
    REFERENCES `item`
        (`VNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_ShopItemItem'

CREATE INDEX `IX_FK_ShopItemItem`
    ON `shopitem`
    (`ItemVNum`);



-- Creating foreign key on `ShopId` in table 'shopitem'

ALTER TABLE `shopitem`
ADD CONSTRAINT `FK_ShopShopItem`
    FOREIGN KEY (`ShopId`)
    REFERENCES `shop`
        (`ShopId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_ShopShopItem'

CREATE INDEX `IX_FK_ShopShopItem`
    ON `shopitem`
    (`ShopId`);



-- Creating foreign key on `CharacterId` in table 'respawn'

ALTER TABLE `respawn`
ADD CONSTRAINT `FK_CharacterRespawn`
    FOREIGN KEY (`CharacterId`)
    REFERENCES `character`
        (`CharacterId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_CharacterRespawn'

CREATE INDEX `IX_FK_CharacterRespawn`
    ON `respawn`
    (`CharacterId`);



-- Creating foreign key on `inventoryitem_InventoryItemId` in table 'inventory'

ALTER TABLE `inventory`
ADD CONSTRAINT `FK_InventoryInventoryItem`
    FOREIGN KEY (`inventoryitem_InventoryItemId`)
    REFERENCES `inventoryitem`
        (`InventoryItemId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_InventoryInventoryItem'

CREATE INDEX `IX_FK_InventoryInventoryItem`
    ON `inventory`
    (`inventoryitem_InventoryItemId`);



-- Creating foreign key on `MapId` in table 'teleporter'

ALTER TABLE `teleporter`
ADD CONSTRAINT `FK_TeleporterMap`
    FOREIGN KEY (`MapId`)
    REFERENCES `map`
        (`MapId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_TeleporterMap'

CREATE INDEX `IX_FK_TeleporterMap`
    ON `teleporter`
    (`MapId`);



-- Creating foreign key on `MapId` in table 'mapmonster'

ALTER TABLE `mapmonster`
ADD CONSTRAINT `FK_MapMapMonster`
    FOREIGN KEY (`MapId`)
    REFERENCES `map`
        (`MapId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_MapMapMonster'

CREATE INDEX `IX_FK_MapMapMonster`
    ON `mapmonster`
    (`MapId`);



-- Creating foreign key on `NpcVNum` in table 'mapnpc'

ALTER TABLE `mapnpc`
ADD CONSTRAINT `FK_MapNpcNpc`
    FOREIGN KEY (`NpcVNum`)
    REFERENCES `npcmonster`
        (`NpcMonsterVNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_MapNpcNpc'

CREATE INDEX `IX_FK_MapNpcNpc`
    ON `mapnpc`
    (`NpcVNum`);



-- Creating foreign key on `MapId` in table 'mapnpc'

ALTER TABLE `mapnpc`
ADD CONSTRAINT `FK_MapMapNpc`
    FOREIGN KEY (`MapId`)
    REFERENCES `map`
        (`MapId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_MapMapNpc'

CREATE INDEX `IX_FK_MapMapNpc`
    ON `mapnpc`
    (`MapId`);



-- Creating foreign key on `MapNpcId` in table 'teleporter'

ALTER TABLE `teleporter`
ADD CONSTRAINT `FK_TeleporterMapNpc`
    FOREIGN KEY (`MapNpcId`)
    REFERENCES `mapnpc`
        (`MapNpcId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_TeleporterMapNpc'

CREATE INDEX `IX_FK_TeleporterMapNpc`
    ON `teleporter`
    (`MapNpcId`);



-- Creating foreign key on `MonsterVNum` in table 'mapmonster'

ALTER TABLE `mapmonster`
ADD CONSTRAINT `FK_MapMonsterNpcMonster`
    FOREIGN KEY (`MonsterVNum`)
    REFERENCES `npcmonster`
        (`NpcMonsterVNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_MapMonsterNpcMonster'

CREATE INDEX `IX_FK_MapMonsterNpcMonster`
    ON `mapmonster`
    (`MonsterVNum`);



-- Creating foreign key on `MapNpcId` in table 'shop'

ALTER TABLE `shop`
ADD CONSTRAINT `FK_ShopMapNpc`
    FOREIGN KEY (`MapNpcId`)
    REFERENCES `mapnpc`
        (`MapNpcId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_ShopMapNpc'

CREATE INDEX `IX_FK_ShopMapNpc`
    ON `shop`
    (`MapNpcId`);



-- Creating foreign key on `MapNpcId` in table 'recipe'

ALTER TABLE `recipe`
ADD CONSTRAINT `FK_MapNpcRecipe`
    FOREIGN KEY (`MapNpcId`)
    REFERENCES `mapnpc`
        (`MapNpcId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_MapNpcRecipe'

CREATE INDEX `IX_FK_MapNpcRecipe`
    ON `recipe`
    (`MapNpcId`);



-- Creating foreign key on `ItemVNum` in table 'recipeitem'

ALTER TABLE `recipeitem`
ADD CONSTRAINT `FK_RecipeItemItem`
    FOREIGN KEY (`ItemVNum`)
    REFERENCES `item`
        (`VNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_RecipeItemItem'

CREATE INDEX `IX_FK_RecipeItemItem`
    ON `recipeitem`
    (`ItemVNum`);



-- Creating foreign key on `RecipeId` in table 'recipeitem'

ALTER TABLE `recipeitem`
ADD CONSTRAINT `FK_RecipeRecipeItem1`
    FOREIGN KEY (`RecipeId`)
    REFERENCES `recipe`
        (`RecipeId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_RecipeRecipeItem1'

CREATE INDEX `IX_FK_RecipeRecipeItem1`
    ON `recipeitem`
    (`RecipeId`);



-- Creating foreign key on `ItemVNum` in table 'recipe'

ALTER TABLE `recipe`
ADD CONSTRAINT `FK_ItemRecipe`
    FOREIGN KEY (`ItemVNum`)
    REFERENCES `item`
        (`VNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_ItemRecipe'

CREATE INDEX `IX_FK_ItemRecipe`
    ON `recipe`
    (`ItemVNum`);



-- Creating foreign key on `ItemVNum` in table 'drop'

ALTER TABLE `drop`
ADD CONSTRAINT `FK_DropItem`
    FOREIGN KEY (`ItemVNum`)
    REFERENCES `item`
        (`VNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_DropItem'

CREATE INDEX `IX_FK_DropItem`
    ON `drop`
    (`ItemVNum`);



-- Creating foreign key on `MonsterVNum` in table 'drop'

ALTER TABLE `drop`
ADD CONSTRAINT `FK_DropNpcMonster`
    FOREIGN KEY (`MonsterVNum`)
    REFERENCES `npcmonster`
        (`NpcMonsterVNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_DropNpcMonster'

CREATE INDEX `IX_FK_DropNpcMonster`
    ON `drop`
    (`MonsterVNum`);



-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
