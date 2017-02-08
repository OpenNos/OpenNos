namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite25 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.MinilandObject", "Durability");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MinilandObject", "Durability", c => c.Int(nullable: false));
        }
    }
}
