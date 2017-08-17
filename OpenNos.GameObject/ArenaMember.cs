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
        public ClientSession Session { get; set; }
        public double? GroupId { get; set; }
        public EventType ArenaType { get; set; }
        public int Time { get;  set; }
    }
}