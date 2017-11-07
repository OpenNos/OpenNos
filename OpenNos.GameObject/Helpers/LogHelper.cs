using System;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject.Helpers
{
    public class LogHelper : Singleton<LogHelper>
    {
        ConcurrentBag<LogCommandsDTO> logCommands = new ConcurrentBag<LogCommandsDTO>();
        ConcurrentBag<LogChatDTO> logChat = new ConcurrentBag<LogChatDTO>();

        public void InsertCommandLog(long characterId, PacketDefinition commandPacket, string ipAddress)
        {
            string withoutHeaderpacket = string.Empty;
            string[] packet = commandPacket.OriginalContent.Split(' ');
            for (int i = 1; i < packet.Length; i++)
            {
                withoutHeaderpacket += $" {packet[i]}";
            }
            LogCommandsDTO command = new LogCommandsDTO
            {
                CharacterId = characterId,
                Command = commandPacket.OriginalHeader,
                Data = withoutHeaderpacket,
                IpAddress = ipAddress,
                Timestamp = DateTime.Now
            };
            logCommands.Add(command);
        }

        public void InsertChatLog(ChatType type, long characterId, string message, string ipAddress)
        {
            LogChatDTO log = new LogChatDTO
            {
                CharacterId = characterId,
                ChatMessage = message,
                IpAddress = ipAddress,
                ChatType = (byte)type,
                Timestamp = DateTime.Now
            };
            logChat.Add(log);
        }

        public void Flush()
        {
            List<LogChatDTO> logch = logChat.ToList();
            List<LogCommandsDTO> logcom = logCommands.ToList();
            logChat.Clear();
            logCommands.Clear();
            DAOFactory.LogChatDAO.InsertOrUpdate(logch);
            DAOFactory.LogCommandsDAO.InsertOrUpdate(logcom);
         
        }
    }
}