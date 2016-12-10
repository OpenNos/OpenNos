using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public enum CharacterRelationType : short
    {
        Blocked = -1,
        Friend = 0,
        HiddenSpouse = 2,
        Spouse = 5
    }
}
