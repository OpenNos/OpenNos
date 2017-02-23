using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite36 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                "dbo.Nosmate",
                c => new
                {
                    NosmateId = c.Long(nullable: false, identity: true),
                    Attack = c.Byte(nullable: false),
                    CanPickUp = c.Boolean(nullable: false),
                    CharacterId = c.Long(nullable: false),
                    NpcMonsterVNum = c.Short(nullable: false),
                    Defence = c.Byte(nullable: false),
                    Experience = c.Long(nullable: false),
                    HasSkin = c.Boolean(nullable: false),
                    IsSummonable = c.Boolean(nullable: false),
                    Level = c.Byte(nullable: false),
                    Loyalty = c.Short(nullable: false),
                    MateType = c.Byte(nullable: false),
                    Name = c.String(maxLength: 255)
                })
                .PrimaryKey(t => t.NosmateId);

            DropForeignKey("dbo.Mate", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.Mate", "NpcMonsterVNum", "dbo.NpcMonster");
            DropIndex("dbo.Mate", new[] { "NpcMonsterVNum" });
            DropIndex("dbo.Mate", new[] { "CharacterId" });
            DropTable("dbo.Mate");
            CreateIndex("dbo.Nosmate", "NpcMonsterVNum");
            CreateIndex("dbo.Nosmate", "CharacterId");
            AddForeignKey("dbo.Nosmate", "CharacterId", "dbo.Character", "CharacterId");
            AddForeignKey("dbo.Nosmate", "NpcMonsterVNum", "dbo.NpcMonster", "NpcMonsterVNum");
        }

        public override void Up()
        {
            DropForeignKey("dbo.Nosmate", "NpcMonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.Nosmate", "CharacterId", "dbo.Character");
            DropIndex("dbo.Nosmate", new[] { "CharacterId" });
            DropIndex("dbo.Nosmate", new[] { "NpcMonsterVNum" });
            CreateTable(
                "dbo.Mate",
                c => new
                {
                    MateId = c.Long(nullable: false, identity: true),
                    Attack = c.Byte(nullable: false),
                    CanPickUp = c.Boolean(nullable: false),
                    CharacterId = c.Long(nullable: false),
                    NpcMonsterVNum = c.Short(nullable: false),
                    Defence = c.Byte(nullable: false),
                    Experience = c.Long(nullable: false),
                    HasSkin = c.Boolean(nullable: false),
                    IsSummonable = c.Boolean(nullable: false),
                    Level = c.Byte(nullable: false),
                    Loyalty = c.Short(nullable: false),
                    MateType = c.Byte(nullable: false),
                    Name = c.String(maxLength: 255)
                })
                .PrimaryKey(t => t.MateId)
                .ForeignKey("dbo.NpcMonster", t => t.NpcMonsterVNum)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId)
                .Index(t => t.NpcMonsterVNum);

            DropTable("dbo.Nosmate");
        }

        #endregion
    }
}