










































-- -----------------------------------------------------------
-- Entity Designer DDL Script for MySQL Server 4.1 and higher
-- -----------------------------------------------------------
-- Date Created: 10/17/2015 10:07:19

-- Generated from EDMX file: C:\Users\Alex\Documents\GitHub\OpenNos\OpenNos.DAL.EF.MySQL\DB\OpenNos.edmx
-- Target version: 3.0.0.0

-- --------------------------------------------------



-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------


--    ALTER TABLE `Character` DROP CONSTRAINT `FK_AccountCharacter`;


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------
SET foreign_key_checks = 0;

    DROP TABLE IF EXISTS `Account`;

    DROP TABLE IF EXISTS `Character`;

    DROP TABLE IF EXISTS `charafamily`;

    DROP TABLE IF EXISTS `family`;

    DROP TABLE IF EXISTS `familyhistory`;

    DROP TABLE IF EXISTS `friend`;

    DROP TABLE IF EXISTS `inventory`;

    DROP TABLE IF EXISTS `items`;

    DROP TABLE IF EXISTS `listskill`;

    DROP TABLE IF EXISTS `miniland`;

    DROP TABLE IF EXISTS `monsters`;

    DROP TABLE IF EXISTS `npcs`;

    DROP TABLE IF EXISTS `partner`;

    DROP TABLE IF EXISTS `pet`;

    DROP TABLE IF EXISTS `portals`;

    DROP TABLE IF EXISTS `runes`;

    DROP TABLE IF EXISTS `shop`;

    DROP TABLE IF EXISTS `warehouse`;

SET foreign_key_checks = 1;

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------


CREATE TABLE `Account`(
	`AccountId` bigint NOT NULL, 
	`Name` varchar (255) NOT NULL, 
	`Password` varchar (255) NOT NULL, 
	`LastIp` varchar (255) NOT NULL, 
	`Authority` smallint NOT NULL, 
	`LastSession` int NOT NULL, 
	`LastConnect` datetime NOT NULL, 
	`LoggedIn` bool NOT NULL);

ALTER TABLE `Account` ADD PRIMARY KEY (AccountId);





CREATE TABLE `Character`(
	`CharacterId` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`AccountId` bigint NOT NULL, 
	`Name` varchar (255) NOT NULL, 
	`Slot` int NOT NULL, 
	`Gender` int NOT NULL, 
	`Class` int NOT NULL, 
	`HairStyle` int NOT NULL, 
	`HairColor` int NOT NULL, 
	`Map` int NOT NULL, 
	`X` int NOT NULL, 
	`Y` int NOT NULL, 
	`Hp` int NOT NULL, 
	`Mp` int NOT NULL, 
	`ArenaWinner` int NOT NULL, 
	`Reput` int NOT NULL, 
	`Dignite` int NOT NULL, 
	`Gold` int NOT NULL, 
	`Backpack` int NOT NULL, 
	`Level` int NOT NULL, 
	`LevelXp` int NOT NULL, 
	`JobLevel` int NOT NULL, 
	`JobLevelXp` int NOT NULL, 
	`Dead` int NOT NULL, 
	`Kill` int NOT NULL, 
	`Contribution` int NOT NULL, 
	`faction` int NOT NULL);

ALTER TABLE `Character` ADD PRIMARY KEY (CharacterId);





CREATE TABLE `charafamily`(
	`CharacterId` int NOT NULL, 
	`FamilyName` varchar (255) NOT NULL, 
	`Xp` varchar (255) NOT NULL, 
	`Authority` varchar (255) NOT NULL, 
	`Message` varchar (255));

ALTER TABLE `charafamily` ADD PRIMARY KEY (CharacterId);





CREATE TABLE `family`(
	`FamilyName` varchar (255) NOT NULL, 
	`FamilyLevel` int, 
	`FamilyInventory` int, 
	`FamilySizeMax` int);

ALTER TABLE `family` ADD PRIMARY KEY (FamilyName);





CREATE TABLE `familyhistory`(
	`Id` int NOT NULL, 
	`Family` varchar (255) NOT NULL, 
	`Message` varchar (255) NOT NULL);

ALTER TABLE `familyhistory` ADD PRIMARY KEY (Id);





CREATE TABLE `friend`(
	`Id` int NOT NULL, 
	`CharacterId` int NOT NULL, 
	`Friend1` int NOT NULL);

ALTER TABLE `friend` ADD PRIMARY KEY (Id);





