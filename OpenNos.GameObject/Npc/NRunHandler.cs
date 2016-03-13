using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class NRunHandler
    {
       public static void NRun(ClientSession Session, byte type, short runner, short data3, short npcid)
        {
            switch (runner)
            {
                case 1:
                    if (Session.Character.Class != (byte)ClassType.Adventurer)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                        return;
                    }
                    if (Session.Character.Level < 15 || Session.Character.JobLevel < 20)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                        return;
                    }

                    if (Session.Character.EquipmentList.isEmpty())
                    {
                        ClientLinkManager.Instance.ClassChange(Session.Character.CharacterId,Convert.ToByte(type));
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                case 2:
                    Session.Client.SendPacket($"wopen 1 0");
                    break;

                case 10:
                    Session.Client.SendPacket($"wopen 3 0");
                    break;

                case 12:
                    Session.Client.SendPacket($"wopen {type} 0");
                    break;

                case 14:
                    // m_list 2 1002 1003 1004 1005 1006 1007 1008 1009 1010 180 181 2127 2178 1242 1243 1244 2504 2505 - 100
                    Session.Client.SendPacket($"wopen 27 0");
                    break;
                default:
                    Logger.Log.Warn(String.Format(Language.Instance.GetMessageFromKey("NO_NRUN_HANDLER"),runner));
                    break;
            }
        }
    }
}
