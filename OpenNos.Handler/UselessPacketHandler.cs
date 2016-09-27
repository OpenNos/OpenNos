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

using OpenNos.Core;
using OpenNos.GameObject;

namespace OpenNos.Handler
{
    public class UselessPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public UselessPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session
        {
            get
            {
                return _session;
            }
        }

        #endregion

        #region Methods

        [Packet("c_close")]
        public void CClose(string packet)
        {
            // idk
        }

        [Packet("f_stash_end")]
        public void FStashEnd(string packet)
        {
            // idk
        }

        [Packet("lbs")]
        public void Lbs(string packet)
        {
            // idk
        }

        [Packet("shopclose")]
        public void ShopClose(string packet)
        {
            // Not needed for now.
        }

        [Packet("snap")]
        public void Snap(string packet)
        {
            // Not needed for now. (pictures)
        }

        #endregion
    }
}