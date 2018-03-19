namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite65 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Character", "Faction", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Character", "Faction", c => c.Int(nullable: false));
        }
    }
}
