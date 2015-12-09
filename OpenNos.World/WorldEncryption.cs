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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.World
{
    public class WorldEncryption : EncryptionBase
    {
        public WorldEncryption() : base(true) { }
    
        public override string Decrypt(byte[] str, int session_id)
        {
            int length = str.Length ;
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
                        byte firstbyte = (byte)(session_key + 0x40);
                        byte highbyte = (byte)(str[i] - firstbyte);
                        encrypted_string += getextendedascii(highbyte);
                    }
                    break;

                case 1:
                    for (int i = 0; i < length; i++)
                    {
                        byte firstbyte = (byte)(session_key + 0x40);
                        byte highbyte = (byte)(str[i] + firstbyte);
                        encrypted_string += getextendedascii(highbyte);
                    }
                    break;

                case 2:
                    for (int i = 0; i < length; i++)
                    {
                        byte firstbyte = (byte)(session_key + 0x40);
                        byte highbyte = (byte)(str[i] - firstbyte ^ 0xC3);
                        encrypted_string += getextendedascii(highbyte);
                    }
                    break;

                case 3:
                    for (int i = 0; i < length; i++)
                    {
                        byte firstbyte = (byte)(session_key + 0x40);
                        byte highbyte = (byte)(str[i] + firstbyte ^ 0xC3);
                        encrypted_string += getextendedascii(highbyte);
                    }
                    break;

                default:
                    encrypted_string += 0xF;
                    break;
            }

            string[] bytes = encrypted_string.Split((char)0xFF);// return string less 255 (2 strings)
        
            string save = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                
                save += Decrypt2(System.Text.ASCIIEncoding.Default.GetBytes(bytes[i]));         
                save += (char)0xFF;

            }
            return save;
          
        }

        public static string Decrypt2(byte[] str)
        {

            string decrypted_string = "";
            char[] table = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };
            int count = 0;


            for (count = 0; count < str.Length - 1;)
            {
                if (str[count] <= 0x7A)
                {
                    byte len = str[count];

                    for (int i = 0; i < (int)len; i++)
                    {
                        count++;

                        decrypted_string += getextendedascii((count < str.Length ? str[count] : 0) ^ 0xFF);
                    }
                    int x = decrypted_string[1];
                    count++;
                }
                else
                {
                    byte len = (byte)str[count];
                    len &= (byte)0x7F;

                    for (int i = 0; i < (int)len;)
                    {
                        count++;
                        byte highbyte = 0xF;
                        byte lowbyte = 0xF;
                        if (count > 0 && count <= str.Length-1)
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
        public override byte[] Encrypt(string str)
        {
            
            string encrypted_string = "";
            int length = str.Length;
            int secondlength = (length / 122);
            int compteur = 0;

            for (int i = 0; i < length; i++)
            {
                if (i == (122 * compteur))
                {
                    if (secondlength == 0)
                    {
                        encrypted_string += getextendedascii((char)Math.Abs((((length / 122) * 122) - length)));
                    }
                    else
                    {
                        encrypted_string += getextendedascii((char)0x7A);
                        secondlength--;
                        compteur++;
                    }
                }

                encrypted_string += getextendedascii((byte)(str[i] ^ (byte)0xFF));
            }

            encrypted_string += getextendedascii((char)0xFF);
            byte[] ret = Encoding.GetEncoding(1252).GetBytes(encrypted_string);
            return ret;
        }
        public static string getextendedascii(int x)
        {
            var e = Encoding.GetEncoding("Windows-1252");
            var s = e.GetString(new byte[] { Convert.ToByte(x) });

            return s;
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
                        encrypted_string += getextendedascii(secondbyte);
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
                        encrypted_string += getextendedascii(firstbyte);
                        break;
                }
            }

            return encrypted_string;

        }
    }

}
