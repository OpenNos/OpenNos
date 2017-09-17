using System.Web.Http;
using OpenNos.Master.Library.Client;
using OpenNos.Domain;

namespace OpenNos.Master.Server.Controllers
{
    public class MailsController : ApiController
    {
        // GET /stats 
        public void Get(long accountId)
        {
            CommunicationServiceClient.Instance.UpdateMails(accountId);
        }
    }
}
