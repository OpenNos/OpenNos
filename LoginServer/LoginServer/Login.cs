using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Net.Sockets;
namespace LoginServer
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
			User result = default(User);
			string[] array = str.Split(new char[]{' '});
			result.Name = array[2];
			result.Psw =  Encryption.sha256(Encryption.GetPassword(array[3]));
			return result;
		}
		public string MakeChannel(int Session)
		{
			string str = "NsTeST ";
			str = str + Conversions.ToString(Session) + " ";
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
                        str += this.game_ip + ":" + Conversions.ToString(this.gameport + num2 - 1) + ":0:1." + Conversions.ToString(num2) + "." + this.channel_name + " ";
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
		public void CheckUser(User User, Mysql SqlCore, NetworkStream network, int session)
		{
			ConfigIni ConfIni = new ConfigIni(MainFile.AppPath(true) + "config/LoginServer.ini");
            //fermé
            bool flag = true;
			if (flag)
			{
                //Maintenance?
                bool flag2 = true;
                if (flag2)
				{
                    User.Name = AntiSqlInjection.ValidateSqlValue(User.Name);
                    Console.WriteLine(User.Psw);
                    flag2 = Operators.ConditionalCompareObjectEqual(SqlCore.MysqlQuery("SELECT Pass FROM Accounts WHERE AccountName='" + User.Name + "';"),User.Psw, false);
					if (flag2)
					{
                        //0 banned 1 register 2 user 3 GM
                        flag = Operators.ConditionalCompareObjectGreater(SqlCore.MysqlQuery("SELECT Authority FROM Accounts WHERE AccountName='" + User.Name + "';"), 0, false);
						if (flag)
						{
                            bool flag3 = true;
                            //Is logged?
                            if (flag3)
							{
                                SqlCore.MysqlQuery("UPDATE Accounts set  LastSession = " + session + "  WHERE AccountName='" + User.Name + "';");
                                SendMsg(MakeChannel(session), network);
                                ConsoleTools.WriteConsole("CONNECT", User.Name + " Connected -- session:" + session);
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
