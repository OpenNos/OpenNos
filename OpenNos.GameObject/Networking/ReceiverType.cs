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
        OnlyMe = 3,
        ClientRegisters = 4, //maybe improvable
        ClientUnregisters = 5 //maybe improvable
    }
}
