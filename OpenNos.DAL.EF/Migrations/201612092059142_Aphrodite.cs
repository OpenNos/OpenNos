using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.PenaltyLog", "AccountId", "dbo.Account");
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            DropForeignKey("dbo.Character", "AccountId", "dbo.Account");
            DropForeignKey("dbo.Respawn", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.QuicklistEntry", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.Mail", "ReceiverId", "dbo.Character");
            DropForeignKey("dbo.Mail", "SenderId", "dbo.Character");
            DropForeignKey("dbo.ItemInstance", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.GeneralLog", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.Character", "FamilyCharacterId", "dbo.FamilyCharacter");
            DropForeignKey("dbo.FamilyCharacter", "FamilyId", "dbo.Family");
            DropForeignKey("dbo.FamilyLog", "FamilyId", "dbo.Family");
            DropForeignKey("dbo.CharacterSkill", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.ShopSkill", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.NpcMonsterSkill", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.NpcMonsterSkill", "NpcMonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.MapNpc", "NpcVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.MapMonster", "MonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.Drop", "MonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.ShopItem", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.RecipeItem", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.Recipe", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.RecipeItem", "RecipeId", "dbo.Recipe");
            DropForeignKey("dbo.Teleporter", "MapNpcId", "dbo.MapNpc");
            DropForeignKey("dbo.Shop", "MapNpcId", "dbo.MapNpc");
            DropForeignKey("dbo.ShopSkill", "ShopId", "dbo.Shop");
            DropForeignKey("dbo.ShopItem", "ShopId", "dbo.Shop");
            DropForeignKey("dbo.Recipe", "MapNpcId", "dbo.MapNpc");
            DropForeignKey("dbo.Teleporter", "MapId", "dbo.Map");
            DropForeignKey("dbo.Portal", "SourceMapId", "dbo.Map");
            DropForeignKey("dbo.Portal", "DestinationMapId", "dbo.Map");
            DropForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType");
            DropForeignKey("dbo.MapType", "ReturnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.MapType", "RespawnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.Respawn", "RespawnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.Respawn", "MapId", "dbo.Map");
            DropForeignKey("dbo.RespawnMapType", "DefaultMapId", "dbo.Map");
            DropForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType");
            DropForeignKey("dbo.MapTypeMap", "MapId", "dbo.Map");
            DropForeignKey("dbo.MapNpc", "MapId", "dbo.Map");
            DropForeignKey("dbo.MapMonster", "MapId", "dbo.Map");
            DropForeignKey("dbo.Character", "MapId", "dbo.Map");
            DropForeignKey("dbo.Mail", "AttachmentVNum", "dbo.Item");
            DropForeignKey("dbo.ItemInstance", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.CellonOption", "WearableInstanceId", "dbo.ItemInstance");
            DropForeignKey("dbo.ItemInstance", "BoundCharacterId", "dbo.Character");
            DropForeignKey("dbo.Drop", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.Combo", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.CharacterSkill", "SkillVNum", "dbo.Skill");
            DropIndex("dbo.PenaltyLog", new[] { "AccountId" });
            DropIndex("dbo.QuicklistEntry", new[] { "CharacterId" });
            DropIndex("dbo.GeneralLog", new[] { "CharacterId" });
            DropIndex("dbo.GeneralLog", new[] { "AccountId" });
            DropIndex("dbo.FamilyLog", new[] { "FamilyId" });
            DropIndex("dbo.FamilyCharacter", new[] { "FamilyId" });
            DropIndex("dbo.RecipeItem", new[] { "RecipeId" });
            DropIndex("dbo.RecipeItem", new[] { "ItemVNum" });
            DropIndex("dbo.ShopSkill", new[] { "SkillVNum" });
            DropIndex("dbo.ShopSkill", new[] { "ShopId" });
            DropIndex("dbo.ShopItem", new[] { "ShopId" });
            DropIndex("dbo.ShopItem", new[] { "ItemVNum" });
            DropIndex("dbo.Shop", new[] { "MapNpcId" });
            DropIndex("dbo.Teleporter", new[] { "MapNpcId" });
            DropIndex("dbo.Teleporter", new[] { "MapId" });
            DropIndex("dbo.Portal", new[] { "SourceMapId" });
            DropIndex("dbo.Portal", new[] { "DestinationMapId" });
            DropIndex("dbo.Respawn", new[] { "RespawnMapTypeId" });
            DropIndex("dbo.Respawn", new[] { "MapId" });
            DropIndex("dbo.Respawn", new[] { "CharacterId" });
            DropIndex("dbo.RespawnMapType", new[] { "DefaultMapId" });
            DropIndex("dbo.MapType", new[] { "ReturnMapTypeId" });
            DropIndex("dbo.MapType", new[] { "RespawnMapTypeId" });
            DropIndex("dbo.MapTypeMap", new[] { "MapTypeId" });
            DropIndex("dbo.MapTypeMap", new[] { "MapId" });
            DropIndex("dbo.MapMonster", new[] { "MonsterVNum" });
            DropIndex("dbo.MapMonster", new[] { "MapId" });
            DropIndex("dbo.MapNpc", new[] { "NpcVNum" });
            DropIndex("dbo.MapNpc", new[] { "MapId" });
            DropIndex("dbo.Recipe", new[] { "MapNpcId" });
            DropIndex("dbo.Recipe", new[] { "ItemVNum" });
            DropIndex("dbo.Mail", new[] { "SenderId" });
            DropIndex("dbo.Mail", new[] { "ReceiverId" });
            DropIndex("dbo.Mail", new[] { "AttachmentVNum" });
            DropIndex("dbo.CellonOption", new[] { "WearableInstanceId" });
            DropIndex("dbo.ItemInstance", new[] { "ItemVNum" });
            DropIndex("dbo.ItemInstance", "IX_SlotAndType");
            DropIndex("dbo.ItemInstance", new[] { "BoundCharacterId" });
            DropIndex("dbo.Drop", new[] { "MonsterVNum" });
            DropIndex("dbo.Drop", new[] { "MapTypeId" });
            DropIndex("dbo.Drop", new[] { "ItemVNum" });
            DropIndex("dbo.NpcMonsterSkill", new[] { "SkillVNum" });
            DropIndex("dbo.NpcMonsterSkill", new[] { "NpcMonsterVNum" });
            DropIndex("dbo.Combo", new[] { "SkillVNum" });
            DropIndex("dbo.CharacterSkill", new[] { "SkillVNum" });
            DropIndex("dbo.CharacterSkill", new[] { "CharacterId" });
            DropIndex("dbo.Character", new[] { "MapId" });
            DropIndex("dbo.Character", new[] { "FamilyCharacterId" });
            DropIndex("dbo.Character", new[] { "AccountId" });
            DropTable("dbo.PenaltyLog");
            DropTable("dbo.QuicklistEntry");
            DropTable("dbo.GeneralLog");
            DropTable("dbo.FamilyLog");
            DropTable("dbo.Family");
            DropTable("dbo.FamilyCharacter");
            DropTable("dbo.RecipeItem");
            DropTable("dbo.ShopSkill");
            DropTable("dbo.ShopItem");
            DropTable("dbo.Shop");
            DropTable("dbo.Teleporter");
            DropTable("dbo.Portal");
            DropTable("dbo.Respawn");
            DropTable("dbo.RespawnMapType");
            DropTable("dbo.MapType");
            DropTable("dbo.MapTypeMap");
            DropTable("dbo.MapMonster");
            DropTable("dbo.Map");
            DropTable("dbo.MapNpc");
            DropTable("dbo.Recipe");
            DropTable("dbo.Mail");
            DropTable("dbo.CellonOption");
            DropTable("dbo.ItemInstance");
            DropTable("dbo.Item");
            DropTable("dbo.Drop");
            DropTable("dbo.NpcMonster");
            DropTable("dbo.NpcMonsterSkill");
            DropTable("dbo.Combo");
            DropTable("dbo.Skill");
            DropTable("dbo.CharacterSkill");
            DropTable("dbo.Character");
            DropTable("dbo.Account");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.Account",
                c => new
                {
                    AccountId = c.Long(nullable: false, identity: true),
                    Authority = c.Short(nullable: false),
                    Email = c.String(maxLength: 255),
                    LastCompliment = c.DateTime(nullable: false),
                    LastSession = c.Int(nullable: false),
                    Name = c.String(maxLength: 255),
                    Password = c.String(maxLength: 255, unicode: false),
                    RegistrationIP = c.String(maxLength: 45),
                    VerificationToken = c.String(maxLength: 32)
                })
                .PrimaryKey(t => t.AccountId);

            CreateTable(
                "dbo.Character",
                c => new
                {
                    CharacterId = c.Long(nullable: false, identity: true),
                    AccountId = c.Long(nullable: false),
                    Act4Dead = c.Int(nullable: false),
                    Act4Kill = c.Int(nullable: false),
                    Act4Points = c.Int(nullable: false),
                    ArenaWinner = c.Int(nullable: false),
                    Backpack = c.Int(nullable: false),
                    Biography = c.String(maxLength: 255),
                    BuffBlocked = c.Boolean(nullable: false),
                    Class = c.Byte(nullable: false),
                    Compliment = c.Short(nullable: false),
                    Dignity = c.Single(nullable: false),
                    EmoticonsBlocked = c.Boolean(nullable: false),
                    ExchangeBlocked = c.Boolean(nullable: false),
                    Faction = c.Int(nullable: false),
                    FamilyCharacterId = c.Long(),
                    FamilyRequestBlocked = c.Boolean(nullable: false),
                    FriendRequestBlocked = c.Boolean(nullable: false),
                    Gender = c.Byte(nullable: false),
                    Gold = c.Long(nullable: false),
                    GroupRequestBlocked = c.Boolean(nullable: false),
                    HairColor = c.Byte(nullable: false),
                    HairStyle = c.Byte(nullable: false),
                    HeroChatBlocked = c.Boolean(nullable: false),
                    HeroLevel = c.Byte(nullable: false),
                    HeroXp = c.Long(nullable: false),
                    Hp = c.Int(nullable: false),
                    HpBlocked = c.Boolean(nullable: false),
                    JobLevel = c.Byte(nullable: false),
                    JobLevelXp = c.Long(nullable: false),
                    LastLogin = c.DateTime(nullable: false),
                    Level = c.Byte(nullable: false),
                    LevelXp = c.Long(nullable: false),
                    MapId = c.Short(nullable: false),
                    MapX = c.Short(nullable: false),
                    MapY = c.Short(nullable: false),
                    MasterPoints = c.Int(nullable: false),
                    MasterTicket = c.Int(nullable: false),
                    MinilandInviteBlocked = c.Boolean(nullable: false),
                    MouseAimLock = c.Boolean(nullable: false),
                    Mp = c.Int(nullable: false),
                    Name = c.String(maxLength: 255, unicode: false),
                    QuickGetUp = c.Boolean(nullable: false),
                    RagePoint = c.Long(nullable: false),
                    Reput = c.Long(nullable: false),
                    Slot = c.Byte(nullable: false),
                    SpAdditionPoint = c.Int(nullable: false),
                    SpPoint = c.Int(nullable: false),
                    State = c.Byte(nullable: false),
                    TalentLose = c.Int(nullable: false),
                    TalentSurrender = c.Int(nullable: false),
                    TalentWin = c.Int(nullable: false),
                    WhisperBlocked = c.Boolean(nullable: false)
                })
                .PrimaryKey(t => t.CharacterId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.FamilyCharacter", t => t.FamilyCharacterId)
                .ForeignKey("dbo.Account", t => t.AccountId)
                .Index(t => t.AccountId)
                .Index(t => t.FamilyCharacterId)
                .Index(t => t.MapId);

            CreateTable(
                "dbo.CharacterSkill",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    CharacterId = c.Long(nullable: false),
                    SkillVNum = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId)
                .Index(t => t.SkillVNum);

            CreateTable(
                "dbo.Skill",
                c => new
                {
                    SkillVNum = c.Short(nullable: false),
                    AttackAnimation = c.Short(nullable: false),
                    BuffId = c.Short(nullable: false),
                    CastAnimation = c.Short(nullable: false),
                    CastEffect = c.Short(nullable: false),
                    CastId = c.Short(nullable: false),
                    CastTime = c.Short(nullable: false),
                    Class = c.Byte(nullable: false),
                    Cooldown = c.Short(nullable: false),
                    CPCost = c.Byte(nullable: false),
                    Damage = c.Short(nullable: false),
                    Duration = c.Short(nullable: false),
                    Effect = c.Short(nullable: false),
                    Element = c.Byte(nullable: false),
                    ElementalDamage = c.Short(nullable: false),
                    HitType = c.Byte(nullable: false),
                    ItemVNum = c.Short(nullable: false),
                    Level = c.Byte(nullable: false),
                    LevelMinimum = c.Byte(nullable: false),
                    MinimumAdventurerLevel = c.Byte(nullable: false),
                    MinimumArcherLevel = c.Byte(nullable: false),
                    MinimumMagicianLevel = c.Byte(nullable: false),
                    MinimumSwordmanLevel = c.Byte(nullable: false),
                    MpCost = c.Short(nullable: false),
                    Name = c.String(maxLength: 255),
                    Price = c.Int(nullable: false),
                    Range = c.Byte(nullable: false),
                    SecondarySkillVNum = c.Short(nullable: false),
                    SkillChance = c.Short(nullable: false),
                    SkillType = c.Byte(nullable: false),
                    TargetRange = c.Byte(nullable: false),
                    TargetType = c.Byte(nullable: false),
                    Type = c.Byte(nullable: false),
                    UpgradeSkill = c.Short(nullable: false),
                    UpgradeType = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.SkillVNum);

            CreateTable(
                "dbo.Combo",
                c => new
                {
                    ComboId = c.Int(nullable: false, identity: true),
                    Animation = c.Short(nullable: false),
                    Effect = c.Short(nullable: false),
                    Hit = c.Short(nullable: false),
                    SkillVNum = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.ComboId)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.SkillVNum);

            CreateTable(
                "dbo.NpcMonsterSkill",
                c => new
                {
                    NpcMonsterSkillId = c.Long(nullable: false, identity: true),
                    NpcMonsterVNum = c.Short(nullable: false),
                    Rate = c.Short(nullable: false),
                    SkillVNum = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.NpcMonsterSkillId)
                .ForeignKey("dbo.NpcMonster", t => t.NpcMonsterVNum)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.NpcMonsterVNum)
                .Index(t => t.SkillVNum);

            CreateTable(
                "dbo.NpcMonster",
                c => new
                {
                    NpcMonsterVNum = c.Short(nullable: false),
                    AmountRequired = c.Byte(nullable: false),
                    AttackClass = c.Byte(nullable: false),
                    AttackUpgrade = c.Byte(nullable: false),
                    BasicArea = c.Byte(nullable: false),
                    BasicCooldown = c.Short(nullable: false),
                    BasicRange = c.Byte(nullable: false),
                    BasicSkill = c.Short(nullable: false),
                    CloseDefence = c.Short(nullable: false),
                    Concentrate = c.Short(nullable: false),
                    CriticalChance = c.Byte(nullable: false),
                    CriticalRate = c.Short(nullable: false),
                    DamageMaximum = c.Short(nullable: false),
                    DamageMinimum = c.Short(nullable: false),
                    DarkResistance = c.Short(nullable: false),
                    DefenceDodge = c.Short(nullable: false),
                    DefenceUpgrade = c.Byte(nullable: false),
                    DistanceDefence = c.Short(nullable: false),
                    DistanceDefenceDodge = c.Short(nullable: false),
                    Element = c.Byte(nullable: false),
                    ElementRate = c.Short(nullable: false),
                    FireResistance = c.Short(nullable: false),
                    HeroLevel = c.Byte(nullable: false),
                    IsHostile = c.Boolean(nullable: false),
                    JobXP = c.Int(nullable: false),
                    Level = c.Byte(nullable: false),
                    LightResistance = c.Short(nullable: false),
                    MagicDefence = c.Short(nullable: false),
                    MaxHP = c.Int(nullable: false),
                    MaxMP = c.Int(nullable: false),
                    MonsterType = c.Byte(nullable: false),
                    Name = c.String(maxLength: 255),
                    NoAggresiveIcon = c.Boolean(nullable: false),
                    NoticeRange = c.Byte(nullable: false),
                    Race = c.Byte(nullable: false),
                    RaceType = c.Byte(nullable: false),
                    RespawnTime = c.Int(nullable: false),
                    Speed = c.Byte(nullable: false),
                    VNumRequired = c.Short(nullable: false),
                    WaterResistance = c.Short(nullable: false),
                    XP = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.NpcMonsterVNum);

            CreateTable(
                "dbo.Drop",
                c => new
                {
                    DropId = c.Short(nullable: false, identity: true),
                    Amount = c.Int(nullable: false),
                    DropChance = c.Int(nullable: false),
                    ItemVNum = c.Short(nullable: false),
                    MapTypeId = c.Short(),
                    MonsterVNum = c.Short()
                })
                .PrimaryKey(t => t.DropId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .ForeignKey("dbo.MapType", t => t.MapTypeId)
                .ForeignKey("dbo.NpcMonster", t => t.MonsterVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.MapTypeId)
                .Index(t => t.MonsterVNum);

            CreateTable(
                "dbo.Item",
                c => new
                {
                    VNum = c.Short(nullable: false),
                    BasicUpgrade = c.Byte(nullable: false),
                    CellonLvl = c.Byte(nullable: false),
                    Class = c.Byte(nullable: false),
                    CloseDefence = c.Short(nullable: false),
                    Color = c.Byte(nullable: false),
                    Concentrate = c.Short(nullable: false),
                    CriticalLuckRate = c.Byte(nullable: false),
                    CriticalRate = c.Short(nullable: false),
                    DamageMaximum = c.Short(nullable: false),
                    DamageMinimum = c.Short(nullable: false),
                    DarkElement = c.Byte(nullable: false),
                    DarkResistance = c.Short(nullable: false),
                    DefenceDodge = c.Short(nullable: false),
                    DistanceDefence = c.Short(nullable: false),
                    DistanceDefenceDodge = c.Short(nullable: false),
                    Effect = c.Short(nullable: false),
                    EffectValue = c.Int(nullable: false),
                    Element = c.Byte(nullable: false),
                    ElementRate = c.Short(nullable: false),
                    EquipmentSlot = c.Byte(nullable: false),
                    FireElement = c.Byte(nullable: false),
                    FireResistance = c.Short(nullable: false),
                    HitRate = c.Short(nullable: false),
                    Hp = c.Short(nullable: false),
                    HpRegeneration = c.Short(nullable: false),
                    IsBlocked = c.Boolean(nullable: false),
                    IsColored = c.Boolean(nullable: false),
                    IsConsumable = c.Boolean(nullable: false),
                    IsDroppable = c.Boolean(nullable: false),
                    IsHeroic = c.Boolean(nullable: false),
                    IsHolder = c.Boolean(nullable: false),
                    IsMinilandObject = c.Boolean(nullable: false),
                    IsSoldable = c.Boolean(nullable: false),
                    IsTradable = c.Boolean(nullable: false),
                    ItemSubType = c.Byte(nullable: false),
                    ItemType = c.Byte(nullable: false),
                    ItemValidTime = c.Long(nullable: false),
                    LevelJobMinimum = c.Byte(nullable: false),
                    LevelMinimum = c.Byte(nullable: false),
                    LightElement = c.Byte(nullable: false),
                    LightResistance = c.Short(nullable: false),
                    MagicDefence = c.Short(nullable: false),
                    MaxCellon = c.Byte(nullable: false),
                    MaxCellonLvl = c.Byte(nullable: false),
                    MaxElementRate = c.Short(nullable: false),
                    MaximumAmmo = c.Byte(nullable: false),
                    MoreHp = c.Short(nullable: false),
                    MoreMp = c.Short(nullable: false),
                    Morph = c.Short(nullable: false),
                    Mp = c.Short(nullable: false),
                    MpRegeneration = c.Short(nullable: false),
                    Name = c.String(maxLength: 255),
                    Price = c.Long(nullable: false),
                    PvpDefence = c.Short(nullable: false),
                    PvpStrength = c.Byte(nullable: false),
                    ReduceOposantResistance = c.Short(nullable: false),
                    ReputationMinimum = c.Byte(nullable: false),
                    ReputPrice = c.Long(nullable: false),
                    SecondaryElement = c.Byte(nullable: false),
                    Sex = c.Byte(nullable: false),
                    Speed = c.Byte(nullable: false),
                    SpType = c.Byte(nullable: false),
                    Type = c.Byte(nullable: false),
                    WaitDelay = c.Short(nullable: false),
                    WaterElement = c.Byte(nullable: false),
                    WaterResistance = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.VNum);

            CreateTable(
                "dbo.ItemInstance",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Amount = c.Int(nullable: false),
                    BoundCharacterId = c.Long(),
                    CharacterId = c.Long(nullable: false),
                    Design = c.Short(nullable: false),
                    DurabilityPoint = c.Int(nullable: false),
                    ItemDeleteTime = c.DateTime(),
                    ItemVNum = c.Short(nullable: false),
                    Rare = c.Short(nullable: false),
                    Slot = c.Short(nullable: false),
                    Type = c.Byte(nullable: false),
                    Upgrade = c.Byte(nullable: false),
                    HP = c.Short(),
                    MP = c.Short(),
                    Ammo = c.Byte(),
                    Cellon = c.Byte(),
                    CellonOptionId = c.Guid(),
                    CloseDefence = c.Short(),
                    Concentrate = c.Short(),
                    CriticalDodge = c.Short(),
                    CriticalLuckRate = c.Byte(),
                    CriticalRate = c.Short(),
                    DamageMaximum = c.Short(),
                    DamageMinimum = c.Short(),
                    DarkElement = c.Byte(),
                    DarkResistance = c.Short(),
                    DefenceDodge = c.Short(),
                    DistanceDefence = c.Short(),
                    DistanceDefenceDodge = c.Short(),
                    ElementRate = c.Short(),
                    FireElement = c.Byte(),
                    FireResistance = c.Short(),
                    HitRate = c.Short(),
                    HP1 = c.Short(),
                    IsEmpty = c.Boolean(),
                    IsFixed = c.Boolean(),
                    LightElement = c.Byte(),
                    LightResistance = c.Short(),
                    MagicDefence = c.Short(),
                    MaxElementRate = c.Short(),
                    MP1 = c.Short(),
                    WaterElement = c.Byte(),
                    WaterResistance = c.Short(),
                    XP = c.Long(),
                    SlDamage = c.Short(),
                    SlDefence = c.Short(),
                    SlElement = c.Short(),
                    SlHP = c.Short(),
                    SpDamage = c.Byte(),
                    SpDark = c.Byte(),
                    SpDefence = c.Byte(),
                    SpElement = c.Byte(),
                    SpFire = c.Byte(),
                    SpHP = c.Byte(),
                    SpLevel = c.Byte(),
                    SpLight = c.Byte(),
                    SpStoneUpgrade = c.Byte(),
                    SpWater = c.Byte(),
                    Discriminator = c.String(nullable: false, maxLength: 128)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.BoundCharacterId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.BoundCharacterId)
                .Index(t => new { t.CharacterId, t.Slot, t.Type }, name: "IX_SlotAndType")
                .Index(t => t.ItemVNum);

            CreateTable(
                "dbo.CellonOption",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Level = c.Byte(nullable: false),
                    Type = c.Byte(nullable: false),
                    Value = c.Int(nullable: false),
                    WearableInstanceId = c.Guid(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItemInstance", t => t.WearableInstanceId, cascadeDelete: true)
                .Index(t => t.WearableInstanceId);

            CreateTable(
                "dbo.Mail",
                c => new
                {
                    MailId = c.Long(nullable: false, identity: true),
                    AttachmentAmount = c.Byte(nullable: false),
                    AttachmentRarity = c.Byte(nullable: false),
                    AttachmentUpgrade = c.Byte(nullable: false),
                    AttachmentVNum = c.Short(),
                    Date = c.DateTime(nullable: false),
                    EqPacket = c.String(maxLength: 255),
                    IsOpened = c.Boolean(nullable: false),
                    IsSenderCopy = c.Boolean(nullable: false),
                    Message = c.String(maxLength: 255),
                    ReceiverId = c.Long(nullable: false),
                    SenderClass = c.Byte(nullable: false),
                    SenderGender = c.Byte(nullable: false),
                    SenderHairColor = c.Byte(nullable: false),
                    SenderHairStyle = c.Byte(nullable: false),
                    SenderId = c.Long(nullable: false),
                    SenderMorphId = c.Short(nullable: false),
                    Title = c.String(maxLength: 255)
                })
                .PrimaryKey(t => t.MailId)
                .ForeignKey("dbo.Item", t => t.AttachmentVNum)
                .ForeignKey("dbo.Character", t => t.SenderId)
                .ForeignKey("dbo.Character", t => t.ReceiverId)
                .Index(t => t.AttachmentVNum)
                .Index(t => t.ReceiverId)
                .Index(t => t.SenderId);

            CreateTable(
                "dbo.Recipe",
                c => new
                {
                    RecipeId = c.Short(nullable: false, identity: true),
                    Amount = c.Byte(nullable: false),
                    ItemVNum = c.Short(nullable: false),
                    MapNpcId = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.RecipeId)
                .ForeignKey("dbo.MapNpc", t => t.MapNpcId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.MapNpcId);

            CreateTable(
                "dbo.MapNpc",
                c => new
                {
                    MapNpcId = c.Int(nullable: false),
                    Dialog = c.Short(nullable: false),
                    Effect = c.Short(nullable: false),
                    EffectDelay = c.Short(nullable: false),
                    IsDisabled = c.Boolean(nullable: false),
                    IsMoving = c.Boolean(nullable: false),
                    IsSitting = c.Boolean(nullable: false),
                    MapId = c.Short(nullable: false),
                    MapX = c.Short(nullable: false),
                    MapY = c.Short(nullable: false),
                    NpcVNum = c.Short(nullable: false),
                    Position = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.MapNpcId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.NpcMonster", t => t.NpcVNum)
                .Index(t => t.MapId)
                .Index(t => t.NpcVNum);

            CreateTable(
                "dbo.Map",
                c => new
                {
                    MapId = c.Short(nullable: false),
                    Data = c.Binary(),
                    Music = c.Int(nullable: false),
                    Name = c.String(maxLength: 255),
                    ShopAllowed = c.Boolean(nullable: false)
                })
                .PrimaryKey(t => t.MapId);

            CreateTable(
                "dbo.MapMonster",
                c => new
                {
                    MapMonsterId = c.Int(nullable: false),
                    IsDisabled = c.Boolean(nullable: false),
                    IsMoving = c.Boolean(nullable: false),
                    MapId = c.Short(nullable: false),
                    MapX = c.Short(nullable: false),
                    MapY = c.Short(nullable: false),
                    MonsterVNum = c.Short(nullable: false),
                    Position = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.MapMonsterId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.NpcMonster", t => t.MonsterVNum)
                .Index(t => t.MapId)
                .Index(t => t.MonsterVNum);

            CreateTable(
                "dbo.MapTypeMap",
                c => new
                {
                    MapId = c.Short(nullable: false),
                    MapTypeId = c.Short(nullable: false)
                })
                .PrimaryKey(t => new { t.MapId, t.MapTypeId })
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.MapType", t => t.MapTypeId)
                .Index(t => t.MapId)
                .Index(t => t.MapTypeId);

            CreateTable(
                "dbo.MapType",
                c => new
                {
                    MapTypeId = c.Short(nullable: false, identity: true),
                    MapTypeName = c.String(),
                    PotionDelay = c.Short(nullable: false),
                    RespawnMapTypeId = c.Long(),
                    ReturnMapTypeId = c.Long()
                })
                .PrimaryKey(t => t.MapTypeId)
                .ForeignKey("dbo.RespawnMapType", t => t.RespawnMapTypeId)
                .ForeignKey("dbo.RespawnMapType", t => t.ReturnMapTypeId)
                .Index(t => t.RespawnMapTypeId)
                .Index(t => t.ReturnMapTypeId);

            CreateTable(
                "dbo.RespawnMapType",
                c => new
                {
                    RespawnMapTypeId = c.Long(nullable: false),
                    DefaultMapId = c.Short(nullable: false),
                    DefaultX = c.Short(nullable: false),
                    DefaultY = c.Short(nullable: false),
                    Name = c.String(maxLength: 255)
                })
                .PrimaryKey(t => t.RespawnMapTypeId)
                .ForeignKey("dbo.Map", t => t.DefaultMapId)
                .Index(t => t.DefaultMapId);

            CreateTable(
                "dbo.Respawn",
                c => new
                {
                    RespawnId = c.Long(nullable: false, identity: true),
                    CharacterId = c.Long(nullable: false),
                    MapId = c.Short(nullable: false),
                    RespawnMapTypeId = c.Long(nullable: false),
                    X = c.Short(nullable: false),
                    Y = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.RespawnId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.RespawnMapType", t => t.RespawnMapTypeId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId)
                .Index(t => t.MapId)
                .Index(t => t.RespawnMapTypeId);

            CreateTable(
                "dbo.Portal",
                c => new
                {
                    PortalId = c.Int(nullable: false, identity: true),
                    DestinationMapId = c.Short(nullable: false),
                    DestinationX = c.Short(nullable: false),
                    DestinationY = c.Short(nullable: false),
                    IsDisabled = c.Boolean(nullable: false),
                    SourceMapId = c.Short(nullable: false),
                    SourceX = c.Short(nullable: false),
                    SourceY = c.Short(nullable: false),
                    Type = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.PortalId)
                .ForeignKey("dbo.Map", t => t.DestinationMapId)
                .ForeignKey("dbo.Map", t => t.SourceMapId)
                .Index(t => t.DestinationMapId)
                .Index(t => t.SourceMapId);

            CreateTable(
                "dbo.Teleporter",
                c => new
                {
                    TeleporterId = c.Short(nullable: false, identity: true),
                    Index = c.Short(nullable: false),
                    MapId = c.Short(nullable: false),
                    MapNpcId = c.Int(nullable: false),
                    MapX = c.Short(nullable: false),
                    MapY = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.TeleporterId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .ForeignKey("dbo.MapNpc", t => t.MapNpcId)
                .Index(t => t.MapId)
                .Index(t => t.MapNpcId);

            CreateTable(
                "dbo.Shop",
                c => new
                {
                    ShopId = c.Int(nullable: false, identity: true),
                    MapNpcId = c.Int(nullable: false),
                    MenuType = c.Byte(nullable: false),
                    Name = c.String(maxLength: 255),
                    ShopType = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.ShopId)
                .ForeignKey("dbo.MapNpc", t => t.MapNpcId)
                .Index(t => t.MapNpcId);

            CreateTable(
                "dbo.ShopItem",
                c => new
                {
                    ShopItemId = c.Int(nullable: false, identity: true),
                    Color = c.Byte(nullable: false),
                    ItemVNum = c.Short(nullable: false),
                    Rare = c.Short(nullable: false),
                    ShopId = c.Int(nullable: false),
                    Slot = c.Byte(nullable: false),
                    Type = c.Byte(nullable: false),
                    Upgrade = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.ShopItemId)
                .ForeignKey("dbo.Shop", t => t.ShopId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.ShopId);

            CreateTable(
                "dbo.ShopSkill",
                c => new
                {
                    ShopSkillId = c.Int(nullable: false, identity: true),
                    ShopId = c.Int(nullable: false),
                    SkillVNum = c.Short(nullable: false),
                    Slot = c.Byte(nullable: false),
                    Type = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.ShopSkillId)
                .ForeignKey("dbo.Shop", t => t.ShopId)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.ShopId)
                .Index(t => t.SkillVNum);

            CreateTable(
                "dbo.RecipeItem",
                c => new
                {
                    RecipeItemId = c.Short(nullable: false, identity: true),
                    Amount = c.Byte(nullable: false),
                    ItemVNum = c.Short(nullable: false),
                    RecipeId = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.RecipeItemId)
                .ForeignKey("dbo.Recipe", t => t.RecipeId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.RecipeId);

            CreateTable(
                "dbo.FamilyCharacter",
                c => new
                {
                    FamilyCharacterId = c.Long(nullable: false, identity: true),
                    Authority = c.Byte(nullable: false),
                    DailyMessage = c.String(maxLength: 255),
                    Experience = c.Int(nullable: false),
                    FamilyId = c.Long(nullable: false),
                    JoinDate = c.DateTime(nullable: false),
                    Rank = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.FamilyCharacterId)
                .ForeignKey("dbo.Family", t => t.FamilyId)
                .Index(t => t.FamilyId);

            CreateTable(
                "dbo.Family",
                c => new
                {
                    FamilyId = c.Long(nullable: false, identity: true),
                    FamilyExperience = c.Int(nullable: false),
                    FamilyLevel = c.Byte(nullable: false),
                    FamilyMessage = c.String(maxLength: 255),
                    MaxSize = c.Byte(nullable: false),
                    Name = c.String(maxLength: 255),
                    Size = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.FamilyId);

            CreateTable(
                "dbo.FamilyLog",
                c => new
                {
                    FamilyLogId = c.Long(nullable: false, identity: true),
                    FamilyId = c.Long(nullable: false)
                })
                .PrimaryKey(t => t.FamilyLogId)
                .ForeignKey("dbo.Family", t => t.FamilyId)
                .Index(t => t.FamilyId);

            CreateTable(
                "dbo.GeneralLog",
                c => new
                {
                    LogId = c.Long(nullable: false, identity: true),
                    AccountId = c.Long(nullable: false),
                    CharacterId = c.Long(),
                    IpAddress = c.String(maxLength: 255),
                    LogData = c.String(maxLength: 255),
                    LogType = c.String(),
                    Timestamp = c.DateTime(nullable: false)
                })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .ForeignKey("dbo.Account", t => t.AccountId)
                .Index(t => t.AccountId)
                .Index(t => t.CharacterId);

            CreateTable(
                "dbo.QuicklistEntry",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    CharacterId = c.Long(nullable: false),
                    Morph = c.Short(nullable: false),
                    Pos = c.Short(nullable: false),
                    Q1 = c.Short(nullable: false),
                    Q2 = c.Short(nullable: false),
                    Slot = c.Short(nullable: false),
                    Type = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);

            CreateTable(
                "dbo.PenaltyLog",
                c => new
                {
                    PenaltyLogId = c.Int(nullable: false, identity: true),
                    AccountId = c.Long(nullable: false),
                    AdminName = c.String(),
                    DateEnd = c.DateTime(nullable: false),
                    DateStart = c.DateTime(nullable: false),
                    Penalty = c.Byte(nullable: false),
                    Reason = c.String(maxLength: 255)
                })
                .PrimaryKey(t => t.PenaltyLogId)
                .ForeignKey("dbo.Account", t => t.AccountId)
                .Index(t => t.AccountId);
        }

        #endregion
    }
}