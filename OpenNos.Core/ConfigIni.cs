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
using System;
using System.Runtime.InteropServices;
using System.Text;
namespace OpenNos.Core
{
    public class Config
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

        public Config(string Filename)
        {
            this.strFilename = Filename;
        }
        public string GetString(string Section, string Key, string Default = "error")
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            int privateProfileString = Config.GetPrivateProfileString(ref Section, ref Key, ref Default, stringBuilder, stringBuilder.Capacity, ref this.strFilename);
            return privateProfileString > 0 ? stringBuilder.ToString().Substring(0, privateProfileString) : String.Empty;
        }
        public int GetInteger(string Section, string Key, int Default = 5)
        {
            return Config.GetPrivateProfileInt(ref Section, ref Key, Default, ref this.strFilename);
        }
        public bool GetBoolean(string Section, string Key, bool Default)
        {
            return Config.GetPrivateProfileInt(ref Section, ref Key, (((Default) ? true : false)) ? 1 : 0, ref this.strFilename) == 1;
        }
    }
}
