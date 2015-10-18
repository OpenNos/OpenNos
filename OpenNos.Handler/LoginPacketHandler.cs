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
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Configuration;

namespace OpenNos.Login
{
    public class LoginPacketHandler : PacketHandlerBase
    {
        private string _loginIp;
        private string _gameIp;
        private string _worldName;
        private int _port;
        private int _channelCount;
        private int _gamePort;

        private readonly IScsServerClient _client;

        public LoginPacketHandler(IScsServerClient client)
        {
            _client = client;
        }

        public void SetData(string loginIp, string gameId, int port, string channelName, int channelCount, int gamePort)
        {
            //TODO initialize values in constructor of LoginPacketHandler
            //-> LoginPacketHandler will be instantiated when client connects
            //each connected clients gots it own LoginPacketHandler
            this._loginIp = loginIp;
            this._gameIp = gameId;
            this._port = port;
            this._worldName = channelName;
            this._channelCount = channelCount;
            this._gamePort = gamePort;
        }
        public int GetPort()
        {
            return this._port;
        }
        public string GetIp()
        {
            return this._loginIp;
        }
        public User GetUser(string str)
        {
            User result = new User();
            string[] array = str.Split(new char[] { ' ' });
            result.Name = array[2];
            result.Password = LoginEncryption.sha256(LoginEncryption.GetPassword(array[3]));
            return result;
        }
        public string MakeChannel(int session)
        {
            //TODO cleanup
            string channelPacket = String.Format("NsTeST {0} ",session);
      
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
                return String.Format("{0}", channelPacket);
            }
        }

        public ScsMessage SendMsg(string packet)
        {
            return new ScsTextMessage(packet);
        }

        [Packet("NoS0575")]
        public ScsMessage CheckUser(string packet, long session)
        {
            User user = GetUser(packet);

            //fermé
            bool flag = true;
            if (flag)
            {
                //TODO: implement check for maintenances
                bool maintenanceCheck = true;
                if (maintenanceCheck)
                {
                    if (DAOFactory.AccountDAO.CheckPasswordValiditiy(user.Name, user.Password))
                    {
                        //0 banned 1 register 2 user 3 GM
                        AuthorityType type = DAOFactory.AccountDAO.LoadAuthorityType(user.Name);

                        switch (type)
                        {
                            case AuthorityType.Banned:
                                {
                                    return SendMsg(String.Format("fail Banned"));
                                }
                            default:
                                {
                                    if (!DAOFactory.AccountDAO.IsLoggedIn(user.Name))
                                    {
                                        DAOFactory.AccountDAO.UpdateLastSessionAndIp(user.Name, (int)session, _client.RemoteEndPoint.ToString());
                                        Logger.Log.DebugFormat("CONNECT {0} Connected -- session:{1}", user.Name, session);
                                        return SendMsg(MakeChannel((int)session));                                    
                                    }
                                    else
                                    {
                                        return SendMsg(String.Format("fail Online"));
                                    }
                                }

                        }
                    }
                    else
                    {
                        return SendMsg(String.Format("fail IDError"));
                    }
                }
                else
                {
                    return SendMsg(String.Format("fail Close"));
                }
            }
            else
            {
                return SendMsg(String.Format("fail Waiting"));
            }
        }
    }
}
