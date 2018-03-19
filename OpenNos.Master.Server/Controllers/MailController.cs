using System.Web.Http;
using OpenNos.Master.Library.Client;
using OpenNos.Domain;
using OpenNos.Data;
using System;
using OpenNos.Master.Server.Controllers.ControllersParam;

namespace OpenNos.Master.Server.Controllers
{
    public class MailController : ApiController
    {
        // POST /mail 
        [AuthorizeRole(AuthorityType.GameMaster)]
        public void Post([FromBody]MailPostParameter mail)
        {
            MailDTO mail2 = new MailDTO
            {
                AttachmentAmount = mail.Amount,
                IsOpened = false,
                Date = DateTime.Now,
                ReceiverId = mail.CharacterId,
                SenderId = mail.CharacterId,
                AttachmentRarity = (byte)mail.Rare,
                AttachmentUpgrade = mail.Upgrade,
                IsSenderCopy = false,
                Title = mail.IsNosmall ? "NOSMALL" : mail.Title,
                AttachmentVNum = mail.VNum,
            };

            CommunicationServiceClient.Instance.SendMail(mail.WorldGroup, mail2);
        }
    }

}
