using OpenNos.ServiceRef.Internal.CommunicationServiceReference;
using OpenNos.WCF.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.ServiceRef.Internal
{
    public class CommunicationCallback : ICommunicationServiceCallback, IDisposable
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void RegisterPlayerLoginCallback(string value)
        {
            //communication callback will be useful for chatting (register a entity to received messages by callback)
            //todo inform client
        }
    }
}
