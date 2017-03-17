using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class EventContainer
    {
        public EventActionType EventActionType { get; set; }
        public Object Parameter { get; set; }
        public MapInstance MapInstance { get; set; }

        public EventContainer(MapInstance mapInstance, EventActionType eventActionType, Object param)
        {
            MapInstance = mapInstance;
            EventActionType = eventActionType;
            Parameter = param;
        } 
    }
}