CREATE TABLE `inventory`(
	`Id` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Character` varchar (255) NOT NULL, 
	`Slot` varchar (255) NOT NULL, 
	`Pos` varchar (255) NOT NULL, 
	`Item` varchar (255) NOT NULL);

ALTER TABLE `inventory` ADD PRIMARY KEY (Id);





CREATE TABLE `items`(
	`id` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`VNUM` int NOT NULL, 
	`Quantity` int NOT NULL, 
	`Rare` int NOT NULL, 
	`Upgraded` int NOT NULL, 
	`Color` int NOT NULL, 
	`DamageMin` int NOT NULL, 
	`DamageMax` int NOT NULL, 
	`ProtectHit` int NOT NULL, 
	`ProtectDist` int NOT NULL, 
	`ProtectMagic` int NOT NULL, 
	`DodgeHit` int NOT NULL, 
	`DodgeDist` int NOT NULL, 
	`Rune` int, 
	`FireRez` int NOT NULL, 
	`LightRez` int NOT NULL, 
	`WaterRez` int NOT NULL, 
	`DarkRez` int NOT NULL, 
	`SlHit` int, 
	`SlDef` int, 
	`SlMp` int, 
	`SlEle` int);

ALTER TABLE `items` ADD PRIMARY KEY (id);





CREATE TABLE `listskill`(
	`Id` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`Name` varchar (255));

ALTER TABLE `listskill` ADD PRIMARY KEY (Id);





CREATE TABLE `miniland`(
	`Id` int NOT NULL, 
	`Owner` int NOT NULL, 
	`Item` varchar (255), 
	`X` varchar (255), 
	`Y` varchar (255));

ALTER TABLE `miniland` ADD PRIMARY KEY (Id);





CREATE TABLE `monsters`(
	`Id` int NOT NULL, 
	`VNUM` int NOT NULL, 
	`Map` int NOT NULL, 
	`X` int NOT NULL, 
	`Y` int NOT NULL);

ALTER TABLE `monsters` ADD PRIMARY KEY (Id);





CREATE TABLE `npcs`(
	`Id` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`VNUM` int NOT NULL, 
	`Map` int NOT NULL, 
	`X` int NOT NULL, 
	`Y` int NOT NULL);

ALTER TABLE `npcs` ADD PRIMARY KEY (Id);





CREATE TABLE `partner`(
	`id` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`owner` int NOT NULL, 
	`Level` int, 
	`isBackpacked` int, 
	`SP` int, 
	`Weapon` int, 
	`Armor` int, 
	`Gloves` int, 
	`Boots` int, 
	`IsTeamed` int, 
	`Hp` int, 
	`Mp` int);

ALTER TABLE `partner` ADD PRIMARY KEY (id);





CREATE TABLE `pet`(
	`Id` int NOT NULL AUTO_INCREMENT UNIQUE, 
	`CharacterId` int NOT NULL, 
	`IsHelper` int NOT NULL, 
	`Owner` varchar (255) NOT NULL, 
	`Level` int NOT NULL, 
	`Def` int NOT NULL, 
	`Hit` int NOT NULL, 
	`isWareHouseAccess` int, 
	`isTeamed` int, 
	`Hp` int, 
	`Mp` int);

ALTER TABLE `pet` ADD PRIMARY KEY (Id);





CREATE TABLE `portals`(
	`id` int NOT NULL, 
	`SrcMap` int NOT NULL, 
	`SrcX` int NOT NULL, 
	`SrcY` int NOT NULL, 
	`DestMap` int NOT NULL, 
	`DestX` int NOT NULL, 
	`DestY` int NOT NULL);

ALTER TABLE `portals` ADD PRIMARY KEY (id);





CREATE TABLE `runes`(
	`id` int NOT NULL AUTO_INCREMENT UNIQUE);

ALTER TABLE `runes` ADD PRIMARY KEY (id);





CREATE TABLE `shop`(
	`id` int NOT NULL, 
	`Npc` int NOT NULL, 
	`Slot` int NOT NULL, 
	`Item` int NOT NULL);

ALTER TABLE `shop` ADD PRIMARY KEY (id);





CREATE TABLE `warehouse`(
	`id` int NOT NULL, 
	`Owner` int NOT NULL, 
	`Item` int NOT NULL, 
	`Pos` int NOT NULL);

ALTER TABLE `warehouse` ADD PRIMARY KEY (id);







-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------


-- Creating foreign key on `AccountId` in table 'Character'

ALTER TABLE `Character`
ADD CONSTRAINT `FK_AccountCharacter`
    FOREIGN KEY (`AccountId`)
    REFERENCES `Account`
        (`AccountId`)
    ON DELETE NO ACTION ON UPDATE NO ACTION;


-- Creating non-clustered index for FOREIGN KEY 'FK_AccountCharacter'

CREATE INDEX `IX_FK_AccountCharacter`
    ON `Character`
    (`AccountId`);



-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
