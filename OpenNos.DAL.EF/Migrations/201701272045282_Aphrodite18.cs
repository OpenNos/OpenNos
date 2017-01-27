namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite18 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Account", "LastSession");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Account", "LastSession", c => c.Int(nullable: false));
        }
    }
}
