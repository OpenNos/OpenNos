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

using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Server;
using System;
using System.Collections.Generic;

namespace OpenNos.Core
{
    public class NetworkClient : ScsServerClient
    {
        #region Members

        private EncryptionBase _encryptor;

        #endregion

        #region Instantiation

        public NetworkClient(ICommunicationChannel communicationChannel) : base(communicationChannel)
        {
        }

        #endregion

        #region Properties

        public bool IsDisposing { get; set; }

        #endregion

        #region Methods

        public void Initialize(EncryptionBase encryptor)
        {
            _encryptor = encryptor;
        }

        public void Send(string packet)
        {
            if (!IsDisposing)
            {
                ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(packet));
                SendMessage(rawMessage);
            }
        }

        public bool SendPacket(string packet)
        {
            try
            {
                Send(packet);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendPacketFormat(string packet, params object[] param)
        {
            return SendPacket(String.Format(packet, param));
        }

        public bool SendPackets(IEnumerable<String> packets)
        {
            bool result = true;

            //TODO maybe send at once with delimiter
            foreach (string packet in packets)
            {
                result = result && SendPacket(packet);
            }

            return result;
        }

        #endregion
    }
}