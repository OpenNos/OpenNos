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

using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using OpenNos.Data;

namespace OpenNos.Master.Server
{
    internal class CommunicationService : ScsService, ICommunicationService
    {
        #region Instantiation
        public CommunicationService()
        {

        }
        #endregion

        #region Methods

        public bool Authenticate(string authKey)
        {
            if (!string.IsNullOrWhiteSpace(authKey) && authKey == ConfigurationManager.AppSettings["MasterAuthKey"])
            {
                MSManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
                return true;
            }
            return false;
        }

        public void Cleanup()
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            MSManager.Instance.ConnectedAccounts.Clear();
            MSManager.Instance.WorldServers.Clear();
        }

        public bool ConnectAccount(Guid worldId, long accountId, long sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));
            if (account != null)
            {
                account.ConnectedWorld = MSManager.Instance.WorldServers.FirstOrDefault(w => w.Id.Equals(worldId));
            }
            return account?.ConnectedWorld != null;
        }

        public bool ConnectCharacter(Guid worldId, long characterId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            //Multiple WorldGroups not yet supported by DAOFactory
            long accountId = DAOFactory.CharacterDAO.FirstOrDefault(s=>s.CharacterId == characterId)?.AccountId ?? 0;

            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.ConnectedWorld?.Id.Equals(worldId) == true);
            if (account == null)
            {
                return false;
            }
            account.CharacterId = characterId;
            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(account.ConnectedWorld.WorldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().CharacterConnected(characterId);
            }
            Console.Title = $"MASTER SERVER - Channels :{MSManager.Instance.WorldServers.Count} - Players : {MSManager.Instance.ConnectedAccounts.Count(s => s.CharacterId != 0)}";
            return true;
        }

        public SerializableWorldServer GetAct4ChannelInfo(string worldGroup)
        {
            WorldServer act4Channel = MSManager.Instance.WorldServers.FirstOrDefault(s => s.IsAct4 && s.WorldGroup == worldGroup);

            if (act4Channel != null)
            {
                return act4Channel.Serializable;
            }
            act4Channel = MSManager.Instance.WorldServers.FirstOrDefault(s => s.WorldGroup == worldGroup);
            if (act4Channel == null)
            {
                return null;
            }
            Logger.Log.Info($"[{act4Channel.WorldGroup}] ACT4 Channel elected on ChannelId : {act4Channel.ChannelId} ");
            act4Channel.IsAct4 = true;
            return act4Channel.Serializable;
        }

        public bool IsCrossServerLoginPermitted(long accountId, int sessionId)
        {
            return MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                   MSManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.CanSwitchChannel);
        }

        public void DisconnectAccount(long accountId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            if (MSManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.CanSwitchChannel))
            {
                return;
            }
            MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(s => s.AccountId != accountId);
        }

        public void DisconnectCharacter(Guid worldId, long characterId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (AccountConnection account in MSManager.Instance.ConnectedAccounts.Where(c => c.CharacterId.Equals(characterId) && c.ConnectedWorld.Id.Equals(worldId)))
            {
                foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(account.ConnectedWorld.WorldGroup)))
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().CharacterDisconnected(characterId);
                }
                if (account.CanSwitchChannel)
                {
                    continue;
                }
                account.CharacterId = 0;
                account.ConnectedWorld = null;
                Console.Title = $"MASTER SERVER - Channels :{MSManager.Instance.WorldServers.Count} - Players : {MSManager.Instance.ConnectedAccounts.Count(s => s.CharacterId != 0)}";
            }
        }

        public int? GetChannelIdByWorldId(Guid worldId)
        {
            return MSManager.Instance.WorldServers.FirstOrDefault(w => w.Id == worldId)?.ChannelId;
        }

        public bool IsAccountConnected(long accountId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            return MSManager.Instance.ConnectedAccounts.Any(c => c.AccountId == accountId && c.ConnectedWorld != null);
        }

        public bool IsCharacterConnected(string worldGroup, long characterId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            return MSManager.Instance.ConnectedAccounts.Any(c => c.ConnectedWorld != null && c.ConnectedWorld.WorldGroup == worldGroup && c.CharacterId == characterId);
        }

        public bool IsLoginPermitted(long accountId, long sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            return MSManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.ConnectedWorld == null);
        }

        public void KickSession(long? accountId, long? sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers)
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().KickSession(accountId, sessionId);
            }
            if (accountId.HasValue)
            {
                MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(s => !s.AccountId.Equals(accountId.Value));
            }
            else if (sessionId.HasValue)
            {
                MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(s => !s.SessionId.Equals(sessionId.Value));
            }
        }

        public void RefreshPenalty(int penaltyId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers)
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdatePenaltyLog(penaltyId);
            }
            foreach (IScsServiceClient login in MSManager.Instance.LoginServers)
            {
                login.GetClientProxy<ICommunicationClient>().UpdatePenaltyLog(penaltyId);
            }
        }

        public void RegisterAccountLogin(long accountId, long sessionId, string accountName)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(a => !a.AccountId.Equals(accountId));
            MSManager.Instance.ConnectedAccounts.Add(new AccountConnection(accountId, sessionId, accountName));
        }

        public void RegisterInternalAccountLogin(long accountId, int sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));

            if (account != null)
            {
                account.CanSwitchChannel = true;
            }
        }

        public bool ConnectAccountInternal(Guid worldId, long accountId, int sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }
            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));
            if (account == null)
            {
                return false;
            }
            {
                account.CanSwitchChannel = false;
                account.PreviousChannel = account.ConnectedWorld;
                account.ConnectedWorld = MSManager.Instance.WorldServers.FirstOrDefault(s => s.Id.Equals(worldId));
                if (account.ConnectedWorld != null)
                {
                    return true;
                }
            }
            return false;
        }

        public SerializableWorldServer GetPreviousChannelByAccountId(long accountId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(s => s.AccountId.Equals(accountId));
            return account?.PreviousChannel?.Serializable;
        }

        public int? RegisterWorldServer(SerializableWorldServer worldServer)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }
            WorldServer ws = new WorldServer(worldServer.Id, new ScsTcpEndPoint(worldServer.EndPointIp, worldServer.EndPointPort), worldServer.AccountLimit, worldServer.WorldGroup)
            {
                ServiceClient = CurrentClient,
                ChannelId = Enumerable.Range(1, 30).Except(MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldServer.WorldGroup)).OrderBy(w => w.ChannelId).Select(w => w.ChannelId))
                    .First(),
                Serializable = new SerializableWorldServer(worldServer.Id, worldServer.EndPointIp, worldServer.EndPointPort, worldServer.AccountLimit, worldServer.WorldGroup),
                IsAct4 = false
            };
            MSManager.Instance.WorldServers.Add(ws);
            return ws.ChannelId;
        }

        public string RetrieveRegisteredWorldServers(long sessionId)
        {
            string lastGroup = string.Empty;
            byte worldCount = 0;
            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(s => s.SessionId == sessionId);
            if (account == null)
            {
                return null;
            }
            string channelPacket = $"NsTeST {account.AccountName} {sessionId} ";
            foreach (WorldServer world in MSManager.Instance.WorldServers.OrderBy(w => w.WorldGroup))
            {
                if (lastGroup != world.WorldGroup)
                {
                    worldCount++;
                }
                lastGroup = world.WorldGroup;

                int currentlyConnectedAccounts = MSManager.Instance.ConnectedAccounts.Count(a => a.ConnectedWorld?.ChannelId == world.ChannelId);
                int channelcolor = (int)Math.Round((double)currentlyConnectedAccounts / world.AccountLimit * 20) + 1;

                channelPacket += $"{world.Endpoint.IpAddress}:{world.Endpoint.TcpPort}:{channelcolor}:{worldCount}.{world.ChannelId}.{world.WorldGroup} ";
            }
            channelPacket += "-1:-1:-1:10000.10000.1";
            return MSManager.Instance.WorldServers.Any() ? channelPacket : null;
        }

        public IEnumerable<string> RetrieveServerStatistics()
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            List<string> result = new List<string>();

            try
            {
                List<string> groups = new List<string>();
                foreach (string s in MSManager.Instance.WorldServers.Select(s => s.WorldGroup))
                {
                    if (!groups.Contains(s))
                    {
                        groups.Add(s);
                    }
                }
                int totalsessions = 0;
                foreach (string s in groups)
                {
                    result.Add($"==={s}===");
                    int groupsessions = 0;
                    foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(s)))
                    {
                        int sessions = MSManager.Instance.ConnectedAccounts.Count(a => a.ConnectedWorld?.Id.Equals(world.Id) == true);
                        result.Add($"Channel {world.ChannelId}: {sessions} Sessions");
                        groupsessions += sessions;
                    }
                    result.Add($"Group Total: {groupsessions} Sessions");
                    totalsessions += groupsessions;
                }
                result.Add($"Environment Total: {totalsessions} Sessions");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while retreiving server Statistics:", ex);
            }

            return result;
        }

        public int? SendMessageToCharacter(SCSCharacterMessage message)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            WorldServer sourceWorld = MSManager.Instance.WorldServers.FirstOrDefault(s => s.Id.Equals(message.SourceWorldId));
            if (message?.Message == null || sourceWorld == null)
            {
                return null;
            }
            switch (message.Type)
            {
                case MessageType.Family:
                case MessageType.FamilyChat:
                    foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(sourceWorld.WorldGroup)))
                    {
                        world.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                    }
                    return -1;

                case MessageType.PrivateChat:
                case MessageType.Whisper:
                case MessageType.WhisperGM:
                    if (message.DestinationCharacterId.HasValue)
                    {
                        AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.CharacterId.Equals(message.DestinationCharacterId.Value));
                        if (account != null && account.ConnectedWorld != null)
                        {
                            account.ConnectedWorld.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                            return account.ConnectedWorld.ChannelId;
                        }
                    }
                    break;

                case MessageType.Shout:
                    foreach (WorldServer world in MSManager.Instance.WorldServers)
                    {
                        world.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                    }
                    return -1;
            }
            return null;
        }

        public void UnregisterWorldServer(Guid worldId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(a => a == null || a.ConnectedWorld?.Id.Equals(worldId) != true);
            MSManager.Instance.WorldServers.RemoveAll(w => w.Id.Equals(worldId));
        }

        public void UpdateBazaar(string worldGroup, long bazaarItemId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateBazaar(bazaarItemId);
            }
        }

        public void UpdateFamily(string worldGroup, long familyId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateFamily(familyId);
            }
        }

        public void Shutdown(string worldGroup)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            if (worldGroup == "*")
            {
                foreach (WorldServer world in MSManager.Instance.WorldServers)
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().Shutdown();
                }
            }
            else
            {
                foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().Shutdown();
                }
            }
        }

        public void UpdateRelation(string worldGroup, long relationId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateRelation(relationId);
            }
        }

        public void PulseAccount(long accountId)
        {
            Logger.Log.Debug("PulseAccount");
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId));
            if (account != null)
            {
                account.LastPulse = DateTime.Now;
            }
        }

        public void CleanupOutdatedSession()
        {
            AccountConnection[] tmp = new AccountConnection[MSManager.Instance.ConnectedAccounts.Count + 20];
            lock (MSManager.Instance.ConnectedAccounts)
            {
                MSManager.Instance.ConnectedAccounts.ToList().CopyTo(tmp);
            }
            foreach (AccountConnection account in tmp.Where(a => a != null && a.LastPulse.AddMinutes(5) <= DateTime.Now))
            {
                KickSession(account.AccountId, null);
            }
        }

        public void SendMail(string worldGroup, MailDTO mail)
        {
            if (!IsCharacterConnected(worldGroup, mail.ReceiverId))
            {
                CharacterDTO chara = DAOFactory.CharacterDAO.FirstOrDefault(s=>s.CharacterId ==mail.ReceiverId);
                DAOFactory.MailDAO.InsertOrUpdate(ref mail);
            }
            else
            {
                AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.CharacterId.Equals(mail.ReceiverId));
                if (account != null && account.ConnectedWorld != null)
                {
                    account.ConnectedWorld.ServiceClient.GetClientProxy<ICommunicationClient>().SendMail(mail);
                }
            }
        }


        #endregion
    }
}