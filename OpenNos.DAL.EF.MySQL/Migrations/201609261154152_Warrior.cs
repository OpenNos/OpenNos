namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Warrior : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Mail", "EqPacket", c => c.String(maxLength: 255, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Mail", "EqPacket", c => c.String(unicode: false));
        }
    }
}
