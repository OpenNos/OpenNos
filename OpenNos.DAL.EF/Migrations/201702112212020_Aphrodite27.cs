namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite27 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Account", "LastCompliment");
            DropColumn("dbo.Character", "LastLogin");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Character", "LastLogin", c => c.DateTime(nullable: false));
            AddColumn("dbo.Account", "LastCompliment", c => c.DateTime(nullable: false));
        }
    }
}
