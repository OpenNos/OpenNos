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
using OpenNos.Core;
using OpenNos.WCF.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace OpenNos.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CommunicationService : ICommunicationService
    {
        #region Members

        private IDictionary<String, int> _connectedAccounts;
        private IDictionary<String, String> _connectedCharacters;
        private IDictionary<String, long> _registeredAccountLogins;

        #endregion

        #region Instantiation

        public CommunicationService()
        {
            Logger.InitializeLogger(LogManager.GetLogger(typeof(CommunicationService)));
        }

        #endregion

        #region Properties

        public IDictionary<String, int> ConnectedAccounts
        {
            get
            {
                if (_connectedAccounts == null)
                {
                    _connectedAccounts = new Dictionary<String, int>();
                }

                return _connectedAccounts;
            }
        }

        public IDictionary<String, String> ConnectedCharacters
        {
            get
            {
                if (_connectedCharacters == null)
                {
                    _connectedCharacters = new Dictionary<String, String>();
                }

                return _connectedCharacters;
            }
        }

        public IDictionary<String, long> RegisteredAccountLogins
        {
            get
            {
                if (_registeredAccountLogins == null)
                {
                    _registeredAccountLogins = new Dictionary<String, long>();
                }

                return _registeredAccountLogins;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the Account has a connected Character
        /// </summary>
        /// <param name="accountName">Name of the Account</param>
        /// <returns></returns>
        public bool AccountIsConnected(string accountName)
        {
            try
            {
                return ConnectedAccounts.Any(cc => cc.Key.Equals(accountName));
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
                return false;
            }
        }

        /// <summary>
        /// Cleanup hold Data, this is for restarting the server
        /// </summary>
        public void Cleanup()
        {
            _registeredAccountLogins = null;
            _connectedAccounts = null;
            _connectedCharacters = null;
        }

        /// <summary>
        /// Registers that the given Account has now logged in
        /// </summary>
        /// <param name="accountName">Name of the Account.</param>
        /// <param name="sessionId">SessionId of the login.</param>
        public bool ConnectAccount(string accountName, int sessionId)
        {
            try
            {
                // Account cant connect twice
                if (ConnectedAccounts.ContainsKey(accountName))
                {
                    Logger.Log.DebugFormat($"[WCF] Account {accountName} is already connected.");
                    return false;
                }
                else
                {
                    // TODO: move in own method, cannot do this here because it needs to be called by a client who wants to know if the
                    // Account is allowed to connect without doing it actually
                    Logger.Log.DebugFormat($"[WCF] Account {accountName} has connected.");
                    ConnectedAccounts.Add(accountName, sessionId);

                    // inform clients
                    ICommunicationCallback callback = OperationContext.Current.GetCallbackChannel<ICommunicationCallback>();
                    callback.ConnectAccountCallback(accountName, sessionId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
                return false;
            }
        }

        /// <summary>
        /// Registers that the given Character has now logged in
        /// </summary>
        /// <param name="characterName">Name of the Character.</param>
        /// <param name="accountName">Account of the Character to login.</param>
        public bool ConnectCharacter(string characterName, string accountName)
        {
            try
            {
                // character cant connect twice
                if (ConnectedCharacters.ContainsKey(characterName))
                {
                    Logger.Log.DebugFormat($"[WCF] Character {characterName} is already connected.");
                    return false;
                }
                else
                {
                    // TODO: move in own method, cannot do this here because it needs to be called by a client who wants to know if the
                    // character is allowed to connect without doing it actually
                    Logger.Log.DebugFormat($"[WCF] Character {characterName} has connected.");
                    ConnectedCharacters.Add(characterName, accountName);

                    // inform clients
                    ICommunicationCallback callback = OperationContext.Current.GetCallbackChannel<ICommunicationCallback>();
                    callback.ConnectCharacterCallback(characterName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
                return false;
            }
        }

        /// <summary>
        /// Disconnect Account from server.
        /// </summary>
        /// <param name="accountName">Account who wants to disconnect.</param>
        public void DisconnectAccount(string accountName)
        {
            try
            {
                ConnectedAccounts.Remove(accountName);

                // inform clients
                ICommunicationCallback callback = OperationContext.Current.GetCallbackChannel<ICommunicationCallback>();
                callback.DisconnectAccountCallback(accountName);

                Logger.Log.DebugFormat($"[WCF] Account {accountName} has been disconnected.");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        /// <summary>
        /// Disconnect character from server.
        /// </summary>
        /// <param name="characterName">Character who wants to disconnect.</param>
        public void DisconnectCharacter(string characterName)
        {
            try
            {
                ConnectedCharacters.Remove(characterName);

                // inform clients
                ICommunicationCallback callback = OperationContext.Current.GetCallbackChannel<ICommunicationCallback>();
                callback.DisconnectCharacterCallback(characterName);

                Logger.Log.DebugFormat($"[WCF] Character {characterName} has been disconnected.");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        /// <summary>
        /// Checks if the Account is allowed to login, removes the permission to login
        /// </summary>
        /// <param name="accountName">Name of the Account.</param>
        /// <param name="sessionId">SessionId to check for validity.</param>
        /// <returns></returns>
        public bool HasRegisteredAccountLogin(string accountName, long sessionId)
        {
            try
            {
                // return if the player has been registered
                bool successful = RegisteredAccountLogins.Remove(accountName);

                if (successful)
                {
                    Logger.Log.DebugFormat($"[WCF] Account {accountName} has lost the permission to login with SessionId {sessionId}.");
                }
                else
                {
                    Logger.Log.DebugFormat($"[WCF] Account {accountName} is not permitted to login with SessionId {sessionId}.");
                }

                return successful;
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }

            return false;
        }

        /// <summary>
        /// Register Account for Login (Verification for Security)
        /// </summary>
        /// <param name="accountName">Name of the Account</param>
        /// <param name="sessionId">SessionId for the valid connection.</param>
        public void RegisterAccountLogin(string accountName, long sessionId)
        {
            try
            {
                if (!RegisteredAccountLogins.ContainsKey(accountName))
                {
                    RegisteredAccountLogins.Add(accountName, sessionId);
                }
                else
                {
                    RegisteredAccountLogins.Remove(accountName);
                    RegisteredAccountLogins.Add(accountName, sessionId);
                }

                Logger.Log.DebugFormat($"[WCF] Account {accountName} is now permitted to login with SessionId {sessionId}");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        #endregion
    }
}