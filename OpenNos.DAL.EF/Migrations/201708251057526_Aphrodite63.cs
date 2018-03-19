namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite63 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LogCommands",
                c => new
                    {
                        CommandId = c.Long(nullable: false, identity: true),
                        CharacterId = c.Long(),
                        Command = c.String(),
                        Data = c.String(),
                        IpAddress = c.String(maxLength: 255),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CommandId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LogCommands", "CharacterId", "dbo.Character");
            DropIndex("dbo.LogCommands", new[] { "CharacterId" });
            DropTable("dbo.LogCommands");
        }
    }
}
