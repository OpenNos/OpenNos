using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite17 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.StaticBuff", "CharacterId", "dbo.Character");
            DropIndex("dbo.StaticBuff", new[] { "CharacterId" });
            DropColumn("dbo.Character", "MinilandMessage");
            DropColumn("dbo.Character", "MinilandState");
            DropTable("dbo.StaticBuff");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.StaticBuff",
                c => new
                {
                    StaticBuffId = c.Long(nullable: false, identity: true),
                    CharacterId = c.Long(nullable: false),
                    EffectId = c.Int(nullable: false),
                    RemainingTime = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.StaticBuffId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);

            AddColumn("dbo.Character", "MinilandState", c => c.Byte(nullable: false));
            AddColumn("dbo.Character", "MinilandMessage", c => c.String(maxLength: 255));
        }

        #endregion
    }
}