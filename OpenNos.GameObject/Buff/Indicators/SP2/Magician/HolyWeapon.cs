using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP2.Magician
{
    public class HolyWeapon : IndicatorBase
    {
        #region Instantiation

        public HolyWeapon(int Level)
        {
            Name = "Holy Weapon";
            Duration = 3000;
            Id = 89;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Element, SubType.IncreaseLight, Level * 5, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Resistance, SubType.IncreaseLight, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.Increase, Level * 2, 0, false));
        }

        #endregion
    }
}