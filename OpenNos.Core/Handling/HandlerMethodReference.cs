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

using OpenNos.Domain;
using System;
using System.Linq;

namespace OpenNos.Core.Handling
{
    public class HandlerMethodReference
    {
        #region Instantiation

        public HandlerMethodReference(Action<object, object> handlerMethod, IPacketHandler parentHandler, PacketAttribute handlerMethodAttribute)
        {
            HandlerMethod = handlerMethod;
            ParentHandler = parentHandler;
            HandlerMethodAttribute = handlerMethodAttribute;
            Identification = HandlerMethodAttribute.Header;
            PassNonParseablePacket = false;
            Authority = AuthorityType.User;
        }

        public HandlerMethodReference(Action<object, object> handlerMethod, IPacketHandler parentHandler, Type packetBaseParameterType)
        {
            HandlerMethod = handlerMethod;
            ParentHandler = parentHandler;
            PacketDefinitionParameterType = packetBaseParameterType;
            PacketHeaderAttribute headerAttribute = (PacketHeaderAttribute)PacketDefinitionParameterType.GetCustomAttributes(true).FirstOrDefault(ca => ca.GetType().Equals(typeof(PacketHeaderAttribute)));
            Identification = headerAttribute?.Identification;
            PassNonParseablePacket = headerAttribute?.PassNonParseablePacket ?? false;
            Authority = headerAttribute?.Authority ?? AuthorityType.User;
        }

        #endregion

        #region Properties

        public AuthorityType Authority { get; private set; }

        public Action<object, object> HandlerMethod { get; private set; }

        public PacketAttribute HandlerMethodAttribute { get; }

        /// <summary>
        /// Unique identification of the Packet by Header
        /// </summary>
        public string Identification { get; private set; }

        public Type PacketDefinitionParameterType { get; }

        public IPacketHandler ParentHandler { get; private set; }

        public bool PassNonParseablePacket { get; private set; }

        #endregion
    }
}