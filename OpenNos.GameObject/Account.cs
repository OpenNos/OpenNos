using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{

    public class Account : AccountDTO
    {
        public List<PenaltyLog> PenaltyLogs { get; set; }
        public List<GeneralLog> GeneralLogs { get; set; }
        public Account()
        {
            PenaltyLogs = new List<PenaltyLog>();
            GeneralLogs = new List<GeneralLog>();
        }
     
    }
}
