using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.SP3.Archer
{
    public class BoosterOn : IndicatorBase
    {
        public BoosterOn(int Level)
        {
            Name = "Booster On";
            Duration = 50;
            Id = 136;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Speed, BCard.SubType.Increase, 10, 0, false));
        }

        public override void Disable(ClientSession session)
        {
            base.Disable(session);
            IndicatorBase buff = new Haste(_level);
            session.Character.Buff.Add(buff);
        }
    }
}
