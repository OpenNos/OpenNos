using OpenNos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.World
{
    public class WorldEncryption : EncryptionBase
    {
        public WorldEncryption() : base(true) { }

        public override string Decrypt(byte[] data, int size, int customParameter = 0)
        {
            string decryptedPacket = String.Empty;
            int session_key = customParameter & 0xFF;
            uint session_number = (uint)(customParameter >> 6);
            session_number &= (uint)0xFF;
            session_number &= (uint)2147483651;

            switch (session_number)
            {
                case 0:
                    for (int i = 0; i < size; i++)
                    {
                        byte firstbyte = (byte)(session_key + 0x40);
                        byte highbyte = (byte)(data[i] - firstbyte);
                        decryptedPacket += Convert.ToChar(highbyte);
                    }
                    break;

                case 1:
                    for (int i = 0; i < size; i++)
                    {
                        byte firstbyte = (byte)(session_key + 0x40);
                        byte highbyte = (byte)(data[i] + firstbyte);
                        decryptedPacket += Convert.ToChar(highbyte);
                    }
                    break;

                case 2:
                    for (int i = 0; i < size; i++)
                    {
                        byte firstbyte = (byte)(session_key + 0x40);
                        byte highbyte = (byte)(data[i] - firstbyte ^ 0xC3);
                        decryptedPacket += Convert.ToChar(highbyte);
                    }
                    break;

                case 3:
                    for (int i = 0; i < size; i++)
                    {
                        byte firstbyte = (byte)(session_key + 0x40);
                        byte highbyte = (byte)(data[i] + firstbyte ^ 0xC3);
                        decryptedPacket += Convert.ToChar(highbyte);
                    }
                    break;

                default:
                    decryptedPacket += Convert.ToChar(0xF);
                    break;
            }

            string[] decryptedParts = decryptedPacket.Split('ÿ');

            String decryptedPart = String.Empty;

            for (int i = 0; i < decryptedParts.Length; i++)
            {
                decryptedPart += Decrypt2(decryptedParts[i]);
                decryptedPart += Convert.ToChar(0xFF);
            }

            return decryptedPart;
        }

        public static string Decrypt2(string str)
        {
            String decryptedPacket = String.Empty;

            char[] table = new char[] { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '\n' };
            int count = 0;

            for (count = 0; count < str.Length;)
            {
                if ((byte)str[count] <= 0x7A)
                {
                    byte len = (byte)str[count];

                    for (int i = 0; i < len; i++)
                    {
                        count++;
                        decryptedPacket += Convert.ToChar((byte)str[count] ^ 0xFF);
                    }

                    count++;
                }
                else
                {
                    byte len = (byte)str[count];
                    len &= 0x7F;

                    for (int i = 0; i < (int)len;)
                    {
                        count++;

                        byte highbyte = (byte)str[count];
                        highbyte &= 0xF0;
                        highbyte >>= 0x4;

                        byte lowbyte = (byte)str[count];
                        lowbyte &= 0x0F;

                        if (highbyte != 0x0 && highbyte != 0xF)
                        {
                            decryptedPacket += Convert.ToChar(table[highbyte - 1]);
                            i++;
                        }

                        if (lowbyte != 0x0 && lowbyte != 0xF)
                        {
                            decryptedPacket += Convert.ToChar(table[lowbyte - 1]);
                            i++;
                        }
                    }
                    count++;
                }

            }

            return decryptedPacket;
        }

        public override byte[] Encrypt(string data)
        {
            String encryptedString = String.Empty;
            while (data.Length > 60)
            {

                encryptedString += Encrypt2(data.Substring(0, 60));
                encryptedString = encryptedString.Substring(0, encryptedString.Length - 1);
                data = data.Substring(60, data.Length - 60);
            }

            if (data.Length > 0)
                encryptedString += Encrypt2(data);
            else
                encryptedString += 0xFF;

            byte[] encryptedData = new byte[encryptedString.Length];

            for (int i = 0; i < encryptedString.Length; i++)
                encryptedData[i] = (byte)encryptedString[i];

            return encryptedData;
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

        public override string DecryptCustomParameter(byte[] data, int size)
        {
            string decryptedSessionPacket = String.Empty;

            for (int i = 1; i < size; i++)
            {

                if (data[i] == 14)
                {
                    return decryptedSessionPacket;
                }

                byte firstbyte = (byte)(data[i] - 15);
                byte secondbyte = firstbyte;
                secondbyte &= 240;
                firstbyte = (byte)(firstbyte - secondbyte);
                secondbyte >>= 4;

                switch (secondbyte)
                {
                    case 0:
                        decryptedSessionPacket += '\0';
                        break;

                    case 1:
                        decryptedSessionPacket += ' ';
                        break;

                    case 2:
                        decryptedSessionPacket += '-';
                        break;

                    case 3:
                        decryptedSessionPacket += '.';
                        break;

                    default:
                        secondbyte += 0x2C;
                        decryptedSessionPacket += Convert.ToChar(secondbyte);
                        break;
                }

                switch (firstbyte)
                {
                    case 0:
                        decryptedSessionPacket += '\0';
                        break;

                    case 1:
                        decryptedSessionPacket += ' ';
                        break;

                    case 2:
                        decryptedSessionPacket += '-';
                        break;

                    case 3:
                        decryptedSessionPacket += '.';
                        break;

                    default:
                        firstbyte += 0x2C;
                        decryptedSessionPacket += Convert.ToChar(firstbyte);
                        break;
                }
            }

            return decryptedSessionPacket;
        }
    }

}
