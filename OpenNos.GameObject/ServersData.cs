using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class ServersData
    {
        private ServersData()
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

        //same for all class
        private static double[] xpData = null;
        private static double[] firstjobxpData = null;
        private static double[] secondjobxpData = null;
        private static double[] spxpData = null;
        //difference between class
        private static int[,] minHit = null;
        private static int[,] maxHit = null;
        private static int[,] hitRate = null;
        private static int[,] criticalHitRate = null;
        private static int[,] criticalHit = null;
        private static int[,] minDist = null;
        private static int[,] maxDist = null;
        private static int[,] distRate = null;
        private static int[,] criticalDistRate = null;
        private static int[,] criticalDist = null;
        private static int[,] hitDef = null;
        private static int[,] hitDodge = null;
        private static int[,] DistDef = null;
        private static int[,] DistDodge = null;
        private static int[,] magicalDef = null;
        private static int[] hpHealth = null;
        private static int[] mpHealth = null;
        private static int[] hpHealthStand = null;
        private static int[] mpHealthStand = null;
        //STAT DATA
        private static int[] speedData = null;
        private static int[,] HP = null;
        private static int[,] MP = null;
        public static int[,] MPData
        {
            get
            {
                if (MP == null)
                {
                    new ServersData();
                }
                return MP;
            }
        }
        public static int[] HpHealth
        {
            get
            {
                if (hpHealth == null)
                {
                    new ServersData();
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
                    new ServersData();
                }
                return hpHealthStand;
            }
        }
        public static int[] MpHealth
        {
            get
            {
                if (mpHealth == null)
                {
                    new ServersData();
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
                    new ServersData();
                }
                return mpHealthStand;
            }
        }
        public static int[,] HPData
        {
            get
            {
                if (HP == null)
                {
                    new ServersData();
                }
                return HP;
            }
        }
        public static int[] SpeedData
        {
            get
            {
                if (speedData == null)
                {
                    new ServersData();
                }
                return speedData;
            }
        }
        public static double[] XPData
        {
            get
            {
                if (xpData == null)
                {
                    new ServersData();
                }
                return xpData;
            }
        }
        public static double[] FirstJobXPData
        {
            get
            {
                if (firstjobxpData == null)
                {
                    new ServersData();
                }
                return firstjobxpData;
            }
        }
        public static double[] SecondJobXPData
        {
            get
            {
                if (secondjobxpData == null)
                {
                    new ServersData();
                }
                return secondjobxpData;
            }
        }
        public static double[] SpXPData
        {
            get
            {
                if (spxpData == null)
                {
                    new ServersData();
                }
                return spxpData;
            }
        }
        private void LoadHpData()
        {
            HP = new int[4, 100];
            //Adventurer HP
            for (int i = 1; i < HP.GetLength(1); i++)
            {
                HP[(int)ClassType.Adventurer, i] = (int)(1 / 2.0 * i * i + 31 / 2.0 * i + 205);

            }

            //Swordman HP
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

            //Magician HP
            for (int i = 0; i < HP.GetLength(1); i++)
            {
                HP[(int)ClassType.Magician, i] = (int)(((i + 15) * (i + 15) + i + 15.0) / 2.0 - 465 + 550);
            }

            //Archer HP
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
        private void LoadMpHealth()
        {
            mpHealth = new int[4];
            mpHealth[(int)ClassType.Archer] = 50;
            mpHealth[(int)ClassType.Adventurer] = 10;
            mpHealth[(int)ClassType.Swordman] = 30;
            mpHealth[(int)ClassType.Magician] = 80;
        }
        private void LoadHpHealthStand()
        {
            hpHealthStand = new int[4];
            hpHealthStand[(int)ClassType.Archer] = 32;
            hpHealthStand[(int)ClassType.Adventurer] = 25;
            hpHealthStand[(int)ClassType.Swordman] = 26;
            hpHealthStand[(int)ClassType.Magician] = 20;
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
            speedData = new int[4];
            speedData[(int)ClassType.Archer] = 12;
            speedData[(int)ClassType.Adventurer] = 11;
            speedData[(int)ClassType.Swordman] = 11;
            speedData[(int)ClassType.Magician] = 10;
        }
        private void LoadJobXpData()
        {
            //Load JobData
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
                    var2 = 4500;
                if (i > 40)
                    var2 = 15000;
                secondjobxpData[i] = secondjobxpData[i - 1] + var2;

            }
        }
        private void LoadSpXpData()
        {
            //Load SpData
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
        private void LoadXpData()
        {
            //Load XpData
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

                    if (i == 14) var = 6 / 3;
                    else if (i == 39) var = (double)(19 / (double)3);
                    else if (i == 59) var = (double)(70 / (double)3);
                    xpData[i] = Convert.ToInt64(xpData[i - 1] + var * v[i - 1]);
                }
                if (i >= 79)
                {
                    if (i == 79)
                        var = 5000;
                    if (i == 82)
                        var = 9000;
                    if (i == 84)
                        var = 13000;
                    xpData[i] = Convert.ToInt64(xpData[i - 1] + var * (i + 2) * (i + 2));
                }

                //Console.WriteLine("lvl " + (i) + ":" + u[i - 1]);
            }
        }

        //TODO Change or Verify
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
            DistDef = new int[4, 100];
            DistDodge = new int[4, 100];
            magicalDef = new int[4, 100];

           
            for (int i = 0; i < 100; i++)
            {
                //ADVENTURER
                minHit[(int)ClassType.Adventurer, i] = i + 9;//approx
                maxHit[(int)ClassType.Adventurer, i] = i + 9;//approx
                hitRate[(int)ClassType.Adventurer, i] = i + 9;//approx
                criticalHitRate[(int)ClassType.Adventurer, i] = 0;//sure
                criticalHit[(int)ClassType.Adventurer, i] = 0;//sure
                minDist[(int)ClassType.Adventurer, i] = i + 9;//approx
                maxDist[(int)ClassType.Adventurer, i] = i + 9;//approx
                distRate[(int)ClassType.Adventurer, i] = (i + 9) * 2;//approx
                criticalDistRate[(int)ClassType.Adventurer, i] = 0;//sure
                criticalDist[(int)ClassType.Adventurer, i] = 0;//sure
                hitDef[(int)ClassType.Adventurer, i]  = (int)(i + 9) / 2;//approx
                hitDodge[(int)ClassType.Adventurer, i] = i + 9;//approx
                DistDef[(int)ClassType.Adventurer, i] = (int)(i + 9) / 2;//approx
                DistDodge[(int)ClassType.Adventurer, i] = i + 9;//approx
                magicalDef[(int)ClassType.Adventurer, i] = (int)(i + 9) / 2;//approx

                //SWORDMAN
                criticalHitRate[(int)ClassType.Swordman, i] = 0;//approx
                criticalHit[(int)ClassType.Swordman, i] = 0;//approx
                criticalDist[(int)ClassType.Swordman, i] = 0;//approx
                criticalDistRate[(int)ClassType.Swordman, i] = 0;//approx
                minDist[(int)ClassType.Swordman, i] = i + 12;//approx
                maxDist[(int)ClassType.Swordman, i] = i + 12;//approx
                distRate[(int)ClassType.Swordman, i] = 2 * (i +12);//approx
                hitDodge[(int)ClassType.Swordman, i] = i + 12;//approx
                DistDodge[(int)ClassType.Swordman, i] = i + 12;//approx
                magicalDef[(int)ClassType.Swordman, i] = (i+9)/2;//approx
                hitRate[(int)ClassType.Swordman, i] = i + 27;//approx
                hitDef[(int)ClassType.Swordman, i] = i + 2;//approx

                minHit[(int)ClassType.Swordman, i] = 2*i+5;//approx 	Numbers n such that 10n+9 is prime. 
                maxHit[(int)ClassType.Swordman, i] = 2 * i + 5;//approx  	Numbers n such that 10n+9 is prime. 
                DistDef[(int)ClassType.Swordman, i] = i;//approx


                //MAGICIAN
                hitRate[(int)ClassType.Magician, i] = 0;//sure
                criticalHitRate[(int)ClassType.Magician, i] = 0;//sure
                criticalHit[(int)ClassType.Magician, i] = 0;//sure
                criticalDistRate[(int)ClassType.Magician, i] = 0;//sure
                criticalDist[(int)ClassType.Magician, i] = 0;//sure

                minDist[(int)ClassType.Magician, i] = 14 + i;//approx
                maxDist[(int)ClassType.Magician, i] = 14 + i;//approx
                distRate[(int)ClassType.Magician, i] = (14 + i) * 2;//approx
                hitDef[(int)ClassType.Magician, i] = (int)(i + 11) / 2;//approx
                magicalDef[(int)ClassType.Magician, i] = i + 4;//approx
                hitDodge[(int)ClassType.Magician, i] = 24 + i;//approx
                DistDodge[(int)ClassType.Magician, i] = 14 + i;//approx

                minHit[(int)ClassType.Magician, i] = 2 * i + 9;//approx Numbers n such that n ^ 2 is of form x^ 2 + 40y ^ 2 with positive x,y.
                maxHit[(int)ClassType.Magician, i] = 2 * i + 9;//approx Numbers n such that n^2 is of form x^2+40y^2 with positive x,y. 
                DistDef[(int)ClassType.Magician, i] = 20 + i;//approx

                //ARCHER
                criticalHitRate[(int)ClassType.Archer, i] = 0;//sure
                criticalHit[(int)ClassType.Archer, i] = 0;//sure
                criticalDistRate[(int)ClassType.Archer, i] = 0;//sure
                criticalDist[(int)ClassType.Archer, i] = 0;//sure

                minHit[(int)ClassType.Archer, i] = 9 + i * 3;//approximate
                maxHit[(int)ClassType.Archer, i] = 9 + i * 3;//aproximate
                int add = (i % 2 == 0) ? 2 : 4;
                hitRate[(int)ClassType.Archer, 1] = 41;
                hitRate[(int)ClassType.Archer, i] = hitRate[(int)ClassType.Archer, i] + add ;//approximate
                minDist[(int)ClassType.Archer, i] = 2*i;//approximate
                maxDist[(int)ClassType.Archer, i] = 2 * i;//approximate

                distRate[(int)ClassType.Archer, i] = 20+2 * i;//approximate;
                hitDef[(int)ClassType.Archer, i] = i;//approximate;
                magicalDef[(int)ClassType.Archer, i] = i+2;//approximate;
                hitDodge[(int)ClassType.Archer, i] = 41+i;//approximate;
                DistDodge[(int)ClassType.Archer, i] = i+2;//approximate;
                DistDef[(int)ClassType.Archer, i] = i;//approximate;
            }
        }
        private void LoadMpData()
        {
            //ADVENTURER MP
            MP = new int[4, 101];

            int U0 = 9;
            int U1 = 10;
            MP[(int)ClassType.Adventurer, 0] = 60;
            MP[(int)ClassType.Adventurer, 1] = 69;
            MP[(int)ClassType.Adventurer, 2] = 78;
            MP[(int)ClassType.Adventurer, 3] = 87;
            MP[(int)ClassType.Adventurer, 4] = 97;

            for (int i = 5; i < MP.GetLength(1); i += 4)
            {
                MP[(int)ClassType.Adventurer, i] = MP[(int)ClassType.Adventurer, i - 1] + (U0 + i / 2);
                MP[(int)ClassType.Adventurer, i + 1] = MP[(int)ClassType.Adventurer, i] + (U0 + i / 2);
                MP[(int)ClassType.Adventurer, i + 2] = MP[(int)ClassType.Adventurer, i + 1] + (U0 + i / 2);
                MP[(int)ClassType.Adventurer, i + 3] = MP[(int)ClassType.Adventurer, i + 2] + (U1 + i / 2);
               
            }
        
            //SWORDMAN MP
            for (int i = 0; i < MP.GetLength(1)-1; i++)
            {
              
                MP[(int)ClassType.Swordman, i+1] = MP[(int)ClassType.Adventurer, i];
                MP[(int)ClassType.Adventurer, i] = MP[(int)ClassType.Swordman, i + 1];

            }
   
            //ARCHER MP
            for (int i = 0; i < 15; i++)
            {
                MP[(int)ClassType.Archer, i] = MP[(int)ClassType.Adventurer, i];
            }
            MP[(int)ClassType.Archer, 15] = 240;
            MP[(int)ClassType.Archer, 16] = 256;
            MP[(int)ClassType.Archer, 17] = 273;
            MP[(int)ClassType.Archer, 18] = 290;
            MP[(int)ClassType.Archer, 19] = 307;
            U0 = 18;
            for (int i = 20; i < MP.GetLength(1)-1; i+=4)
            {
                MP[(int)ClassType.Archer, i] = MP[(int)ClassType.Archer, i - 1] + U0 + ((i- 19) / 2);
                MP[(int)ClassType.Archer, i + 1] = MP[(int)ClassType.Archer, i] + U0 + 1 + ((i - 19) / 2);
                MP[(int)ClassType.Archer, i + 2] = MP[(int)ClassType.Archer, i + 1] + U0 + 1 + ((i - 19) / 2);
                MP[(int)ClassType.Archer, i + 3] = MP[(int)ClassType.Archer, i + 2] + U0 +1+ ((i - 19) / 2);
                
            }
            //MAGICIAN MP
            for (int i = 0; i < MP.GetLength(1) - 1; i++)
            {

                MP[(int)ClassType.Magician, i ] = 3*MP[(int)ClassType.Adventurer, i];

            }

        }



    }
}
