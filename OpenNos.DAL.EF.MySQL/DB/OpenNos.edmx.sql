










































-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 12/14/2015 20:56:15

-- Generated from EDMX file: C:\Users\Alex\Documents\GitHub\OpenNos\OpenNos.DAL.EF.MySQL\DB\OpenNos.edmx
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


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
SET foreign_key_checks = 0;

    DROP TABLE IF EXISTS `account`;

    DROP TABLE IF EXISTS `character`;

    DROP TABLE IF EXISTS `portal`;

    DROP TABLE IF EXISTS `connectionlog`;

    DROP TABLE IF EXISTS `map`;

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
	`Map` smallint NOT NULL, 
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
	`Faction` int NOT NULL);

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
	`CharacterId` bigint, 
	`LogType` longtext NOT NULL, 
	`LogData` longtext NOT NULL, 
	`CharacterCharacterId` bigint);

ALTER TABLE `connectionlog` ADD PRIMARY KEY (LogId);





CREATE TABLE `map`(
	`MapId` smallint NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` longtext NOT NULL);

ALTER TABLE `map` ADD PRIMARY KEY (MapId);







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



-- Creating foreign key on `CharacterCharacterId` in table 'connectionlog'

ALTER TABLE `connectionlog`
ADD CONSTRAINT `FK_GeneralLogCharacter`
    FOREIGN KEY (`CharacterCharacterId`)
    REFERENCES `character`
        (`CharacterId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_GeneralLogCharacter'

CREATE INDEX `IX_FK_GeneralLogCharacter`
    ON `connectionlog`
    (`CharacterCharacterId`);



-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
