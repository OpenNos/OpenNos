










































-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 12/21/2015 23:18:09

-- Generated from EDMX file: C:\Users\ERWAN\Desktop\OpenNos GIT\OpenNos.DAL.EF.MySQL\DB\OpenNos.edmx
-- Target version: 3.0.0.0

-- --------------------------------------------------



-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------


--    ALTER TABLE `character` DROP CONSTRAINT `FK_AccountCharacter`;

--    ALTER TABLE `connectionlog` DROP CONSTRAINT `FK_accountlog`;

--    ALTER TABLE `portal` DROP CONSTRAINT `FK_portalMap`;

--    ALTER TABLE `portal` DROP CONSTRAINT `FK_portalMap1`;

--    ALTER TABLE `connectionlog` DROP CONSTRAINT `FK_GeneralLogCharacter`;

--    ALTER TABLE `character` DROP CONSTRAINT `FK_CharacterMap`;

--    ALTER TABLE `npc` DROP CONSTRAINT `FK_NpcMap`;

--    ALTER TABLE `iteminstance` DROP CONSTRAINT `FK_ItemListItem`;

--    ALTER TABLE `inventory` DROP CONSTRAINT `FK_CharacterInventory`;

--    ALTER TABLE `inventory` DROP CONSTRAINT `FK_InventoryItem`;


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
SET foreign_key_checks = 0;

    DROP TABLE IF EXISTS `account`;

    DROP TABLE IF EXISTS `character`;

    DROP TABLE IF EXISTS `portal`;

    DROP TABLE IF EXISTS `connectionlog`;

    DROP TABLE IF EXISTS `map`;

    DROP TABLE IF EXISTS `item`;

    DROP TABLE IF EXISTS `npc`;

    DROP TABLE IF EXISTS `iteminstance`;

    DROP TABLE IF EXISTS `inventory`;

SET foreign_key_checks = 1;

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------


CREATE TABLE `account`(
	`AccountId` bigint NOT NULL, 
	`Name` varchar (255) NOT NULL, 
	`Password` varchar (255) NOT NULL, 
	`Authority` smallint NOT NULL, 
	`LastSession` int NOT NULL, 
	`LoggedIn` bool NOT NULL);

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
	`Reput` int NOT NULL, 
	`Dignite` int NOT NULL, 
	`Gold` bigint NOT NULL, 
	`Backpack` int NOT NULL, 
	`Level` TINYINT UNSIGNED NOT NULL, 
	`LevelXp` int NOT NULL, 
	`JobLevel` TINYINT UNSIGNED NOT NULL, 
	`JobLevelXp` int NOT NULL, 
	`Dead` int NOT NULL, 
	`Kill` int NOT NULL, 
	`Contribution` int NOT NULL, 
	`Faction` int NOT NULL, 
	`State` TINYINT UNSIGNED NOT NULL);

ALTER TABLE `character` ADD PRIMARY KEY (CharacterId);





CREATE TABLE `portal`(
	`PortalId` int NOT NULL, 
	`SourceX` smallint NOT NULL, 
	`SourceY` smallint NOT NULL, 
	`DestinationX` smallint NOT NULL, 
	`DestinationY` smallint NOT NULL, 
	`Type` smallint NOT NULL, 
	`DestinationMapId` smallint NOT NULL, 
	`SourceMapId` smallint NOT NULL);

ALTER TABLE `portal` ADD PRIMARY KEY (PortalId);





CREATE TABLE `connectionlog`(
	`LogId` bigint NOT NULL AUTO_INCREMENT UNIQUE, 
	`AccountId` bigint NOT NULL, 
	`IpAddress` longtext NOT NULL, 
	`Timestamp` datetime NOT NULL, 
	`LogType` longtext NOT NULL, 
	`LogData` longtext NOT NULL, 
	`CharacterId` bigint);

ALTER TABLE `connectionlog` ADD PRIMARY KEY (LogId);





CREATE TABLE `map`(
	`MapId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` longtext NOT NULL);

ALTER TABLE `map` ADD PRIMARY KEY (MapId);





CREATE TABLE `item`(
	`VNum` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`Price` bigint NOT NULL, 
	`Name` longtext NOT NULL, 
	`Inventory` TINYINT UNSIGNED NOT NULL, 
	`ItemType` TINYINT UNSIGNED NOT NULL, 
	`EquipmentSlot` TINYINT UNSIGNED NOT NULL, 
	`Morph` TINYINT UNSIGNED NOT NULL, 
	`Type` TINYINT UNSIGNED NOT NULL, 
	`Classe` TINYINT UNSIGNED NOT NULL, 
	`Blocked` TINYINT UNSIGNED NOT NULL, 
	`Droppable` TINYINT UNSIGNED NOT NULL, 
	`Transaction` TINYINT UNSIGNED NOT NULL, 
	`Soldable` TINYINT UNSIGNED NOT NULL, 
	`MinilandObject` TINYINT UNSIGNED NOT NULL, 
	`isWareHouse` TINYINT UNSIGNED NOT NULL, 
	`LevelMinimum` smallint NOT NULL, 
	`DamageMinimum` smallint NOT NULL, 
	`DamageMaximum` smallint NOT NULL, 
	`Concentrate` smallint NOT NULL, 
	`HitRate` smallint NOT NULL, 
	`CriticalLuckRate` smallint NOT NULL, 
	`CriticalRate` smallint NOT NULL, 
	`RangeDefence` smallint NOT NULL, 
	`DistanceDefence` smallint NOT NULL, 
	`MagicDefence` smallint NOT NULL, 
	`Dodge` longtext NOT NULL, 
	`Hp` smallint NOT NULL, 
	`Mp` smallint NOT NULL, 
	`MaxCellon` smallint NOT NULL, 
	`MaxCellonLvl` smallint NOT NULL, 
	`FireResistance` smallint NOT NULL, 
	`WaterResistance` smallint NOT NULL, 
	`LightResistance` smallint NOT NULL, 
	`DarkResistance` smallint NOT NULL, 
	`DarkElement` smallint NOT NULL, 
	`LightElement` smallint NOT NULL, 
	`FireElement` smallint NOT NULL, 
	`WaterElement` smallint NOT NULL, 
	`PvpStrength` smallint NOT NULL, 
	`Speed` smallint NOT NULL, 
	`Element` smallint NOT NULL, 
	`ElementRate` smallint NOT NULL, 
	`PvpDefence` smallint NOT NULL, 
	`DimOposantResistance` smallint NOT NULL, 
	`HpRegeneration` longtext NOT NULL, 
	`MpRegeneration` longtext NOT NULL, 
	`MoreHp` smallint NOT NULL, 
	`MoreMp` smallint NOT NULL, 
	`Colored` bool NOT NULL, 
	`isConsumable` bool NOT NULL);

