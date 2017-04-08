namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite46 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                "dbo.TimeSpace",
                c => new
                {
                    TimespaceId = c.Short(nullable: false, identity: true),
                    MapId = c.Short(nullable: false),
                    PositionX = c.Short(nullable: false),
                    PositionY = c.Short(nullable: false),
                    Winner = c.String(maxLength: 255),
                    Script = c.String(),
                    WinnerScore = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.TimespaceId);

            DropForeignKey("dbo.ScriptedInstance", "MapId", "dbo.Map");
            DropIndex("dbo.ScriptedInstance", new[] { "MapId" });
            DropTable("dbo.ScriptedInstance");
            CreateIndex("dbo.TimeSpace", "MapId");
            AddForeignKey("dbo.TimeSpace", "MapId", "dbo.Map", "MapId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.TimeSpace", "MapId", "dbo.Map");
            DropIndex("dbo.TimeSpace", new[] { "MapId" });
            CreateTable(
                "dbo.ScriptedInstance",
                c => new
                {
                    ScriptedInstanceId = c.Short(nullable: false, identity: true),
                    Type = c.Byte(nullable: false),
                    MapId = c.Short(nullable: false),
                    PositionX = c.Short(nullable: false),
                    PositionY = c.Short(nullable: false),
                    Winner = c.String(maxLength: 255),
                    Script = c.String(),
                    WinnerScore = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.ScriptedInstanceId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .Index(t => t.MapId);

            DropTable("dbo.TimeSpace");
        }

        #endregion
    }
}