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
using System.Text;

namespace OpenNos.Core
{
    public class LoginEncryption : EncryptionBase
    {
        #region Instantiation

        public LoginEncryption() : base(false)
        {
        }

        #endregion

        #region Methods

        public override string Decrypt(byte[] packet, int customParameter = 0)
        {
            try
            {
                string decryptedPacket = string.Empty;

                for (int i = 0; i < packet.Length; i++)
                {
                    if (packet[i] > 14)
                    {
                        decryptedPacket += Convert.ToChar((packet[i] - 15) ^ 195);
                    }
                    else
                    {
                        decryptedPacket += Convert.ToChar((256 - (15 - (packet[i]))) ^ 195);
                    }
                }

                return decryptedPacket;
            }
            catch
            {
                return string.Empty;
            }
        }

        public override string DecryptCustomParameter(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] Encrypt(string packet)
        {
            try
            {
                packet += " ";
                byte[] tmp = new byte[packet.Length + 1];
                tmp = Encoding.UTF8.GetBytes(packet);
                for (int i = 0; i < packet.Length; i++)
                {
                    tmp[i] = Convert.ToByte(packet[i] + 15);
                }
                tmp[tmp.Length - 1] = 25;
                return tmp;
            }
            catch
            {
                return new byte[0];
            }
        }

        #endregion
    }
}