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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class HandlerMethodReference
    {
        public HandlerMethodReference(Action<object, object> handlerMethod, IPacketHandler parentHandler, PacketAttribute handlerMethodAttribute)
        {
            HandlerMethod = handlerMethod;
            ParentHandler = parentHandler;
            HandlerMethodAttribute = handlerMethodAttribute;
            Identification = HandlerMethodAttribute.Header;
        }

        public HandlerMethodReference(Action<object, object> handlerMethod, IPacketHandler parentHandler, Type packetBaseParameterType)
        {
            HandlerMethod = handlerMethod;
            ParentHandler = parentHandler;
            PacketBaseParameterType = packetBaseParameterType;
            Identification = ((PacketHeaderAttribute)PacketBaseParameterType.GetCustomAttributes(true).FirstOrDefault(ca => ca.GetType().Equals(typeof(PacketHeaderAttribute)))).Identification;
        }

        public PacketAttribute HandlerMethodAttribute { get; set; }

        public Action<object, object> HandlerMethod { get; set; }

        public IPacketHandler ParentHandler { get; set; }

        public Type PacketBaseParameterType { get; set; }

        /// <summary>
        /// Unique identification of the Packet by Header
        /// </summary>
        public string Identification { get; set; }
    }
}
