using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class TimeSpace
    {
        public short LevelMaximum { get; set; }

        public short LevelMinimum { get; set; }

        public TimeSpaceType Type { get; set; }

        public PortalDTO Portal { get; set; }

        public List<Gift> BonusItemGift { get; set; }

        public List<Gift> SpecialItemGift { get; set; }

        public List<Gift> DrawItemGift { get; set; }

        public string Label { get; set; }

    }
}