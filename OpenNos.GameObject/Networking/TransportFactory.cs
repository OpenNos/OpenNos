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

namespace OpenNos.GameObject
{
    public class TransportFactory
    {
        #region Members

        private static TransportFactory instance;
        private long _lastTransportId = 100000;

        #endregion

        #region Instantiation

        private TransportFactory()
        {
            // do nothing
        }

        #endregion

        #region Properties

        public static TransportFactory Instance => instance ?? (instance = new TransportFactory());

        #endregion

        #region Methods

        public long GenerateTransportId()
        {
            _lastTransportId++;

            if (_lastTransportId >= long.MaxValue)
            {
                _lastTransportId = 0;
            }

            return _lastTransportId;
        }

        #endregion
    }
}