namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Hestia1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItemCard", "CardId", "dbo.Card");
            DropForeignKey("dbo.ItemCard", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.SkillCard", "CardId", "dbo.Card");
            DropForeignKey("dbo.SkillCard", "SkillVNum", "dbo.Skill");
            DropIndex("dbo.SkillCard", new[] { "SkillVNum" });
            DropIndex("dbo.SkillCard", new[] { "CardId" });
            DropIndex("dbo.ItemCard", new[] { "ItemVNum" });
            DropIndex("dbo.ItemCard", new[] { "CardId" });
            CreateTable(
                "dbo.BCard",
                c => new
                    {
                        BCardId = c.Short(nullable: false, identity: true),
                        SubType = c.Byte(nullable: false),
                        Type = c.Byte(nullable: false),
                        FirstData = c.Int(nullable: false),
                        SecondData = c.Int(nullable: false),
                        CardId = c.Short(),
                        ItemVNum = c.Short(),
                        SkillVNum = c.Short(),
                        NpcMonsterVNum = c.Short(),
                        IsDelayed = c.Boolean(nullable: false),
                        Delay = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.BCardId)
                .ForeignKey("dbo.Card", t => t.CardId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .ForeignKey("dbo.NpcMonster", t => t.NpcMonsterVNum)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.CardId)
                .Index(t => t.ItemVNum)
                .Index(t => t.SkillVNum)
                .Index(t => t.NpcMonsterVNum);
            
            AddColumn("dbo.Card", "Delay", c => c.Int(nullable: false));
            AddColumn("dbo.Card", "TimeoutBuff", c => c.Short(nullable: false));
            AddColumn("dbo.Card", "TimeoutBuffChance", c => c.Byte(nullable: false));
            AddColumn("dbo.Card", "BuffType", c => c.Byte(nullable: false));
            AddColumn("dbo.StaticBuff", "CardId", c => c.Short(nullable: false));
            CreateIndex("dbo.StaticBuff", "CardId");
            AddForeignKey("dbo.StaticBuff", "CardId", "dbo.Card", "CardId");
            DropColumn("dbo.Skill", "Damage");
            DropColumn("dbo.Skill", "ElementalDamage");
            DropColumn("dbo.Skill", "SecondarySkillVNum");
            DropColumn("dbo.Skill", "SkillChance");
            DropColumn("dbo.Card", "FirstData");
            DropColumn("dbo.Card", "Period");
            DropColumn("dbo.Card", "SecondData");
            DropColumn("dbo.Card", "SubType");
            DropColumn("dbo.Card", "Type");
            DropColumn("dbo.StaticBuff", "EffectId");
            DropTable("dbo.SkillCard");
            DropTable("dbo.ItemCard");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItemCard",
                c => new
                    {
                        ItemVNum = c.Short(nullable: false),
                        CardId = c.Short(nullable: false),
                        CardChance = c.Short(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItemVNum, t.CardId });
            
            CreateTable(
                "dbo.SkillCard",
                c => new
                    {
                        SkillVNum = c.Short(nullable: false),
                        CardId = c.Short(nullable: false),
                        CardChance = c.Short(nullable: false),
                    })
                .PrimaryKey(t => new { t.SkillVNum, t.CardId });
            
            AddColumn("dbo.StaticBuff", "EffectId", c => c.Int(nullable: false));
            AddColumn("dbo.Card", "Type", c => c.Short(nullable: false));
            AddColumn("dbo.Card", "SubType", c => c.Byte(nullable: false));
            AddColumn("dbo.Card", "SecondData", c => c.Int(nullable: false));
            AddColumn("dbo.Card", "Period", c => c.Short(nullable: false));
            AddColumn("dbo.Card", "FirstData", c => c.Int(nullable: false));
            AddColumn("dbo.Skill", "SkillChance", c => c.Short(nullable: false));
            AddColumn("dbo.Skill", "SecondarySkillVNum", c => c.Short(nullable: false));
            AddColumn("dbo.Skill", "ElementalDamage", c => c.Short(nullable: false));
            AddColumn("dbo.Skill", "Damage", c => c.Short(nullable: false));
            DropForeignKey("dbo.BCard", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.BCard", "NpcMonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.BCard", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.BCard", "CardId", "dbo.Card");
            DropForeignKey("dbo.StaticBuff", "CardId", "dbo.Card");
            DropIndex("dbo.StaticBuff", new[] { "CardId" });
            DropIndex("dbo.BCard", new[] { "NpcMonsterVNum" });
            DropIndex("dbo.BCard", new[] { "SkillVNum" });
            DropIndex("dbo.BCard", new[] { "ItemVNum" });
            DropIndex("dbo.BCard", new[] { "CardId" });
            DropColumn("dbo.StaticBuff", "CardId");
            DropColumn("dbo.Card", "BuffType");
            DropColumn("dbo.Card", "TimeoutBuffChance");
            DropColumn("dbo.Card", "TimeoutBuff");
            DropColumn("dbo.Card", "Delay");
            DropTable("dbo.BCard");
            CreateIndex("dbo.ItemCard", "CardId");
            CreateIndex("dbo.ItemCard", "ItemVNum");
            CreateIndex("dbo.SkillCard", "CardId");
            CreateIndex("dbo.SkillCard", "SkillVNum");
            AddForeignKey("dbo.SkillCard", "SkillVNum", "dbo.Skill", "SkillVNum");
            AddForeignKey("dbo.SkillCard", "CardId", "dbo.Card", "CardId");
            AddForeignKey("dbo.ItemCard", "ItemVNum", "dbo.Item", "VNum");
            AddForeignKey("dbo.ItemCard", "CardId", "dbo.Card", "CardId");
        }
    }
}
