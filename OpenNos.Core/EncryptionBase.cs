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
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OpenNos.Core
{
    public abstract class EncryptionBase
    {
        #region Instantiation

        public EncryptionBase(bool hasCustomParameter)
        {
            HasCustomParameter = hasCustomParameter;
        }

        #endregion

        #region Properties

        public bool HasCustomParameter { get; set; }

        #endregion

        #region Methods

        public static string Sha512(string inputString)
        {
            using (SHA512 hash = SHA512Managed.Create())
            {
                return String.Join(String.Empty, hash.ComputeHash(Encoding.UTF8.GetBytes(inputString)).Select(item => item.ToString("x2")));
            }
        }

        public static string GetPassword(string passcrypt)
        {
            bool equal = passcrypt.Length % 2 == 0 ? true : false;
            string str = equal == true ? passcrypt.Remove(0, 3) : passcrypt.Remove(0, 4);
            string decpass = string.Empty;
            for (int i = 0; i < str.Length; i += 2) decpass += str[i];
            if (decpass.Length % 2 != 0)
            {
                str = decpass = string.Empty;
                str = passcrypt.Remove(0, 2);
                for (int i = 0; i < str.Length; i += 2) decpass += str[i];
            }
            StringBuilder temp = new StringBuilder();
            for (int i = 0; i < decpass.Length; i += 2)
                temp.Append(Convert.ToChar(Convert.ToUInt32(decpass.Substring(i, 2), 16)));
            decpass = temp.ToString();
            return decpass;
        }

        public abstract string Decrypt(byte[] data, int customParameter = 0);

        public abstract string DecryptCustomParameter(byte[] data);

        public abstract byte[] Encrypt(string data);

        #endregion
    }
}