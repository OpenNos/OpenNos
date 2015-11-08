using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class ServersData
    {
        private static double[] xpData = null;
        private static double[] firstjobxpData = null;
        private static double[] secondjobxpData = null;
        private static double[] spxpData = null;
        private static int[] speedData = null;
        private static int[,] HP = null;
        private static int[,] MP = null;
        private void LoadHpData()
        {
            HP = new int[4,100];
            //Adventurer HP
            for (int i=1;i<HP.GetLength(1);i++)
            {
                HP[(int)ClassType.Adventurer, i] = (int)(1 / 2.0 * i * i + 31 / 2.0 * i + 205);
            }
            //Swordman HP
            HP[(int)ClassType.Swordman, 15] = 905;
            for (int i = 16; i < HP.GetLength(1); i++)
            {
                HP[(int)ClassType.Swordman, i] = HP[(int)ClassType.Swordman, i-1] + 4 * (i+4) + 3;
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

            for (int i = 5; i < MP.GetLength(1); i+=4)
            {
                MP[(int)ClassType.Adventurer, i] = MP[(int)ClassType.Adventurer, i-1] + (U0+i/2);
                MP[(int)ClassType.Adventurer, i+1] = MP[(int)ClassType.Adventurer, i] + (U0 + i/2);
                MP[(int)ClassType.Adventurer, i+2] = MP[(int)ClassType.Adventurer, i+1] + (U0 + i/2);
                MP[(int)ClassType.Adventurer, i+3] = MP[(int)ClassType.Adventurer, i+2] + (U1 + i/2);
            }
            //SWORDMAN MP
            for (int i = 15; i < MP.GetLength(1); i++)
            {
                MP[(int)ClassType.Swordman, i] = MP[(int)ClassType.Adventurer, i];
            }

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
        private ServersData() {

            LoadSpeedData();
            LoadJobXpData();
            LoadSpXpData();
            LoadHpData();
            LoadMpData();

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

    }
}
