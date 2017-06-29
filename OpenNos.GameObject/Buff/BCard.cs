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

using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class BCard : BCardDTO
    {
        public override void Initialize()
        {

        }

        public void ApplyBCards(Object session)
        {
            switch ((BCardType.CardType)Type)
            {
                case BCardType.CardType.Buff:
                    if (session.GetType() == typeof(Character))
                    {
                        if (ServerManager.Instance.RandomNumber() < FirstData)
                        {
                            (session as Character).AddBuff(new Buff(SecondData, (session as Character).Level));
                        }
                    }
                    else if (session.GetType() == typeof(MapMonster))
                    {

                    }
                    else if (session.GetType() == typeof(MapNpc))
                    {

                    }
                    else if (session.GetType() == typeof(Mate))
                    {

                    }
                    break;

                case BCardType.CardType.Move:
                    if (session.GetType() == typeof(Character))
                    {
                        (session as Character).LastSpeedChange = DateTime.Now;
                        (session as Character).Session.SendPacket((session as Character).GenerateCond());
                    }
                    break;

                case BCardType.CardType.Summons:
                    if (session.GetType() == typeof(Character))
                    {

                    }
                    else if (session.GetType() == typeof(MapMonster))
                    {
                        List<MonsterToSummon> summonParameters = new List<MonsterToSummon>();
                        for (int i = 0; i < FirstData; i++)
                        {
                            short x = (short)(ServerManager.Instance.RandomNumber(-3, 3) + (session as MapMonster).MapX);
                            short y = (short)(ServerManager.Instance.RandomNumber(-3, 3) + (session as MapMonster).MapY);
                            summonParameters.Add(new MonsterToSummon((short)SecondData, new MapCell() { X = x, Y = y }, -1, true));
                        }
                        int rnd = ServerManager.Instance.RandomNumber();
                        if (rnd <= Math.Abs(ThirdData) || ThirdData == 0)
                        {
                            switch (SubType)
                            {
                                case 20:
                                    EventHelper.Instance.RunEvent(new EventContainer((session as MapMonster).MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                                    break;
                                default:
                                    if (!(session as MapMonster).OnDeathEvents.Any(s => s.EventActionType == EventActionType.SPAWNMONSTERS))
                                    {
                                        (session as MapMonster).OnDeathEvents.Add(new EventContainer((session as MapMonster).MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                                    }
                                    break;
                            }
                        }
                    }
                    else if (session.GetType() == typeof(MapNpc))
                    {

                    }
                    else if (session.GetType() == typeof(Mate))
                    {

                    }
                    break;
                case BCardType.CardType.SpecialAttack:
                    break;
                case BCardType.CardType.SpecialDefence:
                    break;
                case BCardType.CardType.AttackPower:
                    break;
                case BCardType.CardType.Target:
                    break;
                case BCardType.CardType.Critical:
                    break;
                case BCardType.CardType.SpecialCritical:
                    break;
                case BCardType.CardType.Element:
                    break;
                case BCardType.CardType.IncreaseDamage:
                    break;
                case BCardType.CardType.Defence:
                    break;
                case BCardType.CardType.DodgeAndDefencePercent:
                    break;
                case BCardType.CardType.Block:
                    break;
                case BCardType.CardType.Absorption:
                    break;
                case BCardType.CardType.ElementResistance:
                    break;
                case BCardType.CardType.EnemyElementResistance:
                    break;
                case BCardType.CardType.Damage:
                    break;
                case BCardType.CardType.GuarantedDodgeRangedAttack:
                    break;
                case BCardType.CardType.Morale:
                    break;
                case BCardType.CardType.Casting:
                    break;
                case BCardType.CardType.Reflection:
                    break;
                case BCardType.CardType.DrainAndSteal:
                    break;
                case BCardType.CardType.HealingBurningAndCasting:
                    break;
                case BCardType.CardType.HPMP:
                    break;
                case BCardType.CardType.SpecialisationBuffResistance:
                    break;
                case BCardType.CardType.SpecialEffects:
                    break;
                case BCardType.CardType.Capture:
                    break;
                case BCardType.CardType.SpecialDamageAndExplosions:
                    break;
                case BCardType.CardType.SpecialEffects2:
                    break;
                case BCardType.CardType.CalculatingLevel:
                    break;
                case BCardType.CardType.Recovery:
                    break;
                case BCardType.CardType.MaxHPMP:
                    break;
                case BCardType.CardType.MultAttack:
                    break;
                case BCardType.CardType.MultDefence:
                    break;
                case BCardType.CardType.TimeCircleSkills:
                    break;
                case BCardType.CardType.RecoveryAndDamagePercent:
                    break;
                case BCardType.CardType.Count:
                    break;
                case BCardType.CardType.NoDefeatAndNoDamage:
                    break;
                case BCardType.CardType.SpecialActions:
                    break;
                case BCardType.CardType.Mode:
                    break;
                case BCardType.CardType.NoCharacteristicValue:
                    break;
                case BCardType.CardType.LightAndShadow:
                    break;
                case BCardType.CardType.Item:
                    break;
                case BCardType.CardType.DebuffResistance:
                    break;
                case BCardType.CardType.SpecialBehaviour:
                    break;
                case BCardType.CardType.Quest:
                    break;
                case BCardType.CardType.SecondSPCard:
                    break;
                case BCardType.CardType.SPCardUpgrade:
                    break;
                case BCardType.CardType.HugeSnowman:
                    break;
                case BCardType.CardType.Drain:
                    break;
                case BCardType.CardType.BossMonstersSkill:
                    break;
                case BCardType.CardType.LordHatus:
                    break;
                case BCardType.CardType.LordCalvinas:
                    break;
                case BCardType.CardType.SESpecialist:
                    break;
                case BCardType.CardType.FourthGlacernonFamilyRaid:
                    break;
                case BCardType.CardType.SummonedMonsterAttack:
                    break;
                case BCardType.CardType.BearSpirit:
                    break;
                case BCardType.CardType.SummonSkill:
                    break;
                case BCardType.CardType.InflictSkill:
                    break;
                case BCardType.CardType.HideBarrelSkill:
                    break;
                case BCardType.CardType.FocusEnemyAttentionSkill:
                    break;
                case BCardType.CardType.TauntSkill:
                    break;
                case BCardType.CardType.FireCannoneerRangeBuff:
                    break;
                case BCardType.CardType.VulcanoElementBuff:
                    break;
                case BCardType.CardType.DamageConvertingSkill:
                    break;
                case BCardType.CardType.MeditationSkill:
                    break;
                case BCardType.CardType.FalconSkill:
                    break;
                case BCardType.CardType.AbsorptionAndPowerSkill:
                    break;
                case BCardType.CardType.LeonaPassiveSkill:
                    break;
                case BCardType.CardType.FearSkill:
                    break;
                case BCardType.CardType.SniperAttack:
                    break;
                case BCardType.CardType.FrozenDebuff:
                    break;
                case BCardType.CardType.JumpBackPush:
                    break;
                case BCardType.CardType.FairyXPIncrease:
                    break;
                case BCardType.CardType.SummonAndRecoverHP:
                    break;
                case BCardType.CardType.TeamArenaBuff:
                    break;
                case BCardType.CardType.ArenaCamera:
                    break;
                case BCardType.CardType.DarkCloneSummon:
                    break;
                case BCardType.CardType.AbsorbedSpirit:
                    break;
                case BCardType.CardType.AngerSkill:
                    break;
                case BCardType.CardType.MeteoriteTeleport:
                    break;
                case BCardType.CardType.StealBuff:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}