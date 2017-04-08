namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite39 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Character", "MaxMateCount");
        }

        public override void Up()
        {
            AddColumn("dbo.Character", "MaxMateCount", c => c.Byte(nullable: false));
        }

        #endregion
    }
}