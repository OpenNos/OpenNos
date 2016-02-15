using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class SessionFactory
    {
        #region Members

        private Random _randomSessionId;
        private IList<int> _sessionIds;

        #endregion

        #region Singleton

        private static SessionFactory _instance;

        private SessionFactory() { }

        public static SessionFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SessionFactory();

                return _instance;
            }
        }

        #endregion

        #region Properties

        private Random RandomSessionId
        {
            get
            {
                if (_randomSessionId == null)
                {
                    _randomSessionId = new Random();
                }

                return _randomSessionId;
            }
        }

        private IList<int> SessionIds
        {
            get
            {
                if (_sessionIds == null)
                {
                    _sessionIds = new List<int>();
                }

                return _sessionIds;
            }
        }

        #endregion

        #region Methods

        public int GenerateSessionId()
        {
            bool sessionIdFound = false;

            while (!sessionIdFound)
            {
                int newSessionId = RandomSessionId.Next(10000, UInt16.MaxValue);

                if (!SessionIds.Contains(newSessionId))
                {
                    SessionIds.Add(newSessionId);
                    return newSessionId;
                }
            }

            if (SessionIds.Count.Equals(UInt16.MaxValue + 10000))
            {
                SessionIds.Clear();
                Logger.Log.Info("Resetted SessionIds");
                int newSessionId = RandomSessionId.Next(10000, UInt16.MaxValue);

                if (!SessionIds.Contains(newSessionId))
                {
                    SessionIds.Add(newSessionId);
                    return newSessionId;
                }
            }

            return 0;
        }

        #endregion
    }
}
