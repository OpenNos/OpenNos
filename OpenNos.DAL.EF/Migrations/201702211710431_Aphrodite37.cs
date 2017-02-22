using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite37 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Mate", "Mp");
            DropColumn("dbo.Mate", "Hp");
            DropColumn("dbo.Mate", "IsTeamMember");
        }

        public override void Up()
        {
            AddColumn("dbo.Mate", "IsTeamMember", c => c.Boolean(nullable: false));
            AddColumn("dbo.Mate", "Hp", c => c.Int(nullable: false));
            AddColumn("dbo.Mate", "Mp", c => c.Int(nullable: false));
        }

        #endregion
    }
}