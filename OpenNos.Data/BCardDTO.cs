using static OpenNos.Domain.BCardType;

namespace OpenNos.Data
{
    public class BCardDTO : MappingBaseDTO
    {
        public short BCardId { get; set; }

        public byte SubType { get; set; }

        public byte Type { get; set; }

        public int FirstData { get; set; }

        public int SecondData { get; set; }

        public short? CardId { get; set; }

        public short? ItemVnum { get; set; }

        public short? SkillVNum { get; set; }

        public short? NpcMonsterVNum { get; set; }

        public short Delay { get; set; }

        public byte Probability { get; set; }

        public bool IsDelayed { get; set; }
    }
}