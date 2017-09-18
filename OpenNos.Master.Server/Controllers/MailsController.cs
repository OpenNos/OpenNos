using System.Web.Http;
using OpenNos.Master.Library.Client;
using OpenNos.Domain;
using OpenNos.Data;

namespace OpenNos.Master.Server.Controllers
{
    public class MailsController : ApiController
    {
        // GET /stats 
        public void Post(long accountId, string worldgroup)
        {
            MailDTO mail = new MailDTO();
            CommunicationServiceClient.Instance.SendMail(worldgroup, mail);
        }
    }
}
