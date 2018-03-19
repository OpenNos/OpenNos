namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite58 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.CellonOption", newName: "EquipmentOption");
            AddColumn("dbo.Family", "FamilyFaction", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Family", "FamilyFaction");
            RenameTable(name: "dbo.EquipmentOption", newName: "CellonOption");
        }
    }
}
