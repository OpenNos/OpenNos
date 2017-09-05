/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Domain;

namespace OpenNos.GameObject.Helpers
{
    public static class DamageHelper
    {
        public ushort GenerateDamage(ClientSession session, MapMonster monsterToAttack, Skill skill, ref int hitmode, ref bool onyxEffect)
        {
            #region Definitions

            if (session == null)
            {
                return 0;
            }

            if (monsterToAttack == null)
            {
                return 0;
            }
            if (session.Character.Inventory == null)
            {
                return 0;
            }

            // int miss_chance = 20;
            int monsterDefence = 0;
            int monsterDodge = 0;

            int morale = session.Character.Level + session.Character.GetBuff(BCardType.CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased, false)[0];

            short mainUpgrade = (short)session.Character.GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0];
            int mainCritChance = 0;
            int mainCritHit = 0;
            int mainMinDmg = 0;
            int mainMaxDmg = 0;
            int mainHitRate = morale;

            short secUpgrade = mainUpgrade;
            int secCritChance = 0;
            int secCritHit = 0;
            int secMinDmg = 0;
            int secMaxDmg = 0;
            int secHitRate = morale;

            // int CritChance = 4; int CritHit = 70; int MinDmg = 0; int MaxDmg = 0; int HitRate = 0;
            // sbyte Upgrade = 0;

            #endregion

            #region Get Weapon Stats

            WearableInstance weapon = session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            if (weapon != null)
            {
                mainUpgrade += weapon.Upgrade;
            }

            mainMinDmg += session.Character.MinHit;
            mainMaxDmg += session.Character.MaxHit;
            mainHitRate += session.Character.HitRate;
            mainCritChance += session.Character.HitCriticalRate;
            mainCritHit += session.Character.HitCritical;

            WearableInstance weapon2 = session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
            if (weapon2 != null)
            {
                secUpgrade += weapon2.Upgrade;
            }

            secMinDmg += session.Character.MinDistance;
            secMaxDmg += session.Character.MaxDistance;
            secHitRate += DistanceRate;
            secCritChance += DistanceCriticalRate;
            secCritHit += DistanceCritical;

            #endregion

            #region Switch skill.Type

            int boost, boostpercentage;

