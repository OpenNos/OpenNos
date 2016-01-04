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
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace OpenNos.Core
{
    public class LoginEncryption : EncryptionBase
    { 
        public LoginEncryption() : base(false) { }
        public override string Decrypt(byte[] data, int customParameter = 0)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(data[i] - 0xF ^ 0xC3);
            return Encoding.UTF8.GetString(data).Substring(0, data.Length);
        }

        public override byte[] Encrypt(string data)
        {
            data += " ";
            char[] tmp = new char[(data.Length + 1)];
            tmp = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(data));
            for (int i = 0; i < data.Length; i++)
                tmp[i] = Convert.ToChar(data[i] + 15);
            tmp[tmp.Length - 1] = (char)25;
            return Encoding.UTF8.GetBytes(new string(tmp));
        }

        public static string GetPassword(string passcrypt)
        {
            bool equal = passcrypt.Length % 2 == 0 ? true : false;
            string str = equal == true ? passcrypt.Remove(0, 3) : passcrypt.Remove(0, 4);
           string decpass = string.Empty;
            for (int i = 0; i<str.Length; i += 2) decpass += str[i];

            if (decpass.Length % 2 != 0)
            {
                str = decpass = string.Empty;
                str = passcrypt.Remove(0, 2);
                for (int i = 0; i<str.Length; i += 2) decpass += str[i];
            }

            StringBuilder temp = new StringBuilder();
            for (int i = 0; i<decpass.Length; i += 2) temp.Append(Convert.ToChar(Convert.ToUInt32(decpass.Substring(i, 2), 16)));
            decpass = temp.ToString();

            return decpass;
        }

        public override string DecryptCustomParameter(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
