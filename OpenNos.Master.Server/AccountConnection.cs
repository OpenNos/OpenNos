using OpenNos.Master.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Master.Server
{
    class AccountConnection
    {
        public AccountConnection(long accountId, long session)
        {

        }

        public long AccountId { get; private set; }

        public long SessionId { get; private set; }

        public long CharacterId { get; set; }

        public WorldServer ConnectedWorld { get; set; }
    }
}
