namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Vestfalia : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ShopItem", "Rare", c => c.SByte(nullable: false));
            AlterColumn("dbo.ItemInstance", "Rare", c => c.SByte(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItemInstance", "Rare", c => c.Byte(nullable: false));
            AlterColumn("dbo.ShopItem", "Rare", c => c.Byte(nullable: false));
        }
    }
}
