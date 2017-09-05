using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;

namespace OpenNos.GameObject.Helpers
{
    public static class LogHelper
    {
        public static void InsertCommandLog(long characterId, PacketDefinition commandPacket, string ipAddress)
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
            DAOFactory.LogCommandsDAO.InsertOrUpdate(ref command);
        }

        public static void InsertChatLog(ChatType type, long characterId, string message, string ipAddress)
        {
            
        }
    }
}
