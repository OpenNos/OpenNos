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
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    public class MapNpc
    {
        #region Instantiation

        public MapNpc()
        {
            Recipe = new HashSet<Recipe>();
            Shop = new HashSet<Shop>();
            Teleporter = new HashSet<Teleporter>();
        }

        #endregion

        #region Properties

        public short Dialog { get; set; }

        public short Effect { get; set; }

        public short EffectDelay { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsMoving { get; set; }

        public bool IsSitting { get; set; }

        public virtual Map Map { get; set; }

        public short MapId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MapNpcId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public virtual NpcMonster NpcMonster { get; set; }

        public short NpcVNum { get; set; }

        public byte Position { get; set; }

        public virtual ICollection<Recipe> Recipe { get; set; }

        public virtual ICollection<Shop> Shop { get; set; }

        public virtual ICollection<Teleporter> Teleporter { get; set; }

        #endregion
    }
}