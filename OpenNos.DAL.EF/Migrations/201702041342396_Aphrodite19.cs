using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite19 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.SkillCard", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.SkillCard", "CardId", "dbo.Card");
            DropIndex("dbo.SkillCard", new[] { "CardId" });
            DropIndex("dbo.SkillCard", new[] { "SkillVNum" });
            DropTable("dbo.Card");
            DropTable("dbo.SkillCard");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.SkillCard",
                c => new
                {
                    SkillVNum = c.Short(nullable: false),
                    CardId = c.Short(nullable: false),
                    CardChance = c.Short(nullable: false)
                })
                .PrimaryKey(t => new { t.SkillVNum, t.CardId })
                .ForeignKey("dbo.Card", t => t.CardId)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.SkillVNum)
                .Index(t => t.CardId);

            CreateTable(
                "dbo.Card",
                c => new
                {
                    CardId = c.Short(nullable: false),
                    Duration = c.Int(nullable: false),
                    EffectId = c.Int(nullable: false),
                    FirstData = c.Short(nullable: false),
                    Level = c.Byte(nullable: false),
                    Name = c.String(maxLength: 255),
                    Period = c.Short(nullable: false),
                    Propability = c.Byte(nullable: false),
                    SecondData = c.Short(nullable: false),
                    SubType = c.Byte(nullable: false),
                    Type = c.Byte(nullable: false)
                })
                .PrimaryKey(t => t.CardId);
        }

        #endregion
    }
}