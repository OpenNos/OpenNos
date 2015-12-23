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
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ChatManager
    {
        private static ChatManager _instance;
        public List<ClientSession> sessions { get; set; }
        public bool shutdownActive
        {
            get; set;
        }

        private ChatManager()
        {
            sessions = new List<ClientSession>();
        }

        public static ChatManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ChatManager();

                return _instance;
            }
        }

        public bool Broadcast(ClientSession client, String message, ReceiverType receiver, String CharacterName ="", int CharacterId = -1)
        {
            switch (receiver)
            {
                case ReceiverType.All:
                    foreach (ClientSession session in sessions)
                    {
                        session.Client.SendPacket(message);
                    }

                    break;

                case ReceiverType.AllExceptMe:
                    foreach (ClientSession session in sessions)
                    {
                        if(session != client)
                        session.Client.SendPacket(message);
                    }
                    break;

                case ReceiverType.AllOnMap:
                    foreach (ClientSession session in sessions)
                    {
                        if (session.Character !=null && client.Character.MapId == session.Character.MapId)
                            session.Client.SendPacket(message);
                    }
                  
                    break;
                case ReceiverType.AllOnMapExceptMe:
                    foreach (ClientSession session in sessions)
                    {
                        if (session.Character != null && session != client && client.Character.MapId == session.Character.MapId)
                            session.Client.SendPacket(message);
                    }
                    break;
                case ReceiverType.OnlyMe:
                    client.Client.SendPacket(message);
                    break;
                case ReceiverType.OnlySomeone:
                    foreach (ClientSession session in sessions)
                    {
                        if (session.Character != null &&  (session.Character.Name == CharacterName || session.Character.CharacterId == CharacterId))
                        {
                            session.Client.SendPacket(message);
                            return true;
                        }
                           
                    }
                    return false;
                      
            }
            return true;
        }

        public void RequiereBroadcastFromMap(short MapId, string Message)
        {
                foreach (ClientSession session in sessions)
                {
                if (session.Character != null && session.Character.MapId == MapId)
                    Broadcast(session, String.Format(Message, session.Character.CharacterId),ReceiverType.AllOnMap);
                }
            
        }
        public void RequiereBroadcastFromAllMapUsers(ClientSession client, string methodName)
        {
            foreach (ClientSession session in sessions)
            {

                if (session.Character != null && session.Character.Name != client.Character.Name)
                {
                    Type t = session.Character.GetType();
                    MethodInfo method = t.GetMethod(methodName);
                    string result = (string)method.Invoke(session.Character, null);
                    client.Client.SendPacket(result);
                }
            }
        }
        public void RequiereBroadcastFromUser(ClientSession client, long CharacterId, string methodName)
        {
            foreach (ClientSession session in sessions)
            {
           
                if (session.Character != null && session.Character.CharacterId == CharacterId)
                {
                    Type t = session.Character.GetType();
                    MethodInfo method = t.GetMethod(methodName);
                    string result = (string)method.Invoke(session.Character, null);
                    client.Client.SendPacket(result);
                }
            }
        }
        public void RequiereBroadcastFromUser(ClientSession client, string CharacterName, string methodName)
        {
            foreach (ClientSession session in sessions)
            {
      
                if (session.Character != null && session.Character.Name == CharacterName)
                {
                    Type t = session.Character.GetType();
                    MethodInfo method = t.GetMethod(methodName);
                    string result = (string)method.Invoke(session.Character, null);
                    client.Client.SendPacket(result);
                }
                  
            }
        }
        public bool Kick(String CharacterName)
        {
            foreach (ClientSession session in sessions)
            {
                if (session.Character != null && session.Character.Name == CharacterName)
                {
                    session.Client.Disconnect();
                    return true;
                }
            }
            return false;
        }

     
    }
}
