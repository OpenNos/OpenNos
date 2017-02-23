using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite11 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Family", "MemberAuthorityType");
            DropColumn("dbo.Family", "MemberCanGetHistory");
            DropColumn("dbo.Family", "ManagerCanGetHistory");
            DropColumn("dbo.Family", "ManagerAuthorityType");
            DropColumn("dbo.Family", "ManagerCanNotice");
            DropColumn("dbo.Family", "ManagerCanShout");
            DropColumn("dbo.Family", "ManagerCanInvite");
        }

        public override void Up()
        {
            AddColumn("dbo.Family", "ManagerCanInvite", c => c.Boolean(nullable: false));
            AddColumn("dbo.Family", "ManagerCanShout", c => c.Boolean(nullable: false));
            AddColumn("dbo.Family", "ManagerCanNotice", c => c.Boolean(nullable: false));
            AddColumn("dbo.Family", "ManagerAuthorityType", c => c.Byte(nullable: false));
            AddColumn("dbo.Family", "ManagerCanGetHistory", c => c.Boolean(nullable: false));
            AddColumn("dbo.Family", "MemberCanGetHistory", c => c.Boolean(nullable: false));
            AddColumn("dbo.Family", "MemberAuthorityType", c => c.Byte(nullable: false));
        }

        #endregion
    }
}