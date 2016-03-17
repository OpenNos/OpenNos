-- This File contains all of the additions to the OpenNos.edmx
USE opennos;

-- Unique constraints
ALTER TABLE account
ADD CONSTRAINT ucName UNIQUE (Name);

-- Account
INSERT INTO `opennos`.`account` (`AccountId`,`Name`,`Password`,`Authority`,`LastSession`,`LastCompliment`)
VALUES (1, 'test', '9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08',2,0,'0000-00-00 00:00:00');

-- Character
INSERT INTO `character` (`CharacterId`,`AccountId`,`MapId`,`Name`,`Slot`,`Gender`,`Class`,`HairStyle`,`HairColor`,`MapX`,`MapY`,`Hp`,`Mp`,`ArenaWinner`,`Reput`,`Dignite`,`Gold`,`Backpack`,`Level`,`LevelXp`,`JobLevel`,`JobLevelXp`,`Act4Dead`,`Act4Kill`,`Faction`,`SpPoint`,`SpAdditionPoint`,`State`,`Compliment`,`ExchangeBlocked`,`FriendRequestBlocked`,`WhisperBlocked`,`GroupRequestBlocked`,`MouseAimLock`,`HeroChatBlocked`,`EmoticonsBlocked`,`QuickGetUp`,`HpBlocked`,`BuffBlocked`,`MinilandInviteBlocked`,`FamilyRequestBlocked`,`Act4Points`,`TalentWin`,`TalentLose`,`TalentSurrender`,`MasterPoints`,`MasterTicket`)
VALUES (1,1,1,'test',1,1,0,0,4,78,115,221,69,0,0,0,0,0,1,0,1,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);