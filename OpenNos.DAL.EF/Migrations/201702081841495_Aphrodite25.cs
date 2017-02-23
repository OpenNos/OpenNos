using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite25 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.MinilandObject", "Durability", c => c.Int(nullable: false));
        }

        public override void Up()
        {
            DropColumn("dbo.MinilandObject", "Durability");
        }

        #endregion
    }
}