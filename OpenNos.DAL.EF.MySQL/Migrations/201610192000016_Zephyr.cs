namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Zephyr : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Item", "EquipmentSlot", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Item", "EquipmentSlot", c => c.Short(nullable: false));
        }
    }
}
