namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "Email", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "Email");
        }
    }
}
