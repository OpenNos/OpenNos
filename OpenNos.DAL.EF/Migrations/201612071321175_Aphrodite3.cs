namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "RegistrationIP", c => c.String(maxLength: 45));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "RegistrationIP");
        }
    }
}
