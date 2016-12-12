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
using System.Runtime.CompilerServices;

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

        /// <summary>
        /// Wraps up the message with the CallerMemberName
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sessionId"></param>
        /// <param name="memberName"></param>
        public static void Debug(string message, int sessionId = 0, [CallerMemberName] string memberName = "")
        {
            Log?.Debug($"Session: {sessionId} Method: {memberName} Packet: {message}");
        }

        /// <summary>
        /// Wraps up the error message with the CallerMemberName
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="innerException"></param>
        public static void Error(Exception innerException = null, [CallerMemberName]string memberName = "")
        {
            if (innerException != null)
            {
                Log?.Error($"{memberName}: {innerException.Message}", innerException);
            }
        }

        public static void InitializeLogger(ILog log)
        {
            Log = log;
        }

        public static void InitializeLogger(object p)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}