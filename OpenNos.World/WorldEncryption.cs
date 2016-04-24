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
using System;

namespace OpenNos.World
{
    public class WorldEncryption : EncryptionBase
    {
        #region Instantiation

        public WorldEncryption() : base(true)
        {
        }

        #endregion

        #region Methods

        public static string Decrypt2(char[] str)
        {
            string decrypted_string = "";
            char[] table = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };
            int count = 0;

            for (count = 0; count < str.Length - 1;)
            {
                if (str[count] <= 0x7A)
                {
                    int len = str[count];

                    for (int i = 0; i < (int)len; i++)
                    {
                        count++;

                        decrypted_string += Convert.ToChar((count < str.Length ? str[count] : 0) ^ 0xFF);
                    }
                    count++;
                }
                else
                {
                    int len = str[count];
                    len &= 0x7F;

                    for (int i = 0; i < (int)len;)
                    {
                        count++;
                        int highbyte = 0xF;
                        int lowbyte = 0xF;
                        if (count > 0 && count <= str.Length - 1)
                        {
                            highbyte = str[count];
                            lowbyte = str[count];
                        }
                        highbyte &= 0xF0;
                        highbyte >>= 0x4;

                        lowbyte &= 0x0F;

                        if (highbyte != 0x0 && highbyte != 0xF)
                        {
                            decrypted_string += table[highbyte - 1];
                            i++;
                        }

                        if (lowbyte != 0x0 && lowbyte != 0xF)
                        {
                            decrypted_string += table[lowbyte - 1];
                            i++;
                        }
                    }
                    count++;
                }
            }

            return decrypted_string;
        }

        public static string Encrypt2(String str)
        {
            String encryptedString = String.Empty;

            for (int i = 0; i < str.Length; i++)
            {
                if (i % 0x7A == 0)
                {
                    if ((str.Length - i) > 0x7A)
                    {
                        encryptedString += Convert.ToChar(0x7A);
                    }
                    else
                    {
                        encryptedString += Convert.ToChar(str.Length - i);
                    }
                }

                encryptedString += Convert.ToChar((byte)str[i] ^ 0xFF);
            }

            encryptedString += Convert.ToChar(0xFF);

            return encryptedString;
        }

        public override string Decrypt(byte[] str, int session_id)
        {
            int length = str.Length;
            string encrypted_string = "";
            byte session_key = (byte)(session_id & 0xFF);
            byte session_number = (byte)(session_id >> 6);
            session_number &= (byte)0xFF;
            session_number &= unchecked((byte)0x80000003);

            switch (session_number)
            {
                case 0:
                    for (int i = 0; i < length; i++)
                    {
                        int firstbyte = (int)(byte)(session_key + 0x40);
                        int highbyte = (int)(byte)(str[i] - firstbyte);
                        encrypted_string += Convert.ToChar(highbyte);
                    }
                    break;

                case 1:
                    for (int i = 0; i < length; i++)
                    {
                        int firstbyte = (int)(byte)(session_key + 0x40);
                        int highbyte = (int)(byte)(str[i] + firstbyte);
                        encrypted_string += Convert.ToChar(highbyte);
                    }
                    break;

                case 2:
                    for (int i = 0; i < length; i++)
                    {
                        int firstbyte = (int)(byte)(session_key + 0x40);
                        int highbyte = (int)(byte)(str[i] - firstbyte ^ 0xC3);
                        encrypted_string += Convert.ToChar(highbyte);
                    }
                    break;

                case 3:
                    for (int i = 0; i < length; i++)
                    {
                        int firstbyte = (int)(byte)(session_key + 0x40);
                        int highbyte = (int)(byte)(str[i] + firstbyte ^ 0xC3);
                        encrypted_string += Convert.ToChar(highbyte);
                    }
                    break;

                default:
                    break;
            }

            string[] bytes = encrypted_string.Split(Convert.ToChar(0xFF));// return string less 255 (2 strings)

            string save = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                save += Decrypt2(bytes[i].ToCharArray());
                save += Convert.ToChar(0xFF);
            }
            return save;
        }

        public override string DecryptCustomParameter(byte[] str)
        {
            string encrypted_string = "";
            for (int i = 1; i < str.Length; i++)
            {
                if (Convert.ToChar(str[i]) == 0xE) { return encrypted_string; }
                string var = (str[i] - 0xF).ToString();

                int firstbyte = Convert.ToInt32((int)str[i] - (int)0xF);
                int secondbyte = firstbyte;
                secondbyte &= 0xF0;
                firstbyte = Convert.ToInt32(firstbyte - secondbyte);
                secondbyte >>= 0x4;

                switch (secondbyte)
                {
                    case 0:
                        encrypted_string += ' ';
                        break;

                    case 1:
                        encrypted_string += ' ';
                        break;

                    case 2:
                        encrypted_string += '-';
                        break;

                    case 3:
                        encrypted_string += '.';
                        break;

                    default:
                        secondbyte += 0x2C;
                        encrypted_string += Convert.ToChar(secondbyte);
                        break;
                }

                switch (firstbyte)
                {
                    case 0:
                        encrypted_string += ' ';
                        break;

                    case 1:
                        encrypted_string += ' ';
                        break;

                    case 2:
                        encrypted_string += '-';
                        break;

                    case 3:
                        encrypted_string += '.';
                        break;

                    default:
                        firstbyte += 0x2C;
                        encrypted_string += Convert.ToChar(firstbyte);
                        break;
                }
            }

            return encrypted_string;
        }

        public override byte[] Encrypt(string str)
        {
            String encryptedString = String.Empty;
            while (str.Length > 60)
            {
                encryptedString += Encrypt2(str.Substring(0, 60));
                encryptedString = encryptedString.Substring(0, encryptedString.Length - 1);
                str = str.Substring(60, str.Length - 60);
            }

            if (str.Length > 0)
                encryptedString += Encrypt2(str);
            else
                encryptedString += 0xFF;

            byte[] encryptedData = new byte[encryptedString.Length];

            for (int i = 0; i < encryptedString.Length; i++)
                encryptedData[i] = (byte)encryptedString[i];

            return encryptedData;
        }

        #endregion
    }
}