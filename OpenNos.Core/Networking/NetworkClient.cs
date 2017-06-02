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

using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Server;
using System.Collections.Generic;

namespace OpenNos.Core
{
    public class NetworkClient : ScsServerClient, INetworkClient
    {
        #region Members

        private EncryptionBase _encryptor;
        private object _session;

        #endregion

        #region Instantiation

        public NetworkClient(ICommunicationChannel communicationChannel) : base(communicationChannel)
        {
        }

        #endregion

        #region Properties

        public string IpAddress
        {
            get
            {
                return RemoteEndPoint.ToString();
            }
        }

        public bool IsConnected
        {
            get
            {
                return CommunicationState == CommunicationStates.Connected;
            }
        }

        public bool IsDisposing { get; set; }

        #endregion

        #region Methods

        public void Initialize(EncryptionBase encryptor)
        {
            _encryptor = encryptor;
        }

        public void SendPacket(string packet, byte priority = 10)
        {
            if (!IsDisposing && packet != null && packet != string.Empty)
            {
                ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(packet));
                SendMessage(rawMessage, priority);
            }
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            SendPacket(string.Format(packet, param));
        }

        public void SendPackets(IEnumerable<string> packets, byte priority = 10)
        {
            // TODO: maybe send at once with delimiter
            foreach (string packet in packets)
            {
                SendPacket(packet, priority);
            }
        }

        public void SetClientSession(object clientSession)
        {
            _session = clientSession;
        }

        #endregion
    }
}