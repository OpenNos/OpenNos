namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Alpha : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuicklistEntry", "Morph", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuicklistEntry", "Morph");
        }
    }
}
