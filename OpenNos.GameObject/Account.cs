using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class Account : AccountDTO
    {
        #region Instantiation

        public Account()
        {
            PenaltyLogs = new List<PenaltyLogDTO>();
            GeneralLogs = new List<GeneralLogDTO>();
        }

        #endregion

        #region Properties

        public List<GeneralLogDTO> GeneralLogs { get; set; }

        public List<PenaltyLogDTO> PenaltyLogs { get; set; }

        #endregion
    }
}