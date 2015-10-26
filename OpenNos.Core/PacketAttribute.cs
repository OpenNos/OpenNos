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

namespace OpenNos.Core
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class Packet : System.Attribute
    {
        private string _header;
        private int _amount;

        public Packet(string header, int amount = 1)
        {
            this._header = header;
            this._amount = amount;
        }

        #region Properties

        public string Header
        {
            get
            {
                return _header;
            }
        }

        public int Amount
        {
            get
            {
                return _amount;
            }
        }

        #endregion
    }
}
