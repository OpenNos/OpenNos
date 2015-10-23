-- This File contains all of the additions to the OpenNos.edmx
USE opennos;

-- Unique constraints
ALTER TABLE account
ADD CONSTRAINT ucName UNIQUE (Name);

ALTER TABLE `character`
ADD CONSTRAINT ucAccountIdSlot UNIQUE(AccountId,Slot);

-- Test Data
INSERT INTO `opennos`.`account` (`AccountId`,`Name`,`Password`,`Authority`,`LastSession`,`LoggedIn`)
VALUES (1, 'test', '9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08',2,0,0);