using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class EventContainer
    {
        #region Instantiation

        public EventContainer(MapInstance mapInstance, EventActionType eventActionType, Object param)
        {
            MapInstance = mapInstance;
            EventActionType = eventActionType;
            Parameter = param;
        }

        #endregion

        #region Properties

        public EventActionType EventActionType { get; set; }

        public MapInstance MapInstance { get; set; }

        public Object Parameter { get; set; }

        #endregion
    }
}