using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.Indicators.Item
{
    public class ExperiencePotion : IndicatorBase
    {
        public ExperiencePotion(int Level)
        {
            Name = "Experience Increase";
            Duration = 6000;
            Id = 119;
            _level = Level;
            DirectBuffs.Add(new BCardEntry(BCard.Type.Experience, BCard.SubType.IncreasePercentage, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.JobExperience, BCard.SubType.IncreasePercentage, 20, 0, false));
            DirectBuffs.Add(new BCardEntry(BCard.Type.SPExperience, BCard.SubType.IncreasePercentage, 20, 0, false));
        }
    }
}
