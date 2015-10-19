using OpenNos.Core.Communication.Scs.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core.Communication.Scs.Communication.Channels;

namespace OpenNos.Core
{
    public class CustomScsServerClient : ScsServerClient
    {
        private IDictionary<string, object> _handlers { get; set; }
 
        public int SessionId { get; set; }
        public int LastKeepAliveIdentity { get; set; }

        public CustomScsServerClient(ICommunicationChannel communicationChannel) : base(communicationChannel)
        {
            SessionId = 0;
        }

        public IDictionary<string, object> Handlers
        {
            get
            {
                if (_handlers == null)
                {
                    _handlers = new Dictionary<string, object>();
                }

                return _handlers;
            }

            set
            {
                _handlers = value;
            }
        }
    }
}
