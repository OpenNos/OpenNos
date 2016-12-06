namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "VerificationToken", c => c.String(maxLength: 32));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "VerificationToken");
        }
    }
}
