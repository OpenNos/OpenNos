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

namespace OpenNos.Handler
{
    public class LoginPacketHandler
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
                    for (int j = 1; j <= serv.channelAmount; j++)
                    {
                        channelPacket += $"{serv.WorldIp}:{(serv.WorldPort + j - 1)}:1:{w}.{j}.{serv.name} ";
                    }
                }
                return channelPacket;
            }
        }

        [Packet("NoS0575")]
        public void VerifyLogin(string packet)
        {
            UserDTO user = PacketFactory.Deserialize<UserDTO>(packet);
            //closed
            bool flag = true;
            if (flag)
            {
                //TODO: implement check for maintenances
                bool maintenanceCheck = true;
                if (maintenanceCheck)
                {
                    AccountDTO loadedAccount = DAOFactory.AccountDAO.LoadByName(user.Name);

                    if (loadedAccount != null && loadedAccount.Password.Equals(user.PasswordDecrypted))
                    {
                        DAOFactory.AccountDAO.WriteGeneralLog(loadedAccount.AccountId, _session.Client.RemoteEndPoint.ToString(), null, "Connection", "LoginServer");

                        if (!ServiceFactory.Instance.CommunicationService.AccountIsConnected(loadedAccount.Name))
                        {
                            AuthorityType type = loadedAccount.AuthorityEnum; //0 banned 1 registered 2 user 3 GM

                            switch (type)
                            {
                                case AuthorityType.Banned:
                                    {
                                        _session.Client.SendPacket($"fail {Language.Instance.GetMessageFromKey("BANNED")}");
                                    }
                                    break;

                                case AuthorityType.Unknown:
                                    {
                                        _session.Client.SendPacket($"fail {Language.Instance.GetMessageFromKey("NOTVALIDATE")}");
                                    }
                                    break;

                                default:
                                    {
                                        int newSessionId = SessionFactory.Instance.GenerateSessionId();

                                        DAOFactory.AccountDAO.UpdateLastSessionAndIp(user.Name, (int)newSessionId, _session.Client.RemoteEndPoint.ToString());
                                        Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CONNECTION"), user.Name, newSessionId);

                                        //inform communication service about new player from login server
                                        try
                                        {
                                            ServiceFactory.Instance.CommunicationService.RegisterAccountLogin(user.Name, newSessionId);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Log.Error(ex.Message);
                                        }
                                        _session.Client.SendPacket(BuildServersPacket((int)newSessionId));
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            _session.Client.SendPacket($"fail {Language.Instance.GetMessageFromKey("ALREADY_CONNECTED").ToString()}");
                        }
                    }
                    else
                    {
                        _session.Client.SendPacket($"fail {Language.Instance.GetMessageFromKey("IDERROR").ToString()}");
                    }
                }
                else
                {
                    _session.Client.SendPacket($"fail {Language.Instance.GetMessageFromKey("CLOSE").ToString()}");
                }
            }
            else
            {
                _session.Client.SendPacket($"fail {Language.Instance.GetMessageFromKey("WAITING").ToString()}");
            }
        }

        #endregion
    }
}