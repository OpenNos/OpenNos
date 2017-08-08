using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenNos.GameObject
{
    public class ArenaMember
    {
        public double CharacterId { get; set; }
        public double? GroupId { get; set; }
        public EventType ArenaType { get; set; }

    }
}