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
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class Logger
    {
        #region Members

        private static ILog _log;

        #endregion

        #region Properties

        public static ILog Log
        {
            get
            {
                return _log;
            }
            set
            {
                _log = value;
            }
        }

        #endregion

        #region Methods

        public static void InitializeLogger(ILog log)
        {
            Log = log;
        }

        #endregion

    }
}
