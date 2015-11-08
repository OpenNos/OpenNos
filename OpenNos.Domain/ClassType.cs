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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public enum ClassType : byte
    {
        Adventurer = 0,
        Swordman = 1,
        Archer = 2,
        Magician = 3
    }
    public enum GenderType : byte
    {
        Male = 0,
        Female = 1
    }
    public enum HairColorType : byte
    {
        DarkPurple = 0,
        Yello = 1,
        Blue = 2,
        Purple = 3,
        Orange = 4,
        Brown = 5,
        Green = 6,
        Black = 7,
        Grey = 8,
        Red = 9
      
    }
    public enum HairStyleType : byte
    {
        HairTypeA = 0,
        HairTypeB = 1           
    }
}
