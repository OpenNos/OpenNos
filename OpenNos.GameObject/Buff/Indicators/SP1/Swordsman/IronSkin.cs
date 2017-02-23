using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Swordsman
{
    public class IronSkin : IndicatorBase
    {
        #region Instantiation

        public IronSkin(int Level)
        {
            Name = "Iron Skin";
            Duration = 300;
            Id = 71;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.DecreaseMeleePercentage, 20, 0, false, true));
            DirectBuffs.Add(new BCardEntry(Type.Damage, SubType.DecreaseDistancePercentage, 65, 0, false, true));
            DelayedBuffs.Add(new BCardEntry(Type.Cooldown, SubType.DecreasePercentage, 15, 0, false));
            Delay = 2;
        }

        #endregion
    }
}