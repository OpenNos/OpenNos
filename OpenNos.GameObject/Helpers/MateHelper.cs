using System;

namespace OpenNos.GameObject.Helpers
{
    public class MateHelper
    {
        #region Members

        private double[] _xpData;

        #endregion

        #region Properties

        public double[] XPData
        {
            get
            {
                if (_xpData == null)
                {
                    new MateHelper();
                }
                return _xpData;
            }
        }

        #endregion

        #region Methods

        private void LoadXPData()
        {
            // Load XpData
            _xpData = new double[256];
            double[] v = new double[256];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            _xpData[0] = 300;
            for (int i = 2; i < v.Length; i++)
            {
                v[i] = v[i - 1] + 420 + 120 * (i - 1);
            }
            for (int i = 1; i < _xpData.Length; i++)
            {
                if (i < 79)
                {
                    if (i == 14)
                    {
                        var = 6 / 3d;
                    }
                    else if (i == 39)
                    {
                        var = 19 / 3d;
                    }
                    else if (i == 59)
                    {
                        var = 70 / 3d;
                    }
                    _xpData[i] = Convert.ToInt64(_xpData[i - 1] + var * v[i - 1]);
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
                    _xpData[i] = Convert.ToInt64(_xpData[i - 1] + var * (i + 2) * (i + 2));
                }
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