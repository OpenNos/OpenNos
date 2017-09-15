using System.Web.Http;
using OpenNos.Master.Library.Client;

namespace OpenNos.Master.Server.Controllers
{
    public class SessionController : ApiController
    {
        // GET api/stats 
        public void Delete(long accountId)
        {
            CommunicationServiceClient.Instance.KickSession(accountId, null);
        }
    }
}
