using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenNos.GameObject
{
    public class ArenaTeamMember
    {
        public ClientSession Session { get; set; }
        public ArenaTeamType ArenaTeamType { get; set; }
        public byte? Order { get; set; }
        public bool Dead { get; set; }

        public ArenaTeamMember(ClientSession session, ArenaTeamType arenaTeamType, byte? order)
        {
            Session = session;
            ArenaTeamType = arenaTeamType;
            Order = order;
        }
    }
}