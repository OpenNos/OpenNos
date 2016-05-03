namespace OpenNos.DAL.EF.MySQL
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class CharacterSkill
    {
        #region Properties

        public virtual Character Character { get; set; }
        public long CharacterId { get; set; }
        public long CharacterSkillId { get; set; }

        public virtual Skill Skill { get; set; }
        public short SkillVNum { get; set; }

        #endregion
    }
}