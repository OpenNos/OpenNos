using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP4.Swordsman
{
    public class Berserker : IndicatorBase
    {
        #region Instantiation

        public Berserker(int Level)
        {
            Name = "Berserker";
            Duration = 600;
            Id = 149;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreaseMelee, 400, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Speed, SubType.Increase, 2, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Defense, SubType.Decrease, 200, 0, false));

            _level = Level;
            DelayedBuffs.Add(new BCardEntry(Type.Speed, SubType.Increase, 4, 0, false));
            DelayedBuffs.Add(new BCardEntry(Type.Damage, SubType.IncreasePercentage, 20, 0, true));
            Delay = 400;
        }

        #endregion
    }
}