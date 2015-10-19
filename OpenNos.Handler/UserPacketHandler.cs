using OpenNos.Core;
using OpenNos.Core.Communication.Scs.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class UserPacketHandler : PacketHandlerBase
    {
        private readonly CustomScsServerClient _client;

        public UserPacketHandler(CustomScsServerClient client)
        {
            _client = client;
        }

        [Packet("OpenNos.EntryPoint")]
        public ScsTextMessage Initialize(long clientId)
        {
            //TODO Initialize User
            return new ScsTextMessage();
        }
    }
}
