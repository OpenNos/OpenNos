using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    class ServersData
    {
        private static double[] xpData = null;
        private static double[] firstjobxpData = null;
        private static double[] secondjobxpData = null;
        private static double[] spxpData = null;


        private ServersData() {

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

            double[] spxpData = new double[99];
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

            double[] xpData = new double[100];
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
