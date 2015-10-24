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

namespace OpenNos.Login
{
    public class LoginPacketHandler : PacketHandlerBase
    {
        private readonly NetworkClient _client;

        public LoginPacketHandler(NetworkClient client)
        {
            _client = client;
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
        public string VerifyLogin(string packet, long clientId)
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
                        DAOFactory.AccountDAO.WriteConnectionLog(loadedAccount.AccountId, _client.RemoteEndPoint.ToString());

                        //0 banned 1 register 2 user 3 GM
                        AuthorityType type = loadedAccount.AuthorityEnum;

                        switch (type)
                        {
                            case AuthorityType.Banned:
                                {
                                    return String.Format("fail {O}", Language.Instance.GetMessageFromKey("BANNED").ToString());
                                }
                            default:
                                {
                                    if (!DAOFactory.AccountDAO.IsLoggedIn(user.Name))
                                    {
                                        int newSessionId = SessionFactory.Instance.GenerateSessionId();

                                        DAOFactory.AccountDAO.UpdateLastSessionAndIp(user.Name, (int)newSessionId, _client.RemoteEndPoint.ToString());
                                        Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CONNECTION"), user.Name, newSessionId);

                                        return BuildServersPacket((int)newSessionId);
                                    }
                                    else
                                    {
                                        return String.Format("fail {O}", Language.Instance.GetMessageFromKey("ONLINE").ToString());
                                    }
                                }

                        }
                    }
                    else
                    {
                        return String.Format("fail {0}", Language.Instance.GetMessageFromKey("IDERROR").ToString());
                    }
                }
                else
                {
                    return String.Format("fail {O}", Language.Instance.GetMessageFromKey("CLOSE").ToString());
                }
            }
            else
            {
                return String.Format("fail {O}", Language.Instance.GetMessageFromKey("WAITING").ToString());
            }
        }
    }
}