ALTER TABLE `item` ADD PRIMARY KEY (VNum);





CREATE TABLE `npc`(
	`NpcId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` longtext NOT NULL, 
	`Vnum` smallint NOT NULL, 
	`Dialog` smallint NOT NULL, 
	`MapId` smallint NOT NULL, 
	`MapX` smallint NOT NULL, 
	`MapY` smallint NOT NULL, 
	`Position` smallint NOT NULL, 
	`Level` smallint NOT NULL);

ALTER TABLE `npc` ADD PRIMARY KEY (NpcId);





CREATE TABLE `iteminstance`(
	`ItemId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`DamageMinimum` smallint NOT NULL, 
	`DamageMaximum` smallint NOT NULL, 
	`Concentrate` smallint NOT NULL, 
	`HitRate` smallint NOT NULL, 
	`CriticalLuckRate` smallint NOT NULL, 
	`CriticalRate` smallint NOT NULL, 
	`RangeDefence` smallint NOT NULL, 
	`DistanceDefence` smallint NOT NULL, 
	`MagicDefence` smallint NOT NULL, 
	`Dodge` longtext NOT NULL, 
	`ElementRate` smallint NOT NULL, 
	`Upgrade` smallint NOT NULL, 
	`Rare` smallint NOT NULL, 
	`Color` longtext NOT NULL, 
	`Amount` longtext NOT NULL, 
	`Level` smallint NOT NULL, 
	`SlElement` smallint NOT NULL, 
	`SlHit` smallint NOT NULL, 
	`SlDefence` smallint NOT NULL, 
	`SlHP` smallint NOT NULL, 
	`DarkElement` smallint NOT NULL, 
	`LightElement` smallint NOT NULL, 
	`WaterElement` smallint NOT NULL, 
	`FireElement` smallint NOT NULL, 
	`ItemVNum` smallint NOT NULL);

ALTER TABLE `iteminstance` ADD PRIMARY KEY (ItemId);





CREATE TABLE `inventory`(
	`InventoryId` bigint NOT NULL AUTO_INCREMENT UNIQUE, 
	`CharacterId` bigint NOT NULL, 
	`Type` smallint NOT NULL, 
	`Slot` smallint NOT NULL, 
	`item_ItemId` smallint NOT NULL);

ALTER TABLE `inventory` ADD PRIMARY KEY (InventoryId);







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



-- Creating foreign key on `AccountId` in table 'connectionlog'

ALTER TABLE `connectionlog`
ADD CONSTRAINT `FK_accountlog`
    FOREIGN KEY (`AccountId`)
    REFERENCES `account`
        (`AccountId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_accountlog'

CREATE INDEX `IX_FK_accountlog`
    ON `connectionlog`
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



-- Creating foreign key on `CharacterId` in table 'connectionlog'

ALTER TABLE `connectionlog`
ADD CONSTRAINT `FK_GeneralLogCharacter`
    FOREIGN KEY (`CharacterId`)
    REFERENCES `character`
        (`CharacterId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_GeneralLogCharacter'

CREATE INDEX `IX_FK_GeneralLogCharacter`
    ON `connectionlog`
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



-- Creating foreign key on `MapId` in table 'npc'

ALTER TABLE `npc`
ADD CONSTRAINT `FK_NpcMap`
    FOREIGN KEY (`MapId`)
    REFERENCES `map`
        (`MapId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_NpcMap'

CREATE INDEX `IX_FK_NpcMap`
    ON `npc`
    (`MapId`);



-- Creating foreign key on `ItemVNum` in table 'iteminstance'

ALTER TABLE `iteminstance`
ADD CONSTRAINT `FK_ItemListItem`
    FOREIGN KEY (`ItemVNum`)
    REFERENCES `item`
        (`VNum`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_ItemListItem'

CREATE INDEX `IX_FK_ItemListItem`
    ON `iteminstance`
    (`ItemVNum`);



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



-- Creating foreign key on `item_ItemId` in table 'inventory'

ALTER TABLE `inventory`
ADD CONSTRAINT `FK_InventoryItem`
    FOREIGN KEY (`item_ItemId`)
    REFERENCES `iteminstance`
        (`ItemId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_InventoryItem'

CREATE INDEX `IX_FK_InventoryItem`
    ON `inventory`
    (`item_ItemId`);



-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
