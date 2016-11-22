/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class BattlePacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public BattlePacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session
        {
            get
            {
                return _session;
            }
        }

        #endregion

        #region Methods

        [Packet("mtlist")]
        public void SpecialZoneHit(string packet)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted())
            {
                Session.SendPacket("cancel 0 0");
                Session.CurrentMap?.Broadcast(Session.Character.Gender == GenderType.Female ? Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1) :
                    Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                return;
            }
            if ((DateTime.Now - Session.Character.LastTransform).TotalSeconds < 3)
            {
                Session.SendPacket("cancel 0 0");
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACKNOW"), 0));
                return;
            }
            if (Session.Character.IsVehicled)
            {
                Session.SendPacket("cancel 0 0");
                return;
            }
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (packetsplit.Length > 3)
            {
                // get amount of mobs which are targeted
                short mobAmount = 0;
                short.TryParse(packetsplit[2], out mobAmount);
                if ((packetsplit.Length - 3) / 2 != mobAmount)
                {
                    return;
                }

                for (int i = 3; i < packetsplit.Length - 2; i += 2)
                {
                    List<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp.GetAllItems() : Session.Character.Skills.GetAllItems();
                    if (skills != null)
                    {
                        short mapMonsterTargetId = -1;
                        short skillCastId = -1;

                        if (short.TryParse(packetsplit[i], out skillCastId) && short.TryParse(packetsplit[i + 1], out mapMonsterTargetId))
                        {
                            Task t = Task.Factory.StartNew((Func<Task>)(async () =>
                            {
                                CharacterSkill ski = skills.FirstOrDefault(s => s.Skill.CastId == skillCastId - 1);
                                if (ski.CanBeUsed())
                                {
                                    MapMonster mon = Session.CurrentMap.GetMonster(mapMonsterTargetId);
                                    if (mon != null && mon.IsInRange(Session.Character.MapX, Session.Character.MapY, ski.Skill.Range) && ski != null && mon.CurrentHp > 0)
                                    {
                                        Session.Character.LastSkillUse = DateTime.Now;
                                        mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.SpecialZoneHit, Session, ski.Skill));
                                    }

                                    await Task.Delay((ski.Skill.Cooldown) * 100);
                                    Session.SendPacket($"sr {skillCastId - 1}");
                                }
                            }));
                        }
                    }
                }
            }
        }

        public void TargetHit(int castingId, int targetId)
        {
            if ((DateTime.Now - Session.Character.LastTransform).TotalSeconds < 3)
            {
                Session.SendPacket("cancel 0 0");
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                return;
            }

            bool doNotCancel = false;
            List<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp.GetAllItems() : Session.Character.Skills.GetAllItems();

            if (skills != null)
            {
                CharacterSkill ski = skills.FirstOrDefault(s => s.Skill?.CastId == castingId && s.Skill?.UpgradeSkill == 0);
                Session.SendPacket("ms_c 0");
                if (!Session.Character.WeaponLoaded(ski))
                {
                    Session.SendPacket("cancel 2 0");
                    return;
                }
                for (int i = 0; i < 10 && !ski.CanBeUsed(); i++)
                {
                    Thread.Sleep(100);
                    if (i == 10)
                    {
                        Session.SendPacket("cancel 2 0");
                        return;
                    }
                }

                if (ski != null && Session.Character.Mp >= ski.Skill.MpCost)
                {
                    if (ski.Skill.TargetType == 1 && ski.Skill.HitType == 1) // AOE Target hit
                    {
                        Session.Character.LastSkillUse = DateTime.Now;
                        if (!Session.Character.HasGodMode)
                        {
                            Session.Character.Mp -= ski.Skill.MpCost;
                        }

                        if (Session.Character.UseSp && ski.Skill.CastEffect != -1)
                        {
                            Session.SendPackets(Session.Character.GenerateQuicklist());
                        }

                        Session.SendPacket(Session.Character.GenerateStat());
                        CharacterSkill skillinfo = Session.Character.Skills.GetAllItems().OrderBy(o => o.SkillVNum).FirstOrDefault(s => s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0 && s.Skill.SkillType == 2);
                        Session.CurrentMap?.Broadcast($"ct 1 {Session.Character.CharacterId} 1 {Session.Character.CharacterId} {ski.Skill.CastAnimation} {(skillinfo != null ? skillinfo.Skill.CastEffect : ski.Skill.CastEffect)} {ski.Skill.SkillVNum}");

                        // Generate scp
                        ski.LastUse = DateTime.Now;
                        if (ski.Skill.CastEffect != 0)
                        {
                            Thread.Sleep(ski.Skill.CastTime * 100);
                        }

                        doNotCancel = true;
                        Session.CurrentMap.Broadcast($"su 1 {Session.Character.CharacterId} 1 {Session.Character.CharacterId} {ski.Skill.SkillVNum} {ski.Skill.Cooldown} {ski.Skill.AttackAnimation} {(skillinfo != null ? skillinfo.Skill.Effect : ski.Skill.Effect)} {Session.Character.MapX} {Session.Character.MapY} 1 {((int)((double)Session.Character.Hp / Session.Character.HPLoad()) * 100)} 0 -2 {ski.Skill.SkillType - 1}");
                        if (ski.Skill.TargetRange != 0)
                        {
                            foreach (MapMonster mon in Session.CurrentMap.GetListMonsterInRange(Session.Character.MapX, Session.Character.MapY, ski.Skill.TargetRange).Where(s => s.CurrentHp > 0))
                            {
                                mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.AOETargetHit, Session, ski.Skill, skillEffect: (skillinfo != null ? skillinfo.Skill.Effect : ski.Skill.Effect)));
                            }
                        }
                    }
                    else if (ski.Skill.TargetType == 0) // monster target
                    {
                        MapMonster monsterToAttack = Session.CurrentMap.GetMonster(targetId);
                        if (monsterToAttack != null && monsterToAttack.IsAlive)
                        {
                            if (monsterToAttack != null && ski != null && ski.CanBeUsed())
                            {
                                if (Session.Character.Mp >= ski.Skill.MpCost)
                                {
                                    short distanceX = (short)(Session.Character.MapX - monsterToAttack.MapX);
                                    short distanceY = (short)(Session.Character.MapY - monsterToAttack.MapY);

                                    if (Map.GetDistance(new MapCell() { X = Session.Character.MapX, Y = Session.Character.MapY },
                                                        new MapCell() { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }) <= ski.Skill.Range + (DateTime.Now - monsterToAttack.LastMove).TotalSeconds * 2 * (monsterToAttack.Monster.Speed == 0 ? 1 : monsterToAttack.Monster.Speed) || ski.Skill.TargetRange != 0)
                                    {
                                        Session.Character.LastSkillUse = DateTime.Now;
                                        ski.LastUse = DateTime.Now;
                                        doNotCancel = true;
                                        if (!Session.Character.HasGodMode)
                                        {
                                            Session.Character.Mp -= ski.Skill.MpCost;
                                        }
                                        if (Session.Character.UseSp && ski.Skill.CastEffect != -1)
                                        {
                                            Session.SendPackets(Session.Character.GenerateQuicklist());
                                        }
                                        Session.SendPacket(Session.Character.GenerateStat());
                                        CharacterSkill characterSkillInfo = Session.Character.Skills.GetAllItems().OrderBy(o => o.SkillVNum)
                                            .FirstOrDefault(s => s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0 && s.Skill.SkillType == 2);

                                        Session.CurrentMap?.Broadcast($"ct 1 {Session.Character.CharacterId} 3 {monsterToAttack.MapMonsterId} {ski.Skill.CastAnimation} {(characterSkillInfo != null ? characterSkillInfo.Skill.CastEffect : ski.Skill.CastEffect)} {ski.Skill.SkillVNum}");
                                        Session.Character.Skills.GetAllItems().Where(s => s.Id != ski.Id).ToList().ForEach(i => i.Hit = 0);

                                        // Generate scp
                                        ski.LastUse = DateTime.Now;
                                        if ((DateTime.Now - ski.LastUse).TotalSeconds > 3)
                                        {
                                            ski.Hit = 0;
                                        }
                                        else
                                        {
                                            ski.Hit++;
                                        }

                                        if (ski.Skill.CastEffect != 0)
                                        {
                                            Thread.Sleep(ski.Skill.CastTime * 100);
                                        }

                                        ComboDTO skillCombo = ski.Skill.Combos.FirstOrDefault(s => ski.Hit == s.Hit);
                                        if (skillCombo != null)
                                        {
                                            if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                            {
                                                ski.Hit = 0;
                                            }
                                            monsterToAttack.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.SingleTargetHitCombo, Session, ski.Skill, skillCombo: skillCombo));
                                        }
                                        else
                                        {
                                            monsterToAttack.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.SingleTargetHit, Session, ski.Skill));
                                        }

                                        if (ski.Skill.TargetRange != 0)
                                        {
                                            IEnumerable<MapMonster> monstersInAOERange = Session.CurrentMap?.GetListMonsterInRange(monsterToAttack.MapX, monsterToAttack.MapY, ski.Skill.TargetRange).ToList();
                                            foreach (MapMonster mon in monstersInAOERange.Where(s => s.CurrentHp > 0))
                                            {
                                                mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.SingleAOETargetHit, Session, ski.Skill, skillEffect: (characterSkillInfo != null ? characterSkillInfo.Skill.Effect : ski.Skill.Effect)));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Task t = Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay((ski.Skill.Cooldown * 100));
                        Session.SendPacket($"sr {castingId}");
                    });
                }
                else
                {
                    doNotCancel = false;
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                }
            }

            if (!doNotCancel)
            {
                Session.SendPacket($"cancel 2 {targetId}");
            }
        }

        [Packet("u_s")]
        public void UseSkill(string packet)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted())
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.SendPacket("cancel 0 0");
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
                else
                {
                    Session.SendPacket("cancel 0 0");
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
                return;
            }
            if (Session.Character.CanFight)
            {
                Logger.Debug(packet, Session.SessionId);
                string[] packetsplit = packet.Split(' ');
                if (packetsplit.Length > 6)
                {
                    short MapX = -1, MapY = -1;
                    if (!short.TryParse(packetsplit[5], out MapX) || !short.TryParse(packetsplit[6], out MapY))
                    {
                        return;
                    }
                    Session.Character.MapX = MapX;
                    Session.Character.MapY = MapY;
                }
                byte usrType;
                if (!byte.TryParse(packetsplit[3], out usrType))
                {
                    return;
                }
                byte usertype = usrType;
                if (Session.Character.IsSitting)
                {
                    Session.Character.Rest();
                }
                if (Session.Character.IsVehicled || Session.Character.InvisibleGm)
                {
                    Session.SendPacket("cancel 0 0");
                    return;
                }
                switch (usertype)
                {
                    case (byte)UserType.Monster:
                        if (packetsplit.Length > 4)
                        {
                            if (Session.Character.Hp > 0)
                            {
                                TargetHit(Convert.ToInt32(packetsplit[2]), Convert.ToInt32(packetsplit[4]));
                            }
                        }
                        break;

                    case (byte)UserType.Player:
                        if (packetsplit.Length > 4)
                        {
                            if (Session.Character.Hp > 0 && Convert.ToInt64(packetsplit[4]) == Session.Character.CharacterId)
                            {
                                TargetHit(Convert.ToInt32(packetsplit[2]), Convert.ToInt32(packetsplit[4]));
                            }
                            else
                            {
                                Session.SendPacket("cancel 2 0");
                            }
                        }
                        break;

                    default:
                        Session.SendPacket("cancel 2 0");
                        return;
                }
            }
        }

        [Packet("u_as")]
        public void UseZonesSkill(string packet)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted())
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.SendPacket("cancel 0 0");
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
                else
                {
                    Session.SendPacket("cancel 0 0");
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
            }
            else
            {
                if (Session.Character.LastTransform.AddSeconds(3) > DateTime.Now)
                {
                    Session.SendPacket("cancel 0 0");
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                    return;
                }
                if (Session.Character.IsVehicled)
                {
                    Session.SendPacket("cancel 0 0");
                    return;
                }
                Logger.Debug(packet, Session.SessionId);
                if (Session.Character.CanFight)
                {
                    string[] packetsplit = packet.Split(' ');
                    if (packetsplit.Length > 4)
                    {
                        if (Session.Character.Hp > 0)
                        {
                            int CastingId;
                            short x = -1;
                            short y = -1;
                            if (!int.TryParse(packetsplit[2], out CastingId) || !short.TryParse(packetsplit[3], out x) || !short.TryParse(packetsplit[4], out y))
                            {
                                return;
                            }
                            ZoneHit(CastingId, x, y);
                        }
                    }
                }
            }
        }

        private void ZoneHit(int Castingid, short x, short y)
        {
            List<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp.GetAllItems() : Session.Character.Skills.GetAllItems();
            CharacterSkill characterSkill = skills.FirstOrDefault(s => s.Skill.CastId == Castingid);
            if (!Session.Character.WeaponLoaded(characterSkill))
            {
                Session.SendPacket("cancel 2 0");
                return;
            }

            if (characterSkill != null && characterSkill.CanBeUsed())
            {
                if (Session.Character.Mp >= characterSkill.Skill.MpCost)
                {
                    Task t = Task.Factory.StartNew(async () =>
                    {
                        Session.CurrentMap?.Broadcast($"ct_n 1 {Session.Character.CharacterId} 3 -1 {characterSkill.Skill.CastAnimation} {characterSkill.Skill.CastEffect} {characterSkill.Skill.SkillVNum}");
                        characterSkill.LastUse = DateTime.Now;
                        if (!Session.Character.HasGodMode)
                        {
                            Session.Character.Mp -= characterSkill.Skill.MpCost;
                        }
                        Session.SendPacket(Session.Character.GenerateStat());
                        characterSkill.LastUse = DateTime.Now;
                        await Task.Delay(characterSkill.Skill.CastTime * 100);
                        Session.Character.LastSkillUse = DateTime.Now;

                        Session.CurrentMap?.Broadcast($"bs 1 {Session.Character.CharacterId} {x} {y} {characterSkill.Skill.SkillVNum} {characterSkill.Skill.Cooldown} {characterSkill.Skill.AttackAnimation} {characterSkill.Skill.Effect} 0 0 1 1 0 0 0");

                        IEnumerable<MapMonster> monstersInRange = Session.CurrentMap.GetListMonsterInRange(x, y, characterSkill.Skill.TargetRange).ToList();
                        foreach (MapMonster mon in monstersInRange.Where(s => s.CurrentHp > 0))
                        {
                            mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.ZoneHit, Session, characterSkill.Skill, x, y));
                        }

                        await Task.Delay((characterSkill.Skill.Cooldown) * 100);
                        Session.SendPacket($"sr {Castingid}");
                    });
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                    Session.SendPacket("cancel 2 0");
                }
            }
            else
            {
                Session.SendPacket("cancel 2 0");
            }
        }

        #endregion
    }
}