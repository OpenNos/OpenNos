namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite44 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeSpace", "Script", c => c.String());
            DropColumn("dbo.TimeSpace", "XMLName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TimeSpace", "XMLName", c => c.String());
            DropColumn("dbo.TimeSpace", "Script");
        }
    }
}
