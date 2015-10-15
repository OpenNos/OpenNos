using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class Logger
    {
        #region Members

        private static ILog _log;

        #endregion

        #region Properties

        public static ILog Log
        {
            get
            {
                return _log;
            }
            set
            {
                _log = value;
            }
        }

        #endregion

        #region Methods

        public static void InitializeLogger(ILog log)
        {
            Log = log;
        }

        #endregion

    }
}
