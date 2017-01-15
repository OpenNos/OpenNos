using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff.BCard
{
    public enum SubType
    {
        //Static Increase
        Increase,
        IncreaseMelee,
        IncreaseDistance,
        IncreaseMagic,
        IncreaseFire,
        IncreaseWater,
        IncreaseLight,
        IncreaseShadow,
        IncreaseLevel,
        IncreaseCriticalDamage,
        IncreaseCriticalChance,
        AlwaysCritical,
        //Increase by Percentage
        IncreasePercentage,
        IncreaseMeleePercentage,
        IncreaseDistancePercentage,
        IncreaseMagicPercentage,
        //Increase by Percentage and Chance
        IncreasePercentageChance,
        IncreaseMeleePercentageChance,
        IncreaseDistancePercentageChance,
        IncreaseMagicPercentageChance,

        //Static Decrease
        Decrease,
        DecreaseMelee,
        DecreaseDistance,
        DecreaseMagic,
        DecreaseFire,
        DecreaseWater,
        DecreaseLight,
        DecreaseShadow,
        DecreaseLevel,
        DecreaseCriticalDamage,
        DecreaseCriticalChance,
        NeverCritical,
        //Decrease by Percentage
        DecreasePercentage,
        DecreaseMeleePercentage,
        DecreaseDistancePercentage,
        DecreaseMagicPercentage,
        //Decrease by Percentage and Chance
        DecreasePercentageChance,
        DecreaseMeleePercentageChance,
        DecreaseDistancePercentageChance,
        DecreaseMagicPercentageChance,

        //Recover, Steal etc
        Recover,
        RecoverPercentage,
        Steal,
        StealPercentage,
        
        //Effects
        EagleEyes
        
    }
}
