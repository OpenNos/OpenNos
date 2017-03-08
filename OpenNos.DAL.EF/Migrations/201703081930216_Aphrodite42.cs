namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite42 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ItemInstance", "Design", c => c.Byte(nullable: false));
            DropColumn("dbo.Mail", "AttachmentDesign");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Mail", "AttachmentDesign", c => c.Short());
            AlterColumn("dbo.ItemInstance", "Design", c => c.Short(nullable: false));
        }
    }
}
