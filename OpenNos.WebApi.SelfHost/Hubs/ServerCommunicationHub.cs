// Copyright (c) .NET Foundation. All rights reserved. Licensed under the Apache License, Version
// 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.SignalR;
using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Data;
using System;
using System.Collections.Generic;
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
                return ServerCommunicationHelper.Instance.ConnectedAccounts.ContainsKey(accountName.ToLower());
            }
            catch (Exception)
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
        /// <param name="sessionId"></param>
        public bool ConnectAccount(string accountName, long sessionId)
        {
            try
            {
                // Account cant connect twice
                if (ServerCommunicationHelper.Instance.ConnectedAccounts.ContainsKey(accountName.ToLower()))
                {
                    Logger.Log.InfoFormat($"Account {accountName} is already connected.");
                    return false;
                }

                // TODO: move in own method, cannot do this here because it needs to be called by a
                // client who wants to know if the Account is allowed to connect without doing it actually
                Logger.Log.InfoFormat($"Account {accountName} has connected.");
                ServerCommunicationHelper.Instance.ConnectedAccounts[accountName.ToLower()] = sessionId;

                // inform clients
                Clients.All.accountConnected(accountName, sessionId);
                return true;
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

                // TODO: move in own method, cannot do this here because it needs to be called by a
                // client who wants to know if the character is allowed to connect without doing it actually
                Logger.Log.InfoFormat($"Character {characterName} has connected.");
                ServerCommunicationHelper.Instance.ConnectedCharacters[characterName] = accountName;

                // inform clients
                Clients.All.characterConnected(characterName);
                return true;
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
                ServerCommunicationHelper.Instance.ConnectedAccounts.Remove(accountName.ToLower());

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
        /// <param name="characterId">same as for characterName</param>
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
                bool successful = ServerCommunicationHelper.Instance.RegisteredAccountLogins.Remove(accountName.ToLower());

                Logger.Log.InfoFormat(successful
                    ? $"Account {accountName} has lost the permission to login with SessionId {sessionId}."
                    : $"Account {accountName} is not permitted to login with SessionId {sessionId}.");

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
                if (!ServerCommunicationHelper.Instance.RegisteredAccountLogins.ContainsKey(accountName.ToLower()))
                {
                    ServerCommunicationHelper.Instance.RegisteredAccountLogins[accountName.ToLower()] = sessionId;
                }
                else
                {
                    ServerCommunicationHelper.Instance.RegisteredAccountLogins.Remove(accountName.ToLower());
                    ServerCommunicationHelper.Instance.RegisteredAccountLogins[accountName.ToLower()] = sessionId;
                }

                Logger.Log.InfoFormat($"Account {accountName} is now permitted to login with SessionId {sessionId}");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
        }

        public void RegisterWorldserver(string servergroup, ScsTcpEndPoint ipAddress)
        {
            try
            {
                if (!ServerCommunicationHelper.Instance.Worldservers.ContainsKey(servergroup))
                {
                    ServerCommunicationHelper.Instance.Worldservers[servergroup] = new WorldserverGroupDTO { GroupName = servergroup, Addresses = new List<ScsTcpEndPoint>() { ipAddress } };
                    Logger.Log.InfoFormat($"World with address {ipAddress} has been registered to new Servergroup {servergroup}.");
                }
                else if (ServerCommunicationHelper.Instance.Worldservers[servergroup].Addresses.Contains(ipAddress))
                {
                    Logger.Log.InfoFormat($"World with address {ipAddress} is already registered.");
                }
                else
                {
                    ServerCommunicationHelper.Instance.Worldservers[servergroup].Addresses.Add(ipAddress);
                    Logger.Log.InfoFormat($"World with address {ipAddress} has been added to Servergroup {servergroup}.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error($"Registering world {ipAddress} failed.", ex);
            }
        }

        public IEnumerable<WorldserverGroupDTO> RetrieveRegisteredWorldservers()
        {
            return ServerCommunicationHelper.Instance.Worldservers.GetAllItems();
        }

        public void UnregisterWorldserver(string servergroup, ScsTcpEndPoint ipAddress)
        {
            try
            {
                if (ServerCommunicationHelper.Instance.Worldservers.ContainsKey(servergroup) && ServerCommunicationHelper.Instance.Worldservers[servergroup].Addresses.Contains(ipAddress))
                {
                    //servergroup does exist with the given ipaddress
                    ServerCommunicationHelper.Instance.Worldservers[servergroup].Addresses.Remove(ipAddress);
                    Logger.Log.InfoFormat($"World with address {ipAddress} has been unregistered successfully.");

                    if (!ServerCommunicationHelper.Instance.Worldservers[servergroup].Addresses.Any())
                    {
                        ServerCommunicationHelper.Instance.Worldservers.Remove(servergroup);
                        Logger.Log.InfoFormat($"World server group {servergroup} has been removed as no member was left.");
                    }
                }
                else if (!ServerCommunicationHelper.Instance.Worldservers.ContainsKey(servergroup)
                    && !ServerCommunicationHelper.Instance.Worldservers.GetAllItems().Any(sgi => sgi.Addresses.Contains(ipAddress)))
                {
                    //servergroup doesnt exist and world is not in a group named like the given servergroup and in no other
                    Logger.Log.InfoFormat($"World with address {ipAddress} has already been unregistered before.");
                }
                else if (!ServerCommunicationHelper.Instance.Worldservers.ContainsKey(servergroup)
                    && ServerCommunicationHelper.Instance.Worldservers.GetAllItems().Any(sgi => sgi.Addresses.Contains(ipAddress)))
                {
                    //servergroup does not exist but world does run in a different servergroup
                    WorldserverGroupDTO worldserverGroupDto = ServerCommunicationHelper.Instance.Worldservers.GetAllItems().SingleOrDefault(sgi => sgi.Addresses.Contains(ipAddress));
                    worldserverGroupDto?.Addresses.Remove(ipAddress);
                    Logger.Log.InfoFormat($"World with address {ipAddress} has been remove from a different servergroup.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error($"Registering world {ipAddress} failed.", ex);
            }
        }

        #endregion
    }
}