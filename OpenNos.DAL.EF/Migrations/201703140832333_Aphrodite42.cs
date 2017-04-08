namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite42 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.TimeSpace", "MapId", "dbo.Map");
            DropIndex("dbo.TimeSpace", new[] { "MapId" });
            DropTable("dbo.TimeSpace");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.TimeSpace",
                c => new
                {
                    TimespaceId = c.Short(nullable: false, identity: true),
                    MapId = c.Short(nullable: false),
                    PositionX = c.Short(nullable: false),
                    PositionY = c.Short(nullable: false),
                    LevelMinimum = c.Int(nullable: false),
                    LevelMaximum = c.Int(nullable: false),
                    Winner = c.String(),
                    DrawItemGift = c.String(),
                    BonusItemGift = c.String(),
                    SpecialItemGift = c.String(),
                    Label = c.String(),
                    WinnerScore = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.TimespaceId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .Index(t => t.MapId);
        }

        #endregion
    }
}