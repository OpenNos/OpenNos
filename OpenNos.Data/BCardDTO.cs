using static OpenNos.Domain.BCardType;

namespace OpenNos.GameObject
{
    public class BCardDTO
    {
        public byte Type { get; set; }
        public byte SubType { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }
}