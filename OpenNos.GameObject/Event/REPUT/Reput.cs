using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.WebApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Reput
    {
      public static void GenerateReput()
        {
            foreach (var genlog in ServerManager.GeneralLogs.Where(s => s.LogData == "MINILAND" && s.Timestamp > DateTime.Now.AddDays(-1)).GroupBy(s => s.CharacterId))
            {
                ClientSession Session = ServerManager.Instance.GetSessionByCharacterId((long)genlog.Key);
                if (Session == null)
                {
                    Session.Character.GetReput(2 * genlog.Count());
                }
                else if (!ServerCommunicationClient.Instance.HubProxy.Invoke<bool>("CharacterIsConnected", (long)genlog.Key, ServerManager.ServerGroup).Result)
                {
                    CharacterDTO chara = DAOFactory.CharacterDAO.LoadById((long)genlog.Key);
                    if (chara != null)
                    {
                        chara.Reput += 2 * genlog.Count();
                        DAOFactory.CharacterDAO.InsertOrUpdate(ref chara);
                    }
                }
                ServerManager.Instance.StartedEvents.Remove(EventType.REPUTEVENT);
            }
        }

    }
}
