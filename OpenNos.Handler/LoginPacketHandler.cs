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
using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Configuration;
using OpenNos.Core.Communication.Scs.Communication.Messages;
using OpenNos.Core.Communication.Scs.Server;
using OpenNos.GameObject;
using OpenNos.ServiceRef.Internal;

namespace OpenNos.Handler
{
    public class LoginPacketHandler
    {
        private readonly ClientSession _session;

        public LoginPacketHandler(ClientSession session)
        {
            _session = session;
        }

        public string BuildServersPacket(int session)
        {
            string channelPacket = String.Format("NsTeST {0} ", session);
            List<ServerConfig.Server> myServs = (List<ServerConfig.Server>)ConfigurationManager.GetSection("Servers");

            checked
            {
                int w = 0;
                foreach (ServerConfig.Server serv in myServs)
                {
                    w++;
                    for (int j = 1; j <= serv.channelAmount; j++)
                    {
                        channelPacket += String.Format("{0}:{1}:1:{2}.{3}.{4} ",
                            serv.WorldIp,
                            (serv.WorldPort + j - 1),
                            w,
                            j,
                            serv.name);
                    }
                }
                return channelPacket;
            }
        }

        [Packet("NoS0575")]
        public void VerifyLogin(string packet)
        {
            User user = PacketFactory.Deserialize<User>(packet);
            //fermé
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
                        DAOFactory.AccountDAO.WriteConnectionLog(loadedAccount.AccountId, _session.Client.RemoteEndPoint.ToString(), null, "Connection", "LoginServer");

                        if (!ServiceFactory.Instance.CommunicationService.AccountIsConnected(loadedAccount.Name))
                        {
                            //0 banned 1 register 2 user 3 GM
                            AuthorityType type = loadedAccount.AuthorityEnum;

                            switch (type)
                            {
                                case AuthorityType.Banned:
                                    {
                                        _session.Client.SendPacket(String.Format("fail {0}", Language.Instance.GetMessageFromKey("BANNED")));
                                    }
                                    break;
                                case AuthorityType.Unknown:
                                    {
                                        _session.Client.SendPacket(String.Format("fail {0}", Language.Instance.GetMessageFromKey("NOTVALIDATE")));
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
                            _session.Client.SendPacket(String.Format("fail {0}", Language.Instance.GetMessageFromKey("ALREADY_CONNECTED").ToString()));
                        }
                    }
                    else
                    {
                        _session.Client.SendPacket(String.Format("fail {0}", Language.Instance.GetMessageFromKey("IDERROR").ToString()));
                    }
                }
                else
                {
                    _session.Client.SendPacket(String.Format("fail {O}", Language.Instance.GetMessageFromKey("CLOSE").ToString()));
                }
            }
            else
            {
                _session.Client.SendPacket(String.Format("fail {O}", Language.Instance.GetMessageFromKey("WAITING").ToString()));
            }
        }
    }
}
