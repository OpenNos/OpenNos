using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public enum ConfigType : byte
    {
        ExchangeBlocked = 1,
        FriendRequestBlocked = 2,
        WhisperBlocked = 4,
        GroupRequestBlocked = 5,
        MouseAimLock = 9,
        HeroChatBlocked = 10,
        EmoticonsBlocked = 11,
        QuickGetUp = 12,
        HpBlocked = 13,
        BuffBlocked = 14,
        MinilandInviteBlocked = 15
    }
}
