using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public enum AuthorityType : short
    {
        Unknown = 0,
        User = 1,
        Admin = 2,
        Banned = 3
    }
}
