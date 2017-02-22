using System;

namespace OpenNos.Domain
{
    public class Schedule
    {
        #region Properties

        public EventType Event { get; set; }

        public TimeSpan Time { get; set; }

        #endregion
    }
}