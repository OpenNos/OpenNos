/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

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