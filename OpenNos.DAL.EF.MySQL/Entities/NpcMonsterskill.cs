namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class NpcMonsterSkill
    {
        #region Properties

        [Key]
        public long CharacterSkillId { get; set; }

        public virtual NpcMonster NpcMonster { get; set; }
        public short NpcMonsterVNum { get; set; }
        public short Rate { get; set; }

        public virtual Skill Skill { get; set; }
        public short SkillVNum { get; set; }

        #endregion
    }
}