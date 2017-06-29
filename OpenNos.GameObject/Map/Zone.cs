using OpenNos.Domain;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class ZoneEvent
    {

        #region Properties

        public short X { get; set; }

        public short Y { get; set; }

        public short Range { get; set; }

        public List<EventContainer> Events { get; set; }

        public ZoneEvent()
        {
            Events = new List<EventContainer>();
            Range = 1;
        }

        public bool InZone(short positionX, short positionY)
        {
            return positionX <= X + Range && positionX >= X - Range && positionY <= Y + Range && positionY >= Y - Range ;
        }

        #endregion
    }
}