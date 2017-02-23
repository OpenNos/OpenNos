using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite38 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Mate", "Direction");
            DropColumn("dbo.Mate", "MapY");
            DropColumn("dbo.Mate", "MapX");
        }

        public override void Up()
        {
            AddColumn("dbo.Mate", "MapX", c => c.Short(nullable: false));
            AddColumn("dbo.Mate", "MapY", c => c.Short(nullable: false));
            AddColumn("dbo.Mate", "Direction", c => c.Byte(nullable: false));
        }

        #endregion
    }
}