using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using MySql.Data.MySqlClient;
using System;
namespace LoginServer
{
	internal class Mysql
	{
		private MySqlConnection con;
		private string server;
		private string user;
		private string password;
		private string database;

		public Mysql()
		{
			this.con = new MySqlConnection();
		}
		public bool StartMysql()
		{
            this.con.ConnectionString = "server=" +
                this.server +
                "; user id=" +
                this.user +
                "; password=" +
                this.password +
                "; database=" +
                this.database;
			try
			{
				this.con.Open();
				ConsoleTools.WriteConsole("INFO","MySQL started !");
                return true;
			}
			catch (Exception exception)
			{
				ProjectData.SetProjectError(exception);      
                ConsoleTools.WriteConsole("ERROR", exception.Message);
                ProjectData.ClearProjectError();
                return false;
            
			}
		}
		public void StopMysql()
		{
			try
			{
				this.con.Dispose();
				this.con.Close();
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				Interaction.MsgBox(ex.Message, MsgBoxStyle.OkOnly, null);
				ProjectData.ClearProjectError();
			}
		}
		public void SetData(string zserver, string zuser, string zpassword, string zdatabase)
		{
			this.server = zserver;
			this.user = zuser;
			this.password = zpassword;
			this.database = zdatabase;
		}
		public object MysqlQuery(string qString)
		{
			MySqlCommand mySqlCommand = new MySqlCommand();
            mySqlCommand.Connection = this.con;
            mySqlCommand.CommandText = qString;
			string result;
			try
			{
				result = Conversions.ToString(mySqlCommand.ExecuteScalar());
			}
			catch (Exception ex)
			{
				ProjectData.SetProjectError(ex);
				result = "Erreur & " + ex.Message;
				ProjectData.ClearProjectError();
			}
			return result;
		}
	}
}
