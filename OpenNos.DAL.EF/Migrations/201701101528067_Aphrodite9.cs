using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite9 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.StaticBonus", "CharacterId", "dbo.Character");
            DropIndex("dbo.StaticBonus", new[] { "CharacterId" });
            DropTable("dbo.StaticBonus");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.StaticBonus",
                c => new
                {
                    StaticBonusId = c.Long(nullable: false, identity: true),
                    CharacterId = c.Long(nullable: false),
                    DateEnd = c.DateTime(nullable: false),
                    StaticBonusType = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.StaticBonusId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);
        }

        #endregion
    }
}