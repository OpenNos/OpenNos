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
            Session = session;
        }

        private ClientSession Session { get; }

        /// <summary>
        /// suctl packet
        /// </summary>
        /// <param name="suctlPacket"></param>
        public void Attack(SuctlPacket suctlPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                return;
            }
            Mate attacker = Session.Character.Mates.First(x => x.MateTransportId == suctlPacket.MateTransportId);
            switch (suctlPacket.TargetType)
            {
                case UserType.Monster:
                    if (attacker.Hp > 0)
                    {
                        MapMonster target = Session?.CurrentMapInstance?.GetMonster(suctlPacket.TargetId);
                        NpcMonsterSkill skill = attacker.Monster.Skills.FirstOrDefault(x => x.NpcMonsterSkillId == suctlPacket.CastId);
                        AttackMonster(attacker, skill, target);
                    }
                    return;

                case UserType.Npc:
                    return;

                case UserType.Player:
                    return;

                case UserType.Object:
                    return;

                default:
                    return;
            }
        }

        public void AttackMonster(Mate attacker, NpcMonsterSkill skill, MapMonster target)
        {
            if (target == null || attacker == null)
            {
                return;
            }
            if (target.CurrentHp > 0)
            {
                int dmg = 100; //TEST
                if (skill == null)
                {
                    Session?.CurrentMapInstance?.Broadcast($"ct 2 {attacker.MateTransportId} 3 {target.MapMonsterId} -1 -1 0");
                    target.CurrentHp -= dmg;
                    if (target.CurrentHp <= 0)
                    {
                        target.CurrentHp = 0;
                        target.IsAlive = false;
                    }
                    Session?.CurrentMapInstance?.Broadcast($"su 2 {attacker.MateTransportId} 3 {target.MapMonsterId} 0 12 11 200 0 0 {(target.IsAlive ? 1 : 0)} {(int) ((double) target.CurrentHp / target.Monster.MaxHP * 100)} {dmg} 0 0");
                }
            }
        }

        public void AttackCharacter(Mate attacker, NpcMonsterSkill skill, Character target)
        {
            
        }
    }
}
