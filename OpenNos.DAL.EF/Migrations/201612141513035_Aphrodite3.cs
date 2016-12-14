namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItemInstance", "HoldingVNum", c => c.Short());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItemInstance", "HoldingVNum");
        }
    }
}
