namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite47 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.ScriptedInstance", "WinnerScore", c => c.Int(nullable: false));
            AddColumn("dbo.ScriptedInstance", "Winner", c => c.String(maxLength: 255));
        }

        public override void Up()
        {
            DropColumn("dbo.ScriptedInstance", "Winner");
            DropColumn("dbo.ScriptedInstance", "WinnerScore");
        }

        #endregion
    }
}