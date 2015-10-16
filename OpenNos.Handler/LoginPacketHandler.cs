using OpenNos.Core;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Net.Sockets;

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

        public void SetData(string loginIp, string gameId, int port, string channelName, int channelCount, int gamePort)
        {
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
            int num = this._channelCount;
            int channels = 5;
            int worlds = 3;
            checked
            {
                for (int w = 1; w <= worlds; w++)
                {
                    for (int j = 1; j <= channels; j++)
                    {
                        channelPacket += String.Format("{0}:{1}:1:{2}.{3}.{4} ",
                            this._gameIp,
                            (this._gamePort + j - 1),
                            w,
                            j,
                            this._worldName);
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

            Config ConfIni = new Config(String.Format("{0}config.ini", Application.AppPath(true)));
            //fermé
            bool flag = true;
            if (flag)
            {
                //TODO: implement check for maintenances
                bool maintenanceCheck = true;
                if (maintenanceCheck)
                {
                    Console.WriteLine(user.Password);
                    if (DAOFactory.AccountDAO.CheckPasswordValiditiy(user.Name, user.Password))
                    {
                        //0 banned 1 register 2 user 3 GM
                        AuthorityType type = DAOFactory.AccountDAO.LoadAuthorityType(user.Name);

                        switch (type)
                        {
                            case AuthorityType.Banned:
                                {
                                    return SendMsg(String.Format("fail {0}", ConfIni.GetString("MESSAGE", "Banned", "error")));
                                }
                            default:
                                {
                                    if (!DAOFactory.AccountDAO.IsLoggedIn(user.Name))
                                    {
                                        DAOFactory.AccountDAO.UpdateLastSession(user.Name, (int)session);
                                        Logger.Log.Debug(String.Format("CONNECT {0} Connected -- session:{1}", user.Name, session));
                                        return SendMsg(MakeChannel((int)session));                                    
                                    }
                                    else
                                    {
                                        return SendMsg(String.Format("fail {0}", ConfIni.GetString("MESSAGE", "Online", "error")));
                                    }
                                }

                        }
                    }
                    else
                    {
                        return SendMsg(String.Format("fail {0}", ConfIni.GetString("MESSAGE", "IDError", "error")));
                    }
                }
                else
                {
                    return SendMsg(String.Format("fail {0}",  ConfIni.GetString("MESSAGE", "Close", "error")));
                }
            }
            else
            {
                return SendMsg(String.Format("fail {0}", ConfIni.GetString("MESSAGE", "Waiting", "error")));
            }
        }
    }
}
