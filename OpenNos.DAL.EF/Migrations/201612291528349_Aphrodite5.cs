using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite5 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Character", "BlockFXp");
            DropColumn("dbo.Character", "BlockRep");
            DropColumn("dbo.Character", "BlockExp");
        }

        public override void Up()
        {
            AddColumn("dbo.Character", "BlockExp", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "BlockRep", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "BlockFXp", c => c.Boolean(nullable: false));
        }

        #endregion
    }
}