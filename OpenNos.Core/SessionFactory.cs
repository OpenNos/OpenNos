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

        private int _sessionCounter;
        
        #endregion

        #region Singleton

        private static SessionFactory _instance;

        private SessionFactory() { }

        public static SessionFactory Instance => _instance ?? (_instance = new SessionFactory());

        #endregion


        #region Methods

        public int GenerateSessionId()
        {
            _sessionCounter += 2;
            return _sessionCounter; 
        }

        #endregion
    }
}
