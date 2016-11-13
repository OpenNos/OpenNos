-- ========================================== --
-- Current Migration: 201611122120497_Ares1
-- ========================================== --

DECLARE @CurrentMigration [nvarchar](max)

IF object_id('[dbo].[__MigrationHistory]') IS NOT NULL
    SELECT @CurrentMigration =
        (SELECT TOP (1) 
        [Project1].[MigrationId] AS [MigrationId]
        FROM ( SELECT 
        [Extent1].[MigrationId] AS [MigrationId]
        FROM [dbo].[__MigrationHistory] AS [Extent1]
        WHERE [Extent1].[ContextKey] = N'OpenNos.DAL.EF.Migrations.Configuration'
        )  AS [Project1]
        ORDER BY [Project1].[MigrationId] DESC)

IF @CurrentMigration IS NULL
    SET @CurrentMigration = '0'

IF @CurrentMigration < '201610211827432_Ares'
BEGIN
    CREATE TABLE [dbo].[Account] (
        [AccountId] [bigint] NOT NULL IDENTITY,
        [Authority] [smallint] NOT NULL,
        [LastCompliment] [datetime] NOT NULL,
        [LastSession] [int] NOT NULL,
        [Name] [nvarchar](255),
        [Password] [varchar](255),
        CONSTRAINT [PK_dbo.Account] PRIMARY KEY ([AccountId])
    )
    CREATE TABLE [dbo].[Character] (
        [CharacterId] [bigint] NOT NULL IDENTITY,
        [AccountId] [bigint] NOT NULL,
        [Act4Dead] [int] NOT NULL,
        [Act4Kill] [int] NOT NULL,
        [Act4Points] [int] NOT NULL,
        [ArenaWinner] [int] NOT NULL,
        [Backpack] [int] NOT NULL,
        [Biography] [nvarchar](max),
        [BuffBlocked] [bit] NOT NULL,
        [Class] [tinyint] NOT NULL,
        [Compliment] [smallint] NOT NULL,
        [Dignity] [real] NOT NULL,
        [EmoticonsBlocked] [bit] NOT NULL,
        [ExchangeBlocked] [bit] NOT NULL,
        [Faction] [int] NOT NULL,
        [FamilyRequestBlocked] [bit] NOT NULL,
        [FriendRequestBlocked] [bit] NOT NULL,
        [Gender] [tinyint] NOT NULL,
        [Gold] [bigint] NOT NULL,
        [GroupRequestBlocked] [bit] NOT NULL,
        [HairColor] [tinyint] NOT NULL,
        [HairStyle] [tinyint] NOT NULL,
        [HeroChatBlocked] [bit] NOT NULL,
        [HeroLevel] [tinyint] NOT NULL,
        [HeroXp] [bigint] NOT NULL,
        [Hp] [int] NOT NULL,
        [HpBlocked] [bit] NOT NULL,
        [JobLevel] [tinyint] NOT NULL,
        [JobLevelXp] [bigint] NOT NULL,
        [Level] [tinyint] NOT NULL,
        [LevelXp] [bigint] NOT NULL,
        [MapId] [smallint] NOT NULL,
        [MapX] [smallint] NOT NULL,
        [MapY] [smallint] NOT NULL,
        [MasterPoints] [int] NOT NULL,
        [MasterTicket] [int] NOT NULL,
        [MinilandInviteBlocked] [bit] NOT NULL,
        [MouseAimLock] [bit] NOT NULL,
        [Mp] [int] NOT NULL,
        [Name] [varchar](255),
        [QuickGetUp] [bit] NOT NULL,
        [RagePoint] [bigint] NOT NULL,
        [Reput] [bigint] NOT NULL,
        [Slot] [tinyint] NOT NULL,
        [SpAdditionPoint] [int] NOT NULL,
        [SpPoint] [int] NOT NULL,
        [State] [tinyint] NOT NULL,
        [TalentLose] [int] NOT NULL,
        [TalentSurrender] [int] NOT NULL,
        [TalentWin] [int] NOT NULL,
        [WhisperBlocked] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.Character] PRIMARY KEY ([CharacterId])
    )
    CREATE INDEX [IX_AccountId] ON [dbo].[Character]([AccountId])
    CREATE INDEX [IX_MapId] ON [dbo].[Character]([MapId])
    CREATE TABLE [dbo].[CharacterSkill] (
        [Id] [uniqueidentifier] NOT NULL,
        [CharacterId] [bigint] NOT NULL,
        [SkillVNum] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.CharacterSkill] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_CharacterId] ON [dbo].[CharacterSkill]([CharacterId])
    CREATE INDEX [IX_SkillVNum] ON [dbo].[CharacterSkill]([SkillVNum])
    CREATE TABLE [dbo].[Skill] (
        [SkillVNum] [smallint] NOT NULL,
        [AttackAnimation] [smallint] NOT NULL,
        [BuffId] [smallint] NOT NULL,
        [CastAnimation] [smallint] NOT NULL,
        [CastEffect] [smallint] NOT NULL,
        [CastId] [smallint] NOT NULL,
        [CastTime] [smallint] NOT NULL,
        [Class] [tinyint] NOT NULL,
        [Cooldown] [smallint] NOT NULL,
        [CPCost] [tinyint] NOT NULL,
        [Damage] [smallint] NOT NULL,
        [Duration] [smallint] NOT NULL,
        [Effect] [smallint] NOT NULL,
        [Element] [tinyint] NOT NULL,
        [ElementalDamage] [smallint] NOT NULL,
        [HitType] [tinyint] NOT NULL,
        [ItemVNum] [smallint] NOT NULL,
        [Level] [tinyint] NOT NULL,
        [LevelMinimum] [tinyint] NOT NULL,
        [MinimumAdventurerLevel] [tinyint] NOT NULL,
        [MinimumArcherLevel] [tinyint] NOT NULL,
        [MinimumMagicianLevel] [tinyint] NOT NULL,
        [MinimumSwordmanLevel] [tinyint] NOT NULL,
        [MpCost] [smallint] NOT NULL,
        [Name] [nvarchar](255),
        [Price] [int] NOT NULL,
        [Range] [tinyint] NOT NULL,
        [SecondarySkillVNum] [smallint] NOT NULL,
        [SkillChance] [smallint] NOT NULL,
        [SkillType] [tinyint] NOT NULL,
        [TargetRange] [tinyint] NOT NULL,
        [TargetType] [tinyint] NOT NULL,
        [Type] [tinyint] NOT NULL,
        [UpgradeSkill] [smallint] NOT NULL,
        [UpgradeType] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.Skill] PRIMARY KEY ([SkillVNum])
    )
    CREATE TABLE [dbo].[Combo] (
        [ComboId] [int] NOT NULL IDENTITY,
        [Animation] [smallint] NOT NULL,
        [Effect] [smallint] NOT NULL,
        [Hit] [smallint] NOT NULL,
        [SkillVNum] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.Combo] PRIMARY KEY ([ComboId])
    )
    CREATE INDEX [IX_SkillVNum] ON [dbo].[Combo]([SkillVNum])
    CREATE TABLE [dbo].[NpcMonsterSkill] (
        [NpcMonsterSkillId] [bigint] NOT NULL IDENTITY,
        [NpcMonsterVNum] [smallint] NOT NULL,
        [Rate] [smallint] NOT NULL,
        [SkillVNum] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.NpcMonsterSkill] PRIMARY KEY ([NpcMonsterSkillId])
    )
    CREATE INDEX [IX_NpcMonsterVNum] ON [dbo].[NpcMonsterSkill]([NpcMonsterVNum])
    CREATE INDEX [IX_SkillVNum] ON [dbo].[NpcMonsterSkill]([SkillVNum])
    CREATE TABLE [dbo].[NpcMonster] (
        [NpcMonsterVNum] [smallint] NOT NULL,
        [AmountRequired] [tinyint] NOT NULL,
        [AttackClass] [tinyint] NOT NULL,
        [AttackUpgrade] [tinyint] NOT NULL,
        [BasicArea] [tinyint] NOT NULL,
        [BasicCooldown] [smallint] NOT NULL,
        [BasicRange] [tinyint] NOT NULL,
        [BasicSkill] [smallint] NOT NULL,
        [CloseDefence] [smallint] NOT NULL,
        [Concentrate] [smallint] NOT NULL,
        [CriticalLuckRate] [tinyint] NOT NULL,
        [CriticalRate] [smallint] NOT NULL,
        [DamageMaximum] [smallint] NOT NULL,
        [DamageMinimum] [smallint] NOT NULL,
        [DarkResistance] [smallint] NOT NULL,
        [DefenceDodge] [smallint] NOT NULL,
        [DefenceUpgrade] [tinyint] NOT NULL,
        [DistanceDefence] [smallint] NOT NULL,
        [DistanceDefenceDodge] [smallint] NOT NULL,
        [Element] [tinyint] NOT NULL,
        [ElementRate] [smallint] NOT NULL,
        [FireResistance] [smallint] NOT NULL,
        [HeroLevel] [tinyint] NOT NULL,
        [IsHostile] [bit] NOT NULL,
        [JobXP] [int] NOT NULL,
        [Level] [tinyint] NOT NULL,
        [LightResistance] [smallint] NOT NULL,
        [MagicDefence] [smallint] NOT NULL,
        [MaxHP] [int] NOT NULL,
        [MaxMP] [int] NOT NULL,
        [MonsterType] [tinyint] NOT NULL,
        [Name] [nvarchar](255),
        [NoAggresiveIcon] [bit] NOT NULL,
        [Race] [tinyint] NOT NULL,
        [RaceType] [tinyint] NOT NULL,
        [RespawnTime] [int] NOT NULL,
        [Speed] [tinyint] NOT NULL,
        [VNumRequired] [smallint] NOT NULL,
        [WaterResistance] [smallint] NOT NULL,
        [XP] [int] NOT NULL,
        CONSTRAINT [PK_dbo.NpcMonster] PRIMARY KEY ([NpcMonsterVNum])
    )
    CREATE TABLE [dbo].[Drop] (
        [DropId] [smallint] NOT NULL IDENTITY,
        [Amount] [int] NOT NULL,
        [DropChance] [int] NOT NULL,
        [ItemVNum] [smallint] NOT NULL,
        [MapTypeId] [smallint],
        [MonsterVNum] [smallint],
        CONSTRAINT [PK_dbo.Drop] PRIMARY KEY ([DropId])
    )
    CREATE INDEX [IX_ItemVNum] ON [dbo].[Drop]([ItemVNum])
    CREATE INDEX [IX_MapTypeId] ON [dbo].[Drop]([MapTypeId])
    CREATE INDEX [IX_MonsterVNum] ON [dbo].[Drop]([MonsterVNum])
    CREATE TABLE [dbo].[Item] (
        [VNum] [smallint] NOT NULL,
        [BasicUpgrade] [tinyint] NOT NULL,
        [CellonLvl] [tinyint] NOT NULL,
        [Class] [tinyint] NOT NULL,
        [CloseDefence] [smallint] NOT NULL,
        [Color] [tinyint] NOT NULL,
        [Concentrate] [smallint] NOT NULL,
        [CriticalLuckRate] [tinyint] NOT NULL,
        [CriticalRate] [smallint] NOT NULL,
        [DamageMaximum] [smallint] NOT NULL,
        [DamageMinimum] [smallint] NOT NULL,
        [DarkElement] [tinyint] NOT NULL,
        [DarkResistance] [smallint] NOT NULL,
        [DefenceDodge] [smallint] NOT NULL,
        [DistanceDefence] [smallint] NOT NULL,
        [DistanceDefenceDodge] [smallint] NOT NULL,
        [Effect] [smallint] NOT NULL,
        [EffectValue] [int] NOT NULL,
        [Element] [tinyint] NOT NULL,
        [ElementRate] [smallint] NOT NULL,
        [EquipmentSlot] [tinyint] NOT NULL,
        [FireElement] [tinyint] NOT NULL,
        [FireResistance] [smallint] NOT NULL,
        [HitRate] [smallint] NOT NULL,
        [Hp] [smallint] NOT NULL,
        [HpRegeneration] [smallint] NOT NULL,
        [IsBlocked] [bit] NOT NULL,
        [IsColored] [bit] NOT NULL,
        [IsConsumable] [bit] NOT NULL,
        [IsDroppable] [bit] NOT NULL,
        [IsHeroic] [bit] NOT NULL,
        [IsMinilandObject] [bit] NOT NULL,
        [IsSoldable] [bit] NOT NULL,
        [IsTradable] [bit] NOT NULL,
        [IsWarehouse] [bit] NOT NULL,
        [ItemSubType] [tinyint] NOT NULL,
        [ItemType] [tinyint] NOT NULL,
        [ItemValidTime] [bigint] NOT NULL,
        [LevelJobMinimum] [tinyint] NOT NULL,
        [LevelMinimum] [tinyint] NOT NULL,
        [LightElement] [tinyint] NOT NULL,
        [LightResistance] [smallint] NOT NULL,
        [MagicDefence] [smallint] NOT NULL,
        [MaxCellon] [tinyint] NOT NULL,
        [MaxCellonLvl] [tinyint] NOT NULL,
        [MaxElementRate] [smallint] NOT NULL,
        [MaximumAmmo] [tinyint] NOT NULL,
        [MoreHp] [smallint] NOT NULL,
        [MoreMp] [smallint] NOT NULL,
        [Morph] [smallint] NOT NULL,
        [Mp] [smallint] NOT NULL,
        [MpRegeneration] [smallint] NOT NULL,
        [Name] [nvarchar](255),
        [Price] [bigint] NOT NULL,
        [PvpDefence] [smallint] NOT NULL,
        [PvpStrength] [tinyint] NOT NULL,
        [ReduceOposantResistance] [smallint] NOT NULL,
        [ReputationMinimum] [tinyint] NOT NULL,
        [ReputPrice] [bigint] NOT NULL,
        [SecondaryElement] [tinyint] NOT NULL,
        [Sex] [tinyint] NOT NULL,
        [Speed] [tinyint] NOT NULL,
        [SpType] [tinyint] NOT NULL,
        [Type] [tinyint] NOT NULL,
        [WaitDelay] [smallint] NOT NULL,
        [WaterElement] [tinyint] NOT NULL,
        [WaterResistance] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.Item] PRIMARY KEY ([VNum])
    )
    CREATE TABLE [dbo].[ItemInstance] (
        [Id] [uniqueidentifier] NOT NULL,
        [Amount] [int] NOT NULL,
        [BoundCharacterId] [bigint],
        [CharacterId] [bigint] NOT NULL,
        [Design] [smallint] NOT NULL,
        [DurabilityPoint] [int] NOT NULL,
        [ItemDeleteTime] [datetime],
        [ItemVNum] [smallint] NOT NULL,
        [Rare] [smallint] NOT NULL,
        [Slot] [smallint] NOT NULL,
        [Type] [tinyint] NOT NULL,
        [Upgrade] [tinyint] NOT NULL,
        [HP] [smallint],
        [MP] [smallint],
        [Ammo] [tinyint],
        [Cellon] [tinyint],
        [CellonOptionId] [uniqueidentifier],
        [CloseDefence] [smallint],
        [Concentrate] [smallint],
        [CriticalDodge] [smallint],
        [CriticalLuckRate] [tinyint],
        [CriticalRate] [smallint],
        [DamageMaximum] [smallint],
        [DamageMinimum] [smallint],
        [DarkElement] [tinyint],
        [DarkResistance] [smallint],
        [DefenceDodge] [smallint],
        [DistanceDefence] [smallint],
        [DistanceDefenceDodge] [smallint],
        [ElementRate] [smallint],
        [FireElement] [tinyint],
        [FireResistance] [smallint],
        [HitRate] [smallint],
        [HP1] [smallint],
        [IsEmpty] [bit],
        [IsFixed] [bit],
        [LightElement] [tinyint],
        [LightResistance] [smallint],
        [MagicDefence] [smallint],
        [MaxElementRate] [smallint],
        [MP1] [smallint],
        [WaterElement] [tinyint],
        [WaterResistance] [smallint],
        [XP] [bigint],
        [SlDamage] [smallint],
        [SlDefence] [smallint],
        [SlElement] [smallint],
        [SlHP] [smallint],
        [SpDamage] [tinyint],
        [SpDark] [tinyint],
        [SpDefence] [tinyint],
        [SpElement] [tinyint],
        [SpFire] [tinyint],
        [SpHP] [tinyint],
        [SpLevel] [tinyint],
        [SpLight] [tinyint],
        [SpStoneUpgrade] [tinyint],
        [SpWater] [tinyint],
        [Discriminator] [nvarchar](128) NOT NULL,
        CONSTRAINT [PK_dbo.ItemInstance] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_BoundCharacterId] ON [dbo].[ItemInstance]([BoundCharacterId])
    CREATE INDEX [IX_SlotAndType] ON [dbo].[ItemInstance]([CharacterId], [Slot], [Type])
    CREATE INDEX [IX_ItemVNum] ON [dbo].[ItemInstance]([ItemVNum])
    CREATE TABLE [dbo].[CellonOption] (
        [Id] [uniqueidentifier] NOT NULL,
        [Level] [tinyint] NOT NULL,
        [Type] [tinyint] NOT NULL,
        [Value] [int] NOT NULL,
        [WearableInstanceId] [uniqueidentifier] NOT NULL,
        CONSTRAINT [PK_dbo.CellonOption] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_WearableInstanceId] ON [dbo].[CellonOption]([WearableInstanceId])
    CREATE TABLE [dbo].[Mail] (
        [MailId] [bigint] NOT NULL IDENTITY,
        [AttachmentAmount] [tinyint] NOT NULL,
        [AttachmentRarity] [tinyint] NOT NULL,
        [AttachmentUpgrade] [tinyint] NOT NULL,
        [AttachmentVNum] [smallint],
        [Date] [datetime] NOT NULL,
        [EqPacket] [nvarchar](255),
        [IsOpened] [bit] NOT NULL,
        [IsSenderCopy] [bit] NOT NULL,
        [Message] [nvarchar](255),
        [ReceiverId] [bigint] NOT NULL,
        [SenderClass] [tinyint] NOT NULL,
        [SenderGender] [tinyint] NOT NULL,
        [SenderHairColor] [tinyint] NOT NULL,
        [SenderHairStyle] [tinyint] NOT NULL,
        [SenderId] [bigint] NOT NULL,
        [SenderMorphId] [smallint] NOT NULL,
        [Title] [nvarchar](255),
        CONSTRAINT [PK_dbo.Mail] PRIMARY KEY ([MailId])
    )
    CREATE INDEX [IX_AttachmentVNum] ON [dbo].[Mail]([AttachmentVNum])
    CREATE INDEX [IX_ReceiverId] ON [dbo].[Mail]([ReceiverId])
    CREATE INDEX [IX_SenderId] ON [dbo].[Mail]([SenderId])
    CREATE TABLE [dbo].[Recipe] (
        [RecipeId] [smallint] NOT NULL IDENTITY,
        [Amount] [tinyint] NOT NULL,
        [ItemVNum] [smallint] NOT NULL,
        [MapNpcId] [int] NOT NULL,
        CONSTRAINT [PK_dbo.Recipe] PRIMARY KEY ([RecipeId])
    )
    CREATE INDEX [IX_ItemVNum] ON [dbo].[Recipe]([ItemVNum])
    CREATE INDEX [IX_MapNpcId] ON [dbo].[Recipe]([MapNpcId])
    CREATE TABLE [dbo].[MapNpc] (
        [MapNpcId] [int] NOT NULL,
        [Dialog] [smallint] NOT NULL,
        [Effect] [smallint] NOT NULL,
        [EffectDelay] [smallint] NOT NULL,
        [IsDisabled] [bit] NOT NULL,
        [IsMoving] [bit] NOT NULL,
        [IsSitting] [bit] NOT NULL,
        [MapId] [smallint] NOT NULL,
        [MapX] [smallint] NOT NULL,
        [MapY] [smallint] NOT NULL,
        [NpcVNum] [smallint] NOT NULL,
        [Position] [tinyint] NOT NULL,
        CONSTRAINT [PK_dbo.MapNpc] PRIMARY KEY ([MapNpcId])
    )
    CREATE INDEX [IX_MapId] ON [dbo].[MapNpc]([MapId])
    CREATE INDEX [IX_NpcVNum] ON [dbo].[MapNpc]([NpcVNum])
    CREATE TABLE [dbo].[Map] (
        [MapId] [smallint] NOT NULL,
        [Data] [varbinary](max),
        [Music] [int] NOT NULL,
        [Name] [nvarchar](255),
        [ShopAllowed] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.Map] PRIMARY KEY ([MapId])
    )
    CREATE TABLE [dbo].[MapMonster] (
        [MapMonsterId] [int] NOT NULL,
        [IsDisabled] [bit] NOT NULL,
        [IsMoving] [bit] NOT NULL,
        [MapId] [smallint] NOT NULL,
        [MapX] [smallint] NOT NULL,
        [MapY] [smallint] NOT NULL,
        [MonsterVNum] [smallint] NOT NULL,
        [Position] [tinyint] NOT NULL,
        CONSTRAINT [PK_dbo.MapMonster] PRIMARY KEY ([MapMonsterId])
    )
    CREATE INDEX [IX_MapId] ON [dbo].[MapMonster]([MapId])
    CREATE INDEX [IX_MonsterVNum] ON [dbo].[MapMonster]([MonsterVNum])
    CREATE TABLE [dbo].[MapTypeMap] (
        [MapId] [smallint] NOT NULL,
        [MapTypeId] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.MapTypeMap] PRIMARY KEY ([MapId], [MapTypeId])
    )
    CREATE INDEX [IX_MapId] ON [dbo].[MapTypeMap]([MapId])
    CREATE INDEX [IX_MapTypeId] ON [dbo].[MapTypeMap]([MapTypeId])
    CREATE TABLE [dbo].[MapType] (
        [MapTypeId] [smallint] NOT NULL IDENTITY,
        [MapTypeName] [nvarchar](max),
        [PotionDelay] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.MapType] PRIMARY KEY ([MapTypeId])
    )
    CREATE TABLE [dbo].[Portal] (
        [PortalId] [int] NOT NULL IDENTITY,
        [DestinationMapId] [smallint] NOT NULL,
        [DestinationX] [smallint] NOT NULL,
        [DestinationY] [smallint] NOT NULL,
        [IsDisabled] [bit] NOT NULL,
        [SourceMapId] [smallint] NOT NULL,
        [SourceX] [smallint] NOT NULL,
        [SourceY] [smallint] NOT NULL,
        [Type] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.Portal] PRIMARY KEY ([PortalId])
    )
    CREATE INDEX [IX_DestinationMapId] ON [dbo].[Portal]([DestinationMapId])
    CREATE INDEX [IX_SourceMapId] ON [dbo].[Portal]([SourceMapId])
    CREATE TABLE [dbo].[Teleporter] (
        [TeleporterId] [smallint] NOT NULL IDENTITY,
        [Index] [smallint] NOT NULL,
        [MapId] [smallint] NOT NULL,
        [MapNpcId] [int] NOT NULL,
        [MapX] [smallint] NOT NULL,
        [MapY] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.Teleporter] PRIMARY KEY ([TeleporterId])
    )
    CREATE INDEX [IX_MapId] ON [dbo].[Teleporter]([MapId])
    CREATE INDEX [IX_MapNpcId] ON [dbo].[Teleporter]([MapNpcId])
    CREATE TABLE [dbo].[Shop] (
        [ShopId] [int] NOT NULL IDENTITY,
        [MapNpcId] [int] NOT NULL,
        [MenuType] [tinyint] NOT NULL,
        [Name] [nvarchar](255),
        [ShopType] [tinyint] NOT NULL,
        CONSTRAINT [PK_dbo.Shop] PRIMARY KEY ([ShopId])
    )
    CREATE INDEX [IX_MapNpcId] ON [dbo].[Shop]([MapNpcId])
    CREATE TABLE [dbo].[ShopItem] (
        [ShopItemId] [int] NOT NULL IDENTITY,
        [Color] [tinyint] NOT NULL,
        [ItemVNum] [smallint] NOT NULL,
        [Rare] [smallint] NOT NULL,
        [ShopId] [int] NOT NULL,
        [Slot] [tinyint] NOT NULL,
        [Type] [tinyint] NOT NULL,
        [Upgrade] [tinyint] NOT NULL,
        CONSTRAINT [PK_dbo.ShopItem] PRIMARY KEY ([ShopItemId])
    )
    CREATE INDEX [IX_ItemVNum] ON [dbo].[ShopItem]([ItemVNum])
    CREATE INDEX [IX_ShopId] ON [dbo].[ShopItem]([ShopId])
    CREATE TABLE [dbo].[ShopSkill] (
        [ShopSkillId] [int] NOT NULL IDENTITY,
        [ShopId] [int] NOT NULL,
        [SkillVNum] [smallint] NOT NULL,
        [Slot] [tinyint] NOT NULL,
        [Type] [tinyint] NOT NULL,
        CONSTRAINT [PK_dbo.ShopSkill] PRIMARY KEY ([ShopSkillId])
    )
    CREATE INDEX [IX_ShopId] ON [dbo].[ShopSkill]([ShopId])
    CREATE INDEX [IX_SkillVNum] ON [dbo].[ShopSkill]([SkillVNum])
    CREATE TABLE [dbo].[RecipeItem] (
        [RecipeItemId] [smallint] NOT NULL IDENTITY,
        [Amount] [tinyint] NOT NULL,
        [ItemVNum] [smallint] NOT NULL,
        [RecipeId] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.RecipeItem] PRIMARY KEY ([RecipeItemId])
    )
    CREATE INDEX [IX_ItemVNum] ON [dbo].[RecipeItem]([ItemVNum])
    CREATE INDEX [IX_RecipeId] ON [dbo].[RecipeItem]([RecipeId])
    CREATE TABLE [dbo].[GeneralLog] (
        [LogId] [bigint] NOT NULL IDENTITY,
        [AccountId] [bigint] NOT NULL,
        [CharacterId] [bigint],
        [IpAddress] [nvarchar](255),
        [LogData] [nvarchar](255),
        [LogType] [nvarchar](max),
        [Timestamp] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.GeneralLog] PRIMARY KEY ([LogId])
    )
    CREATE INDEX [IX_AccountId] ON [dbo].[GeneralLog]([AccountId])
    CREATE INDEX [IX_CharacterId] ON [dbo].[GeneralLog]([CharacterId])
    CREATE TABLE [dbo].[QuicklistEntry] (
        [Id] [uniqueidentifier] NOT NULL,
        [CharacterId] [bigint] NOT NULL,
        [Morph] [smallint] NOT NULL,
        [Pos] [smallint] NOT NULL,
        [Q1] [smallint] NOT NULL,
        [Q2] [smallint] NOT NULL,
        [Slot] [smallint] NOT NULL,
        [Type] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.QuicklistEntry] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_CharacterId] ON [dbo].[QuicklistEntry]([CharacterId])
    CREATE TABLE [dbo].[Respawn] (
        [RespawnId] [bigint] NOT NULL IDENTITY,
        [CharacterId] [bigint] NOT NULL,
        [MapId] [smallint] NOT NULL,
        [RespawnType] [tinyint] NOT NULL,
        [X] [smallint] NOT NULL,
        [Y] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.Respawn] PRIMARY KEY ([RespawnId])
    )
    CREATE INDEX [IX_CharacterId] ON [dbo].[Respawn]([CharacterId])
    CREATE TABLE [dbo].[PenaltyLog] (
        [PenaltyLogId] [int] NOT NULL IDENTITY,
        [AccountId] [bigint] NOT NULL,
        [AdminName] [nvarchar](max),
        [DateEnd] [datetime] NOT NULL,
        [DateStart] [datetime] NOT NULL,
        [Penalty] [tinyint] NOT NULL,
        [Reason] [nvarchar](255),
        CONSTRAINT [PK_dbo.PenaltyLog] PRIMARY KEY ([PenaltyLogId])
    )
    CREATE INDEX [IX_AccountId] ON [dbo].[PenaltyLog]([AccountId])
    ALTER TABLE [dbo].[Character] ADD CONSTRAINT [FK_dbo.Character_dbo.Map_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[Character] ADD CONSTRAINT [FK_dbo.Character_dbo.Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([AccountId])
    ALTER TABLE [dbo].[CharacterSkill] ADD CONSTRAINT [FK_dbo.CharacterSkill_dbo.Skill_SkillVNum] FOREIGN KEY ([SkillVNum]) REFERENCES [dbo].[Skill] ([SkillVNum])
    ALTER TABLE [dbo].[CharacterSkill] ADD CONSTRAINT [FK_dbo.CharacterSkill_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[Combo] ADD CONSTRAINT [FK_dbo.Combo_dbo.Skill_SkillVNum] FOREIGN KEY ([SkillVNum]) REFERENCES [dbo].[Skill] ([SkillVNum])
    ALTER TABLE [dbo].[NpcMonsterSkill] ADD CONSTRAINT [FK_dbo.NpcMonsterSkill_dbo.NpcMonster_NpcMonsterVNum] FOREIGN KEY ([NpcMonsterVNum]) REFERENCES [dbo].[NpcMonster] ([NpcMonsterVNum])
    ALTER TABLE [dbo].[NpcMonsterSkill] ADD CONSTRAINT [FK_dbo.NpcMonsterSkill_dbo.Skill_SkillVNum] FOREIGN KEY ([SkillVNum]) REFERENCES [dbo].[Skill] ([SkillVNum])
    ALTER TABLE [dbo].[Drop] ADD CONSTRAINT [FK_dbo.Drop_dbo.Item_ItemVNum] FOREIGN KEY ([ItemVNum]) REFERENCES [dbo].[Item] ([VNum])
    ALTER TABLE [dbo].[Drop] ADD CONSTRAINT [FK_dbo.Drop_dbo.MapType_MapTypeId] FOREIGN KEY ([MapTypeId]) REFERENCES [dbo].[MapType] ([MapTypeId])
    ALTER TABLE [dbo].[Drop] ADD CONSTRAINT [FK_dbo.Drop_dbo.NpcMonster_MonsterVNum] FOREIGN KEY ([MonsterVNum]) REFERENCES [dbo].[NpcMonster] ([NpcMonsterVNum])
    ALTER TABLE [dbo].[ItemInstance] ADD CONSTRAINT [FK_dbo.ItemInstance_dbo.Character_BoundCharacterId] FOREIGN KEY ([BoundCharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[ItemInstance] ADD CONSTRAINT [FK_dbo.ItemInstance_dbo.Item_ItemVNum] FOREIGN KEY ([ItemVNum]) REFERENCES [dbo].[Item] ([VNum])
    ALTER TABLE [dbo].[ItemInstance] ADD CONSTRAINT [FK_dbo.ItemInstance_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[CellonOption] ADD CONSTRAINT [FK_dbo.CellonOption_dbo.ItemInstance_WearableInstanceId] FOREIGN KEY ([WearableInstanceId]) REFERENCES [dbo].[ItemInstance] ([Id]) ON DELETE CASCADE
    ALTER TABLE [dbo].[Mail] ADD CONSTRAINT [FK_dbo.Mail_dbo.Item_AttachmentVNum] FOREIGN KEY ([AttachmentVNum]) REFERENCES [dbo].[Item] ([VNum])
    ALTER TABLE [dbo].[Mail] ADD CONSTRAINT [FK_dbo.Mail_dbo.Character_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[Mail] ADD CONSTRAINT [FK_dbo.Mail_dbo.Character_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[Recipe] ADD CONSTRAINT [FK_dbo.Recipe_dbo.MapNpc_MapNpcId] FOREIGN KEY ([MapNpcId]) REFERENCES [dbo].[MapNpc] ([MapNpcId])
    ALTER TABLE [dbo].[Recipe] ADD CONSTRAINT [FK_dbo.Recipe_dbo.Item_ItemVNum] FOREIGN KEY ([ItemVNum]) REFERENCES [dbo].[Item] ([VNum])
    ALTER TABLE [dbo].[MapNpc] ADD CONSTRAINT [FK_dbo.MapNpc_dbo.Map_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[MapNpc] ADD CONSTRAINT [FK_dbo.MapNpc_dbo.NpcMonster_NpcVNum] FOREIGN KEY ([NpcVNum]) REFERENCES [dbo].[NpcMonster] ([NpcMonsterVNum])
    ALTER TABLE [dbo].[MapMonster] ADD CONSTRAINT [FK_dbo.MapMonster_dbo.Map_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[MapMonster] ADD CONSTRAINT [FK_dbo.MapMonster_dbo.NpcMonster_MonsterVNum] FOREIGN KEY ([MonsterVNum]) REFERENCES [dbo].[NpcMonster] ([NpcMonsterVNum])
    ALTER TABLE [dbo].[MapTypeMap] ADD CONSTRAINT [FK_dbo.MapTypeMap_dbo.Map_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[MapTypeMap] ADD CONSTRAINT [FK_dbo.MapTypeMap_dbo.MapType_MapTypeId] FOREIGN KEY ([MapTypeId]) REFERENCES [dbo].[MapType] ([MapTypeId])
    ALTER TABLE [dbo].[Portal] ADD CONSTRAINT [FK_dbo.Portal_dbo.Map_DestinationMapId] FOREIGN KEY ([DestinationMapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[Portal] ADD CONSTRAINT [FK_dbo.Portal_dbo.Map_SourceMapId] FOREIGN KEY ([SourceMapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[Teleporter] ADD CONSTRAINT [FK_dbo.Teleporter_dbo.Map_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[Teleporter] ADD CONSTRAINT [FK_dbo.Teleporter_dbo.MapNpc_MapNpcId] FOREIGN KEY ([MapNpcId]) REFERENCES [dbo].[MapNpc] ([MapNpcId])
    ALTER TABLE [dbo].[Shop] ADD CONSTRAINT [FK_dbo.Shop_dbo.MapNpc_MapNpcId] FOREIGN KEY ([MapNpcId]) REFERENCES [dbo].[MapNpc] ([MapNpcId])
    ALTER TABLE [dbo].[ShopItem] ADD CONSTRAINT [FK_dbo.ShopItem_dbo.Shop_ShopId] FOREIGN KEY ([ShopId]) REFERENCES [dbo].[Shop] ([ShopId])
    ALTER TABLE [dbo].[ShopItem] ADD CONSTRAINT [FK_dbo.ShopItem_dbo.Item_ItemVNum] FOREIGN KEY ([ItemVNum]) REFERENCES [dbo].[Item] ([VNum])
    ALTER TABLE [dbo].[ShopSkill] ADD CONSTRAINT [FK_dbo.ShopSkill_dbo.Shop_ShopId] FOREIGN KEY ([ShopId]) REFERENCES [dbo].[Shop] ([ShopId])
    ALTER TABLE [dbo].[ShopSkill] ADD CONSTRAINT [FK_dbo.ShopSkill_dbo.Skill_SkillVNum] FOREIGN KEY ([SkillVNum]) REFERENCES [dbo].[Skill] ([SkillVNum])
    ALTER TABLE [dbo].[RecipeItem] ADD CONSTRAINT [FK_dbo.RecipeItem_dbo.Recipe_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [dbo].[Recipe] ([RecipeId])
    ALTER TABLE [dbo].[RecipeItem] ADD CONSTRAINT [FK_dbo.RecipeItem_dbo.Item_ItemVNum] FOREIGN KEY ([ItemVNum]) REFERENCES [dbo].[Item] ([VNum])
    ALTER TABLE [dbo].[GeneralLog] ADD CONSTRAINT [FK_dbo.GeneralLog_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[GeneralLog] ADD CONSTRAINT [FK_dbo.GeneralLog_dbo.Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([AccountId])
    ALTER TABLE [dbo].[QuicklistEntry] ADD CONSTRAINT [FK_dbo.QuicklistEntry_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[Respawn] ADD CONSTRAINT [FK_dbo.Respawn_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[PenaltyLog] ADD CONSTRAINT [FK_dbo.PenaltyLog_dbo.Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([AccountId])
    CREATE TABLE [dbo].[__MigrationHistory] (
        [MigrationId] [nvarchar](150) NOT NULL,
        [ContextKey] [nvarchar](300) NOT NULL,
        [Model] [varbinary](max) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])
    )
    INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES (N'201610211827432_Ares', N'OpenNos.DAL.EF.Migrations.Configuration',  0x1F8B0800000000000400ED7D5B73DC3A92E6FB46EC7F50E86966E2B464B9BB67BB1DF64CC8926CEB1CC9D651D96DF793836241556CB14836C992A598D85FB60FFB93F62F2CC02B2E09103792258FE244F8A870F9884B02C84C6426FEDFFFF9BFAFFFF36113EFDDA3BC88D2E4CDFED1C18BFD3D9484E9324A566FF6B7E5ED1FFEB2FF9FFFF13FFFC7EBB3E5E661EF6F6DB93F9272B86652BCD95F9765F6EAF0B008D7681314079B28CCD322BD2D0FC27473182CD3C3972F5EFCF5F0E8E81061887D8CB5B7F7FA7A9B94D106553FF0CF93340951566E83F8325DA2B868D271CEA242DDFB186C509105217AB3FF2943C9C7B438383DBE38387B7770FA767FEF388E02DC90058A6FF7F7822449CBA0C4CD7CF5A5408B324F93D522C30941FCF93143B8DC6D1017A869FEABBEB86E4F5EBC243D39EC2BB650E1B628D38D21E0D11F9BA139E4AB5B0DF07E377478F0CEF020978FA4D7D500BED93F0EC3148FFCFE1EFFAD5727714ECA09C3DBD4F8658F4DFFA523054C31E4BF5FF64EB671B9CDD19B046DCB3C887FD9BBDADEC451F81B7AFC9CDEA1E44DB28D63BA75B87D388F49C04957799AA1BC7CBC46B76C9BCF97FB7B876CF543BE7E575BAC5A77EE3C29FFFD4FFB7B1F7153829B1875A4400DC4A24C73F41E25280F4AB4BC0ACA12E57826CF97A81A4CA111FC27B7E53ACDAB82F52709511E74A9240DF8BE1AF32228CA937493C578C590C9AB814F71FB3EE3142BB8052A8A8AC4BA71F9E34B6320F26FD7CD32C73BC6FEDE65F070819255B97EB3FFF2CF7FDEDF7B173DA0659BD2A07E4922BCC1E04A65BE1DFCC85550143FD27CE9F421B83B1F83FB6855CD3AF7CD93759007219EF8FDBD6B1457258A7594B5935953D577AAD4BB3CDD5CA7714F727DE6F745BACD43324EA9ACC4E7205FA152BF793575C617E94AD93EBA98D8C03E57DA42AA886913AF5012C4E5E35013E9626213FB5C6913A92250135F1FF65BA07263A4E6527B6BECEACCB839766DB0D91E99CA936D90BA7BF2104CF9A753142CDD763082F25B8487DE19E52A8D92B270C4C931357F8D928490A10BD0DB20BCC3CCD29D234A94AEF2205B3F2AB65EFCA7873DFEEDF6F6F66D9C8677A89BCFB7295EE74162DCE893189F171DC863697E3C8A272D1EBEA37F37C6398D5609CD07E0A18BCD5B73B6494B3C9249E1697CCE1EC27590AC9027B87778FF70E623DE059B28C69BDA3FB7A8287D352C8F50B2F48B8937BD65BF34AD88EB7D1A3B6E7CEFF3749BF9EDD78720CA4FD23875EB1A4159948F31724341798ACF256F5DC37017E81EC5CE8DFA96B9CDDB87CC6D917CC83C0DC9AFE98DFB88B420AEA3E2DE122FCDB80C328621B1D8EE31C4376784BFBB22149819F3C18AD4489F234C70A52352944471902CCF93FBA8F475EC5CA6DB021D479B8BB46772ACB11C17A6B314ACF595DFB7782EDEA3F24BE6DADFEB60852A12715B32D728DB3A422CE2B4745AFB8BEC78B98C2AF193EB90C53C2E321F205848723B003F0731663D2FD202B9B5A4C6596CF39CE15A5CC0B068E206F3751D15F8B7ED2620D536746A4D776D4DAB45906B6B5A5584B132697157499A401B7B70BE70DF5459194135222DE859CD446987C0B185F285E1050B998E303E5A3075A6F9E3C0E052E5A071EDB21543DA97311DCDCB201A9AFCBA08D43492A36855956DD3A0238D161DC99B7434D0A6238B46656093703A4C654C86405E6CAE295D55676E1C15E559520E12175F181A33B68C62F0B882A6A3784DEECA7E24032DEE4A414D6D32156D6C4BF8D1BB36FB9DB9F2B5AA38A306D646F1DACB1AEFB7D1D24ADDFA314D863559FAFADD01AE868CF1DF3E6E3786E289E5058BD371C86F0183E7A66E9BE5C7778D276F2B942FB4132C04B5517B6D992EA9B957124565A60B4A9B40FD2CACE3B20CC2BBE324DA049CF6D3426827CA6F57E5C30916D73D3587409DDDDEA2D051FD4D707C74ABBEE27642F1702590C6CBF487EBC85E9DA4859BC07B1A6CB0F4EE782FB1CD3D90890F12398B117DCD6235220D4610FB189A0F51591B683834E8BC441B8BA3721CBD2851BE6DFAB658013518C74B22F9E07D3F776F5B0B99876B8F7097C12A0AA320F106B82076271B2F8019BDF4ED48621A6B9B3C0A1D154FD7E47AD14DAD87C2345906F9A32DDF09710798B34A42C7FDA10272DE216ACECF7D986A1CF7E6B8027CC95679B0440DBBE932BE0D12DD226F928667F69D978B953CBE7663D3CD4DAA6A639D2F348D24CB5A54E59936E463165EE2EF6B0C9B50926F1C5740D24CBE94698317EB341B6A2A55866F649725695E9FEFA6EDA8E74F5FC941CACF695D46BE6F6559D656541F241EADCAFC083F3EB85ACC447A38657C6A5B06B73ED5B622D153305B8ED56210760EED65C1D59C7181702DB1592A00C454A698FDA7DD79AC6BEADE7347C8BEEF1E48FB7DB6EA109397125686A2A86F5DA3F1A90BAF629D665AAC67ABA5BC13ABD84EFB68B68E3CA92037E45A9AD801463975956EC3BFD7DA4C772D598DD3F0F04E486F83220A8F7314B8A3F851DD5550EE125B05E3414E3A89D3029DA25BE42CD1568E7749993BEFDE2779544661105F6CC3BB6B571B9816CCFD50A9B58297C103AD06738262356AB650F9DD352AA2A274D7493464709A2E9D15C335928F057CDA74CD0B8D72601E7AEA51F1EC4EA2EFF006EE8B18FC983B9F171FD2A28C284B6E7B53E36F576EDA440F8AF068B52E7D0D70A563F642D5784FFAE0383618E2D215A2E65B688D5B6D6F43A7EFA2A2FA637ABC5AE57856EFD179D88BFDF646B1FD7C5A51190170D6A4362635DC95A7952D2B72640A0927CB73977684FE15EF90B9AFF567BC9D4845AC53FC73482EACCB80C220C91294857CBE85F59BA6C44A9704DBD71750B5922A65D1560CA4D1CEAA94AC8DF8D740FB488931D4D7CEE2BFA2D93ADA6C6DB9BA26426D899A149F5196269FB75183B5F55C65676D85F1A6B61877D965499B856B450B1C3F260378AD90EAC2206A3100C0E7B5ADAF71EBC1154632842DB44B1454527D8EA9B6ACE9B9CC7098E455C0056738DC674086C354AE6983F4358F43278C42C7281D2AED9DA59E39ED9D85149F7167B1D3CD4DA791AB943B3E84E71314C7697271EF26F678B07AF3A86572752E7ED653CDA5A7F2A12BD9517DD78E2BA97C585856187F0BE2AD238BB2531AB3332C966604CAD98194E8DE7C74CDAB0E2FF230444CB401ABFAD768551F83CE4615E7BE62979C17D541E2072829B61B52C51D8B3062991F28A2BF8D42779CD6E9FED3CD3FA83DC41E6F91C64B3F3DFC8CD9233F485F831CAD491C006728CCD72EB637CEDA3B82E30504EFD7D1925302DAC6D6F835BDF16104EECD9ABCD2C6FBD8727757AD5FF3EF6EF6E22D8AAB148081BC1DBB0D777ABCD9A46E6DC25BB8EBF944302EDD31B2B523846B133C1EB3935CBBF0FE0116DBD2D57DE665A5611CDCCDAA2F8E972ECB6D883E65691124DE36942A3A4A35A93E36CD0ACDC3D0778E153E36E0057A70ABEF7C47B5C89C8F5B6780AF41549EA2387874A397EA92CCC7ACB8DDB699DF960D2879F94B1251FD6BA2663E4FEA5E15F2A670C5B83631B970E3D822DEA27C54213A6A852BAD836E1201FD739B63AA7BBE466124D18557786D3E373275323C244D9E79140A524D7D3D4097015BC48C01D02A66F4745B46DC39D4EDEA4B70AD6A33E03675B94E377E34091AEAE7DB6ACF0131A0CFF8B8F47B8B2196F2C81A030A635F11394EF10EBF7264188923F84D1463DAF310F88C101F3E07511B93BEC6EA63D44F71037A1DE48E6C1BAD50B443706628AC6E90A43B1D4BADD2FDAEDD36BEF3E5D9DD4F520CDC0B65658D7D223543C56807F7920788110380F9B90337E149C05B71986DB13A5BBE14848E2C4E17B6A2AFF365EF6D5020CAEE9039FAD41AEA2B6EA10E48DAAAE2DAA3F715615AB11B3FBEEAFC23282A7274EEA5CD6B7CCAC8C0F047BCF5DDB3ED45B1DE552E7471E772A36C7D816C7F5F6C7F3D6C791BEC70F96B7DD7EB72B5EB7C936B7BCF697919E970F76873D53850C76CDF3D2FCE3659FF0A4277473354ABD2541AD692DF2DB85C2558DF1C3868E61DCE3213DD928B2A495D97B1D306E42239BB471D1930C74715F82E9EC714EFA72A287281CAD24E2C17DB25FDE00F54B567711E5CB7CEBE41CED29A07D31A9ED806074F7729898B63BCE5248489D55A7C56CB6991A1300A48705C0B2E5CAC3C161F2E8CE580E2018C5F37A4ADB039781631771668D6323CF81719DB239D1387D4C9EF0C6BB043A057C9E2345C6484D132ABD10F995E7960431BAC427817B32A78C74D602FDFA19A154720AFA2BD84EB5B12ED454B8ACF780292CFDB9C826D3DB59ED7A34B0B89BFB02674CDEAB9ED8339AC6BF6947ECED411CC877D7C8F36ECB522C8A23DAB6DFD7EEAD93FAF02FA5D9CD18C2DCE0B42F33ECC1D17D51B202769260A62A6162BA828A85D7DB4BE5FA31045F71E4297D73D777686A8713CBC005703F97972ADC7727F78ADC6F233DC9565956BF0E7CF51198F4067E63AFC81AB7B9EF5152FF50D2ECC2B9A777D94437EC3C13EDAA17D5DDE50BDCBE325EA36B909D8AD4983364351579891A5686C172C988ABEA67A6D8DE5293B634CECDA197C38AAA49F4B3BB59D0C784DC7D9D0E8DB0C493DE91BFF77B12D4C06E4BD4AE5DA59104947A741852D77844C617B144B386903DAC1331026488559C589968CCD050ABD05E047BB761A053179FACB65C9FAF361F36063795E9C46D525B60786FA32BDAFF81167C63C2A4B0F403FC9FBA598BADD8F89ABB488686B7A470B1AD58B646274933E157C8B8C8B78A2DB06FD78023AF1561431052CDBA7B039D53CC144261A3ADF4CEC3B55ADA9F385B69064594BAA3CD3767C4631CAD25C366FED7C50A58436F599B29651255C4F52B36374DE33D4F200F5C1396B9D9E41D9472F8D9280D8A551C22BFE13165E07B6D86DD1BB87DA5D37917F47D7DE90D5721CC7E90F9FAFBCAA2D01355F88041611F07EA4C1E1A0DA989B2D1F88D4C5E680AD1A2736D7E0B125698A4D18AE26888EE2046DB2BF574584D83C4D8E2C384F9B6D7A5A5DE1CD3290796964DFDB6C7674EA5470749A2CD3D1A9ABC12FB0F6B0477043C49757E93CFFE794FC901A38A17C1E4FE6E1C5FB4AF31E564D232CCF2CAAF61482DFEEC9493F8978E3EDB18569459CE1E34B22EAF0A7DB28E28E76184CB5D8A36AABC91ED51D78267B545369D7186A5949D2DCD1D8EFC145D87EDD08C686F6F59813E0CC837817439E49A34D753159BBC89F036DAB8AB89EC9752B4C897D5E4A77A05F3DE2F377F9D27C724068934A9366B11D52D2621B45ABD26D1C76D6D68CCB29A1602E6AE7280289C6FA520B26CC12B45A5FAD28A2BDBCEA0A33AEAEBA01368BABAFE9CAE3EAAEAD535494515207E9703F99283447469342726438FD71F235197B18A61AC871846A10C7C1A9D7F7E85C845A9F0071CE9CAAC1A00DD6CA047933606B11ED1D8C561B68EF627DA51977B2BE1136BB195B7B2A6EE13C59F64179AC196C0F3CBA8D9DC62E08EC560B5C4B3506AD304073E6CF6644F75649623B32D0367D6F8DB5D1EB05A4F88C8BBE8ADF62B1DCDB7A53B12E9E96184AB6F4296865DB35D97D92794B1D568FEA8658B26298DB63939B6AA9A5558D0844226232C427AB995CCF2F69B7D8E2BBA34C8EBC511E5E44E987C4685F99F9FD82B609D6FB4B5377AA3DC6DD3C7D67A2FA68EDCE0318AEA1C69D375ABF81815CC39F8106B0C296E46CB0A3B9050ACF1B831BA4F56663FA40795767E6EDC6FA5172A6F2541B8E97656AF988F8FCABDD616D0C9DC5D2D531CEE3E47093D82CC983E4EA46193A8418B2087DA5F91D432CD904B6F6545A879D7110D1F58DF1E9203214BA54E128E23BBCABA16306DF32B9EB86D51AACE929BE202E04DA6BB0AF34E31AC45FB7597C4DB58EF4C6F6F60E43B2EC5C1D366D23999E67C7CB658E7AAFDAD1647F3CAEB431ED98DFA14FEDD1AE5989CF7951069BEEE9007D4774E9AED01003B82D3479DFE905D9EF0B62AEB03100454C772DB5E92E050C5AF042F98282012CE4A466F87D1B857724540CCE21E6DBDABB185BF1397693D9BE636C54E7FC8AC7555AB801FC7EE458FFA53F7165FECB50DD88BDFCFA827CC7D9320A2F72AEA0A3F8503DC36E243B543566151CAA16D8490D5DD5A998177FABDFFDBAB2E9BEB37ECEF1B6D2DB55A5EEF2EBA81C5A774DA662C1B525DC0C9C5012C4E5A39990D0579AD3D0A96B8495B113537B2A059B2791E178B98992492C1209677C962CCD19650067510679E98CD44C5CD773B2A49AB47A0331DE7C82A2B7E39F20CA8D8EA440AF4A5152E873A5920255C46C7BD86EA8CDE1785BAED33C6A47F6BC781707ABA21B008DFD026747C90183436D1955AE75B8C72F24264EFC88E78BDEB6D959B8449B1B94B7F73705D992AB80A16FF65F0833C694BDC4338BD778DA57385257A8D66457F8A5BAF097E42E497FF4C5FF7024CE4F3D138AD9691C261CE78642196D66AAD3DC603806E6E60AEF2309D29E998F5727BAF3F21591E63765FF38D08AC6AAB0292DEA6B98D26F53A2A869CAFE595DF62C8E08F7D3141643EAB1CD4041962EBBD2FF4B5DBA89BEDA15FF8BBAF87BB4596441D8ECD44D9DBF5AD02AB3435BD32A85321BAD5E6E4BB4D4A5D4B7B8535469AD557E5C14299E233210F4754FAF53A96F76D8AFE2337A8FBEC6610BF7AA9AFECC6CEE872EF12045191E16BC3B432B6900B83B7F7A60AE9DDC17FE4DF802E6E8504ED8A720266F42E3A98A925264FFA2248CB220D6E8255717BE9BED6F29058FABC3EE637CCE29CA4820B6A4D41812E756741FE338DBA1F17A7D4851901661A59B9B74889E48199F6454E141D453356654A2A1BB3239ADD0FD7E0A24D2BF2E299B50E0A9C97E3AEB4B307DEA105FA7A4C0EA472E47210DA1133A73E34C13426F75BEDADFD4CE4610B2C7CA54B33AF872194B347D64F9A1F9D6FB8CEAA0E2BFF1E2E040A4516BB21A6891CE9433CA3A477A1B98079DE6880F30CE4289EA87136444A2F98A024526CCE321FAD4A8F9FA49FF21F1ED077EDFE447F953523FF8B8771C92AFE06F0645585928F2EA30DC2C0FE4ACD525AD3DCC8D8EB5E650A71DD0CB1FF31DB3EC2B85CA2352F28CB2E3C12B7908D17667B63F89C1EE4D772483E3F004CEE63E4EB66C9A81A0D9FDECD601A4F57738E0F16C3501FA3B58854F4F421CC2E8E97C957F2C6126CAA063B3C927140CD4465388C080AB361516CF8405F3B499801DD29934593416135A01FAEEF4E989C8848AE8A39A5720628F1BA140818218C02ECED068A422F6693A5A11BBFF448885F8F10DCC2B13D5D095489800BA0C58E55138267150FD989430A82E3F01A22093D2466D52CC25554C421C4D093396048E5B694874B644027C7B324201C6F389104B138268604EF9784402A968719B32D869D58D609734678B54F4432D6CD79D3F3FF9EE524DBADE4E507577A45DA6C286771A882C47D96DE8364C4E47C0183F11626A4303A9580E3EEEB013FFC2071DA26C981A7382D1F817B61FD3F12F6C9775BE2B86059B993E8E3408E4C82B851CCD48224773D1C891D1C5281D0F6D36FAA062FCA826158AD0ED442550F8A01E908E3C341AB1887D9A8E5EC4EE3F0156977A854531B5D0932C4352EE00B540EF94F590AD3FEB588422764873AEC037BA4C4945ECBBEBD7A7B0CB61626EC86656128388B2CD598B028CD234078EE8C102427A7D5FF6395087B4CE0230689811A9807D77FBF6948432601328890CE58154A46680540C8A7189C5DCF6CF23B5989BFCCD492E7468B581F3827D89CBC301C4066E53539EDFC387EECAD4470FDDEB2770F05C0A912B07667590B1B52396F9D95BB87353938F359F3B271189515064B3AD78AB76883F55119122D20A0F3B223723ED9CCE14CA9EA7362222E938B8B6602A83A80169097AF2D9D5046A263909E88ACE24F9317732978F76C00659677F91857AF24324B3ED2C926E4D4D30A63BCA2E10CDA0500D47D174259819856AB043D3918A8D483D33A150CF92291D5FB87210B1D0EFA8195D4EF3D8D35E4F4B7AA633777D5567FA918C811607AC6EC2B444A46150A77E094F8BA034A969662B3B6547E7A32F4BE3BB1DA33295259EFC8979CFD43587799EB473B35294A1D51E2EBE1B94D4FFA9D62ACBAB8C425BFC370022E35B3E3AB549FA3D1FD94906C94F8326F348D7A540B0B4472FF59D213965477526D7A70FBB038DCDEDCDCE8747574FBFFAC6CC8EA4E6BD3383BB353501D95D9CCD4C3ABD2F8F6620165905D0C758E607A52228E907E60ACA32D4639D69F6E5EA3E34381EDA3201D181719965F4A00ED2DCD3021D865ADFE65919DD79DAA80AAAA64C4963AA017F1AF4D537FB3CB9C79069FEA8B19F756547D8CA7AEC79DCCF15BD9C67FB1206E4A95156E5553E3CF1956FB57F7AAA60417D848EAFBB2309D17D9A877AE8EE6B7159046757A8466E76CF951B896E4023FCC908C7C80C7F1CCA3132C8BF46218AEE778278B8B8F6C3B32D895DEF8F9C6451F4FB0FF0B1F847A630B8C7F3901A3C384FED9C6BA3B90F13031FB8DD1F990941E369AB8426D8FCC884C5756E1E8AE2C6E16990521BBB7B58D2134A42A4D4C519D72724117786982DD2CEE9CC221560DF8190A4E3E0DC8409C98812F387E61B7857CA0F21416F5AD929229C4849ECDFF4B4240EC5532226EA4182A139079E1EF0434CD0B30794732CF564C2A8C424F66F7A62128762F788E9AC7A6102D729718D2E227A13B59D24A3871288FDFEA5404DF8F7A2795983A70B02BB4025DBA9FDBDB3EE450B9ED404CA6211A8034DC0A0F274511A0DBB1CAA2930802783D1ABDD84D1161B51A70FD4162E35051CA18436A2126C10A7367A1310EAE481BAB591A550B74ED6A8DB6B1E418C3E7B686E9880B2E21431D90358B53642C0A89307EAB606ED42ED3663F0DBB5490FF0F53A63B83E5C59A7A69C9AE8CC619C2EFA0B84D365EAE1C8410611DAD00E02409B31509F76891230E8CCA15D670DAEAF3A59A3AE648DF5591A18D29DAFCFD3A26C495BE8CC011C9A411570E8CC011C5EB72360F10506FBD7C8F240E79A9C217AA3B82591E6A84C0E873AEBB9A38ABF16DFA38AD24797F2FA9CE1FB942F99741DEA4E4A81A519406A79480A69E0883E647BAF3F32F5A12B1D10F1FD0DA8F5CC0B1C36DD679EDCA07B0DF204E69DED9F8A00BA2A79478269A7F89204D54A68C14A6A037D041904BB2ECA1E3F90745AEBAD04A12343AF25700323E33DB471558BC1C3B8A943F503236710DB9F55856A45F7A77BA9E0B60C9081F153F28496AB8B8D102F5B668A38F2E28A8123C91B2F3C3874FC78C3D1873A07464112079D69B518099D6A2CC43F4B6A4BBAEAA58B74CC6EB09BD2A0DE5C63A1B0DE4C7785CD510130F25E71C9C6A096F45BE65423B41BF0A731EC39E032C322C8C40DEBBE57D293B4DF829B07D462DAC3C3BCBFB413075B1B12ECACFA49470986FB2A8D23CCB7188A24CCB65A22C8A980E0EEFBEB7B13F456DE75282A2ED4602E2EAED8719D5E735170FD734E62A8D0E15917E3BB2A268C89F0EA3AFB4C48D7E111B55CE9AD6C0FAF742834A9B056B9E0A4862B9D8B464AD596A81D5CFA79A4EEA8F8BA11D4D623B7AE1E8DDE575AD7027757165C476832105BC7B0D340181D0A41A116B2EA3B15DF10EEBA2C0022DF6E2004E2E021A4C000BA2ED1705A08F84CA43E48C49787F263657430981F2DE6032A380502D069A912CEA1DB725D8F22321DDC6CB97E47BBE752C58E5C75684DE975ABA4742EC65883289489B26645E34C58B58141B3EFECE0AEA68A1A06357B706FD3ECFE843B9C18D60A188A81D8574C17E4D1AF0677AA011CE98EE769F9D3C199644A88C18D1E8ADE64AC7698608BE7830DA93B3C30E7B28844961D9F6EB655679C22B28ED870D519A7D7F589CE383E0C0CD07165A418A6E1B2583154D3E5B7E04AA4F144363884897A18B4D432EA68274E4332A1AE468CBD3138346ACD8D3C4487EB908CADCE51048F508F89D2D15FD62199BFBFD328C95CFD4150BFD7831A43A6111601B8F1D31828DDCBC31987472D66A81CFA817EA8050DCDC1984CD490FA9C4337665AFEE9EC95D69087BA8E865F0F718ACB76D0591A18A961A76AA64F4AB76AAA3F727B9061B8B1AF5F01375F250949BC8125732DFA033B108EE8003CDE2522E7A4AA1C12D18D55D201C691D5612018CFD581BB49E7BE83CA59A537A6A2D5A092D6A6FBA0BAD67BFF79E32BD540A83C0B251D91F8163A0C8DC49990421CB0277319ADCEC24C354CA02F9CA437BC379CC3C0F0EE6F8C440C1BC6990F85E8A6050CC5802F17D37EB93717D57E8931FB00D0C8E70AE06AA4180B994312D807C025C97234001F24AD53DB613C68034AF978C87C6AC05E005E3596E301B8D1D0976172DB4FF978BC3EAC113A678F2EEFF5E1225CA34DD024BC3EC445429495DB20BE4C97282EDA0C2C8B6651B22AFA9A4DCADE220B42B2CAFFB0D8DF7BD8C449F1667F5D96D9ABC3C3A2822E0E365198A7457A5B1E84E9E63058A6872F5FBCF8EBE1D1D1E1A6C6380C997B74DE35A5FB12663B8215E2728981E412BD8BF2A23C0DCAE02628F0D89F2C374231CEB5851DBC6E88DB8FB1DE2BE2B4B536B96D79F237E34273707A7C7170F6EEE0F4ED8104A51FC477B85F1BCC56555D4400E1083571DD4518C441DEBA11893E442769BCDD2483AE450AAC6DB94E73DC140EAB4FD6C7BA088AF224DD647144FAC902F27966A80B54149571200FD965E8E3917F59A03A451FE12A288A1F69CE0D7F9F2A22BD3EE48880A7B54381D8B875CFD3AE1665AB4E3E13DA96E26850B7A2AE6C7C1937707A8895FEE1723CAFEB252CFF748A0201AA4D3543FAAD92BA79A4DF4015881AE92A8D92B210B1DA7403B41C1F3F5FA32421EC09034767E8E3BD0DC23B7C7CDCB1607DAA015294AEF2205B737B15956C80B5BDBD7D1BA7E11DE2A692C9D0C73B89F1F2E7A8B54E32C090EC9D76FBE669B44A847DBD4BD4C739DBA46514A649010E97986B80FC10AE83648560603E531FF71DDE2384D3A24B34C1D944F1E335FAE7161525D848B884C117F208254BE517C012FA5F785F05936231DB34039434E6DA55A71820E4E9365375142CA08FFF2188720C97725DA592CDB016E5638C44AC26D9000BE5293EB6E03E0B9966B817E81EC52262936C86F52D1381BE01263F0A141EC1B0363C4299C5D8FC9ADE0043D3A79A23F1A343A71B70B262A38C5B0436C7A22D97F53BCC34CA25FC34B312E39B00F1CD10E1EF02C2DFCD10C86D17C4FDB039A6889F234C752584D8E6182046491407C9F23CB98F4AF8B4931431F846BA2DD071B4B948794E8BCD3140E488ECD288BEDCA5AC4A7FFA1E955FB876D0E9FA68D7C10A55B4C08251C9065828DBF23875923EC6224E39883AC500213B5E2E23C2D300FD12324D70413C739C322839126892F4313E0731E67A2FD28203A2D34DD116DB3C07982221D314174B46106295AC8FF5751D15F837B849F079BBA76790DD765B291B4030138D83044036F4FC79687618FA565A50B1F69925240FC13FDBF4FB9875DBC9369C639B51952A4ACA3208EF8E93681388E2A69069A69AE089A84D33A047CCAC48DAC66599619EDDDEA290575050E96668C26269D2CC503E473CB3D1A74EADC2C17271FA831FF02ED500E9EA242DF8716ED20C9440C10673389C0EA8493340D9E60019F5A9062A1F807ACC29E72C46A296AC4B34C609626898844C0361362A6B174546946D13F571FA4738997349FA34A71CC99BE84904950DDF2236C74C30C2558E97C4DC689BA31C68A6AC8CF957F270ADFC029D6F8C7E19ACA2300A12393E57C2F80B0B72B9B4517D812B6122EA897B4D9B36E9C55A1E851C44936422E825FC4A6E920C782F14A6C932C81F65EC02906FC8D961AE30E1FBCA6418E289FB0D956C22CA106B04600C990C533CB17174BA019A886388F0255BE5C11235E6AE34129B638C28368DC9D8195E5D16E8CA48428330740433B89EE23AEC261518C436D1804F9770E8361CB01FFE05B302026FF093CA86C3FE1226943780A641838308D2D38DADC8D325906D707272CF7EC2C0A6E7CCB5A07FBB3654BF3D493AF346624ED46543583E27FF78436C5BC87D6A94F37A443ECF54E50108CA4C86295E735242885D9689BD491185C7390A7883932ED9100B96E9B92C434C80C3A2D30DD10076864E375181A4053A45B748E04DD91C138508AE9094B9B0173119067879544638E3621BDE891B9C986B8E2C473545AC150797C18328317359C6989014CE659960E677D7A8885A9F1E1694CD3340AD89E5345D0A0A2826C71811DC2EF83C133BA9BA6F20D90B99D6B8D03880256653838984CF64185835E16345464F7CDE3C162FE7C587B42823DEC2874A36B211F97625988790A489D573D16A5DCA865DC834B187584521B838D81C13C4870F573C549564847129625C9A61D4AC9528BA3319532AC03EA6C7AB558EA7E91E9D87BCC02A649A8802FCDCD5296608E240F5A926D6139517987867C36498D82B209EC16D92F43108770DF3CA6C8EC13D3EDE3373D96A1432F571F9AD06DE676612C22401584CC42F084243F082AB498F66FC9317E0DB3453110B12AD8CD804FC135206D3E973DC183561360123C136D978A3151B3620DCCE44C692104A26640C416890315C4DB56B89BB95B1F408F2D36C8E8114558568BFB8E7B8192A79E20BF911245AC1CAFCC4D4C2FC592A7E1A5231286131193FA194FD74E5613FC625558DBF05F1969786E98CA72FA59F61A63623154553642ECB4CF207FBCB64CCAE49888051EC120D701C3D5EAED1AA8A7C20DE4DF279269A0DD074F8DCC63FEFBCA80E3611AB4B36C34A8AED86BCD822C2F53926888445CE20402AC3482784F2340A059550936A82D4BA737CBAF987B02389B926C88B345E425DEED34DD03E63F60E42EBD34DD0BE06395A136F131E8ECA30136516DB1B51E9C06498E1C160566679411C2D451D069765A801FC35BD91DBD7D199731AED55AA44709367737E52CD652DC9089AC736D9024B1096D81C23442993C0E719A156E6919B4D2A40F61926CA801CF127679B66862278A835694628D95A00214926C693BCE1A4596D3917C0E73D3553CCABFB0C5C7D74BA11DAA2CC51B22AD7025C9F61A2875E6E43F4294B8B2091EE3ED24226DFC9B6F5BBDAE0360C641B62035345A75B98BD82BBBB986B82FCC0833D4CADE35F64808DACE47126398ABB09EAD7202A4F511C709139A864C35B0670B6D89CE9EE2D6654D92A9EA694AB6E31AF8BFFFE74FB2FB00EB7C5FC574B65AEAC4DF20970F342F47327C13E4C2AF8A209B906EA44CF5E92A7981857BC5F52936680B2CD839B28C61307B83E0B996682025ED3A844B0A440E7CD71C7738DE532FE4237373B329CDDD8BDD9F4B3208AAB0BD72DCAF832E84BA17A3977477712DE42C3D03C83B7CD90D53EC13C4515A760AF52ACE28963078B57108779B48992A084EF5B269FDAA167917774724561CE548A83C4607319987ED0593811B8BC9FD18A15B804E1B29EEF009FEF0007519FEF0047B565FD6F74A3251CFA47665700679B8C0F39D9259AE0BC8B1EC46BA726F15955AD87388A3258E0EB8C0864F7F415325C3D3B4B88811558C21D676117190AA3803C54F1CCC43E33B1CF4CEC3313AB81F7CCC43E33B1CF4CEC00CE3313FBCCC4EA23CEC9C44A74FB5058B485453C345C07A20A2AD9040BBE9D8E6DAEA5637EAFA8534C2E95C131CA2CC6282367A788931B056D5EC016170B1B838B45068F746633D219391778A03ACD044598AFCC74BE0057DB2ED10887EC93024E9D68828325A9047669E7F34C50AB4D83876B12F5C458402CDC7141562D83CE2DC20A03CCBEA33AE9D0DAC60EA38459D710620A288DB952571F47DDE0C347DFFDB21BF04B31F648E13554FCB840F93B43849237804D880F82D0203AB89A9C758D8420616D9A818E8B44415A93E680963D42AE0DF27500BCFF27E4DA20CB033BB1D936D8A2050C9F67A2D1E0858B536367AAAB407C30A44F35111B09C18A72639B6A82B4A89E32384933419AA5730C0427541402B3DB259A98AD8628BA178DC0E87413E3D2AA2FA2B33093618A073D9CC5E698224A9EA812326D7081E7AA844C535C7E76FA5453A4CAA61E86EBB20C0ED1A8E4FBDA24EDCC21850939CACC0C51C5630A06D138A8641515CB3112233DF4A9539B9F7A0D62F1310B8118164DEACE104CDD2467BE0602D1E26CE08AFE8655AE4B0F62F2CC352BFED46906A7AF47C76FC0349FC930397FB12047F868E12CEFD34DD02ED37B3C893C569B6AC4174465094075C9460BECA778460E132E187DD574C3B94A8B4874E4EA537769C371DF6DECB61AA37DC699BAC87BF1026F6F1404F6725BF05EE14D92017D05AE0E798B759A1DC771FA437084A2337689BE2EBDC4639603E9519BB4B282E89A3A00ED51394FFF20F85936EF4BCBF0623FC326DE20BA2F320990DE2293561E99F48C23D4CD3B517E66C97E8A8CE6C75FF4BFBA927804321926CB932C43804F67327666EEAFD2BC0C5C55D63088C6CCCB2ACA079794E7E7BD4F3531E82A4A7285453CDBC5D52EE65A217F93A21A9D4154BDBF4B118DCE24BF67FE22DDE621028691C930C5FB0661198D5B5DE5EF108ED168E9DF8BCDB4883FA31865781138B3B372208DC5ACAA2C1DDAAE0E4F3B6C8E0169274B3E9C43933439EBE9490B3525133BD71BBD6BE7D0CC108406D1C2D55462AEB0C9AD4D43337B240D946C8100F15DEAD42A0020864997BA53C4E62188B20C4693E8CC8229B73540E26BD24D7C2DDC8304EF549C072FCBF2BF63B4088725E4E55D75198EE622327D5FBDAD02118BC5CB789EE8CEE3ABEF53D2F0ACB7D71E3670399006F5A92A4B37BAAE8EE436DB7823DFB51B6DF3BBFA9988E87D15B830BE48578E442407D220225565D910E3C2FCF83649066413868446781C2AD98097F01C33EB3C3B5E2E73C45B4B51C90646B1E94ABC60EB128D70C46DB14B34B10DDAA0A20C365C4C4E2A796716C8EFDB28BC23F6F5383F7F745C246A308D853204202526279B6ADFA4ED239EEB55CA2D8C2A41BFFEEF476CF5DF8D9CCB7E7FC9D57EF9CC9C70CC49F54A98336702A268B125929AF213BBAA201ED95DF28CEBC5831AAE7DB64D7C1C8ECED0C7E33471466A384E07B74B0AB82B940471F9E8CE0FC98174AE831495A51B625747B8166272E6E18E8E979B2811756B54B2991FC059C2DF54B58966388B32C885780B5DB2C171548F3138F066CB3428789386366DE245D2669EA44919449841E78B745F6F52BADF459B40E89A44C34897282EFA7A8B708D3641D563BCF78495D66D89DE457951128EF42628505D647F0F0FCF7DB444393E221F0B2C881D9002078B7FC62771848874D716B80C92E816738F9FD33B94BCD97FF9E2081FC9C7711414C4863DBEDDDF7BD8C449F12ADC1665BA099224ADC379BFD95F9765F6EAF0B0A8BE581C6CA2304F8BF4B63C08D3CD61B04C0F31D61F0F8F8E0ED17273C8576F60B5505EFCB545298A25A3ADA1F61676EDB1CBFFF56FE8919FE1967EAED1ADB86C0FB9D9E6EBBF96AF78D29637FB37D1AA0AA65B6D50B59456A2E55550E2332C216C24AA1ABEBFF7711BC7E4BEF4CDFE6D108B2F88081FDA96EBB4F658AA3F546C8238AE3E6588741114E549BAC9E2A87676AEE196B8996515B0D7026E81A5AB6A6A6B2C9B56D55B5A5D3FB90FF2101FFCFB7B97C1C34515001FD3E79FFF4C8396B9E81BC8635E0545F123CDBBB9B182A57704250176AC8A0D092AF89C6122642A8F4C86327AE797F9ABEAEAF4CDFE7F55155FED9D7FFBDED5FD65EF538E37A0577B2FF6FEB7450BCA3F9DA260E9426C04E337A2FF75C4A80267174E28393EF1BE4649421CAFEC61DE06E11D3E17EE9C30A2749507D9FA915F86FFB2091EFED574EDBDDDDEDE764F73B5A462DEA8C6C9AD0628A3E4D1A667E26E67BD799E46AB84DA847314C4C618679BB48CC234293C8CCFD9039EA164853C40BDC35B88E326FE2ED844F12379561AF3153E9A94639665E90FAFF5707423A7F7692C6C7FA61079BACDFCF58B72B274EB1AE555E90884F2141F4B5E3A87A19A8004EE6DFA9639CEDC87CC65817CC83C0CC8AFE98D97F168719CC7C44B6BFC34A5D1F2F0BBBC067752D574E24C6ADB2AC703A636AF720621EE01EEDC498DF339AA7DFD1D709A8721CF93FBA8F471565D9247178FA3CD45DAF33C56384EAB9996593C892CD575C57B547EC95CBA751DAC50F3A88BD36AAA9EDB72C4A815F56E7BC3825CE1554E2A4CAFECA0DC21CA2A88865B973E0731E6472FD202B934A546596CF39CE16CECA1B028E202F2751D15F8B7F10A3717B201FB1D3D49DB46C0EE8F946D1261A62DAAC4E7DB888CB8A92C2297D5350E29AAB6D351455908591C955D6D833668CFB0F5C44AAD9E86E757311CA6F23C89D573779C449B8016A6ACF188180D3134A654870F727F8D22686D18020F509EFA573F37E60AE447DF8025C4F4878781BE3A490BE7D3B38DD9E9AAFAD8E67EC8C713E974D13ADD46A78109624FC3F4212AEB9B61B766F53674AEEA7E6FC25917A2DD0DAB81395EDEE361DFE628F7D2C2161573E17E11AB10CF5190F8C45C905B898D2FCC8CDE24ACE964940B98FAA55E7B7EF29AE8369DA587F6815F7FC77C858499B12474DF302A2C1F5BC6E7205FA1D2CB90D5505E1AE501A3F162A83943D7E16EC0E8766962E90B29E9E626B5BA052415AD6E00DB8AD452F37EF5E78D7FF4C4007C88DC31765810FA98854D9C0F6B918883B0A12C0062DC1BE6FE83D6D3C2423809CAD794AAE7E7263237FAB293BB87E6DA7487AAFC57C8B55A94F7DA27DB53A716E5BD08843554E78CE706F63628A2F038478117206FB26A85E685F9A890FC1CF6EC9362AE60F47362AE58C28B5F8E2A07E6AD2F57499F7DE5CB0F1A2B3C3AA0B10F71B9C2312F59F901F3B4CE8597BC9C4D57A0C7BB764A09E3857AF987BA5CB94C5FE607E7C5072CA747BD6D85E5DD3F79ECC85EA2F6A30EE2DFD272BEAFA69FD172067BF8E0344418E0D20DA0E6697C88BE6368653EA6C7AB558E67EF1E9D87BD386777D11C3A779160F818AAD62D87BA06B0BB2046EEBC236167793ED49AA08587D21CF14C36106D41E234E7A3E1E88910A49E8D5CDAD6E3C7C2BBD6A3F189B72728D2525655E8F73E4043846C2BBB9A37914FDB9B57D5B5656DD0D9B95C15031A5A0107070031A483DE12B0939DBD48CC95A8E5895FADDFAEBAB877E62EFCDCC1FA95FC3CD8D63E8B8F3B223E7A9259765A127D12B2A3273B800AA6792CCEFE80DD4149F60C33901941F36143C93C89ED0EE553C28EFC0C576F9AEF00718D5635F3E8E3A2EBDC878BCF79511D3FEE2049B1DD902A6E38849DCDDC61885A853C35E182D19A957FBAF907B591D8612DD278E9DEABCF98917247F91AE4684D2CDC9D603037BAD8DEF890AC09942F1CBC4F474B5A4677F139F935BDF16494E4D3C0897DBEFEE7D6B4D54CBFB309530BE4417AC0583ECFE0868D3DDE6C52E796E14DDCC31945602EBDC090F851AE281E1AE2F7CC1D4363CAD8B1596E5857F799AF7587A116655E75C75D69BADC86E853961641E2739FA93C87EA5710FC6CAA15A08F79E8AC013DEDD00B1220DE11C287CA7991F938A27D607C0DA2F651411F9A6F4FF3E4A645375202B6EF8B3F41F7247775F75B0CB074F372E2215C54C66E2D2182F771B224F5FA46FCB2775E7CA906FAD5DE3B322416CAF4534C882BF7C38EF864DC4431264167AF4642B878D5A212D1DC3914164867D8E7BFAEA823C13B8E2FAD783133DA0308E7C807E1801BB465835EFA6890A7DB83FECA1E9A2CADCB21678401265F6BBB199287F4413E65642AF5F6762D54CD1B112D2CBDBB0C2DA8E6EA6150A76C02A6712F6202372449EA6069DF8A18800D5F8AE881E9DC89E822E9B1585A689A970D5A58FA172216705E9AA8A9B5D081D2D3F5EB22F99B510D4DBF16CCD5912BC47971B6C9FA385E9C82550FE15DF42055CAEB2068EA09B5A1FCCD93AE96500F4B571FA777C83B4FBDA650A90DE56FD87BC32C51B7A1537FA1E1C3AC89E367EE173137CEF640EEFCE1226347C772CE094C7EE70EC28EB0358E1F425E6464A37706E927C91A62C83E591385EC87CE288B324D34ACF9F5C0AABDC295BF8A8A308F36E4DDD6DE1E0954721FBDFC8B2F8D162D8D3C418D961783771F6A51670B95AF28C849F156BDA81A240D8D8008378683E1651059B9AE927A3694D3D663CF51EF56C1C4A16F4D365E5661EAE420B8AE99243AC8B73B9E27F54C0F68ADD563215C74BAA71423691DB7FCEC9F57011DD7D0D75DE179F129C384E668ADB3A8E2B99DA4994C44D1BB5A454541713CBEBA788D4214DD5BAAD4FBCA6E11CCEA01F2612A5C43F9890D5C63798BC7DBC37989CA5BC3D9DD8434553D4C5A6579E021E8D7E7A88C1D695BFB18AB1F22B439C8644F180E1F657D4D7EA0C67572F11FB26A421795E6216C8ACBD2734EA9EA8DC300116C3B16087ED65B870902C6C1F46A310AE274E5BC48BD1A79FBB12A382FB01C45CA3B9ED197E97D94AC1CCFF9A82C1D419E435E57665F59E8122FC679EBB94A8B88B65FD3DB444D7611CB2DC472FFF0703ED76FA5D620F874BE8992207FB47A4CE4725BF4B6E2BB6214489EEB3E8EE3F4C728818EF1143804E6E96B5B4E3F55DBC1A4646736DAE73DB20219DF7D7647F64AF2CBE796292B49BEE371839D8A1EDB66DB7DBFAE3D12AF5C695AEDA6CD61326403E25BD06A3E051D475607E3554AA6C9862DD69E93AB342F032B156E5DD36646FA9AD4E1E37B2E4E5151929B1462A36EBD1C7910A7954981B99F181498FBC9E1E7245FA4DB3C44F6834DD57753435538EE435CE3B88F2E7DBBE47BF97E4631CAF072B2E323FBDA36CB98AD3DF6DE5AD18B0F466BCE93792AF5D5EEB0A9FA0F83ACED220C917A36D4DBD61BF1089A77C251B2F571AF3D966C6DDE362352B28DD6D3D6B526A9A6EE8864E5E5EA697E6DBE1F0F0E7115EB701A552D3726C343CC0E1FCBD3CA00C06825D9BFD8D456B65D4B5C48F25116D37C043479D4F05D2162C35B59DB9DBCAFED703BCBECE6CF37B41AA623923B6D3DD391682CD5533D57F145BAB221255CCD86869A6A23DBAA8521A11D2BAB8FAEAED3A48FFEE6A1965D1679433447BDB9902F5E154F227DD3E51196DE399DD484C4A3B728834D17AC43D36A4E7BF554CFD6C65151E29CFCF1095A09EFC6B39C7E62B25CA58533C6EF4A371B3D8897EECC3BE87EBD43DAB526E6B41DF75155B5633DBAAAE31E1D3BB22ABC5823B4E1C13D8854EE7AB2D19464572809E2F2D1928DE96B5BDDDA30B54714C9E6E7688E979B28F1768B47ECD9CF92A5E1E10CE32CCA202F9D919AA9745D28D72828FA6B7EBF26BAC74591865135CDB4C0FC5DF942381EE5BDEB346E0BB72D5BA0F8F6A049B9DCC66594C55188BFF966FF48E8620FC17D89C6E2B358D07F1340F1124439590D414CE274967980875B5CAF518205A02066BAC09582152DB2A7B1C9F076B07CCE29CA88BD7652C2FD75FE7087CFED334383F1FA909A7B2D9200DE63F44B09D5071802A8537E8E7917876F47A79BA828BE032F51F43355E98AE889AA13F4A7BA42A701EA8451265A546C49C6DD798AC531937CA9D720CD36C1ADBF2217264C3AE57D097087E6E7EEC5C1816AFE99F86E3C21F519A3D083A4AF92A962987707DA9047B4937C588C00370BADD07ECB82B3AB7283309EDF237E5C5E7F4AEA586A7BC721F93E6E4D5084D5350CCF55E34F4B2997F6BC668897C9186DEF319A75472A937B994BBE077943CF77E8D0C3558C77FACCB9FB4C771A99D3DEBCA712F139FF0EDC42194EFCC0C95379D2D31875C2139F6B313E80E44BBC07FA4C339D7D1FE63888E5393B510287A82557EA312C9EA65C309797CC83CC3EDE428E74FBDE44D32D75C4F136DFD41738942EF989CFB86C0C7777CA45975D9FD34DD039842AE9E94FB3306EBB39C564EC9B991E719A9BAFF0285DF2D39FEEB62B4F64CA2BDD909C3D6FDD8F80F932E4D5A6D412814E53F23920651D275E5B5BA4FCDCE42B5D1C25BBB9DFD1553F311198AFFEB949E13BE8CAE66DC36FD0698436E9896FF4D0B8493E263AD5CD3CDB47CFD33DE274331E7DB3CDB4D4C7CDDB64535FA051E8E4273EE9B23134FDE034538EC58CEF60482E0B314B35EFCD3768903669ACF93690A0C0E85046B30E8DA1C5F7A6B8C95FA7D97789DF167597BFE6F8EE3A417FC2BB2FF02090C2D6D77DBEE0D8289902D893D168C2E11134FEDE94133E60C7E363C60113903EF5A79873039B913927BDD9DA01575FCF1BBB16D93CD14D5D9BB666DED29BC9D6E3DEDCA77C461E6ED2E937E7E4E624829A03F92EF5E4B3E0C586193AE184A79347210203464B1630D682B1D33EE955DF9CCA8462809F77B69D989A979FEE0EDD80B876C07E4F67AD7B9AECE9D7F9D493AEFDBD5D98F841F1CD79DAE710DFA69B7223E16DE609C73C4573E5AF36C8EECB31734627EFECCD1BD5489D19E98B4F67AF3DF0C1690941C394C6981C76D1B0663EBA3035B4D931EA5059DDF8A68C496D7066A5085DE9B30BA13E3725F47FAA757F5E4982FF288C38A64E703E22510EB8F5E727F3EFD3A5170F9E7EF353C9D4DE7F36B431B71F604D161A9707EE0431D3F5C1C44460788530F3F47706E6BACEE08616F7BBEC143E8F8FA0856BF8FC0E827DC82F0D9F0EBF5EA454B4311A8E4EFE89A843165B6D6729A3DF40CE937B0C990A31AD46D93B9E7D8B9F146D004FDB8E4216533AFECD4306DA4E80FD03963B30F772D3D2E7C91F63F2E96767679E7E65BCC351E880FB228DC767FD44B4A11A67BB4F4F4A267078C051E8A3FD147BDBD9A4FD4414010EE9CE9242136D4E43BE684A32F3D7A5EDBAEF78DB509D49A182F74DE643AEFCE68464200D32ED9D0EE61230A7A70443E1725748411AA8D33B29505F629C95A8E49F84146463BA13A4705645B1C4754A5C03E5ED11952ED1BB282F4A1242FC262844FB37526B814AB6CDFB7B675D504C9E3216E11A6D8237FBCB2A24611D56B3CB14A88485A70E0AE103541EF4092A5BF7238D1E54FEA5A680F2734D99816FCA3EA5F8821E7013E851EC439D0E36BDCE1A00166E93844F0825A08F0985B43FABFCE2D0C706BF539BE5085FA89321EC3A6700B536F31250EB6408B5CED140EDD56F207A9F2DFB4A5F6288A298E06E226131D9207D312506BE56AB1384AFD4C9107A9D3380DA1AD60AB86D0684DCE60DB6B836D400DA5C67C0ADAEF386B1616029AA0EA47C41D199920FE82E283AC400F49D2E53F29D2E5FEF3BF28F28BF3008DFFA500BE86D0604DEE60D60D3AE1E023E9D097D83CE1F3A6CD6E0DE56278347CD5A636FEBCD584164F91ED7E76A7C417A50F679B26FE81D30B409B6647B907785CE1FF80E2DEA08DFA133A1EFD0F903DFE1B56FC2B7F802D0F7F8328363D8287680016C72E0D16B3287D620251A88EB90CA04D72295CF7D87E28C39CE8CBFA8DFA38AD29C9AF2429F917A183AC65F93F073B0F244AC3CC06E1EB25DD3EF76CD0A4A7B0B45DE76ED24CDB0D67D03F951F32EF5D1B7810E494273334DA33785AA65D042676AD05C645503E412EDBA228B332DE99C56586A99AE0EA23465C72196B41B3229AB693E10EA20CAC05018445D76EF103B9C00DF5C8FA88A1FB6247336DEAF8CDE1551811D097FAAF9EF43DB027D94C4BD75EC1A2D9154354089C3A62B74EC56B03BD2E0AE5C0333AE7DC296E3B2CE2DBB468B0A70DFE4CE010E9D13C59BB69A4C70B1EE5E25C049BB0658B9BB758B1232DB2A900869D51D3AFE24DC2569844AD76E7142625B4D26FE5977AF89B528EF1D148C116A2CD4D2599802209AE0E0DC4141F93C7471A2796C23E649561D1C28CC813C59E1BFAA2211ED5DBA73A4EE8FF898C3EE7688564AC07D92478370E896A848A9AA29542456DDA3A27CC1BD938501B3DBCA995AAC06B3AA25D14F5AC87E4C2C2B48FA9307BB62E5BF35B7D3418A24A186C0924915440E5D93CBF18AB84E1E3A0788C452D5943551D62D9192241480C79920B586C3BE4B833B892A548F87EE4DB1A588B16680AE0E04A4B1DB24805AC22A54E8362D8554F9DE290DB8E228BB8DB96BF2B143D4DDF22E964E366BAA83411146C3B17B631F0C7C3C08A073CA90114C63C54BEAAAB9F2EBE749E40338D281BAA35A32B95B77A790D04537FEC16EABE575E72E8F26BD2BFCD4D57D1E70A4F5D67989E10807E1F71E436324345CB59DEF36E6E8BA9AD95539223B77777C7E57EA490B69FEB5BC6EBDDD804C7959077A8A024330EC51EAADFBE2F57655597171ED32FBBD1BA472E225DE924FEFD68BF5F153F619725FF2D4DDB16E427837B6C1FEA95462BBDD41DEB042D553B5B391A72EC3E62215C0801188CB3074961FAAFE4B5C6B3C759CB359698427D820C5BCABA2A30DD0D5016F1CA6B99CA175D55C8915F5C4977D803389A2AF72F7080F9D9DE210021C2614DD95BB0078E8AE6817556BF4E5164FF2EE92879F0942E710D0E5BD3EACCDAA9A04FC139FA8C10A5DA64B141755EAEBC36BDCD66883EA5FA7A888563DC46B8C99A0EADDE91EB42D739EDCA6AD2B04D7A2B6489BDD2A0851192C833238CECBE8161331CE0E51514409EEEEDF82788B8B9C6D6ED0F23CF9B42DB36D89BB8C363731B381117F0AD5F75F1F0A6D7E5D1B6C143EBA809B19E12EA04FC9DB6D142FBB76BF0BE282E3176410C451A3A6EB6A2E4BFC7FB47AEC903EA6892650337C9D7FC967B4C9620C567C4A16C13DB269DB97025DA055103EE2F4FB6849761919C8F044B0C3FEFA340A5679B0291A8CBE3EFE896978B979F88FFF0FAF5DBCA168120300 , N'6.1.3-40302')
END

IF @CurrentMigration < '201611122120497_Ares1'
BEGIN
    ALTER TABLE [dbo].[NpcMonster] ADD [NoticeRange] [tinyint] NOT NULL DEFAULT 0
    INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES (N'201611122120497_Ares1', N'OpenNos.DAL.EF.Migrations.Configuration',  0x1F8B0800000000000400ED7D5B73DC3A92E6FB46EC7F50E86966E2B464B9BB67BB1DF64CC8926CEB1CC9D651D96DF793836241556CB14836C992A598D85FB60FFB93F62F2CC02B2E09103792258FE244F8A870F9884B02C84C6426FEDFFFF9BFAFFFF36113EFDDA3BC88D2E4CDFED1C18BFD3D9484E9324A566FF6B7E5ED1FFEB2FF9FFFF13FFFC7EBB3E5E661EF6F6DB93F9272B86652BCD95F9765F6EAF0B008D7681314079B28CCD322BD2D0FC27473182CD3C3972F5EFCF5F0E8E81061887D8CB5B7F7FA7A9B94D106553FF0CF93340951566E83F8325DA2B868D271CEA242DDFB186C509105217AB3FF2943C9C7B438383DBE38387B7770FA767FEF388E02DC90058A6FF7F7822449CBA0C4CD7CF5A5408B324F93D522C30941FCF93143B8DC6D1017A869FEABBEB86E4F5EBC243D39EC2BB650E1B628D38D21E0D11F9BA139E4AB5B0DF07E377478F0CEF020978FA4D7D500BED93F0EC3148FFCFE1EFFAD5727714ECA09C3DBD4F8658F4DFFA523054C31E4BF5FF64EB671B9CDD19B046DCB3C887FD9BBDADEC451F81B7AFC9CDEA1E44DB28D63BA75B87D388F49C04957799AA1BC7CBC46B76C9BCF97FB7B876CF543BE7E575BAC5A77EE3C29FFFD4FFB7B1F7153829B1875A4400DC4A24C73F41E25280F4AB4BC0ACA12E57826CF97A81A4CA111FC27B7E53ACDAB82F52709511E74A9240DF8BE1AF32228CA937493C578C590C9AB814F71FB3EE3142BB8052A8A8AC4BA71F9E34B6320F26FD7CD32C73BC6FEDE65F070819255B97EB3FFF2CF7FDEDF7B173DA0659BD2A07E4922BCC1E04A65BE1DFCC85550143FD27CE9F421B83B1F83FB6855CD3AF7CD93759007219EF8FDBD6B1457258A7594B5935953D577AAD4BB3CDD5CA7714F727DE6F745BACD43324EA9ACC4E7205FA152BF793575C617E94AD93EBA98D8C03E57DA42AA886913AF5012C4E5E35013E9626213FB5C6913A92250135F1FF65BA07263A4E6527B6BECEACCB839766DB0D91E99CA936D90BA7BF2104CF9A753142CDD763082F25B8487DE19E52A8D92B270C4C931357F8D928490A10BD0DB20BCC3CCD29D234A94AEF2205B3F2AB65EFCA7873DFEEDF6F6F66D9C8677A89BCFB7295EE74162DCE893189F171DC863697E3C8A272D1EBEA37F37C6398D5609CD07E0A18BCD5B73B6494B3C9249E1697CCE1EC27590AC9027B87778FF70E623DE059B28C69BDA3FB7A8287D352C8F50B2F48B8937BD65BF34AD88EB7D1A3B6E7CEFF3749BF9EDD78720CA4FD23875EB1A4159948F31724341798ACF256F5DC37017E81EC5CE8DFA96B9CDDB87CC6D917CC83C0DC9AFE98DFB88B420AEA3E2DE122FCDB80C328621B1D8EE31C4376784BFBB22149819F3C18AD4489F234C70A52352944471902CCF93FBA8F475EC5CA6DB021D479B8BB46772ACB11C17A6B314ACF595DFB7782EDEA3F24BE6DADFEB60852A12715B32D728DB3A422CE2B4745AFB8BEC78B98C2AF193EB90C53C2E321F205848723B003F0731663D2FD202B9B5A4C6596CF39CE15A5CC0B068E206F3751D15F8B7ED2620D536746A4D776D4DAB45906B6B5A5584B132697157499A401B7B70BE70DF5459194135222DE859CD446987C0B185F285E1050B998E303E5A3075A6F9E3C0E052E5A071EDB21543DA97311DCDCB201A9AFCBA08D43492A36855956DD3A0238D161DC99B7434D0A6238B46656093703A4C654C86405E6CAE295D55676E1C15E559520E12175F181A33B68C62F0B882A6A3784DEECA7E24032DEE4A414D6D32156D6C4BF8D1BB36FB9DB9F2B5AA38A306D646F1DACB1AEFB7D1D24ADDFA314D863559FAFADD01AE868CF1DF3E6E3786E289E5058BD371C86F0183E7A66E9BE5C7778D276F2B942FB4132C04B5517B6D992EA9B957124565A60B4A9B40FD2CACE3B20CC2BBE324DA049CF6D3426827CA6F57E5C30916D73D3587409DDDDEA2D051FD4D707C74ABBEE27642F1702590C6CBF487EBC85E9DA4859BC07B1A6CB0F4EE782FB1CD3D90890F12398B117DCD6235220D4610FB189A0F51591B683834E8BC441B8BA3721CBD2851BE6DFAB658013518C74B22F9E07D3F776F5B0B99876B8F7097C12A0AA320F106B82076271B2F8019BDF4ED48621A6B9B3C0A1D154FD7E47AD14DAD87C2345906F9A32DDF09710798B34A42C7FDA10272DE216ACECF7D986A1CF7E6B8027CC95679B0440DBBE932BE0D12DD226F928667F69D978B953CBE7663D3CD4DAA6A639D2F348D24CB5A54E59936E463165EE2EF6B0C9B50926F1C5740D24CBE94698317EB341B6A2A55866F649725695E9FEFA6EDA8E74F5FC941CACF695D46BE6F6559D656541F241EADCAFC083F3EB85ACC447A38657C6A5B06B73ED5B622D153305B8ED56210760EED65C1D59C7181702DB1592A00C454A698FDA7DD79AC6BEADE7347C8BEEF1E48FB7DB6EA109397125686A2A86F5DA3F1A90BAF629D665AAC67ABA5BC13ABD84EFB68B68E3CA92037E45A9AD801463975956EC3BFD7DA4C772D598DD3F0F04E486F83220A8F731438A17C2496B3C85DCEAA5AE347055841796A910779EB244E0B748A6E91B3645C39F02565EE7C0A9CE4119EB520BED88677D7AEB6342D98FBE1546B172F83075A9DE604C56AE66CA1F2BB6B544445E9AEDB68C8E0345D3A2B986B241F1BC169D3352F34CA8179E8A94705B63B89BEC307812F62F063367D5E7C488B32A22CC2ED4D96BF5DB969253D28D4A3D5BAF435C095AEDA0B55E33DE983E3D860884B57889AFFA13577B5DD0E9DBE8B0AEF8FE9F16A95E359BD47E761AF3EB037AEEDE7D38ACA0880B346B631CDE1AE4EAD6C629123734938629E4BB523F4AF7887CC7DAD3FE3ED442AAA9DE29F43F2655D06142A4996A074E4F32DACE834255FBA24D8BEBE80AA9554298BB662208D7656A5646DC4BF06DA474A8CA106775623289AADA315D796CF6B22D496CC49F1196572F2791B755A5BCF5506D7563C6F6ACB73975D96B459B89EB4C0F1637A80D70AA92E0CA21603007C5EDB8A1BB71E5C612443D842BB4441B5D5E7986ADD9A9ECB0C90495E055C7006C87D0664804CE59A36485F833974C2287495D2A1D2DE59EA99D3DE5948F11977163B1DDF749ABD4AB9E343783E41719C2617F76E628F07EB398F5A265727E5673DD55C7A2A1FBA921DD577EDB892CA87A56685F1B720DE3AB2283BA5313BC3626946A09C1D5189EECD47D7BCEAF0220F43C4442DB0AA7F8D56F531E86C9C71EE2B06CA79511D247E809262BB2155DCB1082396F98122FADB2874C7699DF73FDDFC83DA43ECF11669BCF4D3C3CF983DF283F435C8D19AC4137086C27CED627BE3ACBD23385E40F07E1D2D3925A06D8C8E5FD31B1FC6E4DEACD22B6DBC8F2D7777D5FA35FFEE6677DEA2B84A0118C8DBB1DB70A7C79B4DEAD626BC85BB9E4F04E3D21D235B3B42B836C1E3313BC9B50BEF6760B12D5DDD675E561AC6C1DDACFAE278E9B2DC86E853961641E26D43A9A2AC5493EA63D3ACD03C0C7DE7A0E163035EA007B7FACE77548BCCF9B87506F81A44E5298A8347377AA92EC97CCC8ADB6D9BF96DD9809297BF2411D5BF266AE6F3A4EE55216F0A578C6B13930B378E2DE22D5A4815EAA356B8D23AE82611D03FB739A6BAE76B1446125D7885D7E673235327C343D2E49947B320D5D4D7037419B045CC1800AD62464FB765C42D44DDAEBE04D7AA36036E5397EB74E34793A0A17EBEADF61C5803FA8C8F4BBFB71862298FD031A030F615D9E314EFF02B4786913894DF4431A63D0F01D408F1E17310B5B1ED6BAC3ED6FD1437A0D741EEC8B6D10A453B046786C2EA0649BAD3B1D42ADDEFDA6DE33B5F9EDDFD24C5C0BD5056D6D8B75233E48C76903079A0193190989F3B70139E04BC1587D916ABB3E54B41E8C8E274612BFA3A5FF6DE0605A2EC0E99A34FADA1BEE216EA80A4AD2AAE3D7A5F11A615BBF1E3ABCE3F82A22247E75EDABCC6A78C0C0C7FC45BDF3DDB5E14EB5DE54217772E37CAD617C8F6F7C5F6D7C396B7C10E97BFD677BD2E57BBCE37B9B6F79C9697910E778F36578D0375CCF6DDF3E26C93F5AF2974773443B52A4DA5612DF9DD82CB5582F5CD818366DEE12C33D12DB9A892D475193B6D402E92B37BD49101737C5481EFE2794CF17EAA822217A82CEDC472B15DD20F2241557B16E7C175EBEC1BE42CAD7930ADE1896D70F0749792B838C65B4E42B859ADC567B59C16190AA38004D9B5E0C2C5CA63F1E1C2580E281EC0387843DA0A9B836711736781662DC3837F91B13DD23971489DFCCEB0063B047A952C4EC34546182DB31AFD90E9950736B4C12A847731AB8277DC04F6F21DAA597104F22ADA4BB8BE25D15EB4A4F88C2720F9BCCD29D8D653EB793DBAB490380E6B42D7AC9EDB3E28C4BA664FE967511DC17CD8C7F768C35E2B822CDAB3DAD6EFB09EFDF32AA0DFD719CDD8E2BC2034EFC3DC7151BD25729266A220666AB1828A82DAD547EBFB350A5174EF21047ADD736767881AC7C34B7235909FA7DB7A2CF707DC6A2C3FC35D5956B90691FE1C95F1087466AEC31FB8BAE7595FF152DFE0C2BCA279D7C73DE4371CECE31FDAD7E50DD5BB3C82A26E939B80DD9A34683314758519598AC676C182A9E86BAAD7D6589EB233C6D6AE9DC187A353FAB9B453DBC980D7749C0D8DBECD90D493BEF17F17DBC26440DEAB54AE9D059174741A54D87247C814B647B1849336A01D3C0361825498559C68C9D85CA0D05B007EB46BA751109327C45C96AC3F1F360F3696E7C569545D627B60A82FD3FB8A1F7166CCA3B2F400F493BC838AA9DBFD98B84A8B88B6A677B4A051BD6C264637E953C137CDB88827BA6DD08F27A0136F451153C0B27D0A9B53CD134C64A2A1F3CDC4BE53D59A3A5F680B4996B5A4CA336DC76714A32CCD65F3D6CE07554A68539F296B1955C2F524353B46E73D432D0F501F9CB3D6E919947D14D42809885D1A25BCE23F61E175608BDD16BD7BA8DD7513F97774ED0D592DC7719CFEF0F95AACDA1250F3A549601101EF501A1C0EAA8DB9D9F281485D6C0ED8AA7162730D1E5B92A6D884E16A82E8284ED026FB7B554488CDD3E4C882F3B4D9A6A7D515DE2C03999746F6BDCD6647A74E0547A7C9321D9DBA1AFC926B0F7B0437447CC195CEF37F4EC90FA98113CAE7F1641EA6BCAF34EF61D534C2F2CCA26A4F21F8ED9E9CF4938837DE1E6D9856C4193EBE24A20E7FBA8D22EE6887C1548B3DAAB69AEC51DD8167B2473595768DA1969524CD1D8DFD1E5C84EDD78D606C685F8F3901CE3C887731E49934DA541793B58BFC39D0B6AA88EB995CB7C294D8E7A57407FAD5233E7F972FCD27078436A9346916DB21252DB651B42ADDC661676DCDB89C120AE6A2768E229068AC2FB560C22C41ABF5D58A22DACBABAE30E3EAAA1B60B3B8FA9AAE3CAEEEDA3A4545192575900EF793894273643429244786D31F275F93B18761AA811C47A806711C9C7A7D8FCE45A8F50910E7CCA91A0CDA60AD4C903703B616D1DEC168B581F62ED6579A7127EB1B61B39BB1B5A7E216CE93651F94C79AC1F6C0A3DBD869EC82C06EB5C0B55463D00A033467FE6C46746F9524B623036DD3F7D6581BBD5E408ACFB8E8ABF82D16CBBDAD3715EBE26989A1644B9F8276AFF54D759F64DE5287D5A3BA2196AC18E6F6D8E4A65A6A695523029188980CF1E96B26D7F38BDC2DB6F87E2993236F94871751FA2131DA57667EBFA06D82F5FED2D49D6A8F71374FDF99A83E5ABBF300866BA871E78DD66F6020D7F067A001ACB025391BEC686E81C233C9E00669BDD9983E74DED59979BBB17EDC9CA93CD586E365995A3E463EFF6A77581B4367B174758CF3C839DC24364BF2B0B9BA51860E21862C425F697EC7104B3681AD3D95D661671C44747D637C3A880C852E55388AF80EEF6AE898C1B74CEEBA61B5066B7A8A2F880B81F61AEC2BCDB806F1D76D165F53AD23BDB1BDBDC3902C3B57874DDB48A6E7D9F17299A3DEAB7634D91F8F2B6D4C3BE677E8537BB46B56E2735E94C1A67B3A40DF115DBA2B34C4006E0B4DDE777A41F6FB82982B6C0C4011D35D4B6DBA4B018316BC50BEA060000B39A9197EDF46E11D0915837388F9B6F62EC6567C8EDD64B6EF181BD539BFE27195166E00BF1F39D67FE94F5C99FF325437622FBFBE20DF71B68CC28B9C2BE8283E54CFB01BC90E558D590587AA0576524357752AE6C5DFEA77BFAE6CBAEFAC9F73BCADF47655A9BBFC3A2A87D65D93A958706D09370327940471F9682624F495E63474EA1A6165ECC4D49E4AC1E64964385E6EA264128B44C2199F254B734619C05994415E3A233513D7F59C2CA926ADDE408C379FA0E8EDF8278872A32329D0AB529414FA5CA9A4401531DB1EB61B6A7338DE96EB348FDA913D2FDEC5C1AAE8064063BFC0D95172C0E0505B46956B1DEEF10B8989133FE2F9A2B76D76162ED1E606E5EDFD4D41B6E42A60E89BFD17C28C31652FF1CCE2359EF6158ED415AA35D9157EA92EFC25B94BD21F7DF13F1C89F353CF8462761A8709C7B9A150469B99EA3437188E81B9B9C2FB4882B467E6E3D589EEBC7C45A4F94DD93F0EB4A2B12A6C4A8BFA1AA6F4DB94286A9AB27F56973D8B23C2FD3485C5907A6C335090A5CBAEF4FF52976EA2AF76C5FFA22EFE1E6D165910363B7553E7AF16B4CAECD0D6B44AA1CC46AB97DB122D7529F52DEE14555A6B951F17458AE7880C047DDDD3EB54EA9B1DF6ABF88CDEA3AF71D8C2BDAAA63F339BFBA14B3C4851868705EFCED04A1A00EECE9F1E986B27F7857F13BE80393A9413F62988C99BD078AAA2A414D9BF2809A32C88357AC9D585EF66FB5B4AC1E3EAB0FB189F738A3212882D293586C4B915DDC738CE7668BC5E1F5214A44558E9E6261DA22752C62719557810F5548D199568E8AE4C4E2B74BF9F0289F4AF4BCA2614786AB29FCEFA124C9F3AC4D72929B0FA91CB514843E884CEDC38D384D05B9DAFF637B5B31184ECB132D5AC0EBE5CC6124D1F597E68BEF53EA33AA8F86FBC38381069D49AAC065AA433E58CB2CE91DE06E641A739E2038CB350A2FAE104199168BEA2409109F378883E356ABE7ED27F487CFB81DF37F951FE94D40F3EEE1D87E42BF89B411156168ABC3A0C37CB03396B75496B0F73A363AD39D46907F4F2C77CC72CFB4AA1F288943CA3EC78F04A1E42B4DD99ED4F62B07BD31DC9E0383C81B3B98F932D9B662068763FBB750069FD1D0E783C5B4D80FE0E56E1D3931087307A3A5FE51F4B988932E8D86CF2090503B5D1142230E0AA4D85C53361C13C6D26608774264D168DC5845680BE3B7D7A2232A122FAA8E61588D8E3462850A02006B08B33341AA9887D9A8E56C4EE3F1162217E7C03F3CA443574251226802E035679148E491C543F26250CAACB4F8028C8A4B4519B1473491593104753C28C2581E3561A129D2D9100DF9E8C5080F17C22C4D284201A98533E1E91402A5ADCA60C765A7523D825CDD92215FD500BDB75E7CF4FBEBB5493AEB71354DD1D6997A9B0E19D0622CB51761BBA0D93D31130C64F8498DAD0402A96838F3BECC4BFF04187281BA6C69C6034FE85EDC774FC0BDB659DEF8A61C166A68F230D0239F24A21473392C8D15C34726474314AC7439B8D3EA8183FAA498522743B5109143EA807A4230F8D462C629FA6A317B1FB4F80D5A55E61514C2DF424CB90943B402DD03B653D64EBCF3A16A1881DD29C2BF08D2E535211FBEEFAF529EC7298981BB29995C420A26C73D6A200A334CD81237AB080905EDF977D0ED421ADB3000C1A66442A60DFDDBE3D25A10CD8044A2243792015A919201583625C6231B7FDF3482DE6267F73920B1D5A6DE0BC605FE2F27000B181DBD494E7F7F0A1BB32F5D143F7FA091C3C9742E4CA81591D646CED88657EF616EEDCD4E463CDE7CE4944621414D96C2BDEAA1DE24F5544A488B4C2C38EC8CD483BA73385B2E7A98D88483A0EAE2D98CA206A405A829E7C7635819A494E02BAA233497ECC9DCCE5A31DB041D6D95F64A19EFC10C96C3B8BA45B53138CE98EB20B44332854C351345D096646A11AECD074A4622352CF4C28D4B3644AC717AE1C442CF43B6A4697D33CF6B4D7D3929EE9CC5D5FD5997E2463A0C501AB9B302D116918D4A95FC2D322284D6A9AD9CA4ED9D1F9E8CBD2F86EC7A84C6589277F62DE3375CD619E27EDDCAC146568B5878BEF0625F57FAAB5CAF22AA3D016FF0D80C8F8968F4E6D927ECF47769241F2D3A0C93CD27529102CEDD14B7D67484ED9519DC9F5E9C3EE4063737BB3F3E1D1D5D3AFBE31B323A979EFCCE06E4D4D4076176733934EEFCBA31988455601F43196F941A9084AFA81B982B20CF558679A7DB9BA0F0D8E87B64C4074605C66193DA88334F7B44087A1D6B7795646779E36AA82AA2953D2986AC09F067DF5CD3E4FEE31649A3F6AEC675DD911B6B21E7B1EF773452FE7D9BE8401796A945579950F4F7CE55BED9F9E2A58501FA1E3EBEE4842749FE6A11EBAFB5A5C16C1D915AA919BDD73E546A21BD0087F32C23132C31F87728C0CF2AF5188A2FB9D201E2EAEFDF06C4B62D7FB23275914FDFE037C2CFE91290CEEF13CA4060FCE533BE7DA68EEC3C4C0076EF7476642D078DA2AA109363F3261719D9B87A2B871781AA4D4C6EE1E96F4849210297571C6F50949C49D21668BB4733AB34805D8772024E9383837614232A2C4FCA1F906DE95F24348D09B56768A08275212FB373D2D8943F19488897A906068CE81A707FC1013F4EC01E51C4B3D99302A3189FD9B9E98C4A1D83D623AAB5E98C0754A5CA38B88DE446D27C9E8A10462BF7F295013FEBD685ED6E0E982C02E50C9766A7FEFAC7BD182273581B25804EA401330A83C5D9446C32E876A0A0CE0C960F46A3761B4C546D4E903B5854B4D014728A18DA8041BC4A98DDE04843A79A06E6D6429D4AD9335EAF69A4710A3CF1E9A1B26A0AC38454CF60056AD8D1030EAE481BAAD41BB50BBCD18FC766DD2037CBDCE18AE0F57D6A929A7263A7318A78BFE02E174997A387290418436B48300D0660CD4A75DA2040C3A7368D75983EBAB4ED6A82B59637D96068674E7EBF3B4285BD2163A7300876650051C3A730087D7ED08587C81C1FE35B23CD0B9266788DE286E49A4392A93C3A1CE7AEEA8E2AFC5F7A8A2F4D1A5BC3E67F83EE54B265D87BA9352606906905A1E92421A38A20FD9DEEB8F4C7DE84A07447C7F036A3DF302874DF7992737E85E833C817967FBA72280AE4ADE9160DA29BE2441B5125AB092DA401F4106C1AE8BB2C70F249DD67A2B41E8C8D06B09DCC0C8780F6D5CD562F0306EEA50FDC0C819C4F66755A15AD1FDE95E2AB82D036460FC943CA1E5EA6223C4CB9699228EBCB862E048F2C60B0F0E1D3FDE70F4A1CE815190C441675A2D4642A71A0BF1CF92DA92AE7AE9221DB31BECA634A837D75828AC37D35D617354008CBC575CB231A825FD9639D508ED06FC690C7B0EB8CCB0083271C3BAEF95F424EDB7E0E601B598F6F030EF2FEDC4C1D686043BAB7ED25182E1BE4AE308F32D862209B3AD9608722A20B8FBFEFADE04BD95771D8A8A0B35988B8B2B765CA7D75C145CFF9C93182A7478D6C5F8AE8A096322BCBACE3E13D27578442D577A2BDBC32B1D0A4D2AAC552E38A9E14AE7A29152B5256A07977E1EA93B2ABE6E04B5F5C8ADAB47A3F795D6B5C0DD9505D7119A0CC4D631EC341046874250A885ACFA4EC53784BB2E0B80C8B71B0881387808293080AE4B349C16023E13A90F12F1E5A1FC58191D0CE6478BF9800A4E8100745AAA8473E8B65CD7A3884C07375BAEDFD1EEB954B123571D5A537ADD2A299D8B31D6200A65A2AC59D13813566D60D0EC3B3BB8ABA9A28641CD1EDCDB34BB3FE10E2786B502866220F615D30579F4ABC19D6A0047BAE3795AFE74702699126270A387A23719AB1D26D8E2F96043EA0E0FCCB92C229165C7A79B6DD519A788AC23365C75C6E9757DA2338E0F0303745C19298669B82C560CD574F92DB812693C910D0E61A21E062DB58C3ADA89D3904CA8AB11636F0C0E8D5A73230FD1E13A2463AB7314C123D463A274F4977548E6EFEF344A32577F10D4EFF5A0C690698445006EFC34064AF7F270C6E1518B192A877EA01F6A41437330261335A43EE7D08D99967F3A7BA535E4A1AEA3E1D7439CE2B21D749606466AD8A99AE993D2AD9AEA8FDC1E64186EECEB57C0CD574942126F60C95C8BFEC00E84233A008F7789C839A92A874474639574807164751808C67375E06ED2B9EFA07256E98DA96835A8A4B5E93EA8AEF5DE7FDEF84A35102ACF42494724BE850E43237126A41007ECC95C46ABB330530D13E80B27E90DEF0DE73030BCFB1B2311C38671E64321BA69014331E0CBC5B45FEECD45B55F62CC3E0034F2B902B81A29C642E69004F6017049B21C0DC00749EBD476180FDA80523E1E329F1AB01780578DE578006E34F46598DCF6533E1EAF0F6B84CED9A3CB7B7DB808D768133409AF0F71911065E536882FD3258A8B3603CBA25994AC8ABE6693B2B7C88290ACF23F2CF6F71E367152BCD95F9765F6EAF0B0A8A08B834D14E66991DE960761BA390C96E9E1CB172FFE7A787474B8A9310E43E61E9D774DE9BE84D98E6085B85C6220B944EFA2BC284F8332B8090A3CF627CB8D508C736D6107AF1BE2F663ACF78A386DAD4D6E5B9EFCCDB8D01C9C1E5F1C9CBD3B387D7B2041E907F11DEED706B3555517114038424D5C7711067190B76E44A20FD1491A6F37C9A06B91026B5BAED31C3785C3EA93F5B12E82A23C4937591C917EB2807C9E19EA021545651CC8437619FA78E45F16A84ED147B80A8AE2479A73C3DFA78A48AF0F3922E069ED5020366EDDF3B4AB45D9AA93CF84B6A5381AD4ADA82B1B5FC60D9C1E62A57FB81CCFEB7A09CB3F9DA240806A53CD907EABA46E1EE9375005A246BA4AA3A42C44AC36DD002DC7C7CFD72849087BC2C0D119FA786F83F00E1F1F772C589F6A8014A5AB3CC8D6DC5E45251B606D6F6FDFC6697887B8A96432F4F14E62BCFC396AAD930C30247BA7DDBE791AAD12615FEF12F571CE36691985695280C325E61A203F84EB2059211898CFD4C77D87F708E1B4E8124D703651FC788DFEB945450936122E61F0853C42C952F905B084FE17DE57C1A458CC36CD00258DB976D529060879BACD541D050BE8E37F08A21CC3A55C57A96433AC45F9182311AB4936C042798A8F2DB8CF42A619EE05BA47B188D8249B617DCB44A06F80C98F02854730AC0D8F50663136BFA637C0D0F4A9E648FCE8D0E9069CACD828E31681CDB168CB65FD0E338D72093FCDACC4F826407C3344F8BB80F077330472DB05713F6C8E29E2E708535D0921B63906885112C541B23C4FEEA3123EED24450CBE916E0B741C6D2E529ED362730C103922BB34A22F7729ABD29FBE47E517AE1D74BA3EDA75B042152DB06054B20116CAB63C4E9DA48FB188530EA24E3140C88E97CB88F03440BF844C135C10CF1CA70C4A8E049A247D8CCF418CB9DE8BB4E080E87453B4C536CF01A648C834C5C59211845825EB637D5D4705FE0D6E127CDEEEE91964B7DD56CA0610CC44E32001900D3D7F1E9A1D86BE951654AC7D6609C943F0CF36FD3E66DD76B20DE7D86654A58A92B20CC2BBE324DA04A2B829649AA92678226AD30CE811332B92B67159669867B7B728E4151454BA199AB0589A343394CF11CF6CF4A953AB70B05C9CFEE007BC4B3540BA3A490B7E9C9B34032550B0C11C0EA7036AD20C50B63940467DAA81CA07A01E73CA398B91A825EB128D7182181A2621D340988DCADA45911165DB447D9CFE114EE65C923ECD2947F2267A124165C3B788CD31138C7095E3253137DAE628079A292B63FE953C5C2BBF40E71BA35F06AB288C82448ECF9530FEC2825C2E6D545FE04A98887AE25ED3A64D7AB196472107D12499087A09BF929B2403DE0B8569B20CF24719BB00E41B7276982B4CF8BE32198678E27E43259B8832C41A01184326C3144F6C1C9D6E8026E218227CC95679B0448DB92B8DC4E618238A4D63327686579705BA3292D0200C1DC10CAEA7B80EBB490506B14D34E0D3251CBA0D07EC877FC1AC80C01BFCA4B2E1B0BF8409E50DA069D0E02082F474632BF27409641B9C9CDCB39F30B0E939732DE8DFAE0DD56F4F92CEBC91981375D91096CFC93FDE10DB16729F1AE5BC1E91CF33557900823293618AD79C9410629765626F5244E1718E02DEE0A44B365898C4B80101EC109361D8365847C0651962024DA4D30DD100F6884E3751A9A4053A45B748E075D91C13050BAE9094B9B0B73119067879846732882FB6E19DB8618AB9E6C8725453C45A1171193C88123897658C0949F55C9609667E778D8AA8F5116241D93C03D49A584ED3A5A0D062728C11C1ED87CF33B1BBAAFB0692BD90698D0B8D03586236B59A48F84C868195143EA664F4C4E7CD6341735E7C488B32E22D86A864239B936F5782B909499A58DD17ADD6A56CD8854C13FB8A5514828B83CD31417CF870C543554946189722C6A51946CDAA89AA0026634A85DAC7F478B5CAF134DDA3F3901780854C13D1829FBB3AC50C411CA83ED5C41AA3F22A13EF80980C13FB07C433CC4D923E06E1D661DE9BCD31B00BC07B662E5B8D42A63E2EBFD5C0FBCC4C429D24A08B893807416808727035E9D18C7FF20A8136CD5464834435233601FF8494CB74FA1C37504DD84EC0E8B04D36DE68C5860D08CB3391B1242493091943101A640C5753ED5AE26E652C3D82FC349B6320455521DF2FEE396E864A9EF8827F048956B05A3F31B5587F968A9F86540C4A584CC64F28653F5D79D88FB14A55E36F41BCE5A5613AE3E94BE96798A9CD4845D1B499CB3293FCC1FE3219B36B12226014BB44031C470F9A6BB4AA222988779D7C9E89660334453EB7F1F73B2FAA834DC4EA92CDB09262BB212FC088707D8E09226191330890CA30D209A13C8D424125D4A49A20B5EE219F6EFE21EC4862AE09F2228D975097FB7413B4CF98BD83D0FA7413B4AF418ED6C47B8587A332CC4499C5F646543A30196678309895995F10474B5187C165196A007F4D6FE4F67A74E69C4680952A11DCE4D99C9F5473594B3282E6B14DB6C012842536C70851CA24F07946A895B9E566930A907D8689322047FCC9D9A699A1081E6F4D9A114AB6164048928931266F8869565BCE05F0794FCDB4F3EA3E03571F9D6E84B6287394ACCAB500D76798E8A197DB107DCAD22248A4BB8FB490C977B26DFD4E37B80D03D986D8C054D1E91666B4E0EE2EE69A203FF0600F53EBF8171960732B79EC498EE26ED2FA3588CA5314075CA40F2AD9F096019C2D3667BA7B8B1955B68AA72EE5AA5BCCEBE2BF3FDDFE0BACC36D31FFD552992B6B937C02DCBC1AFDDC49B00F9D0ABE6D42AE813AD1B3D7E52926C615EFE7D4A419A06CF3E0268AF1C401AED442A699A080D7342A112C29D07973DCF15C63B98CBFD0CDCD8E0C67B7786F3E022C88E2EAC2758B32BE0CFA52A85EE2DDD19D84B7D03034CFE06D3364B54F304F51C53DD8AB14AB78E2D8C1E215C4611E6DA22428E1FB96C9A776E899E51D9D5C51983395E22031D85C06A61F88164E042EEF67B462052E41B8ACE73BC0E73BC041D4E73BC0516D59FF1BDD680987FE91D915C0D926E3435876892638EFA207F1DAA9497C5655EB218EA20C16F83A2302D93D7D850C57CFCE126260059670C759D84586C228200F5F3C33B1CF4CEC3313FBCCC46AE03D33B1CF4CEC33133B80F3CCC43E33B1FA887332B112DD3E14666D61115F0DD781A8824A36C1826FA7639B6BE998DF2BEA14934B65708C328B31CAC8D929E2E44641A017B0C5C5C2C6E06291C1239DD98C7446CE051EA84E334111E62B339D2FC0D5B64B34C221FBA48053279AE060492A815DDAF93C13D46AD3E0E19A443D3116100B775C9055CBA0738BB0C200B3EFB24E3AB4B6B1C82861D6352499024A63AED4D5C75137F8F0D177BFEC06FC528C3D52780D153F2E50FECE10A1E44D6113E2832034880EAE26675D2321E8589B66A0E3225195D6A439A0658F906B837C1D00EF090AB936C8F240516CB60DB66801C3E799683478E1E2D4D899EA2A101F20E9534DC44642B0A2DCD8A69A202DAAA7114ED24C9066E91C03C1091585C0EC76892666AB218AEE4523303ADDC4B8B4EA8BE82CCC6498E2410F71B139A6889227AF844C1B5CE0F92B21D314979F9D3ED514A9B2A987E1BA2C8343342AF9BE36493B734861428E32334354F1988241340E2A5945C5728CC4480F7DEAD4E6A75E83587CCC4220864593BA33045337C999AF8140B4381BB8A2BF6195EBD283983C9BCD8A3F759AC1E9EBD1F11B30CD67324CCE5F2CC8113E5A38CBFB7413B4CBF41E4F228FD5A61AF105515902505DB2D102FB299EA5C3840B467335DD70AED222121DB9FAD45DDA70DC771BBBADC6689F71A62EF2FEBCC0DB1B0595BDDC16BC57789364405F81AB43DE629D66C7719CFE101CA1E88C5DA2AF4B2FF19DE5407AD426ADAC20BAA60E407B54CED33F087E96CDFBD232BCD8CFB0893788EE8B4C02A4B7C8A49547263DE30875F34E949F59B29F22A3F9F117FDAFAE241E814C86C9F224CB10E0D3998C9D99FBAB342F035795350CA231F3B28AF2C125E5F979EF534D0CBA8A925C6111CF7671B58BB956C8DFA4A846671055EFEF5244A333C9EF99BF48B77988806164324CF1BE415846E35657F93B8463345AFAF762332DE2CF2846195E04CEECAC1C486331AB2A4B87B6ABC3D30E9B6340DAC9920FE7D0244DCE7A7AD2424DC9C4CEF5E6EFDA39343304A141B4703595982B6C726BD3D0CC1E4903255B20407C973AB50A008861D2A5EE14B17908A22C83D1243AB360CA6D0D90F89A74135F0BF720C13B15E7C1CBB2FCEF182DC261097979A75D86A3B9884CDF6B6FAB40C462F1D29E27BAF3F88AFC94343CEBEDB5870D5C0EA4417DAACAD28DAEAB23B9CD36DEC877ED46DBFCAE7E26227A5F052E8C2FD2952311C981348848555936C4B8303FBE4D9201D98421A1111E874A36E0253CC7CC3ACF8E97CB1CF1D65254B281516CBA122FD8BA44231C715BEC124D6C8336A828830D1793934ADE9905F2FB360AEF887D3DCECF1F1D17891A4C63A10C014889C9C9A6DA3769FB88E77A95720BA34AD0AFFFFB115BFD7723E7B2DF5F72B55F3E33271C7352BD12E6CC9980285A6C89A4A6FCC4AE2A884776973CE37AF1A0866B9F6D131F87A333F4F1384D9C911A8ED3C1ED9202EE0A25415C3EBAF34372209DEB204565E986D8D511AE85989C79B8A3E3E5264A44DD1A956CE6077096F037556DA219CEA20C7221DE42976C701CD5630C0EBCD9320D0ADEA4A14D9B7891B499276952061166D0F922DDD79B94EE77D12610BA26D130D2258A8BBEDE225CA34D50F518EF3D61A5755BA277515E948423BD090A5417D9DFC3C3731F2D518E8FC8C7020B6207A4C0C1E29FF1491C2122DDB5052E8324BAC5DCE3E7F40E256FF65FBE38C247F2711C0505B1618F6FF7F71E367152BC0AB745996E822449EB70DE6FF6D76599BD3A3C2CAA2F16079B28CCD322BD2D0FC27473182CD3438CF5C7C3A3A343B4DC1CF2D51B582D94177F6D518A62C9686BA8BD855D7BECF27FFD1B7AE467B8A59F6B742B2EDB436EB6F9FAAFE52B9EB4E5CDFE4DB4AA82E9561B542DA59568791594F80C4B081B89AA86EFEF7DDCC631B92F7DB37F1BC4E20B22C287B6E53AAD3D96EA0F159B208EAB4F19225D044579926EB238AA9D9D6BB8256E665905ECB5805B60E9AA9ADA1ACBA655F59656D74FEE833CC407FFFEDE65F0705105C0C7F4F9E73FD3A0652EFA06F298574151FC48F36E6EAC60E91D4149801DAB6243820A3E67980899CA2393A18CDEF965FEAABA3A7DB3FF5F55C5577BE7DFBE77757FD9FB94E30DE8D5DE8BBDFF6DD182F24FA72858BA101BC1F88DE87F1D31AAC0D985134A8E4FBCAF519210C72B7B98B7417887CF853B278C285DE541B67EE497E1BF6C82877F355D7B6FB7B7B7DDD35C2DA99837AA7172AB01CA2879B4E999B8DB596F9EA7D12AA136E11C05B131C6D9262DA3304D0A0FE373F680672859210F50EFF016E2B889BF0B3651FC489E95C67C858F26E5986559FAC36B3D1CDDC8E97D1A0BDB9F29449E6E337FFDA29C2CDDBA4679553A02A13CC5C79297CE61A82620817B9BBE658E33F7217359201F320F03F26B7AE3653C5A1CE731F1D21A3F4D69B43CFC2EAFC19D54359D3893DAB6CAF180A9CDAB9C41887B803B7752E37C8E6A5F7F079CE661C8F3E43E2A7D9C5597E4D1C5E3687391F63C8F158ED36AA665164F224B755DF11E955F32976E5D072BD43CEAE2B49AAAE7B61C316A45BDDBDEB020577895930AD32B3B287788B20AA2E1D6A5CF418CF9D18BB4402E4DA95116DB3C67381B7B282C8AB8807C5D4705FE6DBCC2CD856CC07E474FD2B611B0FB23659B4498698B2AF1F93622236E2A8BC865758D438AAAED745451164216476557DBA00DDA336C3DB152ABA7E1F9550C87A93C4F62F5DC1D27D126A085296B3C2246430C8D29D5E183DC5FA3085A1B86C00394A7FED5CF8DB902F9D137600931FDE161A0AF4ED2C2F9F46C6376BAAA3EB6B91FF2F1443A5DB44EB7D1696082D8D3307D88CAFA66D8AD59BD0D9DABBADF9B70D6856877C36A608E97F778D8B739CABDB4B045C55CB85FC42AC47314243E3117E45662E30B33A337096B3A19E502A67EA9D79E9FBC26BA4D67E9A17DE0D7DF315F2161662C09DD378C0ACBC796F139C857A8F432643594974679C068BC186ACED075B81B30BA5D9A58FA424ABAB949AD6E014945AB1BC0B622B5D4BC5FFD79E31F3D31001F22778C1D16843E666113E7C35A24E2206C280B8018F786B9FFA0F5B4B0104E82F235A5EAF9B989CC8DBEECE4EEA1B936DDA12AFF1572AD16E5BDF6C9F6D4A945792F02610DD539E3B981BD0D8A283CCE51E00AF4915C43232FFC42D5266F626F85E6AF5D7EF806F675325730FA6532572CE1F13047ED05F36C98ABD2807D30CC0F1A2B873AA0B16F7AB9C2318F62F901F3B465088F82395BC140EF80ED943EC70BF5F26F7EB932ACBE2C19CE8B0F58E48F7A330D4B3302F26E92BD70EE47B3C43FCBE57CF54DBFC8E50CF6F0C1698830C0A51B40CD1EF990A2C750F07C4C8F57AB1CCFDE3D3A0F7BC9D0EECE3A74EE22C1F03154AD870F75A36077D78CDCD950C219F32CAD35410B6FAE39E2996C20DA32C969CE07D6D19346483D1B11B7ADC78F8577054AE35E6F4F50A4A5ACD6D1EFD5828634DA5676B594229FB6B7D4AA6BCBDAA0B373B9EA1834140C0EBE04627408BD256027867B11BE2B51CB13BF5A3F837571EFCC5DF8B9CEF52BF97930D37D161F77447CF424B3ECB424FA2464474F2605154CF3EE9CFD01BB8392EC1966203382E6C31C93795DDB1DCAA7841DF919AEDECADF01E21AAD6AE6D1C79DD9B90F6FA1F3A23A7EDC419262BB2155DC70083B9BB9C310B50A79B5C205A3B550FF74F30F6A23B1C35AA4F1D2BD579F3123E58EF235C8D19A18CB3BC1606E74B1BDF1215913285F38789F8E96B48CEEE2BEF26B7AE3C9BEC9A7AD54A51DF3B4D5EEBAA6AD66FA9DADA15A200FD203C6F27906376CECF166933AB70C6FE21ECE280273E9058684A27245F1D010BF67EE181A53C624CE72C3BABACF7CAD3B0CB528F3AA3BEE4AD3E536449FB2B408129FFB4CE584543FA8E06753AD007DCC436758E869875E9058F38E103E54CE8BCCC711ED03E36B10B5EF13FAD07C7B9A27372DBA9112B07DAAFC097A3AB9ABBBDF6280A59BC3140FE1A232766B0911BC8F9325A9D737E297BDF3E24B35D0AFF6DE9121B150A69F62425CB91F76C4BDE3268A31093A3B4812C2C5AB169588E6CEA108433AC33EFF75451D54DE717C69C58B99FD1F4038473E0807DCA02D1BF4D247833CDD1EF457F6D064695D0E39230C30F95ADBCD903CA40FF2292353A9B7B76BA16ADE886861E9DD65684135570F833A6513308D7B1113B8214952074BFB56C4006CF852440F4CE74E4417498FC5D242D3BC6CD0C2D2BF10B180F3D2444DAD850E949EAE5F17C9DF8C6A68FAB560AE8E5C21CE8BB34DD68704E314AC7A08EFA207A9525E0741534FA80DE56F9E74B5847A58BAFA38BD43DE79EA35854A6D287FC3DE1B6689BA0D9DFA0B0D77684D1C3F73BF88B971B60772E70F17193B3A96734E60F23B77107684AD71FC10F222231BBD33483F49D61043F6C99A28643F7446599469A261CDAF0756ED15AEFC55548479B4214FC0F6F648A092FBE8E55F7C69B46869E4096AB4BC18BCFB508B3A5BA87C45414E8AB7EA45D52069680444B8317C152F83C8CA0B96D4B3A19CB61E7B8E7AB70A26BE816BB2F1B20A53275FC375CD24D1F1C2DDF13CA9677A406BAD1E0BE1A2D33DA51849EB10E867FFBC0AE81089BEEE0ACF8B4F192634476B9D45151AEE24CD64228ADED52A2A0A8AE3F1D5C56B14A2E8DE52A5DE57760B86560F900F53E11ACA4F98E11ACB5B68DF1ECE4B80DF1ACEEE26A4A9EA61D22ACB030FF1C33E4765EC48DBDAC758FDA6A1CD41267B0D71F828EB6BF20335AE938BFFE85713BAA8346F6A535C969E734A556F1C068860DBB140F00BE13A4C10300EA6578B5110A72BE745EAD5C8DB8F55C17981E52852DEF18CBE4CEFA364E578CE4765E908F21C3DBB32FBCA4297D033CE5BCF555A44B4FD9ADE266AB28B586E2196FB8787F3B97E76B506C1A7F34D9404F9A3D5BB2497DBA2B715DF15A340F2F2F7711CA73F4689998CA7C021C64F5FDB72FAA9DA0E26253BB3D13EEF9115C8F8EEB33BB257925F3EB74C5949F21D8F1BEC54F4D836DBEEFB75ED9178E54AD36A376D0E93211B10DF8256F329E838B23A18AF52324D366CB1F69C5CA5791958A970EB9A3633D2D7A40E1FDF73718A8A92DCA4101B75EBE5C88338AD4C0ACCFDC4A0C0DC4F0E3F27F922DDE621B21F6CAABE9B1AAAC2711FE21AC77D74E9DB25DFCBF7338A518697931D1FD9D7B659C66CEDB1F7D68A5E7C305A739ECC53A9AF76874DD57F63646D176188D4B3A1DEB6DE8847D0BC138E92AD8F7BEDB1646BF3B61991926DB49EB6AE3549357547242B2F574FF36BF3FD787088AB5887D3A86AB931191E6276F8589E560600462BC9FEF1A7B6B2ED5AE2A29B8FB298E623A0C90390EF0A111BDECADAEEE47D6D87DB5966377FBEA1D5301D91DC69EB998E4463A99EEAB98A2FD2950D29E16A3634D4541BD9562D0C09ED58597D74759D267DF4E713B5ECB2C873A439EACD857CF1AA7812E99B2E8FB0F4CEE9A426241EBD45196CBA601D9A5673DAABA77A01378E8A12E7E48F4FD04A78375EF8F41393E52A2D9C317E57BAD9E841BC7467DE41F7EB1DD2AE3531A7EDB88FAAAA1DEBD1551DF7E8D89155E1C51AA10D0FEE41A472D7938DA624BB424910978F966C4C5FDBEAD686A93DA248363F4773BCDC4489B75B3C62CF7E962C0D0F671867510679E98CD44CA5EB42B94641D15FF3FB35D13D2E8A348CAA69A605E6EFCAC7C6F128EF5DA7715BB86DD902C5B7074DCAE5362EA32C8E42FCCD37FB4742177B08EE4B34169FC582FE9B008A9720CAC96A086212A7B3CC033CDCE27A8D122C000531D305AE14AC6891BDB24D86B783E5734E5146ECB59312EEAFF3873B7C6E9F191A8CD787D4DC6B9104F0B4A35F4AA83EC010409DF273CCBB387C3B3ADD4445F11D7889A29FA94A57444F549DA03FD5153A0D50278C32D1A2624B32EECE532C8E99E44BBD0669B6096EFD15B93061D229EF4B803B343F772F0E0E54F3CFC477E309A9CF18851E247D954C15C3BC3BD0863CA29DE4C36204B8596885F65B169C5D951B84F1FC1EF1E3F2FA5352C752DB3B0EC9F7716B8222ACAE6178AE1A7F5A4AB9B4E73543BC4CC6687B8FD1AC3B5299DCCB5CF23DC81B7ABE43871EAE62BCD367CEDD67BAD3C89CF6E63D9588CFF977E016CA70E2074E9ECA939EC6A8139EF85C8BF101245FE23DD0679AE9ECFB30C7412CCFD9891238442DB9528F61F134E582B9BC641E64F6F11672A4DBF7269A6EA9238EB7F9A6BEC0A174C94F7CC66563B8BB532EBAECFA9C6E82CE2154494F7F9A8571DBCD292663DFCCF488D3DC7C8547E9929FFE74B75D7922535EE986E4EC79EB7E04CC9721AF36A59608749A92CF0129EB38F1DADA22E5E7265FE9E228D9CDFD8EAEFA8989C07CF5CF4D0ADF4157366F1B7E834E23B4494F7CA387C64DF231D1A96EE6D93E7A9EEE11A79BF1E89B6DA6A53E6EDE269BFA028D42273FF149978DA1E907A799722C667C074372598859AA796FBE4183B44963CDB78104054687329A75680C2DBE37C54DFE3ACDBE4BFCB6A8BBFC35C777D709FA13DE7D81078114B6BEEEF305C746C914C09E8C46130E8FA0F1F7A69CF0013B1E1F330E9880F4A93FC59C1BD88CCC39E9CDD60EB8FA7ADED8B5C8E6896EEADAB435F396DE4CB61EF7E63EE533F270934EBF3927372711D41CC877A9279F052F36CCD009273C9D3C0A1118305AB280B1168C9DF649AFFAE654261403FCBCB3EDC4D4BCFC7477E806C4B503F67B3A6BDDD3644FBFCEA79E74EDEFEDC2C40F8A6FCED33E87F836DD941B096F334F38E6299A2B7FB541765F8E99333A79676FDEA846EACC485F7C3A7BED810F4E4B081AA634C6E4B08B8635F3D185A9A1CD8E5187CAEAC637654C6A83332B45E84A9F5D08F5B929A1FF53ADFBF34A12FC4761C4317582F3118972C0AD3F3F997F9F2EBD78F0F49B9F4AA6F6FEB3A18DB9FD006BB2D0B83C70278899AE0F262602C32B8499A7BF3330D7750637B4B8DF65A7F0797C042D5CC3E77710EC437E69F874F8F522A5A28DD17074F24F441DB2D86A3B4B19FD06729EDC63C854886935CADEF1EC5BFCA4680378DA7614B298D2F16F1E32D07602EC1FB0DC81B9979B963E4FFE18934F3F3B3BF3F42BE31D8E4207DC17693C3EEB27A20DD538DB7D7A523281C3038E421FEDA7D8DBCE26ED27A20870487796149A68731AF245539299BF2E6DD77DC7DB86EA4C0A15BC6F321F72E537272403699069EF74309780393D25180A97BB420AD2409DDE4981FA12E3AC4425FF24A4201BD39D2085B32A8A25AE53E21A286F8FA87489DE4579519210E2374121DABF915A0B54B26DDEDF3BEB8262F294B108D76813BCD95F562109EBB09A5DA640252C3C7550081FA0F2A04F50D9BA1F69F4A0F22F3505949F6BCA0C7C53F629C517F4809B408F621FEA74B0E975D600B0709B247C4228017D4C28A4FD59E517873E36F89DDA2C47F8429D0C61D73903A8B59997805A2743A8758E066AAF7E03D1FB6CD957FA124314C5047713098BC906E98B2931F0B55A9D207CA54E86D0EB9C01D4D6B056C06D3320E4366FB0C5B5A106D0E63A036E759D378C0D034B517520E50B8ACE947C407741D12106A0EF749992EF74F97ADF917F44F98541F8D6875A406F3320F0366F009B76F510F0E94CE81B74FED061B306F7B63A193C6AD61A7B5B6FC60A22CBF7B83E57E30BD283B2CF937D43EF80A14DB025DB83BC2B74FEC077685147F80E9D097D87CE1FF80EAF7D13BEC51780BEC797191CC346B1030C6093038F5E9339B40629D1405C875426B816A97CEE3B1467CC7166FC45FD1E5594E6D49417FA8CD4C3D031FE9A849F83952762E50176F390ED9A7EB76B5650DA5B28F2B66B276986B5EE1BC88F9A77A98FBE0D7448129A9B691ABD29542D83163A5383E622AB1A209768D715599C6949E7B4C252CB747510A5293B0EB1A4DD9049594DF3815007510686C220EAB27B87D8E104F8E67A4455FCB02599B3F17E65F4AE880AEC48F853CD7F1FDA16E8A324EEAD63D76889A4AA014A1C365DA163B782DD910677E51A9871ED13B61C97756ED9355A5480FB26770E70E89C28DEB4D564828B75F72A014EDA35C0CADDAD5B9490D956814448ABEED0F127E12E492354BA768B1312DB6A32F1CFBA7B4DAC4579EFA0608C5063A196CEC21400D10407E70E0ACAE7A18B13CD631B314FB2EAE040610EE4C90AFF55158968EFD29D23757FC4C71C76B743B45202EE933C1A8443B744454A554DA122B1EA1E15E50BEE9D2C0C98DD56CED4623598552D897ED242F663625941D29F3CD8152BFFADB99D0E5224093504964CAA2072E89A5C8E57C475F2D039402496AAA6AC89B26E899424A1003CCE04A9351CF65D1ADC4954A17A3C746F8A2D458C3503747520208DDD2601D41256A142B76929A4CAF74E69C01547D96DCC5D938F1DA2EE9677B174B259531D0C8A301A8EDD1BFB60E0E341009D53868C601A2B5E5257CD955F3F4F221FC0910ED41DD592C9DDBA3B85842EBAF10F765B2DAF3B777934E95DE1A7AEEEF38023ADB7CE4B0C473808BFF7181A23A1E1AAED7CB73147D7D5CCAECA11D9B9BBE3F3BB524F5A48F3AFE575EBED0664CACB3AD053141882618F526FDD17AFB7ABCA8A8B6B97D9EFDD2095132FF1967C7AB75EAC8F9FB2CF90FB92A7EE8E7513C2BBB10DF64FA512DBED0EF28615AA9EAA9D8D3C75193617A900068C405C86A1B3FC50F55FE25AE3A9E39CCD4A233CC10629E65D151D6D80AE0E78E330CDE50CADABE64AACA827BEEC039C49147D95BB4778E8EC148710E030A1E8AEDC05C0437745BBA85AA32FB7789277973CFC4C103A87802EEFF5616D56D524E09FF8440D56E8325DA2B8A8525F1F5EE3B6461B54FF3A4545B4EA215E63CC0455EF4EF7A06D99F3E4366D5D21B816B545DAEC564188CA601994C1715E46B798887176888A224A7077FF16C45B5CE46C738396E7C9A76D996D4BDC65B4B989990D8CF853A8BEFFFA5068F3EBDA60A3F0D105DCCC0877017D4ADE6EA378D9B5FB5D10171CBF2083208E1A355D577359E2FFA3D56387F4314D34819AE1EBFC4B3EA34D1663B0E253B208EE914DDBBE14E802AD82F011A7DF474BB2CBC8408627821DF6D7A751B0CA834DD160F4F5F14F4CC3CBCDC37FFC7F98CBB0A04B130300 , N'6.1.3-40302')
END

