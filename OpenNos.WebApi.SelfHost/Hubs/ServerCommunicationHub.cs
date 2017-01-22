// Copyright (c) .NET Foundation. All rights reserved. Licensed under the Apache License, Version
// 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.SignalR;
using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Data;
using OpenNos.Domain;
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
                //iterate thru all registered servers and check if the given account logged in
                return ServerCommunicationHelper.Instance.Worldservers.Any(c => c.ConnectedAccounts.ContainsKey(accountName.ToLower()));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Refresh Family
        /// </summary>
        public void FamilyRefresh(long FamilyId)
        {
            Clients.All.refreshFamily(FamilyId);
        }
        /// <summary>
        /// Refresh Bazaar
        /// </summary>
        public void BazaarRefresh(long BazaarItemId)
        {
            Clients.All.refreshBazaar(BazaarItemId);
        }
        /// <summary>
        /// Refresh Ranking
        /// </summary>
        public void RankingRefresh()
        {
            Clients.All.refreshRanking();
        }
        /// <summary>
        /// Cleanup hold Data, this is for restarting the server
        /// </summary>
        public void Cleanup()
        {
            ServerCommunicationHelper.Instance.RegisteredAccountLogins = null;
        }

        /// <summary>
        /// Registers that the given Account has now logged in
        /// </summary>
        /// <param name="accountName">Name of the Account.</param>
        /// <param name="sessionId"></param>
        public bool ConnectAccount(Guid worldId, string accountName, long sessionId)
        {
            try
            {
                // Account cant connect twice
                if (AccountIsConnected(accountName))
                {
                    Logger.Log.InfoFormat($"Account {accountName} is already connected.");
                    return false;
                }

                // TODO: move in own method, cannot do this here because it needs to be called by a
                // client who wants to know if the Account is allowed to connect without doing it actually
                Logger.Log.InfoFormat($"Account {accountName} has been connected to world {worldId}.");
                ServerCommunicationHelper.Instance.Worldservers.SingleOrDefault(w => w.Id == worldId).ConnectedAccounts[accountName.ToLower()] = sessionId;

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
        /// <param name="characterId">If of the Character.</param>
        public bool ConnectCharacter(Guid worldId, string characterName, long characterId)
        {
            try
            {
                // character cant connect twice
                if (ServerCommunicationHelper.Instance.Worldservers.SingleOrDefault(w => w.Id == worldId).ConnectedCharacters.ContainsKey(characterName))
                {
                    Logger.Log.InfoFormat($"Character {characterName} is already connected to world {worldId}.");
                    return false;
                }

                // TODO: move in own method, cannot do this here because it needs to be called by a
                // client who wants to know if the character is allowed to connect without doing it actually
                Logger.Log.InfoFormat($"Character {characterName} has connected to world {worldId}.");
                ServerCommunicationHelper.Instance.Worldservers.SingleOrDefault(w => w.Id == worldId).ConnectedCharacters[characterName] = characterId;

                // inform clients
                Clients.All.characterConnected(characterName, characterId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log.Error($"Error while connecting Character to world {worldId}", ex);
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
                WorldserverDTO worldserver = ServerCommunicationHelper.Instance.Worldservers.SingleOrDefault(s => s.ConnectedAccounts.ContainsKey(accountName.ToLower()));

                if (worldserver != null)
                {
                    worldserver.ConnectedAccounts.Remove(accountName.ToLower());

                    // inform clients
                    Clients.All.accountDisconnected(accountName);

                    Logger.Log.InfoFormat($"Account {accountName} has been disconnected.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while disconnecting account.", ex);
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
                WorldserverDTO worldserver = ServerCommunicationHelper.Instance.Worldservers.SingleOrDefault(s => s.ConnectedCharacters.ContainsKey(characterName));

                if (worldserver != null)
                {
                    worldserver.ConnectedCharacters.Remove(characterName);

                    // inform clients
                    Clients.All.characterDisconnected(characterName, characterId);

                    Logger.Log.InfoFormat($"Character {characterName} has been disconnected.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while disconnecting character.", ex);
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
                bool successful = ServerCommunicationHelper.Instance.RegisteredAccountLogins.Remove(accountName.ToLower());

                Logger.Log.InfoFormat(successful
                    ? $"Account {accountName} has lost the permission to login with SessionId {sessionId}."
                    : $"Account {accountName} is not permitted to login with SessionId {sessionId}.");

                return successful;
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while registering account login.", ex);
            }

            return false;
        }

        public bool CharacterIsConnected(long CharacterId)
        {
            return ServerCommunicationHelper.Instance.Worldservers.Any(c => c.ConnectedCharacters.ContainsValue(CharacterId));
        }
        public IEnumerable<string> RetrieveServerStatistics()
        {
            List<string> result = new List<string>();
            try
            {
                foreach(WorldserverGroupDTO servergroup in ServerCommunicationHelper.Instance.WorldserverGroups)
                {
                    int serverSessionAmount = 0;
                    foreach(WorldserverDTO world in servergroup.Servers)
                    {
                        int channelSessionAmount = world.ConnectedAccounts.Count();
                        serverSessionAmount += channelSessionAmount;
                        result.Add($"Channel{world.ChannelId}: {channelSessionAmount} sessions.");
                    }

                    result.Add($"Server {servergroup.GroupName}: {serverSessionAmount} sessions.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while retriving server statistics.", ex);
            }

            return result;
        }

        /// <summary>
        /// Kick a session by providing SessionId or AccountName
        /// </summary>
        public void KickSession(long? sessionId, string accountName)
        {
            try
            {
                if (sessionId.HasValue || !string.IsNullOrEmpty(accountName))
                {
                    // inform clients
                    Clients.All.kickSession(sessionId, accountName);

                    if (!string.IsNullOrEmpty(accountName))
                    {
                        DisconnectAccount(accountName);
                    }

                    //release session login
                    if (sessionId.HasValue)
                    {
                        accountName = ServerCommunicationHelper.Instance.Worldservers.SingleOrDefault(w => w.ConnectedAccounts.ContainsValue(sessionId.Value)).ConnectedAccounts
                                                                                            .SingleOrDefault(w => w.Value == sessionId.Value).Key;
                        DisconnectAccount(accountName);
                    }

                    Logger.Log.InfoFormat($"Session {sessionId} {accountName} has been kicked.");
                }
                else
                {
                    Logger.Log.WarnFormat($"Ignored empty kicking event.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
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

        public int? RegisterWorldserver(string groupName, WorldserverDTO worldserver)
        {
            int? newChannelId = null;

            try
            {
                if (!ServerCommunicationHelper.Instance.WorldserverGroups.Any(g => g.GroupName == groupName))
                {
                    //add world server
                    ServerCommunicationHelper.Instance.Worldservers.Add(worldserver);

                    //create new server group
                    worldserver.ChannelId = 1;
                    newChannelId = 1;
                    ServerCommunicationHelper.Instance.WorldserverGroups.Add(new WorldserverGroupDTO(groupName, worldserver));
                    Logger.Log.InfoFormat($"World {worldserver.Id} with address {worldserver.Endpoint} has been registered to new server group {groupName}.");
                }
                else if (ServerCommunicationHelper.Instance.WorldserverGroups.SingleOrDefault(wg => wg.GroupName == groupName)?.Servers.Contains(worldserver) ?? false)
                {
                    //worldserver is already registered
                    Logger.Log.InfoFormat($"World {worldserver.Id} with address {worldserver.Endpoint} is already registered.");
                }
                else
                {
                    //add worldserver to existing group
                    WorldserverGroupDTO worldserverGroup = ServerCommunicationHelper.Instance.WorldserverGroups.SingleOrDefault(wg => wg.GroupName == groupName);

                    //add world server
                    worldserver.ChannelId = worldserverGroup.Servers.Count() + 1;
                    newChannelId = worldserver.ChannelId;
                    ServerCommunicationHelper.Instance.Worldservers.Add(worldserver);
                    worldserverGroup?.Servers.Add(worldserver);
                    Logger.Log.InfoFormat($"World {worldserver.Id} with address {worldserver.Endpoint} has been added to server group {groupName}.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error($"Registering world {worldserver.Id} with endpoint {worldserver.Endpoint} failed.", ex);
            }

            return newChannelId;
        }

        /// <summary>
        /// Build the channel packets
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public string RetrieveRegisteredWorldservers(int sessionId)
        {
            if (ServerCommunicationHelper.Instance.WorldserverGroups.Any())
            {
                int servercount = 1;
                string channelPacket = $"NsTeST {sessionId} ";

                foreach (WorldserverGroupDTO worldserverGroup in ServerCommunicationHelper.Instance.WorldserverGroups)
                {
                    int channelCount = 1;
                    foreach (WorldserverDTO world in worldserverGroup.Servers)
                    {
                        //TODO account limit
                        //int currentlyConnectedAccounts = world.ConnectedAccounts.Count();
                        //int slotsLeft = world.AccountLimit - currentlyConnectedAccounts;
                        //int channelcolor = (world.AccountLimit / slotsLeft) + 1;

                        channelPacket += $"{world.Endpoint.IpAddress}:{world.Endpoint.TcpPort}:1:{servercount}.{channelCount}.{worldserverGroup.GroupName} ";
                        channelCount++;
                    }
                    servercount++;
                }
                return channelPacket;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Send a message to a Character
        /// </summary>
        /// <returns></returns>
        public int? SendMessageToCharacter(string messagePacket, int fromChannel, MessageType messageType, string characterName, int? characterId = null)
        {
            try
            {
                WorldserverDTO worldserver = ServerCommunicationHelper.Instance.Worldservers.SingleOrDefault(c => c.ConnectedCharacters.Any(cc => cc.Key == characterName)
                                                                                                             || characterId.HasValue && c.ConnectedCharacters.ContainsValue(characterId.Value));

                if (worldserver != null)
                {
                    if (string.IsNullOrEmpty(characterName) && characterId.HasValue)
                    {
                        characterName = worldserver.ConnectedCharacters.SingleOrDefault(c => c.Value == characterId.Value).Key;
                    }

                    //character is connected to different world
                    Clients.All.sendMessageToCharacter(characterName, messagePacket, fromChannel, messageType);
                    return worldserver.ChannelId;
                }
                else if (messageType == MessageType.Shout)
                {
                    //send to all registered worlds
                    Clients.All.sendMessageToCharacter(characterName, messagePacket, fromChannel, messageType);
                    return null;
                }
                else if (messageType == MessageType.Family)
                {
                    Clients.All.sendMessageToCharacter(characterName, messagePacket, fromChannel, messageType);
                    return null;
                }
                else if (messageType == MessageType.FamilyChat)
                {
                    Clients.All.sendMessageToCharacter(characterName, messagePacket, fromChannel, messageType);
                    return null;
                }
            }
            catch (Exception)
            {
                Logger.Log.Error("Sending message to character failed.");
                return null;
            }

            return null;
        }

        public void UnregisterWorldserver(string groupName, ScsTcpEndPoint endpoint)
        {
            try
            {
                if (ServerCommunicationHelper.Instance.WorldserverGroups.Any(g => g.GroupName == groupName)
                    && ServerCommunicationHelper.Instance.WorldserverGroups.SingleOrDefault(g => g.GroupName == groupName).Servers.Any(w => w.Endpoint.Equals(endpoint)))
                {
                    //servergroup does exist with the given ipaddress
                    RemoveWorldFromWorldserver(endpoint, groupName);
                }
                else if (!ServerCommunicationHelper.Instance.WorldserverGroups.Any(g => g.GroupName == groupName)
                    && !ServerCommunicationHelper.Instance.WorldserverGroups.Any(sgi => sgi.Servers.Any(w => w.Endpoint.Equals(endpoint))))
                {
                    //servergroup doesnt exist and world is not in a group named like the given servergroup and in no other
                    Logger.Log.InfoFormat($"World with address {endpoint} has already been unregistered before.");
                }
                else if (!ServerCommunicationHelper.Instance.WorldserverGroups.Any(g => g.GroupName == groupName)
                    && ServerCommunicationHelper.Instance.WorldserverGroups.Any(sgi => sgi.Servers.Any(w => w.Endpoint.Equals(endpoint))))
                {
                    //servergroup does not exist but world does run in a different servergroup
                    WorldserverGroupDTO worldserverGroupDTO = ServerCommunicationHelper.Instance.WorldserverGroups.SingleOrDefault(sgi => sgi.Servers.Any(w => w.Endpoint.Equals(endpoint)));
                    RemoveWorldFromWorldserver(endpoint, worldserverGroupDTO.GroupName);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error($"Registering world {endpoint} failed.", ex);
            }
        }

        private void RemoveWorldFromWorldserver(ScsTcpEndPoint endpoint, string groupName)
        {
            WorldserverDTO worldserverToRemove = ServerCommunicationHelper.Instance.WorldserverGroups
                .SingleOrDefault(g => g.GroupName == groupName).Servers.SingleOrDefault(w => w.Endpoint.Equals(endpoint));

            ServerCommunicationHelper.Instance.WorldserverGroups.SingleOrDefault(g => g.GroupName == groupName).Servers.Remove(worldserverToRemove);
            ServerCommunicationHelper.Instance.Worldservers.Remove(worldserverToRemove);
            Logger.Log.InfoFormat($"World {worldserverToRemove.Id} with address {endpoint} has been unregistered successfully.");

            if (!ServerCommunicationHelper.Instance.WorldserverGroups.SingleOrDefault(g => g.GroupName == groupName)?.Servers.Any() ?? false)
            {
                WorldserverGroupDTO worldserverGroup = ServerCommunicationHelper.Instance.WorldserverGroups.SingleOrDefault(g => g.GroupName == groupName);

                if (worldserverGroup != null)
                {
                    ServerCommunicationHelper.Instance.WorldserverGroups.Remove(worldserverGroup);
                    Logger.Log.InfoFormat($"World server group {groupName} has been removed as no world was left.");
                }
            }
        }

        #endregion
    }
}