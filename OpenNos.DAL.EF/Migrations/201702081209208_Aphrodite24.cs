namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite24 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "MinilandPoint", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Character", "MinilandPoint");
        }
    }
}
