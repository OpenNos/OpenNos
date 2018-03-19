using System.Web.Http;
using OpenNos.Master.Library.Client;
using OpenNos.Domain;

namespace OpenNos.Master.Server.Controllers
{
    public class SessionController : ApiController
    {
        // GET /stats 
        public void Delete(long accountId)
        {
            CommunicationServiceClient.Instance.KickSession(accountId, null);
        }
    }
}
