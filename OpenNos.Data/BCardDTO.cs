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

        public short? ItemVNum { get; set; }

        public short? SkillVNum { get; set; }

        public short? NpcMonsterVNum { get; set; }

        public bool IsDelayed { get; set; }

        public short Delay { get; set; }

    }
}