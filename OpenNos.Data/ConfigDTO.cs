using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class ConfigDTO
    {
        public long ConfigId { get; set; }

        public bool FriendRequestBlocked { get; set; }

        public bool WhisperBlocked { get; set; }

        public bool GroupRequestBlocked { get; set; }

        public bool MouseAimLock { get; set; }

        public bool HeroChatBlocked { get; set; }

        public bool QuickGetUp { get; set; }

        public bool EmoticonsBlocked { get; set; }

        public bool HpBlocked { get; set; }

        public bool BuffBlocked { get; set; }

        public bool MinilandInviteBlocked { get; set; }
    }
}
