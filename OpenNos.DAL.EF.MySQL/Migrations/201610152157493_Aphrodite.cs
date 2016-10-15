namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Item", "DarkResistance", c => c.Short(nullable: false));
            AlterColumn("dbo.Item", "FireResistance", c => c.Short(nullable: false));
            AlterColumn("dbo.Item", "LightResistance", c => c.Short(nullable: false));
            AlterColumn("dbo.Item", "WaterResistance", c => c.Short(nullable: false));
            AlterColumn("dbo.ItemInstance", "Rare", c => c.SByte(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItemInstance", "Rare", c => c.Byte(nullable: false));
            AlterColumn("dbo.Item", "WaterResistance", c => c.Byte(nullable: false));
            AlterColumn("dbo.Item", "LightResistance", c => c.Byte(nullable: false));
            AlterColumn("dbo.Item", "FireResistance", c => c.Byte(nullable: false));
            AlterColumn("dbo.Item", "DarkResistance", c => c.Byte(nullable: false));
        }
    }
}
