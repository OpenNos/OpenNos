using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite2 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AlterColumn("dbo.RecipeItem", "Amount", c => c.Byte(nullable: false));
        }

        public override void Up()
        {
            AlterColumn("dbo.RecipeItem", "Amount", c => c.Short(nullable: false));
        }

        #endregion
    }
}