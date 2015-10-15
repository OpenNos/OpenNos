using System.Runtime.InteropServices;
using System.Text;
namespace OpenNos.Login
{
	public class ConfigIni
	{
		private string strFilename;
		public string FileName
		{
			get
			{
				return this.strFilename;
			}
		}
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "GetPrivateProfileStringA", ExactSpelling = true, SetLastError = true)]
		private static extern int GetPrivateProfileString([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpApplicationName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpKeyName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpDefault, StringBuilder lpReturnedString, int nSize, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpFileName);
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "GetPrivateProfileIntA", ExactSpelling = true, SetLastError = true)]
		private static extern int GetPrivateProfileInt([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpApplicationName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpKeyName, int nDefault, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpFileName);

        public ConfigIni(string Filename)
		{
			this.strFilename = Filename;
		}
		public string GetString(string Section, string Key, string Default = "error")
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			int privateProfileString = ConfigIni.GetPrivateProfileString(ref Section, ref Key, ref Default, stringBuilder, stringBuilder.Capacity, ref this.strFilename);
			bool flag = privateProfileString > 0;
			string result;
            result = (flag)? stringBuilder.ToString().Substring(0, privateProfileString) :result = null;
			return result;
		}
		public int GetInteger(string Section, string Key, int Default = 5)
		{
			return ConfigIni.GetPrivateProfileInt(ref Section, ref Key, Default, ref this.strFilename);
		}
		public bool GetBoolean(string Section, string Key, bool Default)
		{
			return ConfigIni.GetPrivateProfileInt(ref Section, ref Key, (((Default) ? true : false)) ? 1 : 0, ref this.strFilename) == 1;
		}
	}
}
