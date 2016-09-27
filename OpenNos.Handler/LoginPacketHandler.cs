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
using OpenNos.ServiceRef.Internal;
using System;
using System.Collections.Generic;
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

        public string BuildServersPacket(int session)
        {
            string channelPacket = $"NsTeST {session} ";
            List<ServerConfig.Server> myServs = (List<ServerConfig.Server>)ConfigurationManager.GetSection("Servers");

            checked
            {
                int w = 0;
                foreach (ServerConfig.Server serv in myServs)
                {
                    w++;
                    for (int j = 1; j <= serv.ChannelAmount; j++)
                    {
                        channelPacket += $"{serv.WorldIp}:{(serv.WorldPort + j - 1)}:1:{w}.{j}.{serv.Name} ";
                    }
                }
                return channelPacket;
            }
        }

        [Packet("NoS0575")]
        public void VerifyLogin(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            UserDTO user = new UserDTO() { Name = packetsplit[2], Password = packetsplit[3] };

            // closed
            bool flag = true;
            if (flag)
            {
                // TODO: implement check for maintenances
                bool maintenanceCheck = true;
                if (maintenanceCheck)
                {
                    AccountDTO loadedAccount = DAOFactory.AccountDAO.LoadByName(user.Name);

                    if (loadedAccount != null && loadedAccount.Password.ToUpper().Equals(user.Password))
                    {
                        DAOFactory.AccountDAO.WriteGeneralLog(loadedAccount.AccountId, _session.IpAddress, null, "Connection", "LoginServer");

                        if (!ServiceFactory.Instance.CommunicationService.AccountIsConnected(loadedAccount.Name))
                        {
                            AuthorityType type = loadedAccount.Authority;
                            PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(loadedAccount.AccountId).FirstOrDefault(s => s.DateEnd > DateTime.Now && s.Penalty == PenaltyType.Banned);
                            if (penalty != null)
                            {
                                _session.SendPacket($"fail {String.Format(Language.Instance.GetMessageFromKey("BANNED"), penalty.Reason, (penalty.DateEnd).ToString("yyyy-MM-dd-HH:mm"))}");
                            }
                            else
                            {
                                switch (type)
                                {
                                    case AuthorityType.Unknown:
                                        {
                                            _session.SendPacket($"fail {Language.Instance.GetMessageFromKey("NOTVALIDATE")}");
                                        }
                                        break;

                                    default:
                                        {
                                            int newSessionId = SessionFactory.Instance.GenerateSessionId();

                                            DAOFactory.AccountDAO.UpdateLastSessionAndIp(user.Name, newSessionId, _session.IpAddress);
                                            Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CONNECTION"), user.Name, newSessionId);

                                            // inform communication service about new player from login server
                                            try
                                            {
                                                ServiceFactory.Instance.CommunicationService.RegisterAccountLogin(user.Name, newSessionId);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Log.Error("General Error SessionId: " + newSessionId, ex);
                                            }
                                            _session.SendPacket(BuildServersPacket(newSessionId));
                                        }
                                        break;
                                }
                            }
                        }
                        else
                        {
                            _session.SendPacket($"fail {String.Format(Language.Instance.GetMessageFromKey("ALREADY_CONNECTED"))}");
                        }
                    }
                    else
                    {
                        _session.SendPacket($"fail {String.Format(Language.Instance.GetMessageFromKey("IDERROR"))}");
                    }
                }
                else
                {
                    _session.SendPacket($"fail {String.Format(Language.Instance.GetMessageFromKey("MAINTENANCE"))}"); // add estimated time of maintenance/end of maintenance
                }
            }
            else
            {
                _session.SendPacket($"fail {String.Format(Language.Instance.GetMessageFromKey("CLIENT_DISCONNECTED"))}");
            }
        }

        #endregion
    }
}