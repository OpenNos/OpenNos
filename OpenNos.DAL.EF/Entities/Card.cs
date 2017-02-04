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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    public class Card
    {
        #region Instantiation

        public Card()
        {
            SkillCard = new HashSet<SkillCard>();
        }

        #endregion

        #region Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CardId { get; set; }

        public int Duration { get; set; }

        public int EffectId { get; set; }

        public short FirstData { get; set; }

        public byte Level { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public short Period { get; set; }

        public byte Propability { get; set; }

        public short SecondData { get; set; }

        public virtual ICollection<SkillCard> SkillCard { get; set; }

        public byte SubType { get; set; }

        public byte Type { get; set; }

        #endregion
    }
}