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

using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.WebApi.Reference;
using System;
using System.Configuration;
using System.Linq;

namespace OpenNos.Handler
{
    public class LoginPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public LoginPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Methods

        public string BuildServersPacket(string accountName, int sessionId)
        {
            string channelpacket = ServerCommunicationClient.Instance.HubProxy.Invoke<string>("RetrieveRegisteredWorldservers", sessionId).Result;

            if (channelpacket == null)
            {
                Logger.Log.Error("Could not retrieve Worldserver groups. Please make sure they've already been registered.");
                _session.SendPacket($"fail {string.Format(Language.Instance.GetMessageFromKey("MAINTENANCE"), DateTime.Now)}");

                // release account's login permission
                bool hasRegisteredAccountLogin = ServerCommunicationClient.Instance.HubProxy.Invoke<bool>("HasRegisteredAccountLogin", accountName, sessionId).Result;
            }

            return channelpacket;
        }

        [Packet("NoS0575")]
        public void VerifyLogin(string packet)
        {
            // TODO: implement check for maintenances
            string[] packetsplit = packet.Split(' ');
            UserDTO user = new UserDTO { Name = packetsplit[2], Password = ConfigurationManager.AppSettings["UseOldCrypto"] == "true" ? EncryptionBase.Sha512(LoginEncryption.GetPassword(packetsplit[3])).ToUpper() : packetsplit[3] };
            AccountDTO loadedAccount = DAOFactory.AccountDAO.LoadByName(user.Name);
            if (loadedAccount != null && loadedAccount.Password.ToUpper().Equals(user.Password))
            {
                DAOFactory.AccountDAO.WriteGeneralLog(loadedAccount.AccountId, _session.IpAddress, null, "Connection", "LoginServer");

                //check if the account is connected
                if (!ServerCommunicationClient.Instance.HubProxy.Invoke<bool>("AccountIsConnected", loadedAccount.Name).Result)
                {
                    AuthorityType type = loadedAccount.Authority;
                    PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(loadedAccount.AccountId).FirstOrDefault(s => s.DateEnd > DateTime.Now && s.Penalty == PenaltyType.Banned);
                    if (penalty != null)
                    {
                        _session.SendPacket($"fail {string.Format(Language.Instance.GetMessageFromKey("BANNED"), penalty.Reason, penalty.DateEnd.ToString("yyyy-MM-dd-HH:mm"))}");
                    }
                    else
                    {
                        switch (type)
                        {
                            case AuthorityType.Unconfirmed:
                                {
                                    _session.SendPacket($"fail {Language.Instance.GetMessageFromKey("NOTVALIDATE")}");
                                }
                                break;
                            case AuthorityType.Banned:
                                {
                                    _session.SendPacket($"fail {Language.Instance.GetMessageFromKey("IDERROR")}");
                                }
                                break;
                            case AuthorityType.Closed:
                                {
                                    _session.SendPacket($"fail {Language.Instance.GetMessageFromKey("IDERROR")}");
                                }
                                break;
                            default:
                                {
                                    int newSessionId = SessionFactory.Instance.GenerateSessionId();
                                    Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CONNECTION"), user.Name, newSessionId);

                                    // inform communication service about new player from login server
                                    try
                                    {
                                        ServerCommunicationClient.Instance.HubProxy.Invoke("RegisterAccountLogin", user.Name, newSessionId);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Log.Error("General Error SessionId: " + newSessionId, ex);
                                    }
                                    _session.SendPacket(BuildServersPacket(user.Name, newSessionId));
                                }
                                break;
                        }
                    }
                }
                else
                {
                    _session.SendPacket($"fail {string.Format(Language.Instance.GetMessageFromKey("ALREADY_CONNECTED"))}");
                }
            }
            else
            {
                _session.SendPacket($"fail {string.Format(Language.Instance.GetMessageFromKey("IDERROR"))}");
            }
        }

        #endregion
    }
}