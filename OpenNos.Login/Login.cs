
using OpenNos.DAL;
using OpenNos.Domain;
using System;
using System.Net.Sockets;

namespace OpenNos.Login
{
	internal class Login
	{
		private string Login_ip;
		private string game_ip;
		private string channel_name;
		private int port;
        private int countch;
		private int gameport;

		public void SetData(string data_login_ip, string data_game_ip, int data_port, string data_channel_name, int data_countch, int data_gameport)
		{
			this.Login_ip = data_login_ip;
			this.game_ip = data_game_ip;
			this.port = data_port;
			this.channel_name = data_channel_name;
			this.countch = data_countch;
			this.gameport = data_gameport;
		}
		public int GetPort()
		{
			return this.port;
		}
		public string GetIp()
		{
			return this.Login_ip;
		}
		public User GetUser(string str)
		{
			User result = new User();
			string[] array = str.Split(new char[]{' '});
			result.Name = array[2];
			result.Password =  Encryption.sha256(Encryption.GetPassword(array[3]));
			return result;
		}
		public string MakeChannel(int Session)
		{
			string str = "NsTeST ";
			str = str + Session.ToString() + " ";
			int num = this.countch;
			int num2 = 1;
            int world = 1;
            int channel = 1;
			checked
			{
                for (int w = 0; w < world; w++)
                {
                    for (int i = 0; i < channel; i++)
                    {
                        int arg = num2;
                        int num3 = num;
                        if (arg > num3)
                        {
                            break;
                        }
                        str += this.game_ip + ":" + (this.gameport + num2 - 1).ToString() + ":0:1." + (num2).ToString() + "." + this.channel_name + " ";
                        num2++;
                    }
                }
				return str + ":1.1.1 ";
			}
		}
		public void SendMsg(string str, NetworkStream NetWorkStream)
		{
            NetWorkStream.Write(Encryption.LoginEncrypt(str + " "), 0, checked(Encryption.LoginEncrypt(str).Length + 1));
		}
		public void CheckUser(User user, NetworkStream network, int session)
		{
			ConfigIni ConfIni = new ConfigIni(MainFile.AppPath(true) + "config.ini");
            //fermé
            bool flag = true;
			if (flag)
			{
                //Maintenance?
                bool flag2 = true;
                if (flag2)
				{
					if (DAOFactory.AccountDAO.CheckPasswordValiditiy(user.Name, user.Password))
					{
                        //0 banned 1 register 2 user 3 GM
                        AuthorityType type = DAOFactory.AccountDAO.LoadAuthorityType(user.Name);
						if (flag)
						{
                            bool flag3 = true;
                            //Is logged?
                            if (flag3)
							{
                                DAOFactory.AccountDAO.UpdateLastSession(user.Name, session);
                                SendMsg(MakeChannel(session), network);
                                ConsoleTools.WriteConsole("CONNECT", user.Name + " Connected -- session:" + session);
							}
							else
							{
                                SendMsg("fail " + ConfIni.GetString("MESSAGE", "Online", "error"), network);
							}
						}
						else
						{
                            SendMsg("fail " + ConfIni.GetString("MESSAGE", "Banned", "error"), network);
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
