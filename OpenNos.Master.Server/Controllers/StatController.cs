using OpenNos.Domain;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OpenNos.Master.Server
{

    public class StatController : ApiController
    {
        [AuthorizeRole(AuthorityType.Moderator)]
        // GET /stat
        public IEnumerable<string> Get()
        {
            return CommunicationServiceClient.Instance.RetrieveServerStatistics();
        }
        
    }
}
