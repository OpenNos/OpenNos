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

using OpenNos.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static OpenNos.Domain.BCardType;

namespace OpenNos.DAL.EF
{
    public class Card
    {
        #region Instantiation

        public Card()
        {
            BCards = new HashSet<BCard>();
        }

        #endregion

        #region Properties

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CardId { get; set; }

        public int Duration { get; set; }

        public int EffectId { get; set; }   

        public byte Level { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public int Delay { get; set; }

        public short TimeoutBuff { get; set; } 

        public byte TimeoutBuffChance { get; set; }

        public CardType BuffType { get; set; }

        public byte Propability { get; set; }   

        public virtual ICollection<BCard> BCards { get; set; }

        public virtual ICollection<StaticBuff> StaticBuff { get; set; }

        #endregion
    }
}