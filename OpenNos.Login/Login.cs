using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Net.Sockets;

namespace OpenNos.Login
{
    public class Login
    {
        private string _loginIp;
        private string _gameIp;
        private string _channelName;
        private int _port;
        private int _channelCount;
        private int _gamePort;

        public void SetData(string loginIp, string gameId, int port, string channelName, int channelCount, int gamePort)
        {
            this._loginIp = loginIp;
            this._gameIp = gameId;
            this._port = port;
            this._channelName = channelName;
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
            result.Password = Encryption.sha256(Encryption.GetPassword(array[3]));
            return result;
        }
        public string MakeChannel(int session)
        {
            string channelPacket = String.Format("NsTeST {0} ",session);
            int num = this._channelCount;
            int num2 = 1;
            int worlds = 1;
            int channels = 1;
            checked
            {
                for (int i = 0; i < worlds; i++)
                {
                    for (int j = 0; j < channels; j++)
                    {
                        int arg = num2;
                        int num3 = num;
                        if (arg > num3)
                        {
                            break;
                        }
                        channelPacket += String.Format("{0}:{1}:0:1.{2}.{3} ",
                            this._gameIp, 
                            (this._gamePort + num2 - 1),
                            num2,
                            this._channelName);
                        num2++;
                    }
                }
                return String.Format("{0}:1.1.1 ", channelPacket);
            }
        }
        public void SendMsg(string str, NetworkStream NetWorkStream)
        {
            NetWorkStream.Write(Encryption.LoginEncrypt(str + " "), 0, checked(Encryption.LoginEncrypt(str).Length + 1));
        }
        public void CheckUser(User user, NetworkStream network, int session)
        {
            Config ConfIni = new Config(Program.AppPath(true) + "config.ini");
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
                                    SendMsg("fail " + ConfIni.GetString("MESSAGE", "Banned", "error"), network);
                                    break;
                                }
                            default:
                                {
                                    if (!DAOFactory.AccountDAO.IsLoggedIn(user.Name))
                                    {
                                        DAOFactory.AccountDAO.UpdateLastSession(user.Name, session);
                                        SendMsg(MakeChannel(session), network);
                                        ConsoleTools.WriteConsole("CONNECT", user.Name + " Connected -- session:" + session);
                                    }
                                    else
                                    {
                                        SendMsg("fail " + ConfIni.GetString("MESSAGE", "Online", "error"), network);
                                    }
                                    break;
                                }

                        }
                    }
                    else
                    {
                        SendMsg("fail " + ConfIni.GetString("MESSAGE", "IDError", "error"), network);
                    }
                }
                else
                {
                    SendMsg("fail " + ConfIni.GetString("MESSAGE", "Close", "error"), network);
                }
            }
            else
            {
                SendMsg("fail " + ConfIni.GetString("MESSAGE", "Waiting", "error"), network);
            }
        }
    }
}
