using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public enum ReceiverType : byte
    {
        Unknown = 0,
        All = 1,
        AllExceptMe = 2,
        OnlyMe = 3
    }
}
