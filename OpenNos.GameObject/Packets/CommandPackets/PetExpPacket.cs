using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.CommandPackets
{
    [PacketHeader("$PetExp", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class PetExpPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int Amount { get; set; }

        public static string ReturnHelp()
        {
            return "PetExp AMOUNT";
        }
    }
}
