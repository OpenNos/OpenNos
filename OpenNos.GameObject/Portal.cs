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

using OpenNos.Data;
using System;

namespace OpenNos.GameObject
{
    public class Portal : PortalDTO
    {
        #region Members

        private Guid destinationMapInstanceNodeId;
        private Guid sourceMapInstanceNodeId;

        #endregion

        #region Properties

        public Guid DestinationMapInstanceNodeId
        {
            get
            {
                if (destinationMapInstanceNodeId == default(Guid))
                {
                    destinationMapInstanceNodeId = ServerManager.Instance.GetBaseMapInstanceNodeIdByMapId(DestinationMapId);
                }
                return destinationMapInstanceNodeId;
            }
            set { destinationMapInstanceNodeId = value; }
        }

        public Guid SourceMapInstanceNodeId
        {
            get
            {
                if (sourceMapInstanceNodeId == default(Guid))
                {
                    sourceMapInstanceNodeId = ServerManager.Instance.GetBaseMapInstanceNodeIdByMapId(SourceMapId);
                }
                return sourceMapInstanceNodeId;
            }
            set { sourceMapInstanceNodeId = value; }
        }

        #endregion
    }
}