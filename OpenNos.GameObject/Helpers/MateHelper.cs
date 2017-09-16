using System;
using OpenNos.Core;

namespace OpenNos.GameObject.Helpers
{
    public class MateHelper : Singleton<MateHelper>
    {
        #region Instantiation

        #endregion

        #region Members

        public MateHelper()
        {
            LoadXPData();
        }

        #endregion

        #region Properties

        public double[] XPData { get; private set; }

        #endregion

        #region Methods

        private void LoadXPData()
        {
            // Load XpData
            XPData = new double[256];
            double[] v = new double[256];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            XPData[0] = 300;
            for (int i = 2; i < v.Length; i++)
            {
                v[i] = v[i - 1] + 420 + 120 * (i - 1);
            }
            for (int i = 1; i < XPData.Length; i++)
            {
                if (i < 79)
                {
                    switch (i)
                    {
                        case 14:
                            var = 6 / 3d;
                            break;
                        case 39:
                            var = 19 / 3d;
                            break;
                        case 59:
                            var = 70 / 3d;
                            break;
                    }
                    XPData[i] = Convert.ToInt64(XPData[i - 1] + var * v[i - 1]);
                }
                if (i < 79)
                {
                    continue;
                }
                switch (i)
                {
                    case 79:
                        var = 5000;
                        break;
                    case 82:
                        var = 9000;
                        break;
                    case 84:
                        var = 13000;
                        break;
                }
                XPData[i] = Convert.ToInt64(XPData[i - 1] + var * (i + 2) * (i + 2));
            }
        }

        #endregion

        #region Singleton

        private static MateHelper _instance;

        public static MateHelper Instance
        {
            get { return _instance ?? (_instance = new MateHelper()); }
        }

        #endregion
    }
}