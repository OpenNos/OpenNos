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
        //STAT DATA
        private static int[] speedData = null;
        private static int[,] HP = null;
        private static int[,] MP = null;
        private void LoadStats()
        {
            minHit = new int[4, 99];
            maxHit = new int[4, 99];
            hitRate = new int[4, 99];
            criticalHitRate = new int[4, 99];
            criticalHit = new int[4, 99];
            minDist = new int[4, 99];
            maxDist = new int[4, 99];
            distRate = new int[4, 99];
            criticalDistRate = new int[4, 99];
            criticalDist = new int[4, 99];
            hitDef = new int[4, 99];
            hitDodge = new int[4, 99];
            DistDef = new int[4, 99];
            DistDodge = new int[4, 99];
            magicalDef = new int[4, 99];

           
            for (int i = 0; i < 99; i++)
            {
                //ADVENTURER
                minHit[(int)ClassType.Adventurer, i] = i + 9;
                maxHit[(int)ClassType.Adventurer, i] = i + 9;
                hitRate[(int)ClassType.Adventurer, i] = i + 9;
                criticalHitRate[(int)ClassType.Adventurer, i] = 0;
                criticalHit[(int)ClassType.Adventurer, i] = 0;
                minDist[(int)ClassType.Adventurer, i] = i + 9;
                maxDist[(int)ClassType.Adventurer, i] = i + 9;
                distRate[(int)ClassType.Adventurer, i] = (i + 9) * 2;
                criticalDistRate[(int)ClassType.Adventurer, i] = 0;
                criticalDist[(int)ClassType.Adventurer, i] = 0;
                hitDef[(int)ClassType.Adventurer, i]  = (int)(i + 9) / 2;
                hitDodge[(int)ClassType.Adventurer, i] = i + 9;
                DistDef[(int)ClassType.Adventurer, i] = (int)(i + 9) / 2;
                DistDodge[(int)ClassType.Adventurer, i] = i + 9;
                magicalDef[(int)ClassType.Adventurer, i] = (int)(i + 9) / 2;

                //SWORDMAN
                minHit[(int)ClassType.Swordman, i] = i +22;
                maxHit[(int)ClassType.Swordman, i] = i + 22;
                hitRate[(int)ClassType.Swordman, i] = i + 27;
                criticalHitRate[(int)ClassType.Swordman, i] = 0;
                criticalHit[(int)ClassType.Swordman, i] = 0;
                minDist[(int)ClassType.Swordman, i] = i -3;
                maxDist[(int)ClassType.Swordman, i] = i -3;
                distRate[(int)ClassType.Swordman, i] = 2 * i - 6;
                criticalDistRate[(int)ClassType.Swordman, i] = 0;
                criticalDist[(int)ClassType.Swordman, i] = 0;
                hitDef[(int)ClassType.Swordman, i] = i+2;
                hitDodge[(int)ClassType.Swordman, i] = i -3;
                DistDef[(int)ClassType.Swordman, i] = (i+1)/2-4;
                DistDodge[(int)ClassType.Swordman, i] = i + -3;
                magicalDef[(int)ClassType.Swordman, i] = i / 2 -3;

                //MAGICIAN
                minHit[(int)ClassType.Magician, i] = 9+i*2;
                maxHit[(int)ClassType.Magician, i] = 9 + i * 2;
                hitRate[(int)ClassType.Magician, i] = 0;
                criticalHitRate[(int)ClassType.Magician, i] = 0;
                criticalHit[(int)ClassType.Magician, i] = 0;
                minDist[(int)ClassType.Magician, i] = 2*i - 1;
                maxDist[(int)ClassType.Magician, i] = 2 * i - 1;
                distRate[(int)ClassType.Magician, i] = (2 * i - 1)*2;
                criticalDistRate[(int)ClassType.Magician, i] = 0;
                criticalDist[(int)ClassType.Magician, i] = 0;
                hitDef[(int)ClassType.Magician, i] = (i + 1)/2+5;
                hitDodge[(int)ClassType.Magician, i] = 25+i;
                DistDef[(int)ClassType.Magician, i] = (i + 1) / 2 - 4;
                DistDodge[(int)ClassType.Magician, i] = 19 + i;
                magicalDef[(int)ClassType.Magician, i] = i+4;

                //ARCHER
   
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
            for (int i = 0; i < 16; i++)
            {
                HP[(int)ClassType.Swordman, i] = 905;
            }
            for (int i = 16; i < HP.GetLength(1); i++)
            {
                HP[(int)ClassType.Swordman, i] = HP[(int)ClassType.Swordman, i - 1] + 4 * (i + 4) + 3;
            }

            //Magician HP
            for (int i = 0; i < 16; i++)
            {
                HP[(int)ClassType.Magician, i] = 550;
            }
            for (int i = 16; i < HP.GetLength(1); i++)
            {
                double var = 0;
                for (int j = 2; j < i + 24; j++)
                    var += Math.Floor(1 / 2 + Math.Sqrt(2 * j + 4));
                HP[(int)ClassType.Magician, i] = (int)((i+24) * (i+24 + 1) / 2 - 4 - var-7);
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
            for (int i = 0; i < MP.GetLength(1); i++)
            {
                MP[(int)ClassType.Swordman, i] = MP[(int)ClassType.Adventurer, i];
            }
            //MAGICIAN MP

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

        //XP DATA
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

    }
}
