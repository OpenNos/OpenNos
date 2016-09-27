using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class Account : AccountDTO
    {
        #region Instantiation

        public Account()
        {
            PenaltyLogs = new List<PenaltyLog>();
            GeneralLogs = new List<GeneralLog>();
        }

        #endregion

        #region Properties

        public List<GeneralLog> GeneralLogs { get; set; }

        public List<PenaltyLog> PenaltyLogs { get; set; }

        #endregion
    }
}