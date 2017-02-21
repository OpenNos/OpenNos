using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Magician
{
    public class ManaTransfusion : IndicatorBase
    {
        public ManaTransfusion(int Level)
        {
            Name = "Mana Transfusion";
            Duration = 1800;
            Id = 370;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.DecreasePercentage, 20, 0, false, true));
        }
    }
}
