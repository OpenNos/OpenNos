using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Domain
{
    public class Schedule
    {
        public EventType Event { get; set; }
        public TimeSpan Time { get; set; }

    }
}
