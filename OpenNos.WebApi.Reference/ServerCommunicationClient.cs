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

using Microsoft.AspNet.SignalR.Client;
using System;

namespace OpenNos.WebApi.Reference
{
    public class ServerCommunicationClient
    {
        #region Members

        private const string remoteUrl = "http://localhost:6666/";

        private static ServerCommunicationClient _instance;
        private bool _disposed;
        private HubConnection _hubconnection;
        private IHubProxy _hubProxy;

        #endregion

        #region Events

        public event EventHandler AccountConnectedEvent;

        public event EventHandler AccountDisconnectedEvent;

        public event EventHandler CharacterConnectedEvent;

        public event EventHandler CharacterDisconnectedEvent;

        #endregion

        #region Properties

        public static ServerCommunicationClient Instance
        {
            get
            {
                return _instance ?? (_instance = new ServerCommunicationClient());
            }
        }

        public IHubProxy HubProxy
        {
            get
            {
                if (_hubProxy == null)
                {
                    InitializeAndRegisterCallbacks();
                }

                return _hubProxy;
            }
        }

        #endregion

        #region Methods

        public void DisconnectCharacterCallback(string characterName, long characterId)
        {
            // inform clients about a disconnected character
            OnCharacterDisconnected(characterName, characterId);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public void OnAccountDisconnected(string accountName)
        {
            if (AccountDisconnectedEvent != null && !string.IsNullOrEmpty(accountName))
            {
                AccountDisconnectedEvent(accountName, new EventArgs());
            }
        }

        public void OnCharacterDisconnected(string characterName, long characterId)
        {
            if (CharacterDisconnectedEvent != null && !string.IsNullOrEmpty(characterName))
            {
                CharacterDisconnectedEvent(new System.Collections.Generic.KeyValuePair<string, long>(characterName, characterId), new EventArgs());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose communication callback service
            }
        }

        public void InitializeAndRegisterCallbacks()
        {
            _hubconnection = new HubConnection(remoteUrl);

            _hubProxy = _hubconnection.CreateHubProxy("servercommunicationhub");

            //register callback methods
            _hubProxy.On<string, long>("accountConnected", OnAccountConnected);

            _hubProxy.On<string>("accountDisconnected", OnAccountDisconnected);

            _hubProxy.On<string>("characterConnected", OnCharacterConnected);

            _hubProxy.On<string, long>("characterDisconnected", OnCharacterDisconnected);

            _hubconnection.Start().Wait();
        }

        private void OnAccountConnected(string accountName, long sessionId)
        {
            if (AccountConnectedEvent != null && !string.IsNullOrEmpty(accountName))
            {
                AccountConnectedEvent(new System.Collections.Generic.KeyValuePair<string, long>(accountName, sessionId), new EventArgs());
            }
        }

        private void OnCharacterConnected(string characterName)
        {
            if (CharacterConnectedEvent != null && !string.IsNullOrEmpty(characterName))
            {
                CharacterConnectedEvent(characterName, new EventArgs());
            }
        }

        #endregion
    }
}