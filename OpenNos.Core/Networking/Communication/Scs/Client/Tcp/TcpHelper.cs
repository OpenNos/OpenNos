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
using System.Net;
using System.Net.Sockets;

namespace OpenNos.Core.Networking.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to simplify TCP socket operations.
    /// </summary>
    public static class TcpHelper
    {
        #region Methods

        /// <summary>
        /// This code is used to connect to a TCP socket with timeout option.
        /// </summary>
        /// <param name="endPoint">IP endpoint of remote server</param>
        /// <param name="timeoutMs">Timeout to wait until connect</param>
        /// <returns>Socket object connected to server</returns>
        /// <exception cref="SocketException">Throws SocketException if can not connect.</exception>
        /// <exception cref="TimeoutException">
        /// Throws TimeoutException if can not connect within specified timeoutMs
        /// </exception>
        public static Socket ConnectToServer(EndPoint endPoint, int timeoutMs)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Blocking = false;
                socket.Connect(endPoint);
                socket.Blocking = true;
                return socket;
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode != 10035)
                {
                    socket.Close();
                    throw;
                }

                if (!socket.Poll(timeoutMs * 1000, SelectMode.SelectWrite))
                {
                    socket.Close();
                    throw new TimeoutException("The host failed to connect. Timeout occured.");
                }

                socket.Blocking = true;
                return socket;
            }
        }

        #endregion
    }
}