            switch (skill.Type)
            {
                case 0:
                    monsterDefence = monsterToAttack.Monster.CloseDefence;
                    monsterDodge = (int)(monsterToAttack.Monster.DefenceDodge * 0.95);
                    if (Class == ClassType.Archer)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                        + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                    boostpercentage = GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0]
                        + GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 1:
                    monsterDefence = monsterToAttack.Monster.DistanceDefence;
                    monsterDodge = (int)(monsterToAttack.Monster.DistanceDefenceDodge * 0.95);
                    if (Class == ClassType.Swordman || Class == ClassType.Adventurer || Class == ClassType.Magician)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                        + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, false)[0];
                    boostpercentage = GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0]
                        + GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 2:
                    monsterDefence = monsterToAttack.Monster.MagicDefence;
                    boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                    boostpercentage = GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0]
                        + GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 3:
                    switch (Class)
                    {
                        case ClassType.Swordman:
                            monsterDefence = monsterToAttack.Monster.CloseDefence;
                            boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                            boostpercentage = GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0]
                                + GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Archer:
                            monsterDefence = monsterToAttack.Monster.DistanceDefence;
                            boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, false)[0];
                            boostpercentage = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                                + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased, false)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Magician:
                            monsterDefence = monsterToAttack.Monster.MagicDefence;
                            boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                            boostpercentage = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                                + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Adventurer:
                            monsterDefence = monsterToAttack.Monster.CloseDefence;
                            boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                            boostpercentage = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                                + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;
                    }
                    break;

                case 5:
                    monsterDefence = monsterToAttack.Monster.CloseDefence;
                    monsterDodge = monsterToAttack.Monster.DefenceDodge;
                    if (Class == ClassType.Archer)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    if (Class == ClassType.Magician)
                    {
                        boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                        boostpercentage = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                        mainMinDmg += boost;
                        mainMaxDmg += boost;
                        mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                        mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    }
                    else
                    {
                        boost = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                        boostpercentage = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] + GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                        mainMinDmg += boost;
                        mainMaxDmg += boost;
                        mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                        mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    }
                    break;
            }

            #endregion

            #region Basic Damage Data Calculation

            mainCritChance += GetBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased, false)[0];
            mainCritChance -= GetBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, false)[0];
            mainCritHit += GetBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased, false)[0];
            mainCritHit -= GetBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, false)[0];

            mainUpgrade -= monsterToAttack.Monster.DefenceUpgrade;
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

            if (Class != ClassType.Magician)
            {
                double multiplier = monsterDodge / (mainHitRate + 1);
                if (multiplier > 5)
                {
                    multiplier = 5;
                }
                double chance = -0.25 * Math.Pow(multiplier, 3) - 0.57 * Math.Pow(multiplier, 2) + 25.3 * multiplier - 1.41;
                if (chance <= 1)
                {
                    chance = 1;
                }
                if (GetBuff(BCardType.CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased, false)[0] != 0)
                {
                    chance = 10;
                }
                if ((skill.Type == 0 || skill.Type == 1) && !HasGodMode)
                {
                    if (ServerManager.Instance.RandomNumber() <= chance)
                    {
                        hitmode = 1;
                        return 0;
                    }
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(mainMinDmg, mainMaxDmg + 1);
            // baseDamage += skill.Damage / 4; it's a bcard need a skillbcardload
            baseDamage += morale - monsterToAttack.Monster.Level; //Morale
            if (Class == ClassType.Adventurer)
            {
                //HACK: Damage is ~10 lower in OpenNos than in official. Fix this...
                baseDamage += 20;
            }
            int elementalDamage = GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0];
            //   elementalDamage += skill.ElementalDamage / 4; it's a bcard need a skillbcardload
            switch (mainUpgrade)
            {
                case -10:
                    monsterDefence += monsterDefence * 2;
                    break;

                case -9:
                    monsterDefence += (int)(monsterDefence * 1.2);
                    break;

                case -8:
                    monsterDefence += (int)(monsterDefence * 0.9);
                    break;

                case -7:
                    monsterDefence += (int)(monsterDefence * 0.65);
                    break;

                case -6:
                    monsterDefence += (int)(monsterDefence * 0.54);
                    break;

                case -5:
                    monsterDefence += (int)(monsterDefence * 0.43);
                    break;

                case -4:
                    monsterDefence += (int)(monsterDefence * 0.32);
                    break;

                case -3:
                    monsterDefence += (int)(monsterDefence * 0.22);
                    break;

                case -2:
                    monsterDefence += (int)(monsterDefence * 0.15);
                    break;

                case -1:
                    monsterDefence += (int)(monsterDefence * 0.1);
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
                    baseDamage += baseDamage * 2;
                    break;

                // sush don't tell ciapa
                default:
                    if (mainUpgrade > 10)
                    {
                        baseDamage += baseDamage * (mainUpgrade / 5);
                    }
                    break;
            }
            if (skill.Type == 1)
            {
                if (Map.GetDistance(new MapCell { X = PositionX, Y = PositionY }, new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }) < 4)
                {
                    baseDamage = (int)(baseDamage * 0.85);
                }
            }

            #endregion

            #region Elementary Damage

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            short monsterResistance = 0;
            switch (Element)
            {
                case 0:
                    break;

                case 1:
                    elementalDamage += GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.FireIncreased, false)[0];
                    monsterResistance = monsterToAttack.Monster.FireResistance;
                    switch (monsterToAttack.Monster.Element)
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
                    elementalDamage += GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.WaterIncreased, false)[0];
                    monsterResistance = monsterToAttack.Monster.WaterResistance;
                    switch (monsterToAttack.Monster.Element)
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
                    elementalDamage += GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.LightIncreased, false)[0];
                    monsterResistance = monsterToAttack.Monster.LightResistance;
                    switch (monsterToAttack.Monster.Element)
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
                    elementalDamage += GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.DarkIncreased, false)[0];
                    monsterResistance = monsterToAttack.Monster.DarkResistance;
                    switch (monsterToAttack.Monster.Element)
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

            if (skill.Element == 0)
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
                    elementalBoost = 0.15;
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
            else if (skill.Element != Element)
            {
                elementalBoost = 0;
            }

            elementalDamage = (int)((elementalDamage + (elementalDamage + baseDamage) * ((ElementRate + ElementRateSP) / 100D)) * (1 + elementalBoost));
            elementalDamage = elementalDamage / 100 * (100 - monsterResistance);

            #endregion

            #region Critical Damage

            baseDamage -= monsterDefence;
            if (GetBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, false)[0] == 0)
            {
                if (ServerManager.Instance.RandomNumber() <= mainCritChance || GetBuff(BCardType.CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] != 0)
                {
                    if (skill.Type == 2)
                    {
                    }
                    else if (skill.Type == 3 && Class != ClassType.Magician)
                    {
                        double multiplier = mainCritHit / 100D;
                        if (multiplier > 3)
                        {
                            multiplier = 3;
                        }
                        baseDamage += (int)(baseDamage * multiplier);
                        hitmode = 3;
                    }
                    else
                    {
                        double multiplier = mainCritHit / 100D;
                        if (multiplier > 3)
                        {
                            multiplier = 3;
                        }
                        baseDamage += (int)(baseDamage * multiplier);
                        hitmode = 3;
                    }
                }
            }

            #endregion

            baseDamage += GetBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksIncreased, false)[0];

            #region Soft-Damage

            int[] soft = GetBuff(BCardType.CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false);
            if (ServerManager.Instance.RandomNumber() < soft[0])
            {
                baseDamage += baseDamage * (int)(soft[1] / 100D);
                Session?.CurrentMapInstance.Broadcast(Session.Character.GenerateEff(15));
            }

            #endregion

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage;
            if (totalDamage < 5)
            {
                totalDamage = ServerManager.Instance.RandomNumber(1, 6);
            }

            #endregion

            #endregion

            if (monsterToAttack.DamageList.ContainsKey(CharacterId))
            {
                monsterToAttack.DamageList[CharacterId] += totalDamage;
            }
            else
            {
                monsterToAttack.DamageList.Add(CharacterId, totalDamage);
            }
            if (monsterToAttack.CurrentHp <= totalDamage)
            {
                monsterToAttack.IsAlive = false;
                monsterToAttack.CurrentHp = 0;
                monsterToAttack.CurrentMp = 0;
                monsterToAttack.Death = DateTime.Now;
                monsterToAttack.LastMove = DateTime.Now;
            }
            else
            {
                monsterToAttack.CurrentHp -= totalDamage;
            }

            while (totalDamage > ushort.MaxValue)
            {
                totalDamage -= ushort.MaxValue;
            }

            // only set the hit delay if we become the monsters target with this hit
            if (monsterToAttack.Target == -1)
            {
                monsterToAttack.LastSkill = DateTime.Now;
            }
            ushort damage = Convert.ToUInt16(totalDamage);

            int nearestDistance = 100;
            foreach (KeyValuePair<long, long> kvp in monsterToAttack.DamageList)
            {
                ClientSession session = monsterToAttack.MapInstance.GetSessionByCharacterId(kvp.Key);
                if (session == null)
                {
                    continue;
                }
                int distance = Map.GetDistance(new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }, new MapCell { X = session.Character.PositionX, Y = session.Character.PositionY });
                if (distance >= nearestDistance)
                {
                    continue;
                }
                nearestDistance = distance;
                monsterToAttack.Target = session.Character.CharacterId;
            }


            #region Onyx Wings

            int[] onyxBuff = GetBuff(BCardType.CardType.StealBuff, (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon, false);
            if (onyxBuff[0] > ServerManager.Instance.RandomNumber())
            {
                onyxEffect = true;
            }

            #endregion

            return damage;
        }
    }
}
*/