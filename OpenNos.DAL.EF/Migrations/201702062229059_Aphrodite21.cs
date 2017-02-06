namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite21 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MinilandObject", "MinilandObjectVNum", "dbo.Item");
            DropIndex("dbo.MinilandObject", new[] { "MinilandObjectVNum" });
            AddColumn("dbo.MinilandObject", "ItemInstanceId", c => c.Guid());
            CreateIndex("dbo.MinilandObject", "ItemInstanceId");
            AddForeignKey("dbo.MinilandObject", "ItemInstanceId", "dbo.ItemInstance", "Id");
            DropColumn("dbo.MinilandObject", "MinilandObjectVNum");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MinilandObject", "MinilandObjectVNum", c => c.Short(nullable: false));
            DropForeignKey("dbo.MinilandObject", "ItemInstanceId", "dbo.ItemInstance");
            DropIndex("dbo.MinilandObject", new[] { "ItemInstanceId" });
            DropColumn("dbo.MinilandObject", "ItemInstanceId");
            CreateIndex("dbo.MinilandObject", "MinilandObjectVNum");
            AddForeignKey("dbo.MinilandObject", "MinilandObjectVNum", "dbo.Item", "VNum");
        }
    }
}
