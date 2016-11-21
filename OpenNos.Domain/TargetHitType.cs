using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public enum TargetHitType : byte
    {
        SingleTargetHit = 0,
        SingleTargetHitCombo = 1,
        SingleAOETargetHit = 2,
        AOETargetHit = 3,
        ZoneHit = 4,
        SpecialZoneHit = 5
    }
}
