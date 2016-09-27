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

using OpenNos.ServiceRef.Internal.CommunicationServiceReference;
using System;

namespace OpenNos.ServiceRef.Internal
{
    public class CommunicationCallback : ICommunicationServiceCallback, IDisposable
    {
        #region Events

        public event EventHandler AccountConnectedEvent;

        public event EventHandler AccountDisconnectedEvent;

        public event EventHandler CharacterConnectedEvent;

        public event EventHandler CharacterDisconnectedEvent;

        #endregion

        #region Methods

        public void ConnectAccountCallback(string accountName, int sessionId)
        {
            OnAccountConnected(accountName);
        }

        public void ConnectCharacterCallback(string characterName)
        {
            // inform clients about a new connected character
            OnCharacterConnected(characterName);
        }

        public void DisconnectAccountCallback(string accountName)
        {
            OnAccountDisconnected(accountName);
        }

        public void DisconnectCharacterCallback(string characterName)
        {
            // inform clients about a disconnected character
            OnCharacterDisconnected(characterName);
        }

        public void Dispose()
        {
            // dispose communication callback service
        }

        public void OnAccountDisconnected(string accountName)
        {
            if (AccountDisconnectedEvent != null && !String.IsNullOrEmpty(accountName))
            {
                AccountDisconnectedEvent(accountName, new EventArgs());
            }
        }

        public void OnCharacterDisconnected(string characterName)
        {
            if (CharacterDisconnectedEvent != null && !String.IsNullOrEmpty(characterName))
            {
                CharacterDisconnectedEvent(characterName, new EventArgs());
            }
        }

        private void OnAccountConnected(string accountName)
        {
            if (AccountConnectedEvent != null && !String.IsNullOrEmpty(accountName))
            {
                AccountConnectedEvent(accountName, new EventArgs());
            }
        }

        private void OnCharacterConnected(string characterName)
        {
            if (CharacterConnectedEvent != null && !String.IsNullOrEmpty(characterName))
            {
                CharacterConnectedEvent(characterName, new EventArgs());
            }
        }

        #endregion
    }
}