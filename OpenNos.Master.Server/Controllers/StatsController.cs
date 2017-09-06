using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OpenNos.Master.Server
{
    public class StatsController : ApiController
    {
        // GET api/stats 
        public IEnumerable<string> Get()
        {
            return CommunicationServiceClient.Instance.RetrieveServerStatistics();
        }
        
    }
}
