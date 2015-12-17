










































-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 12/16/2015 12:06:55

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


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
SET foreign_key_checks = 0;

    DROP TABLE IF EXISTS `account`;

    DROP TABLE IF EXISTS `character`;

    DROP TABLE IF EXISTS `portal`;

    DROP TABLE IF EXISTS `connectionlog`;

    DROP TABLE IF EXISTS `map`;

    DROP TABLE IF EXISTS `itemlist`;

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
	`MapId` smallint NOT NULL);

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





CREATE TABLE `itemlist`(
	`ItemId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`Price` bigint NOT NULL, 
	`Name` longtext NOT NULL, 
	`Inventory` TINYINT UNSIGNED NOT NULL, 
	`ItemType` TINYINT UNSIGNED NOT NULL, 
	`EqSlot` TINYINT UNSIGNED NOT NULL, 
	`Morph` TINYINT UNSIGNED NOT NULL, 
	`Type` TINYINT UNSIGNED NOT NULL, 
	`Classe` TINYINT UNSIGNED NOT NULL, 
	`Blocked` TINYINT UNSIGNED NOT NULL, 
	`Droppable` TINYINT UNSIGNED NOT NULL, 
	`Transaction` TINYINT UNSIGNED NOT NULL, 
	`Soldable` TINYINT UNSIGNED NOT NULL, 
	`MinilandObject` TINYINT UNSIGNED NOT NULL, 
	`isWareHouse` TINYINT UNSIGNED NOT NULL, 
	`LvlMin` smallint NOT NULL, 
	`DamageMin` smallint NOT NULL, 
	`DamageMax` smallint NOT NULL, 
	`Concentrate` smallint NOT NULL, 
	`HitRate` smallint NOT NULL, 
	`CriticalLuckRate` smallint NOT NULL, 
	`CriticalRate` smallint NOT NULL, 
	`RangeDef` smallint NOT NULL, 
	`DistanceDef` smallint NOT NULL, 
	`MagicDef` smallint NOT NULL, 
	`Dodge` longtext NOT NULL, 
	`Hp` smallint NOT NULL, 
	`Mp` smallint NOT NULL, 
	`MaxCellon` smallint NOT NULL, 
	`MaxCellonLvl` smallint NOT NULL, 
	`FireRez` smallint NOT NULL, 
	`EauRez` smallint NOT NULL, 
	`LightRez` smallint NOT NULL, 
	`DarkRez` smallint NOT NULL, 
	`DarkElementAdd` smallint NOT NULL, 
	`LightElementAdd` smallint NOT NULL, 
	`FireElementAdd` smallint NOT NULL, 
	`WaterElementAdd` smallint NOT NULL, 
	`PvpStrength` smallint NOT NULL, 
	`Speed` smallint NOT NULL, 
	`Element` smallint NOT NULL, 
	`ElementRate` smallint NOT NULL, 
	`PvpDef` smallint NOT NULL, 
	`DimOposantRez` smallint NOT NULL);

ALTER TABLE `itemlist` ADD PRIMARY KEY (ItemId);







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



-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
