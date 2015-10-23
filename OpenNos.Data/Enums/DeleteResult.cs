using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public enum DeleteResult : byte
    {
        Unknown = 0,
        Deleted = 1,
        Error = 2
    }
}
