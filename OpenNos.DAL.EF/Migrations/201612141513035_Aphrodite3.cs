using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite3 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.ItemInstance", "HoldingVNum");
        }

        public override void Up()
        {
            AddColumn("dbo.ItemInstance", "HoldingVNum", c => c.Short());
        }

        #endregion
    }
}