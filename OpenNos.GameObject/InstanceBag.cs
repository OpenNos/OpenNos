using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class InstanceBag
    {
        #region Instantiation

        public InstanceBag()
        {
            Clock = new Clock(1);
            DeadList = new List<long>();
        }

        #endregion

        #region Properties

        public Clock Clock { get; set; }

        public int Combo { get; set; }

        public long Creator { get; set; }

        public List<long> DeadList { get; set; }

        public byte EndState { get; set; }

        public short Lives { get; set; }

        public bool Lock { get; set; }

        public int MonstersKilled { get; set; }

        public int NpcsKilled { get; set; }

        public int Point { get; set; }

        public int RoomsVisited { get; set; }

        #endregion

        #region Methods

        public string GenerateScore()
        {
            return $"rnsc {Point}";
        }

        #endregion
    }
}