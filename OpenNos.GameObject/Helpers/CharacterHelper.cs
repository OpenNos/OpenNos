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

using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class CharacterHelper
    {
        #region Members

        private static int[,] criticalDist = null;
        private static int[,] criticalDistRate = null;
        private static int[,] criticalHit = null;
        private static int[,] criticalHitRate = null;
        private static int[,] distDef = null;
        private static int[,] distDodge = null;
        private static int[,] distRate = null;
        private static double[] firstjobxpData = null;
        private static int[,] hitDef = null;
        private static int[,] hitDodge = null;
        private static int[,] hitRate = null;
        private static int[,] HP = null;
        private static int[] hpHealth = null;
        private static int[] hpHealthStand = null;
        private static int[,] magicalDef = null;
        private static int[,] maxDist = null;
        private static int[,] maxHit = null;
        private static int[,] minDist = null;

        // difference between class
        private static int[,] minHit = null;

        private static int[,] MP = null;
        private static int[] mpHealth = null;
        private static int[] mpHealthStand = null;
        private static double[] secondjobxpData = null;

        // STAT DATA
        private static byte[] speedData = null;

        private static double[] spxpData = null;

        // same for all class
        private static double[] xpData = null;

        #endregion

        #region Instantiation

        private CharacterHelper()
        {
            LoadSpeedData();
            LoadJobXpData();
            LoadSpXpData();
            LoadXpData();
            LoadHpData();
            LoadMpData();
            LoadStats();
            LoadHpHealth();
            LoadMpHealth();
            LoadHpHealthStand();
            LoadMpHealthStand();
        }

        #endregion

        #region Properties

        public static double[] FirstJobXPData
        {
            get
            {
                if (firstjobxpData == null)
                {
                    new CharacterHelper();
                }
                return firstjobxpData;
            }
        }

        public static int[,] HPData
        {
            get
            {
                if (HP == null)
                {
                    new CharacterHelper();
                }
                return HP;
            }
        }

        public static int[] HpHealth
        {
            get
            {
                if (hpHealth == null)
                {
                    new CharacterHelper();
                }
                return hpHealth;
            }
        }

        public static int[] HpHealthStand
        {
            get
            {
                if (hpHealthStand == null)
                {
                    new CharacterHelper();
                }
                return hpHealthStand;
            }
        }

        public static int[,] MPData
        {
            get
            {
                if (MP == null)
                {
                    new CharacterHelper();
                }
                return MP;
            }
        }

        public static int[] MpHealth
        {
            get
            {
                if (mpHealth == null)
                {
                    new CharacterHelper();
                }
                return mpHealth;
            }
        }

        public static int[] MpHealthStand
        {
            get
            {
                if (mpHealthStand == null)
                {
                    new CharacterHelper();
                }
                return mpHealthStand;
            }
        }

        public static double[] SecondJobXPData
        {
            get
            {
                if (secondjobxpData == null)
                {
                    new CharacterHelper();
                }
                return secondjobxpData;
            }
        }

        public static byte[] SpeedData
        {
            get
            {
                if (speedData == null)
                {
                    new CharacterHelper();
                }
                return speedData;
            }
        }

        public static double[] SpXPData
        {
            get
            {
                if (spxpData == null)
                {
                    new CharacterHelper();
                }
                return spxpData;
            }
        }

        public static double[] XPData
        {
            get
            {
                if (xpData == null)
                {
                    new CharacterHelper();
                }
                return xpData;
            }
        }

        #endregion

        #region Methods

        public static int LoadFairyXpData(int i)
        {
            if (i < 40)
            {
                return i * i + 50;
            }
            else
            {
                return i * i * 3 + 50;
            }
        }

        public static int MagicalDefence(byte @class, byte level)
        {
            return magicalDef[@class, level];
        }

        public static int MaxDistance(byte @class, byte level)
        {
            return maxDist[@class, level];
        }

        public static int MaxHit(byte @class, byte level)
        {
            return maxHit[@class, level];
        }

        public static int MinDistance(byte @class, byte level)
        {
            return minDist[@class, level];
        }

        public static int MinHit(byte @class, byte level)
        {
            return minHit[@class, level];
        }

        public static int RarityPoint(short rarity, short lvl)
        {
            Random random = new Random();
            int p;
            switch (rarity)
            {
                default:
                    p = 0;
                    break;

                case -2:
                    p = 0;
                    break;

                case -1:
                    p = 0;
                    break;

                case 0:
                    p = 0;
                    break;

                case 1:
                    p = 1;
                    break;

                case 2:
                    p = 2;
                    break;

                case 3:
                    p = 3;
                    break;

                case 4:
                    p = 4;
                    break;

                case 5:
                    p = 5;
                    break;

                case 6:
                    p = 7;
                    break;

                case 7:
                    p = 10;
                    break;

                case 8:
                    p = random.Next(11, 16);
                    break;
            }
            return p * (lvl / 5) + 1;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "Easier to read")]
        public static int SlPoint(short spPoint, short mode)
        {
            int point = 0;
            switch (mode)
            {
                case 0:
                    if (spPoint <= 10)
                        point = spPoint;
                    else if (spPoint <= 28)
                        point = 10 + (spPoint - 10) / 2;
                    else if (spPoint <= 88)
                        point = 19 + (spPoint - 28) / 3;
                    else if (spPoint <= 168)
                        point = 39 + (spPoint - 88) / 4;
                    else if (spPoint <= 268)
                        point = 59 + (spPoint - 168) / 5;
                    else if (spPoint <= 334)
                        point = 79 + (spPoint - 268) / 6;
                    else if (spPoint <= 383)
                        point = 90 + (spPoint - 334) / 7;
                    else if (spPoint <= 391)
                        point = 97 + (spPoint - 383) / 8;
                    else if (spPoint <= 400)
                        point = 98 + (spPoint - 391) / 9;
                    else if (spPoint <= 410)
                        point = 99 + (spPoint - 400) / 10;
                    break;

                case 2:
                    if (spPoint <= 20)
                        point = spPoint;
                    else if (spPoint <= 40)
                        point = 20 + (spPoint - 20) / 2;
                    else if (spPoint <= 70)
                        point = 30 + (spPoint - 40) / 3;
                    else if (spPoint <= 110)
                        point = 40 + (spPoint - 70) / 4;
                    else if (spPoint <= 210)
                        point = 50 + (spPoint - 110) / 5;
                    else if (spPoint <= 270)
                        point = 70 + (spPoint - 210) / 6;
                    else if (spPoint <= 410)
                        point = 80 + (spPoint - 270) / 7;
                    break;

                case 1:
                    if (spPoint <= 10)
                        point = spPoint;
                    else if (spPoint <= 48)
                        point = 10 + (spPoint - 10) / 2;
                    else if (spPoint <= 81)
                        point = 29 + (spPoint - 48) / 3;
                    else if (spPoint <= 161)
                        point = 40 + (spPoint - 81) / 4;
                    else if (spPoint <= 236)
                        point = 60 + (spPoint - 161) / 5;
                    else if (spPoint <= 290)
                        point = 75 + (spPoint - 236) / 6;
                    else if (spPoint <= 360)
                        point = 84 + (spPoint - 290) / 7;
                    else if (spPoint <= 400)
                        point = 97 + (spPoint - 360) / 8;
                    else if (spPoint <= 410)
                        point = 99 + (spPoint - 400) / 10;
                    break;

                case 3:
                    if (spPoint <= 10)
                        point = spPoint;
                    else if (spPoint <= 50)
                        point = 10 + (spPoint - 10) / 2;
                    else if (spPoint <= 110)
                        point = 30 + (spPoint - 50) / 3;
                    else if (spPoint <= 150)
                        point = 50 + (spPoint - 110) / 4;
                    else if (spPoint <= 200)
                        point = 60 + (spPoint - 150) / 5;
                    else if (spPoint <= 260)
                        point = 70 + (spPoint - 200) / 6;
                    else if (spPoint <= 330)
                        point = 80 + (spPoint - 260) / 7;
                    else if (spPoint <= 410)
                        point = 90 + (spPoint - 330) / 8;
                    break;
            }
            return point;
        }

        public static int SpPoint(short spLevel, short upgrade)
        {
            int point = (spLevel - 20) * 3;
            if (spLevel <= 20)
            {
                point = 0;
            }
            switch (upgrade)
            {
                case 1:
                    point += 5;
                    break;

                case 2:
                    point += 10;
                    break;

                case 3:
                    point += 15;
                    break;

                case 4:
                    point += 20;
                    break;

                case 5:
                    point += 28;
                    break;

                case 6:
                    point += 36;
                    break;

                case 7:
                    point += 46;
                    break;

                case 8:
                    point += 56;
                    break;

                case 9:
                    point += 68;
                    break;

                case 10:
                    point += 80;
                    break;

                case 11:
                    point += 95;
                    break;

                case 12:
                    point += 110;
                    break;

                case 13:
                    point += 128;
                    break;

                case 14:
                    point += 148;
                    break;

                case 15:
                    point += 173;
                    break;
            }

            return point;
        }

        internal static int DarkResistance(byte @class, byte level)
        {
            return 0;
        }

        internal static int Defence(byte @class, byte level)
        {
            return hitDef[@class, level];
        }

        internal static int DefenceRate(byte @class, byte level)
        {
            return hitDodge[@class, level];
        }

        internal static int DistanceDefence(byte @class, byte level)
        {
            return distDef[@class, level];
        }

        internal static int DistanceDefenceRate(byte @class, byte level)
        {
            return distDodge[@class, level];
        }

        internal static int DistanceRate(byte @class, byte level)
        {
            return distRate[@class, level];
        }

        internal static int DistCritical(byte @class, byte level)
        {
            return criticalDist[@class, level];
        }

        internal static int DistCriticalRate(byte @class, byte level)
        {
            return criticalDistRate[@class, level];
        }

        internal static int Element(byte @class, byte level)
        {
            return 0;
        }

        internal static int FireResistance(byte @class, byte level)
        {
            return 0;
        }

        internal static int HitCritical(byte @class, byte level)
        {
            return criticalHit[@class, level];
        }

        internal static int HitCriticalRate(byte @class, byte level)
        {
            return criticalHitRate[@class, level];
        }

        internal static int HitRate(byte @class, byte level)
        {
            return hitRate[@class, level];
        }

        internal static int LightResistance(byte @class, byte level)
        {
            return 0;
        }

        internal static double UpgradeBonus(byte upgrade)
        {
            switch (upgrade)
            {
                case 1:
                    return 1.10;

                case 2:
                    return 1.15;

                case 3:
                    return 1.22;

                case 4:
                    return 1.32;

                case 5:
                    return 1.43;

                case 6:
                    return 1.54;

                case 7:
                    return 1.65;

                case 8:
                    return 1.90;

                case 9:
                    return 2.20;

                case 10:
                    return 3;

                default:
                    return 1;
            }
        }

        internal static int WaterResistance(byte @class, byte level)
        {
            return 0;
        }

        private void LoadHpData()
        {
            HP = new int[4, 100];

            // Adventurer HP
            for (int i = 1; i < HP.GetLength(1); i++)
            {
                HP[(int)ClassType.Adventurer, i] = (int)(1 / 2.0 * i * i + 31 / 2.0 * i + 205);
            }

            // Swordsman HP
            for (int i = 0; i < HP.GetLength(1); i++)
            {
                int j = 16;
                int hp = 946;
                int inc = 85;
                while (j <= i)
                {
                    if (j % 5 == 2)
                    {
                        hp += inc / 2;
                        inc += 2;
                    }
                    else
                    {
                        hp += inc;
                        inc += 4;
                    }
                    ++j;
                }
                HP[(int)ClassType.Swordman, i] = hp;
            }

            // Magician HP
            for (int i = 0; i < HP.GetLength(1); i++)
            {
                HP[(int)ClassType.Magician, i] = (int)(((i + 15) * (i + 15) + i + 15.0) / 2.0 - 465 + 550);
            }

            // Archer HP
            for (int i = 0; i < HP.GetLength(1); i++)
            {
                int hp = 680;
                int inc = 35;
                int j = 16;
                while (j <= i)
                {
                    hp += inc;
                    ++inc;
                    if (j % 10 == 1 || j % 10 == 5 || j % 10 == 8)
                    {
                        hp += inc;
                        ++inc;
                    }
                    ++j;
                }
                HP[(int)ClassType.Archer, i] = hp;
            }
        }

        private void LoadHpHealth()
        {
            hpHealth = new int[4];
            hpHealth[(int)ClassType.Archer] = 60;
            hpHealth[(int)ClassType.Adventurer] = 30;
            hpHealth[(int)ClassType.Swordman] = 90;
            hpHealth[(int)ClassType.Magician] = 30;
        }

        private void LoadHpHealthStand()
        {
            hpHealthStand = new int[4];
            hpHealthStand[(int)ClassType.Archer] = 32;
            hpHealthStand[(int)ClassType.Adventurer] = 25;
            hpHealthStand[(int)ClassType.Swordman] = 26;
            hpHealthStand[(int)ClassType.Magician] = 20;
        }

        private void LoadJobXpData()
        {
            // Load JobData
            firstjobxpData = new double[21];
            secondjobxpData = new double[81];
            firstjobxpData[0] = 2200;
            secondjobxpData[0] = 17600;
            for (int i = 1; i < firstjobxpData.Length; i++)
            {
                firstjobxpData[i] = firstjobxpData[i - 1] + 700;
            }

            for (int i = 1; i < secondjobxpData.Length; i++)
            {
                int var2 = 400;
                if (i > 3)
                {
                    var2 = 4500;
                }
                if (i > 40)
                {
                    var2 = 15000;
                }
                secondjobxpData[i] = secondjobxpData[i - 1] + var2;
            }
        }

        private void LoadMpData()
        {
            // ADVENTURER MP
            MP = new int[4, 101];

            MP[(int)ClassType.Adventurer, 0] = 60;
            int baseAdventurer = 9;

            for (int i = 1; i < MP.GetLength(1); i += 4)
            {
                MP[(int)ClassType.Adventurer, i] = MP[(int)ClassType.Adventurer, i - 1] + baseAdventurer;
                MP[(int)ClassType.Adventurer, i + 1] = MP[(int)ClassType.Adventurer, i] + baseAdventurer;
                MP[(int)ClassType.Adventurer, i + 2] = MP[(int)ClassType.Adventurer, i + 1] + baseAdventurer;
                baseAdventurer++;
                MP[(int)ClassType.Adventurer, i + 3] = MP[(int)ClassType.Adventurer, i + 2] + baseAdventurer;
                baseAdventurer++;
            }

            // SWORDSMAN MP
            for (int i = 1; i < MP.GetLength(1) - 1; i++)
            {
                MP[(int)ClassType.Swordman, i] = MP[(int)ClassType.Adventurer, i];
            }

            // ARCHER MP
            for (int i = 0; i < MP.GetLength(1) - 1; i++)
            {
                MP[(int)ClassType.Archer, i] = MP[(int)ClassType.Adventurer, i + 1];
            }

            // MAGICIAN MP
            for (int i = 0; i < MP.GetLength(1) - 1; i++)
            {
                MP[(int)ClassType.Magician, i] = 3 * MP[(int)ClassType.Adventurer, i];
            }
        }

        private void LoadMpHealth()
        {
            mpHealth = new int[4];
            mpHealth[(int)ClassType.Archer] = 50;
            mpHealth[(int)ClassType.Adventurer] = 10;
            mpHealth[(int)ClassType.Swordman] = 30;
            mpHealth[(int)ClassType.Magician] = 80;
        }

        private void LoadMpHealthStand()
        {
            mpHealthStand = new int[4];
            mpHealthStand[(int)ClassType.Archer] = 28;
            mpHealthStand[(int)ClassType.Adventurer] = 5;
            mpHealthStand[(int)ClassType.Swordman] = 16;
            mpHealthStand[(int)ClassType.Magician] = 40;
        }

        private void LoadSpeedData()
        {
            speedData = new byte[4];
            speedData[(int)ClassType.Archer] = 12;
            speedData[(int)ClassType.Adventurer] = 11;
            speedData[(int)ClassType.Swordman] = 11;
            speedData[(int)ClassType.Magician] = 10;
        }

        private void LoadSpXpData()
        {
            // Load SpData
            spxpData = new double[99];
            spxpData[0] = 15000;
            spxpData[19] = 218000;
            for (int i = 1; i < 19; i++)
            {
                spxpData[i] = spxpData[i - 1] + 10000;
            }
            for (int i = 20; i < spxpData.Length; i++)
            {
                spxpData[i] = spxpData[i - 1] + 6 * (3 * i * (i + 1) + 1);
            }
        }

        // TODO: Change or Verify
        private void LoadStats()
        {
            minHit = new int[4, 100];
            maxHit = new int[4, 100];
            hitRate = new int[4, 100];
            criticalHitRate = new int[4, 100];
            criticalHit = new int[4, 100];
            minDist = new int[4, 100];
            maxDist = new int[4, 100];
            distRate = new int[4, 100];
            criticalDistRate = new int[4, 100];
            criticalDist = new int[4, 100];
            hitDef = new int[4, 100];
            hitDodge = new int[4, 100];
            distDef = new int[4, 100];
            distDodge = new int[4, 100];
            magicalDef = new int[4, 100];

            for (int i = 0; i < 100; i++)
            {
                // ADVENTURER
                minHit[(int)ClassType.Adventurer, i] = i + 9; // approx
                maxHit[(int)ClassType.Adventurer, i] = i + 9; // approx
                hitRate[(int)ClassType.Adventurer, i] = i + 9; // approx
                criticalHitRate[(int)ClassType.Adventurer, i] = 0; // sure
                criticalHit[(int)ClassType.Adventurer, i] = 0; // sure
                minDist[(int)ClassType.Adventurer, i] = i + 9; // approx
                maxDist[(int)ClassType.Adventurer, i] = i + 9; // approx
                distRate[(int)ClassType.Adventurer, i] = (i + 9) * 2; // approx
                criticalDistRate[(int)ClassType.Adventurer, i] = 0; // sure
                criticalDist[(int)ClassType.Adventurer, i] = 0; // sure
                hitDef[(int)ClassType.Adventurer, i] = (int)(i + 9) / 2; // approx
                hitDodge[(int)ClassType.Adventurer, i] = i + 9; // approx
                distDef[(int)ClassType.Adventurer, i] = (int)(i + 9) / 2; // approx
                distDodge[(int)ClassType.Adventurer, i] = i + 9; // approx
                magicalDef[(int)ClassType.Adventurer, i] = (int)(i + 9) / 2; // approx

                // SWORDMAN
                criticalHitRate[(int)ClassType.Swordman, i] = 0; // approx
                criticalHit[(int)ClassType.Swordman, i] = 0; // approx
                criticalDist[(int)ClassType.Swordman, i] = 0; // approx
                criticalDistRate[(int)ClassType.Swordman, i] = 0; // approx
                minDist[(int)ClassType.Swordman, i] = i + 12; // approx
                maxDist[(int)ClassType.Swordman, i] = i + 12; // approx
                distRate[(int)ClassType.Swordman, i] = 2 * (i + 12); // approx
                hitDodge[(int)ClassType.Swordman, i] = i + 12; // approx
                distDodge[(int)ClassType.Swordman, i] = i + 12; // approx
                magicalDef[(int)ClassType.Swordman, i] = (i + 9) / 2; // approx
                hitRate[(int)ClassType.Swordman, i] = i + 27; // approx
                hitDef[(int)ClassType.Swordman, i] = i + 2; // approx

                minHit[(int)ClassType.Swordman, i] = 2 * i + 5; // approx Numbers n such that 10n+9 is prime.
                maxHit[(int)ClassType.Swordman, i] = 2 * i + 5; // approx Numbers n such that 10n+9 is prime.
                distDef[(int)ClassType.Swordman, i] = i; // approx

                // MAGICIAN
                hitRate[(int)ClassType.Magician, i] = 0; // sure
                criticalHitRate[(int)ClassType.Magician, i] = 0; // sure
                criticalHit[(int)ClassType.Magician, i] = 0; // sure
                criticalDistRate[(int)ClassType.Magician, i] = 0; // sure
                criticalDist[(int)ClassType.Magician, i] = 0; // sure

                minDist[(int)ClassType.Magician, i] = 14 + i; // approx
                maxDist[(int)ClassType.Magician, i] = 14 + i; // approx
                distRate[(int)ClassType.Magician, i] = (14 + i) * 2; // approx
                hitDef[(int)ClassType.Magician, i] = (int)(i + 11) / 2; // approx
                magicalDef[(int)ClassType.Magician, i] = i + 4; // approx
                hitDodge[(int)ClassType.Magician, i] = 24 + i; // approx
                distDodge[(int)ClassType.Magician, i] = 14 + i; // approx

                minHit[(int)ClassType.Magician, i] = 2 * i + 9; // approx Numbers n such that n^2 is of form x^ 2 + 40y ^ 2 with positive x,y.
                maxHit[(int)ClassType.Magician, i] = 2 * i + 9; // approx Numbers n such that n^2 is of form x^2+40y^2 with positive x,y.
                distDef[(int)ClassType.Magician, i] = 20 + i; // approx

                // ARCHER
                criticalHitRate[(int)ClassType.Archer, i] = 0; // sure
                criticalHit[(int)ClassType.Archer, i] = 0; // sure
                criticalDistRate[(int)ClassType.Archer, i] = 0; // sure
                criticalDist[(int)ClassType.Archer, i] = 0; // sure

                minHit[(int)ClassType.Archer, i] = 9 + i * 3; // approx
                maxHit[(int)ClassType.Archer, i] = 9 + i * 3; // approx
                int add = (i % 2 == 0) ? 2 : 4;
                hitRate[(int)ClassType.Archer, 1] = 41;
                hitRate[(int)ClassType.Archer, i] = hitRate[(int)ClassType.Archer, i] + add; // approx
                minDist[(int)ClassType.Archer, i] = 2 * i; // approx
                maxDist[(int)ClassType.Archer, i] = 2 * i; // approx

                distRate[(int)ClassType.Archer, i] = 20 + 2 * i; // approx
                hitDef[(int)ClassType.Archer, i] = i; // approx
                magicalDef[(int)ClassType.Archer, i] = i + 2; // approx
                hitDodge[(int)ClassType.Archer, i] = 41 + i; // approx
                distDodge[(int)ClassType.Archer, i] = i + 2; // approx
                distDef[(int)ClassType.Archer, i] = i; // approx
            }
        }

        private void LoadXpData()
        {
            // Load XpData
            xpData = new double[100];
            double[] v = new double[100];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            xpData[0] = 300;
            for (int i = 2; i < v.Length; i++)
            {
                v[i] = v[i - 1] + 420 + 120 * (i - 1);
            }
            for (int i = 1; i < xpData.Length; i++)
            {
                if (i < 79)
                {
                    if (i == 14)
                    {
                        var = 6 / 3;
                    }
                    else if (i == 39)
                    {
                        var = (19 / (double)3);
                    }
                    else if (i == 59)
                    {
                        var = (70 / (double)3);
                    }
                    xpData[i] = Convert.ToInt64(xpData[i - 1] + var * v[i - 1]);
                }
                if (i >= 79)
                {
                    if (i == 79)
                    {
                        var = 5000;
                    }
                    if (i == 82)
                    {
                        var = 9000;
                    }
                    if (i == 84)
                    {
                        var = 13000;
                    }
                    xpData[i] = Convert.ToInt64(xpData[i - 1] + var * (i + 2) * (i + 2));
                }

                // Console.WriteLine("lvl " + (i) + ":" + u[i - 1]);
            }
        }

        #endregion
    }
}