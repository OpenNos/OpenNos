CREATE TABLE `Accounts` (
`AccountName` int(255) NOT NULL,
`Pass` varchar(255) NOT NULL,
`LastIp` varchar(255) NOT NULL,
`Authority` int(255) NOT NULL DEFAULT 1,
`LastSession` int(255) NOT NULL DEFAULT 0,
`LastConnect` varchar(255) NOT NULL,
PRIMARY KEY (`AccountName`) 
);

CREATE TABLE `Characters` (
`Id` int NOT NULL AUTO_INCREMENT,
`Name` varchar(255) NOT NULL,
`Account` varchar(255) NOT NULL,
`Slot` int(255) NOT NULL,
`Gender` int(255) NOT NULL,
`Class` int(255) NOT NULL,
`HairStyle` int(255) NOT NULL,
`HairColor` int(255) NOT NULL,
`Map` int(255) NOT NULL,
`X` int(255) NOT NULL,
`Y` int(255) NOT NULL,
`Hp` int(255) NOT NULL,
`Mp` int(255) NOT NULL,
`ArenaWinner` int(255) NOT NULL,
`Reput` int(255) NOT NULL,
`Dignite` int(255) NOT NULL,
`Gold` int(255) NOT NULL,
`Backpack` int(255) NOT NULL,
`Level` int(255) NOT NULL,
`LevelXp` int(255) NOT NULL,
`JobLevel` int(255) NOT NULL,
`JobLevelXp` int(255) NOT NULL,
`Dead` int(255) NOT NULL,
`Kill` int(255) NOT NULL,
`Contribution` int(255) NOT NULL,
`faction` int(255) NOT NULL,
PRIMARY KEY (`Id`) 
);

CREATE TABLE `Family` (
`FamilyName` varchar(255) NOT NULL,
`FamilyLevel` int(255) NULL,
`FamilyInventory` int(255) NULL,
`FamilySizeMax` int(255) NULL,
PRIMARY KEY (`FamilyName`) 
);

CREATE TABLE `Items` (
`id` int(255) NOT NULL AUTO_INCREMENT,
`VNUM` int(255) NOT NULL,
`Quantity` int(255) NOT NULL,
`Rare` int(255) NOT NULL,
`Upgraded` int(255) NOT NULL,
`Color` int(255) NOT NULL,
`DamageMin` int(255) NOT NULL,
`DamageMax` int(255) NOT NULL,
`ProtectHit` int(255) NOT NULL,
`ProtectDist` int(255) NOT NULL,
`ProtectMagic` int(255) NOT NULL,
`DodgeHit` int(255) NOT NULL,
`DodgeDist` int(255) NOT NULL,
`Rune` int(255) NULL,
`FireRez` int(255) NOT NULL,
`LightRez` int(255) NOT NULL,
`WaterRez` int(255) NOT NULL,
`DarkRez` int(255) NOT NULL,
`SlHit` int(255) NULL,
`SlDef` int(255) NULL,
`SlMp` int(255) NULL,
`SlEle` int(255) NULL,
PRIMARY KEY (`id`) 
);

CREATE TABLE `Monsters` (
`Id` int(255) NOT NULL,
`VNUM` int(255) NOT NULL,
`Map` int(255) NOT NULL,
`X` int(255) NOT NULL,
`Y` int(255) NOT NULL,
PRIMARY KEY (`Id`) 
);

CREATE TABLE `NPCs` (
`Id` int(255) NOT NULL AUTO_INCREMENT,
`VNUM` int(255) NOT NULL,
`Map` int(255) NOT NULL,
`X` int(255) NOT NULL,
`Y` int(255) NOT NULL,
PRIMARY KEY (`Id`) 
);

CREATE TABLE `Portals` (
`id` int(255) NOT NULL,
`SrcMap` int(255) NOT NULL,
`SrcX` int(255) NOT NULL,
`SrcY` int(255) NOT NULL,
`DestMap` int(255) NOT NULL,
`DestX` int(255) NOT NULL,
`DestY` int(255) NOT NULL,
PRIMARY KEY (`id`) 
);

CREATE TABLE `Shop` (
`id` int(255) NOT NULL,
`Npc` int(255) NOT NULL,
`Slot` int(255) NOT NULL,
`Item` int(255) NOT NULL,
PRIMARY KEY (`id`) 
);

CREATE TABLE `CharaFamily` (
`CharacterId` int(255) NOT NULL,
`FamilyName` varchar(255) NOT NULL,
`Xp` varchar(255) NOT NULL,
`Authority` varchar(255) NOT NULL,
`Message` varchar(255) NULL,
PRIMARY KEY (`CharacterId`) 
);

CREATE TABLE `Inventory` (
`Id` int(255) NOT NULL AUTO_INCREMENT,
`Character` varchar(255) NOT NULL,
`Slot` varchar(255) NOT NULL,
`Pos` varchar(255) NOT NULL,
`Item` varchar(255) NOT NULL,
PRIMARY KEY (`Id`) 
);

CREATE TABLE `FamilyHistory` (
`Id` int(255) NOT NULL,
`Family` varchar(255) NOT NULL,
`Message` varchar(255) NOT NULL,
PRIMARY KEY (`Id`) 
);

CREATE TABLE `MiniLand` (
`Id` int(255) NOT NULL,
`Owner` int(255) NOT NULL,
`Item` varchar(255) NULL,
`X` varchar(255) NULL,
`Y` varchar(255) NULL,
PRIMARY KEY (`Id`) 
);

CREATE TABLE `WareHouse` (
`id` int(255) NOT NULL,
`Owner` int(255) NOT NULL,
`Item` int(255) NOT NULL,
`Pos` int(255) NOT NULL,
PRIMARY KEY (`id`) 
);

CREATE TABLE `Pet` (
`Id` int(255) NOT NULL AUTO_INCREMENT,
`CharacterId` int(255) NOT NULL,
`IsHelper` int(255) NOT NULL,
`Owner` varchar(255) NOT NULL,
`Level` int(255) NOT NULL,
`Def` int(255) NOT NULL,
`Hit` int(255) NOT NULL,
`isWareHouseAccess` int(255) NULL,
`isTeamed` int(255) NULL,
`Hp` int(255) NULL,
`Mp` int(255) NULL,
PRIMARY KEY (`Id`) 
);

CREATE TABLE `Friend` (
`Id` int(255) NOT NULL,
`CharacterId` int(255) NOT NULL,
`Friend` int(255) NOT NULL,
PRIMARY KEY (`Id`) 
);

CREATE TABLE `Partner` (
`id` int(255) NOT NULL AUTO_INCREMENT,
`owner` int(255) NOT NULL,
`Level` int(255) NULL,
`isBackpacked` int(255) NULL,
`SP` int(255) NULL,
`Weapon` int(255) NULL,
`Armor` int(255) NULL,
`Gloves` int(255) NULL,
`Boots` int(255) NULL,
`IsTeamed` int(255) NULL,
`Hp` int(255) NULL,
`Mp` int(255) NULL,
PRIMARY KEY (`id`) 
);

CREATE TABLE `Runes` (
`id` int NOT NULL AUTO_INCREMENT,
PRIMARY KEY (`id`) 
);

CREATE TABLE `ListSkill` (
`Id` int(255) NOT NULL AUTO_INCREMENT,
`Name` varchar(255) NULL,
PRIMARY KEY (`Id`) 
);

