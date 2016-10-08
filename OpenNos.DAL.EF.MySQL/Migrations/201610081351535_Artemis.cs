namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Artemis : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Mail", name: "ItemVNum", newName: "AttachmentVnum");
            RenameIndex(table: "dbo.Mail", name: "IX_ItemVNum", newName: "IX_AttachmentVnum");
            AddColumn("dbo.Mail", "AttachmentAmount", c => c.Byte(nullable: false));
            AddColumn("dbo.Mail", "AttachmentRarity", c => c.Byte(nullable: false));
            AddColumn("dbo.Mail", "AttachmentUpgrade", c => c.Byte(nullable: false));
            DropColumn("dbo.Mail", "Amount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Mail", "Amount", c => c.Byte(nullable: false));
            DropColumn("dbo.Mail", "AttachmentUpgrade");
            DropColumn("dbo.Mail", "AttachmentRarity");
            DropColumn("dbo.Mail", "AttachmentAmount");
            RenameIndex(table: "dbo.Mail", name: "IX_AttachmentVnum", newName: "IX_ItemVNum");
            RenameColumn(table: "dbo.Mail", name: "AttachmentVnum", newName: "ItemVNum");
        }
    }
}
