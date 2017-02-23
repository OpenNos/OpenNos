using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite6 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Character", "BlockFXp", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "BlockRep", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "BlockExp", c => c.Boolean(nullable: false));
        }

        public override void Up()
        {
            DropColumn("dbo.Character", "BlockExp");
            DropColumn("dbo.Character", "BlockRep");
            DropColumn("dbo.Character", "BlockFXp");
        }

        #endregion
    }
}