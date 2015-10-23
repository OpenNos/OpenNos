using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public enum SaveResult : byte
    {
        Unknown = 0,
        Inserted = 1,
        Updated = 2,
        Error = 3
    }
}
