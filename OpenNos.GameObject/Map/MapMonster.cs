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

using EpPathFinding;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapMonster : MapMonsterDTO
    {
        #region Members

        private int _movetime;
        private Random _random;

        #endregion

        #region Instantiation

        public MapMonster()
        {
            HitQueue = new ConcurrentQueue<HitRequest>();
        }

        #endregion

        #region Properties

        public int CurrentHp { get; set; }

        public int CurrentMp { get; set; }

        public IDictionary<long, long> DamageList { get; set; }

        public DateTime Death { get; set; }

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public ConcurrentQueue<HitRequest> HitQueue { get; set; }

        public bool InWaiting { get; set; }

        public bool IsAlive { get; set; }

        public DateTime LastEffect { get; set; }

        public DateTime LastMove { get; set; }

        public Map Map { get; set; }

        public NpcMonster Monster { get; set; }

        public List<GridPos> Path { get; set; }

        public bool? ShouldRespawn { get; set; }

        public List<NpcMonsterSkill> Skills { get; set; }

        public long Target { get; set; }

        #endregion

        #region Methods

        public string GenerateEff(int effect)
        {
            return $"eff 3 {MapMonsterId} {effect}";
        }

        public string GenerateIn3()
        {
            if (IsAlive && !IsDisabled)
            {
                return $"in 3 {MonsterVNum} {MapMonsterId} {MapX} {MapY} {Position} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {(int)(((float)CurrentMp / (float)Monster.MaxMP) * 100)} 0 0 0 -1 {(byte)InRespawnType.TeleportationEffect} 0 -1 - 0 -1 0 0 0 0 0 0 0 0";
            }
            else
            {
                return String.Empty;
            }
        }

        public string GenerateMv3()
        {
            return $"mv 3 {MapMonsterId} {MapX} {MapY} {Monster.Speed}";
        }

        public void Initialize(Map currentMap)
        {
            Map = currentMap;
            Initialize();
        }

        public override void Initialize()
        {
            FirstX = MapX;
            FirstY = MapY;
            LastEffect = LastMove = DateTime.Now;
            Target = -1;
            Path = new List<GridPos>();
            IsAlive = true;
            ShouldRespawn = (ShouldRespawn.HasValue ? ShouldRespawn.Value : true);
            Monster = ServerManager.GetNpc(MonsterVNum);
            CurrentHp = Monster.MaxHP;
            CurrentMp = Monster.MaxMP;
            Skills = Monster.Skills.ToList();
            DamageList = new Dictionary<long, long>();
            _random = new Random(MapMonsterId);
            _movetime = _random.Next(400, 3200);
        }

        /// <summary>
        /// Check if the Monster is in the given Range.
        /// </summary>
        /// <param name="mapX">The X coordinate on the Map of the object to check.</param>
        /// <param name="mapY">The Y coordinate on the Map of the object to check.</param>
        /// <param name="distance">The maximum distance of the object to check.</param>
        /// <returns>True if the Monster is in range, False if not.</returns>
        public bool IsInRange(short mapX, short mapY, byte distance)
        {
            return Map.GetDistance(
             new MapCell()
             {
                 X = mapX,
                 Y = mapY
             }, new MapCell()
             {
                 X = MapX,
                 Y = MapY
             }) <= distance + 1;
        }

        /// <summary>
        /// Generate the Monster -&gt; Character Damage
        /// </summary>
        /// <param name="targetCharacter"></param>
        /// <param name="skill"></param>
        /// <param name="hitmode"></param>
        /// <returns></returns>
        internal int GenerateDamage(Character targetCharacter, Skill skill, ref int hitmode)
        {
            //Warning: This code contains a huge amount of copypasta!

            #region Definitions

            if (targetCharacter == null)
            {
                return 0;
            }

            short distanceX = (short)(MapX - targetCharacter.MapX);
            short distanceY = (short)(MapY - targetCharacter.MapY);
            Random random = new Random();
            int generated = random.Next(0, 100);

            int playerDefense = 0;
            byte playerDefenseUpgrade = 0;
            int playerDodge = 0;

            WearableInstance playerArmor = targetCharacter.Inventory.LoadBySlotAndType<WearableInstance>((byte)Domain.EquipmentType.Armor, Domain.InventoryType.Wear);
            if (playerArmor != null)
            {
                playerDefenseUpgrade = playerArmor.Upgrade;
            }

            short mainUpgrade = Monster.AttackUpgrade;
            int mainCritChance = Monster.CriticalChance;
            int mainCritHit = Monster.CriticalRate;
            int mainMinDmg = Monster.DamageMinimum;
            int mainMaxDmg = Monster.DamageMaximum;
            int mainHitRate = Monster.Concentrate; //probably missnamed, check later
            if (mainMaxDmg == 0)
            {
                mainMinDmg = Monster.Level * 8;
                mainMaxDmg = Monster.Level * 12;
                mainCritChance = 10;
                mainCritHit = 120;
                mainHitRate = (Monster.Level / 2) + 1;
            }

            #endregion

            #region Get Player defense

            switch (Monster.AttackClass)
            {
                case 0:
                    playerDefense = targetCharacter.Defence;
                    playerDodge = targetCharacter.DefenceRate;
                    break;

                case 1:
                    playerDefense = targetCharacter.DistanceDefence;
                    playerDodge = targetCharacter.DistanceDefenceRate;
                    break;

                case 2:
                    playerDefense = targetCharacter.MagicalDefence;
                    break;

                default:
                    throw new Exception(String.Format("Monster.AttackClass {0} not implemented", Monster.AttackClass));
            }

            #endregion

            #region Basic Damage Data Calculation

#warning TODO: Implement BCard damage boosts, see Issue

            mainUpgrade -= playerDefenseUpgrade;
            if (mainUpgrade < -10)
            {
                mainUpgrade = -10;
            }
            else if (mainUpgrade > 10)
            {
                mainUpgrade = 10;
            }

            #endregion

            #region Detailed Calculation

            #region Dodge

            double multiplier = playerDodge / mainHitRate;
            if (multiplier > 5)
            {
                multiplier = 5;
            }
            double chance = -0.25 * Math.Pow(multiplier, 3) - 0.57 * Math.Pow(multiplier, 2) + 25.3 * multiplier - 1.41;
            if (chance <= 1)
            {
                chance = 1;
            }
            if (Monster.AttackClass == 0 || Monster.AttackClass == 1)
            {
                if (random.Next(0, 100) <= chance)
                {
                    hitmode = 1;
                    return 0;
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = new Random().Next(mainMinDmg, mainMaxDmg + 1);
            int elementalDamage = 0; // placeholder for BCard etc...

            if (skill != null)
            {
                baseDamage += (skill.Damage / 4);
                elementalDamage += (skill.ElementalDamage / 4);
            }

            switch (mainUpgrade)
            {
                case -10:
                    playerDefense += (int)(playerDefense * 2);
                    break;

                case -9:
                    playerDefense += (int)(playerDefense * 1.2);
                    break;

                case -8:
                    playerDefense += (int)(playerDefense * 0.9);
                    break;

                case -7:
                    playerDefense += (int)(playerDefense * 0.65);
                    break;

                case -6:
                    playerDefense += (int)(playerDefense * 0.54);
                    break;

                case -5:
                    playerDefense += (int)(playerDefense * 0.43);
                    break;

                case -4:
                    playerDefense += (int)(playerDefense * 0.32);
                    break;

                case -3:
                    playerDefense += (int)(playerDefense * 0.22);
                    break;

                case -2:
                    playerDefense += (int)(playerDefense * 0.15);
                    break;

                case -1:
                    playerDefense += (int)(playerDefense * 0.1);
                    break;

                case 0:
                    break;

                case 1:
                    baseDamage += (int)(baseDamage * 0.1);
                    break;

                case 2:
                    baseDamage += (int)(baseDamage * 0.15);
                    break;

                case 3:
                    baseDamage += (int)(baseDamage * 0.22);
                    break;

                case 4:
                    baseDamage += (int)(baseDamage * 0.32);
                    break;

                case 5:
                    baseDamage += (int)(baseDamage * 0.43);
                    break;

                case 6:
                    baseDamage += (int)(baseDamage * 0.54);
                    break;

                case 7:
                    baseDamage += (int)(baseDamage * 0.65);
                    break;

                case 8:
                    baseDamage += (int)(baseDamage * 0.9);
                    break;

                case 9:
                    baseDamage += (int)(baseDamage * 1.2);
                    break;

                case 10:
                    baseDamage += (int)(baseDamage * 2);
                    break;
            }

            #endregion

            #region Elementary Damage

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            int playerRessistance = 0;
            switch (Monster.Element)
            {
                case 0:
                    break;

                case 1:
                    playerRessistance = targetCharacter.FireResistance;
                    switch (targetCharacter.Element)
                    {
                        case 0:
                            elementalBoost = 1.3; // Damage vs no element
                            break;

                        case 1:
                            elementalBoost = 1; // Damage vs fire
                            break;

                        case 2:
                            elementalBoost = 2; // Damage vs water
                            break;

                        case 3:
                            elementalBoost = 1; // Damage vs light
                            break;

                        case 4:
                            elementalBoost = 1.5; // Damage vs darkness
                            break;
                    }
                    break;

                case 2:
                    playerRessistance = targetCharacter.WaterResistance;
                    switch (targetCharacter.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 2;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1.5;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }
                    break;

                case 3:
                    playerRessistance = targetCharacter.LightResistance;
                    switch (targetCharacter.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1.5;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1;
                            break;

                        case 4:
                            elementalBoost = 3;
                            break;
                    }
                    break;

                case 4:
                    playerRessistance = targetCharacter.DarkResistance;
                    switch (targetCharacter.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1;
                            break;

                        case 2:
                            elementalBoost = 1.5;
                            break;

                        case 3:
                            elementalBoost = 3;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }
                    break;
            }

            #endregion;

            if (Monster.Element == 0)
            {
                if (elementalBoost == 0.5)
                {
                    elementalBoost = 0;
                }
                else if (elementalBoost == 1)
                {
                    elementalBoost = 0.05;
                }
                else if (elementalBoost == 1.3)
                {
                    elementalBoost = 0;
                }
                else if (elementalBoost == 1.5)
                {
                    elementalBoost = 0.15;
                }
                else if (elementalBoost == 2)
                {
                    elementalBoost = 0.2;
                }
                else if (elementalBoost == 3)
                {
                    elementalBoost = 0.2;
                }
            }

            elementalDamage = (int)((elementalDamage + ((elementalDamage + baseDamage) * ((Monster.ElementRate) / 100D))) * elementalBoost);
            elementalDamage = elementalDamage / 100 * (100 - playerRessistance);

            #endregion

            #region Critical Damage

            if (random.Next(100) <= mainCritChance)
            {
                if (Monster.AttackClass == 2)
                {
                }
                else
                {
                    baseDamage += (int)(baseDamage * ((mainCritHit / 100D)));
                    hitmode = 3;
                }
            }

            #endregion

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage - playerDefense;
            if (totalDamage < 5)
            {
                totalDamage = random.Next(1, 6);
            }

            #endregion

            #endregion

            #region Minimum damage

            if (Monster.Level < 45)
            {
                //no minimum damage
            }
            else if (Monster.Level < 55)
            {
                totalDamage += Monster.Level;
            }
            else if (Monster.Level < 60)
            {
                totalDamage += Monster.Level * 2;
            }
            else if (Monster.Level < 65)
            {
                totalDamage += Monster.Level * 3;
            }
            else if (Monster.Level < 70)
            {
                totalDamage += Monster.Level * 4;
            }
            else
            {
                totalDamage += Monster.Level * 5;
            }

            #endregion

            return totalDamage;
        }

        /// <summary>
        /// Handle any kind of Monster interaction
        /// </summary>
        internal async void MonsterLife()
        {
            // handle hit queue
            HitRequest hitRequest = null;
            while (HitQueue.TryDequeue(out hitRequest))
            {
                if (IsAlive)
                {
                    int hitmode = 0;

                    // calculate damage
                    int damage = hitRequest.Session.Character.GenerateDamage(this, hitRequest.Skill, ref hitmode);

                    switch (hitRequest.TargetHitType)
                    {
                        case Domain.TargetHitType.SingleTargetHit:
                            {
                                // Target Hit
                                Map?.Broadcast($"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {(hitRequest.SkillEffect)} {hitRequest.Session.Character.MapX} {hitRequest.Session.Character.MapY} {(IsAlive ? 1 : 0)} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {damage} {hitmode} {hitRequest.Skill.SkillType - 1}");
                                break;
                            }
                        case Domain.TargetHitType.SingleTargetHitCombo:
                            {
                                // Taget Hit Combo
                                Map?.Broadcast($"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.SkillCombo.Animation} {hitRequest.SkillCombo.Effect} {hitRequest.Session.Character.MapX} {hitRequest.Session.Character.MapY} {(IsAlive ? 1 : 0)} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {damage} {hitmode} {hitRequest.Skill.SkillType - 1}");
                                break;
                            }
                        case Domain.TargetHitType.SingleAOETargetHit:
                            {
                                // Target Hit Single AOE
                                Map?.Broadcast($"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {(hitRequest.SkillEffect)} {hitRequest.Session.Character.MapX} {hitRequest.Session.Character.MapY} {(IsAlive ? 1 : 0)} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {damage} 5 {hitRequest.Skill.SkillType - 1}");
                                break;
                            }
                        case Domain.TargetHitType.AOETargetHit:
                            {
                                // Target Hit AOE
                                Map?.Broadcast($"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {(hitRequest.SkillEffect)} {hitRequest.Session.Character.MapX} {hitRequest.Session.Character.MapY} {(IsAlive ? 1 : 0)} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {damage} 5 {hitRequest.Skill.SkillType - 1}");
                                break;
                            }
                        case Domain.TargetHitType.ZoneHit:
                            {
                                // Zone HIT
                                Map?.Broadcast($"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} {hitRequest.MapX} {hitRequest.MapY} {(IsAlive ? 1 : 0)} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {damage} 5 {hitRequest.Skill.SkillType - 1}");
                                break;
                            }
                        case Domain.TargetHitType.SpecialZoneHit:
                            {
                                // Special Zone hit
                                Map?.Broadcast($"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} {hitRequest.Session.Character.MapX} {hitRequest.Session.Character.MapY} {(IsAlive ? 1 : 0)} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {damage} 0 {hitRequest.Skill.SkillType - 1}");
                                break;
                            }
                    }

                    // generate the kill bonus
                    hitRequest.Session.Character.GenerateKillBonus(this);
                }
                else
                {
                    // monster already has been killed, send cancel
                    hitRequest.Session.SendPacket($"cancel 2 {MapMonsterId}");
                }
            }

            // Respawn
            if (!IsAlive && ShouldRespawn.Value)
            {
                double timeDeath = (DateTime.Now - Death).TotalSeconds;
                if (timeDeath >= Monster.RespawnTime / 10)
                {
                    Respawn();
                }
                return;
            }
            else if (Target == -1) // normal movement
            {
                Move();
                return;
            }
            else // target following
            {
                ClientSession targetSession = Map.GetSessionByCharacterId(Target);

                // remove target in some situations
                if (targetSession == null || targetSession.Character.Invisible || targetSession.Character.Hp <= 0)
                {
                    RemoveTarget();
                    return;
                }

                NpcMonsterSkill npcMonsterSkill = null;
                if (_random.Next(10) > 8)
                {
                    npcMonsterSkill = Skills.Where(s => (DateTime.Now - s.LastSkillUse).TotalMilliseconds >= 100 * s.Skill.Cooldown).OrderBy(rnd => _random.Next()).FirstOrDefault();
                }

                // check if target is in range
                if (targetSession != null && !targetSession.Character.InvisibleGm && !targetSession.Character.Invisible && targetSession.Character.Hp > 0
                    && ((npcMonsterSkill != null && CurrentMp - npcMonsterSkill.Skill.MpCost >= 0 &&
                           Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY },
                                           new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) < npcMonsterSkill.Skill.Range)
                                           || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY },
                                                               new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) <= Monster.BasicRange)))
                {
                    TargetHit(targetSession, npcMonsterSkill);
                }
                else
                {
                    FollowTarget(targetSession);
                }
            }
        }

        /// <summary>
        /// Remove the current Target from Monster.
        /// </summary>
        internal void RemoveTarget()
        {
            Path = Map.StraightPath(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = FirstX, y = FirstY });
            if (!Path.Any())
            {
                Path = Map.JPSPlus(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = FirstX, y = FirstY });
            }
            Target = -1;
        }

        /// <summary>
        /// Follow the Monsters target to it's position.
        /// </summary>
        /// <param name="targetSession">The TargetSession to follow</param>
        private void FollowTarget(ClientSession targetSession)
        {
            int distance = 0;

            if (targetSession != null)
            {
                distance = Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY });
            }
            if (IsMoving)
            {
                short maxDistance = 22;
                if (Path.Count() == 0 && targetSession != null && distance > 1 && distance < maxDistance)
                {
                    short xoffset = (short)_random.Next(-1, 1);
                    short yoffset = (short)_random.Next(-1, 1);

                    Path = Map.StraightPath(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = (short)(targetSession.Character.MapX + xoffset), y = (short)(targetSession.Character.MapY + yoffset) });
                    if (!Path.Any())
                    {
                        Path = Map.JPSPlus(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = (short)(targetSession.Character.MapX + xoffset), y = (short)(targetSession.Character.MapY + yoffset) });
                    }
                }
                if (DateTime.Now > LastMove && Monster.Speed > 0 && Path.Any())
                {
                    short mapX;
                    short mapY;
                    int maxindex = Path.Count > Monster.Speed / 2 ? Monster.Speed / 2 : Path.Count;
                    mapX = (short)Path.ElementAt(maxindex - 1).x;
                    mapY = (short)Path.ElementAt(maxindex - 1).y;
                    double waitingtime = (double)(Map.GetDistance(new MapCell() { X = mapX, Y = mapY, MapId = MapId }, new MapCell() { X = MapX, Y = MapY, MapId = MapId })) / (double)(Monster.Speed);
                    Map.Broadcast(new BroadcastPacket(null, $"mv 3 {this.MapMonsterId} {mapX} {mapY} {Monster.Speed}", ReceiverType.AllInRange, xCoordinate: mapX, yCoordinate: mapY));
                    LastMove = DateTime.Now.AddSeconds((waitingtime > 1 ? 1 : waitingtime));
                    Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay((int)((waitingtime > 1 ? 1 : waitingtime) * 1000));
                        this.MapX = mapX;
                        this.MapY = mapY;
                    });

                    for (int j = maxindex; j > 0; j--)
                    {
                        Path.RemoveAt(0);
                    }
                }

                if (Path.Count() == 0 && (DateTime.Now - LastEffect).Seconds > 20 && (targetSession == null || MapId != targetSession.Character.MapId || distance > maxDistance))
                {
                    RemoveTarget();
                }
            }
        }

        private void Move()
        {
            // Normal Move Mode
            if (!IsAlive)
            {
                return;
            }

            if (IsMoving && Monster.Speed > 0)
            {
                double time = (DateTime.Now - LastMove).TotalMilliseconds;

                if (Path.Where(s => s != null).Any())
                {
                    int timetowalk = 1000 / (2 * Monster.Speed);
                    if (time > timetowalk)
                    {
                        int mapX = Path.ElementAt(0).x;
                        int mapY = Path.ElementAt(0).y;
                        Path.RemoveAt(0);

                        Task.Factory.StartNew(async () =>
                        {
                            await Task.Delay(timetowalk);
                            MapX = (short)mapX;
                            MapY = (short)mapY;
                        });
                        LastMove = DateTime.Now;
                        Map.Broadcast(new BroadcastPacket(null, GenerateMv3(), ReceiverType.AllInRange, xCoordinate: mapX, yCoordinate: mapY));
                        return;
                    }
                }
                else if (time > _movetime)
                {
                    _movetime = _random.Next(600, 3000);
                    byte point = (byte)_random.Next(2, 4);
                    byte fpoint = (byte)_random.Next(0, 2);

                    byte xpoint = (byte)_random.Next(fpoint, point);
                    byte ypoint = (byte)(point - xpoint);

                    short mapX = FirstX;
                    short mapY = FirstY;
                    if (Map?.GetFreePosition(ref mapX, ref mapY, xpoint, ypoint) ?? false)
                    {
                        Task.Factory.StartNew(async () =>
                        {
                            await Task.Delay(1000 * (xpoint + ypoint) / (2 * Monster.Speed));
                            this.MapX = mapX;
                            this.MapY = mapY;
                        });
                        LastMove = DateTime.Now.AddSeconds((xpoint + ypoint) / (2 * Monster.Speed));
                        Map.Broadcast(new BroadcastPacket(null, GenerateMv3(), ReceiverType.AllInRange, xCoordinate: mapX, yCoordinate: mapY));
                    }
                }
            }

            if (Monster.IsHostile)
            {
                Character character = ServerManager.Instance.Sessions.FirstOrDefault(s => s != null && s.Character != null && s.Character.Hp > 0 && !s.Character.InvisibleGm && !s.Character.Invisible && s.Character.MapId == MapId && Map.GetDistance(new MapCell() { X = MapX, Y = MapY }, new MapCell() { X = s.Character.MapX, Y = s.Character.MapY }) < Monster.NoticeRange)?.Character;
                if (character != null)
                {
                    Target = character.CharacterId;
                    if (!Monster.NoAggresiveIcon)
                    {
                        character.Session.SendPacket(GenerateEff(5000));
                    }
                }
            }
        }

        private void Respawn()
        {
            DamageList = new Dictionary<long, long>();
            IsAlive = true;
            Target = -1;
            CurrentHp = Monster.MaxHP;
            CurrentMp = Monster.MaxMP;
            MapX = FirstX;
            MapY = FirstY;
            Path = new List<GridPos>();
            Map.Broadcast(GenerateIn3());
        }

        /// <summary>
        /// Hit the Target Character.
        /// </summary>
        /// <param name="targetSession"></param>
        /// <param name="npcMonsterSkill"></param>
        private void TargetHit(ClientSession targetSession, NpcMonsterSkill npcMonsterSkill)
        {
            if (((DateTime.Now - LastEffect).TotalMilliseconds >= 1000 + Monster.BasicCooldown * 200 && !Skills.Any()) || npcMonsterSkill != null)
            {
                int damage = 0;
                int hitmode = 0;

                if (npcMonsterSkill != null)
                {
                    damage = GenerateDamage(targetSession.Character, npcMonsterSkill.Skill, ref hitmode);
                }
                else
                {
                    damage = GenerateDamage(targetSession.Character, null, ref hitmode);
                }

                if (npcMonsterSkill != null)
                {
                    npcMonsterSkill.LastSkillUse = DateTime.Now;
                    CurrentMp -= npcMonsterSkill.Skill.MpCost;
                    Map.Broadcast($"ct 3 {MapMonsterId} 1 {Target} {npcMonsterSkill.Skill.CastAnimation} {npcMonsterSkill.Skill.CastEffect} {npcMonsterSkill.Skill.SkillVNum}");
                }
                LastMove = DateTime.Now;

                // deal 0 damage to GM with GodMode
                if (targetSession.Character.HasGodMode)
                {
                    damage = 0;
                }
                if (targetSession.Character.IsSitting)
                {
                    targetSession.Character.IsSitting = false;
                    Map.Broadcast(targetSession.Character.GenerateRest());
                    Thread.Sleep(500);
                }
                if (npcMonsterSkill != null && npcMonsterSkill.Skill.CastEffect != 0)
                {
                    Map.Broadcast(GenerateEff(npcMonsterSkill.Skill.CastEffect), MapX, MapY);
                    Thread.Sleep(npcMonsterSkill.Skill.CastTime * 100);
                }
                Path = new List<GridPos>();
                targetSession.Character.LastDefence = DateTime.Now;
                targetSession.Character.GetDamage(damage);

                Map.Broadcast(null, ServerManager.Instance.GetUserMethod<string>(Target, "GenerateStat"), ReceiverType.OnlySomeone, "", Target);

                if (npcMonsterSkill != null)
                {
                    Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} {npcMonsterSkill.SkillVNum} {npcMonsterSkill.Skill.Cooldown} {npcMonsterSkill.Skill.AttackAnimation} {npcMonsterSkill.Skill.Effect} {this.MapX} {this.MapY} {(targetSession.Character.Hp > 0 ? 1 : 0)} { (int)(targetSession.Character.Hp / targetSession.Character.HPLoad() * 100) } {damage} {hitmode} 0");
                }
                else
                {
                    Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(targetSession.Character.Hp > 0 ? 1 : 0)} { (int)(targetSession.Character.Hp / targetSession.Character.HPLoad() * 100) } {damage} {hitmode} 0");
                }

                LastEffect = DateTime.Now;
                if (targetSession.Character.Hp <= 0)
                {
                    Thread.Sleep(1000);
                    ServerManager.Instance.AskRevive(targetSession.Character.CharacterId);
                    RemoveTarget();
                }
                if (npcMonsterSkill != null && (npcMonsterSkill.Skill.Range > 0 || npcMonsterSkill.Skill.TargetRange > 0))
                {
                    foreach (Character characterInRange in Map.GetCharactersInRange(npcMonsterSkill.Skill.TargetRange == 0 ? this.MapX : targetSession.Character.MapX, npcMonsterSkill.Skill.TargetRange == 0 ? this.MapY : targetSession.Character.MapY, npcMonsterSkill.Skill.TargetRange).Where(s => s.CharacterId != Target && s.Hp > 0))
                    {
                        if (characterInRange.IsSitting)
                        {
                            characterInRange.IsSitting = false;
                            Map.Broadcast(characterInRange.GenerateRest());
                            Thread.Sleep(500);
                        }
                        if (characterInRange.HasGodMode)
                        {
                            damage = 0;
                            hitmode = 1;
                        }
                        bool AlreadyDead2 = characterInRange.Hp <= 0;
                        characterInRange.GetDamage(damage);
                        characterInRange.LastDefence = DateTime.Now;
                        Map.Broadcast(null, characterInRange.GenerateStat(), ReceiverType.OnlySomeone, "", characterInRange.CharacterId);
                        Map.Broadcast($"su 3 {MapMonsterId} 1 {characterInRange.CharacterId} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(characterInRange.Hp > 0 ? 1 : 0)} { (int)(characterInRange.Hp / characterInRange.HPLoad() * 100) } {damage} {hitmode} 0");
                        if (characterInRange.Hp <= 0 && !AlreadyDead2)
                            damage = characterInRange.HasGodMode ? 0 : 100;
                        bool alreadyDead = characterInRange.Hp <= 0;
                        characterInRange.GetDamage(damage);
                        characterInRange.LastDefence = DateTime.Now;
                        characterInRange.Session.SendPacket(characterInRange.GenerateStat());

                        Map.Broadcast($"su 3 {MapMonsterId} 1 {characterInRange.CharacterId} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(characterInRange.Hp > 0 ? 1 : 0)} { (int)(characterInRange.Hp / characterInRange.HPLoad() * 100) } {damage} {hitmode} 0");
                        if (characterInRange.Hp <= 0 && !alreadyDead)
                        {
                            Thread.Sleep(1000);
                            ServerManager.Instance.AskRevive(characterInRange.CharacterId);
                        }
                    }
                }
            }
        }

        #endregion
    }
}