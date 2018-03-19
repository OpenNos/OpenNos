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
    public class RollGeneratedItem
    {
        #region Instantiation

        #endregion

        #region Properties

        [Key]
        public short RollGeneratedItemId { get; set; }
        
        public short OriginalItemDesign { get; set; }

        public virtual Item OriginalItem { get; set; }

        public short OriginalItemVNum { get; set; }

        public short Probability { get; set; }

        public byte ItemGeneratedAmount { get; set; }

        public short ItemGeneratedVNum { get; set; }

        public byte ItemGeneratedUpgrade { get; set; }

        public bool IsRareRandom { get; set; }

        public byte MinimumOriginalItemRare { get; set; }

        public byte MaximumOriginalItemRare { get; set; }

        public virtual Item ItemGenerated { get; set; }

        public bool IsSuperReward { get; set; }

        #endregion
    }
}