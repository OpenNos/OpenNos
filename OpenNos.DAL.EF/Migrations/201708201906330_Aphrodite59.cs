namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite59 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LogChat",
                c => new
                    {
                        LogId = c.Long(nullable: false, identity: true),
                        CharacterId = c.Long(),
                        ChatType = c.Byte(nullable: false),
                        ChatMessage = c.String(maxLength: 255),
                        IpAddress = c.String(maxLength: 255),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LogChat", "CharacterId", "dbo.Character");
            DropIndex("dbo.LogChat", new[] { "CharacterId" });
            DropTable("dbo.LogChat");
        }
    }
}
