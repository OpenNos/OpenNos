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
using System.Linq;
using System.Reactive.Linq;
using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class Buff
    {
        #region Members
        
        public int Level;
        public Card Card { get; set; }
        public bool StaticBuff { get; set; }
        public int RemainingTime { get; set; }
        public DateTime Start { get; set; }

        public Buff(int id, byte level)
        {
            Card = ServerManager.Instance.Cards.FirstOrDefault(s => s.CardId == id);
            Level = level;
        }
        

        #endregion

    }
}