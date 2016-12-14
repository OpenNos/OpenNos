// Copyright (c) .NET Foundation. All rights reserved. Licensed under the Apache License, Version
// 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.SignalR;
using OpenNos.Core;
using System;
using System.Linq;

namespace OpenNos.WebApi.SelfHost
{
    public class ServerCommunicationHub : Hub
    {
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
                return ServerCommunicationHelper.Instance.ConnectedAccounts.ContainsKey(accountName);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Cleanup hold Data, this is for restarting the server
        /// </summary>
        public void Cleanup()
        {
            ServerCommunicationHelper.Instance.ConnectedAccounts = null;
            ServerCommunicationHelper.Instance.ConnectedCharacters = null;
            ServerCommunicationHelper.Instance.RegisteredAccountLogins = null;
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
                if (ServerCommunicationHelper.Instance.ConnectedAccounts.ContainsKey(accountName))
                {
                    Logger.Log.InfoFormat($"Account {accountName} is already connected.");
                    return false;
                }
                else
                {
                    // TODO: move in own method, cannot do this here because it needs to be called by
                    //       a client who wants to know if the Account is allowed to connect without
                    // doing it actually
                    Logger.Log.InfoFormat($"Account {accountName} has connected.");
                    ServerCommunicationHelper.Instance.ConnectedAccounts[accountName] = sessionId;

                    // inform clients
                    Clients.All.accountConnected(accountName, sessionId);
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
                if (ServerCommunicationHelper.Instance.ConnectedCharacters.ContainsKey(characterName))
                {
                    Logger.Log.InfoFormat($"Character {characterName} is already connected.");
                    return false;
                }
                else
                {
                    // TODO: move in own method, cannot do this here because it needs to be called by
                    //       a client who wants to know if the character is allowed to connect
                    // without doing it actually
                    Logger.Log.InfoFormat($"Character {characterName} has connected.");
                    ServerCommunicationHelper.Instance.ConnectedCharacters[characterName] = accountName;

                    // inform clients
                    Clients.All.characterConnected(characterName);
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
                ServerCommunicationHelper.Instance.ConnectedAccounts.Remove(accountName);

                // inform clients
                Clients.All.accountDisconnected(accountName);

                Logger.Log.InfoFormat($"Account {accountName} has been disconnected.");
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
        public void DisconnectCharacter(string characterName, long characterId)
        {
            try
            {
                ServerCommunicationHelper.Instance.ConnectedCharacters.Remove(characterName);

                // inform clients
                Clients.All.characterDisconnected(characterName, characterId);

                Logger.Log.InfoFormat($"Character {characterName} has been disconnected.");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        public bool HandleError(Exception error)
        {
            //we do not handle any errors to wrap them up for the client
            return true;
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
                bool successful = ServerCommunicationHelper.Instance.RegisteredAccountLogins.Remove(accountName);

                if (successful)
                {
                    Logger.Log.InfoFormat($"Account {accountName} has lost the permission to login with SessionId {sessionId}.");
                }
                else
                {
                    Logger.Log.InfoFormat($"Account {accountName} is not permitted to login with SessionId {sessionId}.");
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
                if (!ServerCommunicationHelper.Instance.RegisteredAccountLogins.ContainsKey(accountName))
                {
                    ServerCommunicationHelper.Instance.RegisteredAccountLogins[accountName] = sessionId;
                }
                else
                {
                    ServerCommunicationHelper.Instance.RegisteredAccountLogins.Remove(accountName);
                    ServerCommunicationHelper.Instance.RegisteredAccountLogins[accountName] = sessionId;
                }

                Logger.Log.InfoFormat($"Account {accountName} is now permitted to login with SessionId {sessionId}");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        #endregion
    }
}