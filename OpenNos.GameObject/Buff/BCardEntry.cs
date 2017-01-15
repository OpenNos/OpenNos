using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Buff
{
    public class BCardEntry
    {
        public BCardEntry(BCard.Type type, BCard.SubType subType, int value1, int value2, bool pvpOnly, bool affectingOpposite = false)
        {
            Type = type;
            SubType = subType;
            Value1 = value1;
            Value2 = value2;
            PVPOnly = pvpOnly;
            AffectingOpposite = affectingOpposite;
        }
        public readonly BCard.Type Type;
        public readonly BCard.SubType SubType;
        public readonly int Value1;
        public readonly int Value2;
        public readonly bool PVPOnly;
        public readonly bool AffectingOpposite;
    }
}
