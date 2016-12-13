namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RecipeItem", "Amount", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RecipeItem", "Amount", c => c.Byte(nullable: false));
        }
    }
}
