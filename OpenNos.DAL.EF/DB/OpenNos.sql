-- ========================================== --
-- Current Migration: 201612021830097_Aphrodite
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

IF @CurrentMigration < '201612021830097_Aphrodite'
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
        [Biography] [nvarchar](255),
        [BuffBlocked] [bit] NOT NULL,
        [Class] [tinyint] NOT NULL,
        [Compliment] [smallint] NOT NULL,
        [Dignity] [real] NOT NULL,
        [EmoticonsBlocked] [bit] NOT NULL,
        [ExchangeBlocked] [bit] NOT NULL,
        [Faction] [int] NOT NULL,
        [FamilyCharacterId] [bigint],
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
        [LastLogin] [datetime] NOT NULL,
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
    CREATE INDEX [IX_FamilyCharacterId] ON [dbo].[Character]([FamilyCharacterId])
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
        [CriticalChance] [tinyint] NOT NULL,
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
        [NoticeRange] [tinyint] NOT NULL,
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
        [IsHolder] [bit] NOT NULL,
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
        [RespawnMapTypeId] [bigint],
        [ReturnMapTypeId] [bigint],
        [MapTypeName] [nvarchar](max),
        [PotionDelay] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.MapType] PRIMARY KEY ([MapTypeId])
    )
    CREATE INDEX [IX_RespawnMapTypeId] ON [dbo].[MapType]([RespawnMapTypeId])
    CREATE INDEX [IX_ReturnMapTypeId] ON [dbo].[MapType]([ReturnMapTypeId])
    CREATE TABLE [dbo].[RespawnMapType] (
        [RespawnMapTypeId] [bigint] NOT NULL,
        [DefaultMapId] [smallint] NOT NULL,
        [DefaultX] [smallint] NOT NULL,
        [DefaultY] [smallint] NOT NULL,
        [Name] [nvarchar](255),
        CONSTRAINT [PK_dbo.RespawnMapType] PRIMARY KEY ([RespawnMapTypeId])
    )
    CREATE INDEX [IX_DefaultMapId] ON [dbo].[RespawnMapType]([DefaultMapId])
    CREATE TABLE [dbo].[Respawn] (
        [RespawnId] [bigint] NOT NULL IDENTITY,
        [CharacterId] [bigint] NOT NULL,
        [MapId] [smallint] NOT NULL,
        [RespawnMapTypeId] [bigint] NOT NULL,
        [X] [smallint] NOT NULL,
        [Y] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.Respawn] PRIMARY KEY ([RespawnId])
    )
    CREATE INDEX [IX_CharacterId] ON [dbo].[Respawn]([CharacterId])
    CREATE INDEX [IX_MapId] ON [dbo].[Respawn]([MapId])
    CREATE INDEX [IX_RespawnMapTypeId] ON [dbo].[Respawn]([RespawnMapTypeId])
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
    CREATE TABLE [dbo].[FamilyCharacter] (
        [FamilyCharacterId] [bigint] NOT NULL IDENTITY,
        [Authority] [tinyint] NOT NULL,
        [DailyMessage] [nvarchar](255),
        [Experience] [int] NOT NULL,
        [FamilyId] [bigint] NOT NULL,
        [JoinDate] [datetime] NOT NULL,
        [Rank] [tinyint] NOT NULL,
        CONSTRAINT [PK_dbo.FamilyCharacter] PRIMARY KEY ([FamilyCharacterId])
    )
    CREATE INDEX [IX_FamilyId] ON [dbo].[FamilyCharacter]([FamilyId])
    CREATE TABLE [dbo].[Family] (
        [FamilyId] [bigint] NOT NULL IDENTITY,
        [FamilyExperience] [int] NOT NULL,
        [FamilyLevel] [tinyint] NOT NULL,
        [FamilyMessage] [nvarchar](255),
        [MaxSize] [tinyint] NOT NULL,
        [Name] [nvarchar](255),
        [Size] [tinyint] NOT NULL,
        CONSTRAINT [PK_dbo.Family] PRIMARY KEY ([FamilyId])
    )
    CREATE TABLE [dbo].[FamilyLog] (
        [FamilyLogId] [bigint] NOT NULL IDENTITY,
        [FamilyId] [bigint] NOT NULL,
        CONSTRAINT [PK_dbo.FamilyLog] PRIMARY KEY ([FamilyLogId])
    )
    CREATE INDEX [IX_FamilyId] ON [dbo].[FamilyLog]([FamilyId])
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
    ALTER TABLE [dbo].[Character] ADD CONSTRAINT [FK_dbo.Character_dbo.FamilyCharacter_FamilyCharacterId] FOREIGN KEY ([FamilyCharacterId]) REFERENCES [dbo].[FamilyCharacter] ([FamilyCharacterId])
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
    ALTER TABLE [dbo].[MapType] ADD CONSTRAINT [FK_dbo.MapType_dbo.RespawnMapType_RespawnMapTypeId] FOREIGN KEY ([RespawnMapTypeId]) REFERENCES [dbo].[RespawnMapType] ([RespawnMapTypeId])
    ALTER TABLE [dbo].[MapType] ADD CONSTRAINT [FK_dbo.MapType_dbo.RespawnMapType_ReturnMapTypeId] FOREIGN KEY ([ReturnMapTypeId]) REFERENCES [dbo].[RespawnMapType] ([RespawnMapTypeId])
    ALTER TABLE [dbo].[RespawnMapType] ADD CONSTRAINT [FK_dbo.RespawnMapType_dbo.Map_DefaultMapId] FOREIGN KEY ([DefaultMapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[Respawn] ADD CONSTRAINT [FK_dbo.Respawn_dbo.Map_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Map] ([MapId])
    ALTER TABLE [dbo].[Respawn] ADD CONSTRAINT [FK_dbo.Respawn_dbo.RespawnMapType_RespawnMapTypeId] FOREIGN KEY ([RespawnMapTypeId]) REFERENCES [dbo].[RespawnMapType] ([RespawnMapTypeId])
    ALTER TABLE [dbo].[Respawn] ADD CONSTRAINT [FK_dbo.Respawn_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
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
    ALTER TABLE [dbo].[FamilyCharacter] ADD CONSTRAINT [FK_dbo.FamilyCharacter_dbo.Family_FamilyId] FOREIGN KEY ([FamilyId]) REFERENCES [dbo].[Family] ([FamilyId])
    ALTER TABLE [dbo].[FamilyLog] ADD CONSTRAINT [FK_dbo.FamilyLog_dbo.Family_FamilyId] FOREIGN KEY ([FamilyId]) REFERENCES [dbo].[Family] ([FamilyId])
    ALTER TABLE [dbo].[GeneralLog] ADD CONSTRAINT [FK_dbo.GeneralLog_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[GeneralLog] ADD CONSTRAINT [FK_dbo.GeneralLog_dbo.Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([AccountId])
    ALTER TABLE [dbo].[QuicklistEntry] ADD CONSTRAINT [FK_dbo.QuicklistEntry_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId])
    ALTER TABLE [dbo].[PenaltyLog] ADD CONSTRAINT [FK_dbo.PenaltyLog_dbo.Account_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([AccountId])
    CREATE TABLE [dbo].[__MigrationHistory] (
        [MigrationId] [nvarchar](150) NOT NULL,
        [ContextKey] [nvarchar](300) NOT NULL,
        [Model] [varbinary](max) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])
    )
    INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES (N'201612021830097_Aphrodite', N'OpenNos.DAL.EF.Migrations.Configuration',  0x1F8B0800000000000400ED7DDD72DCB892E6FD46EC3B2874353371C6B2DCEDEE76873D13B224B7D5C7B2D52AF7719F2B07CD824A1CB1481E92A5B667639F6C2FF691F61516E02F7E12FF2059F27174B4ED02901F81442291480089FFF77FFEEFF3FFFCBC4D0FEE51592579F6E2F0F8D1E3C30394C5F93AC9362F0E77F5CDBFFF74F89FFFF13FFFC7F3F3F5F6F3C1DFFA72DF91729832AB5E1CDED675F1F3D15115DFA26D543DDA26719957F94DFD28CEB747D13A3F7AF2F8F1B3A3E3E32384210E31D6C1C1F3EB5D56275BD4FCC03F4FF32C4645BD8BD2CB7C8DD2AA4BC739AB06F5E06DB4455511C5E8C5E1BB02656FF3EAD1D9C99B47E7AF1E9DBD3C3C38499308576485D29BC38328CBF23AAA71357FFEBD42ABBACCB3CDAAC00951FAFE4B8170B99B28AD5057FD9FC7E2A62D79FC84B4E46824ECA1E25D55E75B4BC0E3EF3AD61CF1E44E0C3E1C588799778E995C7F21AD6E18F8E2F0248E73CCF9C303FE5B3F9FA6252927B0B7A3F8CB019BFE974114B0C490FFFE7270BA4BEB5D895E6468579751FA9783ABDDA73489FF8ABEBCCFEF50F622DBA5295D3B5C3F9CC724E0A4AB322F50597FB946376C9D2FD68707472CF9114F3F508BA46DE32EB2FA87EF0F0FDEE2AA449F52348802C588559D97E81794A132AAD1FA2AAA6B54E29EBC58A386994225F84FEEEADBBC6C0AB69F2442F968482569C0F7D5986FA2AA3ECDB7458A470CE9BC16F80CD7EF3D4E71825BA1AA6A446CE0CB774FAC81C89F4333EB126B8CC383CBE8F31B946DEADB17874F9E3E3D3C78957C46EB3EA543FD3D4BB082C14475B9D37EE42AAAAA3FF372EDF521B8396FA3FB64D3F43AF7CDD3DBA88C62DCF18707D7286D4A54B749D177662B551FA952AFCA7C7B9DA7A3C88D991F57F9AE8C099F725989F751B941B579F55AE94CDFE41B65FDE8626205C75C690DA922B655BC425994D65F7455A48B89551C73A555A48A40557C7E34AA40A562A4FAD258350E340B2AC7A10E2EEA91219E4D419AEA641D4CFDFD198AD67E1A8CA0FC35C1ACF746B9CA93ACAE3C714A2CCD1F922C2362E803F4328AEFB0B174E78992E49B322A6EBF4CAEE35FEE6E6E5EA6797C8786FE7C99E3711E65D6953E4DF17C31807CA9EDA74771A6C5EC3BFEC11AE72CD964B41D805997DAD7E67C9BD798935915883FE79FE3DB28DBA04070AFB0FEF0B6235E45DB24FD22D74626D4D7E81F3B54D5A19A5526285B87C5C42A733D0EEC661A6C939C6CC25FF2D45383FE52E6BB226C135F4749799AA739DBCA21D5A9A1847A557F499180D9A4BA61A232C7E216ACD918EE0DBA47A997DE21287F147E7DFABAF01B89AF8B402CF935FFE4CF911EC4972B64C1832DC4645053EE4B27EF360569D06554304AD26176C2107F7823FCDD17A1C2EA3E84E5D422BD4FB0E8D69E484996A451B6BEC8EE933AD42C7999EF2A74926CDFE4A34DE68CE539C4BD17ED465FF96D87FBE21754FF5EF8B6F73ADAA04644FC86CC352A769E10AB34AFBDC6FEAA3859AF9366B5CC35C8A11F57450810BCA6435E6D7A1FA5D8527E9357C8AF262DCE6A57968C99E403F621F1B44B3FDC2615FEEDAA04A4CE91C10BEBEF5CEA9D1E72E752EF39B1F67DADEE9A853150C7119C2F3C56555646F0E4480BDABA9CB85584A6E64269A8EA5C2145DDF992815D7A94270EAC32942FC80658C8563CF0BC8887565E7ED1F0972A077176C856F0742C63CBCDCB28D1496E5B04AA1AC951D4AAC976A9D0B1418D8EE5553AD6D4E9D8A1520558259C0E4B19932188179B6B2B578DC19026557D9ED55AE1E20B433C63CB2898C715B4E5E235D997FC33D3D478280555B5CB54D4B12F11C6C7DD296B7B477743B8A0B7DBC5C93D2E947ED9256B27D7F6DB3CD37B0DCD7DE91A938CF0F86F6F775BCBB595E36696D75CCEAB00EDA46F5A67B9EDD1E2C9EB0AE50BF5040B4175341E5BB6436AE991444999ED803216D03003EBA4AEA3F8EE244BB611E76976F038908D065FCFC96954D581AA43A0CE6F6E50ECB9D540704234ABF58979A104D87EC9D375FEA72F67AF4EF3CA6FB57E166DA38D273BCE76650031092122E729A2B7B49C38D261446908D6BC4EEAD677EF51A18B1A6D1DA6CA699CBAC473B81DEBE204D4619CACC9CA07EBFDD2BF6E3D6419DF0684BB8C36499C445930C01539E3B30D0258D043DF4D24E639D95426B1A7D7EC9A6CE5FAF924519C67EBA8FCE26A7742D601B6ACB2D8533F3440DE1AA2B5FCFCD9D4E2F857C717E0F76253466BD4999B3EFCED90E81A055B690436DFF975B1D2C637AE6CBEFD94ABEAD8E60B5523C9B21A3579B615795BC497F8FB066C134AF295E30A48AAC997B2ADF0EA362F7455A5CAF0951CB224D51BF3FDBC1D6DFF993B3948F9254FF291EF3B9DE2EB09D51349C0137C61163F21AC5A6C44069865427A5BB4AA4FA556247E0A46E5380D064173180F0B8E72C101C2D5C465A80010731D7B1D3FED6F635D539BB67B22F663F340D91FB3559398BC943032144543FB1AAD675D78149B54D3613C3B0DE5BD18C56EDE47BB7114C805B9257BEAE4A8645252E7005CECF7D69BE9EF256B713A1BDE0BE9655425F14989227F9430AEBB06CA7FC5D6C00458279DA67985CED00DF25ED136971CB3BAF4D6DEA765522771C42DB3DDBCAD1D94FF94D2FA042FA3CFB413CC0B8AF5A7B9429577D7A84AAADADF23D109C159BEF6760BB7482186EF59D7B42012CA8105686940B7B3BF88BEC2EA3B94308439F37D51BDCEAB3A190FB77B9CB7FEE3CACF9718C00D9E6C6EEB500C6E3CCC41A41AEBA4D79EBCC11097BE10ADD542FBDBDAD33674FA3EBAA9DFE6279B4D897BF51E5DC4E3A2DF5552DF92FB4CC87F7EBF8E3CA73E02E0ED8FED0EE6701BA74EC77991A76949EC61DE46751B301FB0A62D438D636BB5245DA89DE19FBAD5655B065C52922CC1E5C8E73B9CA1335CF7D225C1FA8D0554B5A44A39D4150319D4B32925AB23FEA5A91F29318513DCDB89A0A8B6894FDC7875DE0AA1F1BA9C145F70454E3EEFE24CEBE97C57E0C66EE76D7B68DE47CB923A0B9B930E38610E1EE0B142C805261A1912C0E78DCF70E3DA83238C64082A7448141C5B638EADCFAD6BB9ECF831C96B802BEEF8F198011D3FA6726D2B64EEBFD4CD300A4FA59455C69AA5ED3963CD428A2FA859DC3C7CF3F9F51A17518845F8294AD33C7B73EFB77C0A70762EA0AF8ABAC5ED788E2FB0B7EBCD2EBEBBF6BDAEF6CFE0EF0AE173D953BFD99E3BBB429CD36C30FE16A53B4F1365AF3C6FE778595A1028FA0E6D33B30E394E0E10E2D10BD1D0A09EC12400C398400E4EF4D768D34E8ADE07352E42C59EB9A89A69250C5056EDB684C41F8B9865451828E2154E627F9C3E0AC1BB4FFF45691477BC559EAEC3B4F03D369602F10AD769BCE8ED8C834DDCD5EE93B7238FE00401C1AA3B5973FE40979825C41BFF6BFE29C4A9F260C7D31B077F087DBBBF3B05AD29EF7700BD47F15D1060A060337067A89E6CB7B95F9DB0FEF69D9C08C6A53F4671EB09E15B858073EC2C3B39FC850307B574755F041969180737B3698BE7FECB7A17A377455E45593085D2C48A693A3584D26CD002B07EB8A9114201AFD0673F7AEFEDAA55E13DDD7A037C8892FA0CA5D1173F7969F6CB42F48ADFC69BFDC699C6DFCBEF97889E601B8FF345D6B6AA9257852BC6D589C9852BC716091636A489F9D1FA5E6977749708B8A2FB1C5B37F4358A13895BBCC1EBF339CEB4C9304BBA3CFBB016844CBD534097016BC4F000A815C33DD39A91FB21EA7A8D25B85AF519709D865CAFCD3F5A042D5DF53DD9B7081BD06742ECFFBDC4106BD740B3C1427C9C610DBFF13418C9CDF24F498A652F401838227C781E447D54CC166B8C9239C766E875547A9A6DB46FD10DC1DBA070DA4C926A3A565AA5FAAE571B1FF9F2ACF693140375A1ACACF5254BC3D833C6D1C2E41167C4886261B6C36D6C1270831C365B9CE696DF2B22470EB30B4B186A7E3978195588F2E533539FDA3D7DC50D54CD4A5B55DC987B1F10961537FEF1A4CB735074E4986C51DB53BC2B0863F829DE791BDA75CFD86C5717DAC3F3D95C76DE4B76DF3A76DF2976DC18F6D80776DEF6F5D9E5F5DED475DDF274DC89F4D87874D967D4D0D8E9DD8BEA7C5B8C4F580C7B343AAAC653694925DF5BF0D94A70DE39F0F0CC7BCC6536BE251F57929A9639B20DAC8BE4E61E3565C0161F55E0A3381F53B69FAAA068052A4B7B995C6C93CCA3495064DF96F3E0B8F5BE6EE4BD5A0B70CA8617362DF34C87923838A61B4E42DC59A3C1E7349C56058A938844DB75B0C245E2A9EC7081971AC70318104FE7AD709978562937171852594EFCAB826D91C98C4368CA3B4B0A960566440EB3E1AA2086961DC5C832B3F28042D39210DBC58E046BDC0CBE38ACA36C2C023989F1106E77498C072D29BEE00C483EEF320BF6746A3F6FC0DB2D24A0C32D916BD6CFED1E1DE2B6354FE9B7683DC1421C951FD1F4175884B5E8686A3BBFE074FE8FAB887E2568B2C316171591F910671D57CD8B28A779212EC46C4FACA0AAA2B4FA646DBF46314AEE03C4426F5B4EDF8B68ED1E92D29A7F4E88619FE26B31A778FB6E440EFD025E8B1CA67F9AA358BEE1A7DF27753A8160DA3BFD357BFDBCAD2C9E02B0D8616F0689EFB320F22D11F6D910E3FDF56E70F83C9FA2AE93DF8ABC3F03616C81B4040BDA20DD6107072B64A4548FADA96ED92E1895BBBD48AE8F6B1966974F7DB006DCD7E30EDD981F3292DEC2EFEECE8B756132A09BAF54AEDB912329773A54F8A88F9029A847B18497FBA0679EC5EA83102CBAFEE8C5D87E05623600C2B8E3CE9228258F8FF90CD970F7DF021CCABCA8CE9266D73B80057E99DF37F688B7259FD47500A0AFE4F9572CDDFED3C4555E25F4F17BCF2337AA37D1C4C828632AF81A1A172DC5B40EE6B1084C62B528E21138D64F7148D57006138D68687EB33910AAAA4D9B2FD48524CB6AD2E4D9D6E33D4A519197B27EEBFB832A25D469CC94D58C2AE13B93DA4DA3CBCEA18E136808CBD968F68CEA317E6A9245E4201BB578C5FF8417AF1A15BBABC6CBA46EFB53E4CFC9DD3D64B49CA469FE67C84772D547070DDFA8040611F082A5C5E4A052CC9DCA07A27CB13960ADA689EBA59DB624557109E1D505E051CCA05DF6C7A68810D7A7CB9105F6E9B36D67AB2BAC2C23D9B58EE2639FCD72A74D05B9D365D972A72583DF801D618FE18A886FBFD279215F2CED1F1AE5BA884A16FA87CEB337251A5A553828B688A46254AEAC7E7411DB6AEA6776F9B4AE99D3434EE8F621E147A265A7F7AE128EB33C453DC75279FF56965FC98230D80319F32E0AF513BE6471C8DB03932C108D838EAA178AAABADAE8A8C144B0D1511DD1BE2D4164254975275BB0680761FF752B1817D93733E780390FB2F62CAD4C833AB5C564F56A6C0175DD9A22BE73725B0B5B615F56D23DE4D74CF8C26D57B1C69DE585CE6B8439E848DB5169D6D7D285BF5DDC8E9CB0CAC527AE0C09005FC4BF340BBF2A193A5C70D649D68E06035BBD8664C67EC0A54A0FCD171579C8AD4964CCE48AD92FF428F9D6D498290955982AA0A82F5DCA4B75F23CB4D8EDA70917DDF5E75593FDEEBF52B94DE5CB44371166490063A443F25C577428BE9B4D933842ADCD266B6F86B8A12E7578582A5BB5FAB7D661B278DC1255675B59A9E7CC457FC9EB0A69B9907E35395BE112526F9B015B6D75ADBD925D5EBB7AA8D5394F77070B1D12402BDBCE2A9ABB827E95B1D5EADE9126065987CE2EF6834B7E7CB12FE1A0C65CFDED9219C049F59BFBDBED3595AC9E0646ACB1A6EA776B8C15554BB0A09E6A2BE0A2A6464A5FA7B6A9963A43559D646DE0C310D6DF80E66D010E489E566038D77D2BED01D8D4027972A805F1644E3BCE27771BAAB75C215739B71B6B5107E7FD567935E003F5C61A8CDE2734D66223D1829A6CAC848B3663A9E7720F5E64EB31D0A9B3473D8053DEE528FB3EECD0390D70A3BD706884015BE5E18ED59B1EBC931CAFD7D4CDFC06FCADD5E370A4F88283BE8989E930DC7BBAB94C9740430C653B7A1674BAFE32DB913BFB9A7A8C1ED5215AC988610ED8DA1CE6955E46691181E8AE4C86B01261735D8E17CB1FA51CB0858728D91C79A5023C3839B2C44AAF2CFC3C5C5F0567FDD2D1CEA563FCDF44DB9B48A946DA598341455B5D267C4FD860ABBE21A5C13B82824AF2BED360A802F9DAC00AD259D974CACE4ADB34340BAB9BA60EAEFA66209E4BE10419A6A4D2FE3A6781D1EE31367473B17474B053B57185E4D6418B075689CD12EBC4E507B8336F69228C44CBDF9D77341358EAB9BC0E7B7387DE347C409839D2EC3908C55DFAD04F6658DE5D17B75A65B7DB9DC6E0AB689BA45FA87D2AE381C8512E381AB99AB80C490062AE5DD8935D7D9BD3F1B01A29692B44E5D9EE4E44987CAEC84AE79FF1AF840B1BE86016B48DF6DDFCFD354FB22051B9AEA3EC0EE8944BB4FD84CA3673F29D61617C423BC45C21C54E315FD256AFB5F4609D79E8BEE85861B884E0839014F3F245F495B1D46E8B2B35775D36A70A6BBF18560BF847E2ED47EB3C3A1003AE92FF7E084E5AEB5A6AB4C13052E1B37BCE7A81D7621AF56157E937F94655DD8F7429BEA654A64479D12502E8AD3724068EA5EAC2348B6B2F5C077705D611CFABC3AC6D0FF799D258C4E0710009A19388B52C4BED646C245A50C81CC56B66C13A8963B2F0F6B56A5DDF87BB284ED6EB1251B14AA79A58305FE98823537E87F6DB4D76C189AC19AA3ADA16F60B09A956E88401540B5DDE477A408E7A41CC15140350C47672542F4B2860704502E50B332458C86B92FC6D97C47724003FCE296D8C7C96F0DB8B18767AC7FA1EBDF7DBE85779E507F0DBB127FD93701B16CB1F8734F541F0E30B7241B065141E08AEA097E97285B228AD2DCDE39168C903D843259C0E6133D4736DFC0532644ED6DB24D3AC74C34CE064BE3ECFD6F6D33780B3AAA3B2F646EA3A8E712A7669EDC0B6DE4A89AA31A0C80C37084DEC177A548AF6CB982BB55FA82276EA61B7A594C3E03B6F397B51BD4AA34D3530C0405FE0EC247BC4E0502AA3C9757EDAE77712CE3CFD82FB8BD6E56C2FB48EE6FE5C4945F474F338D48BC3C7428F31652F71CFE2319E8F04C76A82664C0E859FA80BFF9EDD65F99F63F17F3F16FBA7ED0945EFD02F1B3877CD083259BF341E3215AB23F2408059B7BC425BBAB40BDBB8971B9C39C7E02CC63CF234D3D5AE2CCC59F877F208D89FA662FDB27949CD4CAAB97A7CA72EFDAE8CB2CD585A5C85B3F528E9F1F2545DF89712A1B1B0F8FE8CC0414CF06528FFA3BA7CF3B613C3969F346C49B2BB6B7231A92BFECC009FEF244D9F3634E43B2385A65F1B0A964FC79AEEED49464E1D6BBA7875F78561D4B1AE93D328A69AA0E965D2719C10016F0D4948FE16954994D54F46524DB70BA4DF8DA41A011048BF1F4935C220903E1D07A3462604D21F465213E1E058FBC4443A40428D908C8420B54664386A10422347AB1D36CF40420369E206EA1313E5018DF0271A19FA6B7497F0241AD919DBC5117EA7911C9E701C24DF690487A71CC7C8775AF1A9EB14011CFD4E233E6DF14EC4472A8DD83054C72399465418328A2D1A4161C8289E688485211B75C6771A5161C8467DF19D465C18B251577C6F644E74643F8E641A4961C87E1AC93462C2903D1BC93442D24C74BC8C7CAF91119A681491EF0D6625F8731A2169882EF3B2190402B14654E82F8E72F9BD465268AA512CBFD7080A4D354AE5538D9CD054A3503ED588094D35CAE4538D94D054A3483EB5109251229F5A48C928904F4D2CD45EB61E8F642632D2938D32F9D4423A8E47F1786A211EC7A37C3CB5908FE351407EB01090E351427EB09090E351447EB01091E351467EB09091E35148803D4439D928253F5848C993514A7ED048C9B0421194D70FA66B1B4195FCA091159E7214971F4CD63B14E528313F9AAC7A28CA51687E34317029CA516E7E34317329CA51747E34B17329CA517A7E34B17129CA51807ED408902009A310FD682B44A314FD68294594CAF9D1528C28B5F3A3C152895D40FE64293F94B2FAC952802885F593A504514AEB274B11A214D74FDAA5F54035D268A487A2695F00EFE934A2C3D1B1B41AD9016847E9F949233DAFD2A8BA655D331AA11928984A3ED3792169AAB17ACF3442C3908D82FD4C23300CD928A2CF34C2C2908DE2F94C23280CD9289ACF34B2C2908D62F94C232A0CD9A8119F69A484211BD5E1338D7C7CB84DEA51369ED9C806A5398F1F9B8AC73145632A1B9447EAB1A96050AEA8C7A65241F9A01E9B8AC4538AC6441E3887F0F1631369687D993CA58940509434174D9406454A33D3444228528AA73A8F2D4F4AB1D6C875CB3348E7BC6DCB0B06E1B1CE85CBD0515CD5B972193A8AA53AA72E4347F353233A0C1DCD4C8DE030743F50743AA73E4DF72345A71119866E5478C73A6F2E43F78CA2D3080BDBEF9412D3F97359424A6274FE5C969012199D2B9725A46446E7C0650929A1D1397059424A6A7837AEF1C622F570BBD7C6E280B3D8C6E2508B13D38DC581E2A5E9E6E240716ABAC538509C996E33BECD098D6C9BD1A863BB27523CBB954259AC53F973073AA5139575868C8F40BCBD32EEC70F8854DFAC07FBB082700F0A0B889C9CD3EE5D55EAB2E7296D0FEA94058A8A7CDCC5D5CC2BAB02C509556B9DAB0D6D5745147747A27A03D54156CFFFB14B8A2DCA6A4F696570163C1A92641F08DF8D45F6A4DC9A9FD9791DD5A602FB4B9ADFA3CA54645FE6795D994AEC0AC579B68ECA2F5C4B35C2FB16C57769141BCBEF7573B0CD4C7A5F96183845B5A9F85E46D59D4C6C05CB17AB63EA0881AE3FB73BBA1E3ADB78555093B8BAE8695ED5BB2D5AED120A5ED3AF1D0D2D373A43B8EDD4D51D75388CB7818D06F6295E30549E837AC0586C409FACEFB14EC1B8C607F1567FE6E57A1B65A663FAA48C6F29748D105C469B04EBE94C36B08DBA460834E0DC411CD2B4DDC436146C53CBABF69EBE67A346A8055BC51C14766E1085B2DCCCB8ABA923659A31F412378A2A6D746AF2A4AA723C320823FA91D8842B1AAE21B4818FD8AF9E67EB033ACA115B78BCC7341EDDEEC2275D62262505660B967A68886B808763D02330574FEE0BFF267CE11ADDA0929CE28FD25362A997B8376BA6C801393B9E64715244A9412B39DA033074D918C4EB88FFD6D1F0313EE70C15E4D46E561BB0C4BB16C3C758761DE9F8F5FC88922023C1CAB79F729D3C913221C5A8C183A4A7A9CCA4424337657659A1DBFD1044A489D2449EA0930AC85002128F364694B9748C60806C34D5984634844698F48DB74C08AD35F9EA18C86C3181B8C0105116A38F2FF35DB61EAFE2A97A554223139ABEB8BEBFCD3EA39AA8F86F3C7EF448945167B1D2D4C8A4CB15E1C1ACE54DD30F26D56129E13ACD2089A7E44862F6AE20491FF16AAF2497CF06B9910989920A92469AC0461AD51F02E4516882A037792EBF234F98A21A1D9CC4E42BF89B511537017CB95A6143731D409C8D9A64A4C3FCE4D8A80F4DEAC1D32E26CBCD4C400FCF4A3DDF3245834DBC2C2A20A2769AD97D26069B37DF940CF2E101CCCD9751923695970ACF500292199269A3E1463089A84C37B10A9F9E453804EE997CF5A4AEA3F89678F417958CE2A3DE4E634AC1122218E02AA5C2E2D99860819409D820934EEBDE28F29215A0ED5E9F9E494CF0FFDD8EA9524EC66281048502042485AAD574A222B6693E59119BFF4084853C73A3E9D7E6ED9A5042D23C1D050A48F3E0CE94C241B56356C1A09AFC008482744A5771956050C524C2D195B03349185C584EF442E72A24C0B7671314809F0F44581A17987CC1C39452888A91B529839DD7DD0836C9B0B708611869619BEEFDF91904867BF958A561C4A290E8F06F329B6B1A007F566D23FFFE3C1A47CE5F93EF9FA19B08736549E5433F14AEE9648DF43888CD62F2B298A03CA47949F262BBAE4F65CFB7071118C9A3EFC6BA2CB018C1D531E95896329070C1AC0F559D194D214391838B5B18470646B4B7C88574F7A96A34BBD4A9B9FF20A5AEDE95164247950E2E7334F65E881C50A105250E60BD596D28C2A5E5AD5BA69A081B55742A2F814CD2A69D48E575987D1D08F0F8212C0649B5BB2B2A2A97615B2490FFB10303A4A5BF2C3399FF916DC77CFE47B6C9668BBBAA4EB2A6AB96F64FB7953F361090E3A01272BCA0881C2F2523C73642D2D67169F9788F5254E0CA6B36BBC66281A48402040485AAD574C222B6693E79119BBFEF2E81CB7617A67B1B55D1B5632989A800BB541A69A1204183B87DAE752A41111B64D85798D05F54C4B6FB7E7D8E73F5CC93F2B29E855F90A7CFD6DF8A1B10CAA3F5F083F52C20742E27D4F97AA841467341F72CBBCFF17AA8ED7EDF9E535034777AE0D7D443888AF41A0FF5C4FAB4C2627F7727A0B4D85FD959525C3A7DD874B666BE685B1870026A00257232EDE4433765EEA9876EF5039878BA5A9B59B56CC9A0C2B2BC790B376E6EF171B6739714A2CEDC6AFF525A30424978CF0BB24F554224C24ACDDE09AD1969E3CCDCAD0D95EFE696840FBE3598EB428366B5449509768561A17512D014934E0A735DC17E7DB40777084DF40B572EB0902CA65924CD9A5B606C35CA3E088D7651CD940A26300B2EAAC106CD272A2E4BEA850505DB4EFD497AE5C575AE1C242C6311CBC3A53CF6BCC74B252D33E9BB91D45B7E243C30B280D5559857882EF51762C0D2760265284D97CBDE9251367439F912B9F210A54C759346283995742D71BD46DAB84525CAF2D60D2EBE1F9234FE53ED5596934C225BFC370021E36B3EB9B449DABD9CD8499814A642B34594329540B074C028537B2372CA869A746EC818541E32B67434AA8E6FFA0D33B65C40915A76CF0C6ED6DC02E4B671B6B0E88C77F10D0329CA08C01841B238062A81927E60A9A08ABA169B7473A850553AE604A8CB0C42D74660FDD8FEF526DFC82FB90A2521316B736D644C8405846BC89D48AEA46D33E9C496CA539AA46CF0ADC16C32348E874E08D43DCE17974B93450816ED47A4B235D1197A753566962E19CB1F8088F155D7470A925258CD8ECAF079D22F48A56C86808EDA5A99F776F89952D215C1AA348324FE823254462956D106E1AAA0C290FC8DE56C0410445F268CA8AA2A731A652A863F0C836CACF645461E44C84BF94C0A949DC0F61FB19789B7A868E532F6BEC0908726594D18457DC737C104C3CB53030B3AF04D823B7A8A10DDA665A4876EBE915B82E0EC8BD4C8EFA971E526921BF0D6DA6C8263756F6D1AC9B1BAC1768D6294DCEF85F0FCB64BE2BB34A9EAF3AC369AD1588209C489FB0020575C9DA79630B8C5CB881ACC9C8736CFF5C16CF4C2D04769092F663DB23C50C4E482C5356E1989E2F8F03044E9248EF35D561BACF48492902875856C0449C45D2048B1B47126BDD8117B0A92940FDE5598518CA865BEAEBFA9856C5841A2800149B27144788992D8BEF9654964C54312A6EEA93913611A8B0616260A188A2631D6705A6112DB37BF3089ACD83F61C2C61C663FA6A931C5F00460F74C2149469F6BE0B1C3DF2BD4BD7758BD38ACCB9D785F86C0AE50CD36EAF0A0CD80444D902C16819AD0040C2ACF14A5DB92964375053478321833EAEEDD38B1126DBA865A380524E008258C1195605A9CF694B880D0266B68DB5B09026D9B6C403B7A1E418C315BD737CC0B4A621731D91AACD61B2160B4C91ADAFE069840DD6768BFDD9E8105BEDE66E8E96162134AB934D1997A9C215C1A8433649AE1C8410CFA820DE307F4095BC00C4F0EA445E8A33309007D86869EBED52C60D0993A3D780B8EF836D9805632EAC72C030CA92E1EF38CC69AA42E74A60647D8D915C084124688522043FAC6EC924034791A147A2920C0D0991A1CDE8B2660F10574A380B22AC59140657238944DC44DE9FC79BB03AA283DC52BCFE531F6B1F289E3A141834521987E1AA4DED6A69034A6CC11DB7A73CEB4C6899421E2C3BC50ED99A7795D9ACFBCC54BB71AB49DEC1B3BBE210B3455F2C02C534FF18959AA96901A9150036D040D29B726CA5E459534DAE81155A121BA675439C6C86C34635CD56008C037F51B9E00E72C1EFD645DC646CF7ED2AD5458A516C800FF94B6B3E3E8629F8E940D33C50393E288819F98B41E78F09B92D3B1637C0311E082E48144A6D6E213895465A17586845AD2D4204DA41FF3039B297DED8FAB2CF4DE1FD35C41392A0026D61597ECE3749276CB6EEB0AF5062EEA5AB61CB88BCB22C89665CE6D6F5699D2760BF747A11AD35747EDDB4BDF0E65A9A105B0533BA988D492B6D2259435A60A4ADA2C59F0AA80E0E6876B7BF71A96BCE9D073595085B907B3C4869BB49A7B1E2BBCE5043CE904345DF7F0135375C5D34F54FDD50E061DE06452403F4B2467841907344D3769F33C8DE5FB42DE6ED5FB2860FD252FA4387243F2248AB15879A8053D8F2425F5635CCF231B8DB1288FE8774C942C923E78226913F4E4893383A0374E66E0CF25F5EE86766A35E30C5538D8142B634B507EF44F4748CC29E86109C120E29E96B034A7B8B724286A89C7D9A79DC7EA861E1BB5F4D8AFA9C793B79576B3C3CD95854615AA0C4446B56C341004954250EC0838B59D8A4E0F375D16BE9EAF3710C05E6BE92B3040F5066EB73978519938EB901F551E889D758482A1D8695F2AB0FBA240001A2DDD7FF168B6DCA1AE882B0E575BEE44376EB9D47B2EDF357296F4B65652391723644312CAC4C87692712628B68669EE8DD56A3555CC67A8DA5ADD66D8FC19359C1894185C242823177316BD2C76B156536970A41A2FD0F0A743EBCA3CBD5A450FC5DEB5F6EDCEA0E2F950B1EA066BFA5C164FD6B1E1F3F5B66A8E53C445152BAE9AE3CC9A3ED31CC707F1041AAE8CF3C9545C16E993AABAFC489612693ABF181C8052CD0623DFB73A56A5174B6674888B9113B5AC51BBC7E501167D5932B5CF5C11FA4FCD13659836598364D1DABCB8240BD40682863D8361C03283A076AC45AC0C6BE770426341F6A89719AA706C403BD40B0D4366CCB6D490460C838E2518451763CF0DE8E28B996CA39A21CE71A2490C7505B049130F8B698D3C2216D50CF8CC9E06076087FCE09E2B2784804D5276A8433B016D910677121A6426379A684E5A767B8D2DA1A2AAC1A50C4F24190BB200451EC34B1691C89CFF0E2C0343E300DCD287D0615AA50CA24335487E26550F37F5213220A88B528C24B15F24BD2D467FF1101D31DCCB7447A1B890244A9688414B240D60C2967830828953A23961E5DD7670F743197B43516B7017C4A5F9E07E48F0F6F307C0558C50C5919034441249C2833592D01114A2E64CBB0FB786C3002A3681910F24ADE1631F7830860F76A03FC3E0C00AF1523EC00ACDCD7DA6FEF2BBFB54FD255717354013CF2BC0C572052F64D7CFC1360017D01DB901DC38379AB53DF8415F0391F34376831A6C057087DA911FC0A5697AB7597E8345CE8FE7472DC270B577C87B7EB48A6FD136EA129E1FE122312AEA5D945EE66B94567DC665541449B6A946CA2EE56055443119E5FFBE3A3CF8BC4DB3EAC5E16D5D173F1F1D550D74F5689BC4655EE537F5A338DF1E45EBFCE8C9E3C7CF8E8E8F8FB62DC651CCACA39E73B51DBE84CD8E6883B85C72CD638D5E2565559F4575F429AA30EF4FD75BA11877919965DEC0E2FE63EC5D65B1DBFA9B457D79F26FE6C2F4A3B393378FCE5F3D3A7BF948823232F1156ED7169B554D131120380225A65DC5511A95FDA571F1C6F8699EEEB699F622B9026B57DFE625AE0A8735269B63BD89AAFA34DF166942DAC902F27976A82B545509B9E2C0430E19E678E44F16A84D3147B88AAAEACFBCE4D83FA68A48CF8F3821E065ED4810366EDCF3B26B24D9AA99CF46B6A53806D2ADA095F19709FA43B358190D488E1774BCC4F5F7672812A0FA543BA4BF366E2D1EE9AFA08F518D749527595D89587DBA055A89A79F0F499611F38481A333CCF15E46F11D9E3EEE58B031D50229C9376554DC72BA8A4AB6C0DADDDCBC4CF3F80E715DC96498E39DA678F873D2DA2659604874A79BDE3C4B3699A0D78744739CF36D5E27719E5520BBC45C0BE4CFF16D946D100CCC679AE3BEC23A42982D86441B1C21DE348BA80D47ADC3BE46FFD8A1AA06190097B0F84299A06CADFC0258C2FC0BBF34614959CC3ECD02254FB97AB529160865BE2B540D050B98E3BF8E9212C3E55C53A9643BAC55FD25452256976C8185CA1C8B1FDC6621D30EF70DBA47A988D825DB61FD5188407F00E7F514283C822535CCA1C28137BFE69F00D68CA9F6483C77E8743B2B192F1813C046EE922DB0C4065AB70E6C9A43BBB079C9EBDD2EC90AE30F01E20F4B84BF0B087FB74320DBDE9095C6E6D822BE4FB004D710629F63819864491A65EB8BEC3EA9E1595952C4E21BF9AE4227C9F64DCE5B846C8E052227649756F2E5BF1A6CFCBCBFA0FA77AE1E74BA39DA75B4418D2CB06054B205162A763C4E9B648EB14A730EA24DB140284ED6EB84D85E40BB844C1B5C10CF1EA78E6A4E04BA24738CF7518AADF33779C501D1E9B668AB5D5902069690698BFB819F23A86473AC0FB749857F834A82CFDB3F7F88ECD88B93530404B3F18C480064ACE7E743BBC930B473857A32911942F2971417EBFE10BDEEDAD9967DECC255A943A7AEA3F8EE244BB691B82C1632ED5C28BC10F56916F2888D1549DDB82C3BCCF39B1B14F38E142ADD0E4D182C5D9A1DCAFB843736C6D4B95D4D788D9DFFC9337C48B540BA3ACD2B9ECF5D9A85B32ADA620B87F35575691628BB1210A331D5C23505488FBDE49CA748F4E60D89D638510AB149C8B4581827757B57995916F789E638E44890A8ACC6D405969E64A1B2E56BC4E6D82D8C30C9C99A1C8BDA95A804AA292B63FF9532BE557E81CEB746BF8C36499C44991C9F2B61FD8515D904DBAABEC095B059EA89BAA64F9B7503B04C620EA24BB259E865FC48EE922C6C2F14E7D93A2ABFC8CC0520DFD2B2C35661C6B795C9B0C413F50D956CB39421A726001E3219B67862E5E8740B3411C712E1F76253466BD49D7BA791D81C6B44B16A4CC6DED8EAB2B0A2562B3408C3646106D329B6ED3EE58281D8275AD8E9120BDDC5020E63BF605340B00DBED2B5A1FEE2948DE469D00C64508B209DDD58425E2E816C8B997320163B95CFB3990979FFDBB5A5FBED41CA593011F3922E17C10AD9F9275B720687ECCD2625EF47E4F36C5D1EC04299C9B0C5EB664A0871C8B239175325F1498922FE60CC906C8905AFE9B92C4B4CC0C2A2D32DD10073864EB77181E4153A433748B04DD91C1B870826C8EA52D0454C86055E99D409CE80EC673ECF1E5554996C8EADE3E532FA2CAE96B92C6B4C6805CE65D9609677D7A84AFA7B472C289B6781DA0ACA59BE169C4F4C8E3522A82AF83C9BB35C6DDB409117329D71213E80251673818982CF64589C8EC2538A4C9EF8BC654ECE5C54AFF3AA4EF8934254B2D559933FAE846326246966D75CB2B9AD656C17326DCE426C92181C1C6C8E0DE2E7D7573C549364857129625CDA61B46695B86C6732E6747EBDCD4F369B1277D33DBA88F9C5AA9069838B273004181C4C86CDB282978536C50E4164FC986A7312A3B9F926EEFF301936671F106F2C7749E618C45287ED6E36C7E24C00D6C1A56C740B99E6B8BCEA82F5D6420B3A4954279BA51C0461B08883C9A4533DFEC93B03FA34DBE51AB44CB3323BF04FC830A6D397D87DEA62F702070EFB646BC52D564CB3505E488C2571D96CC41882301063984CA5B5446D65BD1205ED7336C76255D63CAEF3E69EB38EA8E49937F727581D0BA7DF4F6D4FBE4FB5C27EB38BEFE4EBE131F7DB2ADB04B3BC03576C4CC657B86A7FB8EBEB3007551A8ABF45E98E5F5DD3190F7FD57F8E8DDA82108AC79AB92C3B4F02D85E266371CF4402707148B4C0F1BC89738D364DB407719F93CFB3F19480C7902F5CEE245E54CDC426620DC9765859B5DB92B7F644B831C7069198C80504486558F998509927B1E062EA526D90FAAB21EF3EFD97A091C45C1BE4559EAEA1268FE93668EFB17907A18DE9763EBA5438A23FA6DA2D6256BB4FA2BB81C9B0C383C19C0EF74569B216BD175C96A52FF1D7FC93FC941E9DB9E4D1BFC62909AA7736E72BF581B66B18C187D9273B6009CB2436C70A516A1EF07956A8CD21CBED361720C70C1B374089F839B34FB34311EEB975695628C5AD0042926C8E60F2C72FEDA8E5F33F9FF7D00E745EDD17E0E8A3D3ADD0567589B24D7D2BC08D19361EE8F52E46EF8ABC8A32A9F69116B2F94EB1AB9B1E04D530906D890D74159DEE707816D4EE62AE0DF2671EECF3DCDEFD55019CB495BCFE2647F13FC8FA214AEA3394465C1C122AD9727F01EC2D3667BE1D8B059DB58AE7C4E54E5B6CE5E27FBFBBF917D87BDB63FEABA31B5756277907F8DD650CB31BC13E262FDC6813722D1C8981EF5A9E6161DCF0B79BBA340B945D197D4A52DC71C0056A21D36EA180C734AA11BC52A0F396D8DDB98E4A612BB7B49B32BC2FC307BB19C08228362D7C5594F536D0EF155945CB55C15E6A12FEAC87E5410FFE94878CFA14DB144DB48383C6A58A3B8E6516EF1A8ECB649B64510DEFB4CCDEB51F10565B0FAF73C5C59CED2A0E5A06DBAF815B8A770511006146E0F2BEC6B3B0C0F60797F56DF7EFDBEE9F16F5DBEEDFA4A762FF89F6B28449FFD8CEF97FBE2DF8009B43A20DCEABE4B3B8E1D4257E73559B214EE20C16EC3A2B01D93F7F850CD7EC842564C00A26E19E9BB0AB02C549449EE5F866C47E3362BF19B1DF8C5803BC6F46EC3723F69B11ABC1F966C47E3362CD1197346225BE7D28B8DACA21AA1AA681A4824AB6C18277A753976DE994D7156D8ACDA632C8A3C2814705993B459CD22AF4F30A3E71B1723970B12A604E172E9C2EC8BCC003B5693628427F15B6FD055CDA1D12AD70889E1470DA441B1CBC92CAE0CBF17C9E0D6AA33478B82ED16C190B2C0BF77C21AB5E832EBD841518CCBE1A3B2B6B5D2390518B59DF40640A2883BE52934FE36E0871DBDF7FB31BB891627D1785F750F17C81F2F74608252F1EDB081F046120743099DC744D8450637D9A858F8BC452BA25D5014FF608B92EC8D711F0DAA190EB822C0F0FC566BB608B2760F83C1B8F06BFB838B3BE46751589CF8E8CA936CB4622B0E2BAB14FB5415A350F229CE685B09AA5732C164EA8AA04637748B439B61AA3E45E3C0446A7DB1C2E6DDA225E1366326CF1A0A7BCD81C5B44C9A35942A60B2EF0809690698BCBF7CE986A8BD49CA987E1862C8B4934A9F9B676497B334961414E0ABB83A8E2340583184C543242C5704CC4180F63EADCC74F8386AF785BC440F48A2E756F04A6AD92B75D0381185936306138B6CA7DE9514A1EF566973F6D9AC5EC1BF0CA3770349FC9B0997FF1428ED8D1C25C3EA6DBA05DE6F7B81379AC3ED5CA2E48EA1A801A92AD06D857F1181D165C3086ABADC2B9CAAB44BCC835A6EE93C2F1D7366EAAC64ACF784B17B6E423C1B6B70A257BB9ABF8FBE05D92857C45BE17F256B7797192A6F99FC245283A639FE4EB32485467399099B449891542D7D100B247E53CFC89E06B51DE978E81C5BE0625DE21FA0F320990D92093124F2C7AD6B1E996EDA830BDE4DE4556FD132CEE5F17D1530229E6DA20D7BB520ECC655A4B9638693319360A85280E6065C164EC8DB4B25DE2EDC55081197933D400F38BDD19BA8976690DE83036C71AF10F10CD6A36ED68FE0E22D92D898C4DD665C5348C7CBA0BA6B5444A4471D98BD42126E4E9461C3730AC46043714C031B090045FE5651DF9EE66C22006F22B2394CF62A43CDFAD63AA8D92AA6A72BA81043D8194289FEB842C28533AC7095150AA74CE52CBC155BE2B6304B091C9B0C5FB03C2B2E25B4BF27708C78A5BE64726161AC4EF518A0A3C08BC3D1D722083C1AC2296B276A0E16587CDB110ED6CCD47FAE99266F74A04DAA098D3BFB1D423F0B7DEF1FA210803A185C9541E5041C9DDDAC6EB0F281A28DB01AF900CA9737B8781F05643EA5E095B80C8FA321843A1B38BB0DF5380C2D7A55B58EE0122C7EF5508A020C3F29F319090C7100AF124AB14C77010593EC33A9040C2E2F0F46A20B9737828751F6478D1834D0114B81CC8C8F32227963B277A1AC941276B45BE6F879DEC8F712D2444AFA26D927E191C559E92A4413310272D828CE31C21CF7A20DB42BA76F56D0E1CCC1E936DCE40E07A808777D91C8BB3529FF1AF44BC6846A79BA3B58C82D967C7B55FF324134F738FA936865176C71B461978176FD1511464F0388F19DBA112A28B5B1A99008AB9B6C8C03D1F26C3160F1C775C96CDF2F5F32AF96FF12A749B38EB9253A886AC0E8B0E9037F926C81801708C870948AB91C27C030F962E634AE5BA507FFDD284BA4FFD3B4C0E64D0632A62198B81CEB2EEA6933826A6238F43252FB73978519CACD725E2EFD750C916D728F38D78247348B4C211574B43A2CD6D922DAAEA68CBBDE24025EFCD00F96D97C477E44636CE2F7D677E3598C140D1014885C9EB166EF07DEF002F805CE5DCC06812CCE97F3B66C97FB30A47F2DB138EFAC9379F05BBCB8EB228AD031802722083E1A222960AD64023ECBA3339CBCC3227EB6D92897624956C754A1F338B3F08D027DAE1ACEAA814229D0DC916C3BAE531C8783BBF4C54F18789FBB49907499F799A6775946043872F327CBD4B197E577D02916B12872E5FA3B41AE956F12DDA464D8BAB228A9B4D8D357A9594554D66F64F5185DA228707983DF7C91A9558D57CA96AB47D440A3C5AFD233D4DF192AD1E0B5C4659728367E1F7F91DCA5E1C3E797C8C55DB499A4415B93D9ADE1C1E7CDEA659F573BCABEA7C1B6559DE3EA4F3E2F0B6AE8B9F8F8EAAE68BD5A36D12977995DFD48FE27C7B14ADF3238CF5DDD1F1F1115A6F8F78F20ED608E5F1B31EA5AAD68C339CD22DECD86387FFF3BFA22F7C0FF7F2738D6EC4617BC4F5364FFF5C3EE2495D5E1C7E4A36CD33168D826AADDD1AADAFA21ACF9D19998E5153F1C383B7BB3425C7515E1CDE4469250C67E143A367AAFD50B58DD2B4F99425D29BA8AA4FF36D91266D98A1166E8DAB59374F6538C0ADB095DA746D8BE552AB56A5B5F4D97D54C6D8E0383CC04BEF37CDD353583E9F3EA541EB528CCAC1635E4555F5675E0E7DE3044B6B04A5004A3C9C6622A8B0AFF442C8104F2C863279E787F9CFCDC9941787FFAB21FCF9E0E28F8F03ED5F0EDE955801FD7CF0F8E07F3BD4A0FEFE0C456B1F6123187F25DB6B9E18CD933595174A8967BC0F4996919007EE302FA3F80ECF0B775E1849BE29A3E2F64BE861F872777333BC8CDB4B8D7DFDBA48132D409D645F5C1A292A3E673D7A966C324A1F97284AAD31CEB7799DC4795605E0CFF967DC59D90605807A85B589A73E077667AC15868021531C2652D8825DA37FECB0CD138247C403BE0E87D7C73DF193EF5FF254E0B42D4499EF8A70EDA242AFF8358D8AB5E20984CA1C0B5590C661A86EE3C2BF4E7F149E3DF7BAF019B1AF8B000CF935FF14841F3D8E374F88798AD7F1C9A0CCDC0DDD10ED0AD3A8EEC42E3F8119A8D486D2CBFE6A0FE87ACE9DED195D6F1072FDD8DF066B71DE276D2C310F9CEEC9F98BEC3EA9434CC397F9AE4227C9F64D3E5A764E385E7A815E9905B2081BE7F62FA8FEBDF069D675B441DDA3915EA3A979CED713A375EBFAE98615D9F0692EC133AD7283F287A89B231D7E4D7A1FA5D8D47E9357C8A72A2DCA6A57968C8DE40EF521F1B26D3FDC2615FE6D3DC2ED5D09C02150337F828B1B619C52765982CDBFA47112DC2484E3B6CB2C2FBBDFC0E23713E1F198A9C35439505BD4C1B8879D3B567A7456DFBF0A76D87A2D482CD0BB932CD946F43AD1198F78082083C656EAF0441EAE5204AD0F7316002A50FBDAE78C7D81C2B852F05A33FF3300A3AF4EF3CA7BF6ECDF04F0F5EAECCA30E213487486D700FCB8D3C144692036BD4EEA761FDAAF5AE3416CDF4D8D608BB3E109283FAC0EE6647D8FD9BE2B5119A4863D2AB6C2C322364FC8245116127345F65EB6A1300B5A4938CBC924DB4C65127BD9B7D7C46DEBBD7A40719EADA3F24BB869BE41C2C65816FB2B8C062B84CA781F951B540761590B15A4520130BAAB70AD65E8CBEE0E8CAE972196F92225DF7ECA9DF63A09A1D33E674F480DB5E01B9CC1ECC74006C0EBC41F638F17426F8BB88B23E8BC24E2205C240B8098761F7DFCA073B7B0105E0BE56BCAD5F3750B999F7CB9ADBB757D6DABA19A4B9064832E2947EF93EBACD32EE5832C085BA8E146B71FD8CBA84AE2931245418082AD551BB420C647831466B2679F2CF605A39F2BF6C5EADEFE656D48678703F392B0EF3A9F7D43380C1ABB74F440639FF9F58563DEC90D031668940BEF047B9FC9819E06DE2B174C10E9E59F01F6B531431D63B8A85EE3557A329ED1703C43409E52755F4F877106F12FF57AEF56D38FF47A837D7EEDC5220C70E907D05A342116BE53F864DEE6279B4D897BEF1E5DC4E362CE451CDF92237A28C89C7F1DF9CF82042304D7BB80A1F47E82DB4E33F23742895DCC1BB4CE634378D1D913CF461719AF48CE4A3E369BD95A84D0B92C707B3A9E17C1DD275D84167781223565EDC5B01B0B066BD19ED8F79C14F9B4FB39AD96DAE7C8ABAF87C1C0BDE0715F420C30643604DC16E14196DECD9A2D90E9DB3EB2FBE6DEDB5009B3991B760919E0B8EF14EBD037BBF8EE3AC0B1A67FAA9568A0E5CF5E2F6A1FC43234D0818206A67BD5DA7D82DDC345F13936200B8216E230265961076A63E8C57A12865DE36D010F886BB4698DC7103B661721AE415D54CDF4E30F9255BB2D21F1C321E66CE10F433C34E44D3C1F8CFE7CFABB4FFF45291237AC559EAEFD5BF51E1B52017883EB329E0A76C2C0A6E86AF729C4B29A4085C2C14A3A59D30B74D73B30C41FF66BFE29D0D1A690C7A41A2F5B203DBBEF1EBBD6E2F73E08D50305583A60AC90137067C39E6CB7B977CDB0060F30411198CB20302466912F4A808A849D70A7F0BC32A7E11C15D6D57D116ADC61A8555D36CDF1F798AE77317A57E4559485D433CDFDA3F6419E304AB5010CD10FC399C2401A7A45DE2AF18408E16F5E1521A6E810181FA2A47FFA3C84DB3B503FF9B9D0AD3C801759F70D074FE0C2979CFC7DDD2F31C0DAEFAE140FE1E32FF6AB0959759F646B423756E22F0717D5EF0DA37F3E784558E2E0493FC382B8F19FECC8CD8E4F498A45D0FB6E24115C3C6A518D68EB1CBA596EC2F6E5F72ADA47493CF94B7B5DEC8EFE0182731C42704005ED58A127212A1468EB60DCFA873ACB6867C81B4163E41BA91BDD7AC81CE45D41BAD24CB71BA11A6E871861996D64184175FB0E5A87B20D98C1A6880D9C6E25698265BC256201A6DF11310333D91031453233B18CD00C771A8CB0CC77431CE08254D1D06B611417CAC8D16F8A14AE470DDCFC463057C7BE1017D5F9B618039D710E56338457C967A947DE04C1D04F680C15AE9F4CBD846658A6FE38B349DEBBEB0D1795C650E1D83E9ECA127D1B26F42B839BD0863861FA7E95727C7607F2B70F5705CB1DC73E2730E59D3F08CB61679C3082BC2A88A2F706193BC9194277CED91085E8436F94559D6706B702CCC01A5DE16B5F25555C265BF284F878180974721F3FF92994478B5E8D3C408F569083F321DCA2DEC7533EA0A824C57BF7A28A49061E01116E8A6B8A9751E2740196D0B9484E4FC7CEA3C18F04936B81B744F1B20E53AF6B86B7AD91440744F7C70BE49E19019DBD7A2C848F4FF78C32249D435F9EFFE32AA2A32386DA2BBCA8DE1558D03C8FEAAC9AA870A779215BA2986DADF6AFC5856DE2358A5172EFE8521F89FDE2A0B50C0A714EB8850A13ABB8C50A161F78840B1225B88573DB09E94803745A73F22040E8B0F7499D7ACAB6F134D6BE89EB3291C95ED3D54F652325CFA8696FB8840F7C35E3FD94B745CC455331BB99D2D04D6300116C3713A86F8DBD1104F0C1766B3189D27CE33D48839EF00E73AAE0A2C2EB2852DE738EBECCEF936CE339CF2775ED09F22D707673ECAB887DA2CE78AB9EABBC4AE8F36B664AD4468B38AA1047FD11607E6EDFE76C41F0ECFC29C9A2F2CBBF6CA3CFFF6AEDF5DD55E341F17D3914B8BACD8B9334CDFF9C245C32EE028FF03E23B563F753D41E474AF646D17ED3910DC8F47767F74457925F2155A6AC24F94E40053B973CF6D576FB7E4B3D91ADDC785ADDBACDA333640C09BDD0EA0254085FB4F29FB0103E8EB46B54EF4ACFDA30085E37FB5B1068AA76321AAE725267972583855780EE0B37EF002F10F65E02B548591F4CBD897669EDAEA068002F3DD501F9CF9D1D50803586B715692B591E22E5214BD3EF9FECC75B1A4BCEC233CE0446F5F11F66B6E3CB782C5CE5651D396D22B6942E2361A4A4963FA147C119AA6AB2974F6E4979E85B16C453E70E6021F4EE00E6AF7BC3AC2557F9AE8C913BB3297ABF8D9006C79FC52D8E3F77E9F30DA187EF7B94A2020F27374FC648ED328C59EAA9ADFB465E422CF5975C1BCEB581B23F8E12F307AE6EDD02DC113A17E9EDE9269C8296ED7094ED429CAC9ACABB6B5F372B51720D16D7D33A8B54473BA1580539FCB0FC7E72983B84E22836B1341A2A3F232340C8A810C3D3E9089AD548727F79B027761D4BDCD31A930CA6E50468F6D72FF645882DCF05B96AF291DAE37C10A3CDBF9D113238BC28395565767831996AF3E355B44DD22F836FCB459E380817A10220A675059EECEADB3CC499E7B308577CA213AFE79FF1AF047906706E79EBE4EBEB49BD44FFD73CC9829C9DBE8E32D50D28FF51E02EFCEE323FBDA8B7DF09294B412EB7B450138D1C4CBD4AFE7B3F9778D6F5B294E237F9C65D9031B1BB2C77C47388F35CEACC98F76D0B5347E63BB27D16869FC431B1C19C383ED07ACD2093EFDB19DDB0294ED6EB128D173F422904DC89F499C580B0F40AC4EB500389CD54D5D1B6B09CC38D47CF6FBB24BE4B93AAC639A5D33CBCF07DCF3DD95A0E125DF32AAFBC317E53064C308378E2EF04030369EDD12ED515CAA2B4769DB2476AA7CD66867A424FD2F213C8C97A9B649031E7A40DC962E63C5B7BAF6708CEAA8ECADA1BA9EB4A5F73F71A45D5783E36EC59A393AACAE3A4E966DACF37AA3EC89B8AB97C709DA77DE1BE662B94DE3CEA522E77699D146912E36FBE383C169A3842705FA2B1F82C16F4DF04503C04514946439492E8F6751961768BE335C9E2A48852A6095C29D83F3C7A408503C147032C9F73860A72D131ABE1F67A7F78C0E7F48C8E19CF8FA8BE371209E039F4B092D07C80118036E5EBE877917D7BDADDC4B3FA1178BF6DECA9C6C54D77549B60DED50D3A0DD0264CD2D1A23F5EC277EF2E167926F9D2E8F85EAC83FB401F5C7C5D69978F25400DCDF7DDE3478F54FDCF0446E60569CC98441E246D957495C27F6E251BF250D0920F8BA1931791153AE08F102546A920ACFBF798E7CBF377591B84F8E02426DFC7B589AAB8D93DE6AD6AFC69A9E4D2218B18E1653226D33D56BDEE2965F2F04C92EF416184969B74687655D3CD3E4B6A9FF966237BD95B765622C19A3E029BE7961DAF99799A105434469BF0C0FB5A0CAC25F9121FBA69A19E2E3EEA2D0E726593ED28C142345A579A192C81BA5CB8672AE907D9C5528775A4DFF766EA6EE90DF660FD4D7D814319921F788FCB78B8BF5D2EC6BA09D9DD049D4368921E7E370B7CDBCF2E26BCEF7A7AC26EEEBEC2A30CC90FBFBBFBA63C902E6F7C4372F3BCBF000DF497A5AD36A79708BCB62DEF0352D6B3E38DBD45CACFCDD0EDECD5D229473B77859E46E2B31EF8A857050B907C94BD8CBFA424CC200250DF7F2D9DBEF77ABEEF6665448B00A376BFE4C06148EA02764C261D261F9ED120985A50B43E1D638BE32B9195D01F9E5556A82041DF4465CF44850B01B5072B4BA5985874E79EAE32675E74D8AF369716858F60C49760F666874E23F4490FDCDA84F826F998187B66E1DE3EFED6DD13763713F866B19E96868209D6D9D41768143AF98177BA8C87B61F9CA7CBDF16F147F0ED0407B7BE7AEDD87C83351ADBA4A9FADBC2630F86F1B75C298A3C74F8DE1C27476FF3E2A324BC097576F496F3F3B609E61D3E7C8107810E08843A3F2AC4FF9174011CF0C7AAC3610E5A7F6FCE0ED79C1B0FD1E3C091E331F5ABE8738B33CA4B767AA7DA8188588115BB91D83C50A56E2C5B0BABF4AEB3CDAC37FF2E5FD0869BB5FBED2DB92585A0B5403E4A03DE38D8627A834E98E1E9E4897C77C68696EC652F07C3CE78A6577D73AE23BB1A7BDEFBACEEDCB6FC7C67362D846B0FEE8B988CF5409D3DFF389FBBD38DBFB70F1DAF5DBE7977FB12CBB7F9BADC6AF1B67087639BA23B62AABE00389663FA8C4EDEDB935E54254D7A642C3EDFFD40CD07E71504D9B1631F71D8C783DCCBC985EDC1EE3D930ED529EFD09231EB99EF4525C274F539BC75B9B4248CFF54FBFE828A04FF5118714A9FE07242A264B8F3E7678B27612A2F01224B2C2F2573479B70918DA5E34EB46261B079E02F100B6D1FCC2C04965B080B77FF70A1D134F890E50DCF7D0E42B44C4C0A875044CB07A468A3857E1C429BCA2F2275D183E9CEEC93CC25630CDE2AE234A993C80314F858D223B248C75692200951EBF0C9D9246054176094E849C400D63842DE572412768A6961C1E045421F63C0A35B351EAD65C20FB8775AF03925DCB767909C314CB441608AB0A1B0A808D5341C9DFC15991CB278DC6E9F9D55A75C64F7183217E2204F62907E0B90F6A064438CBF338D58CC19BD681931308E64B42264FBD2F7F2FB0ADF3A7F8ACEBF46314AEEF7A2FB9531F2279103EE8B341E9FF515C9868ACF6E9F9E554CC0DBDDD3C8C7FC57E79791089B0BF3CB8B421732DF607DD19564FA6F48DBF700787D454D3A857A8160B615A7F29B338A81F461A2E072B0D402737E49B05C5CEE8B28485F1B092E0AD497981BB054F257220A329EEE85289C374F71609A1A53A0B29FA2F2357A9594554D9E9DFA1455E2A16A42B542355BE7C383F3E1650F5E3256F12DDA462F0ED7CDBB0AEDDB2043A620252C3C3551081FA0F2A04F50D9A61FE936D7E45FEA0A283FD795D17C53F629C517CC80BBD72AC436B4E960D5DB2C0DB0704441F8845002FA9850C8F8B3CA2FEA3EA6FD4E7BD653F8429B0C61B7391AD4F6ECB080DA2643A86D8E01EAE87E03D1C76CD957C6123A896222D48B82C56483F2C594D07CAD7527085F699321F4364783DADFD61070FB0C08B9CFD3D6B83DFD07D4B9CD806BDDE6E9B1616029AA09A47C40D199920F980E283A6E0DF49D2153F29D21DFEC3BF28F28BF6020396CA0274082D802B024B165CCBE29FF98F22B5AF83ED68880DE6740E07D9E069BBE1229E0D399D037E87CDDFC790BAAEB36199C3D6F0DD4F578DD034496ABED31D7E00BD2B97FCC937DC36CCEA4AF2A49349EBC2974BEE63BC2E6B4F031A104F445A190D167A55F537DC410BB598448E09B3CF9179A6CCD47E855AFF0153A13FA0C9DAFF90EEF8815BEC51780BEC797D129176A19272A182A135432543EF71D6A15C359D1FC49BD03AA286D552B4FF4312B546680E2AF496C6FD8D125126B9606476CD3CC9BDD9AEDD2D6424FBDF936925E5CB46D03D70EF64D1A9F7B031A24790B8EA91AADED9A9A411A8CA1A02DFE8602B4E8DD9A227BD84CD238A377D0647E5548D2940D87960F03CBA4CB027B46A85FED025861F1CC977F835876026B9C96A3AAB58BA398B30F4CC9E45DF10C95A7E0CFD5FFE35B4A401B250F2D79368D5E3D3614E0EAD0A529F463416073A4AF0971152CB8FA092AC7679C3B368D5ED6C16D93DF0EF4689CB814EDC9648B4CE7E6358B6D69D3806B6E7ECDA21C023D09B4DC776A0E159656D224BA44D066710BFA9E4CB654776E5EF7B887BC75D0EB1F5065A19A2E621400CF5700ADD33D72E1D57FB027A321D5F8279C9BAB69E7340D845A16B0493CAFE4AD5307DD0ED1333336BF97497DF32525A76EBEC588F76A3E1D385ED97A6984F987DAF84B2A16BA76E2D134DB553FCF3409F5F1BE2526031CE6D84375B12ED98644E270F569CEB1BA3DE2D3C7FBDB20DA550CB7491ECBCEA359A27BBB215338AE9D9A47C528865B270B62EC6687720A89DE2AEB1411B811E6E0B86222F142AE2B79A85ED67975CB9969907B5FA010D69352B7BD47D3E44E484554DA008D03FC79D20D0367A16C6B221549287CA8B7401AB1C3BD495A4DA20A341AA07973A81431522668B62AC369BA2909804A18858A1D27470F9B5C774AC3457A3A9EA6D49A7CE44375B382FBD466EB35D5C4A00802E8D9BCA927063E9A1DD03865C03BA6B2E269A8A6BAF2734EB33837E0386DEA861A3914FD9A3B877B510C42A66DB6DAD9E8DDE4C95C8F8A285BEA366BC200056BBCE4842207117613D680130681A65885E4B031BB44D3D5C6AE2A8C927773A7B777A57180A06D4BA398413EDB3A30ED0C270DC4383740FB35C17098CAB367779A4AC32773002AEA4C0945081D16716DA810CE45DA5A75E097604D86E5447760CA47DC056C95BC6B227D8469CECCFBA160140E800BFA681DC146BC785EAC21569C04F3918031C484B2EF25912882357AB6532A6CFC04659BA1ABE1819A3BD5C9053E4480B67D2A2FF07E37903FA9A86AA9FA2277A026C3E72F1B00CDA94A1F360C7B7EAAF64BAE2D076AF8B45B91E22566A0A99A9BCE4C75B94B6C4D752537D4669E8C808BBA8AB6CAAF9E0668EC1C9310701955D15CF9F5CA00CD15CF31B79B58F213CAF2E63E3F6A1186CB9643DEF3A3F6187497807FE21935DAA0CB7C8DD2AA497D7E748DEB9A6C51FBEB0C55C96684788E313314936F8EA07D998BEC26EFAF997235EA8BF4D9BD4F1CD5D13AAAA393B24E6EB010E3EC18555592E1E6FE2D4A77B8C8F9F6135A5F64EF7675B1AB7193D1F6136BCA92BBAAAAEF3F3F12EAFCBC3D6059856802AE66829B80DE652F7749BA1EEAFD2A4A2BCE5E9041904BB0AD5C377D59E3BFD1E6CB80F436CF0C813AF60D7777DFA36D9162B0EA5DB68AEE914BDD7EAFD01BB489E22F38FD3E59132D2303D17704CBF6E76749B429A36DD5618CF4F82796E1F5F6F37FFC7FCBB0E5FF65A00300 , N'6.1.3-40302')
END

