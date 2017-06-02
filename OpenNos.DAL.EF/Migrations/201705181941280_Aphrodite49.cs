namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite49 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Card", "Delay", c => c.Int(nullable: false));
            AddColumn("dbo.Card", "TimeoutBuff", c => c.Short(nullable: false));
            AddColumn("dbo.Card", "TimeoutBuffChance", c => c.Byte(nullable: false));
            AddColumn("dbo.Card", "BuffType", c => c.Byte(nullable: false));
            DropColumn("dbo.Card", "Period");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Card", "Period", c => c.Short(nullable: false));
            DropColumn("dbo.Card", "BuffType");
            DropColumn("dbo.Card", "TimeoutBuffChance");
            DropColumn("dbo.Card", "TimeoutBuff");
            DropColumn("dbo.Card", "Delay");
        }
    }
}
