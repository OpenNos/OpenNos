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

namespace OpenNos.Data
{
    public class TimeSpaceDTO : MappingBaseDTO
    {
        #region Properties

        public short TimespaceId { get; set; }

        public short MapId { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public byte LevelMinimum { get; set; }

        public byte LevelMaximum { get; set; }

        public string Winner { get; set; }

        public int WinnerScore { get; set; }

        public string DrawItemGift { get; set; }

        public string BonusItemGift { get; set; }

        public string SpecialItemGift { get; set; }

        public string Label { get; set; }


        #endregion
    }
}