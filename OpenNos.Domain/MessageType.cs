using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public enum MessageType : byte
    {
        Whisper = 0,
        PrivateChat = 1,
        Family = 2,
        Shout = 3,
        FamilyChat = 4
    }
}
