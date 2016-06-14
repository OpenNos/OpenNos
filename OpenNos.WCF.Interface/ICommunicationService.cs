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

using System.ServiceModel;

namespace OpenNos.WCF.Interface
{
    [ServiceContract(CallbackContract = typeof(ICommunicationCallback))]
    public interface ICommunicationService
    {
        #region Methods

        [OperationContract]
        bool AccountIsConnected(string accountName);

        [OperationContract(IsOneWay = true)]
        void Cleanup();

        [OperationContract]
        bool ConnectAccount(string accountName, int sessionId);

        [OperationContract]
        bool ConnectCharacter(string characterName, string accountName);

        [OperationContract(IsOneWay = true)]
        void DisconnectAccount(string accountName);

        [OperationContract(IsOneWay = true)]
        void DisconnectCharacter(string characterName);

        [OperationContract]
        bool HasRegisteredAccountLogin(string name, long sessionId);

        [OperationContract(IsOneWay = true)]
        void RegisterAccountLogin(string name, long sessionId);

        #endregion
    }
}