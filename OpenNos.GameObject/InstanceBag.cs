using System;
using System.Collections.Generic;
using System.Timers;

namespace OpenNos.GameObject
{
    public class InstanceBag
    {
        public Clock Clock { get; set; }
        public short Lives { get; set; }
        public List<long> DeadList { get; set; }
        public bool Lock { get; set; }
        public int MonstersKilled { get; set; }
        public int NpcsKilled { get; set; }
        public int RoomsVisited { get; set; }
        public long Creator { get; set; }
        public byte EndState { get; set; }
        public int Point { get; set; }
        public int Combo { get; set; }

        public InstanceBag()
        {
            Clock = new Clock(1);
            DeadList = new List<long>();
        }

        public string GenerateScore()
        {
            return $"rnsc {Point}";
        }
    }
}
