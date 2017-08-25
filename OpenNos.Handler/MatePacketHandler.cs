using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class MatePacketHandler : IPacketHandler
    {
        public MatePacketHandler(ClientSession session)
        {
            _session = session;
        }

        private ClientSession _session { get; set; }

        /// <summary>
        /// suctl packet
        /// </summary>
        /// <param name="suctlPacket"></param>
        public void Attack(SuctlPacket suctlPacket)
        {
            PenaltyLogDTO penalty = _session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (_session.Character.IsMuted() && penalty != null)
            {
                if (_session.Character.Gender == GenderType.Female)
                {
                    _session.SendPacket("cancel 0 0");
                    _session.CurrentMapInstance?.Broadcast(_session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    _session.SendPacket(_session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    _session.SendPacket("cancel 0 0");
                    _session.CurrentMapInstance?.Broadcast(_session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    _session.SendPacket(_session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                return;
            }
            Mate mate = _session.Character.Mates.Where(x => x.MateTransportId == suctlPacket.MateTransportId).First();
            switch (suctlPacket.TargetType)
            {
                case UserType.Monster:
                    if (mate.Hp > 0)
                    {

                    }
                    break;

                case UserType.Npc:
                    break;

                case UserType.Player:
                    break;
            }
        }
    }
}
