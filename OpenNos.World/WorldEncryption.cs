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
using System.Collections.Generic;
using System.Text;

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

        public static string Decrypt2(string str)
        {
            List<byte> receiveData = new List<byte>();
            char[] table = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };
            int count;
            for (count = 0; count < str.Length; count++)
            {
                if (str[count] <= 0x7A)
                {
                    int len = str[count];

                    for (int i = 0; i < len; i++)
                    {
                        count++;

                        try
                        {
                            receiveData.Add(unchecked((byte)(str[count] ^ 0xFF)));
                        }
                        catch
                        {
                            receiveData.Add(255);
                        }
                    }
                }
                else
                {
                    int len = str[count];
                    len &= 0x7F;

                    for (int i = 0; i < len; i++)
                    {
                        count++;
                        int highbyte;
                        try
                        {
                            highbyte = str[count];
                        }
                        catch
                        {
                            highbyte = 0;
                        }
                        highbyte &= 0xF0;
                        highbyte >>= 0x4;

                        int lowbyte;
                        try
                        {
                            lowbyte = str[count];
                        }
                        catch
                        {
                            lowbyte = 0;
                        }
                        lowbyte &= 0x0F;

                        if (highbyte != 0x0 && highbyte != 0xF)
                        {
                            receiveData.Add(unchecked((byte)table[highbyte - 1]));
                            i++;
                        }

                        if (lowbyte != 0x0 && lowbyte != 0xF)
                        {
                            receiveData.Add(unchecked((byte)table[lowbyte - 1]));
                        }
                    }
                }
            }
            return Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, receiveData.ToArray()));
        }

        public override string Decrypt(byte[] str, int sessionId = 0)
        {
            string encrypted_string = "";
            int session_key = sessionId & 0xFF;
            byte session_number = unchecked((byte)(sessionId >> 6));
            session_number &= 0xFF;
            session_number &= unchecked((byte)0x80000003);

            switch (session_number)
            {
                case 0:
                    foreach (byte character in str)
                    {
                        byte firstbyte = unchecked((byte)(session_key + 0x40));
                        byte highbyte = unchecked((byte)(character - firstbyte));
                        encrypted_string += (char)highbyte;
                    }
                    break;

                case 1:
                    foreach (byte character in str)
                    {
                        byte firstbyte = unchecked((byte)(session_key + 0x40));
                        byte highbyte = unchecked((byte)(character + firstbyte));
                        encrypted_string += (char)highbyte;
                    }
                    break;

                case 2:
                    foreach (byte character in str)
                    {
                        byte firstbyte = unchecked((byte)(session_key + 0x40));
                        byte highbyte = unchecked((byte)(character - firstbyte ^ 0xC3));
                        encrypted_string += (char)highbyte;
                    }
                    break;

                case 3:
                    foreach (byte character in str)
                    {
                        byte firstbyte = unchecked((byte)(session_key + 0x40));
                        byte highbyte = unchecked((byte)(character + firstbyte ^ 0xC3));
                        encrypted_string += (char)highbyte;
                    }
                    break;

                default:
                    encrypted_string += (char)0xF;
                    break;
            }

            string[] temp = encrypted_string.Split((char)0xFF);
            string save = "";

            for (int i = 0; i < temp.Length; i++)
            {
                save += Decrypt2(temp[i]);
                if (i < temp.Length - 2)
                {
                    save += (char)0xFF;
                }
            }

            return save;
        }

        public override string DecryptCustomParameter(byte[] str)
        {
            try
            {
                string encrypted_string = string.Empty;
                for (int i = 1; i < str.Length; i++)
                {
                    if (Convert.ToChar(str[i]) == 0xE)
                    {
                        return encrypted_string;
                    }

                    int firstbyte = Convert.ToInt32(str[i] - 0xF);
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
            catch (OverflowException)
            {
                return string.Empty;
            }
        }

        public override byte[] Encrypt(string str)
        {
            byte[] StrBytes = Encoding.Default.GetBytes(str);
            int BytesLength = StrBytes.Length;

            byte[] encryptedData = new byte[BytesLength + (int)Math.Ceiling((decimal)BytesLength / 0x7E) + 1];

            int ii = 0;
            for (int i = 0; i < BytesLength; i++)
            {
                if (i % 0x7E == 0)
                {
                    encryptedData[i + ii] = (byte)(BytesLength - i > 0x7E ? 0x7E : BytesLength - i);
                    ii++;
                }
                encryptedData[i + ii] = (byte)~StrBytes[i];
            }
            encryptedData[encryptedData.Length - 1] = 0xFF;

            return encryptedData;
        }

        #endregion
    }
}