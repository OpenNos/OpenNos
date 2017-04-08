namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite45 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AlterColumn("dbo.TimeSpace", "Winner", c => c.String());
        }

        public override void Up()
        {
            AlterColumn("dbo.TimeSpace", "Winner", c => c.String(maxLength: 255));
        }

        #endregion
    }
}