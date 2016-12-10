-- ========================================== --
-- Current Migration: 201612092208001_Aphrodite1
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

IF @CurrentMigration < '201612092059142_Aphrodite'
BEGIN
    CREATE TABLE [dbo].[Account] (
        [AccountId] [bigint] NOT NULL IDENTITY,
        [Authority] [smallint] NOT NULL,
        [Email] [nvarchar](255),
        [LastCompliment] [datetime] NOT NULL,
        [LastSession] [int] NOT NULL,
        [Name] [nvarchar](255),
        [Password] [varchar](255),
        [RegistrationIP] [nvarchar](45),
        [VerificationToken] [nvarchar](32),
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
        [IsHolder] [bit] NOT NULL,
        [IsMinilandObject] [bit] NOT NULL,
        [IsSoldable] [bit] NOT NULL,
        [IsTradable] [bit] NOT NULL,
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
        [RespawnMapTypeId] [bigint],
        [ReturnMapTypeId] [bigint],
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
    VALUES (N'201612092059142_Aphrodite', N'OpenNos.DAL.EF.Migrations.Configuration',  0x1F8B0800000000000400ED7DDD721CB792E6FD46EC3B3078353371461465CBB61CD24C502265D1479468B664F95C294AD560B386D555E5AA6A5A9A8D7DB2BDD847DA5758A07EF193F847553575140E4B6A00F915904824120920F1FFFECFFF7DFA9F9FB7E9C11D2AAB24CF9E1D1E3F78787880B2385F27D9E6D9E1AEBEFEF79F0EFFF33FFEE7FF787AB6DE7E3EF8BD2FF71D298729B3EAD9E14D5D173F1F1D55F10DDA46D5836D129779955FD70FE27C7B14ADF3A3470F1F3E393A3E3E4218E210631D1C3CBDDA6575B245CD0FFCF3459EC5A8A877517A91AF515A75E93867D5A01EBC89B6A82AA2183D3B7C5BA0EC4D5E3D383D79FDE0ECE583D3E78707276912E18AAC507A7D781065595E4735AEE6CFEF2BB4AACB3CDBAC0A9C10A5EFBE140897BB8ED20A75D5FF792C6EDA92878F484B8E46C21E2ADE5575BEB5043CFEAE63CD114FEEC4E0C3817598796798C9F517D2EA8681CF0E4FE238C79C3F3CE0BFF5F38BB424E504F676147F3B60D3FF3688029618F2DFDF0E5EECD27A57A26719DAD56594FEEDE072F7294DE2BFA32FEFF25B943DCB76694AD70ED70FE7310938E9B2CC0B54D65FAED0355BE7F3F5E1C1114B7EC4D30FD42269DBB8F3ACFEE1FBC38337B82AD1A7140DA240316255E725FA0565A88C6AB4BE8CEA1A95B827CFD7A861A65009FE93BBFA262F9B82ED2789503E1852491AF07D35E6D9364AD201AF2EF1D03C3CB8883EBF46D9A6BE7976F8E8F1E3C38397C967B4EE533AD8F75982473226AACB9DF62BAFA3AA7E916F8B148F4B2222EDE74E3117DEE114EB4A13B815AAAA469007EE7FF7C81A88FC3979E32FA3AAFA2B2FD75E1F326ACE15DA24151E2044D8CE2F15DFFB3E44BBF0284DAE93B8F95A330C151F247D63FFC137D15DB269F0B94FBFB889CA28C6A3E7F0E00AA54D89EA2629FA11D10ECD8F54A99765BEBDCAD371DC8E991F57F9AE8C8918E4B212EFA272836AF3EAB5433C7D9D6F94F5A38B89151C73A535A48AD856F11265515A7FD155912E265671CC9556912A0255F1E9D1388F286717AA2F8DE7978166C11966A883CB1CC310CF36CB984E6C3A98FAFB5314ADFD143441F97B92A6FE28977992D595274E89A5F9439265440C7D809E47F12DB6386F3D51927C5346C5CD97C9A7B0E7BBEBEBE7691EDFA2A13F9FE7789C479975A55FA4783A1C40BED4F6B3BF684860F61DFF608D739A6C32DA98C2AC4B5D0CA8BCC69CCCAA40FC39FB1CDF44D90605827B89F587B799F432DA26E917B93632A1BE427FEE5055876A5699A06C1D1613ABCCF538B09B69B04D7232AC7FC9534F0DFA4B99EF8AB04D7C1525E58B3CCDD9560EA94E0D25D4ABFA4B8A04CC26D50D13953916B760CDC670AFD11D4ABDF40E41F9A3F0EBD35785DF487C550462C9AFF9277F8EF420BE5C21EB396C2126839A725F197AB72948832EA28251920EB31386F8C31BE11FBE081556F7212CA716E95D8245B7F6444AB2248DB2F5797697D4A166C98B7C57A19364FB3A1F6D32672CCF21EEED9330FACA6F3BDC17BFA0FA7DE1DBDEAB68831A11F11B3257A8D87942ACD2BCF61AFBABE264BD4E9AD532D720877E5C152140F09A0E79B5E95D94624BF9755E21BF9AB438AB5D593266920FD887C4D32EFD709354F8B7AB12903A470657B6BF73A9777AC89D4BBDE7C4DAF7B5BA6D16C6401D4770BEF058555919C193232D68EB72E256119A9A0BA5A1AA73851475E74B0676E9519E38B0CA50BE201B60215BF1C0F3221E5A79F945C35FAA1CC4D9215BC1D3B18C2D372F9A4D0765FDDA2250D5488EA2564DB64B858E0D6A742CAFD2B1A64EC70E952AC02AE17458CA980C41BCD85C5BB96A0C8634A9EAB3ACD60A175F18E2195B46C13CAEA02D17AFC8E6EE5F99A6C64329A8AA5DA6A28E7D89303EEE4E59DB3BBA1BC205BDDD2E4EEE71A1F4CB2E593BB9B6DFE499DE6B68EE4BD7986484C7BFBFD96D2DD7568E9B595E7339AF02B493BE699DE5B6478B27AF2B942FD4132C04D5D1786CD90EA9A547122565B603CA5840C30CAC93BA8EE2DB932CD9469CA7D9C1E340361A7C3D272FA2AA0E541D0275767D8D62CFAD068213A259AD4FCC0B25C0F64B9EAEF3BF7C397BF922AFFC56EBA7D136DA78B2E3745706109310227296227A4BCB89231D46948660CDABA46E7DF71E153AAFD1D661AA9CC6A94B3C87DBB12E4E401DC6C99AAC7CB0DE2FFDEBD64396F14D40B88B6893C4499405035C91234CDB2080053DF4DD44629E835B65127B7ACDAEC856AE9F4F12C579B68ECA2FAE7627641D60CB2A8B3DF54303E4AD215ACBCF9F4D2D8E7F757C01DE179B325AA3CEDCF4E16F8744D728D84A23B0F9CEAF8B9536BE7165F3EDA75C55C7365FA81A4996D5A8C9B3ADC89B22BEC0DF37609B5092AF1C5740524DBE946D85573779A1AB2A5586AFE49025A9DE98EFE7ED68FBCFDCC941CA2F79928F7CDFE9145F4FA89E48029EE00BB3F80961D5622332C02C13D2DBA2557D2AB522F153302AC76930089AC3785870940B0E10AE262E43058098EBD8EBF8697F1BEB8ADAB4DD13B11F9B07CAFE98AD9AC4E4A58491A1281ADAD7683DEBC2A3D8A49A0EE3D96928EFC52876F33EDA8DA3402EC82DD95327472593923A07E062BFB7DE4C7F2F598BD3D9F05E48CFA32A894F4A14F9A38471DD3550FE2BB60626C03AE9459A57E8145D23EF156D73533423F7A77C81CAA44EE2885B66BB795B3B28FF29A5F5095E449F6927981714EB4F73852A6FAF509554B5BF47A21382D37CEDED166E91420CDFD3AE69412494030BD0D2806E677F117D89D57728610873E6FBBC7A955775321E6EF7386FFDC725D522075F62003778B2B9A94331B8F13007916AAC935E79F206435CF842B4560BED6F6B4FDBD0E9FBE8A67E939F6C3625EED53B741E8F8B7E57497D43EE3321FFF9FD2AF29CFA0880B73FB63B98C36D9C3A1DE7459EA625B187791BD56DC07CC09AB60C358EADD59274A1768A7FEA56976D19704949B20497239FEF7086CE70DD4B9704EB371650D5922AE550570C6450CFA694AC8EF897A67EA4C4144E706F2782A2DA263E71E3D5792B84C6EB72527CC11539F9BC8B33ADA7F35D811BBB9DB7EDA1791F2D4BEA2C6C4E3AE084397880C70A211798686448009F373EC38D6B0F8E309221A8D02151706C8D39B63EB7AEE5B2E3C724AF01AEB8E3C7630674FC98CAB5AD90B9FF5237C3283C955256196B96B6E78C350B29BEA06671F3F0CDE7D76B5C442116E12F509AE6D9EB3BBFE55380B373017D55D42D6EC7737C81BD5DAF77F1ED95EF75B57F067F57089FCB9EFACDF6DCD915E29C6683F17B94EE3C4D94BDF2BC9DE1656941A0E83BB4CDCC3AE438394088472F4443837A0693000C63023938D15FA14D3B297A1FD4380F157BE6BC6AA695304059B5DB12127F2C62961561A08857388903E0E4297509DA1DA78F66F0F6D37F519AC91D6F85EB158653EFB0D11504099BB8ABDD276F471EC10902825577B2E6FC812E314B8837FED7FC538853E5C18EA7370EFE10FA767F770A5A53DEEF007A8FE2BB20C040C166E0CE503DD96E73BF3A61FDED3B39118C0B7F8CE2C613C2B70A01E7D8597672F80B070E6AE9F2AE0832D2300E6E66D316CFFD97F52E466F8BBC8AB2600AA58915D3746A08A5D9A00560FD70532384025EA1CF7EF4DEDB55ABC27BBAF506F81025F5294AA32F7EF2D2EC9785E815BF8D37FB8D338DBF97DF2F113DC1361EE7F3AC6D5525AF0A578CAB13930B578E2D122C6C4813F3A3F5BDD2EEE82E117045F739B66EE82B142712B77883D7E7739C699361967479F6612D08997AA7802E03D688E101502B867BA63523F743D4F51A4B70B5EA33E03A0DB95E9B7FB4085ABAEA7BB26F1136A0CF84D8FF7B8E21D6AE81668385F838C51A7EE36930929BE59F9214CB5E80307044F8F03C88FAA8982DD61825738ECDD0ABA8F434DB68DFA21B82B741E1B49924D574ACB44AF55DAF363EF2E559ED272906EA425959EB4B9686B1678CA385C923CE8811C5C26C87DBD824E006396CB638CD2DEF2B22470EB30B4B186A7E39781E5588F2E533539FDA3D7DC90D54CD4A5B55DC987B1F10961537FEF1A4CB735074E4986C51DB53BC2D9AD75EB829DE791BDA75CFD86C5717DAC3F3D95C76DE4B76DF3A76DF2976DC18F6D80776DEF6F5D9E5F5DED475DDF274DC89F4D87874D967D4D0D8E9DDF3EA6C5B8C4F580C7B343AAAC653694925DF5BF0D94A70DE39F0F0CC7BCC6536BE251F57929A9639B20DAC8BE4E61E3565C0161F55E0A3381F53B69FAAA068052A4B7B995C6C93CCA3495064DF96F3E0B8F5BE6EE4BD5A0B70CA8617362DF34C87923838A61B4E42DC59A3C1E7349C56058A938844DB75B0C245E2A9EC7081971AC70318104FE7AD709978562937171852594EFCAB826D91C98C4368CA5B4B0A960566440EB3E1AA2086961DC5C832B3F28042D39210DBC58E046BDC0CBE38ACA36C2C023989F1106E77498C072D29BEE00C483EEF320BF6746A3F6FC0DB2D24A0C30D916BD6CFED1E1DE2A6354FE9077D3DC1421C951FD1F4175884B5E8686A3BBFE074F6E76544BF1234D9618BF38AC87C88B38EABE645941779212EC46C4FACA0AAA2B4FA646DBF42314AEE02C4426F5B4EDF8B68ED1E92D29A7F4E88619FE26B31A778FB6E440EFD025E8B1CA67F9AA358BEE1A7DF25753A8160DA3BFD357BFDBCAD2C9E02B0D8616F0689EFB320F22D11F6D910E3FDF56E70F83C9FA2AE93DF8ABC3F03616C81B4040BDA20DD6107072B64A4548FADA96ED92E1895BBBD48AE8F6B1966974F7DB006DCD7E30EDD981F3292DEC2EFEECE8B756132A09BAF54AEDB912329773A54F8A88F9029A847B18497FBA0679EC5EA83102CBAFEE8C5D87E05623600C2B8E34E9328258F8FF90CD970F7DF021CCA3CAF4E9366D73B80057E91DF35F688B7259FD47500A0AFE4F9572CDDFED3C4655E25F4F17BCF2337AA37D1C4C828632AF81A1A172DC5B40EE6B1084C62B528E21138D64F7148D57006138D68687EB33910AAAA4D9B2FD48524CB6AD2E4D9D6E31D4A519197B27EEBFB832A25D469CC94D58C2AE13B93DA4DA3CBCEA18E136808CBD968F68CEA317E6A9245E4201BB578C5FF8417AF1A15BBABC6CBA46EFB53E4CFC9DD3D64B49CA469FE57C84772D547070DDFA8040611F082A5C5E4A052CC9DCA07A27CB13960ADA689EBA59DB624557109E1D505E051CCA05DF6C7A68810D7A7CB9105F6E9B36D67AB4BAC2C23D9B58EE2639FCD72A74D05B9D365D972A72583DF801D618FE18A886FBFD279215F2CED1F1AE5BA884A16FA87CEB337251A5A553828B688A46254AEAC7E7411DB6AEA6776F9B4AE99D3434EE8F621E147A265A7F7AE128EB33C453DC75279FF56965FC98230D80319F32E0AF513BE6471C8DB03932C108D838EAA178AAABADAE8A8C144B0D1511DD1BE2D4164254975275BB0680761FF752B1817D93733E780390FB2F62CAD4C833AB5C564F56A6C0175DD9A22BE73725B0B5B615F56D23DE4D74CF8C26D57759FD42C73A5EB6FBBF01939A97100D7346B915ADE42BD42B8DB4D69952101E08BF81766E1572543870BCE3AC9DAD16060ABD790CCD80FB854E9A1F9A2220FB935898C995C31FB851E252A9A1A3325A10A530514F5A54B79A94E9E8716BBFD34E1A2BBFEFC28B7DFFD57EA89A97C99E83AC22C09608C74489EEB8A0EC577B3691247A8B5D964EDCD1037D4A50E0F4B65AB56FFD63A4C168F5BA2EA6C2B2BF59CB9E82F795D212D17D2AF26672B5C42EA6D3360ABADAEB557B2CB6B570FB53AE7E9EE60A143026865DB59457357D0AF32B65ADD3BD2C420EBD0D9C57E70C98F2FF6251CD498ABBF5D320338A97E737FBBBDA692D5D3C08835D654FD6E8DB1A26A0916D4536D055CD4D448E9EBD436D552A7A8AA93AC0D7C18C2FA1BD0BC2DC001C9D30A0CE7BA6FA53D009B5A204F0EB5209ECC69C7F9E46E43F5962BE42AE776632DEAE0BCDF2AAF067CA0DE5883D1FB84C65A6C245A50938D9570D1662CF55CEEC1F36C3D063A75F6A80770CABB1C65DF871D3AA7016EB4170E8D3060AB3CDCB17AD3837792E3F59ABA99DF80BFB17A1C8E145F70D03731311D867B4F3797E9126888A16C47CF824ED75F663B72675F538FD1A33A442B1931CC015B9BC3BCD2CB282D2210DD95C91056226CAECBF162F9A39403B6F010259B23AF54800727479658E995859F87EBABE0AC5F3ADAB9748CFF9B687B1329D5483B6B30A868ABCB84EF091B6CD537A4347847505049DE771A0C55205F1B58413A2B9B4ED959699B86666175D3D4C155DF0CC473299C20C39454DA5FE72C30DA3DC6866E2E968E0E76AA36AE90DC3A68F1C02AB159629DB8FC0077E62D4D849168F9BBF38E66024B3D97D7616FEED09B860F0833479A3D07A1B84B1FFAC90CCBBBEBE256ABEC76BBD3187C196D93F40BB54F653C1039CA054723571397210940CCB50B7BB2AB6F723A1E5623256D85A83CDBDD890893CF1559E9EC33FE957061031DCC82B6D1BE9BBFBFE64916242AD75594DD029D7281B69F50D9664EBE332C8C4F6887982BA4D829E64BDAEAB5961EAC330FDD171D2B0C97107C1092625EBE88BE3296DA6D71A5E6AECBE65461ED17C36A01FF48BCFD689D470762C055F2DFF7C1496B5D4B8D3618462A7C76CF592FF05A4CA33EEC2AFD3ADFA8AAFB912EC5D794CA94282FBA4400BDF59AC4C0B1545D986671ED85EBE0AEC03AE2797598B5EDE13E531A8B183C0E20217412B19665A99D8C8D440B0A99A378CD2C5827714C16DEBE56ADEBFB70E7C5C97A5D222A56E954130BE62B1D7164CAEFD07EBBC96E569135435547DBC27E2121D50A9D30806AA1CBFB480FC8512F88B98262008AD84E8EEA6509050CAE48A07C6186040B794D92BFED92F89604E0C739A58D91CF127E7B11C34EEF58DFA3F77E1BFD32AFFC007E3BF6A47F146EC362F9E390A63E087E7C412E08B68CC203C115F4325D2E5116A5B5A5793C122D79007BA884D3216C867AAE8DBF4086CCC97A9B649A956E98099CCCD767D9DA7EFA0670567554D6DE485DC7314EC52EAD1DD8D65B295135061499E106A189FD428F4AD17E1973A5F60B55C44E3DECB69472187CE72D67CFAB9769B4A9060618E80B9C9D640F181C4A6534B9CE4FFBBC27E1CCD32FB8BF685DCEF642EB68EECF9554444F378F433D3B7C28F41853B6196243E147EAC2CDBB98EBA1F4BF7FA72EFE1CB38C2EAE417F9FC579769D945B9AE658ECD0B6EB14DD493F85E0DC9723C8641DD9B8D4140CB988C88B0266FDF8126DE9D22E6CE39E7A70E61C83B318F3C85B4E97BBB23067E13FC8AB617FC958280877F3F49AD9B8E1EAA119366FCB28DB8CA5C5653B5B8F32FF6B1CC08FD5857F29111A0B8B0FD6081CC4045F86F23FAACB378F41316CF949C39624BBBDA206FB13037CBE93347DDAD090EF8C149A7E6D28583E1D6BBAB727193975ACE9E2D5ED178651C7BA4E4EA3986A82A69749C77142043C4E2421F93D2A9328AB1F8DA49A6E1748BF1B49350220907E3F926A8441207D3C0E468D4C08A43F8CA426C2C1B1F691897480841A211909416A8DC870D42084468E563B6CCF818406D2C40DD44726CA031AE18F3432F4F7E836E14934B233B68B23FC4E23393CE13848BED3080E4F398E91EFB4E253D7290238FA9D467CDAE29D888F541AB161A88E47328DA83064145B3482C290513CD1080B4336EA8CEF34A2C2908DFAE23B8DB83064A3AEF8DEC89CE8C87E1CC93492C290FD349269C484217B32926984A499E87819F95E232334D12822DF1BCC4AF0E73442D2105DE465330804628DA8D05F1CE5F27B8DA4D054A3587EAF11149A6A94CAC71A39A1A946A17CAC11139A6A94C9C71A29A1A946917C6C2124A3443EB6909251201F9B58A8BD6C3D1CC94C64A4271B65F2B185741C8FE2F1D8423C8E47F9786C211FC7A380FC602120C7A384FC602121C7A388FC602122C7A38CFC602123C7A390009B8E72B2514A7EB0909247A394FCA0919261852228AF1F4CD736822AF941232B3CE5282E3F98AC7728CA51627E3459F55094A3D0FC6862E05294A3DCFC6862E65294A3E8FC6862E75294A3F4FC6862E35294A300FDA8112041124621FAD156884629FAD1528A2895F3A3A518516AE74783A512BB80FCC9527E2865F593A500510AEB274B09A294D64F96224429AE9FB44BEB816AA4D1480F45D33E19DED3694487A3636935B203D08ED2F393467A5EA65175C3BA663442335030957CA2F342D25463F59E688486211B05FB89466018B251449F688485211BC5F389465018B251349F686485211BC5F289465418B251233ED148094336AAC3271AF9F87093D4A36C3CB1910D4A731E3F34158F638AC65436288FD44353C1A05C510F4DA582F2413D341589C7148D893C700EE1E38726D2D0FA32794A1381A028692E9A280D8A9466A6898450A4144F751E5B9E9462AD91EB966790CE79DB96170CC2639D0B97A1A3B8AA73E53274144B754E5D868EE6A74674183A9A991AC161E87EA0E8744E7D9AEE478A4E23320CDDA8F08E75DE5C86EE0945A71116B6DF2925A6F3E7B28494C4E8FCB92C2125323A572E4B48C98CCE81CB125242A373E0B28494D4F06E5CE38D45EAA577AF8DC50167B18DC5A11627A61B8B03C573D3CDC581E285E916E340716ABACDF8262734B26D46A38EEDDE54F1EC560A65B14E7D9FDD66F43EAA4EE944659D51872C34DDF9E6D2B81F3F20527DB31EECE310C23D282C207272B0BB7755A9CB9EA5B43DA85316282AF271175733AFAC0A142754AD75AE36B45D1551DC9DA1EA0D5407593DFB7397145B94D59ED2CAE02C783424C93E10BE1B8BEC49B9CD8D05F655549B0AEC2F697E872A53917D9EE775652AB12B14E7D93A2ABF702DD508EF1B14DFA6516C2CBF57CD493833E97D5E62E014D5A6E27B1155B732B1152C5FAC8EA92304BAFEDCEEE87AE86CE355414DE2EAA22FF2AADE6DD16A9750F09A7EED6868B9D119C26DA7AE6EA9E367BC0D6C34B05FE00543E539A8078CC506F4C9FA0EEB148C6B7C726FF5575EAEB751663AA64FCAF88642D708C145B449B09ECE6403DBA86B84C804CE1DC4214DDB4D6C43C136B5BC6A2FF67B366A845AB055CCC962E7065128CBCD8CBB9A3A52A61943DC0955A35393275595E3914118D18FC426BED1706FA18D94C47EF52C5B1FD06191D8C2E3C5A7F1AC77176FE902332929305BB0D443435C033C9C9B1E81B97A725FF837E10B57E81A95E4D87F94BE20967A897BB3668A1C90C3E649162745941AB492A33D00639D8D51BF8EF86F1D0D1FE3734E51414EED66B5014BBC6B317C8C65D7918E5F4F8F28093212AC7CFB29D7C9132913528C1A3C487A9ACA4C2A347453669715BADDF741449AB04EE4CD3AA9800C2520F168834A994BC70806C846538D694443688449DF78CB84D05A93AF8E91CF161388730C116531FAF83CDF65EBF1EE9EAA57253432A1E98BEBFBDBEC33AA898AFFC6C3070F441975162B4D8D4CBA5C114FCC5ADE34FD60521D9612AED30C92F8821C49CCDE1624E9235EED95E4B6DA20373221515241D24813D848A3FA43803C0A4D10F426CFE5B7E4B15554A38393987C057F33AAE226E22F572B6C68AE0388B351938C74989F1C1BF5A1493D78DAC564B99909E8E159A9E75BA668B08997450544D44E33BBCFC460F3E69B92413EDC83B9F9224AD2A6F252E1194A403243326D34DC08261195E92656E1D3B30887C03D93AF9ED47514DF108FFEA292517CD4DB694C29584204035CA554583C1B132C9032011B64D269DDA3465EB202B4DDEBD3338909FEBFDB3155CAC9582C90A0508080A450B59A4E54C436CD272B62F3EF89B090777134FDDA3C76134A489AB7A64001695EE8995238A876CC2A185493EF8150904EE92AAE120CAA984438BA12762609830BCB895EE85C8504F8F66C8202F0F39E084BE302932F7898520A5131B23665B0F3BA1BC12619F616210C232D6CD3BD3F3F83C0704F25AB348C5814121DFE1167734D03E0CFAA6DE4DF9F47E3C8F96BF2FD53741D61AE2CA97CE897C5359DAC911E07B1594C5E161394FB342F499E78D7F5A9ECBDF72002237925DE5897051623B83A261DCB5206122E98F5A1AA33A32964287270710BE3C8C088F616B990EE3E558D66973A35F7EFA5D4D5BBD242E8A8D2C1658EC6DE0B91032AB4A0C401AC37AB0D45B8B4BC75CB541361A38A4EE5259049DAB413A9BC0EB3AF03011EDF87C520A976774545E5326C8B04F23F766080B4F4976526F33FB2ED98CFFFC836D96C7157D549D674D5D2FEE9B6F2C70602721C54428E171491E3A564E4D84648DA3A2E2D1FEF508A0A5C79CD66D7582C9094508080A050B59A4E58C436CD272F62F3F7DD2570D1EEC2748FA92ABA762C25111560974A232D14246810B7EFBB4E252862830CFB0A13FA8B8AD876DFAFCF71AE9E79835ED6B3F093F3F4D9FA1B71034279B41E7EE19E0584CEE5843A5F0F35C8682EE8DE71F7395E0FB5DDEFDB730A8AE64E0FFCFC7A0851915EE3A1DE649F5658ECEFEE049416FB2B3B4B8A4BA70F9BCED6CC176D0B034E400DA0444EA69D7CE8A6CC3DF5D0ADBE07134F576B33AB962D19545896376FE1C6CD2D3ECE76EE9242D4995BED5F4A0B462809EF7941F6A94A884458A9D93BA135236D9C99BBB5A1F2DDDC92F0C1B706735D68D0AC96A832C1AE302CB44E029A62D24961AE2BD8AF8FF6E00EA1897EE1CA051692C5348BA459730B8CAD46D907A1D12EAA9952C10466C14535D8A0F944C56549BDB0A060DBA93F49AFBCB8CE958384652C6279B894C79EF778A9A465267D37927ACB8F84074616B0BA0AF30AD185FE420C58DA4EA00CA5E962D95B32CA862E275F2257EEA394A96ED20825A792AE25AED7481BB7A84459DEBAC1C5F74392C67FAABDCA729249648BFF0620647CCD27973649BB97133B0993C25468B68852A61208960E18656A6F444ED95093CE0D1983CA43C6968E46D5F14DBF61C6960B2852CBEE99C1CD9A5B80DC36CE16169DF12EBE612045190118234816C7402550D20F2C155451D762936E0E15AA4AC79C00759941E8DA08AC1FDBBF5EE71BF92557A12424666DAE8D8C89B080700DB913C995B46D269DD852794A93940DBE35984D86C6F1D00981BAC7F9E27269B208C1A2FD8854B6263A43AFAEC6CCD22563F93D1031BEEAFA4841520AABD951193E4FFA05A994CD10D0515B2BF3DE0E3F534ABA2258956690C45F5086CA28C52ADA205C15541892BFB19C8D0082E8CB84115555654EA34CC5F0FB61908DD53ECFC8830879299F4981B213D8FE23F632F11615AD5CC6DE171872DF24AB09A3A8EFF826986078796A604107BE4970474F11A2DBB48CF4D0CD37724B109C7D911AF93D35AEDC447203DE5A9B4D70ACEEAD4D23395637D8AE508C92BBBD109EDF76497C9B26557D96D546331A4B308138711F00E48AABF3D41206B7781951839973DFE6B93E988D5E18FA282DE1C5AC4796078A985CB0B8C62D23511C1FEE87289DC471BECB6A83959E501212A5AE908D2089B80B04299636CEA4173B624F4192F2C1BB0A338A11B5CCD7F537B5900D2B4814302049368E082F5112DB37BF2C89ACB84FC2D43D3567224C63D1C0C2440143D124C61A4E2B4C62FBE617269115FB274CD898C3ECC73435A6189E00EC9E2924C9E8730D3C76F8BE42DD7B87D5B3C3BADC89F76508EC0AD56CA30E0FDA0C48D404C96211A8094DC0A0F24C51BA2D693954574083278331A3EEDE8D132BD1A66BA8855340028E50C2185109A6C5694F890B086DB286B6BD9520D0B6C906B4A3E711C418B3757DC3BCA024761193ADC16ABD1102469BACA1ED6F8009D47D86F6DBED1958E0EB6D869E1E2636A1944B139DA9C719C2A5413843A6198E1CC4A02FD8307E409FB005CCF0E4405A843E3A9300D06768E8E95BCD02069DA9D38337E0886F930D6825A37ECC32C090EAE231CF68AC49EA42676A70849D5D014C286184280532A46FCC2E094493A741A19702020C9DA9C1E1BD6802165F40370A28AB521C0954268743D944DC94CE9FB73BA08AD253BCF25C1E631F2B9F381E1A34581482E9A741EA6D6D0A4963CA1CB1AD37E74C6B9C4819223ECC0BD59E799AD7A5F9CC5BBC74AB41DBC9BEB1E31BB24053250FCC32F5149F98A56A09A9110935D046D090726BA2EC555449A38D1E51151AA27B4695638CCC4633C6550D86007C53BFE10970CEE2D14FD6656CF4EC27DD4A85556A810CF04F693B3B8E2EF6E948D930533C30298E18F88949EB8107BF29391D3BC63710012E481E48646A2D3E914855165A6748A8254D0DD244FA313FB099D2D7FEB8CA42EFFD31CD1594A30260625D71C13E4E2769B7ECB6AE506FE0A2AE65CB81BBB82C826C59E6DCF66695296DB7707F14AA317D75D4BEBDF4ED50961A5A003BB5938A482D692B5D425963AAA0A4CD9205AF0A086E7EB8B677AF61C99B0E3D970555987B304B6CB849ABB9E7B1C25B4EC0934E40D3750F3F3155573CFD44D55FED60D0014E2605F4B3447246987140D3749336CFD358BE2FE4ED56BD8F02D65FF2428A2337244FA2188B95875AD0F34852523FC6F53CB2D1188BF2887EC744C922E983279236414F9E3833087AE36406FE5C50EF6E68A75633CE5085834DB132B604E547FF7484C49C821E96100C22EE69094B738A7B4B82A296789C7DDA79AC6EE8B1514B8FFD9A7A3C795B69373BDC5C596854A1CA406454CB460341502904C58E8053DBA9E8F470D365E1EBF97A0301ECB596BE0203546FE0769B83179589B30EF951E581D8594728188A9DF6A502BB2F0A04A0D1D2FD178F66CB1DEA8AB8E270B5E54E74E3964BBDE7F25D2367496F6B25957331423624A14C8C6C27196782626B98E6DE58AD5653C57C86AAADD56D86CD9F51C3894189C1458232723167D1CB62176B35950647AAF1020D7F3AB4AECCD3AB55F450EC5D6BDFEE0C2A9E0F15AB6EB0A6CF65F1641D1B3E5F6FABE638455C54B1E2AA39CEACE933CD717C104FA0E1CA389F4CC565913EA9AACB8F642991A6F38BC10128D56C30F27DAB63557AB1644687B8183951CB1AB57B5C1E60D1972553FBCC15A1FFD43C51866993354816ADCD8B4BB2406D2068D83318062C33086AC75AC4CAB0760E273416648F7A99A10AC706B443BDD03064C66C4B0D69C430E85882517431F6DC802EBE98C936AA19E21C279AC45057009B34F1B098D6C8236251CD80CFEC69700076C80FEEB9724208D82465873AB413D016697027A1416672A389E6A465B7D7D8122AAA1A5CCAF04492B1200B50E431BC641189CCF9EFC03230340EC02D7D081DA655CA203A5483E46752F570531F220382BA28C54812FB45D2DB62F4170FD111C3BD4C77148A0B49A2648918B444D200266C8907239838259A1356DE6D07773F94B13714B50677415C9A0FEE87046F3F7F005CC508551C0949432491243C5823091D41216ACEB4FB706B380CA0621318F940D21A3EF6810763F86007FA330C0EAC102FE503ACD0DCDC67EA2FBFBB4FD55F7275510334F1BC025C2C57F04276FD1C6C037001DD911BC08D73A359DB831FF43510393F6437A8C1560077A81DF9015C9AA6779BE53758E4FC787AD4220C577B87BCA747ABF8066DA32EE1E9112E12A3A2DE45E945BE4669D5675C444591649B6AA4EC520E5645149351FEEFABC383CFDB34AB9E1DDED475F1F3D151D540570FB6495CE6557E5D3F88F3ED51B4CE8F1E3D7CF8E4E8F8F868DB621CC5CC3AEA2957DBE14BD8EC883688CB25D73CD6E8655256F56954479FA20AF3FEC57A2B14E32E32B3CC1B58DC7F8CBDAB2C765B7FB3A82F4FFECD5C987E707AF2FAC1D9CB07A7CF1F48504626BEC4EDDA62B3AA6922020447A0C4B4AB384AA3B2BF342EDE187F91A7BB6DA6BD48AEC0DAD5377989ABC2618DC9E65867DBC62CA071BA24738CD75155BFC8B7459A105EB1607C9E1DEA0A555542AE49F0904386391EF993056A53CC112EA3AAFA2B2FB92E1C53CD91AED026210100884A38BF64F1F83C73D4DF51995C277143F92EBF451CDF806C11FBE91127FAFC083B128618A7EDF8116B349E55F3BDCD8896E2188C6905AD8CE34CA8239AD7CA184872BCA05A22AEBF3F459100D5A7DA21FDBD71E6F1487F073DAB6AA4CB3CC9EA4AC4EAD32DD04A3CE97E48B28C18650C1C9D618EF73C8A6FF1A479CB828DA9164849BE29A3E286D3D054B205D6EEFAFA799AC7B788EB4A26C31CEF458A151627AD6D92058644DBBB69FAD3649309B3D990683397E57512E75905B24BCCB540FE1CDF44D906C1C07CA639EE4BAC2384F96D48B4C111A26CB388DA20DC3AEC2BF4E70E5535C800B884C517CA04656BE517C012E65FF8A509C6CA62F669162879CAD5AB4DB14028F35DA16A2858C01CFF559494182EE79A4A25DB61ADEA2F2912B1BA640B2C54E658FCE0360B9976B8AFD11D4A45C42ED90EEB8F4204FA0338A5A840E1112CA9610E150EBCF935FF04B0664CB547E2B943A7DBD9F578999C00567D976C812536D0BA7560D31CDA85CD4B5EEF764956187F08107F5822FC4340F8871D02D9EC87AC3436C716F15D8225B88610FB1C0BC4244BD2285B9F6777490DCFCA922216DFC877153A49B6AF73DE2264732C103921BBB0922FFFF56BE3DDFE05D5EFB97AD0E9166BD868831A59E096AF63B2CD7AB8D8F1386D9239C62ACD398836C502A13859AF13627B01ED12326D70413C7B9C3AAA3911E892CC31DE4529B6CE5FE7150744A7DBA2AD766509185842A62DEE077E8EA092CDB13EDC2415FE0D2A093E6FFFFC21B2C33E4E4E1110CCC633220190B19E9F0FED26C3D0CE15EAA1486608C9DF8F5CACFB43F4BA6B675BF6B10B57A50E9DBA8EE2DB932CD946E2B258C8B473A1F042D4A759C82336562475E3B2EC30CFAEAF51CC3B52A8743B3461B074697628EF12DED81853E77635E13576FE17CFF021D502E9F2455EF17CEED22C9C55D1165B389CAFAA4BB340D99580188DA916AE29407AEC25E72C45A2376F48B4C68952884D42A6C5C238A9DB1BDACCB2B84F34C72107A1446535A62EB0F4240B952D5F2336C76E6184494ED6E430D8AE4425504D5919FBAF94F18DF20B74BE35FA45B449E224CAE4F85C09EB2FACC8B6DD56F505AE84CD524FD4357DDAAC5B96651273105D92CD422FE347729764617BA138CFD651F945662E00F996961DB60A33BEAD4C86259EA86FA8649BA50C392B02F090C9B0C5132B47A75BA089389608EF8B4D19AD5177DA9F466273AC11C5AA31197B63ABCB82A95AADD0200C9385194CA7D8B6FB940B06629F6861A74B2C74170B388CFD824D01C136F84AD786FAEB623692A7413390412D827476630979B904B22D66CE8158EC543ECF6626E4FD6F5796EEB77B2967C144CC4BBA5C042B64E79F6CC9191CB2379B94BC1F91CFB37579000B6526C316AF9B2921C421CBE65C4C95C427258AF8833143B22516BCA6E7B22C31010B8B4EB74403CC193ADDC6059257E8145D23C13665736C1C22982023070479403AC302AF4CEA046740F6339F678F2AAA4C36C7D6F172117D1657CB5C963526B402E7B26C30CBDB2B5425FD6D2B1694CDB3406D05E5345F0BCE2726C71A1154157C9ECD59AEB66DA0C80B99CEB8101FC0128BB9C044C167322C4E47E12945264F7CDE322767CEAB57795527FC49212AD9EAACC91FDC81E82E6966D75CB2B9A9656C17326DCE426C92181C1C6C8E0DE2E757973C549364857121625CD861B46695B86C6732E6747EBDC94F369B1277D31D3A8FF9C5AA9069838B273004181C4C86CDB282978536C50E4164FC986A7312A3B9EF27EEFF301936671F106F2C7749E618C45287ED6E36C7E24C00D6C1A56C740B99E6B8BCEA82F5D6420B3A492C2B9BA51C0461B08883C9A4533DFEC93B03FA34DBE51AB44CB3323BF04FC830A6D397D87DEA221603070EFB646BC52D564CB3505E488C25D1E86CC41882301063984CA5B5446D65BD1205ED7336C76255D63C29F4FA8EB38EA8E49937F727581D0BA7DF5FD89E7C9F6A85FD7A17DFCAD7C363EEB755B60966790BAED8988CAF70D57E7FD7D7610EAA3414BF47E98E5F5DD319F77FD57F868DDA82108AC79AB92C3B4F02D85E266371CF4402707148B4C0F1BC897385364D8C0B719F93CFB3F19480C790CF5DEE249E57CDC426620DC9765859B5DB92170645B831C7069198C80504486558F998509927B1E062EA52EDBC55A970587D4CB541EA2F99BCFDF45F826E13736D9057B83610F3C6741BB477D85084D0C674BB45CC6AF7497437301976783098D3E1BE284DD6A2F782CBB2F425FE9A7F929FD2A333973CFAD7382541F5CEE67CA53ED0760D23F830FB64072C6199C4E658214ACD033ECF0AB53964B9DDE602E49861E30628113F67F6697628C23DB72ECD0AA5B811404892CD114CFEF8A51DB57CFEE7F3EEDB81CECBBB021C7D74BA15DAAA2E51B6A96F04B831C3C603BDDEC5E86D91575126D53ED24236DF297675D383A01A06B22DB181AEA2D31D0ECF82DA5DCCB541FECC837D9EDBBBBF2A8093B69237EFE428FE07593F44497D8AD2888B4342255BEE2F80BDC5E6CCB763B1A0B356F188BADC698B6D53FCEFB7D7FF027B6F7BCC7F7574E3CAEA24EF00BFBB8C6176239E6382B5F45AA4986BE1480C7CD7F2140BE386BFDDD4A559A0ECCAE85392E28E032E500B99760B053CA6518DE095029DB7C4EECE55540A5BB9A5DD94E17D193ED8CD001644B169E1ABA2ACB781DE5764ED2B57057BA949F8B31E96073DF8531E32EA17D8A668A21D1C342E55DC712CB378D7705C26DB248B6A78A765F6AEFD80B0DABA7F9D2B2EE66C5771D032D87E0DDC52BC2D9A1894FC8CC0E57D8D676181ED0F2EEBDBEEDFB7DD3F2DEAB7DDBF494FC5FE13ED650993FEB1DD06C0D9B6E0036C0E8936382F93CFE2865397F8CD556D8638893358B0EBAC0464FFFC15325CB3139690012B98847B6EC2AE0A142711798CE49B11FBCD88FD66C47E33620DF0BE19B1DF8CD86F46AC06E79B11FBCD8835475CD28895F8F6A1E06A2B87A86A9806920A2AD9060BDE9D4E5DB6A5535E57B429369BCA208F0A071E1564EE14714AABD0CF2BF8C4C5CAE5C0C5AA80395DB870BA20F3020FD4A6D9A008FD55D8F61770697748B4C2217A52C069136D70F04A2A832FC7F37936A88DD2E0E1BA44B3652CB02CDCF385AC7A0DBAF412566030FB56EEACAC758D40462D667D039129A00CFA4A4D3E8DBB21C46D7FFFCD6EE0468AF55D14DE43C5F305CADF1B2194BCF36C237C108481D0C16472D33511428DF569163E2E124BE98654073CD923E4BA205F45C01B8F42AE0BB23C3C149BED822D9E80E1F36C3C1AFCE2E2D4FA1AD565243E3B32A6DA2C1B89C08AEBC63ED50669D53C88F0222F84D52C9D63B17042552518BB43A2CDB1D5182577E221303ADDE67069D316F19A3093618B073DE5C5E6D8224A1ECD12325D708107B4844C5B5CBE77C6545BA4E64C3D0C3764594CA249CDB7B54BDA9B490A0B7252D81D4415A72918C460A292112A866322C6781853E73E7E1A347CC59B2206A25774A97B23306D95BCED1A08C4C8B28109C3B155EE4B8F52F29439BBFC69D32C66DF8057BE81A3F94C86CDFC8B1772C48E16E6F231DD06ED22BFC39DC863F5A956764152D700D4906C35C0BE8AC7E8B0E082315C6D15CE655E25E245AE31759F148EBFB6715335567AC65BBAB0251F09B6BD5528D98B5DC5DF07EF922CE42BF2BD90B7BAC98B9334CDFF122E42D119FB245F1741A23ACB81CCA44D4AAC10BA8E06903D2AE7FE4F045F8BF2BE700C2CF63528F10ED17F904980CC0699947862D1B38E4DB76C4785E925F72EB2EA9F7071FF5A22710A64326C86271986809DCE64D878A39A88A392268BB936C8F5AE940373997B23AD6C93BDBD182A30236F861A60FE6E3D45D7D12EAD011DC6E65823FE01A259CDA61DCD3F4024BB2591B1C9BAAC9886914F77C1B4964889282E7B913AC4843CDD88E30686D588E0860238061692E0CBBCAC23DFDD4C18C4407E6584F2799794E7BB754CB55152554D4E3790A0279012E5739D9005654AE738210A4A95CE596A39B8CA77658C00363219B6787F4058567C6B49FE01E15871CBFCC8C44283F81D4A51810781B7A7430E64309855C452D60E34BCECB03916A29DADF9483F5DD2EC5E89401B1473FA37967A04FEC63B5E3F046120B43099CA032A28B91BDB78FD014503653BE0159221756EEF3010DE6A48DD2B610B10595F066328747611F67B0A50F8BA740BCB3D40E4F8BD0A01146458FE330612F21842219E6495E2180E22CB6758071248581C9E5E0D24770E0FA5EE830C2F7AB0298002970319795EE4C472E7444F2339E864ADC8F7EDB093FD31AE8584E865B44DD22F83A3CA5392346806E2A44590719C23E4590F645B48D7AEBEC98183D963B2CD19085C0FF0F02E9B637156EA33FE958817CDE87473B4965130FBECB8F66B9E64E269EE31D5C630CA6E79C32803EFE22D3A8A820C1EE731633B544274714B23134031D71619B8E7C364D8E281E38ECBB259BE7E5E25FF2D5E856E13675D720AD590D561D101F23ADF041923008EF13001693552986FE0C1D2654CA95C17EAAF5F9A50F7A97F87C9810C7A4C452C6331D059D6DD7412C7C474E471A8E4E53607CF8B93F5BA44FCFD1A2AD9E21A65BE118F640E895638E26A6948B4B94DB245551D6DB9571CA8E4BD1920BFED92F896DCC8C6F9A5EFCCAF063318283A00A93079DDC20DBEEF1DE00590CB9C1B184D8239FD6FC72CF96F56E1487E7BC4513FFAE6B36077D95116A5750043400E64305C54C452C11A68845D7726679959E664BD4D32D18EA492AD4EE96366F10701FA443B9C551D9542A4B321D96258B73C06196FE797892AFE30719F36F320E9335FE4591D25D8D0E18B0C5FEF5286DF559F40E49AC4A1CBD728AD46BA557C83B651D3E2AA88E26653638D5E2665559399FD5354A1B6C8E10166CF5DB2462556355FAA1A6D1F90020F567FA62F52BC64ABC7021751965CE359F85D7E8BB267878F1E1E63D576922651456E8FA6D787079FB76956FD1CEFAA3ADF465996B70FE93C3BBCA9EBE2E7A3A3AAF962F5609BC4655EE5D7F58338DF1E45EBFC08637D77747C7C84D6DB239EBC83354279F8A447A9AA35E30CA7740B3BF6D8E1FFF4EFE80BDFC3BDFC5CA16B71D81E71BDCDD33F958F78529767879F924DF38C45A3A05A6BB746EBCBA8C6736746A663D454FCF0E0CD2E4DC971946787D7515A09C359F8D0E8996A3F546DA3346D3E658974B62591163A94EC2E2A633CB71F1EE055EEEBE695272C0A8F1FD3A8752906C0E0415F4755FD22DF1669D2C62E6AD1D7B8ED75F3FE86651D09DC0A9BBE8DBCB4582E4D6DF564D8965E4655F5575E0E1D1E08F60A6DB0B1D7BE45767EA9ACF4F7D6E0BFA332B94EE206BC19EE4AFCEF1EA9F1691DA91C92129FAFD9A054589CFA61C9104F3C30651A80577C3F3767759E1DFEAF86F0E783F33F3E0EB47F3B785B6295FCF3C1C383FFED5083FAFB5314AD7D460AC1F83BD970F4C4681EF1A9BC504A6C037C48B28C04817087791EC5B778A6BCF5C248F24D1915375F42EB90E7BBEBEBE1ADE05E6AECEBD7C5DE6801EA24FBE2D248516B3BCF2CA7C926A366A81245A9C3EC94D7499C675500FE9C7DC69D956D5000A897589B784E46C07E95B5C21030648AC3440A5BB02BF4E70E5B81217844F604D6E1F0FA48307EF2FD4B9E0A9CB68528F35D11AE5D54301ABFA651D1673C81509963A10AD2380CD56DE5F8D7E98FC2B3E75E153E23F655118021BFE69F82F0A3C7F1E609B1AD5FE798D4DF4A0FD1AE308DEACE30F31398814A6D28BDECAFF6C8B2E7DCD99E5AF6062117B2FD6DB016E75DD24657F3C049B2248DB2F5797697D421A6E18B7C57A19364FB3A1F2D3B271C2FBD402F2B0359848DBBFF1754BF2F7C9A75156D50F78CA6D7686A1E38F6C4681DDD7EBA6145B6C09AB0004CABDCA0FC21EAE6908B5F93DE452936B55FE715F2A94A8BB2DA95256323B9437D48BC6CDB0F3749857F5B8F707B5702702CD6CC9FE0E24618A7945D9660F32F699C04D709E1B8ED32CBCBEE37B0F8CD44783C78EB30550ED4167530EE61E78E951E26D6F7AF821DB65E0B121DF5F6244BB611BD4E74C6231E02C8A0B1953A3C9187AB1441EB03BF05800AD4BEF681675FA030AE14BCD6CCFF0AC0E8CB1779E53D7BF6AF24F87A75766518F109243AC3FB087EDCE960A234109B5E2575BB33EF57ADF168BA677DC22DCE8647B1FCB03A9893F51D66FBAE4465901AF6A8D80A0F8BD83CAA93445948CC15D938DA86C22C6825E12C2793EC919549EC65DF5E11B7ADF7EA01C579B68ECA2FE1A6F906091B6359ECAF301AAC102AE35D546E501D84652D54904A05C0E82E07B696A12FBB3B30BA5E8658E68B947CFB2977DAEB24844EFB9C3D2135D4826F7006B31F031900AF127F8C3D5E08BD29E22EB2A2F392888370912C0062DA7DF4F183CEDDC242782D94AF2857CFD72D647EF2E5B6EED6F5B5AD866AAE85920DBAA41CBD4FAEB34EBB940FB2206CA1863BEE7E60CFA32A894F4A1405010AB6566DD082181F0D5298C99E7DC4D9178C7EC0D917AB7B0D99B5219D1D0ECCDBCABEEB7CF655E53068ECD2D1038D7DF8D8178E7939380C58A0512EBC9CEC7D26077A2C79AF5C3041A4977F18D9D7C60C758CE1BC7A8557E9C97846C3F10CC11FC399D0E59C41FCDBC5DEBBD5F4B3C5DE609F5F79B108035CF801B4164D8885EF143E9937F9C96653E2DEBB43E7F1B8987311C737E4881E0A32E75F45FEB320C108C1F52E842ABD9FE0B6D38CFC8D506217F306ADF3D810DEB8F6C4B3D145C62B92D3928F5667B61621742E0BDC9E8EE74570F74917B3C65DA0484D597B31ECC682C15AB427F63D27453EED7E4EABA5F639F2EAEB6130702F78DC9710432E990D01B7457890A577B3660B64FAB6CF0EBFBEF33654C26CE6865D420638EE3BC53AF4F52EBEBD0A70ACE99F6A251A68F9B3D78BDA7BB10C0D74A0A081E9DEF9769F60F770517C860DC882A085388C4956D881DA187AB19E8461D7785BC003E20A6D5AE331C48ED979886B50E75533FDF88364D56E4B48FC7088395BF8C3100F0D7925D00B234FA9D3B36E18FD19F7B79FFE8B52466E582B5C1F7FCEBCC3C698370A364557BB4F2196D5042A140E56D2C99A5EA0BBDE8121FEB05FF34F818E36853C26D578D902E9D97DF7D8B516BFF741A81E28C0D20163859C803B1BF664BBCDBD6B86357880098AC05C048121519C7C51025424EC843B85E795390DE7A8B02EEF8A50E30E43ADEAB2698EBFC774BD8BD1DB22AFA22CA49E69EE1FB54F148551AA0D60887E18CE1406D2D02BF27A8B2744087FF3AA08314587C0F81025FD63F021DCDE81FAC9CF856EE5013CCFBA6F38780217BEE4E4EFEB7E8E01D67E77A578081F7FB15F4DC8AAFB245B13BAB1127F3B38AFDE378CFEF9E02561898327FD140BE2C67FB223373B3E25291641EFBB914470F1A84535A2AD73E866B909DB97DFAB689F69F1E42FED75B13BFA0708CE7108C10115B463851E85A850A0AD8371EB1FEA2CA39D216F048D916FA46E74EB217390B7451349CC48B71BA11A6E871861996D64184175FB0E5A87B20D98C1A6880D9C6E25698265BC256201A6DF11310333D91031453233B18CD00C771A8CB0CC77431CE08254D1D06B611417CAC8D16F8A14AE470DDCFC463097C7BE10E7D5D9B618039D710E56338497C967A947DE28E2A6999FD0182A5C3F997A09CDB04CFD716693BC77D71B2E2A8DA1C2B17D3C9525FA364CE8570637A10D71C2F4FD2AE5F8EC0EE46F1FAE0A963B8E7D4E60CA5B7F1096C3CE38610479551045EF0D3276923384EE9CB3210AD187DE28AB3ACF0C6E05988135BAC2D7BE4AAAB84CB6E451F5F13012E8E43E7EF453288F16BD1AB9871EAD2007E743B845BD8FA77C4051498AF7EE4515930C3C0222DC14D7142F48A87207A921742E92D3D3B1F368F023C1E45AE00D51BCACC3D4EB9AE14D6B24D121E2FDF102B967464067AF1E0BE1E3D33DA50C49E7D097677F5E467474C4507B85E7D5DB020B9AE7519D551315EE455EC89628665BABFDFB79619B78856294DC39BAD44762BF38682D83429C136EA1C2C42A6EB182C5071EE18244096EE1DC76423AD2009DD69C3C08103AEC5D52A79EB26D3C8DB5AF04BB4C64B2F785F553D948C9336ADA1B2EE1035FCD783FE54D1173D154CC6EA63474D3184004DBCD04EA5B636F04017CB0DD5A4CA234DF780FD2A027BCC39C2A38AFF03A8A94F79CA32FF2BB24DB78CEF3495D7B827C0B9CDD1CFB2A629FA833DEAAE732AF12FAFC9A9912B5D1228E2AC4517F04989FDB174B5B103C3B7F4AB2A8FCF22FDBE8F3BF5A7B7D77D578507C5F0E05AE6EF2E2244DF3BF2609978CBBC023BCCF48EDD8FD14B5C79192BD51B4DF74640332FDDDD93DD195E4574895292B49BE1350C1CE258F7DB5DDBEDF524F642B379E56B76EF3E80C1943422FB4BA4F41D391D3C47899936E0A631677C133046E58F97658081F27DF15AA77A5676D188409AEFBB3ED75F30EF04CB7F712A8BBCDFA60EA75B44B6B7705450378E9A90EC87FEEEC8002AC31BCAD485BC9F210290F599A7EFF643FDED25872169E51DB1AD5C77F98D98E2FE3B170999775E4B489D852BA8C8491925AFE841E05A7A8AAC95E3EB925E5A16F59104F9D3B8085D0BB0398BFEE0DB3965CE5BB3246EECCA6E8FD36421A1C7F16B738FEDCA5CF37841EBEEF508A0A3C9CDC3C1923B5CB3066A9A7B6EE1B7909B1D45F726D38D706CAFE384ACC1FB8BA710B7047E85CA4B7A79B700A5AB6C351B60B71B26A2AEFAE7DDDAC44C935585C4FEB2C521DED846215E4F0C3F2FBC961EE108AA3D8C4D268A8FC8C8C0021A3420C4FA723685623C9FDE5C19ED8752C714F6B4C32989613A0D95FBFD81721B63C17E4AAC9476A8FF3418C36FF7646C8E0F0A2E45495D9E1C564AACD8F97D13649BF0CBE2D1779E2205C840A8098D61578B2AB6FF210679E4F235CF1894EBC9E7DC6BF12E419C0B9E5AD93AFAF27F512FD5FF3240B7276FA2ACA5437A0FC4781BBF0BBCBFCF4A2DE7E27A42C05B9DCD2424D347230F52AF9EFFD5CE259D7CB528A5FE71B7741C6C4EEB2DC11CF21CE73A93363DEB72D4C1D99EFC8F659187E12C7C40673E2F840EB35834CBE6F6774C3A63859AF4B345EFC08A5107027D2671603C2D22B10AF031F2436535547DBC2720E371E3DBFED92F8364DAA1AE7944EF3F0C2F73DF7646B394874CDCBBCF2C6F84D1930C10CE291BF130C0CA4B547BB5497288BD2DA75CA1EA99D369B19EA093D49CB4F2027EB6D9241C69C9336248B99B36CEDBD9E2138AB3A2A6B6FA4AE2B7DCDDD2B1455E3F9D8B0678D4EAA2A8F93A69B693FDFA8FA206F2AE6F2C1559EF685FB9AAD507AFDA04BB9D8A57552A4498CBFF9ECF05868E208C17D89C6E2B358D07F1340F1104425190D514AA2DBD76584D92D8ED7248B93224A992670A560FFF0E801150E041F0DB07CCE292AC845C7AC86DBEBFDE1019FD3333A663C3DA2FADE482480E7D0C34A42F3014600DA94AFA3DF45F6ED697713CFEA47E0FDB6B1A71A1737DD516D82795737E834409B3049478BFE7809DFBDBB58E499E44BA3E37BB10EEE037D70F175A55D3E96003534DF770F1F3C50F53F13189917A43163127990B455D2550AFFB9956CC843414B3E2C864E5E4456E8803F429418A582B0EEDF639E2F4FDF666D10E28393987C1FD726AAE266F798B7AAF1A7A5924B872C628497C9984CF758F5BAA794C9C33349BE0785115A6ED2A1D9554D37FB2CA97DE69B8DEC656FD95989046BFA086C9E5B76BC66E6694250D1186DC23DEF6B31B096E44B7CE8A6857ABAF8A8B738C8954DB6A3040BD1685D6966B004EA72E19EA9A41F64174B1DD6917EDF9BA9BBA537D883F537F5050E6548BEE73D2EE3E1FE76B918EB26647713740EA149BAFFDD2CF06D3FBB98F0BEEBE909BBB9FB0A8F3224DFFFEEEE9B724FBABCF10DC9CDF3FE0234D05F96B6DA9C5E22F0DAB6BC0F4859CF8E37F616293F3743B7B3574BA71CEDDC157A1A89CFBAE7A35E152C40F251F632FE9292308308407DFFB574FADEEBF9BE9B95112D028CDAFD92038721A90BD8319974987C784683606A41D1FA748C2D8EAF4456427F785659A102F17C13953D13152ECCD21EAC2C956262D19D7BBACA9C79D161BFDA5C5A143E82115F82D99B1D3A8DD027DD736B13E29BE46362EC99857BFBF85B774FD8DD4CE09BC57A5A1A0A265867535FA051E8E47BDEE9321EDA7E709E2E7F53C41FC1B7131CDCFAEAB563F30DD6686C93A6EA6F0B8F3D18C6DF72A528F2D0E17B739C1CBDC98B8F92F026D4D9D11BCECFDB269877F8F0051E043A2010EAFCA810FF47D20570C01FAB0E873968FDBD393B5C736E3C448F03478EC7D4AFA2CF2DCE282FD9E99D6A0722620556EC4662734F95BAB16C2DACD2BBCE36B3DEFCBB7C411B6ED6EEB7B7E4961482D602F9280D78E3608BE90D3A6186A79327F2DD191B5AB297BD1C0C3BE3995EF5CDB98EEC6AEC79EFB3BA73DBF2F39DD9B410AE3DB82F6232D60375F6FCE37CEE4E37FEDE3E74BC76F9E6DDED4B2CDFE6EB72ABC5DBC21D8E6D8AEE88A9FA02E0588EE9333A796F4F7A519534E991B1F87CF703351F9C571064C78E7DC4611F0F722F2717B607BBF74C3A54A7BC434BC6AC67BE179508D3D5E7F0D6E5D29230FE53EDFB0B2A12FC4761C4297D82CB098992E1CE9F9F2D9E84A9BC04882CB1BC94CC1D6DC24536968E3BD18A85C1E681BF402CB47D30B310586E212CDCFDC38546D3E04396373CF73908D13231291C42112D1F90A28D16FA71086D2ABF88D4450FA63BB34F32978C3178AB88D3A44E220F50E063498FC8221D5B49822444ADC327679380515D8051A227110358E308795F9148D829A685058317097D8C018F6ED578B496093FE0DE69C1E79470DF9E4172C630D1068129C286C2A22254D37074F2576472C8E271BB7D76569D729EDD61C85C88833C8941FA2D40DABD920D31FECE34623167F4A265C4C03892D18A90ED4BDFCBEF2B7CEBFC293AFF0AC528B9DB8BEE57C6C89F440EB82FD2787CD657241B2A3EBB7D7A5631016F774F231FF35F9D5F46226C2ECC2F2F0A5DC87C83F5455792E9BF216DDF03E0F51535E914EA0582D9569CCA6FCE2806D2878982CBC1520BCCF925C17271B92FA2207D6D24B828505F626EC052C95F8928C878BA17A270D63CC581696A4C81CA7E8ACAD7E8655256357976EA53548987AA09D50AD56C9D0F0FCE86973D78C958C537681B3D3B5C37EF2AB46F830C998294B0F0D444217C80CA833E41659B7EA4DB5C937FA92BA0FC5C5746F34DD9A7145F3003EE5EAB10DBD0A683556FB334C0C21105E1134209E8634221E3CF2ABFA8FB98F63BED594FE10B6D3284DDE66850DBB3C3026A9B0CA1B63906A8A3FB0D441FB3655F194BE8248A89502F0A16930DCA175342F3B5D69D207CA54D86D0DB1C0D6A7F5B43C0ED3320E43E4F5BE3F6F41F50E73603AE759BA7C78681A5A82690F20145674A3E603AA0E8B835D077864CC977867CB3EFC83FA2FC8281E4B0819E0009620BC092C49631FBA6FC63CAAF68E1FB5823027A9F0181F7791A6CFA4AA4804F6742DFA0F375F3E70DA8AEDB6470F6BC3150D7E3750F1059AEB6C75C832F48E7FE314FF60DB33993BEAA24D178F2A6D0F99AEF089BD3C2C78412D0178542469F957E4DF51143EC661122816FF2E45F68B2351FA157BDC257E84CE83374BEE63BBC2356F8165F00FA1E5F46A75CA8659CA860A84C50C950F9DC77A8550C6745F327F50EA8A2B455AD3CD1C7AC5099018ABF26B1BD61479748AC591A1CB14D336F766BB64B5B0B3DF5E6DB487A71D1B60D5C3BD837697CEE0D6890E42D38A66AB4B66B6A0669308682B6F81B0AD0A2776B8AEC613349E38CDE4193F955214953361C5A3E0C2C932E0BEC19A17EB50B6085C5335FFE0D62D909AC715A8EAAD62E8E62CE3E30259377C533549E823F57FF8F6F29016D943CB4E4D9347AF5D85080AB4397A6D08F0581CD91BE26C455B0E0EA27A81C9F71EED8347A5907B74D7E3BD0A371E252B427932D329D9BD72CB6A54D03AEB9F9358B7208F424D072DFA93954585A4993E812419BC52DE87B32D952DDB979DDE31EF2D641AF7F4095856ABA8851003C5F01B44EF7C88557FFC19E8C8654E39F706EAEA69DD334106A59C026F1BC92B74E1D743B44CFCCD8FC5E26F5CD97949CBAF91623DEABF974E07865EBA511E6EF6BE32FA858E8DA8947D36C57FD3CD324D4C7FB96980C7098630FD5C5BA641B1289C3D5A739C7EAF6884F1FEF6F83685731DC26792C3B8F6689EEED864CE1B8766A1E15A3186E9D2C88B19B1DCA29247AABAC5344E0469883E38A89C40BB9AEE4A17A59E7D50D67A641EE7D8142584F4ADDF61E4D933B2115516903340EF0E749370C9C85B2AD895424A1F0A1DE0269C40EF7266935892AD06880E6CDA152C44899A0D9AA0CA7E9A624002A61142A769C1C3D6C72DD290D17E9E9789A526BF2910FD5CD0AEE539BADD75413832208A067F3A69E18F8687640E39401EF98CA8AA7A19AEACACF39CDE2DC80E3B4A91B6AE450F46BEE1CEE45310899B6D96A67A3779327733D2AA26CA9DBAC090314ACF192138A1C44D84D58034E18049A621592C3C6EC124D571BBBAA304ADECD9DDEDE95C60182B62D8D6206F96CEBC0B4339C3410E3DC00EDD704C3612ACF9EDD692A0D9FCC01A8A8332514217458C4B5A1423817696BD5815F8235199613DD81291F7117B055F2AE89F411A63933EF87825138002EE8A375041BF1E279B186587112CC4702C61013CABE9744A208D6E8D94EA9B0F113946D86AE86076AEE542717F81001DAF6A9BCC0FBDD40FEA4A2AAA5EA8BDC819A0C9FBF6C0034A72A7DD830ECF9A9DA2FB9B61CA8E1D36E458A979881A66A6E3A33D5E52EB135D595DC509B7932022EEA2ADA2ABF7A1AA0B1734C42C065544573E5D72B0334573CC7DC6E62C94F28CB9BFBF4A845182E5B0E794F8FDA63D05D02FE8967D468832EF2354AAB26F5E9D115AE6BB245EDAF5354259B11E229C6CC504CBE3982F665CEB3EBBCBF66CAD5A82FD267F73E715447EBA88E4ECA3AB9C6428CB363545549869BFB7B94EE7091B3ED27B43ECFDEEEEA6257E326A3ED27D69425775555DF7F7A24D4F9697BC0B20AD1045CCD043701BDCD9EEF92743DD4FB6594569CBD20832097605BB96EFAB2C67FA3CD9701E94D9E190275EC1BEEEEBE43DB22C560D5DB6C15DD2197BABDAFD06BB489E22F38FD2E59132D2303D17704CBF6A7A749B429A36DD5618CF4F82796E1F5F6F37FFC7FD7DA6331E3A30300 , N'6.1.3-40302')
END

IF @CurrentMigration < '201612092208001_Aphrodite1'
BEGIN
    CREATE TABLE [dbo].[CharacterRelation] (
        [CharacterRelationId] [bigint] NOT NULL IDENTITY,
        [CharacterId] [bigint] NOT NULL,
        [RelatedCharacterId] [bigint] NOT NULL,
        [RelationType] [smallint] NOT NULL,
        CONSTRAINT [PK_dbo.CharacterRelation] PRIMARY KEY ([CharacterRelationId])
    )
    CREATE INDEX [IX_CharacterId] ON [dbo].[CharacterRelation]([CharacterId])
    ALTER TABLE [dbo].[CharacterRelation] ADD CONSTRAINT [FK_dbo.CharacterRelation_dbo.Character_CharacterId] FOREIGN KEY ([CharacterId]) REFERENCES [dbo].[Character] ([CharacterId]) ON DELETE CASCADE
    INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES (N'201612092208001_Aphrodite1', N'OpenNos.DAL.EF.Migrations.Configuration',  0x1F8B0800000000000400ED7DDD72DCB892E6FD46EC3B2874353371C6B2DCEDEE76873D13B225B7D5C7B2D52AF7719F2B07C5824A1CB1481E92A5B667639F6C2FF691F61516E02F7E12FF2059F27174B4ED02901F81442291480089FFF77FFEEFF3FFFCBC4D0FEE51592579F6E2F0F8D1E3C30394C5F93AC9362F0E77F5CDBFFF74F89FFFF13FFFC7F3B3F5F6F3C1DFFA72DF91729832AB5E1CDED675F1F3D15115DFA26D543DDA26719957F94DFD28CEB747D13A3F7AF2F8F1B3A3E3E32384210E31D6C1C1F3AB5D56275BD4FCC03F5FE5598C8A7A17A517F91AA555978E73560DEAC1BB688BAA228AD18BC3F705CADEE5D5A3D393B78FCE5E3F3A7D797870922611AEC80AA53787075196E57554E36AFEFC7B85567599679B558113A2F4C39702E17237515AA1AEFA3F8FC54D5BF2F80969C9D148D843C5BBAACEB79680C7DF75AC39E2C99D187C38B00E33EF0C33B9FE425ADD30F0C5E1491CE798F38707FCB77E7E9596A49CC0DE8EE22F076CFA5F0651C01243FEFBCBC1AB5D5AEF4AF42243BBBA8CD2BF1C5CEEAED324FE2BFAF221BF43D98B6C97A674ED70FD701E9380932ECBBC4065FDE50ADDB0753E5F1F1E1CB1E4473CFD402D92B68D3BCFEA1FBE3F3C7887AB125DA76810058A11AB3A2FD12F28436554A3F56554D7A8C43D79BE460D33854AF09FDCD5B779D9146C3F4984F2D1904AD280EFAB31CFB651920E78758987E6E1C145F4F92DCA36F5ED8BC3274F9F1E1EBC4E3EA3759FD2C1FE9E25782463A2BADC69BFF236AAEA57F9B648F1B82422D27EEE1473E1034EB1AE34815BA1AA6A0479E0FE774FAC81C89F9337FE32AAAA3FF372EDF521A3E65CA14D52E1014284EDFC52F1BDEF43B40B8FD2E426899BAF35C350F141D237F61F7C17DD279B069FFBF4ABDBA88C623C7A0E0FAE50DA94A86E93A21F11EDD0FC44957A5DE6DBAB3C1DC7ED98F96995EFCA9888412E2BF1212A37A836AF5E3BC4D3B7F946593FBA9858C131575A43AA886D152F5116A5F5175D15E9626215C75C6915A92250159F1F8DF3887276A1FAD2787E1968169C61863AB8CC310CF16CB38CE9C4A683A9BF3F45D1DA4F411394BF2669EA8F729927595D79E294589A3F265946C4D007E86514DF618BF3CE1325C9376554DC7E997C0A7BB9BBB97999E6F11D1AFAF3658EC779945957FA558AA7C301E44B6D3FFB8B860466DFF10FD638A7C926A38D29CCBAD4C580CA6BCCC9AC0AC49FB3CFF16D946D5020B8D7587F789B49AFA36D927E916B2313EA2BF48F1DAAEA50CD2A1394ADC3626295B91E0776330DB6494E86F52F79EAA9417F29F35D11B6896FA2A47C95A739DBCA21D5A9A1847A557F499180D9A4BA61A232C7E216ACD918EE2DBA47A997DE21287F147E7DFAA6F01B896F8A402CF935BFF6E7480FE2CB15B29EC3166232A829F795A1779B8234E8222A1825E9303B61883FBC11FEEE8B5061751FC2726A913E2458746B4FA4244BD2285B9F67F7491D6A96BCC877153A49B66FF3D12673C6F21CE2DE3E09A3AFFCB6C37DF10BAA7F2F7CDB7B156D5023227E43E60A153B4F88559AD75E637F559CACD749B35AE61AE4D08FAB2204085ED321AF367D88526C29BFCD2BE457931667B52B4BC64CF201FB9878DAA51F6F930AFF76550252E7C8E0CAF6772EF54E0FB973A9F79C58FBBEFACA81D51CF181F2638515C504978EAAACADFB694058DD35AB7BB316748595D56FCA98D4BD2D685B716E29A4A9B9501AAA3A57485177BE6460BF24E54E04AB0CE50B020E16B295713CB963FD90975F34FCA5CA419C1DB2153C1DCBD872F3A2D93951D6AF2D02558DE4286AD564BB54E8D8A046C7F22A1D6BEA74EC50A902AC124E87A58CC910C48BCDB595ABC6EA4993AA3ECB6AAD70F185219EB16514CCE30ADA72F18AEC50FFA953F54329A8AA5DA6A28E7D89308EFA71BAB177D8F7B4FBE0B8EFEBE2E5C0A741E672E45BEC1EE84CF39454202C1ED9346C5C4394D348609781F3C85E303B2BC25E2A1BC20545D24502C7BEFA6597AC9DE4ED5D9EE97DF2A164A3E1F1DFDEEDB6969E0BC7AD622F23939F9BB4D6A8699DE546718B27AF2B942FD4132C04D5D1786CD90EA9A547122565B603CA5840C30CAC93BA8EE2BB932CD946DC3E8E833F8F6CE3F9FA255F45551DA83A04EAECE606C59E1B79042744B35A8FB3174A80CDCD3C5DE77FFA72F6F2555EF9F9C24EA36DB4F164C7E9AE0C20262144E42C45F486B113473A8C280DC19A37494D9B3F4E153AAFD1D661AA9C66CB84F8E5B7635D9C803A8C9335599263BD5FFAD7AD872CE3DB807017D1268993280B06B8220704B741000B7AE8BB89C43CC722CB24F6F4495F9183127E1E7F14E7D93A2ABFB8DA9D9075802DAB2CF6D40F0D90B786682D3F7F36B538FED5F105F8BDD894D11A75E6A60F7F3B24BA46C1561A81CD77DE61A3B4F18D2B9B6FAF73551DDB7CA16A245956A326CFB622EF8AF8027FDF806D4249BE725C014935F952B6155EDDE685AEAA5419BE924396A47A63BE9F1BAEED3F73270729BFA4BB8D7CDFC9C5D613AA279280E763C32C7E4258B5D8880C30CB84F4B668559F4AAD48FC148CCA711A0C82E6301E161CE5820384AB89CB500120E6F2458F9FF6B7B1AEA823117B22F663F340D91FB3559398BC943032144543FB1AAD675D78149B54D3613C3B0DE5BD18C56EDE47BB7114C805B9252756C841E4A4A44ED9B8D8EFAD37D3DF4BD6E27436BC17D2CBA84AE2931245FE28615C770D94FF8AAD8109B04E7A95E6153A4537C87B45DBDCC3CEC8ED445FA032A99338E296D96EDED60ECA7F4A697D8217D167DA09E605C5FAD35CA1CABB2B542555EDEF91E884E0345F7BBB855BA410C3F7B46B5A1009E5C002B434A0DBD95F445F63F51D4A18C2DCA838AFDEE4559D8C57473C6E33FC7149B5C8C19718C00D9E6C6EEB500C6E3CCC41A41AEBA4379EBCC11017BE10ADD5221CF960D2F7D14DFD2E3FD96C4ADCABF7E83C1E17FDAE92FA8EDC1644FEF3FB55E439F511006F7F6C77628CDB38753A2C8F3C4D4B620FF336AADB80F918352790C28C636BB5245DA89DE29FBAD5655B065C52922CC1E5C8E73B1CEE345CF7D225C1FA8D0554B5A44A39D4150319D4B32925AB23FEA5A91F29318513DCDB89A0A8B6894FDC7875DE0AA1F1BA9C145F70454E3EEFE24CEBE97C57E0C66EE76D7B25C547CB923A0B9B930E38610E1EE0B142C805261A1912C0E78D2F17E0DA83238C64082A7448141C5B638EADCFAD6BB9EC5C3CC96B802BEE5CFC98019D8BA7726D2B64EEBFD4CD300A4FA59455C69AA5ED3963CD428A2FA859DC3C7CF3F9F51A17518845F82B94A679F6F6DE6FF914E0EC5C405F151523C1F11C5F606FD7DB5D7C77E57B19F49FC1DF15C2E7B2A77EB33D77768538A7D960FC2D4A779E26CA5E79DECEF0B2B42050F40DF566661D729C1C20C4A317A2A1413D834900863161529CE8AFD0A69D14BD0F6A9C878AEC745E35D34A18A0ACDA6D09893F1631CB8A3050C42B9CC40170F2940A31E08ED3C70A797FFD5F946672C75BE17A85E1D4076C740541C226EE6A77EDEDC823384140B0EA4ED69C3FD0252210F1C6FF9A5F8738551EEC787AE3E00FA16FF777A7A035E5FD0EA0F728BE0B020C146C06EE0CD593ED36F7AB13D6DFBE9313C1B8F0C7286E3D217CAB10708E9D652787BF70E0A0962EEF8B20230DE3E066366DF1DC7F59EF62F4BEC8AB280BA6509A484C4DA786509A0D5A00D60F37354228E015FAEC47EFBD5DB52ABCA75B6F808F51529FA234FAE2272FCD7E59885EF1DB78B3DF38D3F87BF9FD12D1136CE3713ECFDA5655F2AA70C5B83A31B970E5D822C1E2D934C1685ADF2BED8EEE120157749F63EB86BE427122718B37787D3EC7993619664997671F6F8590A9770AE832608D181E00B562B8675A33723F445DAFB10457AB3E03AED390EBB5F9478BA0A5ABBE27FB166103FA4C88FDBF97184211C645E33B0E15E2E3146BF88DA7C1486E965F272996BD00411689F0E17910F531675BAC3106ED1C9BA15751E969B6D1BE4537046F83C2693349AAE9586995EABB5E6D7CE2CBB3DA4F520CD485B2B2CE510E4385B193479C1143DD85D90EB7B149C00D72D86C719A5B7EAF881C39CC2E2C61A8F9E5E0655421CA97CF4C7D6AF7F4253750352B6D557163EE7D445856DCF8C7932ECF41D19163B2456D4FF1BE60A3B6B553BCF336B4EB9EB1D9AE2EB487E7B3B9ECBC97ECBE75ECBE53ECB831ECB10FECBCEDEBB3CBEBBDA9EBBAE5E9B813E9B1F1E8B2CFA8A1B1D3BBE7D5D9B6181F8819F66874548DA7D2924ABEB7E0B395E0BC73E0E199F798CB6C7C4B3EAE24352D73641B5817C9CD3D6ACA802D3EAAC027713EA66C3F5541D10A5496F632B9D826994793A0C8BE2DE7C171EB7DDDC87BB516E0940D2F6C5AE6990E2571704C379C8480C84683CF6938AD0A1427110903ED60858BC453D9E1022F358E0730209ECE5BE132F1AC526E2E30A4B29CF85705DB22931987D0947796142C0BCC881C66C355410C2D3B8A916566E50185A62521B68B1D09D6B8197C715847D958047212E321DCEE92180F5A527CC119907CDE6516ECE9D47EDE80B75B4840875B22D7AC9FDB3D3AC46D6B9ED2CF657B8285382A3FA2E92FB0086BD1D1D4767E1FEDEC1F9711FD06D764872DCE2B22F321CE3AAE9AF7865EE585B810B33DB182AA8AD2EA93B5FD0AC528B90F100BBD6D397D2FA2B57B484A6BFE3921867DE8B2C59CE265C91139F4FB922D7298FE698E62F9869FFE90D4E9048269EFF4D7ECF5F3B6B2780AC06287BD1924BEEFD5C8B744D8F76C8CF7D7BBC1E1F3AE8FBA4E7E2BF2FE0C84B105D2122C688374871D1CAC9091523DB6A6BA65BB6054EEF622B93EAE65985D3EF5C11A705F8F3B74637EC8487A0BBFBB3B2FD685C9806EBE52B96E478EA4DCE950E1A33E42A6A01EC5125EEE839E7916AB0F42B0E8FAA31763FB1588D90008E38E3B4DA294BC8AE73364C3DD7F0B7028F3BC3A4D9A5DEF0016F8457EDFD823DE967C52D70180BE92C795B174FB4F13977995D0C7EF3D8FDCA81EEB1323A38CA9E0337D5CB414D33A98C7223089D5A28847E0583FC52155C3194C34A2A1F9CDE640A8AA366DBE5017922CAB4993675B8F0F2845455ECAFAADEF0FAA9450A731535633AA84EF4C6A378D2E3B873A4EA0212C67A3D933AAC7F8A9491691836CD4E215FF135EBC6A54ECAE1A2F93BAED4F913F2777F790D17292A6F99F219FA0561F1D347C3C151844C0D3AA1693834A31772A1F88F2C5E680B59A26AE9776DA9254C5258457178047318376D99F9A22425C9F2E4716D8A7CFB69DAD2EB1B28C64D73A8A4F7D36CB9D3615E44E9765CB9D960C7E9C78843D862B223E4A4CE7857C4AB77F0197EB222A59E81F3ACFDE94686855E1A0D822928A51B9B2FAD1456CABA99FD9E5D3BA664E0F39A1DB87841F89969DDEBB4A38CEF214F51C4BE5FD5B597E250BC2600F64CCBB28D44FF892C5216F0F4CB240340E3AAA5E28AAEA6AA3A30613C146477544FBB604919524D59D6CC1A21D84FDD7AD605C64DFCC9C03E63CC8DAB3B4320DEAD41693D5ABB105D4756B8AF8CEC96D2D6C857D5949F7905F33E10BB75DD57D52B3CC95AEBFEDC267E4A4C6015CD3AC456A790BF50AE16E37A5558604802FE25F98855F950C1D2E38EB246B478381AD5E4332633FE052A587E68B8A3CE4D624326672C5EC177A94A8686ACC94842A4C1550D4972EE5A53A791E5AECF6D3848BEEFAF3A3DC7EF75FA927A6F265A29B08B3248031D22179AE2B3A14DFCDA6491CA1D66693B53743DC50973A3C2C95AD5AFD5BEB30593C6E89AAB3ADACD473E6A2BFE47585B45C48BF9A9CAD7009A9B7CD80ADB6BAD65EC92EAF5D3DD4EA9CA7BB83850E09A0956D6715CD5D41BFCAD86A75EF481383AC436717FBC1253FBED897705063AEFE76C90CE0A4FACDFDEDF69A4A564F0323D65853F5BB35C68AAA2558504FB51570515323A5AF53DB544B9DA2AA4EB236F06108EB6F40F3B60007244F2B309CEBBE95F6006C6A813C39D4827832A71DE793BB0DD55BAE90AB9CDB8DB5A883F37EABBC1AF0817A630D46EF131A6BB19168414D3656C2459BB1D473B907CFB3F518E8D4D9A31EC029EF72947D1F76E89C06B8D15E3834C280ADF270C7EA4D0FDE498ED76BEA667E03FED6EA7138527CC141DFC4C47418EE3DDD5CA64BA02186B21D3D0B3A5D7F99EDC89D7D4D3D468FEA10AD64C430076C6D0EF34A2FA3B48840745726435889B0B92EC78BE58F520ED8C243946C8EBC52011E9C1C5962A557167E1EAEAF82B37EE968E7D231FE6FA2ED4DA45423EDACC1A0A2AD2E13BE276CB055DF90D2E01D41412579DF693054817C6D6005E9AC6C3A6567A56D1A9A85D54D5307577D3310CFA570820C5352697F9DB3C068F7181BBAB9583A3AD8A9DAB84272EBA0C503ABC4668975E2F203DC99B7341146A2E5EFCE3B9A092CF55C5E87BDB9436F1A3E20CC1C69F61C84E22E7DE827332CEFAE8B5BADB2DBED4E63F075B44DD22FD43E95F140E428171C8D5C4D5C86240031D72EECC9AEBECDE978588D94B415A2F26C7727224C3E5764A5B3CFF857C2850D74300BDA46FB6EFEFE9A275990A85C5751760774CA05DA5EA3B2CD9C7C6758189FD00E315748B153CC97B4D56B2D3D58671EBA2F3A56182E21F82024C5BC7C117D652CB5DBE24ACD5D97CDA9C2DA2F86D502FE9178FBD13A8F0EC480ABE4BF1F8293D6BA961A6D308C54F8EC9EB35EE0B598467DD855FA6DBE5155F7135D8AAF299529515E7489007AEB2D898163A9BA30CDE2DA0BD7C15D8175C4F3EA306BDBC37DA6341631781C4042E824622DCB523B191B8916143247F19A59B04EE2982CBC7DAD5AD7F7E1CE8B93F5BA4454ACD2A92616CC573AE2C894DFA1FD7693DDAC226B86AA8EB685FD4242AA153A6100D54297F7891E90A35E107305C50014B19D1CD5CB120A185C9140F9C20C0916F29A247FDB25F11D09C08F734A1B239F25FCF622869DDEB1BE47EFFD36FA655EF901FC76EC49FF24DC86C5F2C7214D7D10FCF8825C106C198507822BE865BA5CA22C4A6B4BF378245AF200F65009A743D80CF55C1B7F810C9993F536C9342BDD30133899AFCFB2B5FDF40DE0ACEAA8ACBD91BA8E639C8A5D5A3BB0ADB752A26A0C2832C30D4213FB851E95A2FD32E64AED17AA889D7AD86D29E530F8CE5BCE9E57AFD368530D0C30D017383BC91E313894CA68729D9FF6F99D84334FBFE0FEA27539DB0BADA3B93F5752113DDD3C0EF5E2F0B1D0634CD966880D859FA80B37EF62AE87D2FFFE9DBAF84BCC32BAB806FDF72CCEB39BA4DCD234C76287B65DA7E84EFA2904E7BE1C4126EBC8C6A5A660C845445E1430EBC7D7684B9776611BF7D48333E7189CC59847DE72BADC9585390BFF4E5E0DFB53C64241B89BA7D7CCC60D570FCDB0795F46D9662C2D2EDBD97A94F99FE3007EAA2EFC4B89D058587CB046E02026F83294FF515DBE790C8A61CB4F1AB624D9DD1535D89F19E0F39DA4E9D386867C67A4D0F46B43C1F2E958D3BD3DC9C8A9634D17AFEEBE308C3AD675721AC5541334BD4C3A8E1322E0712209C9DFA23289B2FAC948AAE97681F4BB9154230002E9F723A9461804D2A7E360D4C88440FAC3486A221C1C6B9F98480748A811929110A4D6880C470D4268E468B5C3F61C4868204DDC407D62A23CA011FE4423437F8DEE129E44233B63BB38C2EF3492C3138E83E43B8DE0F094E318F94E2B3E759D2280A3DF69C4A72DDE89F848A5111B86EA7824D3880A4346B14523280C19C5138DB03064A3CEF84E232A0CD9A82FBED3880B4336EA8AEF8DCC898EECC7914C23290CD94F2399464C18B267239946489A898E9791EF353242138D22F2BDC1AC047F4E23240DD1455E36834020D6880AFDC5512EBFD7480A4D358AE5F71A41A1A946A97CAA91139A6A14CAA71A31A1A946997CAA91129A6A14C9A71642324AE4530B291905F2A98985DACBD6E391CC44467AB251269F5A48C7F1281E4F2DC4E378948FA716F2713C0AC80F1602723C4AC80F1612723C8AC80F1622723CCAC80F1632723C0A09B0E928271BA5E4070B2979324AC90F1A2919562882F2FAC1746D23A8921F34B2C2538EE2F283C97A87A21C25E64793550F45390ACD8F26062E4539CACD8F26662E45398ACE8F26762E45394ACF8F26362E45390AD08F1A0112246114A21F6D856894A21F2DA58852393F5A8A11A5767E34582AB10BC89F2CE58752563F590A10A5B07EB294204A69FD64294294E2FA49BBB41EA8461A8DF45034ED93E13D9D4674383A9656233B00ED283D3F69A4E7751A55B7AC6B462334030553C9673A2F244D3556EF99466818B251B09F690486211B45F499465818B2513C9F690485211B45F399465618B2512C9F694485211B35E2338D943064A33A7CA6918F8FB7493DCAC6331BD9A034E7F16353F138A6684C6583F2483D36150CCA15F5D8542A281FD4635391784AD198C803E7103E7E6C220DAD2F93A73411088A92E6A289D2A04869669A4808454AF154E7B1E54929D61AB96E7906E99CB76D79C1203CD6B970193A8AAB3A572E4347B154E7D465E8687E6A4487A1A399A9111C86EE078A4EE7D4A7E97EA4E83422C3D08D0AEF58E7CD65E89E51741A6161FB9D52623A7F2E4B48498CCE9FCB125222A373E5B28494CCE81CB82C2125343A072E4B48490DEFC635DE58A45E7AF7DA581C7016DB581C6A7162BAB13850BC34DD5C1C285E996E310E14A7A6DB8CEF724223DB6634EAD8E1A8567FC4C3B38341BCE5CE51BC265794D6C6BD9CACD7285B15F9AE32DE18E64A6B971D797CE77B38A27B07C7B3A72894C506E2EFD95D46EF7DEB268AA8AC33EA608C6608BEBB341E7B1F11A9BED9A8EB6347C2A34EE8F19C1CC637938EB394B6E1750A1E45453E4A92C6165815284EA85AEBDCA368BB2AA2B83BF7D62F2A1C64F5EC1FBBA4D8A2ACF694560667C1E33C49F691F0DD58644FCA6D6E2CB06FA2DA54607F49F37B54998AECCB3CAF2B53895DA138CFD651F9856BA94678DFA1F82E8D6263F9BD6A4E2F9A49EF4B3CA7A014D5A6E27B11557732B115A6083C8552C73E74FDB9DDD1F5D0AD6756056578A98BBECAAB7AB745AB5D42C16BFAB5A3A1E546B778693B7575471D19E4D72D6686035EE455BEC6428FB1D8803E59DF639D82718D4F5BAEFECCCBF536CA4CC7F44919DF52E81A21B8883609D6D3996C601B758D104DC2B98338A469BB896D28D8A696576D3006CF468D500BB68A390DEEDC200A65B999715723633B9B3B556C64039F54558E47066104BF74F9242C3AB89A9E65EB03FE668A40335E5D1B4FEB5377662E30DB9202330A8F035265E60BF81BEFC9AB6CA84607273141C39F8AAAB8090DC87102B76EED52BDE1FC3C50BDA1DD5C35FF4DF8FA15BA4125B90112A5AFC802A0C44252F36DB92C932C4E8A2835E71A0771005D8351C4EC217C19BECAE79CA2829CE4CE6A732E05A8CEF055AEFB741C7C7E4489AA5A82DBA86A433DDAF86C32D9800A4332DB4579E3E59517040DB04ADAC02F041235552B4D3A958A35E821612A9678D7623EC1CAB7D7B94E9E48999062D4E041D2D3546652A1A19B32BBACD0ED7E0822D20493232F654A0564280189471BCACE5C3A463040369A6A4C231A42234CFAC65B2684D69A7C758CB7B898409C6388288BD1A797F92E5B8FD68FAA57253432A1E98BEBFBDBEC33AA898AFFC6E3478F441975162B4D8DE6B4880CFAC1A43A2CE56266D12B72103A7B5F3406DD4784AB739DA2416EA4B6B38A0A34EE29021B69547F089047A109532E26DC0C7C932619E9304FCBDEA40F4DEAC1D32E26CBCD4C400FCF4A3DDF3245834DBC2C2A20A2769AD97D26069B37DF940CF2E101CCCD1751923695970ACF500292199269A3E1463089A84C37B10A9F9E453804EE997CF5A4AEA3F896EC492D2A19C527BD9DC69482254430C0554A85C5B331C1022913B041269DD63DA5E6252B40DBBD3E3D9398E0FFBB3D7FA59C8CC502090A0508480A55ABE944456CD37CB22236FF81080B798D4BD3AFCD135BA184A479E10E1490E65DB02985836AC7AC824135F9010805E994AEE22AC1A08A4984A32B61679230B8B09CE885CE5548806FCF2628003F1F88B0342E30F9828729A51015236B53063BAFBB116C92616F11C230D2C236DDFBF333080CF740BB4AC3884521D1E19F8E37D73400FEACDA46FEFD79348E9CBF26DF3F453711E6CA92CAA76B80891069A4C7416C169397C504E521CD4B7D9539EDA0EB536E488415180E1C901DB52E0B2C4670754C3A96A50C245C30EB4355674653C850E4E0E216C6918111ED2D7221DD7DAA1ACD2E756AEE3F48A9AB77A585D051A583CB1C8DBD1722075468418903586F561B8A706979EB96A926C246159DCA4B2093B4692752791D665F07023C7E088B4152EDEE9295CA65D81609E47FECC00069E9AF7B4DE67F64DB319FFF916DB2D9E2AEAA93ACE9AAA5FDD36DE58F0D04E438A8841C2F2822C74BC9C8B18D90B4755C5A3E3EA01415B8F29ACDAEB1582029A1000141A16A359DB0886D9A4F5EC4E6EFBB4BE0A2DD85E99E705674ED584A222AC02E95465A2848D0206E5F959E4A50C40619F61526F41715B1EDBE5F9FE35CFD2D39488CFF509EC5614A8167EB6FC50D08E5D17A06101095A14A139DAF871A643417101A3F5101DBEEF7ED39054573A78729165454A4D778C65A4D2C2CF67777024A8BFD959D25C5A5D3874D676BE68BB6850127A006502227D34E3E7453E69E7AE8563F8089A7ABB59955CB960C2A2CCB9BB770E3E6161F673B774921EACCADF62FA505239484F7BC20FB54254422ACD4EC9DD09A9136CECCDDDA50F96E6E49F8E05B83B92E3468564B5499605718165A27014D31E9A430D715ECD7477B7087D044BF70E5020BC9629A45D2ACB905C656A3EC83D06817D54CA96002B3E0A21A6CD07CA2E2B2A45E5850B0EDD49FA4575E5CE7CA41C23216B13C5CCA63CF7BBC54D23293BE1B49BDE547C203230B585D857985E8427F21062C6D275086D274B1EC2D19654397932F912B0F51CA54376984925349D712D76BA48D5B54A22C6FDDE0E2FB2149E33FD55E6539C924B2C57F031032BEE6934B9BA4DDCB899D8449612A345B44295309044B078C32B53722A76CA849E7868C41E521634B47A3EAF8A6DF3063CB0514A965F7CCE066CD2D406E1B670B8B0E100D522D433202CB00A08A0041B20F2C155451D762936E9E3078A7B5C82D1FA2AA8D21FCA9FDEB6DBE915F72154A4262D6E6DAC898080B08D7903B915C49DB66D2892D95A73449D9E05B83D964681C0F9D10A87B9C2F2E97268B102CDA8F48656BA233F4EA6ACC2C5D32963F0011E3ABAE8F1424A5B09A1D95E1F3A45F904AD90C011DB5B532EFEDF033A5A42B8255690649FC0565A88C52ACA20DC255418521F91BCBD9082088BE4C18515555E634CA540C7F1806D958EDF38C3CE99197F29914283B81ED3F622F136F51D1CA65EC7D81210F4DB29A308AFA8E6F82098697A7061674E09B0477F41421BA4DCB480FDD7C23B704C1D917A991DF53E3CA4D2437E0ADB5D904C7EADEDA3492637583ED0AC528B9DF0BE1F96D97C4776952D567596D34A3B104138813F70140AEB83A4F2D61708B97113598390F6D9EEB83D9E885A18FD2125ECC7A6479A088C9058B6BDC3212C5F1E16188D2491CE7BBAC3658E909252151EA0AD9089288BB40906269E34C7AB123F61424291FBCAB30A31851CB7C5D7F530BD9B0824401039264E388F01225B17DF3CB92C88A87244CDD638926C234160D2C4C1430144D62ACE1B4C224B66F7E611259B17FC2848D39CC7E4C53638AE111CBEEA14D928C3ED7C0739DBF57A87BB1B37A7158973BF1BE0C815DA19A6DD4E1419B01899A20592C0235A10918549E294AFFA862A5821B9F9E3485ED76BAE5905D010D9E0CC68CBA7B8E4EAC449BAEA1160E17093842096344259816A73D7C2E20B4C91ADAF6B28340DB261BD08E0E4D1063CCD6F50DF33093D8454CB606AB757208186DB286B6BF582650F719DA6FB7476B81AFB7197A7A98D884522E4D74A61E6788C206E10C996638721083BE60A303027DC21630C393036911FAA04F02409FA1A1A72F4B0B1874A64E0FDE8223BE4D36A0958CFA31CB0043AA8BC73CA3B126A90B9DA9C111368C0530A18411A214C890BEB1E624104D9E06855E61083074A6068777CE09587C01DD28A08C55712450991C0E656A012602F0A2F70145C11B0CBA07C0253E26E963D607432B199B4A3035CD407B131F02A54C2C8E41472C870CB807BE260DB04DFFEA34D334E5BBD3549BC071AE47527107561DCE9C694D3B2943C4D792A1DA33EF25BB349F7920996E356879DA37767CD81768AAE4D55FA69EE2BBBF542D21252CA106DA089AA16E4D943D552B69B4D1CBB64243746FDB728C9159B8C6B8AAC110806FEA8755210D6BFE122BAB0E8DDE62A55BA9B0E92D9001FE29571E8EA38B7DCF5336CC14AF7E8A23067EF7D37AE0C10F7D4EC78EF1614A800B92572B995A8BEF565295855669126A49538334917E61116CA6F40946AEB2D0238C4C7305E5A8009858575CB02F064ADA2DBB422DD41BB83D6DD972E082348B205BD43AB7BD59A34BDB2D5CEA856A4CDFE7B56F2F7D6597A586DC074EEDA4C2844BDA4A9750D6982A2869B3C45DA002829B1FAEEDDD1365F2A6436F984115E65E31131B6ED26AEECDB2F09613F0CE16D074DD6B5C4CD515EF7151F557BB677480934901FD56949C11661CD034DDA4CDF33496EF0B79BB558FD680F5973C5BE3C80DC93B35C662E5A116F43C9294D48F713D8F6C34C6A23CA21F9751B248FA0A8DA44DD03B34CE0C821E9E99813F17D46328DAA9D58C3354E16053AC8C2D41F9D1BFE72131A7A0D73E0483887BEFC3D29CE21EF8A0A825FE7A9F761EAB1B7A6CD4D263BFA61E4FDE567A93026EAE2C5EAD5065205CAD65A381C8B41482623FC5A9EDD4930170D3656F0AF0F5065E15D05AFA0A0C50BD819B950E5E5426F83DE4479547C7671DA1607C7CDA970AEC5D291080464B77AF3C9A2D77A82B82BDC3D5963BD18D5B2EF59ECBF7DC9C25BDAD9554CEC5B0E590843281CB9D649C8954AE619A7B63B55A4D15881BAAB656B719367F460D27468A061709CA70D29C452F0B28ADD5541A1CA9C60B34FCE978C7324FAF56D1430191AD7DBB33A8783E7EAFBAC19A3E9705F9756CF87CBDAD9AE314C16AC58AABE638B3A6CF34C7F1915581862B83AF321597855FA5AA2E3FD0A6449ACE2F06470555B3C1C8F7AD0E20EAC592191DE262384B2D6BD4EE7179D44B5F964CED3357C46354F344193B4FD62059083D2F2EC9A2E781A061CF6018B0CC20D2206B112B630D3A9CD058903DEA65862A461ED00EF542C39019B32D35A461DCCC0E7EE958A20DFA16E2C8D77C279AC4F863009B3441CA98D6C8C39451CD804F3C6A700076C88F3DBA724288A22565873ADE16D01669C42DA1416672A309B1A565B7D7D8122AAA1A5CCA985192B1208B1AE531BC6461A2CCF9EFC032305E11C02D7D5C23A655CAC8465483E4277AF570531F220322ED28C548129047D2DB62481E0FD11163F04C77148A8B13A36489184946D20026968C072398E0319A1356DE6D07773F94015114B50677415C9A0FEE87046F3F7F7C5EC50855700F494324E13D3C582389E741216A6E04F8706B380CA06213188E42D21A3E20850763F80814FA330C0EAC10232500ACD0845360EA2F0FA840D55F729F540334F1BC02DCF657F0421613006C031015C0911B401800A359DB831FF4251A393F64D7DAC1560017DB1DF901DC64A7779BE5F77FE4FC787ED4220CF7AD87BCE747ABF8166DA32EE1F9112E12A3A2DE45E945BE4669D5675C444591649B6AA4EC520E5645149351FEEFABC383CFDB34AB5E1CDED675F1F3D151D540578FB6495CE6557E533F8AF3ED51B4CE8F9E3C7EFCECE8F8F868DB621CC5CC3AEA3957DBE14BD8EC883688CB25D73CD6E8755256F5695447D7518579FF6ABD158A71B7CB59E60D2CEE3FC65E2017BBADBF97D59727FF666EB13F3A3D79FBE8ECF5A3D3978F242823135FE3766DB159D534110182235062DA551CA551D9DFE417AFF1BFCAD3DD36D3DEEE5760EDEADBBCC455E1B0C66473ACB36D6316D0385D9239C6DBA8AA5FE5DB224D08AF58303ECF0E7585AA2A21D72478C821C31C8FFCC902B529E608975155FD99975C178EA9E64857689390A80C44259C5FB2787C9E39EADF5099DC247143F921BF431CDF806C11FBF91127FAFC083B128618A7EDF8116B349E55F3BDCD8896E2188C6905AD8CE34CFC299AD7CAC05472BCA05A22AEBF3F459100D5A7DA21FDB571E6F1487F053DAB6AA4CB3CC9EA4AC4EAD32DD04A3CE97E4CB28C18650C1C9D618EF7328AEFF0A479C7828DA9164849BE29A3E296D3D054B205D6EEE6E6659AC77788EB4A26C31CEF558A151627AD6D92058644DBBB69FAD3649309B3D990683397E57512E75905B24BCCB540FE1CDF46D906C1C07CA639EE6BAC2384F96D48B4C111429FB388DAC8E83AEC2BF48F1DAA6A900170098B2F9409CAD6CA2F8025CCBFF04B132197C5ECD32C50F294AB579B628150E6BB42D550B08039FE9B2829315CCE35954AB6C35AD55F52246275C91658A8CCB1F8C16D1632ED70DFA27B948A885DB21DD61F8508F407704A5181C2235852C31C2A1C78F36B7E0DB0664CB547E2B943A7DBD9F578999C00567D976C812536D0BA7560D31CDA85CD4B5EEF764956187F08107F5822FC5D40F8BB1D02D9EC87AC3436C716F1438225B88610FB1C0BC4244BD2285B9F67F7490DCFCA922216DFC877153A49B66F73DE2264732C103921BBB0922FFFF56BE3DDFE05D5BF73F5A0D32DD6B0D10635B2C02D5FC7649BF570B1E371DA24738C559A73106D8A054271B25E27C4F602DA2564DAE08278F63875547322D02599637C88526C9DBFCD2B0E884EB7455BEDCA1230B0844C5BDC8FFC1C41259B637DBC4D2AFC1B54127CDEFEF943C6A849811C23B2B051360E123986D651D2934A1D2674018B056960474C530BB496C242F996E8C40BD75C3A1470879CFD1347D9D933275104C16CE4500220633BDF89CB8A18F5982CA3D1E56FCC2ED6FD217ADDB5B32DFBD885AB52FF625D47F1DD49966C23D14B2364DA79F47821EAD32CE411DBCE92BA7159769867373728E6FD7A54BA1D9A3058BA343B940F096FFB8EA9737B3EF3749DFFC9337C48B540BA7C95573C9FBB340BDF69B4C50637E73AEDD22C507625204663AA85A714901E7BC9394B91E85C1E12AD71A21462939069E1A7496A71EE1E12CD71C8B93C51598DA90B7842C8BA79CBD788CDB15BA763929335399BB82B5109545356C6FE2B657CABFC029D6F8D7E116D92388932393E57C2FA0B2BB28BBC557D812B61E37910754D9F36EB0E7A99C41C449764E377C8F891DC2559D85E28CEB375547E91990B40BEA56587ADC28C6F2B93618927EA1B2AD966654D8E2E013C64326CF1C4CAD1E91668228E25C2EFC5A68CD6A8BB7C4223B139D68862D5988CBDB1D565B17DAD56681086C9C20CA653EC225FE78281D8275AD8E9120BDDC5020E63BF605340B00DBED2B5A1FEF6A28DE469D00C64508B209DDD58425E2E816C8B997320163B95CFB399097977F095A537F841CA593011F3922E17C10AD9F9275B72248C1C15484ADEADCDE7D9BA3C80853293618BD7CD9410E29065734CAB4AE2931245FC39AD21D9120B5ED373599698808545A75BA201E60C9D6EE302C92B748A6E90609BB239360E114C9091F3AA3C209D6181572675823320FB99CFB3471555269B63EB78B9883E8BAB652ECB1A135A8173593698E5DD15AA92FEF21F0BCAE659A0B682729AAF05E71393638D08AA0A3ECFE66861DB3650E4854C675C880F6089C55C60A2E033191687F5F0942293273E6F99835CE7D59BBCAA13FEE01A956C75F4E90FEE7C7E9734B36B2ED9DCD632B60B9936477336490C0E0E36C706F1F39B4B1EAA49B2C2B810312EEC305AB34A5CB63319733ABFDEE5279B4D89BBE91E9DC7FC6255C8B4C1C51318020C0E26C36659C1CB429B628700EC6D0FA9363BE5CDF55371FF87C9B0398A837863B94B32C720963A6C77B339164754A2E6E0033CBA854C735C5E75C17A6BA1059D24B49ACD520E823058C4C164D2A91EFFE49D017D9AED720D5AA659991DF8276418D3E94BEC3E7501B481F3AF7DB2B5E2162BA659282F24C692E08836620C411888314CA6D25AA2B6B25E8982F6399B63B12A6B5EB87A7BCF594754F2CC9BFB13AC8E85CB18AF6C2F624CB5C27EBB8BEFE4EBE131F7DB2ADB04B3BC03576C4CC657B86A7FB8EBEB3007551A8ABF45E98E5F5DD3190F7FD57F868DDA82108AA7ECB92C3B4F02D85E266371CF4402707148B4C0F1BC187685364DC815719F93CFB3F19480A7E2CF5DAEC89E57CDC426620DC9765859B5DB92072F45B831C7069198C80504486558F998509927B1E062EA52EDBC55A97077624CB541EAEF3CBDBFFE2F41B789B936C82B5C1B887963BA0DDA076C28426863BADD2266B5BB16DD0D4C861D1E0CE674B82F4A93B5E8BDE0B22C7D89BFE6D7F2537A74E69247FF1AA724A8DED99CAFD407DAAE61041F669FEC80252C93D81C2B44A979C0E759A136872CB7DB5C801C336CDC0025E2E7CC3ECD0E45B876D9A559A114B7020849B23982C91FBFB4A396CFFF7CDE433BD079795F80A38F4EB7425BD525CA36F5AD003766D878A0D7BB18BD2FF22ACAA4DA475AC8E63BC5AE6E7A1054C340B62536D05574BAC3E15950BB8BB936C89F79B0CF737BF7570570D256F204A31CC5FF20EBC728A94F511A716171A864CBFD05B0B7D89CF9762C1674D60E71906D9CB6D836C5FF7E7FF32FB0F7B6C7FC574737AEAC4EF20EF0BBCB186637E22526905F9115732D1C8981EF5A9E6261DCF0B79BBA340B945D195D2729EE38E03EBF9069B750C0631AD5085E29D0794BECEE5C45A5B0955BDA4D19DEB11982DD0C6041149B16BE2ACA7A1BE8F78AAC7DE5AA602F35097FD6C3F2A0077FCA4346FD0ADB144DF08D83C6A58A3B8E6516EF1A8ECB649B64510DEFB4CCDEB51F11565B0FAF73C5C59CED2A0E5A06DBAF815B8AF705183882CBFB1ACFC202DB1F5CD6B7DDBF6FBB7F5AD46FBB7F939E8AFD27DACB1226FD63BB0D80B36DC1C77B1D126D705E279FC50DA72EF19BABDA0C711267B060D75909C8FEF92B64B866272C2103563009F7DC845D15284E22F236CE3723F69B11FBCD88FD66C41AE07D3362BF19B1DF8C580DCE3723F69B116B8EB8A4112BF1ED43C1D5560E51D5300D241554B20D16BC3B9DBA6C4BA7BCAE68536C3695411E150E3C2AC8DC29E2945691C857F0898B95CB818B550173BA70E17441E6051EA84DB34111FAABB0ED2FE0D2EE90688543F4A480D326DAE0E09554065F8EE7F36C501BA5C1C3758966CB586059B8E70B59F51A74E925ACC060F6E9E65959EB1A818C5ACCFA0622534019F4959A7C1A774388DBFEFE9BDDC08D14EBBB28BC878AE70B94BF3742287976DC46F8200803A183C9E4A66B22841AEBD32C7C5C2496D22DA90E78B247C87541BE8A802747855C176479782836DB055B3C01C3E7D97834F8C5C5A9F535AACB487C05674CB55936128115D78D7DAA0DD2AA799FE3555E08AB593AC762E184AA4A307687449B63AB314AEEA1C717C6749BC3A54D5BC46BC24C862D1EF4B21C9B638B2879C34DC874C105DE7313326D71F9DE19536D919A33F530DC906531892635DFD62E696F26292CC849617710559CA6601083894A46A8188E8918E3614C9DFBF869D0F015EF8A18885ED1A5EE8DC0B455F2B66B201023CB06260CC756B92F3D4AF30DBFFC69D32C66DF8057BE81A3F94C86CDFC8B1772C48E16E6F231DD06ED22BFC79DC863F5A956764152D700D4906C35C0BE8AB711B1E082315C6D15CE655E25E245AE31759F148EBFB6715335567AC65BBAB0251F09B6BD5528D98B5DC5DF07EF922CE42BF2BD90B7BACD8B9334CDFF142E42D119FB245F1741A23ACB81CCA44D4AAC10BA8E06903D2AE7E14F045F8BF2BE700C2CF63528F10ED17F904980CC0699947862D1B38E4DB76C4785E925F72EB2EA9F7071FF5A22710A64326C86271986809DCE64D878A39A88A392268BB936C8F5AE940373997B23AD6C93BDBD182A30236F861A60FE6E3D4537D12EAD011DC6E65823FE01A259CDA61DCDDF4124BB2591B1C9BAAC9886914F77C1B4964889282E7B913AC4843CDD88E30686D588E0860238061692E0CBBCAC23DFDD4C18C4407E6584F2799794E7BB754CB55152554D4E3790A0279012E5739D9005654AE738210A4A95CE596A39B8CA77658C00363219B6787F4058567C6B49FE0EE15871CBFCC8C44283F8034A51810781B7A7430E64309855C452D60E34BCECB03916A29DADF9483F5DD2EC5E89401B1473FA37967A04FED63B5E3F046120B43099CA032A28B95BDB78FD014503653BE0159221756EEF3010DE6A48DD2B610B10595F066328747611F67B0A50F8BA740BCB3D40E4F8BD0A01146458FE330612F21842219E6495E2180E22CB6758071248581C9E5E0D24770E0FA5EE830C2F7AB0298002970319795EE4C472E7444F2339E864ADC8F7EDB093FD31AE8584E875B44DD22F83A3CA5392346806E2A44590719C23E4590F645B48D7AEBECD8183D963B2CD19085C0FF0F02E9B637156EA33FE958817CDE87473B4965130FBECB8F66B9E64E269EE31D5C630CAEE78C32803EFE22D3A8A820C1EE731633B544274714B23134031D71619B8E7C364D8E281E38ECBB259BE7E5E25FF2D5E856E13675D720AD590D561D101F236DF041923008EF13001693552986FE0C1D2654CA95C17EAAF5F9A50F7A97F87C9810C7A4C452C6331D059D6DD7412C7C474E471A8E4E53607CF8B93F5BA44FCFD1A2AD9E21A65BE118F640E895638E26A6948B4B94DB245551D6DB9571CA8E4BD1920BFED92F88EDCC8C6F9A5EFCCAF063318283A00A93079DDC20DBEEF1DE00590CB9C1B184D8239FD6FC72CF96F56E1487E7BC2513FF9E6B36077D95116A5750043400E64305C54C452C11A68845D7726679959E664BD4D32D18EA492AD4EE96366F10701FA443B9C551D9542A4B321D96258B73C06196FE797892AFE30719F36F320E9335FE5591D25D8D0E18B0C5FEF5286DF559F40E49AC4A1CBD728AD46BA557C8BB651D3E2AA88E26653638D5E2765559399FD3AAA505BE4F000B3E73E59A312AB9A2F558DB68F488147AB7FA4AF52BC64ABC702175196DCE059F8437E87B217874F1E1F63D576922651456E8FA63787079FB76956FD1CEFAA3ADF465996B70FE9BC38BCADEBE2E7A3A3AAF962F5689BC4655EE537F5A338DF1E45EBFC08637D77747C7C84D6DB239EBC83354279FCAC47A9AA35E30CA7740B3BF6D8E1FFFCAFE80BDFC3BDFC5CA11B71D81E71BDCDD33F978F7852971787D7C9A679C6A25150ADB55BA3F56554E3B93323D3316A2A7E78F06E97A6E438CA8BC39B28AD84E12C7C68F44CB51FAAB6519A369FB2443ADB92480B1D4A761F95319EDB0F0FF02AF76DF3CA131685A74F69D4BA140360F0A06FA3AA7E956F8B34696317B5E86BDCF6BA797FC3B28E046E854DDF465E5A2C97A6B67A326C4B2FA3AAFA332F870E0F047B8536D8D86BDF223BBF5456FA7B6BF0BFA132B949E206BC19EE4AFCEF9EA8F1691DA91C92129FAFD9A054589CFA61C9104F3C30651A80577C3F3767755E1CFEAF86F0E783F33F3E0DB47F39785F6295FCF3C1E383FFED5083FAFB5314AD7D460AC1F82BD970F4C4681EF1A9BC504A6C037C4CB28C048170877919C57778A6BCF3C248F24D1915B75F42EB9097BB9B9BE1ADE05E6AECEBD7C5DE6801EA24FBE2D248516B3BCF2CA7C926A366A81245A9C3EC94D7499C675500FE9C7DC69D956D5000A8D7589B784E46C07E95B5C21030648AC3440A5BB02BF48F1DB60243F088EC09ACC3E1F59160FCE4FB973C15386D0B51E6BB225CBBA860347E4DA3A2CF7802A132C74215A47118AADBCAF1AFD31F8567CFBD297C46EC9B2200437ECDAF83F0A3C7F1E609B1ADDFE698D4DF4A0FD1AE308DEACE30F31398814A6D28BDECAFF6C8B2E7DCD99E5AF6062117B2FD6DB016E743D24657F3C049B2248DB2F579769FD421A6E18B7C57A19364FB361F2D3B271C2FBD402F2B0359848DBBFF1754FF5EF834EB2ADAA0EE194DAFD1D43C70EC89D13ABAFD74C38A6C813561019856B941F943D4CD2117BF267D88526C6ABFCD2BE453951665B52B4BC6467287FA9878D9B61F6F930AFFB61EE1F6AE842B9446625C5A4B97420FE2E55AA041A67531F8AD190C560B864A2125CD5154C6059078A49A0D2DABA9CF5E6E80E3D46642E32223236B765982970D49D3F3370919A90FB2EFA903DB0E26D6406D5107E31E76EE58E921747DFF2AD861EBED225175EF4EB2641BD1FE05673CE259820C615BA9C30660B84A11B43E606000A840ED6B1F06F7050AE382CBD375FE6700465FBECA2B6FABAB7F5DC3D71BB82BC3884F20D119DED5F0E34E0713A581D8F426A9E909D0B55AE39506CFFA845BD40F8FA9F961753027EB7BCCF65D89CA2035EC51F1EA2D2C62F31853126521315764C3711B0AB3A09584B39C4CB2B75A26B1D7BAE88AB8FBBD579D28CEB375547E0937CD3748D818CB627F85D1608550191FA27283EA202C6BA182542A004677A9B4B50C7DD9DD814DBA48C9B7D7B9D38296103A2D627B426AA805DF180F663F063200DE24FE187BBC107A57C45D444EE7251107E1225900C4B4CE91F183CEDDC242F839492817E1D72D647EF2E5B6EED6F5B5AD866AAE13938DDDA41CBD96AEB34EBB940FB2206CA186D8087E602FA32A894F4A1405010AB6566DD082181F0D5298C99E7DFCDB178C7EF8DB17AB7B459BB5219D1D0ECC9BDCBEEB7CF635EE3068ECD2D1038D7D30DB178E79713A0C58A0512EBCB8ED5B3BF091EDBD72C104915EFE416D5F1B33D4F197F3EA0D5EA527E3D91EC7B3277F0C6789977306F16F5E7B9F72A09FBBF606FBFCC68B4518E0C20FA0B568422C7CA7F0C9BCCB4F369B12F7DE3D3A8FC7C59C8B38BE23473B519039FF2AF29F05094608AE77A177E9FD04B7130AC8DF082576316FD03A8F0DE16D744F3C1B5D64BC22392DF92887666B1142E7B2C0EDE9785E04779F74B18EDC058AD494B517C36E2C18AC457B62DFF375E4D3EEE7FB5A6A9FA3D2BE1E0603F782C73D1B315497D910705B8407597A376BB640A66FFB5CF5DB7B6F4325CC666ED825648063E253AC43DFEEE2BBAB00C7E1FEA956A281963F7BBDA87D10CBD040070A1A98EE7D78F709760F17C567D8802C085A8843BC64851DA88DA117EB4918768DB74C3C20AED0A6351E43EC989D87B83E775E35D38F3F4856EDB684C40F8798B3853F0CF1D090D725BD30F2943A75ED86D1DF8D787FFD5F943272C35AE1FAF873E60336C6BC51B029BADA5D87585613A850385849276B7A81EE7A778AF8C37ECDAF031D6D0A794CAAF1B205D2B3FBEEB16B2D7EEF83503D5080A503C60A39017736ECC9769B7BD70C6BF000131481B9080243A27FF9A204A848D809770ACF2B731ACE51615DDE17A1C61D865AD565D31C7F8FE97A17A3F7455E4559483DD3DC5B6B9FB60AA3541BC010FD309C290CA4A157E4D51F4F8810FEE65511628A0E81F1314AEAEE71D2106EEF40FDE4E742B7F2009E67DD371C3C810B5F72F2F775BFC400AAAB69063E5A1EC2C75FEC5713B2EA3EC9D6846EACC45F0ECEABDF1B46FF7CF09AB0C4C1937E8A0571E33FD9919B1DD7498A45D0FB4E2D115C3C6A518D68EB1C8A4860C2F6E5F72ADAE77D3CF94B7B5DEC8EFE0182731C42704005ED58A127212A1468EB60DCFA873ACB6867C81B4163E41BA91BDD7AC81CE47DC1DE6956E9762354C3ED10232CB38D0C23A86EDF41EB50B60133D814B181D3AD244DB08CB7442CC0F43B226660261B22A648662696119AE14E831196F96E88035C902A1A7A2D8CE2891939FA4D91C2F5A8819BDF08E6F2D817E2BC3ADB1663803CCEC16A86F03AF92CF5C81B456A35F3131A4385EB27532FA11996A93FCE6C92F7EE7AC345A5315438B68FA7B244DF8609FDCAE026B4214E98BE5FA51C9FDD81FCEDC355C172C7B1CF094C79E70FC272D819278C20AF0AA2E8BD41C64E7286D09D73364421FAD01B6555E799C1AD0033B04657F8DA57491597C936C9A27A3C8C043AB98F9FFC14CAA345AF461EA0472BC8C1F9106E51EFE3291F515492E2BD7B51C524038F800837C535C50B12E2DE416A089D8BE4F474EC3C1AFC4830B916784B142FEB30F5BA6678DB1A49F4D302FE7881DC3323A0B3578F85F0F1E99E5286A473C8D4B37F5C467454CD507B85E7D5FB020B9AE7519D55134DF0555EC89628665BABFDBB8B619B78856294DC3BBAD44762BF38682D83429C136EA1C2C4B86EB182C5951EE18244976EE1DC76423AD2009DD69C3C08103AEC4352A79EB26D3C8DB5AF4BBB4C64B277A9F553D948C9336ADA1B2EE1035FCD783FE55D1173D154CC6EA63474D3184004DBCD04EA5B636F04017CB0DD5A4CA234DF780FD2A027BCC39C2A38AFF03A8A94F79CA32FF2FB24DB78CEF3495D7B827C0BB8DE1CFB2A629FA833DEAAE732AF12FAFC9A9912B5D1228E2AC4517F04989FDB976E5B103C3B5F2759547EF9976DF4F95FADBDBEBB6A3C28BE2F870257B7797192A6F99F9384D9C65DE011DE67A476EC7E8ADAE348C9DE28DA6F3AB20199FEEEEC9EE84AF22BA4CA949524DF09A860E792C7BEDA6EDF6FA927B2951B4FAB5BB77974868C21A1175ADDA7A0E9C86962BCCC493785318BBBE0190237AC7C3B2C848F93EF0AD5BBD2B3360CC204D7FDD9F6BA790778A6DB7B09D4DD667D30F526DAA5B5BB82A201BCF45407E43F77764001D618DE56A4AD64798894872CFDB3BCA3B2E42C3CA3B635AA8FFF30B31D5FC663E1322FEBC86913B1A57419092325B5FC093D0A4E515593BD7C724BCA43DFB2209E3A77000BA17707307FDD1B662DB9CA77658CDC994DD1FB6D843438FE2C6E71FCB94B9F6F083D7C3FA014157838B97932466A9761CC524F6DDD37F21262A9BFE4DA70AE0D94FD7194983F7075EB16E08ED0B9486F4F37E114B46C87A36C17E264D554DE5DFBBA5989926BB0B89ED659A43ADA09C52AC8E187E5F793C3DC211447B189A5D150F9191901424685189E4E47D0AC4692FBCB833DB1EB58E29ED69864302D2740B3BF7EB12F426C792EC855938FD41EE783186DFEED8C90C1E145C9A92AB3C38BC9549B1FAFA36D927E197C5B2EF2C441B808150031AD2BF06457DFE621CE3C9F46B8E2139D783DFB8C7F25C8338073CB5B275F5F4FEA25FABFE64916E4ECF45594A96E40F98F0277E17797F9E945BDFD4E48590A72B9A5859A68E460EA55F2DFFBB9C4B3AE97A514BFCD37EE828C89DD65B9239E439CE75267C6BC6F5B983A32DF91EDB330FC248E890DE6C4F181D66B06997CDFCEE8864D71B25E9768BCF8114A21E04EA4CF2C0684A557205E073E486CA6AA8EB685E51C6E3C7A7EDB25F15D9A5435CE299DE6E185EF7BEEC9D67290E89A9779E58DF19B32608219C4137F271818486B8F76A92E5116A5B5EB943D523B6D3633D4137A92969F404ED6DB24838C39276D48163367D9DA7B3D4370567554D6DE485D57FA9ABB5728AAC6F3B161CF1A9D54551E274D37F32A73547F57288D800BFF98D90757794AD3F4955CA1F4E611957AB14BEBA448931857E1C5E1F1213F88DE676DB4C18393987C08234655DCB889F9E1833FAAADC5505FB036632E5BAB7F133E8687342AC9E88A52122DBF2E23DC7DE2F84FB23829A254E0055712D4150AF70C69ED00CEE79CA2825C9FCC6A69DB037C7DF808D7073ABE3C3FA2C44A2D6D8D5F789434C8773F7630F3986CD3A95D0A2F5E7C4702322262F159934807D04049DF50FEF6107211E8C3F38944BEBDCE279584E6038C00B4295F47BF8BECDBD3EE267EFC4FC06B81634F351B2A7447B509E65DDDA0D3006DC2241D2DEEFE48F8EEDDC522CF245F1AB75916EBE03EAC0C17CD399441F1F8D12355FF3361B879411A33BE2273401E785CF2613150F722B242879712621229158475FF4E6584D201B218E1653226D33D56BDEE6B744A838149BE0705AD5A6ED2A1D9554D37FB2CA97DE69B8DEC656FD959898406FB041CD5B0EC78CDCCD3043CA331DA8407DED7621837C997F840610BF574F1496F71900BC26C470916A2D1BAD2CC6009D4E5C2AD66493FC8AE313BAC23FDBE3753774BE32504EB6FEA0B1CCA90FCC07B5CC6C3FDED7231B252C8EE26E81C4293F4F0BB59E0DB7E7631E17DD7D3137673F7151E65487EF8DDDD37E5817479E31B929BE7FD757BA0BF2C6DB539BD44609000791F90B29E1D6FEC2D527E6E866E672F324F39DAB9800D34129FF5C047BD2A3485E4A36CE887252561061180FAFE6BE9F4BDD7F37D372BE3A70418B5FB25070E4352171E6632E930F9F08C06C1D482A2F5E9185B1C5F89AC84FEF0ACB242857DFA262A7B262A5C50AF3D58592AC5C4A23BF7749539F3A2C37EB5B9B4287C02E30B05B3373B741AA14F7AE0D626C437C9C7C448470BF7F6F1B7EE9EB0BB99304B8BF5B434F050B0CEA6BE40A3D0C90FBCD3653CB4FDE03C5DFEAE883F812F7538B8F5D56BC7E61BACD1D8264DD5DF161E7BF0D108CB95A2C84387EFCD7172F4362F3E4982E95067476F393F6F9B60DEE1C3177810E88040A8F3A342B4294917C0E1A5AC3A1CE6A0F5F7E6EC70CDB9F1103D0E1C391E53BF8A3EB738A3BC64A777AA1D88BF1658B11B89CD0355EAC6B2B5B04AEF3ADBCC7AF3EFF2056DB859BBDFDE925B52085A0BE49334BC92832DA637E884199E4E9EC877676C68C9DE917330EC8C677AD537E73AB2ABB1E7BDCFEACE6DCBCF7766D342B8F6E0BE88C9580FD4D9F38FF3B93BDDF87BFBD0F1DAE59B77B72FB17C9BAFCBAD166F0B7738B629BA23A6EA0B806339A6CFE8E4BD3DE94555D2A447C6E2F3DD0FD47C705E41901D3BF611877D3CC8BD9C5CD81EECDE33E9509DF20E2D19B39EF95E54224C579FC3CBAA4B4BC2F84FB5EF2FA848F01F8511A7F4092E27244A863B7F7EB67812A6F21220B2C4F2523277B40917D9583AEE442B16069B07FE02B1D0F6C1CC4260B985B070F70361AED472601DE3CAE0B2E8424188160E51652E24CB07A46863D37E1A02E9CA2F2275B1AAE9CEEC93CC25630C152CE234A993C80314665BD223B2B8DA5692200988ECF0C9D924605417604CF249C400D63842DE572412768A6961C1E045421F63C0A35B351EAD65C20FB8775AF03925DCB767909C3128B941608AB0A1B0A878E8341C9DFC15991CB2E8EF6E9F9D55A79C67F7183217A26E4F62907E0B90F6A064438CBF338D58CC19BD681931308E64B42264FBD2F7F2FB0ADF3A7F8ACEBF42314AEEF7A2FB952F324C2207DC17693C3EEB2B920D159FDD3E3DAB9880B7BBA7918FF9AFCE2F23113617E6971785EE810683F5455792E9BF216DDF03E0F51535E914EABD8BD9569CCA6FCE2806D267B082CBC1520BCCF925C17271B92FA2207DDB26B828505F626EC052C95F8928C878BA17A270D63CFC82696A4C81CA7E8ACAD7E87552563579E4EC3AAAC443D5846A856AB6CE870767C33B32BC64ACE25BB48D5E1CAE9B7715DA9768864C414A58786AA2103E40E5419FA0B24D3F323E0123FFD85846F9D1B198E9C7BB9D3DF997BB02CACF766534DF947D4AF10533E0EEA90CB10D6D3A58F5364B032C9C8F103E2194803E261432FEACF28BBA8F69BFD31E3415BED02643D86D8E06B53DB82CA0B6C9106A9B63803AFAFE40F4315BF695B1844EA298F0F8A26031D9A07C3125345F6B7D19C257DA6408BDCDD1A0F6574504DC3E0342EEF3B4356E8F1E02756E33E05AB7797A6C18588A6A02291F5074A6E403A6038A0E9A037D67C8947C67C837FB8EFC23CA2F18480E1B650A9020B6002C496C19B36FCA3FA6FC8A16BE0F7422A0F71910789FA7C1A6EF630AF87426F40D3A5F377FDE82EABA4D0667CF5B03753DDE350191E56A7BCC35F88274EE1FF364DF309B33E97B52128D276F0A9DAFF98EB0332E7C4C28017D512864F459E9D7541F31C46E564012F8264FFE85265BF3117AC92D7C85CE843E43E76BBEC37B81856FF105A0EFF16574CA855A438A0A86CA04950C95CF7D875A4201963BF022E60145C1DBF1BA073465FEB3A1B18AB50D4CCBAF6B580CE952E5886DB6014BC0671B015EE89F77641AC2E8ACA6F2A02A821B2F126B564BCECD6E5732D2D6424FEFF936925E6FB56D039753F64D1A9FDF031A24799B8FA91A3D01343583943A43412F821A0A7091E3D614D9437392C619BD4B176C9C422BAA8165D295923D23D4AFA8412ACBFCD935FF06B1EC04967D2D4755CB394731671FFC92C9BBE259304FC19FABFFC7B7AD80364A1EBEF26C1ABDA06E28C005B34B53E8C79BC0E6485F77E22A5870F513548ECF38776C1ABDD285DB26BFADE9D1387175DE93C9D6DDCECD6BFC0FD2A601D70EFD9A45F9487A12C803E2D41C2A4CB0A4497489A0CDE27C1C3D99CC7BE1DCBCEEB11579EBA0D758A0CA42355DC428009E13015AA77B74C4ABFF60E74E43AA71D9383757D3CE691A08B52C6093785EC95BA70E821EA267666C7E2F93FAE64B4A4EDD7C8B11EFD57C3A90BFB2F5D288FF0FB5F117546C7AEDC4A369B6AB7E9E6912EAE3AF4B4C0638ECB487EA62BDD40D89C407EDD39C63757BC4A7A8F7B741B4F71C6E933CB6A047B3448F7F43A6F0E53B358F8A190DB74E1654DACD0EE51412BD7BD82922706FD0C171C54446865C57F2D0C9ACF3EA9633D3A01D0F8142584F4A77323C9A2677422AA20407681CE0CF93EEA1380B655B13A94842E15CBD05D2881DEE4DD26A1255E0D700CD9B43A588914B41B35519DED44D490054C228546CC2397AD8E4BA531ABED3D3F134A5D6E42351AA9B15DCA7365BAFA92606455046CFE64D3D31F0D10581C62903103295150F8835D5951FFD9AC5B901C7CD5337D4C8A1E8D7DC39DC8B6250386DB3D5CE46EF264FE67A54443D53B75913962958E32587363988B09BB0069C3008FCC52A24878DD9259AAE36765561ADBC9B3BBDBD2B8DCB6476D242D7E820C72C66386920C61D02DAAF094EC4549E3DCED4541A3EAC045051C76C2842E8FC8C6B4385F03AD2D6AA03F1046B322C27BA33643EE22E60ABE45D1379254C7366DE0F05A3A2005CD0474F0936E2C523740DB1E2709C8F048C213F947D2F890C12ACD1B39D5261E35928DB0C5DD50FD4DCA94E2EF0211BB4ED537981F7BB81FCE14D554BD517EB0335193E92DA00680E9AFAB061D8F353B55F728D3C50C3A7DD8A142F95034DD5DC3C67AACB5D2A6CAA2BB93138F364045C9C56B4557E15384063E7988480CBC18AE6CAAFBB0668AE78B4BBDDC4921FDA9637F7F9518B305C7E1DF29E1FB527C3BB04FC13CFA8D1065DE46B94564DEAF3A32B5CD7648BDA5FA7A84A3623C4738C99A1987C7304EDCB9C6737797FED97AB515FA4CFEE7DE2A88ED6511D9D94757283851867C7A8AA920C37F76F51BAC345CEB6D7687D9EBDDFD5C5AEC64D46DB6BD69425778755DF7F7E24D4F9797BC0B20AD1045CCD043701BDCF5EEE92743DD4FB7594569CBD20832097925BB96EFAB2C67FA3CD9701E95D9E190275EC1BEE527F40DB22C560D5FB6C15DD2397BAFD5EA1B76813C55F70FA7DB2265A4606A2EF0896EDCF4F93685346DBAAC318E9F14F2CC3EBEDE7FFF8FFAC9D040B11B60300 , N'6.1.3-40302')
END

