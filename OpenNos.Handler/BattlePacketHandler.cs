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
using System.Reactive.Linq;
using System.Threading;

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

        /// <summary>
        /// mtlist
        /// </summary>
        /// <param name="mutliTargetListPacket"></param>
        public void MultiTargetListHit(MultiTargetListPacket mutliTargetListPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                Session.SendPacket("cancel 0 0");
                Session.CurrentMap?.Broadcast(Session.Character.Gender == GenderType.Female ? Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1) :
                    Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
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
            Logger.Debug(mutliTargetListPacket.ToString(), Session.SessionId);
            if (mutliTargetListPacket.TargetsAmount > 0 && mutliTargetListPacket.TargetsAmount == mutliTargetListPacket.Targets.Count)
            {
                foreach (MultiTargetListSubPacket subpacket in mutliTargetListPacket.Targets)
                {
                    List<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp.GetAllItems() : Session.Character.Skills.GetAllItems();
                    CharacterSkill ski = skills?.FirstOrDefault(s => s.Skill.CastId == subpacket.SkillCastId - 1);
                    if (ski != null && ski.CanBeUsed() && Session.HasCurrentMap)
                    {
                        MapMonster mon = Session.CurrentMap.GetMonster(subpacket.TargetId);
                        if (mon != null && mon.IsInRange(Session.Character.MapX, Session.Character.MapY, ski.Skill.Range) && mon.CurrentHp > 0)
                        {
                            Session.Character.LastSkillUse = DateTime.Now;
                            mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.SpecialZoneHit, Session, ski.Skill));
                        }

                        Observable.Timer(TimeSpan.FromMilliseconds(ski.Skill.CastTime * 100))
                            .Subscribe(
                                o =>
                                {
                                    Session.SendPacket($"sr {subpacket.SkillCastId - 1}");
                                }
                            );
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

            List<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp.GetAllItems() : Session.Character.Skills.GetAllItems();

            if (skills != null)
            {
                CharacterSkill ski = skills.FirstOrDefault(s => s.Skill?.CastId == castingId && s.Skill?.UpgradeSkill == 0);
                Session.SendPacket("ms_c 0");
                if (ski != null && (!Session.Character.WeaponLoaded(ski) || !ski.CanBeUsed()))
                {
                    Session.SendPacket($"cancel 2 {targetId}");
                    return;
                }

                if (ski != null && Session.Character.Mp >= ski.Skill.MpCost)
                {
                    // AOE Target hit
                    if (ski.Skill.TargetType == 1 && ski.Skill.HitType == 1)
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
                        Session.CurrentMap?.Broadcast($"ct 1 {Session.Character.CharacterId} 1 {Session.Character.CharacterId} {ski.Skill.CastAnimation} {skillinfo?.Skill.CastEffect ?? ski.Skill.CastEffect} {ski.Skill.SkillVNum}");

                        // Generate scp
                        ski.LastUse = DateTime.Now;
                        if (ski.Skill.CastEffect != 0)
                        {
                            Thread.Sleep(ski.Skill.CastTime * 100);
                        }
                        if (Session.HasCurrentMap)
                        {
                            // ReSharper disable once PossibleNullReferenceException HasCurrentMap is nullcheck
                            Session.CurrentMap.Broadcast($"su 1 {Session.Character.CharacterId} 1 {Session.Character.CharacterId} {ski.Skill.SkillVNum} {ski.Skill.Cooldown} {ski.Skill.AttackAnimation} {skillinfo?.Skill.Effect ?? ski.Skill.Effect} {Session.Character.MapX} {Session.Character.MapY} 1 {((int)((double)Session.Character.Hp / Session.Character.HPLoad()) * 100)} 0 -2 {ski.Skill.SkillType - 1}");
                            if (ski.Skill.TargetRange != 0 && Session.HasCurrentMap)
                            {
                                foreach (
                                    MapMonster mon in
                                    Session.CurrentMap.GetListMonsterInRange(Session.Character.MapX,
                                        Session.Character.MapY, ski.Skill.TargetRange).Where(s => s.CurrentHp > 0))
                                {
                                    mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(
                                        TargetHitType.AOETargetHit, Session, ski.Skill,
                                        skillinfo?.Skill.Effect ?? ski.Skill.Effect));
                                }
                            }
                        }
                    }
                    else if (ski.Skill.TargetType == 0 && Session.HasCurrentMap) // monster target
                    {
                        MapMonster monsterToAttack = Session.CurrentMap.GetMonster(targetId);
                        if (monsterToAttack != null && Session.Character.Mp >= ski.Skill.MpCost)
                        {
                            if (Map.GetDistance(new MapCell { X = Session.Character.MapX, Y = Session.Character.MapY },
                                                new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }) <= (ski.Skill.Range) + monsterToAttack.Monster.BasicArea)
                            {
                                Session.Character.LastSkillUse = DateTime.Now;
                                ski.LastUse = DateTime.Now;
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

                                Session.CurrentMap?.Broadcast($"ct 1 {Session.Character.CharacterId} 3 {monsterToAttack.MapMonsterId} {ski.Skill.CastAnimation} {characterSkillInfo?.Skill.CastEffect ?? ski.Skill.CastEffect} {ski.Skill.SkillVNum}");
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
                                // check if we will hit mutltiple targets
                                if (ski.Skill.TargetRange != 0)
                                {
                                    ComboDTO skillCombo = ski.Skill.Combos.FirstOrDefault(s => ski.Hit == s.Hit);
                                    if (skillCombo != null)
                                    {
                                        if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                        {
                                            ski.Hit = 0;
                                        }
                                        IEnumerable<MapMonster> monstersInAOERange = Session.CurrentMap?.GetListMonsterInRange(monsterToAttack.MapX, monsterToAttack.MapY, ski.Skill.TargetRange).ToList();
                                        if (monstersInAOERange != null)
                                        {
                                            foreach (MapMonster mon in monstersInAOERange)
                                            {
                                                mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.SingleTargetHitCombo, Session, ski.Skill
                                                    , skillCombo: skillCombo));
                                            }
                                        }
                                        else
                                        {
                                            Session.SendPacket($"cancel 2 {targetId}");
                                        }
                                        if (!monsterToAttack.IsAlive)
                                        {
                                            Session.SendPacket($"cancel 2 {targetId}");
                                        }
                                    }
                                    else
                                    {
                                        IEnumerable<MapMonster> monstersInAOERange = Session.CurrentMap?.GetListMonsterInRange(monsterToAttack.MapX, monsterToAttack.MapY, ski.Skill.TargetRange).ToList();

                                        //hit the targetted monster
                                        monsterToAttack.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.SingleAOETargetHit, Session, ski.Skill
                                                    , characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect, showTargetAnimation: true));

                                        //hit all other monsters
                                        if (monstersInAOERange != null)
                                        {
                                            foreach (MapMonster mon in monstersInAOERange.Where(m => m.MapMonsterId != monsterToAttack.MapMonsterId)) //exclude targetted monster
                                            {
                                                mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.SingleAOETargetHit, Session, ski.Skill
                                                    , characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect));
                                            }
                                        }
                                        else
                                        {
                                            Session.SendPacket($"cancel 2 {targetId}");
                                        }
                                        if (!monsterToAttack.IsAlive)
                                        {
                                            Session.SendPacket($"cancel 2 {targetId}");
                                        }
                                    }
                                }
                                else
                                {
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
                                }
                            }
                            else
                            {
                                Session.SendPacket($"cancel 2 {targetId}");
                            }
                        }
                        else
                        {
                            Session.SendPacket($"cancel 2 {targetId}");
                        }
                    }
                    else
                    {
                        Session.SendPacket($"cancel 2 {targetId}");
                    }
                    Session.SendPacketAfterWait($"sr {castingId}", ski.Skill.Cooldown * 100);
                }
                else
                {
                    Session.SendPacket($"cancel 2 {targetId}");
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                }
            }
            else
            {
                Session.SendPacket($"cancel 2 {targetId}");
            }
        }

        /// <summary>
        /// u_s
        /// </summary>
        /// <param name="useSkillPacket"></param>
        public void UseSkill(UseSkillPacket useSkillPacket)
        {
            if (Session.Character.CanFight && useSkillPacket != null)
            {
                PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
                if (Session.Character.IsMuted() && penalty != null)
                {
                    if (Session.Character.Gender == GenderType.Female)
                    {
                        Session.SendPacket("cancel 0 0");
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                    }
                    else
                    {
                        Session.SendPacket("cancel 0 0");
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));

                    }
                    return;
                }

                Logger.Debug(useSkillPacket.ToString(), Session.SessionId);

                if (useSkillPacket.MapX.HasValue && useSkillPacket.MapY.HasValue)
                {
                    Session.Character.MapX = useSkillPacket.MapX.Value;
                    Session.Character.MapY = useSkillPacket.MapY.Value;
                }
                if (Session.Character.IsSitting)
                {
                    Session.Character.Rest();
                }
                if (Session.Character.IsVehicled || Session.Character.InvisibleGm)
                {
                    Session.SendPacket("cancel 0 0");
                    return;
                }
                switch (useSkillPacket.UserType)
                {
                    case UserType.Monster:
                        if (Session.Character.Hp > 0)
                        {
                            TargetHit(useSkillPacket.CastId, useSkillPacket.MapMonsterId);
                        }
                        break;

                    case UserType.Player:
                        if (Session.Character.Hp > 0 && useSkillPacket.MapMonsterId == Session.Character.CharacterId)
                        {
                            TargetHit(useSkillPacket.CastId, useSkillPacket.MapMonsterId);
                        }
                        else
                        {
                            Session.SendPacket("cancel 2 0");
                        }
                        break;

                    default:
                        Session.SendPacket("cancel 2 0");
                        return;
                }
            }
        }

        /// <summary>
        /// u_as
        /// </summary>
        /// <param name="useAOESkillPacket"></param>
        public void UseZonesSkill(UseAOESkillPacket useAOESkillPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.SendPacket("cancel 0 0");
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
                else
                {
                    Session.SendPacket("cancel 0 0");
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
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
                Logger.Debug(useAOESkillPacket.ToString(), Session.SessionId);
                if (Session.Character.CanFight)
                {
                    if (Session.Character.Hp > 0)
                    {
                        ZoneHit(useAOESkillPacket.CastId, useAOESkillPacket.MapX, useAOESkillPacket.MapY);
                    }
                }
            }
        }

        private void ZoneHit(int Castingid, short x, short y)
        {
            List<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp.GetAllItems() : Session.Character.Skills.GetAllItems();
            CharacterSkill characterSkill = skills.FirstOrDefault(s => s.Skill.CastId == Castingid);
            if (!Session.Character.WeaponLoaded(characterSkill) || !Session.HasCurrentMap)
            {
                Session.SendPacket("cancel 2 0");
                return;
            }

            if (characterSkill != null && characterSkill.CanBeUsed())
            {
                if (Session.Character.Mp >= characterSkill.Skill.MpCost)
                {
                    Session.CurrentMap?.Broadcast($"ct_n 1 {Session.Character.CharacterId} 3 -1 {characterSkill.Skill.CastAnimation} {characterSkill.Skill.CastEffect} {characterSkill.Skill.SkillVNum}");
                    characterSkill.LastUse = DateTime.Now;
                    if (!Session.Character.HasGodMode)
                    {
                        Session.Character.Mp -= characterSkill.Skill.MpCost;
                    }
                    Session.SendPacket(Session.Character.GenerateStat());
                    characterSkill.LastUse = DateTime.Now;
                    Observable.Timer(TimeSpan.FromMilliseconds(characterSkill.Skill.CastTime * 100))
                    .Subscribe(
                    o =>
                    {
                        Session.Character.LastSkillUse = DateTime.Now;

                        Session.CurrentMap?.Broadcast($"bs 1 {Session.Character.CharacterId} {x} {y} {characterSkill.Skill.SkillVNum} {characterSkill.Skill.Cooldown} {characterSkill.Skill.AttackAnimation} {characterSkill.Skill.Effect} 0 0 1 1 0 0 0");

                        IEnumerable<MapMonster> monstersInRange = Session.CurrentMap?.GetListMonsterInRange(x, y, characterSkill.Skill.TargetRange).ToList();
                        if (monstersInRange != null)
                        {
                            foreach (MapMonster mon in monstersInRange.Where(s => s.CurrentHp > 0))
                            {
                                mon.HitQueue.Enqueue(new GameObject.Networking.HitRequest(TargetHitType.ZoneHit, Session, characterSkill.Skill, x, y));
                            }
                        }
                    });

                    Observable.Timer(TimeSpan.FromMilliseconds(characterSkill.Skill.CastTime * 100))
                    .Subscribe(
                    o =>
                    {
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